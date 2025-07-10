using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.BonaireSintEustatiusAndSabaOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class BonaireSintEustatiusAndSabaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.BonaireSintEustatiusAndSaba;

		protected override string GetConsumptionTaxCode() => OrgCusCodes.ABB;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.CRB;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.CRB;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;
		protected override string GetRecipientTaxIDHeading() => (NoResString)"CLIENT CRIB #";

		#endregion
	}
}
