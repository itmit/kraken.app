using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Xamarin.Forms;
using Android.Content;
using kraken.Messages;

namespace kraken.Droid
{
    [Activity(Label = "Василий Петрович", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static readonly int PickImageId = 1000;

        public System.Threading.Tasks.TaskCompletionSource<string> PickImageTaskCompletionSource { set; get; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            Plugin.FirebasePushNotification.FirebasePushNotificationManager.ProcessIntent(this, Intent);

            WireUpLongRunningTask();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            Plugin.FirebasePushNotification.FirebasePushNotificationManager.ProcessIntent(this, intent);
        }

        void WireUpLongRunningTask()
        {
            MessagingCenter.Subscribe<StartLongRunningTaskMessage>(this, "StartLongRunningTaskMessage", message => {
                var intent = new Intent(this, typeof(LongRunningTaskService));
                StartService(intent);
            });

            MessagingCenter.Subscribe<StopLongRunningTaskMessage>(this, "StopLongRunningTaskMessage", message => {
                Intent intent = new Intent(this, typeof(LongRunningTaskService));
                StopService(intent);
            });
        }
    }
}