using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class CoteDivoireComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.CoteDivoire;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CoteDivoireCodeTypes.NCC;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CoteDivoireCodeTypes.NCC;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
