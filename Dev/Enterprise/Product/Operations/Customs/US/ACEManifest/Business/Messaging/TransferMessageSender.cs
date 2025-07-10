using System;
using Enterprise.Customs.US.AIM.Messaging;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class TransferMessageSender
	{
		public TransferMessageSender()
		{
		}

		public Action SendMessage(AsycudaTransferBill transferBill, bool sendCancel = false)
		{
			var bill = transferBill.Bill;
			var header = bill.Header;
			var messageHeader = new AIMMessageHeaderForTransfer(transferBill, header.IsExpressCourier ? AIMMessageSubTypes.FXC : AIMMessageSubTypes.FRC, sendCancel);
			var builder = new AIMMessageBuilder(messageHeader, transferBill.Factory);
			var message = builder.Build();

			if (bill.IsChildMasterBill)
			{
				message.EM_LinkedObject = header;
			}
			else
			{
				message.EM_LinkedObject = bill;
			}

			transferBill.ATB_MessageStatus = sendCancel ? AIMTransferStatusCodes.Codes.TransferCancelled : AIMTransferStatusCodes.Codes.TransferSent;
			return () =>
			{
				message.Delete();
				transferBill.Reload();
				transferBill.TransferHeader.ArrivalHeader.Header.Reload();
			};
		}
	}
}
