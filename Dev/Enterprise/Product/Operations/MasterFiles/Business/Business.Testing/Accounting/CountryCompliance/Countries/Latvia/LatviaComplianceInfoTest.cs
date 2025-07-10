using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(LatviaComplianceInfo))]
	sealed class LatviaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Latvia;

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		protected override bool ComplianceDateDependOfIsProductionSystem => true;

		protected override ZDate ExpectedEInvoicingComplianceDate
			=> IsProductionSystem
				? ZDate.Empty
				: new ZDate(2040, 1, 1);

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType
			=> AccTransactionHeaderAuthorisationRecordTypes.Latvia;

		protected override string ExpectedGovernmentAllocatedNumberColumnName
			=> AccTransactionHeader.Schema.AH_GovernmentAllocatedID;
	}
}
