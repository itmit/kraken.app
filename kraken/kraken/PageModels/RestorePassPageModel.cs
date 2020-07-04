using FreshMvvm;
using PropertyChanged;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace kraken.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class RestorePassPageModel : FreshBasePageModel
    {
        readonly HttpClient client;

        public string EmailEntry { get; set; } = string.Empty;
        public string CodeEntry { get; set; } = string.Empty;
        public string PasswordEntry { get; set; } = string.Empty;
        public string RepeatPasswordEntry { get; set; } = string.Empty;

        public bool IsFirstStep { get; set; } = true;
        public bool IsSecondStep { get; set; } = false;

        public ICommand SendEmailCommand
        {
            get
            {
                return new Xamarin.Forms.Command(async (param) =>
                {
                    await SendEmail();
                });
            }
        }

        public ICommand SendPasswordCommand
        {
            get
            {
                return new Xamarin.Forms.Command(async () =>
                {
                    await SendPassword();
                    await CoreMethods.PopPageModel();
                });
            }
        }

        public RestorePassPageModel()
        {
            client = new HttpClient();
        }

        private async Task<bool> SendEmail()
        {
            string restMethod = "sendCode";
            System.Uri uri = new System.Uri(string.Format(Constants.RestUrl, restMethod));

            try
            {
                JObject jmessage = new JObject
                {
                    { "email", EmailEntry }
                };

                string json = jmessage.ToString();
                StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = null;
                response = client.PostAsync(uri, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string userInfo = await response.Content.ReadAsStringAsync();
                    IsFirstStep = false;
                    IsSecondStep = true;
                    return true;
                }
                else
                {
                    string errorInfo = await response.Content.ReadAsStringAsync();
                    return false;
                }

            }
            catch (System.Exception ex)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Не выполнено", ex.Message, "OK");
            }

            return true;
        }

        private async Task<bool> SendPassword()
        {
            string restMethod = "resetPassword";
            System.Uri uri = new System.Uri(string.Format(Constants.RestUrl, restMethod));

            try
            {
                JObject jmessage = new JObject
                {
                    { "email", EmailEntry },
                    { "code", CodeEntry },
                    { "password", PasswordEntry },
                    { "password_confirmation", RepeatPasswordEntry }
                };

                string json = jmessage.ToString();
                StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = null;
                response = client.PostAsync(uri, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string userInfo = await response.Content.ReadAsStringAsync();
                    await CoreMethods.DisplayAlert("Выполнено", "Пароль успешно восстановлен", "Ok");
                    return true;
                }
                else
                {
                    string errorInfo = await response.Content.ReadAsStringAsync();
                    return false;
                }

            }
            catch (System.Exception ex)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Не выполнено", ex.Message, "OK");
            }

            return true;
        }
    }
}
