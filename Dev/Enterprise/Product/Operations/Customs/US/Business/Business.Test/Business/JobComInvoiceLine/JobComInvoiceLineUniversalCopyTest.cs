using CargoWise.Common;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobComInvoiceLineUniversalCopyTest : Customs.Business.Testing.BaseAddInfoUniversalCopyTest
	{
		protected override void AssertHasOtherNodes(string[] allNodeNames)
		{
			var expectedNodeNames = new[]
			{
				"CustomFields", "FirstAIILine", "ExportATF", "ExportFWS", "AIILines", "FDAs", "DOTs", "FCCs",
				"VehicleLines", "AMSLines", "ATFLines", "LaceyActLines", "FSISLines", "OMCHeaders", "PSTLines",
				"NHTSALines", "CPSCHeaders", "APHISHeaders", "FWSHeaders", "NMFSLines", "TTBLines", "ACE_FDALines",
				"DEAHeaders", "CensusWarningOverrides", "FirstGroupingRange", "LineGroupingRanges", "DrawbackAdditionalExportTariffNumbers",
				"FeeCusCodes", "LicenceAndPermits", "ReconOriginalCharges", "ReconRefundedFees"
			};

			CombineAssertions(() =>
			{
				expectedNodeNames.ForEach(c => AssertCollectionContains($"Should contains {c}.", c, allNodeNames));
				AssertCollectionNotContains("This node has an effect on subclass, for example: DEAHeader, NMFSLine. So it can't be copied", "CusAddInfos", allNodeNames);
			});
		}

		protected override IAddInfoManager GetManager()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			return invoice.JobComInvoiceLines.AddNew();
		}
	}
}
