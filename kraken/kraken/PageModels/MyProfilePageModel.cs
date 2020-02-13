using FreshMvvm;
using PropertyChanged;
using kraken.Models;
using Realms;
using System;
using System.Linq;
using System.Windows.Input;

namespace kraken.PageModels
{
    [AddINotifyPropertyChangedInterface]

    public class MyProfilePageModel : FreshBasePageModel
    {
        public Realm Realm { get { return Realm.GetInstance(); } }
        public User CurrentUser { get; private set; }

        public string UserName { get; private set; }
        public string UserOrganization { get; private set; }
        public string UserEmail { get; private set; }
        public string UserPhone { get; private set; }
        public string UserAddress { get; private set; }

        public ICommand OnRequestsButtonClicked
        {
            get
            {
                return new FreshAwaitCommand((param, tcs) =>
                {
                    OpenRequestsPage();
                });
            }
        }

        async void OpenRequestsPage()
        {
            await CoreMethods.PushPageModel<MyRequestPageModel>();
        }

        public MyProfilePageModel()
        {

        }

        public override void Init(object initData)
        {
            var users = Realm.All<User>();
            if (users.Count() <= 0)
            {
                return;
            }

            CurrentUser = users.Last();

            UserName = CurrentUser.Name;
            UserOrganization = CurrentUser.Organization;
            UserEmail = CurrentUser.Email;
            UserPhone = CurrentUser.Phone;
            UserAddress = CurrentUser.Address;
        }
    }
}
