using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.PakistanOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class PakistanComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Pakistan;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.NTN;
		protected override bool? GetIsReciprocal() => true;

		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#endregion

		#region Static

		public static string RecipientTaxIdPrefix => Res.GetString("d616ccf4-4662-4f11-93ea-7a3a64d0b6a2", "STRN");
		public static string RecipientTaxIDHeading => Res.GetString("9b5d1b13-a1fd-4c26-b552-e4488a796696", "Client STRN #:");
		public static string RecipientLocalBusinessRegHeading => (NoResString)"CLIENT NTN #";

		#endregion
	}
}
