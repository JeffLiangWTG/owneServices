using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMTransfer : IAIMTransfer
	{
		public AIMTransfer(AsycudaTransferBill transferBill)
		{
			transferHeader = transferBill?.TransferHeader;
			if (transferHeader != null)
			{
				bondedCarrierIDOrOnwardCarrier = !transferHeader.ATF_CarrierID.IsEmpty ? transferHeader.ATF_CarrierID : transferHeader.ATF_OnwardCarrier;
				bondedPremisesIdentifierOrInbondControlNumber = !transferBill.InBondNumber.IsEmpty ? transferBill.InBondNumber : transferHeader.ATF_DestinationWarehouseID;
			}
		}
		readonly AsycudaTransferHeader transferHeader;
		readonly ZString bondedCarrierIDOrOnwardCarrier;
		readonly ZString bondedPremisesIdentifierOrInbondControlNumber;

		public ZString DestinationAirport => transferHeader?.ATF_RL_NKDestinationPortCode.Right(3) ?? "000";

		public ZString DomesticInternationalIdentifier => transferHeader?.ATF_TransferType ?? "";

		public ZString BondedCarrierIDOrOnwardCarrier => bondedCarrierIDOrOnwardCarrier;

		public ZString BondedPremisesIdentifierOrInbondControlNumber => bondedPremisesIdentifierOrInbondControlNumber;
	}
}
