using FreshMvvm;
using PropertyChanged;
using System.Windows.Input;
using Realms;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using kraken.Models;
using System.Threading.Tasks;

namespace kraken.PageModels
{
    [AddINotifyPropertyChangedInterface]

    public class AuthorizationPageModel : FreshBasePageModel
    {
        HttpClient client;

        public Realm Realm { get { return Realm.GetInstance(); } }

        public string UsernameEntry { get; set; } = string.Empty;
        public string EmailEntry { get; set; } = string.Empty;
        public string PasswordEntry { get; set; } = string.Empty;

        public AuthorizationPageModel()
        {

        }

        public ICommand OnLoginButtonClicked
        {
            get
            {
                return new FreshAwaitCommand((param, tcs) =>
                {
                    OnLoginClickedAsync();
                    tcs.SetResult(true);
                });
            }
        }

        public ICommand OpenRegisterationCommand
        {
            get
            {
                return new Xamarin.Forms.Command((param) =>
                {
                    OpenRegisterPage();
                });
            }
        }

        async void OpenRegisterPage()
        {
            await CoreMethods.PushPageModel<RegistrationPageModel>();
        }

        async Task OnLoginClickedAsync()
        {
            if (IsThereInternet() == false)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Ошибка", "Интернет-соединение отсутствует. Подключитесь к работающей сети.", "OK");
                return;
            }

            //if (App.DeviceToken == null)
            //{
            //    App.DeviceToken = Plugin.FirebasePushNotification.CrossFirebasePushNotification.Current.Token;
            //}

            bool isValid = await AreCredentialsCorrectAsync();
            if (isValid)
            {
                App.IsUserLoggedIn = true;
                CoreMethods.SwitchOutRootNavigation(NavigationContainerNames.MainContainer);
            }

        }

        async Task<bool> AreCredentialsCorrectAsync()
        {
            //return user.Name == Constants.Username && user.PhoneNumber == Constants.Password;
            if (string.IsNullOrWhiteSpace(EmailEntry) | EmailEntry.Length <= 7 |
                string.IsNullOrWhiteSpace(PasswordEntry) | PasswordEntry.Length < 6)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Ошибка", "Неверно указан email или пароль", "OK");
                return false;
            }

            bool result = await SendUserInfoAsync();

            return result;
        }

        private bool IsThereInternet()
        {
            return Plugin.Connectivity.CrossConnectivity.Current.IsConnected;
        }

        private async Task<bool> SendUserInfoAsync()
        {
            client = new HttpClient();
            client.MaxResponseContentBufferSize = 209715200; // 200 MB

            var restMethod = "login";
            var uri = new System.Uri(string.Format(Constants.RestUrl, restMethod));

            try
            {
                JObject jmessage = new JObject();
                jmessage.Add("email", EmailEntry);
                jmessage.Add("password", PasswordEntry);
                //jmessage.Add("device_token", user.Phone);

                string json = jmessage.ToString();
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = null;
                response = client.PostAsync(uri, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var userInfo = await response.Content.ReadAsStringAsync();
                    JObject userObj = JObject.Parse(userInfo);

                    User user = new User
                    {
                        Name = userObj["data"]["client_info"]["name"].ToString(),
                        Phone = userObj["data"]["client_info"]["phone"].ToString(),
                        Organization = userObj["data"]["client_info"]["organization"].ToString(),
                        Address = userObj["data"]["client_info"]["address"].ToString(),
                    };

                    Realm.Write(() =>
                    {
                        Realm.Add(user, true);
                    });

                    return true;
                }
                else
                {
                    string errorMessage = "";
                    var errorInfo = await response.Content.ReadAsStringAsync();
                    JObject errorObj = JObject.Parse(errorInfo);

                    if (errorObj.ContainsKey("error"))
                    {
                        errorMessage = (string)errorObj["error"];
                    }
                    else if(errorObj.ContainsKey("errors"))
                    {
                        JToken errors = errorObj["errors"];

                        if (errors["email"] != null)
                        {
                            errorMessage = (string)(errors["email"].First);
                        }
                        else if(errors["password"] != null)
                        {
                            errorMessage = (string)(errors["password"].First);
                        }

                    }

                    await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Ошибка", errorMessage, "OK");
                    return false;
                }

            }
            catch (System.Exception ex)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("ERROR", ex.Message, "OK");
                System.Diagnostics.Debug.WriteLine(@"				ERROR {0}", ex.Message);
            }

            return true;
        }
    }
}
