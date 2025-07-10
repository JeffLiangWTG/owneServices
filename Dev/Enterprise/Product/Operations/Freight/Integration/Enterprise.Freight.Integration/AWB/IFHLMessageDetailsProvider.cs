using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Integration.AWB
{
	public interface IFHLMessageDetailsProvider : IFBaseMessageDetailsProvider
	{
		ZString HouseBill { get; }
		ZInt ShippingLoadAndCount { get; }
		ZString ManifestDescriptionOfGoods { get; }
		ZString DetailedGoodsDescription { get; }
		ZString NatureAndQtyOfGoods { get; }
		ZString ParentTable { get; }
		ZGuid ParentID { get; }
		ZString UniqueReference { get; }
		StringCollectionX GetAvailableHarmonisedCodes();
	}
}
