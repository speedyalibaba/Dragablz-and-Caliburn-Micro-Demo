using Caliburn.Micro;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Links.Services
{
    public class MahappsWindowManager : WindowManager
    {
        protected override Window EnsureWindow(object model, object view, bool isDialog)
        {
            var window = view as Window;

            if (window == null)
            {
                window = new MahApps.Metro.Controls.MetroWindow
                {
                    Content = view,
                    SizeToContent = SizeToContent.Manual,
                    ResizeMode = ResizeMode.CanResizeWithGrip,
                    BorderThickness = new Thickness(0),
                };
                window.SetResourceReference(System.Windows.Documents.TextElement.ForegroundProperty, "MaterialDesignBody");
                window.SetResourceReference(MahApps.Metro.Controls.MetroWindow.BackgroundProperty, "MaterialDesignPaper");
                window.SetValue(System.Windows.Documents.TextElement.FontWeightProperty, FontWeights.Medium);
                window.SetValue(System.Windows.Documents.TextElement.FontSizeProperty, 14D);

                window.SetResourceReference(Control.FontFamilyProperty, "MaterialDesignFont");
                window.SetResourceReference(MahApps.Metro.Controls.MetroWindow.GlowBrushProperty, "AccentColorBrush");

                window.Loaded += (s, e) => window.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

                window.SetValue(View.IsGeneratedProperty, true);

                var owner = InferOwnerOf(window);
                if (owner != null)
                {
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    window.Owner = owner;
                }
                else
                {
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
                return window;
            }
            else
            {
                return base.EnsureWindow(model, view, isDialog);
            }
        }
    }
}
