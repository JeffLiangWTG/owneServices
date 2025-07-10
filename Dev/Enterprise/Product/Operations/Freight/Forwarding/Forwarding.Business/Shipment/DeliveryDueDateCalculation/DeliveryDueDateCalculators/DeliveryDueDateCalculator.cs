using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.Business
{
	public abstract class DeliveryDueDateCalculator
	{
		public DeliveryDueDateCalculationContext DeliveryDueDateCalculationContext { get; }
		protected CalendarDayTypeProvider CalendarDayTypeProvider { get; }

		public DeliveryDueDateCalculator(DeliveryDueDateCalculationContext context)
		{
			DeliveryDueDateCalculationContext = context;
			CalendarDayTypeProvider = new CalendarDayTypeProvider();
		}

		public enum AddressType
		{
			PickupAddress,
			DeliveryAddress,
			CFSPickupAddress,
			CFSDeliveryAddress,
			DeliveryAgentAddress
		}

		public static IEnumerable<string> SupportedDeliveryModes
		{
			get
			{
				return new[]
				{
					Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR,
					Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS,
					Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR,
					Core.Constants.HBLDeliveryModes.Codes.CFS_CFS,
					Core.Constants.HBLDeliveryModes.Codes.ARPT_ARPT,
					Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT,
					Core.Constants.HBLDeliveryModes.Codes.CFS_ARPT,
					Core.Constants.HBLDeliveryModes.Codes.ARPT_DOOR,
					Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS
				};
			}
		}

		public virtual IDeliveryDueDateCalculationResult CalculateDeliveryDueDate()
		{
			if (!AreAllPropertiesValid.Result)
			{
				return DeliveryDueDateCalculationResult.Failure(ZString.Empty,
							Res.GetString("7c0a7449-4cf4-4d73-b50e-4bea1ef507fc", "Checking Properties: Failed to calculate the Delivery Due Date due to the Invalid Property: {0}", AreAllPropertiesValid.Message));
			}

			if (DeliveryDueDateCalculationContext.Factory?.IsInSaveTransaction ?? false)
			{
				RefreshTimetablesDuringSaveTransaction();
			}

			var calculationLogBuilder = DeliveryDueDateCalculationLogHelper.InitCalculationLog(DeliveryDueDateCalculationContext);

			var readyDate = DeliveryDueDateCalculationContext.ReadyDate;
			var finalDateTime = new ZDateTime(readyDate.Year, readyDate.Month, readyDate.Day, readyDate.Hour, readyDate.Minute, 0);
			IDeliveryDueDateCalculationResult calculationResult = DeliveryDueDateCalculationResult.Success(finalDateTime, calculationLogBuilder.ToString());

			foreach (var step in GetCalculationSteps())
			{
				calculationResult = step.Calculate(calculationResult);
				calculationLogBuilder.Append(calculationResult.CalculationLog);

				if (!string.IsNullOrEmpty(calculationResult.ErrorMessage) || !calculationResult.DeliveryDueDate.IsValid)
				{
					return DeliveryDueDateCalculationResult.Failure(calculationResult.ErrorMessage, calculationLogBuilder.ToString());
				}

				if (!DeliveryDueDateCalculationContext.ServiceLevelGenericTransitTimeHasBeenUsed && calculationResult.ServiceLevelGenericTransitTimeHasBeenUsed)
				{
					DeliveryDueDateCalculationContext.ServiceLevelGenericTransitTimeHasBeenUsed = calculationResult.ServiceLevelGenericTransitTimeHasBeenUsed;
				}

				finalDateTime = calculationResult.DeliveryDueDate;
			}

			return DeliveryDueDateCalculationResult.Success(finalDateTime, calculationLogBuilder.ToString(), calculationResult.ArrivalTimeUsedForDeliveryCFS, calculationResult.ServiceLevelGenericTransitTimeHasBeenUsed);
		}

		protected virtual (bool Result, ZString Message) AreAllPropertiesValid
		{
			get
			{
				if (DeliveryDueDateCalculationContext.Factory == null)
				{
					return (false, Res.GetString("7502f74e-bd7d-4712-8999-5a4fea2dc97d", "Factory"));
				}

				if (!DeliveryDueDateCalculationContext.ReadyDate.IsValid)
				{
					return (false, Res.GetString("e5d10e16-7c93-4d31-9788-05f66bfe813f", "Ready Date"));
				}

				if (DeliveryDueDateCalculationContext.ServiceLevel == null)
				{
					return (false, Res.GetString("c94452f1-6349-4a6e-b5ec-c7bf9a5fa250", "Service Level"));
				}

				if (DeliveryDueDateCalculationContext.PickupAddress == null && DeliveryDueDateCalculationContext.IsDoortoX)
				{
					return (false, Res.GetString("9611c801-de3b-4d72-8a9f-39b66e61b8c3", "Pickup Address"));
				}

				if (DeliveryDueDateCalculationContext.CFSPickupAddress == null && DeliveryDueDateCalculationContext.ServiceLevel.RS_DefaultTransitHours == 0)
				{
					return (false, Res.GetString("77ba6728-b8d1-4160-8e57-268a161ca62e", "Pickup CFS Address"));
				}

				if (DeliveryDueDateCalculationContext.DeliveryAddress == null && DeliveryDueDateCalculationContext.IsXtoDoor)
				{
					return (false, Res.GetString("8ca104fe-53dc-45e3-82b5-ac8252f359ae", "Delivery Address"));
				}

				if (DeliveryDueDateCalculationContext.CFSDeliveryAddress == null && !DeliveryDueDateCalculationContext.IsDTC && DeliveryDueDateCalculationContext.ServiceLevel.RS_DefaultTransitHours == 0)
				{
					return (false, Res.GetString("54b83f2d-e6c5-4021-ba77-9eac04dff032", "Delivery CFS Address"));
				}

				if (DeliveryDueDateCalculationContext.DeliveryAgentAddress == null && DeliveryDueDateCalculationContext.IsDTC && DeliveryDueDateCalculationContext.ServiceLevel.RS_DefaultTransitHours == 0)
				{
					return (false, Res.GetString("7414a656-049c-4bec-a151-8b92f9e6b8a3", "Delivery Agent Address for DTC"));
				}

				return (true, ZString.Empty);
			}
		}

		protected abstract IEnumerable<IDeliveryDueDateCalculationStep> GetCalculationSteps();

		#region Refresh Addresses Timetable

		void RefreshTimetablesDuringSaveTransaction()
		{
			RefreshDefaultTimetableForAddress(DeliveryDueDateCalculationContext.PickupAddress);
			RefreshDefaultTimetableForAddress(DeliveryDueDateCalculationContext.DeliveryAddress);
			RefreshDefaultTimetableForAddress(DeliveryDueDateCalculationContext.CFSPickupAddress);
			RefreshDefaultTimetableForAddress(DeliveryDueDateCalculationContext.CFSDeliveryAddress);
		}

		void RefreshDefaultTimetableForAddress(IDocAddress address)
		{
			if (address is OrgAddress orgAddress
				&& orgAddress.TimetablesRangeType == OrgTimeTableRangeType.Default
				&& orgAddress.Timetables.Count == 0)
			{
				orgAddress.Timetables.RefreshFromDb();
			}
		}

		#endregion
	}
}
