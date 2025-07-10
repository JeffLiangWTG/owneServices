using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface IUpdateFromShipment
	{
		ZString HouseBillNumber { set; }
		ZString PaymentTerm { set; }
		ZBool IsCoload { set; }
		ZGuid CoLoadMasterShipmentPK { set; }
		ZString CoLoadMasterShipmentHouseBillNumber { set; }
		ZString GoodsDescription { set; }
		ZDecimal GoodsValue { set; }
		ZString GoodsCurrency { set; }
		ZInt OuterPacks { set; }
		ZString UnitOfWeight { set; }
		ZDecimal ActualWeight { set; }
		ZString Destination { set; }
		ZString Origin { set; }
		ZString UniqueConsignRef { set; }
		bool IsAir { get; set; }
		void CopyConsigneeDetails(CommonShipment shipment);
		void CopyConsignorDetails(CommonShipment shipment);
	}
}
