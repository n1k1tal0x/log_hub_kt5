sealed class ExternalIncidentReporter
{
    private readonly string _logFilePath;

    public ExternalIncidentReporter(string logFilePath)
    {
        _logFilePath = logFilePath;
    }

    public void Report(ExceptionLogRecord record)
    {
        var destination = "Sentry/Application Insights/Email (stub)";
        Console.WriteLine($"Critical incident routed to {destination}.");

        File.AppendAllText(
            _logFilePath,
            $"[{DateTimeOffset.Now:O}] Level: Info{Environment.NewLine}" +
            $"Context: {record.Context}; Notification=ExternalIncidentReporter{Environment.NewLine}" +
            $"Message: Critical incident was routed to {destination}.{Environment.NewLine}" +
            "StackTrace:" + Environment.NewLine +
            "N/A" + Environment.NewLine +
            new string('-', 80) + Environment.NewLine);
    }
}
