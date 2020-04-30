using kraken.Models;
using Realms;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace kraken.Services
{
    public class RequestStorageService : IRequestStorageService
    {
        readonly HttpClient client;

        public static bool AuthenticationHeaderIsSet { get; set; }

        public Realm Realm { get { return Realm.GetInstance(); } }

        public RequestStorageService()
        {
            client = new HttpClient
            {
                MaxResponseContentBufferSize = 209715200 // 200 MB
            };
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            SetAuthenticationHeader();
        }

        #region Get data methods
        public async Task<Request> GetRequestFullInfo(string RequestUuid)
        {
            if (IsThereInternet() == false)
            {
                return null;
            }

            if (!AuthenticationHeaderIsSet)
            {
                SetAuthenticationHeader();
            }

            string restMethod = "entity/" + RequestUuid + "/edit";
            Request Object = new Request();

            System.Uri uri = new System.Uri(string.Format(Constants.RestUrl, restMethod));

            try
            {
                var responseAwaiter = client.GetAsync(uri).ConfigureAwait(false);
                var response = responseAwaiter.GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    JObject objectArr = JObject.Parse(content);
                    var test = objectArr["data"]["entity"].ToString();
                    Object = JsonConvert.DeserializeObject<Request>(objectArr["data"]["entity"].ToString());
                    //Object.Nodes = JsonConvert.DeserializeObject<List<Node>>(objectArr["data"]["nodes"].ToString());
                }
                else
                {
                    string errorInfo = await response.Content.ReadAsStringAsync();
                    string errorMessage = ParseErrorMessage(errorInfo);

                    Xamarin.Forms.Device.BeginInvokeOnMainThread(async () => { await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Не выполнено", errorMessage, "OK"); });
                }
            }
            catch (System.Exception)
            {
            }

            return Object;
        }

        public async Task<List<Request>> GetUserRequestsAsync()
        {
            if (IsThereInternet() == false)
            {
                return null;
            }

            if (!AuthenticationHeaderIsSet)
            {
                SetAuthenticationHeader();
            }

            string restMethod = "inquiry";

            if (App.IsUserMaster)
            {
                restMethod = "masters/getInquiryList";
            }
            
            List<Request> Requests = new List<Request>();

            Uri uri = new Uri(string.Format(Constants.RestUrl, restMethod));

            try
            {
                var responseAwaiter = client.GetAsync(uri).ConfigureAwait(false);
                var response = responseAwaiter.GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    JObject catalogArr = JObject.Parse(content);
                    Requests = JsonConvert.DeserializeObject<List<Request>>(catalogArr["data"].ToString());
                }
                else
                {
                    string errorInfo = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception)
            {
            }

            return Requests;
        }

        public async Task<List<WorkType>> GetWorkTypesAsync()
        {
            if (IsThereInternet() == false)
            {
                return null;
            }

            if (!AuthenticationHeaderIsSet)
            {
                SetAuthenticationHeader();
            }

            string restMethod = "getTypeOfWork";
            List<WorkType> Types = new List<WorkType>();

            Uri uri = new Uri(string.Format(Constants.RestUrl, restMethod));

            try
            {
                var responseAwaiter = client.GetAsync(uri).ConfigureAwait(false);
                var response = responseAwaiter.GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    JObject resultArr = JObject.Parse(content);
                    Types = JsonConvert.DeserializeObject<List<WorkType>>(resultArr["data"].ToString());
                }
                else
                {
                    string errorInfo = await response.Content.ReadAsStringAsync();
                    string errorMessage = ParseErrorMessage(errorInfo);

                    Xamarin.Forms.Device.BeginInvokeOnMainThread(async () => { await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Не выполнено", errorMessage, "OK"); });
                }
            }
            catch (Exception)
            {
            }

            return Types;
        }

        public async Task<List<Master>> GetMastersAsync(string RequestUuid)
        {
            if (IsThereInternet() == false)
            {
                return null;
            }

            if (!AuthenticationHeaderIsSet)
            {
                SetAuthenticationHeader();
            }

            string restMethod = "inquiry/masters";
            List<Master> Masters = new List<Master>();

            Uri uri = new Uri(string.Format(Constants.RestUrl, restMethod));

            try
            {
                JObject jmessage = new JObject
                {
                    { "uuid", RequestUuid }
                };
                string json = jmessage.ToString();
                StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = null;
                response = client.PostAsync(uri, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    JObject resultArr = JObject.Parse(responseContent);
                    Masters = JsonConvert.DeserializeObject<List<Master>>(resultArr["data"].ToString());
                }
                else
                {
                    string errorInfo = await response.Content.ReadAsStringAsync();
                    string errorMessage = ParseErrorMessage(errorInfo);

                    Xamarin.Forms.Device.BeginInvokeOnMainThread(async () => { await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Не выполнено", errorMessage, "OK"); });
                }
            }
            catch (Exception)
            {
            }

            return Masters;
        }
        #endregion

        #region Add/Update methods
        public async Task<bool> SendNewRequestAsync(Request CreatedRequest, string[] FilesArray)
        {
            if (IsThereInternet() == false)
            {
                return false;
            }

            if (!AuthenticationHeaderIsSet)
            {
                SetAuthenticationHeader();
            }

            string restMethod = "inquiry";
            System.Uri uri = new System.Uri(string.Format(Constants.RestUrl, restMethod));

            try
            {
                string userAddress = GetUserAddress();

                JArray FilesJArray = new JArray();
                foreach (string parameterName in FilesArray)
                {
                    FilesJArray.Add(parameterName);
                }

                JObject jmessage = new JObject
                {
                    { "work", CreatedRequest.Work },
                    { "urgency", CreatedRequest.Urgency },
                    { "description", CreatedRequest.Description },
                    { "address", userAddress },
                    { "docs", FilesJArray }
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

        public async Task<bool> SendAcceptRequest(string RequestUuid)
        {
            if (IsThereInternet() == false)
            {
                return false;
            }

            if (!AuthenticationHeaderIsSet)
            {
                SetAuthenticationHeader();
            }

            string restMethod = "masters/applyInquiry";
            System.Uri uri = new System.Uri(string.Format(Constants.RestUrl, restMethod));

            try
            {
                JObject jmessage = new JObject
                {
                    { "uuid", RequestUuid }
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

        public async Task<bool> SendDeclineRequest(string RequestUuid)
        {
            if (IsThereInternet() == false)
            {
                return false;
            }

            if (!AuthenticationHeaderIsSet)
            {
                SetAuthenticationHeader();
            }

            string restMethod = "masters/cancelInquiry";
            System.Uri uri = new System.Uri(string.Format(Constants.RestUrl, restMethod));

            try
            {
                JObject jmessage = new JObject
                {
                    { "uuid", RequestUuid }
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
        #endregion

        #region Utility private methods
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

        private string GetUserAddress()
        {
            Realm realm = Realm.GetInstance();
            var users = realm.All<User>();
            User user;

            if (users.Count() > 0)
            {
                user = users.Last();
                return user.Address;
            }

            return null;
        }
        #endregion
    }
}
