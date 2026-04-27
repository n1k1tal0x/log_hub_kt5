using System.Text;

const string logFilePath = "logs/errors.log";

Directory.CreateDirectory("logs");

var logger = new FileExceptionLogger(logFilePath);
var notifier = new ConsoleAlertNotifier(logger);
var reporter = new ExternalIncidentReporter(logger);
var exceptionHandler = new ExceptionHandler(logger, notifier, reporter);

AppDomain.CurrentDomain.UnhandledException += (_, args) =>
{
    if (args.ExceptionObject is Exception exception)
    {
        exceptionHandler.Handle(
            exception,
            "UnhandledException",
            args.IsTerminating ? ErrorSeverity.Fatal : ErrorSeverity.Error);
    }
};

TaskScheduler.UnobservedTaskException += (_, args) =>
{
    exceptionHandler.Handle(args.Exception, "UnobservedTaskException", ErrorSeverity.Fatal);
    args.SetObserved();
};

var taskManager = new TaskManager();
var app = new TaskManagerApplication(taskManager, exceptionHandler);
app.Run();

return;

enum ErrorSeverity
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}

sealed class TaskManager
{
    private readonly List<string> _tasks = [];

    public void Add(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Task title cannot be empty.", nameof(title));
        }

        if (_tasks.Any(task => string.Equals(task, title, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Task '{title}' already exists.");
        }

        _tasks.Add(title.Trim());
        Console.WriteLine($"Task added: {title.Trim()}");
    }

    public void Remove(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Task title cannot be empty.", nameof(title));
        }

        var taskToRemove = _tasks.FirstOrDefault(task =>
            string.Equals(task, title.Trim(), StringComparison.OrdinalIgnoreCase));

        if (taskToRemove is null)
        {
            throw new KeyNotFoundException($"Task '{title}' was not found.");
        }

        _tasks.Remove(taskToRemove);
        Console.WriteLine($"Task removed: {taskToRemove}");
    }

    public IReadOnlyList<string> List() => _tasks.AsReadOnly();
}

sealed class TaskManagerApplication
{
    private readonly TaskManager _taskManager;
    private readonly ExceptionHandler _exceptionHandler;

    public TaskManagerApplication(TaskManager taskManager, ExceptionHandler exceptionHandler)
    {
        _taskManager = taskManager;
        _exceptionHandler = exceptionHandler;
    }

    public void Run()
    {
        Console.WriteLine("TaskManager");
        Console.WriteLine("Commands: add <task>, remove <task>, list, exit");

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            if (string.Equals(input.Trim(), "exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            ExecuteCommand(input);
        }
    }

    private void ExecuteCommand(string input)
    {
        var parts = input.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0].ToLowerInvariant();
        var argument = parts.Length > 1 ? parts[1] : string.Empty;

        Action operation = command switch
        {
            "add" => () => _taskManager.Add(argument),
            "remove" => () => _taskManager.Remove(argument),
            "list" => ListTasks,
            _ => () => throw new InvalidOperationException($"Unknown command: {command}")
        };

        var severity = command switch
        {
            "list" => ErrorSeverity.Error,
            "add" => ErrorSeverity.Error,
            "remove" => ErrorSeverity.Error,
            _ => ErrorSeverity.Warning
        };

        _exceptionHandler.Execute(operation, $"Operation={command}; Input='{input}'", severity);
    }

    private void ListTasks()
    {
        var tasks = _taskManager.List();

        if (tasks.Count == 0)
        {
            Console.WriteLine("Task list is empty.");
            return;
        }

        Console.WriteLine("Tasks:");
        for (var index = 0; index < tasks.Count; index++)
        {
            Console.WriteLine($"{index + 1}. {tasks[index]}");
        }
    }
}

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

readonly record struct ExceptionLogRecord(
    DateTimeOffset Timestamp,
    ErrorSeverity Severity,
    string Context,
    string Message,
    string StackTrace);

interface IExceptionLogger
{
    void Log(ExceptionLogRecord record);
}

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

interface IAlertNotifier
{
    void Notify(ExceptionLogRecord record);
}

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

interface IIncidentReporter
{
    void Report(ExceptionLogRecord record);
}

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
