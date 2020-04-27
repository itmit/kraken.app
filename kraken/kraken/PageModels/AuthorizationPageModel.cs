using FreshMvvm;
using PropertyChanged;
using System.Windows.Input;
using Realms;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using kraken.Models;
using System.Threading.Tasks;
using System;
using kraken.Services;
using Xamarin.Forms;
using kraken.Messages;

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

        private User currentUser;

        public AuthorizationPageModel()
        {

        }

        public ICommand OnLoginButtonClicked
        {
            get
            {
                return new FreshAwaitCommand(async (param, tcs) =>
                {
                    await OnLoginClickedAsync();
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

            if (App.DeviceToken == null)
            {
                App.DeviceToken = Plugin.FirebasePushNotification.CrossFirebasePushNotification.Current.Token;
            }

            bool isValid = await AreCredentialsCorrectAsync();
            if (isValid)
            {
                if(currentUser.ClientType == "master")
                {
                    App.IsUserMaster = true;
                    StartSendingCoordinates();
                }
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
            client = new HttpClient
            {
                MaxResponseContentBufferSize = 209715200 // 200 MB
            };

            string restMethod = "login";
            Uri uri = new Uri(string.Format(Constants.RestUrl, restMethod));

            try
            {
                JObject jmessage = new JObject
                {
                    { "email", EmailEntry },
                    { "password", PasswordEntry },
                    { "deviceToken", App.DeviceToken }
                };

                string json = jmessage.ToString();
                StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = null;
                response = client.PostAsync(uri, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string userInfo = await response.Content.ReadAsStringAsync();
                    JObject userObj = JObject.Parse(userInfo);

                    var userType = userObj["data"]["client_type"].ToString();

                    if (userType == "master")
                    {
                        currentUser = new User
                        {
                            MasterId = userObj["data"]["client_info"]["master_id"].ToString(),
                            Name = userObj["data"]["client_info"]["name"].ToString(),
                            Phone = userObj["data"]["client_info"]["phone"].ToString(),
                            ClientType = userObj["data"]["client_type"].ToString(),
                            Token = userObj["data"]["access_token"].ToString(),
                            DeviceToken = App.DeviceToken,
                        };
                    }
                    else
                    {
                        currentUser = new User
                        {
                            Name = userObj["data"]["client_info"]["name"].ToString(),
                            Phone = userObj["data"]["client_info"]["phone"].ToString(),
                            Organization = userObj["data"]["client_info"]["organization"].ToString(),
                            Address = userObj["data"]["client_info"]["address"].ToString(),
                            ClientType = userObj["data"]["client_type"].ToString(),
                            Token = userObj["data"]["access_token"].ToString(),
                            DeviceToken = App.DeviceToken,
                        };
                    }

                    Realm.Write(() =>
                    {
                        Realm.Add(currentUser, true);
                    });

                    return true;
                }
                else
                {
                    string errorMessage = "";
                    string errorInfo = await response.Content.ReadAsStringAsync();
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
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Не выполнено", ex.GetType().Name + "\n" + ex.Message + "\n" + ex.StackTrace, "OK");
            }

            return false;
        }

        private void StartSendingCoordinates()
        {
            var message = new StartLongRunningTaskMessage();
            MessagingCenter.Send(message, "StartLongRunningTaskMessage");
        }
    }
}
