using System;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class ArubaComplianceInfo : CountryComplianceInfo
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Aruba;

		protected override string GetConsumptionTaxCode() => String.Empty;

		protected override string GetConsumptionTaxRegistrationCode() => String.Empty;

		protected override bool? GetIsReciprocal() => true;

		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.CorporationCode;

		#endregion
	}
}
