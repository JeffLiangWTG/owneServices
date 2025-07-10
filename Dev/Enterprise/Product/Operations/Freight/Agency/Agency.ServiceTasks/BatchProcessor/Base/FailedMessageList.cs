using System.Collections.Generic;
using Enterprise.Freight.Agency.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Agency.ServiceTasks
{
	internal sealed class FailedMessageList : Dictionary<string, List<FailedMessage>>
	{
		public void AddRange(string errorText, IEnumerable<EDIMessage> messages)
		{
			List<FailedMessage> list;

			if (!TryGetValue(errorText, out list))
			{
				list = new List<FailedMessage>();
				Add(errorText, list);
			}

			foreach (EDIMessage message in messages)
			{
				list.Add(GetJobDetails(errorText, message));
			}
		}
		public void Add(string errorText, EDIMessage message)
		{
			Add(GetJobDetails(errorText, message));
		}

		public void Add(FailedMessage message)
		{
			List<FailedMessage> list;

			if (!TryGetValue(message.ErrorText, out list))
			{
				list = new List<FailedMessage>();
				Add(message.ErrorText, list);
			}

			list.Add(message);
		}

		FailedMessage GetJobDetails(string errorText, EDIMessage message)
		{
			FailedMessage result = new FailedMessage(errorText);
			result.MessageNo = message.EM_MessageNum;
			result.MessageDateTime = message.EM_SystemCreateTimeUtc.ToDateTime();

			BillOfLadingContainer container = message.Factory.Load<BillOfLadingContainer>(message.EM_LinkUniqueID);

			if (container != null)
			{
				result.ContainerNo = container.JC_ContainerNum;
				if (container.Container != null)
				{
					result.ContainerType = container.Container.RC_Code;
				}

				BillOfLading bill = container.Booking;
				if (bill != null)
				{
					result.BillOfLading = bill.JS_HouseBill;
					result.ShipmentNo = bill.JS_UniqueConsignRef;
					result.Origin = bill.JS_RL_NKOrigin;
					result.Destination = bill.JS_RL_NKDestination;

					if (bill.Principal != null)
					{
						result.Principal = bill.Principal.OH_Code;
					}
				}
			}

			return result;
		}
	}
}
