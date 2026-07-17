namespace ONIModLauncher.Core.Interfaces
{
	public interface IMessageBoxes
	{
		bool? ShowMessageBox(string message, string title, bool yesNo = false, bool showCancel = false, string icon = null);
	}
}
