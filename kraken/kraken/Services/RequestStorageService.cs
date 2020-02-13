using kraken.Models;
using Xamarin.Forms;
using Realms;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json.Linq;

namespace kraken.Services
{
    public class RequestStorageService : IRequestStorageService
    {
        HttpClient client;

        public Realm Realm { get { return Realm.GetInstance(); } }

        public List<Request> Requests { get; private set; }
        public User CurrentUser { get; set; }

        public RequestStorageService()
        {
            string authData = string.Format("{0}:{1}", Constants.Username, Constants.Password);
            string authHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(authData));

            client = new HttpClient();
            client.MaxResponseContentBufferSize = 209715200; // 200 MB
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public Request GetRequest(string id)
        {
            return Realm.Find<Request>(id);
        }

        /// <summary>
        /// Gets all Requests
        /// </summary>
        /// <returns>All Requests</returns>
        public async Task<List<Request>> GetAllRequestsAsync()
        {
            string restMethod = "requests";

            Requests = new List<Request>();

            Uri uri = new Uri(string.Format(Constants.RestUrl, restMethod));

            try
            {
                HttpResponseMessage response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    Requests = JsonConvert.DeserializeObject<List<Request>>(content);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"				ERROR {0}", ex.Message);
            }

            return Requests;
        }

        /// <summary>
        /// Adds the Request
        /// </summary>
        /// <param name="request">Request to add</param>
        public void AddRequest(Request request)
        {
            Realm.Write(() =>
            {
                Realm.Add<Request>(request);
            });
        }

        public async Task<Request> CreateRequest()
        {
            string restMethod = "inquiry/store";
            Uri uri = new Uri(string.Format(Constants.RestUrl, restMethod));
            Request newRequest = new Request();

            try
            {
                JObject jcontact = new JObject();
                jcontact.Add("work", "");
                jcontact.Add("urgency", "");
                jcontact.Add("description", "");
                jcontact.Add("address", "");
                string json = jcontact.ToString();
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = null;
                response = await client.PostAsync(uri, content).ConfigureAwait(continueOnCapturedContext: false);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    //newRequest = JsonConvert.DeserializeObject<Request>(responseContent);
                }

            }
            catch (Exception)
            {
            }

            return newRequest;
        }

        /// <summary>
        /// Updates the Request
        /// </summary>
        /// <param name="request">Request to update</param>
        public void UpdateRequest(Request request)
        {
            Realm.Write(() =>
            {
                Realm.Add<Request>(request, true);
            });
        }

        /// <summary>
        /// Deletes the request
        /// </summary>
        /// <param name="request">Request we want to delete</param>
        public void DeleteRequest(Request request)
        {
            Realm.Write(() =>
            {
                Realm.Remove(request);
            });
        }

        /// <summary>
        /// Checks if the given request exists
        /// </summary>
        /// <returns><c>true</c>, if request was found, <c>false</c> otherwise</returns>
        /// <param name="request">The Request we want to know already exists</param>
        public bool DoesRequestExist(Request request)
        {
            return Realm.All<Request>().Contains(request);
        }

        /// <summary>
        /// Deletes all the requests
        /// </summary>
        public void DeleteAllRequests()
        {
            Realm.Write(() =>
            {
                Realm.RemoveAll<Request>();
            });
        }
    }
}
