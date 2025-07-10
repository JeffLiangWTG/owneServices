using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class GibraltarComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Gibraltar;

		protected override string GetConsumptionTaxCode() => string.Empty;

		protected override string GetConsumptionTaxRegistrationCode() => string.Empty;

		protected override string GetLocalBusinessRegNoCodeType() => GibraltarOrgCusCodeInfo.OrgCusCodes.TIN;

		protected override bool? GetIsReciprocal() => false;

		#endregion
	}
}
