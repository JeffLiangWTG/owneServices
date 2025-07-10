using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.CyprusOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class CyprusComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Cyprus;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.TIC;
		protected override bool? GetIsReciprocal() => false;

		#endregion

		#region Static

		public static string RecipientTaxIdPrefix => OrgCusCodes.TIC;
		public static string RecipientLocalBusinessRegHeading => (NoResString)"Client TIC #";

		#endregion
	}
}
