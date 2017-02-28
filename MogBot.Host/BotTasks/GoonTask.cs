using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Discord;
using FeatureSwitcher;
using MogBot.Host.Features;

namespace MogBot.Host.BotTasks
{
    public class GoonTask : TaskBase
    {
        public override string Name { get; } = "GoonTask";
        protected override bool IsEnabledFeature => Feature<EnableGoon>.Is().Enabled;

        protected override Task InitCore()
        {
            Disposer.Add(Observable
                .FromEventPattern<MessageEventArgs>(h => Env.Discord.MessageReceived += h, h => Env.Discord.MessageReceived -= h)
                .Select(i => i.EventArgs)
                .Subscribe(NewMessage));

            return Task.FromResult(0);
        }

        private void NewMessage(MessageEventArgs message)
        {
            if (message.Message.Text.IndexOf("goon", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                message.Channel.SendMessage("GOOOOOOOOON!");
            }
        }
    }
}