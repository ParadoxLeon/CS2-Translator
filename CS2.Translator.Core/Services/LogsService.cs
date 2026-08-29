using System.Diagnostics;
using System.Text.RegularExpressions;
using CS2.Translator.Core.Enums;
using CS2.Translator.Core.Exceptions;
using CS2.Translator.Core.Models;
using CS2.Translator.Core.Helper;

namespace CS2.Translator.Core.Services;

public sealed class LogsService
{
    public event Action<Chat>? ChatReceived;
    public event Action? ChatsUpdated;

    private readonly LinkedList<Log> _logs = new();
    public List<Chat> Chats { get; } = new();

    private readonly string _logFilePath;
    private readonly TranslatorService _translator;
    private readonly string _targetLanguage;
    private readonly string _playerName;
    private readonly bool _autoTranslate;

    private FileSystemWatcher? _watcher;
    private Timer? _pollTimer;
    private CancellationTokenSource? _debounceCts;
    private long _lastFilePosition = 0;
    private DateTime _lastWriteTimeUtc = DateTime.MinValue;
    
    // measuring intervals 
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(1);

    private static void DebugLog(string msg, string tag = "LogsService")
    {
        string formatted = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{tag}] | {msg}";
        DebugLogger.Log(formatted);
    }
    
    public LogsService(
        string cs2InstallationPath,
        TranslatorService translator,
        string targetLanguage,
        string playerName,
        bool autoTranslate = true)
    {
        if (string.IsNullOrWhiteSpace(cs2InstallationPath))
            throw new ArgumentException("CS2 path is empty");

        _logFilePath = Path.Combine(cs2InstallationPath, "game", "csgo", "console.log");
        _translator = translator;
        _targetLanguage = targetLanguage;
        _playerName = playerName?.Trim() ?? "";
        _autoTranslate = autoTranslate;

        DebugLog("Initialized service");
        DebugLog($"Log file path: {_logFilePath}");
        DebugLog($"PlayerName='{_playerName}', AutoTranslate={_autoTranslate}, TargetLanguage='{_targetLanguage}'");
    }
    
    public async Task LoadLogsAsync(int amount)
    {
        var sw = Stopwatch.StartNew();
        DebugLog($"[LOAD] Starting LoadLogsAsync(amount={amount})...");

        var lines = await ReadNewLinesAsync();
        DebugLog($"[LOAD] Read {lines.Count} lines in {sw.ElapsedMilliseconds}ms");

        if (lines.Count == 0)
        {
            DebugLog("[LOAD] No new lines found");
            return;
        }

        var parsed = ParseLines(lines);
        DebugLog($"[PARSE] Parsed {parsed.Count} potential chat lines");

        var newLogs = GetNewLogs(parsed);
        DebugLog($"[FILTER] Found {newLogs.Count} new chat entries");

        foreach (var log in newLogs)
        {
            try
            {
                await SaveLogAsync(log);
            }
            catch (Exception ex)
            {
                DebugLogger.LogException(ex, "SaveLogAsync");
            }
        }

        DebugLog($"[LOAD] Completed LoadLogsAsync in {sw.ElapsedMilliseconds}ms");
    }

    public void StartWatching(int loadAmount = 20)
    {
        if (_watcher != null)
        {
            DebugLog("[WATCHER] Already running");
            return;
        }

        if (!File.Exists(_logFilePath))
            throw new LogfileNotFoundException();

        var dir = Path.GetDirectoryName(_logFilePath)!;
        var file = Path.GetFileName(_logFilePath);
        DebugLog($"[WATCHER] Monitoring '{dir}/{file}'");

        _watcher = new FileSystemWatcher(dir, file)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName
        };

        _watcher.Changed += OnLogFileChanged;
        _watcher.Created += OnLogFileReset;
        _watcher.Renamed += OnLogFileReset;
        _watcher.Deleted += OnLogFileReset;
        _watcher.EnableRaisingEvents = true;

        _pollTimer = new Timer(async _ => await PollLogFileAsync(), null,
            _pollInterval, _pollInterval);

        DebugLog($"[WATCHER] Started successfully with polling every {_pollInterval.TotalSeconds}s");
    }
    
    private async void OnLogFileChanged(object? sender, FileSystemEventArgs e)
    {
        DebugLog($"[WATCHER] File changed event triggered - {e.ChangeType}");
        await DebouncedReload(20);
    }

    private async void OnLogFileReset(object? sender, FileSystemEventArgs e)
    {
        DebugLog($"[WATCHER] Logfile recreated or renamed ({e.ChangeType}) - resetting state");

        _lastFilePosition = 0;
        _lastWriteTimeUtc = DateTime.MinValue;
        _logs.Clear();
        Chats.Clear();

        await DebouncedReload(20);
    }

    public void StopWatching()
    {
        DebugLog("[WATCHER] Stopping file watcher");

        _watcher?.Dispose();
        _pollTimer?.Dispose();
        _watcher = null;
        _pollTimer = null;
        _debounceCts?.Cancel();
        _debounceCts = null;

        DebugLog("[WATCHER] Stopped successfully");
    }

    private async Task PollLogFileAsync()
    {
        try
        {
            if (!File.Exists(_logFilePath))
                return;

            var info = new FileInfo(_logFilePath);

            if (info.Length < _lastFilePosition)
            {
                DebugLog($"[POLL] Detected file truncation - resetting position");
                _lastFilePosition = 0;
                _lastWriteTimeUtc = DateTime.MinValue;
                _logs.Clear();
                Chats.Clear();
                await DebouncedReload(20);
                return;
            }

            if (info.Length > _lastFilePosition || info.LastWriteTimeUtc != _lastWriteTimeUtc)
            {
                DebugLog("[POLL] Detected potential update - reloading");
                await DebouncedReload(10);
            }
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, "PollLogFileAsync");
        }
    }

    // Core
    private async Task SaveLogAsync(Log log)
    {
        DebugLog($"[SAVE] Processing new log entry: {log.RawString}");

        _logs.AddLast(log);

        if (log is not Chat chat)
            return;

        Chats.Insert(0, chat);
        DebugLog($"[CHAT] Added new chat from '{chat.Name}' - {chat.Message}");

        if (_autoTranslate && !string.IsNullOrWhiteSpace(chat.Message))
        {
            if (!string.IsNullOrEmpty(_playerName) &&
                chat.Name.Equals(_playerName, StringComparison.OrdinalIgnoreCase))
            {
                DebugLog("[TRANSLATE] Own message detected - skipping translation");
                chat.Translation = new Translation(_targetLanguage, chat.Message);
            }
            else
            {
                try
                {
                    DebugLog("[TRANSLATE] Translating message...");
                    var sw = Stopwatch.StartNew();
                    chat.Translation = await _translator.TranslateAsync(chat.Message, _targetLanguage);
                    sw.Stop();
                    DebugLog($"[TRANSLATE] Translation finished in {sw.ElapsedMilliseconds}ms - {chat.Translation?.Text}");
                }
                catch (TranslatorException ex)
                {
                    DebugLog($"[TRANSLATE] Translation failed: {ex.Message}");
                    chat.Translation = new Translation(_targetLanguage, $"[error] {ex.Message}");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogException(ex, "Translator general failure");
                }
            }
        }

        DebugLog("[EVENT] Raising ChatReceived & ChatsUpdated");
        ChatReceived?.Invoke(chat);
        ChatsUpdated?.Invoke();
    }

    private async Task DebouncedReload(int loadAmount)
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();

        try
        {
            await Task.Delay(250, _debounceCts.Token);
            DebugLog("[DEBOUNCE] Delay passed - reloading logs");
            await LoadLogsAsync(loadAmount);
        }
        catch (TaskCanceledException)
        {
            DebugLog("[DEBOUNCE] Canceled due to new event");
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, "DebouncedReload");
        }
    }
    
    // PARSING
    private static List<Chat> ParseLines(IEnumerable<string> lines)
    {
        var chats = new List<Chat>();
        foreach (var line in lines)
        {
            if (!Regex.IsMatch(line, @"\s\s\[\w+\]") && !line.Contains("﹫"))
                continue;

            var split = line.Split([": "], 2, StringSplitOptions.None);
            if (split.Length < 2)
                continue;

            var namePart = Regex.Replace(split[0], @"\d{1,2}/\d{1,2} \d{1,2}:\d{1,2}:\d{1,2}", "");
            Match m = Regex.Match(namePart, @"\[(\w+)\]");
            ChatType chatType = ChatType.All;
            if (m.Success && m.Groups.Count > 1)
                if (!Enum.TryParse<ChatType>(m.Groups[1].Value, true, out chatType))
                    continue;

            namePart = Regex.Replace(namePart, @"\[\w+\]", "");
            Match m2 = new Regex(@"﹫(.*)", RegexOptions.IgnoreCase).Match(namePart);
            string? location = null;
            if (m.Success && m.Groups.Count > 1)
                location = m2.Groups[1].Value;

            namePart = Regex.Replace(namePart, @"﹫.*", "").Trim();
            var messagePart = split[1].Trim();

            chats.Add(new Chat(line, chatType, namePart, messagePart, location));
        }

        return chats;
    }

    private List<Log> GetNewLogs(List<Chat> parsed)
    {
        var result = new List<Log>();
        for (var i = parsed.Count - 1; i >= 0; i--)
        {
            if (_logs.Last == null)
            {
                result.Insert(0, parsed[i]);
                continue;
            }

            if (!Compare(_logs.Last, parsed[i]))
                result.Insert(0, parsed[i]);
            else
                break;
        }

        return result;
    }

    private static bool Compare(LinkedListNode<Log> last, Log current)
    {
        var node = last;
        if (node.Value.RawString != current.RawString)
            return false;

        for (var i = 0; i < 3; i++)
        {
            node = node.Previous;
            if (node == null)
                return true;
            if (node.Value.RawString != current.RawString)
                return false;
        }

        return true;
    }
    
    // FILE IO
    private async Task<List<string>> ReadNewLinesAsync()
    {
        var result = new List<string>();
        if (!File.Exists(_logFilePath))
            throw new LogfileNotFoundException();

        await Task.Run(() =>
        {
            using var fs = new FileStream(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (fs.Length < _lastFilePosition)
            {
                DebugLog("[FILE] Truncated - resetting offset");
                _lastFilePosition = 0;
                _lastWriteTimeUtc = DateTime.MinValue;
                _logs.Clear();
                Chats.Clear();
            }

            fs.Seek(_lastFilePosition, SeekOrigin.Begin);
            using var sr = new StreamReader(fs);

            while (sr.ReadLine() is { } line)
                result.Add(line);

            _lastFilePosition = fs.Position;
            _lastWriteTimeUtc = File.GetLastWriteTimeUtc(_logFilePath);
        });

        DebugLog($"[FILE] Read {result.Count} new lines (offset={_lastFilePosition})");
        return result;
    }
}