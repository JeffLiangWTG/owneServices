using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.VanuatuOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class VanuatuComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Vanuatu;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.VATCode;
		protected override bool? GetIsReciprocal() => true;

		public static string RecipientTaxIdPrefix => OrgCusCodes.TIN;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;
		protected override string GetRecipientTaxIDHeading() => (NoResString)"CLIENT TIN #:";

		#endregion

	}
}
