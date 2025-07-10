using System;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.US.ACEManifest.Business.Messaging
{
	public class FSNMessageSender
	{
		public FSNMessageSender(AsycudaTransferBill transferBill, ZString statusCode)
		{
			this.transferBill = transferBill;
			this.statusCode = statusCode;
		}

		readonly AsycudaTransferBill transferBill;
		readonly ZString statusCode;

		public Action SendMessage()
		{
			var builder = new AIMMessageBuilder(new ArrivalMessageHeader(transferBill, statusCode), transferBill.Factory);
			var message = builder.Build();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.USAMA;
			var bill = transferBill.Bill;
			if (bill.IsChildMasterBill)
			{
				message.EM_LinkedObject = bill.Header;
			}
			else
			{
				message.EM_LinkedObject = bill;
			}
			transferBill.ATB_MessageStatus = US.AIM.Messaging.AIMTransferStatusCodes.Codes.ArrivalSent;

			return () =>
			{
				message.Delete();
				transferBill.Reload();
				transferBill.TransferHeader.ArrivalHeader.Header.Reload();
			};
		}
	}
}
