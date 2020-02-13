using kraken.Models;
using System.Collections.Generic;
using Realms;
using System.Threading.Tasks;

namespace kraken.Services
{
    public interface IRequestStorageService
    {
        Realm Realm { get; }

        Request GetRequest(string id);
        Task<List<Request>> GetAllRequestsAsync();
        Task<Request> CreateRequest();

        void AddRequest(Request dialog);
        void UpdateRequest(Request dialog);
        void DeleteRequest(Request dialog);
        bool DoesRequestExist(Request dialog);
        void DeleteAllRequests();
    }
}
