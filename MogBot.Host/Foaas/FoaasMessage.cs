using System;
using Newtonsoft.Json;

namespace MogBot.Host.Foaas
{
    public class FoaasMessage
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("subtitle")]
        public string Subtitle { get; set; }
    }
}