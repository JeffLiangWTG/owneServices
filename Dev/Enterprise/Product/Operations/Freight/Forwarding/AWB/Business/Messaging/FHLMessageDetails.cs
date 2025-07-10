using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Integration.AWB;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	public class FHLMessageDetails : FBaseMessageDetails, IFHLMessageDetailsProvider
	{
		public FHLMessageDetails(ExportAWBHeader parent)
			: base(parent)
		{
		}

		#region IFHLMessageDetailsProvider Members

		public ZString HouseBill => parent.HouseBill;

		public ZInt ShippingLoadAndCount => parent.EH_ShippingLoadAndCount;

		public ZString ManifestDescriptionOfGoods => parent.EH_ManifestDescriptionOfGoods;

		public ZString DetailedGoodsDescription => parent.DetailedGoodsDescription;

		public ZString NatureAndQtyOfGoods => parent.NatureAndQtyOfGoods;

		public StringCollectionX GetAvailableHarmonisedCodes() => parent.GetAvailableHarmonisedCodes();

		public ZString ParentTable => ZString.Empty;

		public ZGuid ParentID => ZGuid.Empty;

		public ZString UniqueReference => parent.UniqueReference;

		#endregion
	}
}
