using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(SerbiaComplianceInfo))]
	sealed class SerbiaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Serbia;

		protected override string ExpectedRecipientTaxIDHeading => "CLIENT PIB #";

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		protected override bool ComplianceDateDependOfIsProductionSystem => true;

		protected override ZDate ExpectedEInvoicingComplianceDate
			=> IsProductionSystem
				? new ZDate(2024, 6, 1)
				: new ZDate(2024, 4, 1);
	}
}
