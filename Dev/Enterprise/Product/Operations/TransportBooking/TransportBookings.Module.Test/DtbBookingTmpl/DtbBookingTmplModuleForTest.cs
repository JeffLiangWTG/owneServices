using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.Module.Testing
{
	public class DtbBookingTmplModuleForTest : DtbBookingTmplModule
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
	}
}
