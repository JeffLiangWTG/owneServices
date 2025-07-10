using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.UzbekistanOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class UzbekistanComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Uzbekistan;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.QQS;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.QQS;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.STR;
		protected override string GetRecipientLocalBusinessRegNumberCodeType() => OrgCusCodes.STR;
		protected override string GetRecipientLocalBusinessRegHeading() => (NoResString)"CLIENT STIR #";
		protected override string GetRecipientTaxIDHeading() => (NoResString)"CLIENT QQS #:";
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
