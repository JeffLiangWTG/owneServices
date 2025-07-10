using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class GlbCompanyModuleForTest : GlbCompanyModule
	{
		public IFilterControl GetNewFilterControlForTest()
		{
			return GetNewFilterControl();
		}

		public IBusinessObjectCollection GetNewGridCollectionForTest()
		{
			return GetNewGridCollection();
		}

		public FilterBusinessObject GetNewFilterBusinessObjectForTest()
		{
			return GetNewFilterBusinessObject();
		}

		public MenuItem[] GetNewActionMenuItemsForTest()
		{
			return GetNewActionMenuItems();
		}

		public void SetSelectedBusinessObjects(BusinessObject[] selectedBusinessObjects)
		{
			selectedBusinessObjects_ForTestOnly = selectedBusinessObjects;
		}

		protected override BusinessObject[] SelectedBusinessObjects => selectedBusinessObjects_ForTestOnly ?? base.SelectedBusinessObjects;
		BusinessObject[] selectedBusinessObjects_ForTestOnly;
	}
}
