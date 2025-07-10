using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.MontenegroOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class MontenegroComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Montenegro;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.PDV;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.PDV;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.PIB;
		protected override string GetRecipientLocalBusinessRegNumberCodeType() => OrgCusCodes.PIB;
		protected override string GetRecipientLocalBusinessRegHeading() => (NoResString)"CLIENT PIB #";
		protected override bool? GetIsReciprocal() => false;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#endregion
	}
}
