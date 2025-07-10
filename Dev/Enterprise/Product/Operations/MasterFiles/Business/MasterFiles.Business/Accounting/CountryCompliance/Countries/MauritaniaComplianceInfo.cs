using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.MauritaniaOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class MauritaniaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.NIF;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#endregion

		public override ZString CountryCode => Core.Constants.CountryCodes.Mauritania;
		public static string RecipientTaxIdPrefix => OrgCusCodes.NIF;
		public static string RecipientLocalBusinessRegHeading => (NoResString)"CLIENT NIF #";
	}
}
