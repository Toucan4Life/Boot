using System;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;

namespace BootPOC.SAL
{
    public class SapSpecificEquipmentServiceAccess : ISAPSpecificEquipmentServiceAccess
    {
        private readonly IMemoryCache _memoryCache;

        public SapSpecificEquipmentServiceAccess(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }
        public void RetrieveLiftList(bool forcePutInCache = false)
        {
            if (forcePutInCache)
            {
                //longrunningtask
                Thread.Sleep(2000);
                _memoryCache.Set("RetrieveLiftList", 2);
                Console.WriteLine("Putting in cache");
            }
            _memoryCache.Get("RetrieveLiftList");
            Console.WriteLine("Retrieving from cache");
        }
    }
}