using FreshMvvm;
using kraken.Models;
using PropertyChanged;
using Realms;
using System;
using System.Linq;
using System.Windows.Input;

namespace kraken.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class ExitPageModel : FreshBasePageModel
    {
        public ICommand ExitCommand
        {
            get
            {
                return new Xamarin.Forms.Command((param) =>
                {
                    ExitClicked();
                });
            }
        }

        private async void ExitClicked()
        {
            bool answer = await CoreMethods.DisplayAlert("Внимание", "Вы действительно хотите выйти из приложения?", "Да", "Нет");

            if (answer == true)
            {
                ExitAppAsync();
            }
        }

        public ExitPageModel()
        {

        }

        private async void ExitAppAsync()
        {
            Realm CurrentRealm = Realm.GetInstance();
            CurrentRealm.Write(() =>
            {
                CurrentRealm.RemoveAll<User>();
            });

            Services.RequestStorageService.AuthenticationHeaderIsSet = false;

            App.IsUserLoggedIn = false;
            App.IsUserMaster = false;

            await CoreMethods.PopPageModel(true, false, true);
            CoreMethods.SwitchOutRootNavigation(NavigationContainerNames.AuthenticationContainer);
        }
    }
}
