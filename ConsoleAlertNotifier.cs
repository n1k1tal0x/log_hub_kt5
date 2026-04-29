sealed class ConsoleAlertNotifier : IAlertNotifier
{
    private readonly IExceptionLogger _logger;

    public ConsoleAlertNotifier(IExceptionLogger logger)
    {
        _logger = logger;
    }

    public void Notify(ExceptionLogRecord record)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("!!! Ошибка !!!");
        Console.ResetColor();
        Console.WriteLine($"[{record.Severity}] {record.Message}");

        _logger.Log(new ExceptionLogRecord(
            DateTimeOffset.Now,
            ErrorSeverity.Info,
            $"{record.Context}; Notification=ConsoleAlert",
            "Console alert was displayed for the captured exception.",
            "N/A"));
    }
}
