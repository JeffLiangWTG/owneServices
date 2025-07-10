using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class RefComplianceListModuleForTest : RefComplianceListModule
	{
		public IFilterControl GetNewFilterControlForTest() => GetNewFilterControl();

		public IBusinessObjectCollection GetNewGridCollectionForTest() => GetNewGridCollection();

		public FilterBusinessObject GetNewFilterBusinessObjectForTest() => GetNewFilterBusinessObject();

		public ZDisplayGrid Grid_Exposed
		{
			get => Grid;
		}

		public void OnIncludeExclude_Exposed(bool exclude)
		{
			IncludeExclude(exclude);
		}

		public MenuItem[] GetNewActionMenuItemsForTest() => GetNewActionMenuItems();

		public MenuItem IncludeComplianceListMenuItem_Exposed => IncludeComplianceListMenuItem;

		public MenuItem ExcludeComplianceListMenuItem_Exposed => ExcludeComplianceListMenuItem;
	}
}
