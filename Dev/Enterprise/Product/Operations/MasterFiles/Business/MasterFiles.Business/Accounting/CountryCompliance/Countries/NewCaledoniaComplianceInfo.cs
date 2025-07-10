using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class NewCaledoniaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.NewCaledonia;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.NewCaledoniaCodeTypes.TGC;
		protected override string GetConsumptionTaxCode() => OrgCusCode.NewCaledoniaCodeTypes.TGC;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.NewCaledoniaCodeTypes.TGC;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
