using Xamarin.Forms;
using System.Threading.Tasks;
using System.Threading;
using Xamarin.Essentials;
using System;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Realms;
using kraken.Models;
using System.Linq;
using static Xamarin.Essentials.Permissions;

namespace kraken
{
    public class TaskCounter
    {
        private const int MinutesInMilliseconds = 60000; // 5 minutes
        private readonly HttpClient client;
        private readonly Uri uri = new Uri(string.Format(Constants.RestUrl, "masters/updateLocation"));

        public TaskCounter()
        {
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
        }

        public async Task RunCounter(CancellationToken token)
        {
            await Task.Run(async () => {

                for (long i = 0; i < long.MaxValue; i++)
                {
                    token.ThrowIfCancellationRequested();

                    SendUserCoordinates();

                    await Task.Delay(MinutesInMilliseconds);
                }
            }, token);
        }

        private async void SendUserCoordinates()
        {
            Location UserLocation = await GetUserCoordinatesAsync();

            if(UserLocation == null)
            {
                return;
            }

            try
            {
                JObject jmessage = new JObject
                {
                    { "latitude", UserLocation.Latitude },
                    { "longitude", UserLocation.Longitude }
                };

                string json = jmessage.ToString();
                StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = null;
                response = client.PostAsync(uri, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var errorMessage = "Произошла ошибка при отправке координат";
                    await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Ошибка", errorMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Не выполнено", ex.GetType().Name + "\n" + ex.Message, "OK");
            }
        }

        private async Task<Location> GetUserCoordinatesAsync()
        {
            string errorMessage = null;
            Location UserLocation = new Location();
            try
            {
                var status = await CheckAndRequestLocationPermission();
                if (status != PermissionStatus.Granted)
                {
                    // Notify user permission was denied
                    await Application.Current.MainPage.DisplayAlert("Ошибка", "невозможно отправить координаты. Отказано в доступе", "OK");
                    return null;
                }
                var location = await Geolocation.GetLastKnownLocationAsync();
                //var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                //var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    UserLocation = location;
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
            catch (System.Reflection.TargetInvocationException ex)
            {
                var test = ex;
                // Unable to get location
                errorMessage = "Unable to get location";
            }
            catch (Exception)
            {
                // Unable to get location
                errorMessage = "Unable to get location";
            }

            if (errorMessage != null)
            {
                Console.WriteLine(errorMessage);
                //await Application.Current.MainPage.DisplayAlert("Ошибка", errorMessage, "OK");
                return null;
            }

            return UserLocation;
        }

        public async Task<PermissionStatus> CheckAndRequestLocationPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await BeginInvokeOnMainThreadAsync(() => Permissions.RequestAsync<Permissions.LocationWhenInUse>());           
            }

            // Additionally could prompt the user to turn on in settings

            return status;
        }

        private Task<T> BeginInvokeOnMainThreadAsync<T>(Func<Task<T>> a)
        {
            var tcs = new TaskCompletionSource<T>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var result = await a();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }
    }
}