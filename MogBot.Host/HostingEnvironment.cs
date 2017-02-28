using System;
using System.Linq;
using Discord;
using FeatureSwitcher;
using MogBot.Host.Features;

namespace MogBot.Host
{
    public class HostingEnvironment
    {
        public delegate HostingEnvironment Create(DiscordClient discord);

        public DiscordClient Discord { get; set; }
        public ITrace Trace { get; set; }

        public HostingEnvironment(DiscordClient discord, ITrace trace)
        {
            Discord = discord;
            Trace = trace;

            trace.Write("General is adult is ", ConsoleColor.White);
            if (Feature<GeneralIsAdult>.Is().Enabled)
            {
                trace.WriteLine("enabled", ConsoleColor.Green);
            }
            else
            {
                trace.WriteLine("disabled", ConsoleColor.Yellow);
            }
        }

        public bool IsAdultOnly(Channel channel)
        {
            // The channel.Server property is null so have to look up the server explicitly for now
            Server server = Discord.FindServers("Obsidian Guard").FirstOrDefault();
            if (server == null)
            {
                return false;
            }

            Role kids = server.FindRoles("kids").FirstOrDefault();

            ChannelPermissionOverrides permissions = channel.GetPermissionsRule(kids);

            // General is a special snowflake as roles can't be denied or granted explicit permissions to read
            // so check for it explicitly and block
            var isGeneralAndChild = string.Compare("general", channel.Name, StringComparison.OrdinalIgnoreCase) == 0
                        && Feature<GeneralIsAdult>.Is().Disabled;

            if (permissions.Connect == PermValue.Allow
                || permissions.ReadMessages == PermValue.Allow
                || isGeneralAndChild)
            {
                return false;
            }

            return true;
        }
    }
}