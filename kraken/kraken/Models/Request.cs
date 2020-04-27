﻿using Newtonsoft.Json;
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

        [JsonProperty("started_at")]
        public string StartedAt { get; set; }

        [JsonProperty("is_finished")]
        public string IsFinished { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonIgnore]
        public string StatusText { get { return StatusDictionary[Status]; } }

        [JsonIgnore]
        public string UrgencyText { get { return UrgencyDictionary[Urgency]; } }


        private Dictionary<string, string> StatusDictionary { get; set; }
        private Dictionary<string, string> UrgencyDictionary { get; set; }

        public Request()
        {
            StatusDictionary = new Dictionary<string, string> {
                { "created", "Создан" },
                { "appointed", "Назначен исполнитель" },
                { "active", "На исполнении" }
            };

            UrgencyDictionary = new Dictionary<string, string> {
                { "urgent", "Срочно" },
                { "now", "Сейчас" },
                { "scheduled", "Заданное время" }
            };
        }
    }
}
