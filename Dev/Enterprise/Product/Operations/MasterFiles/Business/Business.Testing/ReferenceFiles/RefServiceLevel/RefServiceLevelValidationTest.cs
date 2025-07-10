using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefServiceLevelValidationTest : BusinessObjectValidationTestCase
	{
		public void TestServiceDeliveryPercentageRange()
		{
			var serviceLevel = Factory.New<RefServiceLevel>();
			AssertNoErrors(serviceLevel.RS_ServiceDeliveryPercentageInfo);

			serviceLevel.RS_ServiceDeliveryPercentage = 101;
			AssertHasError(serviceLevel.RS_ServiceDeliveryPercentageInfo, "DIFOT Percentage should be between 0 and 100.");

			serviceLevel.RS_ServiceDeliveryPercentage = 75;
			AssertNoErrors(serviceLevel.RS_ServiceDeliveryPercentageInfo);
		}

		public void TestServiceDeliveryPercentage100WhenNonGuaranteed()
		{
			var serviceLevel = Factory.New<RefServiceLevel>();
			AssertNoErrors(serviceLevel.RS_ServiceDeliveryPercentageInfo);

			serviceLevel.RS_ServiceDeliveryPercentage = 60;
			AssertNoErrors(serviceLevel.RS_ServiceDeliveryPercentageInfo);

			serviceLevel.RS_ServiceDeliveryPercentage = 100;
			AssertHasError(serviceLevel.RS_ServiceDeliveryPercentageInfo, "Entering a DIFOT Percentage of 100 means that you are guaranteeing delivery. You should change the Agreement type to Guaranteed.");

			serviceLevel.RS_ServiceDeliveryType = ServiceLevelDeliveryTypeList.Codes.Guaranteed;
			AssertNoErrors(serviceLevel.RS_ServiceDeliveryPercentageInfo);

			serviceLevel.RS_ServiceDeliveryType = ServiceLevelDeliveryTypeList.Codes.DIFOT;
			serviceLevel.RS_ServiceDeliveryPercentage = 100;
			AssertHasError(serviceLevel.RS_ServiceDeliveryPercentageInfo, "Entering a DIFOT Percentage of 100 means that you are guaranteeing delivery. You should change the Agreement type to Guaranteed.");

			serviceLevel.RS_ServiceDeliveryType = string.Empty;
			serviceLevel.RS_ServiceDeliveryPercentage = 100;
			AssertHasError(serviceLevel.RS_ServiceDeliveryPercentageInfo, "Entering a DIFOT Percentage of 100 means that you are guaranteeing delivery. You should change the Agreement type to Guaranteed.");

			serviceLevel.RS_ServiceDeliveryType = ServiceLevelDeliveryTypeList.Codes.DIFOT;
			serviceLevel.RS_ServiceDeliveryPercentage = 60;
			AssertNoErrors(serviceLevel.RS_ServiceDeliveryPercentageInfo);
		}

		public void TestCheckTransitDays()
		{
			CheckTransitDays(true);
			CheckTransitDays(false);
		}

		void CheckTransitDays(bool isRegistry)
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = isRegistry, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var serviceLevel = Factory.New<RefServiceLevel>();
				AssertNoErrors(serviceLevel.DefaultTransitDaysInfo);

				serviceLevel.DefaultTransitDays = -5;
				AssertHasError(serviceLevel.DefaultTransitDaysInfo, "Days cannot be negative.");

				serviceLevel.DefaultTransitDays = 5;
				AssertNoErrors(serviceLevel.DefaultTransitDaysInfo);

				serviceLevel.RS_DefaultArrivalTime = new ZDateTime(1900, 1, 1, 10, 10, 0);
				serviceLevel.DefaultTransitHours = 0;
				serviceLevel.DefaultTransitDays = 0;

				AssertErrorCheck(isRegistry, serviceLevel.DefaultTransitDaysInfo);

				serviceLevel.RS_DefaultArrivalTime = ZDateTime.Empty;
				serviceLevel.RS_DefaultDeliveryDueTime = new ZDateTime(1900, 1, 1, 10, 10, 0);
				serviceLevel.DefaultTransitHours = 1;
				serviceLevel.DefaultTransitDays = 1;
				serviceLevel.DefaultTransitHours = 0;
				serviceLevel.DefaultTransitDays = 0;

				AssertErrorCheck(isRegistry, serviceLevel.DefaultTransitDaysInfo);

				serviceLevel.RS_DefaultArrivalTime = ZDateTime.Empty;
				serviceLevel.RS_DefaultDeliveryDueTime = ZDateTime.Empty;
				serviceLevel.RS_DeliverOnSaturday = true;
				serviceLevel.DefaultTransitHours = 1;
				serviceLevel.DefaultTransitDays = 1;
				serviceLevel.DefaultTransitHours = 0;
				serviceLevel.DefaultTransitDays = 0;

				AssertErrorCheck(isRegistry, serviceLevel.DefaultTransitDaysInfo);

				serviceLevel.RS_DefaultArrivalTime = ZDateTime.Empty;
				serviceLevel.RS_DefaultDeliveryDueTime = ZDateTime.Empty;
				serviceLevel.RS_DeliverOnSaturday = false;
				serviceLevel.RS_DeliverOnSunday = true;
				serviceLevel.DefaultTransitHours = 1;
				serviceLevel.DefaultTransitDays = 1;
				serviceLevel.DefaultTransitHours = 0;
				serviceLevel.DefaultTransitDays = 0;

				AssertErrorCheck(isRegistry, serviceLevel.DefaultTransitDaysInfo);

				serviceLevel.RS_DefaultArrivalTime = ZDateTime.Empty;
				serviceLevel.RS_DefaultDeliveryDueTime = ZDateTime.Empty;
				serviceLevel.RS_DeliverOnSaturday = false;
				serviceLevel.RS_DeliverOnSunday = false;
				serviceLevel.DefaultTransitHours = 1;
				serviceLevel.DefaultTransitDays = 1;
				serviceLevel.DefaultTransitHours = 0;
				serviceLevel.DefaultTransitDays = 0;

				AssertNoErrors(serviceLevel.DefaultTransitDaysInfo);
			}
		}

		public void TestCheckTransitHours()
		{
			CheckTransitHours(true);
			CheckTransitHours(false);
		}

		void CheckTransitHours(bool isRegistry)
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = isRegistry, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var serviceLevel = Factory.New<RefServiceLevel>();
				AssertNoErrors(serviceLevel.DefaultTransitHoursInfo);

				serviceLevel.DefaultTransitHours = -5;
				AssertHasError(serviceLevel.DefaultTransitHoursInfo, "Hours cannot be negative.");

				serviceLevel.DefaultTransitHours = 5;
				AssertNoErrors(serviceLevel.DefaultTransitHoursInfo);

				serviceLevel.DefaultTransitHours = 25;
				AssertHasError(serviceLevel.DefaultTransitHoursInfo, "Hours must be less than 24. Please use the 'Days' component for longer times.");

				serviceLevel.DefaultTransitHours = 23;
				AssertNoErrors(serviceLevel.DefaultTransitHoursInfo);

				serviceLevel.RS_DefaultArrivalTime = new ZDateTime(1900, 1, 1, 10, 10, 0);
				serviceLevel.DefaultTransitHours = 11;
				AssertErrorCheck(isRegistry, serviceLevel.DefaultTransitHoursInfo, "When arrival time has a value, transit hours must be zero, and conversely.");

				serviceLevel.RS_DefaultArrivalTime = ZDateTime.Empty;
				serviceLevel.RS_DefaultDeliveryDueTime = ZDateTime.Empty;
				serviceLevel.RS_DeliverOnSaturday = false;
				serviceLevel.DefaultTransitDays = 0;
				serviceLevel.DefaultTransitHours = 0;
				AssertNoErrors(serviceLevel.DefaultTransitHoursInfo);

				serviceLevel.RS_DefaultArrivalTime = new ZDateTime(1900, 1, 1, 10, 10, 0);
				serviceLevel.RS_DefaultDeliveryDueTime = ZDateTime.Empty;
				serviceLevel.RS_DeliverOnSaturday = false;
				serviceLevel.DefaultTransitDays = 1;
				serviceLevel.DefaultTransitHours = 0;
				AssertNoErrors(serviceLevel.DefaultTransitHoursInfo);

				serviceLevel.RS_DefaultArrivalTime = ZDateTime.Empty;
				serviceLevel.RS_DefaultDeliveryDueTime = new ZDateTime(1900, 1, 1, 10, 10, 0);
				serviceLevel.RS_DeliverOnSaturday = false;
				serviceLevel.DefaultTransitDays = 1;
				serviceLevel.DefaultTransitHours = 0;
				AssertNoErrors(serviceLevel.DefaultTransitHoursInfo);

				serviceLevel.RS_DefaultArrivalTime = ZDateTime.Empty;
				serviceLevel.RS_DefaultDeliveryDueTime = new ZDateTime(1900, 1, 1, 10, 10, 0);
				serviceLevel.RS_DeliverOnSaturday = false;
				serviceLevel.DefaultTransitDays = 0;
				serviceLevel.DefaultTransitHours = 10;
				AssertNoErrors(serviceLevel.DefaultTransitHoursInfo);

				serviceLevel.RS_DefaultArrivalTime = ZDateTime.Empty;
				serviceLevel.RS_DefaultDeliveryDueTime = ZDateTime.Empty;
				serviceLevel.RS_DeliverOnSaturday = true;
				serviceLevel.DefaultTransitDays = 1;
				serviceLevel.DefaultTransitHours = 0;
				AssertNoErrors(serviceLevel.DefaultTransitHoursInfo);

				serviceLevel.RS_DefaultArrivalTime = ZDateTime.Empty;
				serviceLevel.RS_DefaultDeliveryDueTime = ZDateTime.Empty;
				serviceLevel.RS_DeliverOnSaturday = true;
				serviceLevel.DefaultTransitDays = 0;
				serviceLevel.DefaultTransitHours = 10;
				AssertNoErrors(serviceLevel.DefaultTransitHoursInfo);

				serviceLevel.RS_DefaultArrivalTime = new ZDateTime(1900, 1, 1, 10, 10, 0);
				serviceLevel.RS_DefaultDeliveryDueTime = ZDateTime.Empty;
				serviceLevel.RS_DeliverOnSaturday = false;
				serviceLevel.DefaultTransitDays = 0;
				serviceLevel.DefaultTransitHours = 0;
				AssertErrorCheck(isRegistry, serviceLevel.DefaultTransitHoursInfo);

				serviceLevel.RS_DefaultArrivalTime = ZDateTime.Empty;
				serviceLevel.RS_DefaultDeliveryDueTime = new ZDateTime(1900, 1, 1, 10, 10, 0);
				serviceLevel.RS_DeliverOnSaturday = false;
				serviceLevel.DefaultTransitDays = 0;
				serviceLevel.DefaultTransitHours = 0;
				AssertErrorCheck(isRegistry, serviceLevel.DefaultTransitHoursInfo);

				serviceLevel.RS_DefaultArrivalTime = ZDateTime.Empty;
				serviceLevel.RS_DefaultDeliveryDueTime = ZDateTime.Empty;
				serviceLevel.RS_DeliverOnSaturday = true;
				serviceLevel.DefaultTransitDays = 0;
				serviceLevel.DefaultTransitHours = 0;
				AssertErrorCheck(isRegistry, serviceLevel.DefaultTransitHoursInfo);

				serviceLevel.RS_DefaultArrivalTime = ZDateTime.Empty;
				serviceLevel.RS_DefaultDeliveryDueTime = ZDateTime.Empty;
				serviceLevel.RS_DeliverOnSaturday = false;
				serviceLevel.RS_DeliverOnSunday = true;
				serviceLevel.DefaultTransitDays = 0;
				serviceLevel.DefaultTransitHours = 0;
				AssertErrorCheck(isRegistry, serviceLevel.DefaultTransitHoursInfo);

				serviceLevel.RS_DefaultArrivalTime = ZDateTime.Empty;
				serviceLevel.RS_DefaultDeliveryDueTime = ZDateTime.Empty;
				serviceLevel.RS_DeliverOnSaturday = false;
				serviceLevel.RS_DeliverOnSunday = false;
				serviceLevel.DefaultTransitDays = 1;
				serviceLevel.DefaultTransitHours = 1;
				serviceLevel.DefaultTransitDays = 0;
				serviceLevel.DefaultTransitHours = 0;
				AssertNoErrors(serviceLevel.DefaultTransitHoursInfo);
			}
		}

		public void TestCheckRS_DefaultArrivalTime()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var serviceLevel = Factory.New<RefServiceLevel>();
				AssertNoErrors(serviceLevel.RS_DefaultArrivalTimeInfo);

				serviceLevel.DefaultTransitHours = 11;
				serviceLevel.RS_DefaultArrivalTime = new ZDateTime(1900, 1, 1, 10, 10, 0);
				AssertHasError(serviceLevel.RS_DefaultArrivalTimeInfo, "When arrival time has a value, transit hours must be zero, and conversely.");

				serviceLevel.DefaultTransitHours = 0;
				serviceLevel.RS_DefaultArrivalTime = new ZDateTime(1900, 1, 1, 10, 10, 0);
				AssertNoErrors(serviceLevel.RS_DefaultArrivalTimeInfo);

				serviceLevel.DefaultTransitHours = 0;
				serviceLevel.DefaultTransitDays = 0;
				serviceLevel.RS_DefaultArrivalTime = ZDateTime.Empty;
				AssertNoErrors(serviceLevel.RS_DefaultArrivalTimeInfo);
			}
		}

		public void TestArrivalTimeValidZDateTimeRange()
		{
			var serviceLevel = Factory.New<RefServiceLevel>();
			serviceLevel.RS_DefaultArrivalTime = new ZDateTime(1900, 1, 1, 10, 10, 0);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestDeliveryDueTimeValidZDateTimeRange()
		{
			var serviceLevel = Factory.New<RefServiceLevel>();
			serviceLevel.RS_DefaultDeliveryDueTime = new ZDateTime(1900, 1, 1, 10, 10, 0);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		void AssertErrorCheck(bool isError, ZPropertyInfo zPropertyInfo, string errorMessage = null)
		{
			if (isError)
			{
				AssertHasError(zPropertyInfo, errorMessage ?? "Please specify a Transit time greater than zero.");
			}
			else
			{
				AssertNoErrors(zPropertyInfo);
			}
		}

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}
	}
}
