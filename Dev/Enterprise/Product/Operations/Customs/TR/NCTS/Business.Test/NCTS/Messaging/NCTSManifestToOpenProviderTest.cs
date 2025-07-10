using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	public class NCTSManifestToOpenProviderTest : TestCaseWithFactory
	{
		public void TestManifestToOpenMembers()
		{
			using (var helper = new NCTSMessageProviderTestHelper(Factory))
			{
				var header = helper.GetProviderNCTSHeader();
				var nctsHeaderProvider = new NCTSHeaderProvider(header);

				var manifestToOpen = nctsHeaderProvider.ManifestToOpen.ToArray();

				CombineAssertions("ManifestToOpen members for level 1", () =>
				{
					AssertEquals("OpeningStyle", "2", manifestToOpen[0].OpeningStyle);
					AssertEquals("SummaryDeclarationNo", "Manifest Dec.No", manifestToOpen[0].SummaryDeclarationNo);
					AssertEquals("AtWarehouse", "1", manifestToOpen[0].AtWarehouse);
					AssertEquals("BillOfLadingNo", "Bill No", manifestToOpen[0].BillOfLadingNo);
					AssertEquals("WarehouseCode", "A0001", manifestToOpen[0].WarehouseCode);
					AssertEquals("BillOfLadingLineNo", 1, manifestToOpen[0].BillOfLadingLineNo);
					AssertEquals("PackageCountForOpening", 200, manifestToOpen[0].PackageCountForOpening);
				});

				CombineAssertions("ManifestToOpen members for level 2", () =>
				{
					AssertEquals("OpeningStyle", "3", manifestToOpen[1].OpeningStyle);
					AssertEquals("SummaryDeclarationNo", "Manifest Dec.No 2", manifestToOpen[1].SummaryDeclarationNo);
					AssertEquals("AtWarehouse", "0", manifestToOpen[1].AtWarehouse);
					AssertEquals("BillOfLadingNo", "Bill No 2", manifestToOpen[1].BillOfLadingNo);
					AssertEquals("WarehouseCode", "A0002", manifestToOpen[1].WarehouseCode);
					AssertEquals("BillOfLadingLineNo", 2, manifestToOpen[1].BillOfLadingLineNo);
					AssertEquals("PackageCountForOpening", 400, manifestToOpen[1].PackageCountForOpening);
				});
			}
		}
	}
}
