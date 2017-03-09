using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using MogBot.Host.Extensions;

namespace MogBot.Host.BotTasks
{
    public class DiceRollTask : TaskBase
    {
        public override string Name => "DiceRoll";
        protected override Task InitCore()
        {
            Env.Discord.GetService<CommandService>()
                .CreateCommand("roll")
                .Parameter("diceSpec", ParameterType.Multiple)
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
                        current.Add(random.Next(1, num));
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

                return $"{i.Value.Spec} ({string.Join(", ", i.Value.Nums)}){mod} = {i.Value.Total}";
            });

            var total = nums.Aggregate(0, (c, i) => c + i.Value.Total);

            args.Channel.SendMessage($"{args.User.Name} rolled {string.Join(", ", diceRolls)} for a total of {total}");

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