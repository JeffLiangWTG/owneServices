using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.AlbaniaOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class AlbaniaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Albania;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.NIT;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.TVSH;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.NIT;
		protected override string GetRecipientTaxIDHeading() => (NoResString)"CLIENT NIPT #:";
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
