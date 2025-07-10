using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class OpeningBillofLadingsProviderTest : TestCaseWithFactory
	{
		public void TestOpeningBillofLadingsMembers()
		{
			using (var helper = new ProviderTestHelper(Factory))
			{
				var header = helper.GetProviderHeader();
				var sumDec = new ManifestMessageProvider(header);
				var openingSumDec = sumDec.OpeningSummaryDeclaration.ToArray();
				var openingBill1 = openingSumDec[1].OpeningBillofLadings.ToArray();
				var openingBill2 = openingSumDec[2].OpeningBillofLadings.ToArray();
				CombineAssertions("OpeningBillofLadings Provider Test", () =>
				{
					AssertEquals("OpenedBillNumber", "111111111", openingBill1[0].OpenedBillNumber);
					AssertEquals("OpenedBillNumber", "222222222", openingBill2[0].OpenedBillNumber);
				});
			}
		}
	}
}
