using Service;
using System.Diagnostics;

namespace ExchangeRatesUpdater
{
    public class App
    {
        public async Task Start()
        {
            await Task.Run(async () =>
            {
            await Scheduler.ScheduleTask();
            });
        }
        public void Stop()
        {
            ///launcher.Dispose();
        }
    }
}
