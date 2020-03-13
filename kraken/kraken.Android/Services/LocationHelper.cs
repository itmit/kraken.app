using kraken.Services;
using System.Threading.Tasks;
using Android.Gms.Common;
using Android.Gms.Location;
using Android.OS;
using Android.Util;
using Android.App;
using System;
using Android.Support.V4.Content;
using Android;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Support.Design.Widget;

namespace kraken.Droid.Services
{
    public class LocationHelper : ILocationHelper
    {
        const long ONE_MINUTE = 60 * 1000;
        const long FIVE_MINUTES = 5 * ONE_MINUTE;
        const long TWO_MINUTES = 2 * ONE_MINUTE;

        static readonly int RC_LAST_LOCATION_PERMISSION_CHECK = 1000;

        static readonly string KEY_REQUESTING_LOCATION_UPDATES = "requesting_location_updates";

        Activity CurrentActivity = (Activity)Application.Context;

        FusedLocationProviderClient fusedLocationProviderClient;
        bool isGooglePlayServicesInstalled;
        bool isRequestingLocationUpdates;
        LocationCallback locationCallback;
        LocationRequest locationRequest;

        public void SetUpLocationService()
        {
            Bundle bundle = null;
            if (bundle != null)
            {
                isRequestingLocationUpdates = bundle.KeySet().Contains(KEY_REQUESTING_LOCATION_UPDATES) &&
                                              bundle.GetBoolean(KEY_REQUESTING_LOCATION_UPDATES);
            }
            else
            {
                isRequestingLocationUpdates = false;
            }

            isGooglePlayServicesInstalled = IsGooglePlayServicesInstalled();

            if (isGooglePlayServicesInstalled)
            {
                locationRequest = new LocationRequest()
                                  .SetPriority(LocationRequest.PriorityHighAccuracy)
                                  .SetInterval(FIVE_MINUTES)
                                  .SetFastestInterval(TWO_MINUTES);
                locationCallback = new FusedLocationProviderCallback();

                fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(Application.Context);
            }
            else
            {
                // TODO:
                // If there is no Google Play Services installed, then show error or create location prowider implementation.
            }
        }

        public async void RequestLocationUpdates()
        {
            // No need to request location updates if we're already doing so.
            if (isRequestingLocationUpdates)
            {
                StopRequestLocationUpdates();
                isRequestingLocationUpdates = false;
            }
            else
            {
                if (ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.AccessFineLocation) == Permission.Granted)
                {
                    await StartRequestingLocationUpdates();
                    isRequestingLocationUpdates = true;
                }
                else
                {
                    RequestLocationPermission(RC_LAST_LOCATION_PERMISSION_CHECK);
                }
            }
        }

        public async void GetLastLocation()
        {
            if (ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.AccessFineLocation) == Permission.Granted)
            {
                await GetLastLocationFromDevice();
            }
            else
            {
                RequestLocationPermission(RC_LAST_LOCATION_PERMISSION_CHECK);
            }
        }

        private async Task GetLastLocationFromDevice()
        {
            var location = await fusedLocationProviderClient.GetLastLocationAsync();

            if (location == null)
            {
                // TODO:
                // Show alert with "Resource.String.location_unavailable" and "Resource.String.could_not_get_last_location" text
            }
            else
            {
                var lat = location.Latitude;
                var lon = location.Longitude;
                var locationProvider = location.Provider;
            }
        }

        void RequestLocationPermission(int requestCode)
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(CurrentActivity, Manifest.Permission.AccessFineLocation))
            {
                // show alert to accept permissions

                //Snackbar.Make(CurrentActivity., Resource.String.permission_location_rationale, Snackbar.LengthIndefinite)
                //        .SetAction(Resource.String.ok,
                //                   delegate
                //                   {
                //                       ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.AccessFineLocation }, requestCode);
                //                   })
                //        .Show();
            }
            else
            {
                ActivityCompat.RequestPermissions(CurrentActivity, new[] { Manifest.Permission.AccessFineLocation }, requestCode);
            }
        }

        async Task StartRequestingLocationUpdates()
        {
            await fusedLocationProviderClient.RequestLocationUpdatesAsync(locationRequest, locationCallback);
        }

        async void StopRequestLocationUpdates()
        {
            if (isRequestingLocationUpdates)
            {
                await fusedLocationProviderClient.RemoveLocationUpdatesAsync(locationCallback);
            }
        }

        bool IsGooglePlayServicesInstalled()
        {
            var queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(Android.App.Application.Context);
            if (queryResult == ConnectionResult.Success)
            {
                Log.Info("MainActivity", "Google Play Services is installed on this device.");
                return true;
            }

            if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult))
            {
                var errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
                Log.Error("MainActivity", "There is a problem with Google Play Services on this device: {0} - {1}",
                          queryResult, errorString);
            }

            return false;
        }
    }
}