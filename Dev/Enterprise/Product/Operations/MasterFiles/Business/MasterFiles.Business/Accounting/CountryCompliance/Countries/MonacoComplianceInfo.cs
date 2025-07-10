using System;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class MonacoComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Monaco;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.TVACode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.TVACode;
		protected override bool? GetIsReciprocal() => throw new NotSupportedException("Must decide whether the country is reciprocal or not");

		#endregion
	}
}
