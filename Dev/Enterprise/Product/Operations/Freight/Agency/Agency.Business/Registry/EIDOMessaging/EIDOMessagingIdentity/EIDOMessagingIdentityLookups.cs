using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class EIDOMessagingIdentityLookups : ZLookups
	{
		public EIDOMessagingIdentityLookups(EIDOMessagingIdentity parent)
			: base(parent) { }

		public ShipsAgencyPrincipalCollection Principals
		{
			get { return new ShipsAgencyPrincipalCollection(Factory); }
		}
	}
}


