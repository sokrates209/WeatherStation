using System;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Spi;

namespace WeatherClient
{
    public class JobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobFactory> _logger;

        public JobFactory(IServiceProvider serviceProvider, ILogger<JobFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                var jobDetail = bundle.JobDetail;

                var jobType = jobDetail.JobType;
                _logger.LogDebug($"Producing instance of Job '{jobDetail.Key}', class={jobType.FullName}");

                var resolvedJob = _serviceProvider.GetService(jobType);
                var job = resolvedJob as IJob;

                scheduler.ResumeAll();
                return job;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(IJobFactory.NewJob));
                throw new SchedulerException($"Problem instantiating class '{bundle.JobDetail.JobType.FullName}'", ex);
            }
        }

        public void ReturnJob(IJob job)
        {
        }
    }
}