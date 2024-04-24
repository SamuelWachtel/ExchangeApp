using Serilog;
using System.Configuration;
using System.Diagnostics;
using Topshelf;

namespace ExchangeRatesUpdater
{
    class ProgramProperties
    {
        static async Task Main(string[] args)
        {
            var configFilePath = ConfigurationManager.AppSettings["pathToConfigFile"];

            FileSystemWatcher configWatcher = new FileSystemWatcher(Path.GetDirectoryName(configFilePath), Path.GetFileName(configFilePath));
            configWatcher.NotifyFilter = NotifyFilters.LastWrite;
            configWatcher.Changed += OnConfigFileChanged;
            configWatcher.EnableRaisingEvents = true;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(ConfigurationManager.AppSettings["pathToLogsFile"],
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 200000,
                    retainedFileCountLimit: 5
                    )
                .CreateLogger();
            Log.Information($"\n" +
                $"\n---------------------------------------------------------------------\n" +
                $"\nLog from {DateTime.Now}\n" +
                $"\n---------------------------------------------------------------------\n" +
                $"\n");

            var result = await Task.Run(() => HostFactory.Run(x =>
            {
                x.Service<App>(s =>
                {
                    s.ConstructUsing(name => new App());
                    s.WhenStarted(tc => tc.Start().GetAwaiter().GetResult());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();
                x.SetServiceName("ExchangeRatesUpdaterApp");
                x.SetDisplayName("Exchange Rates Updater App");
                x.SetDescription("App to update exchange rates automatically.");
            }));

            var exitCode = (int)Convert.ChangeType(result, result.GetTypeCode());

            Log.Debug("Service execution result: {Result}", result);
            Log.Debug("Exit code: {ExitCode}", exitCode);

            Environment.ExitCode = exitCode;
        }

        private static void OnConfigFileChanged(object sender, FileSystemEventArgs e)
        {

            string assemblyPath = ConfigurationManager.AppSettings["pathToApplication"];
            Process.Start(assemblyPath);
            Environment.Exit(0);
        }
    }
}