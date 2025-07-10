using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbAddressPointLookups : ZLookups
	{
		public DtbAddressPointLookups(DtbAddressPoint parent)
			: base(parent)
		{
		}

		public OrgAddressCollection Addresses
		{
			get { return Factory.GetCachedValue("DtbAddressPointLookups|Addresses", () => new OrgAddressCollection(Factory)); }
		}
	}
}
