﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace kraken.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MasterViewCell : ViewCell
    {
        public MasterViewCell()
        {
            InitializeComponent();
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            Favourites.IsVisible = false;
            Grade.IsVisible = true;
        }
    }
}