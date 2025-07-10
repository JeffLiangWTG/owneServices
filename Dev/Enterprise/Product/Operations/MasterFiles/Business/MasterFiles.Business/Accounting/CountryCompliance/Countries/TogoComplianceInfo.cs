using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class TogoComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Togo;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.TogoCodeTypes.NIF;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.TogoCodeTypes.NIF;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
