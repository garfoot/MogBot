using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using MogBot.Host.Extensions;

namespace MogBot.Host.BotTasks
{
    public abstract class TaskBase : IMogBotTask, IDisposable
    {
        public abstract string Name { get; }

        protected HostingEnvironment Env { get; private set; }

        protected virtual bool IsEnabledFeature => true;

        public void Dispose()
        {
            Disposer?.Dispose();
        }

        public Task Init(HostingEnvironment env)
        {
            Env = env;

            ConsoleExtensions.Write($"{Name} is ", ConsoleColor.White);

            if (IsEnabledFeature)
            {
                ConsoleExtensions.WriteLine("enabled", ConsoleColor.Green);
                return InitCore();
            }
            ConsoleExtensions.WriteLine("disabled", ConsoleColor.Yellow);

            return Task.FromResult(0);
        }

        protected abstract Task InitCore();
        protected readonly CompositeDisposable Disposer = new CompositeDisposable();
    }
}