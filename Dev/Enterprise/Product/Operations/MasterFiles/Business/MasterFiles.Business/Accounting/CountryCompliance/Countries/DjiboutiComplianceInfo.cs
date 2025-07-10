using CargoWise.Types;
using static Enterprise.MasterFiles.Business.DjiboutiOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class DjiboutiComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Djibouti;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.NIF;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.NIF;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
