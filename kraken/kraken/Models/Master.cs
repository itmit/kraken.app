using Newtonsoft.Json;

namespace kraken.Models
{
    public class Master
    {
        public string uuid { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("qualification")]
        public string Qualification { get; set; }

        [JsonProperty("work")]
        public string Work { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("rating")]
        public string Rating { get; set; }
    }
}
