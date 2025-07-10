using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(UnitedKingdomComplianceInfo))]
	sealed class UnitedKingdomComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.UnitedKingdom;

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		#region IComplianceInfoElectronicInvoicing

		protected override string ExpectedGovernmentAllocatedNumberColumnName => AccTransactionHeader.Schema.AH_GovernmentAllocatedID;

		#endregion
	}
}
