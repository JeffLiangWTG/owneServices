using CargoWise.Types;
using static Enterprise.MasterFiles.Business.BeninOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class BeninComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Benin;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.IFU;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.IFU;
		protected override bool? GetIsReciprocal() => true;

		#endregion
	}
}
