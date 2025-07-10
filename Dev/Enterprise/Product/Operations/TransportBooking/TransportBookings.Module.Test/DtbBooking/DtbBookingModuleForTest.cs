using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.Module.Testing
{
	public class DtbBookingModuleForTest : DtbBookingModule
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

		public MenuItem[] GetNewStandardMenuItemsForTest()
		{
			return GetNewStandardMenuItems();
		}

		public void PerformSearchForTest()
		{
			PerformSearch();
		}

		public BusinessObjectFactory FactoryForTest
		{
			get { return Factory; }
		}
	}
}
