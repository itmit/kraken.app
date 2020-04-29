using kraken.Models;
using Realms;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using kraken.Messages;
using Xamarin.Forms;

namespace kraken.Services
{
    public class UserService : IUserService
    {
        readonly HttpClient client;

        public static bool AuthenticationHeaderIsSet { get; set; }

        public Realm Realm { get { return Realm.GetInstance(); } }

        public UserService()
        {
            client = new HttpClient
            {
                MaxResponseContentBufferSize = 209715200 // 200 MB
            };
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            SetAuthenticationHeader();
        }

        public async Task<bool> LoginUserAsync(string EmailEntry, string PasswordEntry)
        {
            if (IsThereInternet() == false)
            {
                return false;
            }

            if (!AuthenticationHeaderIsSet)
            {
                SetAuthenticationHeader();
            }

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

                    User currentUser;

                    var isMaster = (userObj["data"]["client_type"].ToString() == "master");

                    currentUser = new User
                    {
                        Name = userObj["data"]["client_info"]["name"].ToString(),
                        Phone = userObj["data"]["client_info"]["phone"].ToString(),
                        ClientType = userObj["data"]["client_type"].ToString(),
                        Token = userObj["data"]["access_token"].ToString(),
                        Email = EmailEntry,
                        DeviceToken = App.DeviceToken,
                    };

                    if (isMaster)
                    {
                        currentUser.MasterId = userObj["data"]["client_info"]["master_id"].ToString();
                        App.IsUserMaster = true;
                    }
                    else
                    {
                        currentUser.Organization = userObj["data"]["client_info"]["organization"].ToString();
                        currentUser.Address = userObj["data"]["client_info"]["address"].ToString();
                    }

                    Realm.Write(() =>
                    {
                        Realm.Add(currentUser, true);
                    });

                    if(isMaster)
                    {
                        StartSendingCoordinates();
                    }

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

                    }

                    await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Ошибка", errorMessage, "OK");
                    return false;
                }

            }
            catch (Exception ex)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Не выполнено", ex.GetType().Name + "\n" + ex.Message + "\n" + ex.StackTrace, "OK");
            }

            return false;
        }

        public async Task<bool> SetDrivingModeAsync(string modeCode)
        {
            if (IsThereInternet() == false)
            {
                return false;
            }

            if (!AuthenticationHeaderIsSet)
            {
                SetAuthenticationHeader();
            }

            string restMethod = "masters/changeWayToTravel";
            Uri uri = new Uri(string.Format(Constants.RestUrl, restMethod));

            try
            {
                JObject jmessage = new JObject
                {
                    { "way", modeCode }
                };
                string json = jmessage.ToString();

                StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = null;
                response = client.PostAsync(uri, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseMessage = await response.Content.ReadAsStringAsync();

                    return true;
                }
                else
                {
                    string errorInfo = await response.Content.ReadAsStringAsync();
                    string errorMessage = ParseErrorMessage(errorInfo);

                    Xamarin.Forms.Device.BeginInvokeOnMainThread(async () => { await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Не выполнено", errorMessage, "OK"); });

                    return false;
                }
            }
            catch (System.Exception ex)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Не выполнено", ex.GetType().Name + "\n" + ex.Message + "\n" + ex.StackTrace, "OK");
                return false;
            }
        }

        public async Task<bool> SetSearchFilterAsync(string radiusValue)
        {
            if (IsThereInternet() == false)
            {
                return false;
            }

            if (!AuthenticationHeaderIsSet)
            {
                SetAuthenticationHeader();
            }

            string restMethod = "client/changeRadius";
            Uri uri = new Uri(string.Format(Constants.RestUrl, restMethod));

            try
            {
                JObject jmessage = new JObject
                {
                    { "radius", radiusValue }
                };
                string json = jmessage.ToString();

                StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = null;
                response = client.PostAsync(uri, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseMessage = await response.Content.ReadAsStringAsync();

                    return true;
                }
                else
                {
                    string errorInfo = await response.Content.ReadAsStringAsync();
                    string errorMessage = ParseErrorMessage(errorInfo);

                    Xamarin.Forms.Device.BeginInvokeOnMainThread(async () => { await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Не выполнено", errorMessage, "OK"); });

                    return false;
                }
            }
            catch (Exception ex)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Не выполнено", ex.GetType().Name + "\n" + ex.Message + "\n" + ex.StackTrace, "OK");
                return false;
            }
        }

        public async Task<bool> SetStatusAsync(string statusValue)
        {
            if (IsThereInternet() == false)
            {
                return false;
            }

            if (!AuthenticationHeaderIsSet)
            {
                SetAuthenticationHeader();
            }

            string restMethod = "masters/changeStatus";
            Uri uri = new Uri(string.Format(Constants.RestUrl, restMethod));

            try
            {
                JObject jmessage = new JObject
                {
                    { "status", statusValue }
                };
                string json = jmessage.ToString();

                StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = null;
                response = client.PostAsync(uri, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseMessage = await response.Content.ReadAsStringAsync();

                    return true;
                }
                else
                {
                    string errorInfo = await response.Content.ReadAsStringAsync();
                    string errorMessage = ParseErrorMessage(errorInfo);

                    Xamarin.Forms.Device.BeginInvokeOnMainThread(async () => { await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Не выполнено", errorMessage, "OK"); });

                    return false;
                }
            }
            catch (Exception ex)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Не выполнено", ex.GetType().Name + "\n" + ex.Message + "\n" + ex.StackTrace, "OK");
                return false;
            }
        }

        #region Utility private methods
        private void StartSendingCoordinates()
        {
            var message = new StartLongRunningTaskMessage();
            MessagingCenter.Send(message, "StartLongRunningTaskMessage");
        }
        private bool IsThereInternet()
        {
            // TODO: if no internet, show alert
            return Plugin.Connectivity.CrossConnectivity.Current.IsConnected;
        }

        private string ParseErrorMessage(string errorInfo)
        {
            string errorMessage = "";
            JObject errorObj = JObject.Parse(errorInfo);

            if (errorObj.ContainsKey("error"))
            {
                errorMessage = (string)errorObj["error"];
            }
            else if (errorObj.ContainsKey("message"))
            {
                errorMessage = (string)errorObj["message"];
            }
            else if (errorObj.ContainsKey("errors"))
            {
                JToken errors = errorObj["errors"];

                if (errors["data"] != null)
                {
                    errorMessage = (string)(errors["data"].First);
                }
                else if (errors["name"] != null)
                {
                    errorMessage = (string)(errors["name"].First);
                }

            }

            return errorMessage;
        }

        private void SetAuthenticationHeader()
        {
            Realm realm = Realm.GetInstance();
            var users = realm.All<User>();
            User user;

            if (users.Count() > 0)
            {
                user = users.Last();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);
                AuthenticationHeaderIsSet = true;
            }
            else
            {
                AuthenticationHeaderIsSet = false;
            }
        }
        #endregion
    }
}
