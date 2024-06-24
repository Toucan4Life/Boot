using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.MemoryStorage;
using BootPOC.Util;
using Microsoft.Extensions.Options;
using BootPOC.SAL;
using Microsoft.Extensions.Caching.Memory;

namespace BootPOC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.Configure<JobsOptions>(Configuration.GetSection(JobsOptions.Key));
            // Hangfire boot
            services.AddTransient<IBootOptions>(s => new BootOptions
            {
                CacheTTL = new TimeSpan(0,0,5,0),
                Job = s.GetService<IOptions<JobsOptions>>().Value.Jobs.Single(j => j.Identity == "SAPGetLiftListBoot 1.0"),
                LongRunningFunction = () => s.GetService<ISAPSpecificEquipmentServiceAccess>().RetrieveLiftList(true)
            });
            services.AddTransient<IBootOptionsFactory, BootOptionsFactory>();
            services.AddTransient<ISAPSpecificEquipmentServiceAccess, SapSpecificEquipmentServiceAccess>();
            services.AddTransient<IMemoryCache, MemoryCache>();

            services.AddHangfire(config => config.UseMemoryStorage());
                //.UseSqlServerStorage(hangfireOptions.ConnectionString,
                //new SqlServerStorageOptions
                //{
                //    // recommended settings for the database : https://buildmedia.readthedocs.org/media/pdf/hangfire/latest/hangfire.pdf
                //    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                //    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                //    QueuePollInterval = TimeSpan.Zero,
                //    UseRecommendedIsolationLevel = true,
                //    DisableGlobalLocks = true
                //})
            services.AddHangfireServer();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IRecurringJobManager recurringJobManager, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            //It allows us to access the hangfire dashboard in our ASP.NET Core Application.
            app.UseHangfireDashboard("/boot", new DashboardOptions
            {
                IsReadOnlyFunc = context => false
            });
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            foreach (var bootOptions in serviceProvider.GetServices<IBootOptions>())
            {
                recurringJobManager.AddOrUpdate(bootOptions.Job.Identity,
                    () => new Bootable( bootOptions, serviceProvider.GetService<IBootOptionsFactory>()).Execute(bootOptions.Job.Identity), bootOptions.Job.CronSchedule);
            }
        }
    }
}
