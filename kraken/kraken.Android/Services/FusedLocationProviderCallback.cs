using System.Linq;

using Android.Gms.Location;
using Android.Util;

namespace kraken.Droid.Services
{
    public class FusedLocationProviderCallback : LocationCallback
    {
        public FusedLocationProviderCallback()
        {
            
        }

        public override void OnLocationAvailability(LocationAvailability locationAvailability)
        {
            Log.Debug("FusedLocationProviderSample", "IsLocationAvailable: {0}", locationAvailability.IsLocationAvailable);
        }


        public override void OnLocationResult(LocationResult result)
        {
            if (result.Locations.Any())
            {
                var location = result.Locations.First();
            }
            else
            {
                // TODO:
                // Show alert with "Resource.String.location_unavailable" and "Resource.String.could_not_get_last_location" text
            }
        }
    }
}