using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MogBot.Host.BotTasks;
using MogBot.Host.Settings;
using Nito.AsyncEx;

// To register MogBot hit this URL and assign to a server
// https://discordapp.com/oauth2/authorize?client_id=285924018954043392&scope=bot&permissions=0x0000FC47

namespace MogBot.Host
{
    internal class MogBot : IDisposable
    {
        public MogBot(
            ISettings settings,
            IList<IMogBotTask> tasks,
            HostingEnvironment.Create hostingEnvironmentFactory
        )
        {
            _settings = settings;
            _tasks = tasks;
            _hostingEnvironmentFactory = hostingEnvironmentFactory;
        }

        public void Dispose()
        {
            _disposer?.Dispose();
        }

        /// <summary>
        ///     Start the mog bot.
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns>A task of task. The outer task completes when startup is done, the inner task completes on shutdown.</returns>
        public async Task<Task> Start(CancellationToken cancellation)
        {
            _discord = new DiscordClient();
            _disposer.Add(_discord);

            _discord.UsingCommands(i =>
            {
                i.PrefixChar = '!';
                i.HelpMode = HelpMode.Private;
            });

            await InitTasks();

            await _discord.Connect(_settings.GetSetting(DefinedSettings.BotToken), TokenType.Bot);

            _discord.SetGame("with Kupo Bot");

            return _tcs.Task;
        }


        public async Task Stop()
        {
            await _discord.Disconnect();
            _tcs.TrySetResult();
        }

        private async Task InitTasks()
        {
            HostingEnvironment env = _hostingEnvironmentFactory(_discord);

            foreach (IMogBotTask task in _tasks)
            {
                await task.Init(env);
            }
        }

        private readonly CompositeDisposable _disposer = new CompositeDisposable();
        private readonly HostingEnvironment.Create _hostingEnvironmentFactory;
        private readonly ISettings _settings;
        private readonly IList<IMogBotTask> _tasks;
        private readonly TaskCompletionSource _tcs = new TaskCompletionSource();
        private DiscordClient _discord;
    }
}