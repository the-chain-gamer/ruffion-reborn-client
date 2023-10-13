using Godot;
using System;
using System.Runtime.InteropServices;

public class Logger : Node
{
    public static Logger UiLogger;

    public enum LogLevel
    {
        INFO,
        WARNING,
        ERROR
    }

    // Import the JavaScript function to print logs to the browser console
    [DllImport("__Internal")]
    private static extern void ConsoleLog(string message);

    public override void _Ready()
    {
        UiLogger = this;
    }

    public void Log(LogLevel level, string message)
    {
        string prefix;
        switch (level)
        {
            case LogLevel.INFO:
                prefix = "[INFO]";
                break;
            case LogLevel.WARNING:
                prefix = "[WARNING]";
                break;
            case LogLevel.ERROR:
                prefix = "[ERROR]";
                break;
            default:
                prefix = "[UNKNOWN]";
                break;
        }

        string logMessage = $"{prefix} {message}";
        // Custom handling of logs, e.g., appending to a log file or displaying in-game UI
        GetChild<RichTextLabel>(0).Text = "\n" + logMessage;
        // Optionally, print logs to the browser console in a WebGL build
        // if (OS.GetName() == "HTML5")
        // {
        //     ConsoleLog(logMessage);
        // }
    }
}
