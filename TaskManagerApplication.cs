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
