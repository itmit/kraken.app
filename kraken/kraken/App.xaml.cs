using FreshMvvm;
using kraken.Models;
using kraken.PageModels;
using kraken.Pages;
using Realms;
using System;
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

            Page loginPage = FreshPageModelResolver.ResolvePageModel<AuthorizationPageModel>();
            FreshNavigationContainer loginContainer = new FreshNavigationContainer(loginPage, NavigationContainerNames.AuthenticationContainer);

            FreshTabbedNavigationContainer tabbedNavigation = new FreshTabbedNavigationContainer(NavigationContainerNames.MainContainer);
            tabbedNavigation.On<Xamarin.Forms.PlatformConfiguration.Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);

            var tabIconProfile = "ic_action_person.png";
            var tabIconCreateRequests = "ic_action_note_add.png";
            var tabIconMyRequest = "ic_action_list_alt.png";
            var tabIconExit = "ic_action_input.png";

            if(Device.iOS == Device.RuntimePlatform)
            {
                tabIconProfile = "baseline_person_black_24pt_1x.png";
                tabIconCreateRequests = "baseline_note_add_black_24pt_1x.png";
                tabIconMyRequest = "baseline_list_alt_black_24pt_1x.png";
                tabIconExit = "baseline_input_black_24pt_1x.png";
            }

            tabbedNavigation.AddTab<MyProfilePageModel>("Профиль", tabIconProfile, null);
            tabbedNavigation.AddTab<CreateRequestPageModel>("Создать запрос", tabIconCreateRequests, null);
            tabbedNavigation.AddTab<MyRequestPageModel>("Мои запросы", tabIconMyRequest, null);
            tabbedNavigation.AddTab<ExitPageModel>("Выход", tabIconExit, null);

            tabbedNavigation.SelectedTabColor = Color.Red;
            tabbedNavigation.UnselectedTabColor = Color.FromHex("#9E9E9E");

            Realm realm = Realm.GetInstance();
            IQueryable<User> user = realm.All<User>();
            bool UserIsFound = user?.Count() > 0;

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
