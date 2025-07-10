using System;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	interface IBatchDelivery
	{
		IDeliveryResult DeliverBatch(DeliveryContext context, IEDICommunicationsMode mode, IDeliveryStreamWrapper[] batchStreams, Func<Messaging.Integration.IEDIMessage> getmessageFunc = null);
	}
}
