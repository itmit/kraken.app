using FreshMvvm;
using kraken.Models;
using kraken.Services;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace kraken.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class MyRequestPageModel : FreshBasePageModel
    {
        readonly IRequestStorageService _requestStorage;
        Request _selectedRequest;

        public List<Request> UserRequests { get; set; }

        public Request SelectedRequest
        {
            get { return _selectedRequest; }
            set
            {
                _selectedRequest = value;
                if (value != null)
                    OpenDetailPage(value);
            }
        }

        public MyRequestPageModel(IRequestStorageService requestStorage)
        {
            _requestStorage = requestStorage;
        }

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);

            GetUserRequestsAsync();
        }

        private async void GetUserRequestsAsync()
        {
            UserRequests = await _requestStorage.GetUserRequestsAsync();
        }

        private async void OpenDetailPage(Request selectedRequest)
        {
            await CoreMethods.PushPageModel<RequestDetailPageModel>(selectedRequest);
        }
    }
}
