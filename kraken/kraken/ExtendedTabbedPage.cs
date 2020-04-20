using FreshMvvm;

namespace kraken
{
    public class ExtendedTabbedPage : FreshTabbedNavigationContainer
    {
        public ExtendedTabbedPage(string navigationServiceName) : base(navigationServiceName)
        {
        }

        public void NotifyTabReselected()
        {
            CurrentPage.Navigation.PopToRootAsync();
        }
    }
}
