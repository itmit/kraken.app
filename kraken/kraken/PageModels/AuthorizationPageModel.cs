using FreshMvvm;
using PropertyChanged;
using System.Windows.Input;
using Realms;
using System.Threading.Tasks;
using kraken.Services;
using Xamarin.Forms;

namespace kraken.PageModels
{
    [AddINotifyPropertyChangedInterface]

    public class AuthorizationPageModel : FreshBasePageModel
    {
        readonly IUserService _userService;

        public Realm Realm { get { return Realm.GetInstance(); } }

        public string UsernameEntry { get; set; } = string.Empty;
        public string EmailEntry { get; set; } = string.Empty;
        public string PasswordEntry { get; set; } = string.Empty;

        public AuthorizationPageModel(IUserService userService)
        {
            _userService = userService;
        }

        public ICommand OnLoginButtonClicked
        {
            get
            {
                return new FreshAwaitCommand(async (param, tcs) =>
                {
                    await OnLoginClickedAsync();
                    tcs.SetResult(true);
                });
            }
        }

        public ICommand OpenRegisterationCommand
        {
            get
            {
                return new Command((param) =>
                {
                    OpenRegisterPage();
                });
            }
        }

        async void OpenRegisterPage()
        {
            await CoreMethods.PushPageModel<RegistrationPageModel>();
        }

        async Task OnLoginClickedAsync()
        {
            if (App.DeviceToken == null)
            {
                App.DeviceToken = Plugin.FirebasePushNotification.CrossFirebasePushNotification.Current.Token;
            }

            bool isValid = await AreCredentialsCorrectAsync();
            if (isValid)
            {
                App.IsUserLoggedIn = true;
                if (App.IsUserMaster)
                {
                    CoreMethods.SwitchOutRootNavigation(NavigationContainerNames.MasterTabsContainer);
                }
                else
                {
                    CoreMethods.SwitchOutRootNavigation(NavigationContainerNames.ClientTabsContainer);
                }
            }

        }

        async Task<bool> AreCredentialsCorrectAsync()
        {
            if (string.IsNullOrWhiteSpace(EmailEntry) | EmailEntry.Length <= 7 |
                string.IsNullOrWhiteSpace(PasswordEntry) | PasswordEntry.Length < 6)
            {
                await CoreMethods.DisplayAlert("Ошибка", "Неверно указан email или пароль", "OK");
                return false;
            }

            bool result = await _userService.LoginUserAsync(EmailEntry, PasswordEntry);

            return result;
        }
    }
}
