using ExchangeRatesUpdater;
using Network;
using Serilog;
using System.Configuration;

namespace Service
{
    internal class Scheduler
    {
        internal static async Task ScheduleTask()
        {
            string configuredRepetetionToBeParsed = ConfigurationManager.AppSettings["configuredRepetetion"];
            int configuredRepetetion = int.Parse(configuredRepetetionToBeParsed);
            
            string startTime = ConfigurationManager.AppSettings["startTime"];
            TimeOnly chosenLaunchTime = TimeOnly.Parse(startTime);
            
            string runImmediately = ConfigurationManager.AppSettings["runImmediately"];
            bool runImmediatelyBool = bool.Parse(runImmediately.ToLower());
            
            int actualTime = TimeOnly.FromDateTime(DateTime.Now).Hour;
            double secondsUntilChosenLaunchTime = 0;

            if (chosenLaunchTime < TimeOnly.FromDateTime(DateTime.Now))
            {
                TimeOnly calculationTime = chosenLaunchTime.AddHours(configuredRepetetion);
                if (calculationTime < TimeOnly.FromDateTime(DateTime.Now))
                {
                    int chosenLaunchTimeInt = chosenLaunchTime.Hour;
                    for (int i = configuredRepetetion; chosenLaunchTime < TimeOnly.FromDateTime(DateTime.Now);)
                    {
                        chosenLaunchTime = chosenLaunchTime.AddHours(i);
                    }
                }
                else
                {
                    chosenLaunchTime = chosenLaunchTime.AddHours(configuredRepetetion);
                }
            }
            else
            {
            }
            if (runImmediatelyBool)
            {
                Log.Information("Task will run immediately");
                secondsUntilChosenLaunchTime = 3;
                chosenLaunchTime = TimeOnly.FromDateTime(DateTime.Now.AddSeconds(secondsUntilChosenLaunchTime));
            }
            else
            {
                Log.Information("Task will run at the specified time");
                secondsUntilChosenLaunchTime = (chosenLaunchTime - TimeOnly.FromDateTime(DateTime.Now)).TotalSeconds;
            }

            string accessToken = await IdentityServiceClient.GetAccessToken();

            var timer = new Timer(async _ =>
                {
                    Log.Information($"Executing scheduled task at: {DateTime.Now}");
                    await ExchangeRateUpdater.ExecuteScheduledTask(accessToken);
                }, null, TimeSpan.FromSeconds(secondsUntilChosenLaunchTime), TimeSpan.FromHours(configuredRepetetion));
            
            Log.Information($"Task execution scheduled successfully for {chosenLaunchTime.Hour}:{chosenLaunchTime.Minute}:{chosenLaunchTime.Second}");
        }
    }
}