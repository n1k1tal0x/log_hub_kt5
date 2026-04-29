readonly record struct ExceptionLogRecord(
    DateTimeOffset Timestamp,
    ErrorSeverity Severity,
    string Context,
    string Message,
    string StackTrace);
