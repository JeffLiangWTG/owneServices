using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class InformationProviderTestClass : InformationProvider
	{
		public InformationProviderTestClass(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZString ExposeGetPartyId(OrgHeader org)
		{
			return GetPartyId(org);
		}
	}
}
