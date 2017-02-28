using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using FeatureSwitcher.Configuration;
using Microsoft.Azure.WebJobs;
using MogBot.Host.Features;
using MogBot.Host.Settings;
using Nito.AsyncEx;


namespace MogBot.Host
{
    internal class Program : IDisposable
    {
        public void Dispose()
        {
            _disposer?.Dispose();
        }


        private static void Main(string[] args)
        {
            using (var program = new Program())
            {
                try
                {
                    program.Run().Wait();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                    if (Debugger.IsAttached)
                    {
                        Console.WriteLine("Press a key to continue");
                        Console.ReadKey(true);
                    }
                }
            }
        }

        private async Task Run()
        {
            FeatureSwitcher.Configuration.Features.Are.ConfiguredBy.AppConfig().NamedBy.Attribute();

            string webJobId = Environment.GetEnvironmentVariable("WEBJOBS_RUN_ID");
            Action<Task> blockUntilComplete;
            if (webJobId != null)
            {
                var shutdownWatcher = new WebJobsShutdownWatcher();
                _disposer.Add(shutdownWatcher);
                blockUntilComplete = t => { Task.WhenAny(shutdownWatcher.Token.AsTask(), t).Wait(); };
            }
            else
            {
                blockUntilComplete = t =>
                {
                    Console.WriteLine("Press a key to continue");
                    Console.ReadKey(true);
                };
            }

            await RunCore(blockUntilComplete);
        }

        private async Task RunCore(Action<Task> blockUntilComplete)
        {
            using (var mogBot = new MogBot(new AppSettingsProvider()))
            {
                Task completionTask = await mogBot.Start(CancellationToken.None);

                blockUntilComplete(completionTask);

                await mogBot.Stop();
            }
        }


        private readonly CompositeDisposable _disposer = new CompositeDisposable();
    }
}
