using PropertyChanged;
using Realms;

namespace kraken.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Request : RealmObject
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string Work { get; set; }

        public string Urgency { get; set; }

        public string Description { get; set; }

        public string Address { get; set; }
    }
}
