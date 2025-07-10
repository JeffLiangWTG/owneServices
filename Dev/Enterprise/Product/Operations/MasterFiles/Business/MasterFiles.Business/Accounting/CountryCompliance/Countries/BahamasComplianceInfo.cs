using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.BahamasOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class BahamasComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Bahamas;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.TIN;
		protected override string GetRecipientLocalBusinessRegNumberCodeType() => OrgCusCodes.TIN;
		protected override string GetRecipientLocalBusinessRegHeading() => (NoResString)"CLIENT TIN #";
		protected override string GetRecipientTaxIDHeading() => (NoResString)"CLIENT VAT #:";
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#endregion
	}
}
