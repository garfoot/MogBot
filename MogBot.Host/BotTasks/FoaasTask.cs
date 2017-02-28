﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using FeatureSwitcher;
using MogBot.Host.Features;
using MogBot.Host.Foaas;
using Newtonsoft.Json;
using Nito.AsyncEx;
using Refit;

namespace MogBot.Host.BotTasks
{
    public class FoaasTask
    {
        public FoaasTask()
        {
            _foaasOperations = new AsyncLazy<IList<FoaasOperation>>(() => LoadFoaasCommands());
        }

        public Task Init(HostingEnvironment env)
        {
            _env = env;

            _env.Trace.Write("FOAAS task is ", ConsoleColor.White);

            if (Feature<EnableFoaas>.Is().Disabled)
            {
                _env.Trace.WriteLine("disabled", ConsoleColor.Yellow);
                return Task.FromResult(0);
            }

            _env.Trace.WriteLine("enabled", ConsoleColor.Green);


            _env.Discord.GetService<CommandService>()
                   .CreateCommand("fo")
                   .Parameter("PersonOrThing", ParameterType.Required)
                   .Do(Foaas);

            return Task.FromResult(0);
        }

        private async Task Foaas(CommandEventArgs arg)
        {
            string message = await GetFoaasMessage(arg.Args[0], arg.User.Name);
            await arg.Channel.SendMessage($"{message} - {arg.User.Name}");
        }

        private async Task<string> GetFoaasMessage(string targetName, string sourceName)
        {
            var foaasOperations = await GetValidOps();
            var random = new Random((int) DateTime.UtcNow.Ticks);
            int item = random.Next(0, foaasOperations.Count);

            FoaasOperation operation = foaasOperations[item];
            Console.WriteLine($"Chose {operation.Name}");

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
        private HostingEnvironment _env;
    }
}