
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Xaml;

namespace kraken.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TabbedView : Xamarin.Forms.TabbedPage
    {
        public TabbedView()
        {
            InitializeComponent();
            On<Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);
            Effects.Add(new NoShiftEffect());
        }
    }
}