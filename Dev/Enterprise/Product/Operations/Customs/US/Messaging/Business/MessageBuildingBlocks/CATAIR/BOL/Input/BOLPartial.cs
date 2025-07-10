using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BillofLadingUpdate)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse)]
	[OutputBlock("L1")]
	public partial class BOLL1 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BillofLadingUpdate)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse)]
	[OutputBlock("L3")]
	public partial class BOLL3 : MessageBlock, IBlock22BillDetails
	{
		ZString IBlock22BillDetails.ITNo { get { return InBondNumber; } }
		ZDate IBlock22BillDetails.ITDate { get { return ZDate.Empty; } }
		ZString IBlock22BillDetails.MasterBillNumber { get { return MasterBillNumber; } }
		ZString IBlock22BillDetails.HouseBillNumber { get { return HouseBillNumber; } }
		ZString IBlock22BillDetails.SubHouseBillNumber { get { return SubHouseBillNumber; } }
		ZInt IBlock22BillDetails.Quantity { get { return ManifestQuantity; } }
		ZString IBlock22BillDetails.Unit { get { return Unit; } }
		ZString IBlock22BillDetails.IssuerCodeOfMasterBillNumber { get { return IssuerCodeOfMasterBillNumber; } }
		ZString IBlock22BillDetails.IssuerCodeOfHouseBillNumber { get { return IssuerCodeOfHouseBillNumber; } }
		ZString IBlock22BillDetails.IssuerCodeOfSubHouseBillNumber { get { return IssuerCodeOfSubHouseBillNumber; } }
	}
}