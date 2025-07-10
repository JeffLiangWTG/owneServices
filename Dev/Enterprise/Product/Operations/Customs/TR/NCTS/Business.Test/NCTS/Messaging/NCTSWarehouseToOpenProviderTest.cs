using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	public class NCTSWarehouseToOpenProviderTest : TestCaseWithFactory
	{
		public void TestWarehouseToOpenMembers()
		{
			using (var helper = new NCTSMessageProviderTestHelper(Factory))
			{
				var header = helper.GetProviderNCTSHeader();
				var nctsHeaderProvider = new NCTSHeaderProvider(header);

				var warehouseToOpen = nctsHeaderProvider.WarehouseToOpen.FirstOrDefault();

				CombineAssertions("WarehouseToOpen members", () =>
				{
					AssertEquals("DeclarationNo", "22068888AN123456", warehouseToOpen.DeclarationNo);
					AssertEquals("DeclarationItemNo", 1, warehouseToOpen.DeclarationItemNo);
					AssertEquals("ItemNoOfGoodsItems", 1, warehouseToOpen.ItemNoOfGoodsItems);
					AssertEquals("Quantity", 20m, warehouseToOpen.Quantity);
					AssertEquals("Explanation", "Explanation", warehouseToOpen.Explanation);
					AssertEquals("Value", 100m, warehouseToOpen.Value);
					AssertEquals("CurrencyCode", "TRY", warehouseToOpen.CurrencyCode);
					AssertEquals("IncotermCode", "FOB", warehouseToOpen.IncotermCode);
					AssertEquals("MethodOfPaymentCode", "1", warehouseToOpen.MethodOfPaymentCode);
					AssertEquals("ProcedureCode", "11", warehouseToOpen.ProcedureCode);
					AssertEquals("CountryCode", "052", warehouseToOpen.CountryCode);
				});
			}
		}
	}
}
