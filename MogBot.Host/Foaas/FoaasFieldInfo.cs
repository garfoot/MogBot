using System;
using Newtonsoft.Json;

namespace MogBot.Host.Foaas
{
    public class FoaasFieldInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("field")]
        public string Field { get; set; }
    }
}