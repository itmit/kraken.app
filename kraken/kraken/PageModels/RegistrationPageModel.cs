using FreshMvvm;
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

        public string UserToken { get; set; } = string.Empty;

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
                return new FreshAwaitCommand(async (param, tcs) =>
                {
                    await OnRegisterClicked();
                    tcs.SetResult(true);
                });
            }
        }

        async Task OnRegisterClicked()
        {
            if (IsThereInternet() == false)
            {
                MessageLabel = "Интернет-соединение отсутствует. Подключитесь к работающей сети.";
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Ошибка", MessageLabel, "OK");
                return;
            }

            if (App.DeviceToken == null)
            {
                App.DeviceToken = Plugin.FirebasePushNotification.CrossFirebasePushNotification.Current.Token;
            }

            User user = new User
            {
                Name = FullName,
                Organization = Organization,
                Email = Email,
                Phone = Phone,
                Address = Adress,
                Password = Password,
                DeviceToken = App.DeviceToken
            };

            bool isValid = await AreCredentialsCorrectAsync(user);
            if (isValid)
            {
                App.IsUserLoggedIn = true;

                user.Token = UserToken;

                Realm.Write(() =>
                {
                    Realm.Add(user, true);
                });

                CoreMethods.SwitchOutRootNavigation(NavigationContainerNames.ClientTabsContainer);
            }
        }

        async Task<bool> AreCredentialsCorrectAsync(User user)
        {
            //return user.Name == Constants.Username && user.PhoneNumber == Constants.Password;
            if (string.IsNullOrWhiteSpace(user.Name) |
                string.IsNullOrWhiteSpace(user.Phone) | user.Phone.Length < 6 |
                string.IsNullOrWhiteSpace(user.Password) | user.Password.Length < 5)
            {
                MessageLabel = "Неверно указано имя, номер телефона или пароль";
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Ошибка", MessageLabel, "OK");
                return false;
            }

            if (Password != RepeatPassword)
            {
                MessageLabel = "Пароли не совпадают";
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Ошибка", MessageLabel, "OK");
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
            client = new HttpClient
            {
                MaxResponseContentBufferSize = 209715200 // 200 MB
            };

            string restMethod = "register";
            System.Uri uri = new System.Uri(string.Format(Constants.RestUrl, restMethod));

            try
            {
                JObject jmessage = new JObject
                {
                    { "email", user.Email },
                    { "name", user.Name },
                    { "organization", user.Organization },
                    { "address", user.Address },
                    { "phone", ClearNumber(user.Phone) },
                    { "password", user.Password },
                    { "password_confirmation", RepeatPassword },
                    { "deviceToken", App.DeviceToken }
                };

                string json = jmessage.ToString();
                StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = null;
                response = client.PostAsync(uri, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    JObject resultArray = JObject.Parse(result);

                    UserToken = resultArray["data"]["access_token"].ToString();
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
                        else if (errors["organization"] != null)
                        {
                            errorMessage = (string)(errors["organization"].First);
                        }
                        else if (errors["address"] != null)
                        {
                            errorMessage = (string)(errors["address"].First);
                        }
                        else if (errors["phone"] != null)
                        {
                            errorMessage = (string)(errors["phone"].First);
                        }

                    }

                    await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Ошибка", errorMessage, "OK");
                    return false;
                }

            }
            catch (System.Exception ex)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Не выполнено", ex.GetType().Name + "\n" + ex.Message, "OK");
            }

            return false;
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
