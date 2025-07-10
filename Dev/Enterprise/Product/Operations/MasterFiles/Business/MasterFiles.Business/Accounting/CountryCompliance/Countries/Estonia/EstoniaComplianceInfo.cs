using CargoWise.Types;
using static Enterprise.MasterFiles.Business.EstoniaOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	class EstoniaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Estonia;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.KM;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.BusinessRegistrationNumber;
		protected override bool? GetIsReciprocal() => false;

		#endregion
	}
}
