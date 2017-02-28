using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Discord.Commands;
using FeatureSwitcher;
using MogBot.Host.Features;
using MogBot.Host.Foaas;
using Newtonsoft.Json;
using Nito.AsyncEx;
using Refit;

namespace MogBot.Host.BotTasks
{
    public class FoaasTask : TaskBase
    {
        protected override bool IsEnabledFeature => Feature<EnableFoaas>.Is().Enabled;

        public override string Name => "FOAAS";

        public FoaasTask()
        {
            _foaasOperations = new AsyncLazy<IList<FoaasOperation>>(() => LoadFoaasCommands());
        }

        protected override Task InitCore()
        {
            Env.Discord.GetService<CommandService>()
                .CreateCommand("fo")
                .Parameter("PersonOrThing", ParameterType.Required)
                .Do(Foaas);

            return Task.FromResult(0);
        }

        private async Task Foaas(CommandEventArgs arg)
        {
            if (!Env.IsAdultOnly(arg.Channel))
            {
                await arg.Channel.SendMessage($"I'm sorry {arg.User.Name}, I'm afraid I can't do that here.");
                return;
            }

            string message = await GetFoaasMessage(arg.Args[0], arg.User.Name);
            await arg.Channel.SendMessage($"{message} - {arg.User.Name}");
        }

        private async Task<string> GetFoaasMessage(string targetName, string sourceName)
        {
            var foaasOperations = await GetValidOps();
            var random = new Random((int) DateTime.UtcNow.Ticks);
            int item = random.Next(0, foaasOperations.Count);

            FoaasOperation operation = foaasOperations[item];

            Env.Trace.WriteLine($"FOAAS: Chose {operation.Name}", ConsoleColor.Gray);

            string url = operation.Url.UriRelative()
                                  .WithHost("foaas.com")
                                  .WithPath(operation.Url.Replace(":name", targetName).Replace(":from", sourceName))
                                  .ToString();

            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string response = await http.GetStringAsync(url);
                var message = JsonConvert.DeserializeObject<FoaasMessage>(response);

                return message.Message;
            }
        }

        private async Task<IList<FoaasOperation>> GetValidOps()
        {
            var validFields = new[] {"name", "from"};

            var operations = await _foaasOperations;

            // Get the operations which have a "name" field and all fields are in the valid list
            return operations.Where(op => op.Fields.Any(field => field.Field == "name")
                                          && op.Fields.All(field => validFields.Any(valid => valid == field.Field)))
                             .ToArray();
        }

        private async Task<IList<FoaasOperation>> LoadFoaasCommands()
        {
            var foaas = RestService.For<IFoaas>("http://foaas.com");

            return await foaas.GetOperations();
        }

        private readonly AsyncLazy<IList<FoaasOperation>> _foaasOperations;
    }
}