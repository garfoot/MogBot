using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MogBot.Host.Foaas
{
    public class FoaasOperation
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("fields")]
        public IList<FoaasFieldInfo> Fields { get; set; } = new List<FoaasFieldInfo>();
    }
}