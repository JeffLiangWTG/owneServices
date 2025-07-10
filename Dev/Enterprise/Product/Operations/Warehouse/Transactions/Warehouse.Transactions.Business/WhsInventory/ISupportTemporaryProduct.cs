using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ISupportTemporaryProduct
	{
		bool IsTemporaryProduct { get; }
		object this[string propertyName] { get; }
		ISupportTemporaryProduct[] GetSiblingsWithTheSameProduct();

		ZString CommodityCode { get; set; }
		ZString ProductUQ { get; set; }
		ZPropertyInfo ProductUQInfo { get; }
		ZPropertyInfo CommodityCodeInfo { get; }
	}
}
