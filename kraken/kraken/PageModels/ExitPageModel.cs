using FreshMvvm;
using PropertyChanged;
using System;

namespace kraken.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class ExitPageModel : FreshBasePageModel
    {
        public ExitPageModel()
        {

        }

        protected override async void ViewIsAppearing(object sender, System.EventArgs e)
        {
            bool answer = await CoreMethods.DisplayAlert("Question?", "Would you like to play a game", "Yes", "No");
            System.Diagnostics.Debug.WriteLine("Answer: " + answer);
            base.ViewIsAppearing(sender, e);
        }
    }
}
