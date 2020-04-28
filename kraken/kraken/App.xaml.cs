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

            tabbedNavigation.AddTab<MyProfilePageModel>("Профиль", "ic_action_person.png", null);
            tabbedNavigation.AddTab<CreateRequestPageModel>("Создать запрос", "ic_action_note_add.png", null);
            tabbedNavigation.AddTab<MyRequestPageModel>("Мои запросы", "ic_action_list_alt.png", null);
            tabbedNavigation.AddTab<ExitPageModel>("Выход", "ic_action_input.png", null);

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
