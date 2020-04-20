using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Views;
using kraken;
using kraken.Droid;
using Xamarin.Forms.Platform.Android.AppCompat;

[assembly: Xamarin.Forms.ExportRenderer(typeof(ExtendedTabbedPage), typeof(ExtendedTabbedPageRenderer))]

namespace kraken.Droid
{
    public class ExtendedTabbedPageRenderer : TabbedPageRenderer, BottomNavigationView.IOnNavigationItemReselectedListener
    {
        private bool _isShiftModeSet;

        public ExtendedTabbedPageRenderer(Context context) : base(context)
        {

        }

        public void OnNavigationItemReselected(IMenuItem item)
        {
            if (Element is ExtendedTabbedPage)
            {
                var mainTabPage = Element as ExtendedTabbedPage;
                mainTabPage.NotifyTabReselected();
            }
        }

        // Fixed Tabs
        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);
            try
            {
                if (!_isShiftModeSet)
                {
                    var children = GetAllChildViews(ViewGroup);

                    if (children.SingleOrDefault(x => x is BottomNavigationView) is BottomNavigationView bottomNav)
                    {
                        bottomNav.SetOnNavigationItemReselectedListener(this);
                        //bottomNav.SetShiftMode(false, false);
                        _isShiftModeSet = true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error setting ShiftMode: {e}");
            }
        }

        private List<View> GetAllChildViews(View view)
        {
            if (!(view is ViewGroup group))
            {
                return new List<View> { view };
            }

            var result = new List<View>();

            for (int i = 0; i < group.ChildCount; i++)
            {
                var child = group.GetChildAt(i);

                var childList = new List<View> { child };
                childList.AddRange(GetAllChildViews(child));

                result.AddRange(childList);
            }

            return result.Distinct().ToList();
        }
    }
}