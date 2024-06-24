using System;

namespace BootPOC.Util
{
    public interface IBootOptions
    {
        JobsOption Job { get; set; }
        TimeSpan CacheTTL { get; set; }
        Action LongRunningFunction { get; set; }
    }
}