using CargoWise.Types;
using static Enterprise.MasterFiles.Business.MicronesiaOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class MicronesiaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Micronesia;

		protected override string GetConsumptionTaxCode() => string.Empty;

		protected override string GetConsumptionTaxRegistrationCode() => string.Empty;

		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.EIN;

		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
