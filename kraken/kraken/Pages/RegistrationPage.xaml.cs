using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace kraken.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegistrationPage : ContentPage
    {
        public RegistrationPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Выполняет переход на страницу профиля 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MyProfilePage());
        }

        /// <summary>
        /// Выполняет переход на страницу авторизации
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Clicked_1(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}