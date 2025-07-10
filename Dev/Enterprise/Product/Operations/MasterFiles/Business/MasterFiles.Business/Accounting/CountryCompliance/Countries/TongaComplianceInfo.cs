using CargoWise.Types;
using static Enterprise.MasterFiles.Business.TongaOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class TongaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Tonga;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.TongaCodeTypes.TIN;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.CT;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.CorporationCode;
		protected override bool? GetIsReciprocal() => false;

		#endregion
	}
}
