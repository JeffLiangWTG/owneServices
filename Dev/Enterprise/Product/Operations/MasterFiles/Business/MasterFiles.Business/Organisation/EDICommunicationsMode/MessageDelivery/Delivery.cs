using System;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public abstract class Delivery : IDelivery
	{
		#region Null Object Pattern

		class DummyDelivery : Delivery
		{
			public override IDeliveryResult Deliver(DeliveryContext context, IEDICommunicationsMode mode, IDeliveryStreamWrapper stream, Func<Messaging.Integration.IEDIMessage> getMessageFunc = null)
			{
				return DeliveryResult.Success;
			}
		}

		public static IDelivery Dummy
		{
			get
			{
				return new DummyDelivery();
			}
		}

		#endregion

		public static IDelivery GetInstance(IEDICommunicationsMode mode)
		{
			return DeliveryFactory.Build(mode);
		}

		public abstract IDeliveryResult Deliver(DeliveryContext context, IEDICommunicationsMode mode, IDeliveryStreamWrapper stream, Func<Messaging.Integration.IEDIMessage> getmessageFunc = null);
	}
}
