
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace kraken.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ExitPage : ContentPage
    {
        public ExitPage()
        {
            Application.Current.Quit();
            InitializeComponent();
        }
    }
}