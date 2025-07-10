using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IBlock22BillDetails
	{
		ZString ITNo { get; }
		ZDate ITDate { get; }
		ZString MasterBillNumber { get; }
		ZString HouseBillNumber { get; }
		ZString SubHouseBillNumber { get; }
		ZInt Quantity { get; }
		ZString Unit { get; }
		ZString IssuerCodeOfMasterBillNumber { get; }
		ZString IssuerCodeOfHouseBillNumber { get; }
		ZString IssuerCodeOfSubHouseBillNumber { get; }
	}
}
