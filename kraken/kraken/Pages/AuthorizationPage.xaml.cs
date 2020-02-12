using kraken.Views;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace kraken.Pages
{
    public partial class AuthorizationPage : ContentPage
    {
        public AuthorizationPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Выполняет переход на страницу регистрации
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            Navigation.PushAsync(new RegistrationPage());
        }

        /// <summary>
        /// Выполняет переход на страницу профиля
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new TabbedView());
        }
    }
}