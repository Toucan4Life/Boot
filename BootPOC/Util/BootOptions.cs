using System;

namespace BootPOC.Util
{
    public class BootOptions : IBootOptions
    {
        public JobsOption Job { get; set; }
        public TimeSpan CacheTTL { get; set; }
        public Action LongRunningFunction { get; set; }
    }
}