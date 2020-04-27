using FreshMvvm;
using kraken.Models;
using kraken.PageModels;
using Plugin.FirebasePushNotification;
using Realms;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace kraken
{
    public partial class App : Xamarin.Forms.Application
    {
        public static bool IsUserLoggedIn { get; set; }
        public static bool IsUserMaster { get; set; } = false;

        public static string DeviceToken { get; set; }

        public App()
        {
            InitializeComponent();

            SetUpNotifications();

            SetUpIoC();

            Current.On<Xamarin.Forms.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);

            Page loginPage = FreshPageModelResolver.ResolvePageModel<AuthorizationPageModel>();
            FreshNavigationContainer loginContainer = new FreshNavigationContainer(loginPage, NavigationContainerNames.AuthenticationContainer);

            ExtendedTabbedPage tabbedNavigation = SetUpTabbedNavigation();

            bool UserIsFound = IsUserFound();

            if (!IsUserLoggedIn & !UserIsFound)
            {
                MainPage = loginContainer;
            }
            else
            {
                MainPage = tabbedNavigation;
            }
        }

        private ExtendedTabbedPage SetUpTabbedNavigation()
        {
            ExtendedTabbedPage tabbedNavigation = new ExtendedTabbedPage(NavigationContainerNames.MainContainer);
            tabbedNavigation.On<Xamarin.Forms.PlatformConfiguration.Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);

            var tabIconProfile = "ic_action_person.png";
            var tabIconCreateRequests = "ic_action_note_add.png";
            var tabIconMyRequest = "ic_action_list_alt.png";
            var tabIconExit = "ic_action_input.png";

            if (Device.iOS == Device.RuntimePlatform)
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

            //tabbedNavigation.CurrentPageChanged += TabbedNavigation_CurrentPageChanged;

            return tabbedNavigation;
        }

        private void TabbedNavigation_CurrentPageChanged(object sender, System.EventArgs e)
        {
            var navigationContainer = (FreshTabbedNavigationContainer)sender;
            navigationContainer.CurrentPage.Navigation.PopToRootAsync();
        }

        private void SetUpNotifications()
        {
            CrossFirebasePushNotification.Current.Subscribe("general");

            CrossFirebasePushNotification.Current.OnNotificationOpened += (s, p) =>
            {
                //if (p.Data.Keys.Contains("dialog_id"))
                //{
                //    string id = p.Data["dialog_id"].ToString();
                //    string updatedAt = p.Data["updated_at"].ToString();
                //    string phone = p.Data["phone"].ToString();
                //    string avatar = p.Data["avatar"].ToString();

                //    SelectedDialog = new Dialog
                //    {
                //        Id = id,
                //        UserPhone = phone,
                //        Updated_at = updatedAt,
                //        ImageUri = avatar
                //    };
                //    IsNotificationOpened = true;
                //}
            };

            CrossFirebasePushNotification.Current.OnTokenRefresh += (s, p) =>
            {
                DeviceToken = p.Token;
            };
        }

        private void SetUpIoC()
        {
            FreshIOC.Container.Register<Services.IRequestStorageService, Services.RequestStorageService>();
        }

        private bool IsUserFound()
        {
            Realm realm = Realm.GetInstance();
            IQueryable<User> user = realm.All<User>();
            return user?.Count() > 0;
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
