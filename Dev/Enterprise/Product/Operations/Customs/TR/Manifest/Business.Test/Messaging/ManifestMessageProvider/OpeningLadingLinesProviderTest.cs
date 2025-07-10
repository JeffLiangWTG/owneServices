using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class OpeningLadingLinesProviderTest : TestCaseWithFactory
	{
		public void TestOpeningLadingLinesMembers()
		{
			using (var helper = new ProviderTestHelper(Factory))
			{
				var header = helper.GetProviderHeader();
				var sumDec = new ManifestMessageProvider(header);
				var openingSumDec = sumDec.OpeningSummaryDeclaration.ToArray();
				var openingBill = openingSumDec[2].OpeningBillofLadings.ToArray();
				var openingLines = openingBill[0].OpeningLadingLines.FirstOrDefault();
				CombineAssertions("OpeningLadingLines Provider Test", () =>
				{
					AssertEquals("OpeningLineNumber", 1, openingLines.OpeningLineNumber);
					AssertEquals("AmountTtoOpen", 200m, openingLines.AmountTtoOpen);
					AssertEquals("WarehouseCode", "123456", openingLines.WarehouseCode);
				});
			}
		}
	}
}
