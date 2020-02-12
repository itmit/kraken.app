using FreshMvvm;
using kraken.Models;
using kraken.PageModels;
using kraken.Pages;
using Realms;
using System.Linq;
using Xamarin.Forms;

namespace kraken
{
    public partial class App : Application
    {
        public static bool IsUserLoggedIn { get; set; }

        public App()
        {
            InitializeComponent();

            var loginPage = FreshPageModelResolver.ResolvePageModel<AuthorizationPageModel>();
            var loginContainer = new FreshNavigationContainer(loginPage, NavigationContainerNames.AuthenticationContainer);

            var mainPage = FreshPageModelResolver.ResolvePageModel<RootTabbedPageModel>();
            var mainContainer = new FreshNavigationContainer(mainPage, NavigationContainerNames.MainContainer);

            var realm = Realm.GetInstance();
            var user = realm.All<User>();
            var UserIsFound = user?.Count() > 0;

            if (!IsUserLoggedIn & !UserIsFound)
            {
                MainPage = loginContainer;
            }
            else
            {
                MainPage = mainContainer;
            }
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
