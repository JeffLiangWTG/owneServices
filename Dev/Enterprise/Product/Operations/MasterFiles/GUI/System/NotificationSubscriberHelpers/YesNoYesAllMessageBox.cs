using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public static class YesNoYesAllMessageBox
	{
		public static YesNoYesAllNoAllMessageBoxResult Show(string message, string caption, string yesToAllText)
		{
			YesNoYesAllNoAllMessageBoxResult result;

			using (YesNoYesAllDialog dialog = new YesNoYesAllDialog())
			{
				dialog.SetCaption(caption);
				dialog.SetMessage(message);
				dialog.SetYesToAllText(yesToAllText);

				ZFormModaliser.ShowMessageBoxWithoutDispose(dialog);
				result = dialog.Result;
			}

			return result;
		}
	}
}
