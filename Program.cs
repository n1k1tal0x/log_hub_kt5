internal static class Program
{
    public static void Main()
    {
        const string logFilePath = "logs/errors.log";

        Directory.CreateDirectory("logs");

        var exceptionHandler = new ExceptionHandler(logFilePath);

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
    }
}
