using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace kraken.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RequestDetailPage : ContentPage
    {
        public RequestDetailPage()
        {
            InitializeComponent();
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}