using FreshMvvm;
using PropertyChanged;
using kraken.Models;
using Realms;
using System.Linq;
using System.Windows.Input;
using kraken.Services;
using Xamarin.Forms;
using System;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Xamarin.Essentials;

namespace kraken.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class MyProfilePageModel : FreshBasePageModel
    {
        private HttpClient client;

        public Realm Realm { get { return Realm.GetInstance(); } }
        public User CurrentUser { get; set; }

        public string UserName { get; set; }
        public string UserOrganization { get; set; }
        public string UserEmail { get; set; }
        public string UserPhone { get; set; }
        public string UserAddress { get; set; }

        public ICommand OnRequestsButtonClicked
        {
            get
            {
                return new FreshAwaitCommand((param, tcs) =>
                {
                    OpenRequestsPage();
                    tcs.SetResult(true);
                });
            }
        }

        public ICommand SendCoordsCommand
        {
            get
            {
                return new Command((param) =>
                {
                    SendUserCoordinates();
                });
            }
        }

        async void OpenRequestsPage()
        {
            await CoreMethods.SwitchSelectedTab<MyRequestPageModel>();
        }

        public MyProfilePageModel()
        {
            UserName = "";
            UserOrganization = "";
            UserEmail = "";
            UserPhone = "";
            UserAddress = "";
        }

        protected override void ViewIsAppearing(object sender, System.EventArgs e)
        {
            LoadUserInfo();
            base.ViewIsAppearing(sender, e);
        }

        public override void Init(object initData)
        {
            base.Init(initData);

            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            IQueryable<User> users = Realm.All<User>();
            if (users.Count() <= 0)
                return;

            CurrentUser = users.Last();

            UserName = CurrentUser.Name;
            UserOrganization = CurrentUser.Organization;
            UserEmail = CurrentUser.Email;
            UserPhone = CurrentUser.Phone;
            UserAddress = CurrentUser.Address;
        }

        private async void SendUserCoordinates()
        {
            Location UserLocation = await GetUserCoordinatesAsync();

            string restMethod = "masters/updateLocation";
            Uri uri = new Uri(string.Format(Constants.RestUrl, restMethod));

            try
            {
                JObject jmessage = new JObject
                {
                    { "latitude", UserLocation.Latitude },
                    { "longitude", UserLocation.Longitude }
                };

                string json = jmessage.ToString();
                StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                client = new HttpClient
                {
                    MaxResponseContentBufferSize = 209715200 // 200 MB
                };

                Realm realm = Realm.GetInstance();
                var users = realm.All<User>();
                User user;

                if (users.Count() > 0)
                {
                    user = users.Last();
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);
                }
                else
                {
                }

                HttpResponseMessage response = null;
                response = client.PostAsync(uri, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Успех", $"Latitude: {UserLocation.Latitude}, Longitude: {UserLocation.Longitude}", "OK");
                }
                else
                {
                    var errorMessage = "Произошла ошибка при отправке координат";
                    await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Ошибка", errorMessage, "OK");
                }

            }
            catch (System.Exception ex)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Не выполнено", ex.GetType().Name + "\n" + ex.Message + "\n" + ex.StackTrace, "OK");
            }
        }

        private async System.Threading.Tasks.Task<Location> GetUserCoordinatesAsync()
        {
            string errorMessage = null;
            Location UserLocation = new Location();
            try
            {
                UserLocation = await Geolocation.GetLastKnownLocationAsync();

                if (UserLocation != null)
                {
                    Console.WriteLine($"Latitude: {UserLocation.Latitude}, Longitude: {UserLocation.Longitude}, Altitude: {UserLocation.Altitude}");
                }
            }
            catch (FeatureNotSupportedException)
            {
                // Handle not supported on device exception
                errorMessage = "Not supported on device";

            }
            catch (FeatureNotEnabledException)
            {
                // Handle not enabled on device exception
                errorMessage = "Not enabled on device";
            }
            catch (PermissionException)
            {
                // Handle permission exception
                errorMessage = "No permission";
            }
            catch (Exception)
            {
                // Unable to get location
                errorMessage = "Unable to get location";
            }

            if (errorMessage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", errorMessage, "OK");
                return null;
            }

            return UserLocation;
        }
    }
}
