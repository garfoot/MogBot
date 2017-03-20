using System;
using System.Linq;
using Discord;
using FeatureSwitcher;
using MogBot.Host.Features;

namespace MogBot.Host
{
    public class HostingEnvironment
    {
        private Server _server;
        private Role _kids;

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

            _server = Discord.FindServers("Obsidian Guard").FirstOrDefault();
            _kids = _server?.FindRoles("kids").FirstOrDefault();
        }

        public bool IsAdultOnly(Channel channel)
        {
            if (_server == null)
            {
                return false;
            }

            ChannelPermissionOverrides permissions = channel.GetPermissionsRule(_kids);

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

        public bool IsKid(User user)
        {
            if (_kids == null)
            {
                return false;
            }

            return user.HasRole(_kids);
        }
    }
}