using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.GabonOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class GabonComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Gabon;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.NIF;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#endregion

		#region Extra Tax

		protected override bool? HasExtraTaxInfo() => true;
		protected override string GetDescriptionForQCTExtraTaxType() => Res.GetString("513610a8-45b5-4c58-af58-d54e7b831731", "CSS");
		protected override ResourceStringData GetExtraTaxOSAmountCaption() => Res.GetData("c5c7552a-a355-4b54-831c-c720a456993b", "CSS Amt", "CSS Amount", "");
		protected override ResourceStringData GetExtraTaxLocalAmountCaption() => Res.GetData("27b8973e-bad2-444e-ae9f-447f6a3a3d2a", "CSS Local");

		#endregion

		#region Static

		public static string RecipientTaxIdPrefix => OrgCusCodes.NIF;
		public static string RecipientLocalBusinessRegHeading => (NoResString)"Client NIF #";

		#endregion
	}
}
