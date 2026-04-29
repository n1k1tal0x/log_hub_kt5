sealed class ExceptionHandler
{
    private readonly IExceptionLogger _logger;
    private readonly IAlertNotifier _notifier;
    private readonly IIncidentReporter _reporter;

    public ExceptionHandler(
        IExceptionLogger logger,
        IAlertNotifier notifier,
        IIncidentReporter reporter)
    {
        _logger = logger;
        _notifier = notifier;
        _reporter = reporter;
    }

    public void Execute(Action operation, string context, ErrorSeverity severity)
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

        _logger.Log(record);
        _notifier.Notify(record);

        if (severity >= ErrorSeverity.Fatal)
        {
            _reporter.Report(record);
        }
    }
}
