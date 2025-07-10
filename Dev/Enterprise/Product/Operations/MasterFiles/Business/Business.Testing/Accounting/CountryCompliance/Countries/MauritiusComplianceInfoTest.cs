using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(MauritiusComplianceInfo))]
	sealed class MauritiusComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => CountryCodes.Mauritius;

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		#region EInvoicing

		protected override bool ComplianceDateDependOfIsProductionSystem => true;

		protected override ZDate ExpectedEInvoicingComplianceDate
			=> IsProductionSystem
				? new ZDate(2024, 5, 1)
				: new ZDate(2024, 4, 1);

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType
			=> AccTransactionHeaderAuthorisationRecordTypes.Mauritius;

		protected override string ExpectedGovernmentAllocatedNumberColumnName
			=> AccTransactionHeader.Schema.AH_GovernmentAllocatedID;

		#endregion
	}
}
