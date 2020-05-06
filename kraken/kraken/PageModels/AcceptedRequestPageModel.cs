using FreshMvvm;
using kraken.Models;
using kraken.Services;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace kraken.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class AcceptedRequestPageModel : FreshBasePageModel
    {
        readonly IRequestStorageService _requestStorage;
        Request _selectedRequest;

        private List<Request> AllRequests { get; set; } = new List<Request>();

        public ObservableCollection<Request> UserRequests { get; set; }

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

        public bool IsRefreshing { get; set; } = false;

        public ICommand UpdateListCommand
        {
            get
            {
                return new Xamarin.Forms.Command(async () =>
                {
                    IsRefreshing = true;

                    await GetMasterRequestsAsync();

                    IsRefreshing = false;
                });
            }
        }

        public AcceptedRequestPageModel(IRequestStorageService requestStorage)
        {
            _requestStorage = requestStorage;
        }

        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);

            await GetMasterRequestsAsync();
        }

        private async Task GetMasterRequestsAsync()
        {
            var updatedList = await _requestStorage.GetMasterRequestsAsync();

            if (AllRequests.SequenceEqual(updatedList) == false)
            {
                AllRequests = updatedList;
                UserRequests = new ObservableCollection<Request>(AllRequests.ToList());
            }
        }

        private async void OpenDetailPage(Request selectedRequest)
        {
            await CoreMethods.PushPageModel<RequestDetailPageModel>(selectedRequest);
        }
    }
}
