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
