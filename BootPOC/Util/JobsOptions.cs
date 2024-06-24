using System;
using System.Collections.Generic;

namespace BootPOC.Util
{
    public class JobsOptions
    {
        public List<JobsOption> Jobs { get; set; }
        public static string Key => "Jobs";
    }

    public class JobsOption
    {
        public string Identity { get; set; }
        public string CronSchedule { get; set; }
        public TimeSpan MaxExecutionTime { get; set; }
    }
}