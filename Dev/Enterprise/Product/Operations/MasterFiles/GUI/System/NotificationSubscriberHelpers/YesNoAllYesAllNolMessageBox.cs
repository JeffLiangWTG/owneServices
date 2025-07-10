using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public static class YesNoAllYesAllNoAllMessageBox
	{
		public static YesNoYesAllNoAllMessageBoxResult Show(string message, string caption)
		{
			YesNoYesAllNoAllMessageBoxResult result;

			ZFormStrategy.AddFormTypeThatCanBeCreatedDuringDbTransaction(typeof(YesNoAllYesAllNoAllDialog));
			using (var dialog = new YesNoAllYesAllNoAllDialog())
			{
				dialog.SetCaption(caption);
				dialog.SetMessage(message);

				ZFormModaliser.ShowMessageBoxWithoutDispose(dialog);
				result = dialog.Result;
			}

			return result;
		}
	}

	public enum YesNoYesAllNoAllMessageBoxResult
	{
		None,
		Yes,
		No,
		YesToAll,
		NoToAll
	}
}
