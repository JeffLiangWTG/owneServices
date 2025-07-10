using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class ManifestToOpenPackProviderTest : TestCaseWithFactory
	{
		public void TestOpeningTransportBillLinesMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				var summaryDeclaration1 = declaration.SummaryDeclaration.ToArray()[0];
				var openingTransportBills1 = summaryDeclaration1.OpeningTransportBills.ToArray()[0];
				var openingTransportBillLines = openingTransportBills1.OpeningTransportBillLines.ToArray();

				CombineAssertions("Opening Transport Bill Lines", () =>
				{
					AssertEquals("First | OpeningTransportBillLinesNo", "1", openingTransportBillLines[0].OpeningTransportBillLinesNo);
					AssertEquals("First | WareHouseCode", "A00003", openingTransportBillLines[0].WareHouseCode);
					AssertEquals("First | OpeningQuantity", 50m, openingTransportBillLines[0].OpeningQuantity);

					AssertEquals("Second | OpeningTransportBillLinesNo", "2", openingTransportBillLines[1].OpeningTransportBillLinesNo);
					AssertEquals("Second | WareHouseCode", "A00004", openingTransportBillLines[1].WareHouseCode);
					AssertEquals("Second | OpeningQuantity", 50m, openingTransportBillLines[1].OpeningQuantity);
				});
			}
		}
	}
}
