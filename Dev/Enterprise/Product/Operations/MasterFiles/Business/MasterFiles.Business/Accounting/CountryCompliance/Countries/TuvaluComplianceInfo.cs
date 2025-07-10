using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.TuvaluOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class TuvaluComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Tuvalu;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.TCT;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.TCT;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.TCT;
		protected override bool? GetIsReciprocal() => false;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;
		protected override string GetRecipientTaxIDHeading() => (NoResString)"CLIENT TIN #";

		#endregion
	}
}
