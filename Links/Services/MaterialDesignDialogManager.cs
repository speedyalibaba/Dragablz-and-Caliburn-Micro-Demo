using Links.Localization;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Links.Contract.Services;
using Links.UserControls;

namespace Links.Services
{
	public class MaterialDesignDialogManager : Conductor<object>, IDialogManager
	{
		private readonly IWindowManager _windowManager;

		public MaterialDesignDialogManager(IWindowManager windowManager)
		{
			_windowManager = windowManager;
		}

		public bool ShowDialog(IScreen dialogModel)
		{
            return ShowDialog(dialogModel, new ExpandoObject());
        }

        public bool ShowDialog(IScreen dialogModel, ExpandoObject settings)
        {
            var dialogResult = _windowManager.ShowDialog(dialogModel, settings: (dynamic)settings);
            if (dialogResult.GetType() == typeof(bool))
                return dialogResult;
            return (dialogResult & (MessageBoxOptions.Yes | MessageBoxOptions.Ok)) != 0;
        }

        public bool ShowMessageBox(string message, string title, MessageBoxOptions options = MessageBoxOptions.Ok)
		{
			return (ShowMessageBox(message, title, options, MessageBoxType.Info) & (MessageBoxOptions.Yes | MessageBoxOptions.Ok)) != 0;
		}

		public bool ShowErrorMessageBox(string message, string title = null, MessageBoxOptions options = MessageBoxOptions.Ok)
		{
			return (ShowMessageBox(message, title ?? Translations.Error, options, MessageBoxType.Error) & (MessageBoxOptions.Yes | MessageBoxOptions.Ok)) != 0;
		}

		public bool ShowWarnMessageBox(string message, string title, MessageBoxOptions options = MessageBoxOptions.Ok)
		{
			return (ShowMessageBox(message, title ?? Translations.Warning, options, MessageBoxType.Error) & (MessageBoxOptions.Yes | MessageBoxOptions.Ok)) != 0;
		}

		public MessageBoxOptions ShowMessageBox(string message, string title, MessageBoxOptions options, MessageBoxType type)
		{
			var vm = IoC.Get<MessageBoxViewModel>();
            vm.DisplayName = title;
			vm.Buttons = options;
			vm.Content = message;

			dynamic settings = new ExpandoObject();
			settings.ResizeMode = System.Windows.ResizeMode.NoResize;
            settings.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
            settings.TopMost = true;
            switch (type)
			{
				case MessageBoxType.Warning:
					settings.Foreground = Brushes.DarkOrange;
					break;
				case MessageBoxType.Error:
					settings.Foreground = Brushes.Red;
                    settings.MaxHeight = 500;
                    break;
			}

			Execute.OnUIThread(() =>
			{
				_windowManager.ShowDialog(vm, settings: settings);
			});
			return vm.ClickedButton;
		}
	}
}
