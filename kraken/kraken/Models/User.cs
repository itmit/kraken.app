using PropertyChanged;
using Realms;

namespace kraken.Models
{
    [AddINotifyPropertyChangedInterface]
    public class User : RealmObject
    {
        public string Name { get; set; }

        public string Organization { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Adress { get; set; }

        public string Password { get; set; }

        public string DeviceToken { get; set; }
    }
}
