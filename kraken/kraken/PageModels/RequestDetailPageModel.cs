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
    public class RequestDetailPageModel : FreshBasePageModel
    {
        readonly IRequestStorageService _requestStorage;
        Request Request;

        public string RequestName { get; private set; }
        public string RequestDescription { get; private set; }
        public string RequestStatus { get; private set; }
        public string RequestUrgency { get; private set; }
        public List<Master> Masters { get; private set; }

        public bool IsMasterSelected { get; private set; }
        public bool IsCurrentUserMaster { get; private set; }

        public ICommand ShowMasterListCommand
        {
            get
            {
                return new Command(async (param) =>
                {

                    await CoreMethods.PushPageModel<MasterSelectPageModel>(Request);
                });
            }
        }

        public ICommand AcceptRequestCommand
        {
            get
            {
                return new Command((param) =>
                {
                    SendAcceptRequest();
                });
            }
        }

        public ICommand DeclineRequestCommand
        {
            get
            {
                return new Command((param) =>
                {
                    SendDeclineRequest();
                });
            }
        }

        public RequestDetailPageModel(IRequestStorageService requestStorage)
        {
            _requestStorage = requestStorage;
        }

        public override void Init(object initData)
        {
            base.Init(initData);
            Request = (Request)initData;
            CurrentPage.Title = Request.Work;

            FillRequestValues();
            //GetMastersAsync();
        }

        private void FillRequestValues()
        {
            RequestName = Request.Work;
            RequestDescription = Request.Description;
            RequestStatus = "Статус: " + Request.StatusText;
            RequestUrgency = Request.UrgencyText;

            User CurrentUser = GetCurrentUser();

            IsCurrentUserMaster = App.IsUserMaster;

            if (Request.MasterId != null & Request.MasterId == CurrentUser.MasterId)
            {
                IsMasterSelected = true;
            }
            else
            {
                IsMasterSelected = false;
            }
        }

        private User GetCurrentUser()
        {
            Realm realm = Realm.GetInstance();
            var users = realm.All<User>();
            User user = new User();

            if (users.Count() > 0)
            {
                user = users.Last();
            }

            return user;
        }

        private async void SendAcceptRequest()
        {
            bool response = await _requestStorage.SendAcceptRequest(Request.uuid);

            if(response == true)
            {
                IsMasterSelected = true;
            }
        }

        private async void SendDeclineRequest()
        {
            bool response = await _requestStorage.SendDeclineRequest(Request.uuid);
        }
    }
}
