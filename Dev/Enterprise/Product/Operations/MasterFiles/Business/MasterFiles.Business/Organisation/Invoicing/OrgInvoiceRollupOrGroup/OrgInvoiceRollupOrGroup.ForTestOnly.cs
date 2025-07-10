#if DEBUG

using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public partial class OrgInvoiceRollupOrGroup
	{
		public partial class Loader
		{
			internal OrgInvoiceRollupOrGroup LoadForTestOnly(params ZString[] jobTypesInOrderOfPreference)
				=> this.BaseLoadForTestOnly(OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, jobTypesInOrderOfPreference);
		}
	}
}

#endif
