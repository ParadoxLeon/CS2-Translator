using Avalonia;
using ReactiveUI.Avalonia;
using System;
using System.Linq;
using CS2.Translator.Core.Helper;

namespace CS2.Translator.UI.Desktop;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        bool enableDebug = args.Contains("-debug", StringComparer.OrdinalIgnoreCase);
        
        DebugLogger.Initialize(enableDebug);
        
        DebugLogger.Log("===============================================");
        DebugLogger.Log("CS2.Translator started");
        DebugLogger.Log($"OS: {Environment.OSVersion}");
        DebugLogger.Log($".NET Runtime: {Environment.Version}");
        DebugLogger.Log($"Debug Mode: {(enableDebug ? "ON" : "OFF")}");
        DebugLogger.Log("===============================================");

        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, "Unhandled exception in Main()");
            throw;
        }
        finally
        {
            DebugLogger.Log("CS2.Translator shutdown");
        }
    }

    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseReactiveUI(_ => { })
            .LogToTrace();
}