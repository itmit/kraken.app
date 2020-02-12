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
    public class RegistrationPageModel : FreshBasePageModel
    {
        HttpClient client;

        public string MessageLabel { get; set; } = string.Empty;
        public Realm Realm { get { return Realm.GetInstance(); } }

        public string FullName { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Adress { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string RepeatPassword { get; set; } = string.Empty;

        public RegistrationPageModel()
        {

        }

        public ICommand OnRegisterButtonClicked
        {
            get
            {
                return new FreshAwaitCommand((param, tcs) =>
                {
                    OnRegisterClicked();
                    tcs.SetResult(true);
                });
            }
        }

        async Task OnRegisterClicked()
        {
            if (IsThereInternet() == false)
            {
                MessageLabel = "Интернет-соединение отсутствует. Подключитесь к работающей сети.";
                return;
            }

            //if (App.DeviceToken == null)
            //{
            //    App.DeviceToken = Plugin.FirebasePushNotification.CrossFirebasePushNotification.Current.Token;
            //}

            User user = new User
            {
                Name = FullName,
                Organization = Organization,
                Email = Email,
                Phone = Phone,
                Adress = Adress,
                Password = Password,
                //DeviceToken = App.DeviceToken
            };

            bool isValid = await AreCredentialsCorrectAsync(user);
            if (isValid)
            {
                App.IsUserLoggedIn = true;

                Realm.Write(() =>
                {
                    Realm.Add(user, true);
                });

                CoreMethods.SwitchOutRootNavigation(NavigationContainerNames.MainContainer);
            }

        }

        async Task<bool> AreCredentialsCorrectAsync(User user)
        {
            //return user.Name == Constants.Username && user.PhoneNumber == Constants.Password;
            if (string.IsNullOrWhiteSpace(user.Name) |
                string.IsNullOrWhiteSpace(user.Phone) | user.Phone.Length <= 7 |
                string.IsNullOrWhiteSpace(user.Password) | user.Password.Length < 5)
            {
                MessageLabel = "НЕВЕРНО УКАЗАН НОМЕР ТЕЛЕФОНА ИЛИ ПАРОЛЬ";
                return false;
            }

            if (Password != RepeatPassword)
            {
                MessageLabel = "Парои не совпадают";
                return false;
            }

            // TODO:
            // add Email, Organization, Adress validation

            bool result = await SendUserInfoAsync(user);

            return result;
        }

        private bool IsThereInternet()
        {
            return Plugin.Connectivity.CrossConnectivity.Current.IsConnected;
        }

        private async Task<bool> SendUserInfoAsync(User user)
        {
            client = new HttpClient();
            client.MaxResponseContentBufferSize = 209715200; // 200 MB

            var restMethod = "register";
            var uri = new System.Uri(string.Format(Constants.RestUrl, restMethod));

            try
            {
                JObject jmessage = new JObject();
                jmessage.Add("email", user.Email);
                jmessage.Add("name", user.Name);
                jmessage.Add("organization", user.Organization);
                jmessage.Add("address", user.Adress);
                jmessage.Add("phone", ClearNumber(user.Phone));
                jmessage.Add("password", user.Password);
                jmessage.Add("password_confirmation", RepeatPassword);
                //jmessage.Add("device_token", user.Phone);

                string json = jmessage.ToString();
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = null;
                response = client.PostAsync(uri, content).Result;

                if (response.IsSuccessStatusCode)
                {
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
                    else if (errorObj.ContainsKey("errors"))
                    {
                        JToken errors = errorObj["errors"];

                        if (errors["email"] != null)
                        {
                            errorMessage = (string)(errors["email"].First);
                        }
                        else if (errors["password"] != null)
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

        private string ClearNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return "";                    
            }

            phoneNumber = System.Text.RegularExpressions.Regex.Replace(phoneNumber, "[\\D]", "");

            if (phoneNumber[0] == '7')
            {
                phoneNumber = '8' + phoneNumber.Substring(1);
            }

            return phoneNumber;
        }
    }
}
