using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public static class GlbCompanyExtension
	{
		public static ZString GetSenderIDFromOrgProxy(this GlbCompany company, bool isAir)
		{
			var result = ZString.Empty;
			var orgProxy = company.OrgProxy;
			if (orgProxy != null)
			{
				var codeType = isAir ? OrgCusCode.CodeTypes.ControlledPremisesID : OrgCusCode.CodeTypes.CarrierCode;
				result = orgProxy.CustomsCodes.GetCustomsRegNo(codeType, Core.Constants.CountryCodes.UnitedStates);
			}

			return result;
		}
	}
}
