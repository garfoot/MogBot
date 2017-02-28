using System;
using System.Threading.Tasks;

namespace MogBot.Host.BotTasks
{
    public interface IMogBotTask
    {
        Task Init(HostingEnvironment env);
    }
}