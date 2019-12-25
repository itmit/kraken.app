using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace kraken.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Accordion : ContentView
    {
        /// <summary>
        /// Задает представление для индикатора аккордиона
        /// </summary>
        public static readonly BindableProperty IndicatorViewProperty = BindableProperty.Create(nameof(IndicatorView), typeof(View), typeof(Accordion), default(View));
        public View IndicatorView
        {
            get => (View)GetValue(IndicatorViewProperty);
            set => SetValue(IndicatorViewProperty, value);
        }

        /// <summary>
        /// Задает представление для контента внутри аккордиона
        /// </summary>
        public static readonly BindableProperty ContentViewProperty = BindableProperty.Create(nameof(AccordionContentView), typeof(View), typeof(Accordion), default(View));
        public View AccordionContentView
        {
            get => (View)GetValue(ContentViewProperty);
            set => SetValue(ContentViewProperty, value);
        }

        /// <summary>
        /// Задает заголовок аккордиона
        /// </summary>
        public static readonly BindableProperty TitleViewProperty = BindableProperty.Create(nameof(TitleView), typeof(string), typeof(Accordion), default(string));
        public string TitleView
        {
            get => (string)GetValue(TitleViewProperty);
            set => SetValue(TitleViewProperty, value);
        }

        public static readonly BindableProperty IsOpenBindablePropertyProperty = BindableProperty.Create(nameof(IsOpen), typeof(bool), typeof(Accordion), false, propertyChanged: IsOpenChanged);
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenBindablePropertyProperty); }
            set { SetValue(IsOpenBindablePropertyProperty, value); }
        }

        /// <summary>
        /// Задает фон выпадающего аккордиона
        /// </summary>
        public static readonly BindableProperty HeaderBackgroundColorProperty = BindableProperty.Create(nameof(HeaderBackgroundColor), typeof(Color), typeof(Accordion), Color.Transparent);
        public Color HeaderBackgroundColor
        {
            get { return (Color)GetValue(HeaderBackgroundColorProperty); }
            set { SetValue(HeaderBackgroundColorProperty, value); }
        }


        /// <summary>
        /// Управляет открытием и закрытием аккордиона
        /// </summary>
        /// <param name="bindable"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        private static void IsOpenChanged(BindableObject bindable, object oldValue, object newValue)
        {
            bool isOpen;

            if (bindable != null && newValue != null)
            {
                var control = (Accordion)bindable;
                isOpen = (bool)newValue;

                if (control.IsOpen == false)
                {
                    VisualStateManager.GoToState(control, "Open");
                    control.Close();
                }
                else
                {
                    VisualStateManager.GoToState(control, "Closed");
                    control.Open();
                }
            }
        }

        /// <summary>
        /// Задает время анимации
        /// </summary>
        public uint AnimationDuration { get; set; }

        public Accordion()
        {
            InitializeComponent();
            Close();
            AnimationDuration = 250;
            IsOpen = false;
        }

        /// <summary>
        /// Закрывает аккордион
        /// </summary>
        async void Close()
        {
            await Task.WhenAll(
                _accContent.TranslateTo(0, -10, AnimationDuration),
                _indicatorContainer.RotateTo(0, AnimationDuration),
                _accContent.FadeTo(0, 50)
                );
            _accContent.IsVisible = false;
        }

        /// <summary>
        /// Открывает аккордион
        /// </summary>
        async void Open()
        {
            _accContent.IsVisible = true;
            await Task.WhenAll(
                _accContent.TranslateTo(0, 10, AnimationDuration),
                _indicatorContainer.RotateTo(180, AnimationDuration),
                _accContent.FadeTo(30, 50, Easing.SinIn)
            );
        }

        /// <summary>
        /// Реагирует на касание аккордиона
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TitleTapped(object sender, EventArgs e)
        {
            IsOpen = !IsOpen;
        }
    }
}