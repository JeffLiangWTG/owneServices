using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class DemocraticRepublicOfCongoComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.DemocraticRepublicOfCongo;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.CompanyNumber;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
