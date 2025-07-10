using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class BelizeComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Belize;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.GSTCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.GSTCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.GSTCode;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;
		protected override bool? GetIsReciprocal() => true;
		protected override string GetRecipientTaxIDHeading() => (NoResString)"CLIENT TIN #";

		#endregion
	}
}
