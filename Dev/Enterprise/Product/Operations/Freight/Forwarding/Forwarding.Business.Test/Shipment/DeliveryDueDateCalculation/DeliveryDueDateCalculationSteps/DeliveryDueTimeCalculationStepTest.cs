using System;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	internal class DeliveryDueTimeCalculationStepTest : TestCaseWithFactory
	{
		public void TestCalculate()
		{
			var deliveryDueTime = new DeliveryDueTime(new ZDateTime(1900, 1, 1, 9, 0, 0).TimeOfDay, DeliveryDueTimeSource.ZoneItem);
			Func<DeliveryDueTime> deliveryDueTimeFunc = () => deliveryDueTime;

			var calendarDayTypeProvider = new CalendarDayTypeProvider();

			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			Factory.Save();

			var readyDate = new ZDateTime(2022, 09, 26, 7, 12, 22);
	
			var step = new DeliveryDueTimeCalculationStep(CreateDeliveryDueDateCalculationContext(deliveryAddress), calendarDayTypeProvider);
			FieldInfo deliveryDueTimeFuncField = typeof(DeliveryDueTimeCalculationStep).GetField("deliveryDueTimeFunc", BindingFlags.NonPublic | BindingFlags.Instance);
			deliveryDueTimeFuncField.SetValue(step, deliveryDueTimeFunc);

			var initialInput = DeliveryDueDateCalculationResult.Success(readyDate, ZString.Empty);
			var result = step.Calculate(initialInput);
			AssertEquals(readyDate.Date.Add(deliveryDueTime.Time), result.DeliveryDueDate);
			AssertEquals(@"Delivery due time: 09:00:00, Source: ZoneItem
Delivery due time adjusted from 26-Sep-22 07:12:22 to 26-Sep-22 09:00:00 based on ZoneItem's configured delivery due time
", result.CalculationLog);

			readyDate = new ZDateTime(2022, 09, 26, 9, 12, 22);
			step = new DeliveryDueTimeCalculationStep(CreateDeliveryDueDateCalculationContext(deliveryAddress), calendarDayTypeProvider);
			deliveryDueTimeFuncField.SetValue(step, deliveryDueTimeFunc);

			initialInput = DeliveryDueDateCalculationResult.Success(readyDate, ZString.Empty);
			result = step.Calculate(initialInput);
			AssertEquals(readyDate.Date.AddDays(1).Add(deliveryDueTime.Time), result.DeliveryDueDate);
			AssertEquals(@"Delivery due time: 09:00:00, Source: ZoneItem
Adding one extra day: 26-Sep-22 09:12:22 adjusted to 27-Sep-22 09:00:00 because calculated delivery time is after delivery due time
Delivery due time adjusted from 26-Sep-22 09:12:22 to 27-Sep-22 09:00:00 based on ZoneItem's configured delivery due time
", result.CalculationLog);

			readyDate = new ZDateTime(2022, 10, 01, 9, 12, 22);
			step = new DeliveryDueTimeCalculationStep(CreateDeliveryDueDateCalculationContext(deliveryAddress), calendarDayTypeProvider);
			deliveryDueTimeFuncField.SetValue(step, deliveryDueTimeFunc);

			initialInput = DeliveryDueDateCalculationResult.Success(readyDate, ZString.Empty);
			result = step.Calculate(initialInput);
			AssertEquals(readyDate.Date.AddDays(2).Add(deliveryDueTime.Time), result.DeliveryDueDate);
			AssertEquals(@"Delivery due time: 09:00:00, Source: ZoneItem
Adding one extra day: 01-Oct-22 09:12:22 adjusted to 02-Oct-22 09:00:00 because calculated delivery time is after delivery due time
Delivery Address Weekend Days/Public Holidays: Sunday 02-Oct-22; 
02-Oct-22 09:00:00 adjusted to 03-Oct-22 09:00:00 because of Delivery Address Weekends/Public Holidays
Delivery due time adjusted from 01-Oct-22 09:12:22 to 03-Oct-22 09:00:00 based on ZoneItem's configured delivery due time
", result.CalculationLog);
		}

		public void TestCalculate_DeliverOnWeekend()
		{
			var deliveryDueTime = new DeliveryDueTime(new ZDateTime(1900, 1, 1, 9, 0, 0).TimeOfDay, DeliveryDueTimeSource.ZoneItem);
			Func<DeliveryDueTime> deliveryDueTimeFunc = () => deliveryDueTime;
			var calendarDayTypeProvider = new CalendarDayTypeProvider();

			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "NSW", "Sydney", "AUSYD");

			Factory.Save();

			var step = new DeliveryDueTimeCalculationStep(CreateDeliveryDueDateCalculationContext(deliveryAddress), calendarDayTypeProvider);
			FieldInfo deliveryDueTimeFuncField = typeof(DeliveryDueTimeCalculationStep).GetField("deliveryDueTimeFunc", BindingFlags.NonPublic | BindingFlags.Instance);
			deliveryDueTimeFuncField.SetValue(step, deliveryDueTimeFunc);

			FieldInfo deliverOnWeekendField = typeof(DeliveryDueTimeCalculationStep).GetField("deliverOnWeekend", BindingFlags.NonPublic | BindingFlags.Instance);
			deliverOnWeekendField.SetValue(step, true);

			var initialInput = DeliveryDueDateCalculationResult.Success(new ZDateTime(2022, 09, 30, 7, 12, 22), ZString.Empty);
			var result = step.Calculate(initialInput);
			AssertEquals(new ZDateTime(2022, 09, 30, 9, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Delivery due time: 09:00:00, Source: ZoneItem
Delivery due time adjusted from 30-Sep-22 07:12:22 to 30-Sep-22 09:00:00 based on ZoneItem's configured delivery due time
", result.CalculationLog);

			initialInput = DeliveryDueDateCalculationResult.Success(new ZDateTime(2022, 10, 1, 7, 12, 22), ZString.Empty);
			result = step.Calculate(initialInput);
			AssertEquals(new ZDateTime(2022, 10, 1, 9, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Delivery due time: 09:00:00, Source: ZoneItem
Delivery due time adjusted from 01-Oct-22 07:12:22 to 01-Oct-22 09:00:00 based on ZoneItem's configured delivery due time
", result.CalculationLog);
		}

		public void TestCalculate_WhenAddressIsNull()
		{
			var deliveryDueTime = new DeliveryDueTime(new ZDateTime(1900, 1, 1, 9, 0, 0).TimeOfDay, DeliveryDueTimeSource.ServiceLevel);
			Func<DeliveryDueTime> deliveryDueTimeFunc = () => deliveryDueTime;
			var calendarDayTypeProvider = new CalendarDayTypeProvider();

			Factory.Save();

			var step = new DeliveryDueTimeCalculationStep(CreateDeliveryDueDateCalculationContext(null), calendarDayTypeProvider);
			FieldInfo deliveryDueTimeFuncField = typeof(DeliveryDueTimeCalculationStep).GetField("deliveryDueTimeFunc", BindingFlags.NonPublic | BindingFlags.Instance);
			deliveryDueTimeFuncField.SetValue(step, deliveryDueTimeFunc);

			var initialInput = DeliveryDueDateCalculationResult.Success(new ZDateTime(2022, 10, 1, 7, 12, 22), ZString.Empty);
			var result = step.Calculate(initialInput);
			AssertEquals(new ZDateTime(2022, 10, 1, 9, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Delivery due time: 09:00:00, Source: ServiceLevel
Delivery due time adjusted from 01-Oct-22 07:12:22 to 01-Oct-22 09:00:00 based on ServiceLevel's configured delivery due time
", result.CalculationLog);
		}

		#region Helper Methods

		DeliveryDueDateCalculationContext CreateDeliveryDueDateCalculationContext(OrgAddress orgAddress)
		{
			return new DeliveryDueDateCalculationContext(Factory,
				ZDateTime.Now,
				"STD",
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				orgAddress?.Header.OH_Code ?? null,
				orgAddress?.AddressCode ?? null,
				ZString.Empty,
				ZString.Empty
			);
		}

		#endregion
	}
}
