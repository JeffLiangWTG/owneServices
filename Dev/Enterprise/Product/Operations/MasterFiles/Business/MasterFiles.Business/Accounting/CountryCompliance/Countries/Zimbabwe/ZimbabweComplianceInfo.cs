using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.ZimbabweOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class ZimbabweComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Zimbabwe;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetRecipientLocalBusinessRegNumberCodeType() => OrgCusCodes.TIN;
		protected override string GetRecipientLocalBusinessRegHeading() => (NoResString)"CLIENT TIN #";
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetRecipientTaxIDHeading() => (NoResString)"CLIENT VAT #:";
		protected override bool? GetIsReciprocal() => false;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#endregion
	}
}
