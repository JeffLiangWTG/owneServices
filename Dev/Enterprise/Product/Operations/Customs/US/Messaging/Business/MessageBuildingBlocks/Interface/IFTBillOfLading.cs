using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IFTBillOfLading
	{
		ZString BillOfLadingOrAirWaybill { get; set; }
		ZString HouseBill { get; set; }
		ZDecimal Quantity { get; set; }
		ZString CountryOfExport { get; set; }
		ZString ForeignLoadPort { get; set; }
		ZString FIRMSIdentifier { get; set; }
	}
}
