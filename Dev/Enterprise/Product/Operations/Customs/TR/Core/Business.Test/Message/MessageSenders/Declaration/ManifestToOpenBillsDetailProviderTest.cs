using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class ManifestToOpenBillsDetailProviderTest : TestCaseWithFactory
	{
		public void TestOpeningTransportBillsMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				var summaryDeclaration1 = declaration.SummaryDeclaration.ToArray()[0];
				var openingTransportBills = summaryDeclaration1.OpeningTransportBills.ToArray();

				CombineAssertions("Opening Transport Bills case group", () =>
				{
					AssertEquals("1.Bill | OpeningTransportBillNo", "TS1", openingTransportBills[0].OpeningTransportBillNo);
					AssertEquals("1.Bill | OpeningTransportBillLines", 2, openingTransportBills[0].OpeningTransportBillLines.Count);
				});

				var summaryDeclaration2 = declaration.SummaryDeclaration.ToArray()[1];
				var openingTransportBills2 = summaryDeclaration2.OpeningTransportBills.ToArray();

				CombineAssertions("Opening Transport Bills case non group", () =>
				{
					AssertEquals("1.Bill | OpeningTransportBillNo", "TS2", openingTransportBills2[0].OpeningTransportBillNo);
					AssertEquals("1.Bill | OpeningTransportBillLines", 3, openingTransportBills2[0].OpeningTransportBillLines.Count);

					AssertEquals("2.Bill | OpeningTransportBillNo", "TS2-2", openingTransportBills2[1].OpeningTransportBillNo);
					AssertEquals("2.Bill | OpeningTransportBillLines", 1, openingTransportBills2[1].OpeningTransportBillLines.Count);
				});
			}
		}
	}
}
