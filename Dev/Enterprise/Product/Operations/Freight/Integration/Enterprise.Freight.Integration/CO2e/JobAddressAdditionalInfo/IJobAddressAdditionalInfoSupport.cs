using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface IJobAddressAdditionalInfoSupport
	{
		IJobAddressAdditionalInfoCollection JobAddressAdditionalInfoCollection { get; }
		ZGuid JobAddressAdditionalInfoParentID { get; }
		ZString JobAddressAdditionalInfoTableCode { get; }
		ZPropertyInfo GetDependentAddress(string addressType);
	}
}
