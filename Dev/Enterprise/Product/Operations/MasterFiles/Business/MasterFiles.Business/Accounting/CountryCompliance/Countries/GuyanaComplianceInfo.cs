using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.GuyanaOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class GuyanaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo Implementation

		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.TIN;
		protected override bool? GetIsReciprocal() => true;

		#endregion

		public override ZString CountryCode => Core.Constants.CountryCodes.Guyana;
		public static string RecipientTaxIdPrefix => OrgCusCodes.TIN;
		public static string RecipientLocalBusinessRegHeading => (NoResString)"CLIENT TIN #";
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;
	}
}
