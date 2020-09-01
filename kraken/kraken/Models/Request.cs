using Newtonsoft.Json;
using PropertyChanged;
using Realms;
using System.Collections.Generic;

namespace kraken.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Request
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string uuid { get; set; }

        [JsonProperty("master_id")]
        public string MasterId { get; set; }

        [JsonProperty("work")]
        public string Work { get; set; }

        [JsonProperty("urgency")]
        public string Urgency { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("started_at")]
        public string StartedAt { get; set; }

        [JsonProperty("is_finished")]
        public string IsFinished { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }

        [JsonProperty("isMasterRequestExists")]
        public string IsMasterRequestExists { get; set; }

        [JsonProperty("masterDistance")]
        public string MasterDistance { get; set; }

        [JsonIgnore]
        public string StatusText { get { return StatusDictionary[Status]; } }

        [JsonIgnore]
        public string UrgencyText { get { return UrgencyDictionary[Urgency]; } }

        [JsonIgnore]
        public bool IsRedDotVisible 
        { 
            get 
            {
                if (Status != null & Status != "closed")
                {
                    return true;
                }

                return false; 
            } 
        }


        private Dictionary<string, string> StatusDictionary { get; set; }
        private Dictionary<string, string> UrgencyDictionary { get; set; }

        public Request()
        {
            StatusDictionary = new Dictionary<string, string> {
                { "created", "Заявка" },
                { "appointed", "Назначен исполнитель" },
                { "performer appointed", "Назначен исполнитель" },
                { "answered", "Ответ от мастера" },
                { "active", "На исполнении" },
                { "closed", "Закрыт" },
                { "1X customer chose master", "Вам заявка" }
            };

            UrgencyDictionary = new Dictionary<string, string> {
                { "urgent", "Срочно" },
                { "now", "Сейчас" },
                { "scheduled", "Заданное время" }
            };
        }

        public override bool Equals(object obj)
        {
            if (this is null || obj is null) return false;

            if (!(obj is Request)) return false;

            var otherDialog = (Request)obj;
            return uuid == otherDialog.uuid &&
                IsFinished == otherDialog.IsFinished &&
                Status == otherDialog.Status &&
                MasterId == otherDialog.MasterId;
        }
    }
}
