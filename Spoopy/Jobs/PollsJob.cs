using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Quartz;

namespace Spoopy.Jobs
{
    public class PollsJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {

            await Properties.Slupshi.CreateDMChannelAsync();
            await Properties.Slupshi.SendMessageAsync("Test job");
        }
    }
}
