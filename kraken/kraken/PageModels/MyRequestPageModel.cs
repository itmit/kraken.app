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
    public class MyRequestPageModel : FreshBasePageModel
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

                    await GetUserRequestsAsync();

                    IsRefreshing = false;
                });
            }
        }

        public MyRequestPageModel(IRequestStorageService requestStorage)
        {
            _requestStorage = requestStorage;
        }

        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);

            await GetUserRequestsAsync();
        }

        private async Task GetUserRequestsAsync()
        {
            var updatedList = await _requestStorage.GetUserRequestsAsync();

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
