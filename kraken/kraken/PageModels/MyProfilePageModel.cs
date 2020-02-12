using FreshMvvm;
using kraken.Models;
using Realms;
using System;
using System.Linq;

namespace kraken.PageModels
{
    public class MyProfilePageModel : FreshBasePageModel
    {
        public Realm Realm { get { return Realm.GetInstance(); } }
        public User CurrentUser { get; private set; }
        public string UserName { get; private set; }
        public MyProfilePageModel()
        {

        }

        

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
            CurrentUser = Realm.All<User>().Last();

            UserName = CurrentUser.Name;
        }
    }
}
