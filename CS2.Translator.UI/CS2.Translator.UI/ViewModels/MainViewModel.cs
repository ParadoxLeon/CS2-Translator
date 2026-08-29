using System;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CS2.Translator.Core.Exceptions;
using CS2.Translator.Core.Models;
using CS2.Translator.Core.Services;
using CS2.Translator.Core.Helper;

namespace CS2.Translator.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private const int MaxChats = 150;
    public AvaloniaList<Chat> Chats { get; } = new();

    private readonly LogsService _logsService;
    private readonly ConfigService _configService;

    [ObservableProperty]
    private string _statusText = "Idle";

    public event Action? SettingsRequested;

    public double NameFontSize => _configService.Config.NameFontSize;
    public double TranslationFontSize => _configService.Config.TranslationFontSize;

    public MainViewModel(
        LogsService logsService,
        ConfigService configService)
    {
        _logsService = logsService;
        _configService = configService;

        _logsService.ChatReceived += OnChatReceived;
        
        _configService.ConfigChanged += OnConfigChanged;

        DebugLog("MainViewModel initialized");
        _ = InitializeAsync();
    }
    
    private static void DebugLog(string msg)
    {
        string formatted = $"[MainViewModel] | {msg}";
        Console.WriteLine(formatted);
        DebugLogger.Log(formatted);
    }

    private void OnConfigChanged()
    {
        try
        {
            DebugLog("ConfigChanged event received");

            OnPropertyChanged(nameof(NameFontSize));
            OnPropertyChanged(nameof(TranslationFontSize));

            DebugLog("Font size properties updated");
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, "OnConfigChanged");
        }
    }

    private async Task InitializeAsync()
    {
        try
        {
            DebugLog("InitializeAsync started");
            StatusText = "Loading logs…";

            await _logsService.LoadLogsAsync(30);
            DebugLog("Initial logs loaded");

            _logsService.StartWatching();
            DebugLog("Log watcher started");

            await Dispatcher.UIThread.InvokeAsync(FullRefresh);
            DebugLog("UI refresh completed after initialization");

            StatusText = "Watching CS2 console.log";
            DebugLog("Status updated - Watching CS2 console.log");
        }
        catch (LogfileNotFoundException)
        {
            StatusText = "Waiting for CS2 (console.log not found)";
            DebugLog("LogfileNotFoundException: console.log missing");
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
            DebugLogger.LogException(ex, "InitializeAsync");
        }
    }

    private void OnChatReceived(Chat chat)
    {
        try
        {
            DebugLog($"Chat received from '{chat.Name}': {chat.Message}");

            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    Chats.Insert(0, CloneForUi(chat));
                    EnforceLimit();

                    DebugLog($"Chat added to UI (Chats.Count={Chats.Count})");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogException(ex, "UIThread Chat Add");
                }
            });
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, "OnChatReceived");
        }
    }

    private void FullRefresh()
    {
        try
        {
            DebugLog("FullRefresh() started");
            
            Chats.Clear();
            foreach (var chat in _logsService.Chats)
            {
                Chats.Add(CloneForUi(chat));
                EnforceLimit();
            }

            DebugLog($"FullRefresh() completed - total chats: {Chats.Count}");
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, "FullRefresh");
        }
    }

    private void EnforceLimit()
    {
        try
        {
            while (Chats.Count > MaxChats)
                Chats.RemoveAt(0);

            DebugLog($"EnforceLimit() applied (Chats.Count={Chats.Count})");
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, "EnforceLimit");
        }
    }

    [RelayCommand]
    private void OpenSettings()
    {
        try
        {
            DebugLog("OpenSettings() invoked");
            SettingsRequested?.Invoke();
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, "OpenSettings");
        }
    }

    [RelayCommand]
    private async Task Reload()
    {
        try
        {
            DebugLog("Reload command triggered");
            StatusText = "Reloading…";
            
            await _logsService.LoadLogsAsync(30);
            DebugLog("Logs reloaded");

            await Dispatcher.UIThread.InvokeAsync(FullRefresh);
            DebugLog("UI refreshed after reload");
            StatusText = "Reloaded";
        }
        catch (Exception ex)
        {
            StatusText = $"Error during reload: {ex.Message}";
            DebugLogger.LogException(ex, "Reload");
        }
    }

    [RelayCommand]
    private void ClearChats()
    {
        try
        {
            DebugLog("ClearChats() invoked");

            // Clear service-backed chats and UI list
            _logsService.Chats.Clear();
            Dispatcher.UIThread.Post(() => Chats.Clear());

            StatusText = "Chats cleared";
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, "ClearChats");
        }
    }

    private static Chat CloneForUi(Chat c)
    {
        return new Chat(
            rawString: c.RawString,
            chatType: c.ChatType,
            name: c.Name,
            message: c.Message,
            location: c.Location
        )
        {
            Translation = c.Translation
        };
    }
}
