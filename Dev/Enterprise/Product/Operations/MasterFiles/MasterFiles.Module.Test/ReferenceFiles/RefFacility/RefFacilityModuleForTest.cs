using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	public class RefFacilityModuleForTest : RefFacilityModule
	{
		public MenuItem[] GetNewStandardMenuItemsForTest() => base.GetNewStandardMenuItems();

		public IFilterControl GetNewFilterControlForTest() => GetNewFilterControl();

		public IBusinessObjectCollection GetNewGridCollectionForTest() => GetNewGridCollection();

		public FilterBusinessObject GetNewFilterBusinessObjectForTest() => GetNewFilterBusinessObject();
	}
}
