using System;
using Discord;

namespace MogBot.Host
{
    public class HostingEnvironment
    {
        public DiscordClient Discord { get; set; }
        public ITrace Trace { get; set; }

        public HostingEnvironment(DiscordClient discord, ITrace trace)
        {
            Discord = discord;
            Trace = trace;
        }
    }
}