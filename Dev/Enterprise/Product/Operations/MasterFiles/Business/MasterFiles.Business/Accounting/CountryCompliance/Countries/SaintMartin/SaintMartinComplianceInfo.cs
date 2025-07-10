using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.SaintMartinOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class SaintMartinComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.SaintMartin;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.TGCA;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.TGC;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.TGC;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;
		protected override string GetRecipientTaxIDHeading() => (NoResString)"CLIENT TGCA #";
		protected override bool? GetIsReciprocal() => false;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
