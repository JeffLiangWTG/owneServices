using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PGAIndicatorsCalculatorTest : TestCaseWithFactory
	{
		public void TestShouldRemoveIrrelevantIndicator()
		{
			var indicatorCalculator = new PGAIndicatorsCalculator((x) => true);
			foreach (var column in PGAIndicatorColumnsToBeTested)
			{
				AssertEquals(column.Substring(3) + " SHOULD NOT be removed", false, indicatorCalculator.ShouldRemoveIrrelevantIndicator(column));
			}

			indicatorCalculator = new PGAIndicatorsCalculator((x) => false);
			foreach (var column in PGAIndicatorColumnsToBeTested)
			{
				AssertEquals(column.Substring(3) + " SHOULD be removed", true, indicatorCalculator.ShouldRemoveIrrelevantIndicator(column));
			}
		}

		ZString[] PGAIndicatorColumnsToBeTested
		{
			get
			{
				return new ZString[]
				{
					JobComInvoiceLine.Schema.US_ATFInd,
					JobComInvoiceLine.Schema.US_FDAIndicator,
					JobComInvoiceLine.Schema.US_NMFS370Ind,
					JobComInvoiceLine.Schema.US_NMFSAMRInd,
					JobComInvoiceLine.Schema.US_NMFSHMSInd,
					JobComInvoiceLine.Schema.US_NMFSSIMPInd,
					JobComInvoiceLine.Schema.US_VNEInd,
					JobComInvoiceLine.Schema.US_PSTIndicator,
					JobComInvoiceLine.Schema.US_ODSInd,
					JobComInvoiceLine.Schema.US_TSCAInd,
					JobComInvoiceLine.Schema.US_HFCInd,
					JobComInvoiceLine.Schema.US_AMSInd,
					JobComInvoiceLine.Schema.US_NOPInd,
					JobComInvoiceLine.Schema.US_APHISInd,
					JobComInvoiceLine.Schema.US_CPSCInd,
					JobComInvoiceLine.Schema.US_DDTCInd,
					JobComInvoiceLine.Schema.US_DEAInd,
					JobComInvoiceLine.Schema.US_FSISInd,
					JobComInvoiceLine.Schema.US_FWSInd,
					JobComInvoiceLine.Schema.US_LaceyIndicator,
					JobComInvoiceLine.Schema.US_NHTSAIndicator,
					JobComInvoiceLine.Schema.US_OMCInd,
					JobComInvoiceLine.Schema.US_TTBInd
				};
			}
		}
	}
}
