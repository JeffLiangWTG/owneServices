using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.BurkinaFasoOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class BurkinaFasoComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.BurkinaFaso;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.IFU;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.IFU;
		protected override string GetRecipientLocalBusinessRegNumberCodeType() => OrgCusCodes.RCM;
		protected override string GetRecipientTaxIDHeading() => (NoResString)"CLIENT IFU #";
		protected override string GetRecipientLocalBusinessRegHeading() => (NoResString)"CLIENT RCCM #";
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetIsRightHandSideAdressCountry() => true;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#endregion
	}
}
