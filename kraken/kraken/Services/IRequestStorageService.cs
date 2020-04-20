using kraken.Models;
using System.Collections.Generic;
using Realms;
using System.Threading.Tasks;

namespace kraken.Services
{
    public interface IRequestStorageService
    {
        Realm Realm { get; }

        Task<Request> GetRequestFullInfo(string RequestUuid);
        Task<List<Request>> GetUserRequestsAsync();
        Task<List<WorkType>> GetWorkTypesAsync();
        Task<List<Master>> GetMastersAsync(string RequestUuid);

        Task<bool> SendNewRequestAsync(Request CreatedRequest);
        Task<bool> DeleteUserObjectsAsync(object objectToDelete);
    }
}
