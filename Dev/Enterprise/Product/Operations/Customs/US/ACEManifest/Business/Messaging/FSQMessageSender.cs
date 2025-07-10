using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.ACEManifest.Business.Messaging
{
	public class FSQMessageSender
	{
		public FSQMessageSender(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			this.header = header as AsycudaManifestHeader;
		}

		readonly AsycudaManifestHeader header;

		public void SendMessage(SplitBillSelectionItem[] items, ZString freightStatusCode)
		{
			foreach (var item in items)
			{
				var bill = item.Bill;
				var message = GenerateMessage(item.Bill, item.Arrival, freightStatusCode);
				if (bill.IsChildMasterBill)
				{
					header.Messages.Add(message);
					message.EM_LinkedObject = header;
				}
				else
				{
					bill.Messages.Add(message);
					message.EM_LinkedObject = bill;
				}
			}
		}

		EDIMessage GenerateMessage(AsycudaBill bill, AsycudaArrivalHeader arrival, ZString freightStatusCode)
		{
			var messageHeader = new FreightStatusQueryMessageHeader(bill, arrival);
			messageHeader.SetFreightStatusQueryRequestCode(freightStatusCode);
			var message = new AIMMessageBuilder(messageHeader, bill.Factory).Build();
			return message;
		}
	}
}
