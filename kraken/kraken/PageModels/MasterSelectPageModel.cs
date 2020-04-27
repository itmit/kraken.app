using FreshMvvm;
using kraken.Models;
using kraken.Services;
using PropertyChanged;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace kraken.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class MasterSelectPageModel : FreshBasePageModel
    {
        readonly IRequestStorageService _requestStorage;

        public MasterSelectPageModel(IRequestStorageService requestStorage)
        {
            _requestStorage = requestStorage;
        }

        public Request Request { get; private set; }
        public List<Master> Masters { get; private set; }

        public override void Init(object initData)
        {
            base.Init(initData);
            Request = (Request)initData;
            CurrentPage.Title = Request.Work;

            GetMastersAsync();
        }

        private async void GetMastersAsync()
        {
            Masters = await _requestStorage.GetMastersAsync(Request.uuid);
        }
    }
}
