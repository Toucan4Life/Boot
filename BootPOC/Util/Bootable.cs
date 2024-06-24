using System;
using System.Diagnostics;
using System.Reflection;
using Hangfire;
using Microsoft.AspNetCore.Mvc.Abstractions;
using NCrontab;

namespace BootPOC.Util
{
    // https://docs.hangfire.io/en/latest/background-methods/performing-recurrent-tasks.html.
    // If job take more time than the next scheduled job, Hangfire logs warnings about not be able to acquire the lock, So scheduled jobs appear to accumulate
    [AutomaticRetry(Attempts = 5, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public class Bootable : IBootable
    {
        private readonly IBootOptionsFactory _bootOptionsFactory;

        public Bootable(IBootOptions bootOptions, IBootOptionsFactory bootOptionsFactory)
        {
            _bootOptionsFactory = bootOptionsFactory;
            BootOptions = bootOptions;
            EnsureDataIsAlwaysAvailable();
        }

        public void Execute(string jobIdentity)
        {
            // Hangfire recreate a bootable object at each trigger of the Execute function.
            // Since there is multiple injected BootOptions, the injected BootOptions is always the last one for all the jobs.
            // To solve the problem we override the bootOptions by creating the right one with the bootOptionsFactory.
            this.BootOptions = _bootOptionsFactory.Create(jobIdentity);

            var watch = Stopwatch.StartNew();
            try
            {
                BootOptions.LongRunningFunction();

                watch.Stop();

                Console.Write(BootOptions.Job.Identity + "started");
            }
            catch (Exception ex)
            {
                watch.Stop();

                Console.Write(BootOptions.Job.Identity + "finished in " + watch.ElapsedMilliseconds);
            }
        }

        public IBootOptions BootOptions { get; private set; }

        private void EnsureDataIsAlwaysAvailable()
        {
            if (!IsDataIsAlwaysAvailable())
                throw new ArgumentException(
                    $"Job {BootOptions.Job.Identity}: Invalid settings, should be : cacheTTLDelayInMinutes {BootOptions.CacheTTL} >= cronRecurrenceTimeSpan: {BootOptions.Job.CronSchedule} > maxRealJobExecutionDelayInMinutes: {BootOptions.Job.MaxExecutionTime}");
        }

        private bool IsDataIsAlwaysAvailable()
        {
            var cronRecurrenceTimeSpan = RetrieveIntervalOfCronExpression(BootOptions.Job.CronSchedule);

            return BootOptions.CacheTTL >= cronRecurrenceTimeSpan + BootOptions.Job.MaxExecutionTime && cronRecurrenceTimeSpan > BootOptions.Job.MaxExecutionTime;
        }

        private TimeSpan RetrieveIntervalOfCronExpression(string cronExpression)
        {
            var deserializedCronExpression = CrontabSchedule.Parse(cronExpression);

            var cronNextOccurrenceDateTimeFirst = deserializedCronExpression.GetNextOccurrence(DateTime.Now);

            //Get Cron occurrence with delta between two first occurrences
            var cronRecurrenceTimeSpan = deserializedCronExpression.GetNextOccurrence(cronNextOccurrenceDateTimeFirst) -
                                         cronNextOccurrenceDateTimeFirst;
            return cronRecurrenceTimeSpan;
        }
    }
}