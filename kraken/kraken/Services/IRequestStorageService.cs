﻿using kraken.Models;
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
        Task<List<Master>> GetRequestMastersAsync(string RequestUuid);

        Task<bool> SendNewRequestAsync(Request CreatedRequest, string[] FilesArray);
        Task<bool> SendAcceptRequest(string RequestUuid);
        Task<bool> SendDeclineRequest(string RequestUuid);
        Task<bool> SendAcceptMasterRequest(string uuid, Master selectedMaster);
    }
}
