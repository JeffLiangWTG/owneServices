using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.SurinameOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class SurinameComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Suriname;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.BTW;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.BTW;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.BTW;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;
		protected override string GetRecipientTaxIDHeading() => (NoResString)"CLIENT FIN #";

		#endregion
	}
}
