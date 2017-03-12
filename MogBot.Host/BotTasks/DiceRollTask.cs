using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using FeatureSwitcher;
using MogBot.Host.Extensions;
using MogBot.Host.Features;

namespace MogBot.Host.BotTasks
{
    public class DiceRollTask : TaskBase
    {
        protected override bool IsEnabledFeature => Feature<EnableDiceRoll>.Is().Enabled;
        public override string Name => "DiceRoll";
        protected override Task InitCore()
        {
            Env.Discord.GetService<CommandService>()
                .CreateCommand("roll")
                .Alias("r")
                .Parameter("diceSpec", ParameterType.Multiple)
                .Description("Roll some dice. Spec as a space separated list of dice. Eg. 2d6+1 4d4 would roll 2xd6 and add 1 along with 4xd4 and total them.")
                .Do(Roll);

            return Task.FromResult(0);
        }

        private Task Roll(CommandEventArgs args)
        {
            var random = new Random((int) DateTime.UtcNow.Ticks);

            var diceSpec = new Regex("(?\'times\'\\d+)d(?\'num\'\\d+)((?\'modSign\'[+-]{1})(?\'modAmount\'\\d+))?");

            var nums = new Dictionary<string, DiceRollInfo>();

            foreach (string diceInfo in args.Args)
            {
                var match = diceSpec.Match(diceInfo);
                if (match.Success)
                {
                    int num = int.Parse(match.Groups["num"].Value);
                    int mul = int.Parse(match.Groups["times"].Value);

                    var current = new List<int>();
                    for (int i = 0; i < mul; i++)
                    {
                        current.Add(random.Next(1, num + 1));
                    }

                    int? mod = null;
                    if (match.Groups["modSign"].Success
                        && match.Groups["modAmount"].Success)
                    {
                        string sign = match.Groups["modSign"].Value;
                        int modAmount = int.Parse(match.Groups["modAmount"].Value);
                        mod = modAmount * (sign == "+" ? 1 : -1);
                    }

                    var info = new DiceRollInfo
                    {
                        Nums = current,
                        Spec = diceInfo,
                        Total = current.Aggregate(0, (tot, cur) => tot + cur) + (mod ?? 0),
                        Mod = mod
                    };


                    nums.Add(diceInfo, info);
                }
            }



            var diceRolls = nums.Select(i =>
            {
                string mod = null;
                
                if (i.Value.Mod.HasValue)
                {
                    mod = $"{i.Value.Mod:+#;-#;0}";
                }

                return $"{i.Value.Spec} (**{string.Join(", ", i.Value.Nums)}**){mod} = ***{i.Value.Total}***";
            });

            var total = nums.Aggregate(0, (c, i) => c + i.Value.Total);

            string totalMessage = null;
            if (nums.Count > 1)
            {
                totalMessage = $" for a total of ***{total}***";
            }

            var name = args.User.NicknameMention ?? args.User.Nickname ?? args.User.Name;
            args.Channel.SendMessage($"{name} rolled {string.Join(", ", diceRolls)}{totalMessage}");

            return Task.FromResult(0);
        }

        private class DiceRollInfo
        {
            public int? Mod { get; set; }
            public IList<int> Nums { get; set; }
            public int Total { get; set; }
            public string Spec { get; set; }
        }
    }
}