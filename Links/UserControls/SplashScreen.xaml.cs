using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Links
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();

            double deskHeight = SystemParameters.PrimaryScreenHeight;
            double deskWidth = SystemParameters.PrimaryScreenWidth;

            this.Top = deskHeight / 2 - this.Height / 2;
            this.Left = deskWidth / 2 - this.Width / 2;

            ShowProgressbar();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private async Task ShowProgressbar()
        {
            await Task.Delay(1);

            var outAnimation = new DoubleAnimation
            {
                To = 1,
                BeginTime = TimeSpan.FromSeconds(0),
                Duration = TimeSpan.FromMilliseconds(100),
                FillBehavior = FillBehavior.Stop
            };

            outAnimation.Completed += (s, a) => ProgressBar.Opacity = 1;

            ProgressBar.BeginAnimation(UIElement.OpacityProperty, outAnimation);
        }
    }
}
