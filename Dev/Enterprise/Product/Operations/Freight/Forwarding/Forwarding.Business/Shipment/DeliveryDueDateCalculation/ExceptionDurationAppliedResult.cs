using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ExceptionDurationAppliedResult
	{
		public ExceptionDurationAppliedResult(ProcessTask exception, TimeSpan durationApplied, ZDateTime deliveryDueDate)
		{
			Exception = exception;
			DurationApplied = durationApplied;
			DeliveryDueDate = deliveryDueDate;
		}

		public ProcessTask Exception { get; }
		public TimeSpan DurationApplied { get; }
		public ZDateTime DeliveryDueDate { get; }
	}
}
