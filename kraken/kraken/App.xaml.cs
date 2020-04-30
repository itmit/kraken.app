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

            bool UserIsFound = IsUserFound();

            ExtendedTabbedPage tabbedNavigation = SetUpTabbedNavigation();

            if (!IsUserLoggedIn & !UserIsFound)
            {
                MainPage = loginContainer;
            }
            else
            {
                MainPage = tabbedNavigation;
            }

            if (UserIsFound & IsUserMaster)
            {
                Services.IUserService userService = new Services.UserService();
                userService.StartSendingCoordinates();
            }
        }

        private ExtendedTabbedPage SetUpTabbedNavigation()
        {
            var RequestPageLable = "Мои запросы";

            ExtendedTabbedPage tabbedNavigation = new ExtendedTabbedPage(NavigationContainerNames.MainContainer);
            tabbedNavigation.On<Xamarin.Forms.PlatformConfiguration.Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);

            tabbedNavigation.AddTab<MyProfilePageModel>("Профиль", "ic_action_person.png", null);

            if (IsUserMaster)
            {
                RequestPageLable = "Зпросы";
            }
            else
            {
                tabbedNavigation.AddTab<CreateRequestPageModel>("Создать запрос", "ic_action_note_add.png", null);
            }
            
            tabbedNavigation.AddTab<MyRequestPageModel>(RequestPageLable, "ic_action_list_alt.png", null);
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
            FreshIOC.Container.Register<Services.IUserService, Services.UserService>();
        }

        private bool IsUserFound()
        {
            Realm realm = Realm.GetInstance();
            IQueryable<User> users = realm.All<User>();
            if(users?.Count() > 0)
            {
                var user = users.Last();
                IsUserMaster = (user.ClientType == "master");
                return true;
            }

            return false;
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
