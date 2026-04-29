using System.Text;

sealed class FileExceptionLogger : IExceptionLogger
{
    private readonly string _logFilePath;

    public FileExceptionLogger(string logFilePath)
    {
        _logFilePath = logFilePath;
    }

    public void Log(ExceptionLogRecord record)
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
}
