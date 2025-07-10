using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class TRIRecordHelper
	{
		public static ZString GetTRIRecord()
		{
			var orgProxy = GlbBranch.CurrentBranch?.OrgProxy != null
				? GlbBranch.CurrentBranch.OrgProxy
				: GlbCompany.CurrentCompany?.OrgProxy;

			return GetTRIRecord(orgProxy);
		}

		public static ZString GetTRIRecord(OrgHeader organisation)
		{
			return organisation?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == OrgCusCode.CodeTypes.BoleroTitleRegisterID)?.OK_CustomsRegNo ?? ZString.Empty;
		}
	}
}
