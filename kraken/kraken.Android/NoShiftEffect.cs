using Android.Support.Design.BottomNavigation;
using Android.Support.Design.Widget;
using Android.Views;
using kraken.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ResolutionGroupName("itmit")]
[assembly: ExportEffect(typeof(NoShiftEffect), "NoShiftEffect")]
namespace kraken.Droid
{
    public class NoShiftEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            if (!(Container.GetChildAt(0) is ViewGroup layout))
            {
                return;
            }
            if (!(layout.GetChildAt(1) is BottomNavigationView bottomNavigationView))
            {
                return;
            }

            bottomNavigationView.LabelVisibilityMode = LabelVisibilityMode.LabelVisibilityLabeled;
        }

        protected override void OnDetached()
        {
        }
    }
}