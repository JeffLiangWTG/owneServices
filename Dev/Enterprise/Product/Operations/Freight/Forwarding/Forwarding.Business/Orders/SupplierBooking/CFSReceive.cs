using System.Data;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[UniversalDataContext(DataContextType.CFSReceive)]
	public class CFSReceive : JobSupplierBooking
	{
		public CFSReceive(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
