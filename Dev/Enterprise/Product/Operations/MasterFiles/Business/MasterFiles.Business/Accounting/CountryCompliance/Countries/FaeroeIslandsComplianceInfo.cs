using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.FaeroeIslandsOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class FaeroeIslandsComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.FaeroeIslands;
		protected override string GetConsumptionTaxCode() => "MVG";
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.MVG;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.MVG;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;
		protected override string GetRecipientTaxIDHeading() => (NoResString)"CLIENT MVG #";

		#endregion
	}
}
