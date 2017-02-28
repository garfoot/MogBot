using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using FeatureSwitcher;
using MogBot.Host.BotTasks;
using MogBot.Host.Features;
using MogBot.Host.Settings;
using Nito.AsyncEx;

// To register MogBot hit this URL and assign to a server
// https://discordapp.com/oauth2/authorize?client_id=285924018954043392&scope=bot&permissions=0x00007C47

namespace MogBot.Host
{
    internal class MogBot : IDisposable
    {
        private readonly ISettings _settings;

        public MogBot(ISettings settings)
        {
            _settings = settings;
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
                i.PrefixChar = '#';
                i.HelpMode = HelpMode.Private;
            });

            var foaasTask = new FoaasTask();
            await foaasTask.Init(new HostingEnvironment(_discord, new ConsoleTrace()));

            await _discord.Connect(_settings.GetSetting(DefinedSettings.BotToken), TokenType.Bot);

            _discord.SetGame("with Kupo Bot");

            return _tcs.Task;
        }


        public async Task Stop()
        {
            await _discord.Disconnect();
            _tcs.TrySetResult();
        }

        private readonly CompositeDisposable _disposer = new CompositeDisposable();
        private readonly TaskCompletionSource _tcs = new TaskCompletionSource();
        private DiscordClient _discord;
    }
}