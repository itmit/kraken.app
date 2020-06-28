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

        private List<Request> AllRequests { get; set; } = new List<Request>();

        public ObservableCollection<Grouping<string, Request>> RequestsGrouped { get; set; }

        public ObservableCollection<Request> UserRequests { get; set; }

        public Request SelectedRequest
        {
            get => null;
            set
            {
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

            AllRequests = updatedList;
            UserRequests = new ObservableCollection<Request>(AllRequests.ToList());

            var sorted = from request in AllRequests
                         orderby request.StatusText descending
                         group request by request.StatusText into requestGroup
                         select new Grouping<string, Request>(requestGroup.Key, requestGroup);

            RequestsGrouped = new ObservableCollection<Grouping<string, Request>>(sorted);
        }

        private async void OpenDetailPage(Request selectedRequest)
        {
            await CoreMethods.PushPageModel<RequestDetailPageModel>(selectedRequest);
        }
    }

    public class Grouping<K, T> : ObservableCollection<T>
    {
        public K Key { get; private set; }

        public Grouping(K key, IEnumerable<T> items)
        {
            Key = key;
            foreach (var item in items)
                this.Items.Add(item);
        }
    }
}
