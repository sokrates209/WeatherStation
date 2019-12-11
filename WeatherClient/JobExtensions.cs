using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;

namespace WeatherClient
{
    public static class JobExtensions
    {
        private static readonly string JOB_GROUP_NAME = "WeatherStationClient";

        public static void AddJobScheduler(this IServiceCollection services)
        {
            services.AddJobs();

            services.AddSingleton<JobFactory>();

            services.AddSingleton(provider =>
            {
                var factory = provider.GetService<JobFactory>();
                var scheduler = (new StdSchedulerFactory()).GetScheduler().Result;

                scheduler.JobFactory = factory;

                scheduler.StartDelayed(TimeSpan.FromSeconds(10));

                return scheduler;
            });
        }

        public static async Task ScheduleJobs(this IServiceProvider provider)
        {
            var scheduler = provider.GetService<IScheduler>();
            await scheduler.Schedule<WeatherStationClient>(builder =>
                builder.WithMisfireHandlingInstructionIgnoreMisfires().WithIntervalInSeconds(10).RepeatForever());
        }

        private static void AddJobs(this IServiceCollection services)
        {
            services.AddTransient<WeatherStationClient>();
        }

        private static async Task Schedule<T>(this IScheduler scheduler, Action<SimpleScheduleBuilder> schedule)
            where T : IJob
        {
            var jobDetail = JobBuilder.Create<T>()
                .WithIdentity(typeof(T).Name, JOB_GROUP_NAME)
                .Build();

            var jobTrigger = TriggerBuilder.Create()
                .WithIdentity($"{typeof(T).Name}_Trigger", JOB_GROUP_NAME)
                .StartNow()
                .WithSimpleSchedule(schedule)
                .Build();

            await scheduler.ScheduleJob(jobDetail, jobTrigger);
        }
    }
}