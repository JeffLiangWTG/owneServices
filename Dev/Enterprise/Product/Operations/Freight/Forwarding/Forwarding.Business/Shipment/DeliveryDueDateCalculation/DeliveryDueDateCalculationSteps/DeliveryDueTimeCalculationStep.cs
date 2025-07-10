using System;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class DeliveryDueTimeCalculationStep : IDeliveryDueDateCalculationStep
	{
		public DeliveryDueTimeCalculationStep(DeliveryDueDateCalculationContext context, CalendarDayTypeProvider calendarDayTypeProvider)
		{
			this.calendarDayTypeProvider = calendarDayTypeProvider;
			this.deliveryAddress = context.DeliveryAddress;
			this.deliveryDueTimeFunc = context.DeliveryDueTimeFunc;
			this.deliverOnWeekend = context.DeliverOnWeekend;
		}

		readonly CalendarDayTypeProvider calendarDayTypeProvider;
		readonly IDocAddress deliveryAddress;
		readonly Func<DeliveryDueTime> deliveryDueTimeFunc;
		readonly bool deliverOnWeekend;

		public IDeliveryDueDateCalculationResult Calculate(IDeliveryDueDateCalculationResult previousStepResult)
		{
			var deliveryDueTime = deliveryDueTimeFunc();
			var initialDateTime = previousStepResult.DeliveryDueDate;

			if (deliveryDueTime == null || deliveryDueTime.Time.TotalMilliseconds <= 0)
			{
				return DeliveryDueDateCalculationResult.Success(initialDateTime, ZString.Empty, previousStepResult);
			}

			var calculationLogBuilder = new ZStringBuilder();
			calculationLogBuilder.AppendLine(Res.GetString("5653e20d-6117-4342-bab0-abc1647bbeb3", "Delivery due time: {0}, Source: {1}", deliveryDueTime.Time, deliveryDueTime.Source));

			if (deliveryAddress == null || deliverOnWeekend)
			{
				var resultForWeekend = initialDateTime.Date.Add(deliveryDueTime.Time);
				calculationLogBuilder.AppendLine(Res.GetString("0eb423ed-2543-4286-8b65-0ca87c9b517d", "Delivery due time adjusted from {0} to {1} based on {2}'s configured delivery due time",
					initialDateTime, resultForWeekend, deliveryDueTime.Source));
				return DeliveryDueDateCalculationResult.Success(resultForWeekend, calculationLogBuilder.ToString(), previousStepResult);
			}

			var result = DeliveryDueDateCalculationHelper.AdjustTimeToDeliverDueTime(initialDateTime, calculationLogBuilder, deliveryDueTime, calendarDayTypeProvider, deliveryAddress);
			return DeliveryDueDateCalculationResult.Success(result, calculationLogBuilder.ToString(), previousStepResult);
		}
	}
}
