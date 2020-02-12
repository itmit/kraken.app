using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace kraken.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateRequestPage : ContentPage
    {
        public CreateRequestPage()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            ListImage.IsVisible = true;
        }
    }
}