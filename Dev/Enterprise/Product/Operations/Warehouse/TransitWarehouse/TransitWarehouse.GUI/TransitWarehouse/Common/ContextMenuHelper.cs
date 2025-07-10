using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transit.GUI
{
	public static class ContextMenuHelper
	{
		public static void ShowPackageDetailsForm(ZGrid packageStateGrid, bool checkIsMouseOnAValidRow = false)
		{
			var currentRowObject = (WhsItemPackageState)packageStateGrid.GetCurrent();
			var result = !checkIsMouseOnAValidRow || packageStateGrid.IsMouseOnAValidRow;

			if (result && currentRowObject != null)
			{
				using (var form = new PackageDetailsUserControlDialogForm(currentRowObject.Package))
				{
					form.ShowDialog();
				}
			}
		}

		public static void ShowEDocsForm(ZGrid packageStateGrid)
		{
			var currentRowObject = (WhsItemPackageState)packageStateGrid.GetCurrent();

			if (currentRowObject != null)
			{
				using (var form = new ShowEDocsForm(currentRowObject))
				{
					form.ShowDialog();
				}
			}
		}
	}
}
