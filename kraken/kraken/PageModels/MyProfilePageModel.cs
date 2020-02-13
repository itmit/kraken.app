using FreshMvvm;
using PropertyChanged;
using kraken.Models;
using Realms;
using System.Linq;
using System.Windows.Input;

namespace kraken.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class MyProfilePageModel : FreshBasePageModel
    {
        public Realm Realm { get { return Realm.GetInstance(); } }
        public User CurrentUser { get; set; }

        public string UserName { get; set; } = "test";
        public string UserOrganization { get; set; }
        public string UserEmail { get; set; }
        public string UserPhone { get; set; }
        public string UserAddress { get; set; }

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
            UserName = "";
            UserOrganization = "";
            UserEmail = "";
            UserPhone = "";
            UserAddress = "";
        }

        protected override void ViewIsAppearing(object sender, System.EventArgs e)
        {
            LoadUserInfo();
            base.ViewIsAppearing(sender, e);
        }

        public override void Init(object initData)
        {
            base.Init(initData);

            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            IQueryable<User> users = Realm.All<User>();
            if (users.Count() <= 0)
                return;

            CurrentUser = users.Last();

            UserName = CurrentUser.Name;
            UserOrganization = CurrentUser.Organization;
            UserEmail = CurrentUser.Email;
            UserPhone = CurrentUser.Phone;
            UserAddress = CurrentUser.Address;
        }
    }
}
