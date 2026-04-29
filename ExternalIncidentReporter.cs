sealed class ExternalIncidentReporter : IIncidentReporter
{
    private readonly IExceptionLogger _logger;

    public ExternalIncidentReporter(IExceptionLogger logger)
    {
        _logger = logger;
    }

    public void Report(ExceptionLogRecord record)
    {
        var destination = "Sentry/Application Insights/Email (stub)";
        Console.WriteLine($"Critical incident routed to {destination}.");

        _logger.Log(new ExceptionLogRecord(
            DateTimeOffset.Now,
            ErrorSeverity.Info,
            $"{record.Context}; Notification=ExternalIncidentReporter",
            $"Critical incident was routed to {destination}.",
            "N/A"));
    }
}
