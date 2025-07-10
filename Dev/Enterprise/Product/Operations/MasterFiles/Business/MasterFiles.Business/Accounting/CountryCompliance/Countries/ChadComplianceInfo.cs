using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class ChadComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Chad;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.ChadCodeTypes.NIF;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.ChadCodeTypes.NIF;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
