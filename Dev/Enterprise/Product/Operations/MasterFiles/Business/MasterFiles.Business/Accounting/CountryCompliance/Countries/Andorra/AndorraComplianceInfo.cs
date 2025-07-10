using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.AndorraOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class AndorraComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Andorra;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.IGI;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.IGI;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.IGI;
		protected override string GetRecipientTaxIDHeading() => (NoResString)"CLIENT NRT #:";
		protected override bool? GetIsReciprocal() => false;
		protected override bool? GetIsRightHandSideAdressCountry() => true;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => false;

		#endregion
	}
}
