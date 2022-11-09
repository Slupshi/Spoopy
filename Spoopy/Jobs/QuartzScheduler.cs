using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz.Impl;
using Quartz;
using static Quartz.Logging.OperationName;

namespace Spoopy.Jobs
{
    public class QuartzScheduler
    {
        public static async Task StartPollJob(DateTime date)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();
            IJobDetail job = JobBuilder.Create<PollsJob>().Build();
            ITrigger trigger = TriggerBuilder.Create()
             .WithIdentity("IDGJob", "IDG")
               .StartAt(date)
               .WithPriority(1)
               .Build();
            await scheduler.ScheduleJob(job, trigger);
        }
    }
}
