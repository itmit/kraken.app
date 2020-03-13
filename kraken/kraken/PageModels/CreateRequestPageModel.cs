using FreshMvvm;
using kraken.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace kraken.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class CreateRequestPageModel : FreshBasePageModel
    {
        HttpClient client;
        string _selectedUrgency;
        public List<string> monkeyList;

        public List<WorkType> WorkTypes { get; set; } = new List<WorkType>();
        public List<string> Urgency { get; set; } = new List<string>();

        static Dictionary<string, string> UrgencyTypes { get; } = new Dictionary<string, string>
        {
            { "Срочно", "urgent" },
            { "Сейчас", "now" },
            { "Заданное время", "scheduled" },
        };

        public string SelectedType { get; set; }
        public string SelectedUrgency {
            get { return _selectedUrgency; }
            set
            {
                _selectedUrgency = value;
                if (string.IsNullOrEmpty(_selectedUrgency)) return;
                var selectedValue = UrgencyTypes[_selectedUrgency];
            }
        }
        public string Description { get; set; }

        public ICommand CreateRequestCommand
        {
            get
            {
                return new FreshAwaitCommand((param, tcs) =>
                {
                    CreateRequest();
                });
            }
        }

        public CreateRequestPageModel()
        {
            client = new HttpClient();
            client.MaxResponseContentBufferSize = 209715200; // 200 MB
        }

        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);

            if (IsThereInternet() == false)
            {
                return;
            }

            WorkTypes = (await GetWorkTypes());
            GetUrgencyTypes();
        }

        private async Task<List<WorkType>> GetWorkTypes()
        {
            string restMethod = "getTypeOfWork";
            List<WorkType> Types = new List<WorkType>();

            Realms.Realm realm = Realms.Realm.GetInstance();
            User user = realm.All<User>().Last();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);

            Uri uri = new Uri(string.Format(Constants.RestUrl, restMethod));

            try
            {
                var responseAwaiter = client.GetAsync(uri).ConfigureAwait(false);
                var response = responseAwaiter.GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    JObject catalogArr = JObject.Parse(content);
                    Types = JsonConvert.DeserializeObject<List<WorkType>>(catalogArr["data"].ToString());
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(@"				ERROR {0}", ex.Message);
            }

            return Types;
        }

        private void GetUrgencyTypes()
        {
            return;
        }

        private void CreateRequest()
        {
            throw new NotImplementedException();
        }

        private bool IsThereInternet()
        {
            return Plugin.Connectivity.CrossConnectivity.Current.IsConnected;
        }
    }
}
