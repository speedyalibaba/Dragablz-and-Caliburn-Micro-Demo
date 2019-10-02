using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace Links.Common.Controls
{
    public class DialogContent : ContentControl
    {
        #region Fields

        public static readonly DependencyProperty ButtonsProperty =
            DependencyProperty.Register("Buttons", typeof(object),
            typeof(DialogContent), new PropertyMetadata(null));

        /// <summary>
        /// Using a DependencyProperty as the backing store for <see cref="CloseButtonStyle"/> Property.
        /// </summary>
        public static readonly DependencyProperty CloseButtonStyleProperty =
            DependencyProperty.Register(nameof(CloseButtonStyle), typeof(Style),
            typeof(DialogContent), new UIPropertyMetadata(null));

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string),
            typeof(DialogContent), new PropertyMetadata(null));

        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register("IsBusy", typeof(bool),
            typeof(DialogContent), new PropertyMetadata(false));

        /// <summary>
        /// Using a DependencyProperty as the backing store for <see cref="MessageQueue"/> Property.
        /// </summary>
        public static readonly DependencyProperty MessageQueueProperty =
            DependencyProperty.Register(nameof(MessageQueue), typeof(SnackbarMessageQueue),
            typeof(DialogContent), new UIPropertyMetadata(null));

        /// <summary>
        /// Using a DependencyProperty as the backing store for <see cref="ShowTitleBar"/> Property.
        /// </summary>
        public static readonly DependencyProperty ShowTitleBarProperty =
            DependencyProperty.Register(nameof(ShowTitleBar), typeof(bool),
            typeof(DialogContent), new UIPropertyMetadata(true));

        #endregion Fields

        #region Constructors

        public DialogContent()
        {
            SetCurrentValue(CloseButtonStyleProperty, this.FindResource("MaterialDesignFlatButton") as Style);
        }

        #endregion Constructors

        #region Properties

        public object Buttons
        {
            get { return GetValue(ButtonsProperty); }
            set { SetValue(ButtonsProperty, value); }
        }

        public Style CloseButtonStyle
        {
            get { return (Style)GetValue(CloseButtonStyleProperty); }
            set { SetValue(CloseButtonStyleProperty, value); }
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public SnackbarMessageQueue MessageQueue
        {
            get { return (SnackbarMessageQueue)GetValue(MessageQueueProperty); }
            set { SetValue(MessageQueueProperty, value); }
        }

        public bool ShowTitleBar
        {
            get { return (bool)GetValue(ShowTitleBarProperty); }
            set { SetValue(ShowTitleBarProperty, value); }
        }

        #endregion Properties
    }
}