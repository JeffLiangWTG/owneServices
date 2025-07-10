using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.Business.CommonGoodsItemsIntegration
{
	public interface ICommonGoodsItem
	{
		ZString GoodsDescription { get; set; }
		ZDecimal GrossMass { get; set; }
		ZDecimal NetMass { get; set; }
		ZString GrossMassUnit { get; set; }
		ZString NetMassUnit { get; set; }
		ZString CommodityCode { get; set; }
		ZString DispatchCountry { get; set; }
		ZString DestinationCountry { get; set; }
		ZDecimal Value { get; set; }
		ZString JobReference { get; set; }
		(ZString Class, ZString Type, ZString Reference, ZInt? EntryLineNumber) EntryNumber { get; set; }
		ZDecimal SupplementaryQuantity { get; set; }
		ZString SupplementaryQuantityUnit { get; set; }
		IEnumerable<ICommonPackage> Packages { get; set; }
	}
}
