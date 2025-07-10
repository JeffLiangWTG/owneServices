using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class AccBankAccountModuleForTest : AccBankAccountModule
	{
		public IFilterControl GetNewFilterControlForTest()
		{
			return GetNewFilterControl();
		}

		public BusinessObjectCollection GetNewGridCollectionForTest()
		{
			return (BusinessObjectCollection)GetNewGridCollection();
		}

		public FilterBusinessObject GetNewFilterBusinessObjectForTest()
		{
			return GetNewFilterBusinessObject();
		}

		public MenuItem[] GetNewStandardMenuItemsForTest()
		{
			return GetNewStandardMenuItems();
		}
	}
}
