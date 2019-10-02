using System.Drawing;
using System.Windows.Forms;

namespace Links.Services
{
	public static class ScreenExtension
	{
		#region Methods

		public static string GetDisplayName(this Screen screen)
		{
			string name = screen.DeviceName.TrimStart('\\', '.');

			return $"{name} (X:{screen.Bounds.Left}; Y:{screen.Bounds.Top})" + (screen.Primary ? " <Pr>" : "");
		}

		#endregion Methods
	}
}