using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class KyrgyzstanComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Kyrgyzstan;

		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;

		protected override string GetConsumptionTaxRegistrationCode() => KyrgyzstanOrgCusCodeInfo.OrgCusCodes.TIN;
		protected override string GetLocalBusinessRegNoCodeType() => KyrgyzstanOrgCusCodeInfo.OrgCusCodes.TIN;

		protected override bool? GetIsReciprocal() => true;

		protected override string GetRecipientTaxIDHeading() => (NoResString)"CLIENT TIN #";
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#endregion
	}
}
