using Foundation;
using kraken.iOS.Services;
using kraken.Messages;
using UIKit;
using Xamarin.Forms;

namespace kraken.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            WireUpLongRunningTask();

            return base.FinishedLaunching(app, options);
        }

        iOSLongRunningTaskExample longRunningTaskExample;

        void WireUpLongRunningTask()
        {
            MessagingCenter.Subscribe<StartLongRunningTaskMessage>(this, "StartLongRunningTaskMessage", async message => {
                longRunningTaskExample = new iOSLongRunningTaskExample();
                await longRunningTaskExample.Start();
            });

            MessagingCenter.Subscribe<StopLongRunningTaskMessage>(this, "StopLongRunningTaskMessage", message => {
                longRunningTaskExample.Stop();
            });
        }
    }
}
