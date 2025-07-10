using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.MoldovaOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class MoldovaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Moldova;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.NCF;
		protected override string GetRecipientLocalBusinessRegNumberCodeType() => OrgCusCodes.NCF;
		protected override string GetRecipientLocalBusinessRegHeading() => (NoResString)"CLIENT CF #";
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
