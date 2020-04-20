using Newtonsoft.Json;
using PropertyChanged;
using Realms;

namespace kraken.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Request : RealmObject
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string uuid { get; set; }

        [JsonProperty("work")]
        public string Work { get; set; }

        [JsonProperty("urgency")]
        public string Urgency { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("started_at")]
        public string StartedAt { get; set; }

        [JsonProperty("is_finished")]
        public string IsFinished { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
