
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace kraken.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyRequestPage : ContentPage
    {
        public MyRequestPage()
        {
            InitializeComponent();
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            Navigation.PushAsync(new RequestDetailPage());
        }
    }
}