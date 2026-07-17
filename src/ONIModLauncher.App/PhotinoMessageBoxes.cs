using ONIModLauncher.Core.Interfaces;
using Photino.NET;

namespace App1.Photino
{
	public class PhotinoMessageBoxes : IMessageBoxes
	{
		private readonly PhotinoWindow _window;
		
		public PhotinoMessageBoxes(PhotinoWindow window)
		{
			_window = window;
		}
		
		public bool? ShowMessageBox(string message, string title, bool yesNo = false, bool showCancel = false, string icon = null)
		{
			PhotinoDialogButtons buttons = PhotinoDialogButtons.Ok;
			if (yesNo)
			{
				buttons = showCancel ? PhotinoDialogButtons.YesNoCancel : PhotinoDialogButtons.YesNo;
			}
			else
			{
				buttons = showCancel ? PhotinoDialogButtons.OkCancel : PhotinoDialogButtons.Ok;
			}
			var result = _window.ShowMessage(title, message, buttons, GetIcon(icon ?? "info"));
			if (showCancel && result == PhotinoDialogResult.Cancel) return null;
			
			if (yesNo)
			{
				return result == PhotinoDialogResult.Yes;
			}
			else
			{
				return result == PhotinoDialogResult.Ok;
			}
		}
		
		private PhotinoDialogIcon GetIcon(string icon)
		{
			switch (icon.ToLowerInvariant())
			{
				case "info":
				default:
					return PhotinoDialogIcon.Info;
				case "warning":
				case "warn":
					return PhotinoDialogIcon.Warning;
				case "error":
				case "err":
					return PhotinoDialogIcon.Error;
				case "question":
					return PhotinoDialogIcon.Question;
			}
		}
	}
}
