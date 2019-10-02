using Caliburn.Micro;
using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace Links.Contract.Services
{
	public enum MessageBoxType
	{
		Info,
		Warning,
		Error
	}

    [Flags]
    public enum MessageBoxOptions
    {
        Ok = 2,
        Cancel = 4,
        Yes = 8,
        No = 16,

        OkCancel = Ok | Cancel,
        YesNo = Yes | No,
        YesNoCancel = Yes | No | Cancel
    }

    public interface IDialogManager
	{
		#region Methods

		bool ShowDialog(IScreen dialogModel);

		bool ShowErrorMessageBox(string message, string title = null, MessageBoxOptions options = MessageBoxOptions.Ok);

        bool ShowDialog(IScreen dialogModel, ExpandoObject settings);

        bool ShowMessageBox(string message, string title, MessageBoxOptions options = MessageBoxOptions.Ok);

		MessageBoxOptions ShowMessageBox(string message, string title, MessageBoxOptions options, MessageBoxType type);

		bool ShowWarnMessageBox(string message, string title, MessageBoxOptions options = MessageBoxOptions.Ok);

		#endregion Methods
	}
}