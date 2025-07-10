using System;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class LiberiaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Liberia;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.GSTCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.GSTCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.GSTCode;
		protected override bool? GetIsReciprocal() => throw new NotSupportedException("Must decide whether the country is reciprocal or not");

		#endregion
	}
}
