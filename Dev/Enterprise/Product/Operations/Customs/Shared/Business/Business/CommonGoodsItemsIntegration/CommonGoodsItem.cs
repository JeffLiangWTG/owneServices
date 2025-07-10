using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.Business.CommonGoodsItemsIntegration
{
	public class CommonGoodsItem : ICommonGoodsItem
	{
		public ZString GoodsDescription { get; set; }
		public ZDecimal GrossMass { get; set; }
		public ZDecimal NetMass { get; set; }
		public ZString GrossMassUnit { get; set; }
		public ZString NetMassUnit { get; set; }
		public ZString CommodityCode { get; set; }
		public ZString DispatchCountry { get; set; }
		public ZString DestinationCountry { get; set; }
		public ZDecimal Value { get; set; }
		public ZString JobReference { get; set; }
		public (ZString Class, ZString Type, ZString Reference, ZInt? EntryLineNumber) EntryNumber { get; set; }
		public ZDecimal SupplementaryQuantity { get; set; }
		public ZString SupplementaryQuantityUnit { get; set; }
		public IEnumerable<ICommonPackage> Packages { get; set; } = new List<ICommonPackage>();
	}
}
