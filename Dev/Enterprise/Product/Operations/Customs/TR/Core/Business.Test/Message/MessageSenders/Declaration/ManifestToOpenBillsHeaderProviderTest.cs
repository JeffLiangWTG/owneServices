using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class ManifestToOpenBillsHeaderProviderTest : TestCaseWithFactory
	{
		public void TestSummaryDeclarationMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				var summaryDeclaration = declaration.SummaryDeclaration.ToArray();
				AssertEquals("Bills Count", 1, summaryDeclaration[0].OpeningTransportBills.Count);

				CombineAssertions("Summary Declaration with not all", () =>
				{
					AssertEquals("SummaryDeclarationNo", "21067777IM123456", summaryDeclaration[0].SummaryDeclarationNo);
					AssertEquals("SummaryDeclarationProcessContent", "3", summaryDeclaration[0].SummaryDeclarationProcessContent);
					AssertEquals("InWarehouse", "EVET", summaryDeclaration[0].InWarehouse);
					AssertEquals("OtherNature", "EVET", summaryDeclaration[0].OtherNature);

					AssertEquals("Bills Count", 2, summaryDeclaration[1].OpeningTransportBills.Count);
					AssertEquals("SummaryDeclarationNo", "21067777IM123457", summaryDeclaration[1].SummaryDeclarationNo);
					AssertEquals("SummaryDeclarationProcessContent", "3", summaryDeclaration[1].SummaryDeclarationProcessContent);
					AssertEquals("InWarehouse", "EVET", summaryDeclaration[1].InWarehouse);
					AssertEquals("OtherNature", "EVET", summaryDeclaration[1].OtherNature);
				});

				CombineAssertions("Summary Declaration with all", () =>
				{
					var openingTransportBills = summaryDeclaration[2].OpeningTransportBills.ToArray();
					AssertEquals("Bill Lines Count", 0, openingTransportBills[0].OpeningTransportBillLines.Count);
					AssertEquals("SummaryDeclarationNo", "21067777IM123458", summaryDeclaration[2].SummaryDeclarationNo);
					AssertEquals("SummaryDeclarationProcessContent", "2", summaryDeclaration[2].SummaryDeclarationProcessContent);
					AssertEquals("InWarehouse", "EVET", summaryDeclaration[2].InWarehouse);
					AssertEquals("OtherNature", "EVET", summaryDeclaration[2].OtherNature);
				});
			}
		}
	}
}
