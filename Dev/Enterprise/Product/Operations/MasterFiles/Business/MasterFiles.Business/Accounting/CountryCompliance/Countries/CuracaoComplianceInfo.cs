using CargoWise.Types;
using static Enterprise.MasterFiles.Business.CuracaoOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class CuracaoComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Curacao;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.CRB;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.OB;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.CRB;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion
	}
}
