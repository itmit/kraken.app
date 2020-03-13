using System;

namespace kraken.Services
{
    public interface ILocationHelper
    {
        void SetUpLocationService();
        void RequestLocationUpdates();
        void GetLastLocation();
    }
}
