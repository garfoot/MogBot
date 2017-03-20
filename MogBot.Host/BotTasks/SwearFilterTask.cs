using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using FeatureSwitcher;
using MogBot.Host.Features;

namespace MogBot.Host.BotTasks
{
    public class SwearFilterTask : TaskBase
    {
        private readonly List<string> _bannedWords = new List<string>();

        public override string Name { get; } = "SwearFilter";
        protected override bool IsEnabledFeature => Feature<EnableGoon>.Is().Enabled;

        protected override async Task InitCore()
        {
            await LoadWords();

            Disposer.Add(Observable
                .FromEventPattern<MessageEventArgs>(h => Env.Discord.MessageReceived += h, h => Env.Discord.MessageReceived -= h)
                .Select(i => i.EventArgs)
                .Subscribe(NewMessage));
        }

        private async Task LoadWords()
        {
            var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fileName = Path.Combine(dirName, "Content", "banned_words.txt");

            using (var reader = new StreamReader(fileName))
            {
                var line = await reader.ReadLineAsync();

                while (line != null)
                {
                    line = line.Trim();
                    if (line.StartsWith("#"))
                    {
                        continue;
                    }

                    _bannedWords.Add(line);

                    line = await reader.ReadLineAsync();
                }
            }
        }

        private async void NewMessage(MessageEventArgs message)
        {
            // If the channel is adult only or it's a private message then ignore this filter
            // unless they're a kid
            if (!Env.IsKid(message.User)
                && (Env.IsAdultOnly(message.Channel) || message.Channel.IsPrivate))
            {
                return;
            }


            // Really crude filter, just looks for words in a banned list and replaces them
            var replacementWord = @"$@#!\&*";

            bool isReplaced = false;
            string messageText = message.Message.Text;
            foreach (string word in _bannedWords)
            {
                int position = messageText.IndexOf(word, StringComparison.OrdinalIgnoreCase);

                while (position >= 0)
                {
                    string foundWord = messageText.Substring(position, word.Length);
                    messageText = messageText.Replace(foundWord, replacementWord);
                    isReplaced = true;

                    position = messageText.IndexOf(word, StringComparison.OrdinalIgnoreCase);
                }
            }

            if (isReplaced)
            {
                await message.Message.Delete();

                var msg = await message.Channel.SendMessage($"**Naughty {message.User.NicknameMention} said something they shouldn't" +
                                                            $" have**\r\n\r\n \"{messageText}\"");
                
            }
        }
    }
}