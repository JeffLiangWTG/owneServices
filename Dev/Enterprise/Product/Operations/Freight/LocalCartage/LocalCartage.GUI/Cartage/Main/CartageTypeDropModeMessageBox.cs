using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public static class CartageTypeDropModeMessageBox
	{
		public static DropMode Show(CommonCartage cartage, QueryUserCartageTypeDropModeEventArgs e)
		{
			DropMode result;

			using (var dialog = new CartageTypeDropModeDialog(cartage, e.Message, e.AddressButtonText, e.JobTypeButtonText))
			{
				ZFormModaliser.ShowMessageBoxWithoutDispose(dialog);
				result = dialog.Result;
			}

			return result;
		}
	}
}
