using System.Threading.Tasks;

namespace kraken.Services
{
    public interface IUserService
    {
        Task<bool> LoginUserAsync(string EmailEntry, string PasswordEntry);

        Task<bool> SetDrivingModeAsync(string modeCode);
        Task<bool> SetStatusAsync(string radiusValue);
        Task<bool> SetSearchFilterAsync(string statusValue);

        void StartSendingCoordinates();
        void StopSendingCoordinates();
    }
}
