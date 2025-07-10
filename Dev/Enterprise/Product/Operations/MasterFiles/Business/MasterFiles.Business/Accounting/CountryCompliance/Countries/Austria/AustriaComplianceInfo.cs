using CargoWise.Types;
using static Enterprise.MasterFiles.Business.AustriaOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class AustriaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Austria;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.UID;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.MST;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.CorporationCode;
		protected override bool? GetIsReciprocal() => false;

		#endregion
	}
}
