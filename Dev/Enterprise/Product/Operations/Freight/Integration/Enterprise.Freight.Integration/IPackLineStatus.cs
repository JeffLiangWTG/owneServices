using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface IPackLineStatusProvider
	{
		IPackLineStatus GetPackLineStatus(BusinessObjectFactory factory, ZString countryCode);
	}

	public interface IPackLineStatus
	{
		ZString GetCustomsStatusDescription(ZString oceanBill, ZString houseBill, ZString containerNumber);
	}
}
