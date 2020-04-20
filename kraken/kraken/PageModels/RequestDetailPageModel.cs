using FreshMvvm;
using kraken.Models;
using kraken.Services;
using PropertyChanged;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace kraken.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class RequestDetailPageModel : FreshBasePageModel
    {
        readonly IRequestStorageService _requestStorage;
        Request Request;

        public string RequestName { get; private set; }
        public string RequestDescription { get; private set; }
        public List<Master> Masters { get; private set; }

        public RequestDetailPageModel(IRequestStorageService requestStorage)
        {
            _requestStorage = requestStorage;
        }

        public override void Init(object initData)
        {
            base.Init(initData);
            Request = (Request)initData;
            CurrentPage.Title = Request.Work;

            RequestName = Request.Work;
            RequestDescription = Request.Description;

            GetMastersAsync();
        }

        private async void GetMastersAsync()
        {
            Masters = await _requestStorage.GetMastersAsync(Request.uuid);
        }
    }
}
