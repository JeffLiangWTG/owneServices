using System.Linq;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ACEOGABlocksCreatorTest : EntrySummaryMessageBuilderAbstractTest
	{
		public void TestOrderAsAdvisedInAdminMessage15_000544()
		{
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;

			invoiceLine.US_FDAIndicator = "D";
			invoiceLine.US_DOTIndicator = "D";

			var fda = invoiceLine.FDAs.AddNew();
			fda.US_FDACommercialDesc = "FDA Description";

			var dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTCommercialDesc = "DOT Description";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entryLine = invoiceLine.CusEntryLine;

			var blocks = new ACEOGABlocksCreator().CreateBlocks(entryLine, true).ToList();

			var indexOfDOT = blocks.FindIndex(x => x is OGADT01);
			var indexOfFDA = blocks.FindIndex(x => x is OGAFD01);

			Assert(indexOfDOT < indexOfFDA);
		}
	}
}
