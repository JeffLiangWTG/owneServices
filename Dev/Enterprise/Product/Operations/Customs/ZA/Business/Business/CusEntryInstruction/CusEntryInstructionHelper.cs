using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	public static class CusEntryInstructionHelper
	{
		public static ZString GetRegNoWithOrganisationAddress(this OrgAddress checkingAddress, string codeTypeToCheck)
		{
			var result = ZString.Empty;

			if (checkingAddress != null)
			{
				var orgRegNo = checkingAddress.CustomsCodes?.GetCustomsRegNo(codeTypeToCheck, Core.Constants.CountryCodes.SouthAfrica);
				result = orgRegNo ?? ZString.Empty;
			}
			return result;
		}
	}
}
