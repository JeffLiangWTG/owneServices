using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public interface IDelivery
	{
		IDeliveryResult Deliver(DeliveryContext context, IEDICommunicationsMode mode, IDeliveryStreamWrapper stream, Func<Messaging.Integration.IEDIMessage> getmessageFunc = null);
	}

	public interface IDeliveryResult
	{
		bool Succeeded { get; }
		bool CanSave { get; }
		ZDateTime FailTime { get; }
		Exception Exception { get; }
		string FailureReason { get; }
	}

	[ThreadSafe]
	public class DeliveryResult : IDeliveryResult
	{
		DeliveryResult()
		{
			CanSave = Succeeded = true;
		}

		DeliveryResult(Exception e)
		{
			Exception = e;
			FailTime = ZDateTime.Now;
		}

		DeliveryResult(string failureReason)
		{
			FailureReason = failureReason;
			CanSave = true;
			FailTime = ZDateTime.Now;
		}

		public bool Succeeded { get; }
		public bool CanSave { get; }
		public Exception Exception { get; }
		public ZDateTime FailTime { get; }
		public string FailureReason { get; }

		public static DeliveryResult Success { get; } = new DeliveryResult();

		public static DeliveryResult Error(Exception e) => new DeliveryResult(e);

		public static DeliveryResult ReportInvalidConfiguration(string failureReason) => new DeliveryResult(failureReason);
	}
}
