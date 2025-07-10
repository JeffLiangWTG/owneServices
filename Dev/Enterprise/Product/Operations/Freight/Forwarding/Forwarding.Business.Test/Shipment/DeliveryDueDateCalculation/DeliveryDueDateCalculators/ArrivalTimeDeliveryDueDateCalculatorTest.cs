using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	class ArrivalTimeDeliveryDueDateCalculatorTest
	{
		internal static void TestCalculate_ArrivalTimeBeforeOpeningHoursAtDeliveryCFS(BusinessObjectFactory factory, Action<string, ZDateTime> assertDeliveryDueDate, ForwardingShipment shipment, RefServiceLevel serviceLevel, RefTransitTime transitTime, ZInt transitHours, ZByte transitDayOfWeek, ZDateTime expectedTimeTransitTime, ZDateTime expectedTimeServiceLevel)
		{
			shipment.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2022, 8, 8, 10, 0, 0);
			shipment.JS_A_RCV = new ZDateTime(2022, 8, 8, 10, 0, 0);
			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(factory, transitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, transitHours, transitDayOfWeek, new ZDateTime(1900, 1, 1, 7, 0, 0));
			factory.Save();

			assertDeliveryDueDate("Uses ArrivalTime from transit time detail before opening hours at Delivery CFS", expectedTimeTransitTime);

			transitTime.Delete();
			serviceLevel.RS_DefaultTransitHours = 48;
			serviceLevel.RS_DefaultArrivalTime = new ZDateTime(1900, 1, 1, 6, 30, 0);
			factory.Save();

			assertDeliveryDueDate("Uses ArrivalTime from service level before opening hours at Delivery CFS", expectedTimeServiceLevel);
		}
	}
}
