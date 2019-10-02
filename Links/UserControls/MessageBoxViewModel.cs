using Caliburn.Micro;
using Links.Contract.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Links.UserControls
{
	public class MessageBoxViewModel : Screen
	{
		public MessageBoxOptions ClickedButton { get; set; }

		private MessageBoxOptions _buttons;

		public MessageBoxOptions Buttons
		{
			get { return _buttons; }
			set { this.Set(ref _buttons, value); }
		}

		private string _content;

		public string Content
		{
			get { return _content; }
			set { this.Set(ref _content, value); }
		}

		public virtual void Close()
		{
			ClickedButton = MessageBoxOptions.Cancel;
			TryClose(dialogResult: false);
		}

		public virtual void Cancel()
		{
			ClickedButton = MessageBoxOptions.Cancel;
			TryClose(dialogResult: null);
		}

		public virtual void No()
		{
			ClickedButton = MessageBoxOptions.No;
			TryClose(dialogResult: false);
		}

		public virtual void Ok()
		{
			ClickedButton = MessageBoxOptions.Ok;
			TryClose(dialogResult: true);
		}

		public virtual void Yes()
		{
			ClickedButton = MessageBoxOptions.Yes;
			TryClose(dialogResult: true);
		}
	}
}
