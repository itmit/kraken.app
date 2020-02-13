using FreshMvvm;
using kraken.Models;
using kraken.PageModels;
using kraken.Pages;
using Realms;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace kraken
{
    public partial class App : Xamarin.Forms.Application
    {
        public static bool IsUserLoggedIn { get; set; }

        public App()
        {
            InitializeComponent();

            var loginPage = FreshPageModelResolver.ResolvePageModel<AuthorizationPageModel>();
            var loginContainer = new FreshNavigationContainer(loginPage, NavigationContainerNames.AuthenticationContainer);

            var tabbedNavigation = new FreshTabbedNavigationContainer();
            tabbedNavigation.On<Xamarin.Forms.PlatformConfiguration.Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);

            tabbedNavigation.AddTab<MyProfilePageModel>("Профиль", "ic_action_person.png", null);
            tabbedNavigation.AddTab<CreateRequestPageModel>("Создать запрос", "ic_action_note_add.png", null);
            tabbedNavigation.AddTab<MyRequestPageModel>("Мои запросы", "ic_action_list_alt.png", null);
            tabbedNavigation.AddTab<ExitPageModel>("Выход", "ic_action_input.png", null);

            tabbedNavigation.SelectedTabColor = Color.FromHex("#e0e0e0");
            tabbedNavigation.UnselectedTabColor = Color.FromHex("#9E9E9E");

            var realm = Realm.GetInstance();
            var user = realm.All<User>();
            var UserIsFound = user?.Count() > 0;

            if (!IsUserLoggedIn & !UserIsFound)
            {
                MainPage = loginContainer;
            }
            else
            {
                MainPage = tabbedNavigation;
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
