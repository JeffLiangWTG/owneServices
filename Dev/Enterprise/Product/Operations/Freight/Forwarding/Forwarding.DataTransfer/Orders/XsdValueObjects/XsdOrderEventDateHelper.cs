using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public static class XsdOrderMilestoneDateHelper
	{
		public static void ToEstimatedActualDates(Xsd.MilestoneDates milestoneDates, ZPropertyInfo estimatedInfo, ZPropertyInfo actualInfo)
		{
			if (milestoneDates.IsSpecified)
			{
				if (estimatedInfo != null)
				{
					estimatedInfo.Value = ZDateTime.Empty;
				}

				if (actualInfo != null)
				{
					actualInfo.Value = ZDateTime.Empty;
				}

				if (estimatedInfo != null && milestoneDates.Estimated.IsValidSmallDateTime)
				{
					estimatedInfo.Value = new ZDateTime(milestoneDates.Estimated);
				}

				if (actualInfo != null && milestoneDates.Actual.IsValidSmallDateTime)
				{
					actualInfo.Value = new ZDateTime(milestoneDates.Actual);
				}
			}
		}

		public static void ToEstimatedActualDates(Xsd.MilestoneDates milestoneDates, Order order, Event eventType)
		{
			if (milestoneDates.IsSpecified)
			{
				if (milestoneDates.Estimated.IsEmpty || milestoneDates.Estimated.IsValidSmallDateTime)
				{
					order.UpdateEventEstimate(eventType, milestoneDates.Estimated.ToOffset());
				}

				if (milestoneDates.Actual.IsEmpty || milestoneDates.Actual.IsValidSmallDateTime)
				{
					order.UpdateEvent(eventType, milestoneDates.Actual.ToOffset());
				}
			}
		}

		public static Xsd.MilestoneDates FromEstimatedActualDates(Order order, Event eventType)
		{
			return Xsd.MilestoneDates.FromEstimatedAndActual(
					ToLocalZDateTime(order.GetMilestoneEstimatedDate(eventType), true),
					ToLocalZDateTime(order.GetMilestoneActualDate(eventType), false));
		}

		static ZDateTime ToLocalZDateTime(ZDateTime offset, bool floor)
		{
			if (offset.IsValid)
			{
				var asDateTime = floor ? offset.ToZDateTime().ToSmallDateTimeFloor() : offset.ToZDateTime();
				return new ZDateTime(asDateTime, System.DateTimeKind.Local);
			}
			else
			{
				return offset.ToZDateTime();
			}
		}
	}
}
