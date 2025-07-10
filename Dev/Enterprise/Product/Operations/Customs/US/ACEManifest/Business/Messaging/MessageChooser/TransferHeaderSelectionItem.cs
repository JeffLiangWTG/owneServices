using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class TransferHeaderSelectionItem : ISelectionItem
	{
		public TransferHeaderSelectionItem(AsycudaTransferHeader transferHeader)
		{
			PK = ZGuid.NewZGuid();
			TransferHeader = Argument.NotNull(transferHeader, "transferBill");
			DefaultStatusCode();
		}

		public ZGuid PK { get; private set; }
		public AsycudaTransferHeader TransferHeader { get; private set; }
		public ZString StatusCode { get; set; }
		public string SelectionDescription(bool showStatus) => ZString.Empty;

		void DefaultStatusCode()
		{
			if (TransferHeader.TransferBills.Any(x => !x.InBondNumber.IsEmpty))
			{
				StatusCode = AIMArrivalStatusCodes.Codes.InbondArrivedAtDestination;
			}
			else if (TransferHeader.ATF_TransferType == ManifestBase.TransferTypeList.Codes.International)
			{
				StatusCode = AIMArrivalStatusCodes.Codes.InbondExportedAtDestination;
			}
			else
			{
				StatusCode = AIMArrivalStatusCodes.Codes.LocalTransfer;
			}
		}
	}
}
