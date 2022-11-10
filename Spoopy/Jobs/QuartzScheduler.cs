using System;
using System.Threading.Tasks;
using Quartz.Impl;
using Quartz;

namespace Spoopy.Jobs
{
    public class QuartzScheduler
    {
        public static async Task StartBasicPollJob(DateTime date, long messageId)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();
            IJobDetail job = JobBuilder.Create<BasicPollsJob>().Build();
            ITrigger trigger = TriggerBuilder.Create()
             .WithIdentity("IDGJob", "IDG")
               .StartAt(date)
               .UsingJobData("id", messageId)
               .WithPriority(1)
               .Build();
            await scheduler.ScheduleJob(job, trigger);
        }

        public static async Task StartComplexPollJob(DateTime date, long messageId)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();
            IJobDetail job = JobBuilder.Create<ComplexPollJob>().Build();
            ITrigger trigger = TriggerBuilder.Create()
             .WithIdentity("IDGJob", "IDG")
               .StartAt(date)
               .UsingJobData("id", messageId)
               .WithPriority(1)
               .Build();
            await scheduler.ScheduleJob(job, trigger);
        }
    }
}
