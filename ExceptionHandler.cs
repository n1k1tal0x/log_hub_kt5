using System.Text;

sealed class ExceptionHandler
{
    private readonly string _logFilePath;
    private readonly ExternalIncidentReporter _reporter;

    public ExceptionHandler(string logFilePath)
    {
        _logFilePath = logFilePath;
        _reporter = new ExternalIncidentReporter(logFilePath);
    }

    public void Execute(Action operation, string context, ErrorSeverity severity = ErrorSeverity.Error)
    {
        try
        {
            operation();
        }
        catch (Exception exception)
        {
            Handle(exception, context, severity);
        }
    }

    public void Handle(Exception exception, string context, ErrorSeverity severity)
    {
        var record = new ExceptionLogRecord(
            DateTimeOffset.Now,
            severity,
            context,
            exception.Message,
            exception.StackTrace ?? "Stack trace is not available.");

        Log(record);
        Notify(record);

        if (severity == ErrorSeverity.Fatal)
        {
            _reporter.Report(record);
        }
    }

    private void Log(ExceptionLogRecord record)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"[{record.Timestamp:O}] Level: {record.Severity}");
        builder.AppendLine($"Context: {record.Context}");
        builder.AppendLine($"Message: {record.Message}");
        builder.AppendLine("StackTrace:");
        builder.AppendLine(record.StackTrace);
        builder.AppendLine(new string('-', 80));

        File.AppendAllText(_logFilePath, builder.ToString());
    }

    private void Notify(ExceptionLogRecord record)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("!!! Ошибка !!!");
        Console.ResetColor();
        Console.WriteLine($"[{record.Severity}] {record.Message}");

        Log(new ExceptionLogRecord(
            DateTimeOffset.Now,
            ErrorSeverity.Info,
            $"{record.Context}; Notification=ConsoleAlert",
            "Console alert was displayed for the captured exception.",
            "N/A"));
    }
}
