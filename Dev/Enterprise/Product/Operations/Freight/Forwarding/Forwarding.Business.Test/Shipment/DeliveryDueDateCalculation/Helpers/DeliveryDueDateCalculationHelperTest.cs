using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class DeliveryDueDateCalculationHelperTest : TestCaseWithFactory
	{
		public void TestGetRateModeFromShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			void AssertShipmentRateMode(string transportMode, string containerMode, string expectedRateMode)
			{
				shipment.JS_TransportMode = transportMode;
				shipment.JS_PackingMode = containerMode;
				AssertEquals(expectedRateMode, DeliveryDueDateCalculationHelper.GetRateModeFromShipment(shipment));
			}

			AssertShipmentRateMode("AIR", "ULD", "ULD");
			AssertShipmentRateMode("AIR", "BCN", "AIR");
			AssertShipmentRateMode("COU", "BCN", "ALL");
		}

		public void TestGetReadyDateFromShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Empty;

			AssertEquals(ZDateTime.Empty, DeliveryDueDateCalculationHelper.GetReadyDateFromShipment(shipment).ReadyDate);
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS;

			AssertEquals(ZDateTime.Empty, DeliveryDueDateCalculationHelper.GetReadyDateFromShipment(shipment).ReadyDate);

			shipment.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2011, 5, 4);

			AssertEquals(shipment.DocsAndCartage.JP_PickupCartageCompleted, DeliveryDueDateCalculationHelper.GetReadyDateFromShipment(shipment).ReadyDate);

			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			AssertEquals(shipment.DocsAndCartage.JP_PickupCartageCompleted, DeliveryDueDateCalculationHelper.GetReadyDateFromShipment(shipment).ReadyDate);

			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS;
			AssertEquals(shipment.DocsAndCartage.JP_PickupCartageCompleted, DeliveryDueDateCalculationHelper.GetReadyDateFromShipment(shipment).ReadyDate);

			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2011, 2, 4);

			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			AssertEquals(shipment.DocsAndCartage.JP_PickupRequiredBy, DeliveryDueDateCalculationHelper.GetReadyDateFromShipment(shipment).ReadyDate);

			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS;
			AssertEquals(shipment.DocsAndCartage.JP_PickupRequiredBy, DeliveryDueDateCalculationHelper.GetReadyDateFromShipment(shipment).ReadyDate);

			shipment.JS_A_RCV = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Empty;

			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR;
			AssertEquals(ZDateTime.Empty, DeliveryDueDateCalculationHelper.GetReadyDateFromShipment(shipment).ReadyDate);

			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CFS;
			AssertEquals(ZDateTime.Empty, DeliveryDueDateCalculationHelper.GetReadyDateFromShipment(shipment).ReadyDate);

			shipment.JS_A_RCV = new ZDateTime(2011, 5, 5);

			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR;
			AssertEquals(shipment.JS_A_RCV, DeliveryDueDateCalculationHelper.GetReadyDateFromShipment(shipment).ReadyDate);

			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CFS;
			AssertEquals(shipment.JS_A_RCV, DeliveryDueDateCalculationHelper.GetReadyDateFromShipment(shipment).ReadyDate);

			shipment.JS_A_RCV = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2011, 2, 4);

			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR;
			AssertEquals(shipment.DocsAndCartage.JP_PickupRequiredBy, DeliveryDueDateCalculationHelper.GetReadyDateFromShipment(shipment).ReadyDate);

			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CFS;
			AssertEquals(shipment.DocsAndCartage.JP_PickupRequiredBy, DeliveryDueDateCalculationHelper.GetReadyDateFromShipment(shipment).ReadyDate);
		}

		public void TestLoadAddressByOrgCodeAndShortCode()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "ADU";

			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "Test Address";

			var anotherAddress = orgHeader.Addresses.AddNew();
			anotherAddress.OA_Address1 = "Another Test Address";

			Factory.Save();

			AssertEquals(address.PK, DeliveryDueDateCalculationHelper.LoadAddressByOrgCodeAndShortCode(Factory, "ADU", "Test Address").E2_OA_Address);
			AssertEquals(anotherAddress.PK, DeliveryDueDateCalculationHelper.LoadAddressByOrgCodeAndShortCode(Factory, "ADU", "Another Test Address").E2_OA_Address);
			AssertNull(DeliveryDueDateCalculationHelper.LoadAddressByOrgCodeAndShortCode(Factory, "ADU", "Non-exist Test Address"));
			AssertNull(DeliveryDueDateCalculationHelper.LoadAddressByOrgCodeAndShortCode(Factory, "UDA", "Test Address"));
		}

		public void TestConvertOriginLocalTimeToDestinationLocalTime()
		{
			var originAddress = Factory.NewWithValidTestData<OrgAddress>();
			originAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			var destinationAddress = Factory.NewWithValidTestData<OrgAddress>();
			destinationAddress.OA_RL_NKRelatedPortCode = "NZAKL";

			var ausydLocalTime = new ZDateTime(2022, 9, 26, 9, 0, 0);
			AssertEquals("NZAKL local time is 3 hours ahead from AUSYD local time", ausydLocalTime.AddHours(3), DeliveryDueDateCalculationHelper.ConvertOriginLocalTimeToDestinationLocalTime(ausydLocalTime, originAddress, destinationAddress, Factory));
		}

		public void TestConvertOriginLocalTimeToDestinationLocalTime_TimeZoneFallbacks()
		{
			var originAddress = Factory.NewWithValidTestData<OrgAddress>();
			originAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			var destinationAddress = Factory.NewWithValidTestData<OrgAddress>();
			destinationAddress.Header.OH_RL_NKClosestPort = "AUSYD";

			var aucklandLocalTime = new ZDateTime(2022, 9, 26, 9, 0, 0);
			AssertEquals("Destination timezone is from Org closest port. AUSYD local time is 3 hours behind NZAKL local time", aucklandLocalTime.AddHours(-3), DeliveryDueDateCalculationHelper.ConvertOriginLocalTimeToDestinationLocalTime(aucklandLocalTime, originAddress, destinationAddress, Factory));

			destinationAddress.OA_RL_NKRelatedPortCode = "AUADL";
			AssertEquals("Destination timezone is from Address port. AUADL local time is 3.5 hours behind NZAKL local time", aucklandLocalTime.AddHours(-3.5), DeliveryDueDateCalculationHelper.ConvertOriginLocalTimeToDestinationLocalTime(aucklandLocalTime, originAddress, destinationAddress, Factory));

			destinationAddress.OA_City = "Shay Gap";
			destinationAddress.OA_RN_NKCountryCode = "AU";
			AssertEquals("Destination timezone is from Address city and country. AUSGP local time is 5 hours behind NZAKL local time", aucklandLocalTime.AddHours(-5), DeliveryDueDateCalculationHelper.ConvertOriginLocalTimeToDestinationLocalTime(aucklandLocalTime, originAddress, destinationAddress, Factory));
		}

		public void TestDayOfWeek()
		{
			var dayofWeekvalue = DeliveryDueDateCalculationHelper.GetDayOfWeek(DayOfWeek.Monday);
			AssertEquals("It should be one", (ZByte)1, dayofWeekvalue);

			dayofWeekvalue = DeliveryDueDateCalculationHelper.GetDayOfWeek(DayOfWeek.Tuesday);
			AssertEquals("It should be two", (ZByte)2, dayofWeekvalue);

			dayofWeekvalue = DeliveryDueDateCalculationHelper.GetDayOfWeek(DayOfWeek.Wednesday);
			AssertEquals("It should be three", (ZByte)3, dayofWeekvalue);

			dayofWeekvalue = DeliveryDueDateCalculationHelper.GetDayOfWeek(DayOfWeek.Thursday);
			AssertEquals("It should be four", (ZByte)4, dayofWeekvalue);

			dayofWeekvalue = DeliveryDueDateCalculationHelper.GetDayOfWeek(DayOfWeek.Friday);
			AssertEquals("It should be five", (ZByte)5, dayofWeekvalue);

			dayofWeekvalue = DeliveryDueDateCalculationHelper.GetDayOfWeek(DayOfWeek.Saturday);
			AssertEquals("It should be six", (ZByte)6, dayofWeekvalue);

			dayofWeekvalue = DeliveryDueDateCalculationHelper.GetDayOfWeek(DayOfWeek.Sunday);
			AssertEquals("It should be seven", (ZByte)7, dayofWeekvalue);
		}

		public void TestGetRelatedTransportZoneOwner_And_GetZoneItem()
		{
			var zoneOwner = Factory.NewWithValidTestData<OrgHeader>();
			var zoneOwnerAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(zoneOwner, "Zone Owner Address", "", Core.Constants.CountryCodes.VietNam, "Vinh", "49", "VNVNH");
			var zone1 = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, zoneOwner, "1000", "2000", Core.Constants.CountryCodes.VietNam);

			var zoneItem1 = zone1.Items[0];
			var zoneItem2 = zone1.Items.AddNew();
			zoneItem2.TQ_FromPostCode = "3000";
			zoneItem2.TQ_RN_NKCountry = Core.Constants.CountryCodes.VietNam;

			var vinhCity = Factory.NewWithValidTestData<RefCityTown>();
			vinhCity.R9_RN_NKCountry = Core.Constants.CountryCodes.VietNam;
			vinhCity.R9_RW_NKState = "49";
			vinhCity.R9_InternationalName = "Vinh";

			var dalatCity = Factory.NewWithValidTestData<RefCityTown>();
			dalatCity.R9_RN_NKCountry = Core.Constants.CountryCodes.VietNam;
			dalatCity.R9_RW_NKState = "44";
			dalatCity.R9_InternationalName = "Dalat";

			var postCode1 = Factory.New<RefPostCode>();
			postCode1.RK_CityTownPostCode = "3001";
			var postCode2 = Factory.New<RefPostCode>();
			postCode2.RK_CityTownPostCode = "3002";
			var postCode3 = Factory.New<RefPostCode>();
			postCode3.RK_CityTownPostCode = "3003";

			var pivot1 = Factory.New<RefCityPCodePivot>();
			pivot1.R0_R9 = vinhCity.PK;
			pivot1.R0_RK = postCode1.PK;
			var pivot2 = Factory.New<RefCityPCodePivot>();
			pivot2.R0_R9 = vinhCity.PK;
			pivot2.R0_RK = postCode2.PK;
			var pivot3 = Factory.New<RefCityPCodePivot>();
			pivot3.R0_R9 = vinhCity.PK;
			pivot3.R0_RK = postCode3.PK;

			var zoneItem3 = zone1.Items.AddNew();
			zoneItem3.TQ_RN_NKCountry = Core.Constants.CountryCodes.VietNam;
			zoneItem3.TQ_R9_CityTown = vinhCity.PK;

			var zoneItem4 = zone1.Items.AddNew();
			zoneItem4.TQ_RN_NKCountry = Core.Constants.CountryCodes.VietNam;
			zoneItem4.TQ_R9_CityTown = dalatCity.PK;

			zone1.TransportProvider.TP_R9_ZoneHubLocation = vinhCity.PK;

			Factory.Save();

			void AssertProvider(string message, ZString postcode, ZString city, ZString state, ZString country)
			{
				var org = DeliveryDueDateCalculationHelper.GetRelatedTransportZoneOwner(postcode, city, state, country, Factory);
				AssertEquals(message, zoneOwner.PK, org.PK);
			}

			CombineAssertions("GetRelatedTransportZoneOwner", () =>
			{
				AssertProvider("Postcode range", "1500", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.VietNam);
				AssertProvider("Postcode exact match", "3000", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.VietNam);
				AssertProvider("City match with related postcode", "3001", "Vinh", ZString.Empty, Core.Constants.CountryCodes.VietNam);
				AssertProvider("City match with name", "5000", "Dalat", ZString.Empty, Core.Constants.CountryCodes.VietNam);
				AssertProvider("State match", "5000", ZString.Empty, "49", Core.Constants.CountryCodes.VietNam);
				AssertProvider("Country match", "5000", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.VietNam);
			});

			CombineAssertions("GetZoneItem", () =>
			{
				var item1 = DeliveryDueDateCalculationHelper.GetZoneItem(GetIDocAddressMock("1500", "", Core.Constants.CountryCodes.VietNam), zoneOwnerAddress, Factory);
				AssertEquals("Postcode range", zoneItem1.PK, item1.PK);
				var item2 = DeliveryDueDateCalculationHelper.GetZoneItem(GetIDocAddressMock("3000", "", Core.Constants.CountryCodes.VietNam), zoneOwnerAddress, Factory);
				AssertEquals("Postcode exact match", zoneItem2.PK, item2.PK);
				var item3 = DeliveryDueDateCalculationHelper.GetZoneItem(GetIDocAddressMock("3001", "Vinh", Core.Constants.CountryCodes.VietNam), zoneOwnerAddress, Factory);
				AssertEquals("City match with related postcode", zoneItem3.PK, item3.PK);
				var item4 = DeliveryDueDateCalculationHelper.GetZoneItem(GetIDocAddressMock("5000", "Dalat", Core.Constants.CountryCodes.VietNam), zoneOwnerAddress, Factory);
				AssertEquals("City match with name", zoneItem4.PK, item4.PK);
				var item5 = DeliveryDueDateCalculationHelper.GetZoneItem(GetIDocAddressMock("5000", "", Core.Constants.CountryCodes.VietNam), zoneOwnerAddress, Factory);
				AssertNull("Neither postcode nor city matches", item5);
			});

			zone1.Items[2].Delete();

			var zoneOwner2 = Factory.NewWithValidTestData<OrgHeader>();
			DeliveryDueDateCalculationTestHelper.SetupAddress(zoneOwner2, "Zone Owner 2 Address", "", Core.Constants.CountryCodes.VietNam, "Vinh", "49", "VNVNH");
			var zone2 = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, zoneOwner2, "1000", "2000", Core.Constants.CountryCodes.VietNam, providerType: "OPS");
			var zone2Provider = zone2.TransportProvider;
			zone2Provider.TP_R9_ZoneHubLocation = vinhCity.PK;

			Factory.Save();

			AssertNull("Multiple match using postcode, return null", DeliveryDueDateCalculationHelper.GetRelatedTransportZoneOwner("1500", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.VietNam, Factory));
			AssertNull("Multiple match using city with postcode, return null", DeliveryDueDateCalculationHelper.GetRelatedTransportZoneOwner("3001", "Vinh", ZString.Empty, Core.Constants.CountryCodes.VietNam, Factory));
			AssertNull("Multiple match using state, return null", DeliveryDueDateCalculationHelper.GetRelatedTransportZoneOwner("5000", ZString.Empty, "49", Core.Constants.CountryCodes.VietNam, Factory));
			AssertNull("Multiple match using country, return null", DeliveryDueDateCalculationHelper.GetRelatedTransportZoneOwner("5000", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.VietNam, Factory));

			zone2Provider.TP_R9_ZoneHubLocation = ZGuid.Empty;
			zone2Provider.TP_RN_NKCountry = Core.Constants.CountryCodes.VietNam;

			Factory.Save();
			AssertNull("Multiple match using postcode, return null", DeliveryDueDateCalculationHelper.GetRelatedTransportZoneOwner("1500", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.VietNam, Factory));
			AssertProvider("City match with related postcode","3001", "Vinh", ZString.Empty, Core.Constants.CountryCodes.VietNam);
			AssertProvider("City match with name", "5000", "Dalat", ZString.Empty, Core.Constants.CountryCodes.VietNam);
			AssertNull("City match without related postcode or name, return null", DeliveryDueDateCalculationHelper.GetRelatedTransportZoneOwner("5000", "", ZString.Empty, Core.Constants.CountryCodes.VietNam, Factory));
			AssertProvider("State match", "5000", ZString.Empty, "49", Core.Constants.CountryCodes.VietNam);
			AssertNull("Multiple match using country, return null", DeliveryDueDateCalculationHelper.GetRelatedTransportZoneOwner("5000", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.VietNam, Factory));
		}

		IDocAddress GetIDocAddressMock(string postcode, string city, string country)
		{
			var docAddressMock = new Mock<IDocAddress>();
			docAddressMock.SetupGet(x => x.E2_Postcode).Returns(postcode);
			docAddressMock.SetupGet(x => x.E2_City).Returns(city);
			docAddressMock.SetupGet(x => x.E2_RN_NKCountryCode).Returns(country);

			return docAddressMock.Object;
		}

		public void TestGetNextDayOfWeek()
		{
			var tuesday = new ZDateTime(2023, 9, 26);
			AssertEquals("next monday", new ZDate(2023, 10, 2), DeliveryDueDateCalculationHelper.GetNextDayOfWeek(tuesday, DayOfWeek.Monday));
			AssertEquals("same day", new ZDate(2023, 9, 26), DeliveryDueDateCalculationHelper.GetNextDayOfWeek(tuesday, DayOfWeek.Tuesday));
			AssertEquals("next wednesday", new ZDate(2023, 9, 27), DeliveryDueDateCalculationHelper.GetNextDayOfWeek(tuesday, DayOfWeek.Wednesday));
			AssertEquals("next thursday", new ZDate(2023, 9, 28), DeliveryDueDateCalculationHelper.GetNextDayOfWeek(tuesday, DayOfWeek.Thursday));
			AssertEquals("next friday", new ZDate(2023, 9, 29), DeliveryDueDateCalculationHelper.GetNextDayOfWeek(tuesday, DayOfWeek.Friday));
			AssertEquals("next saturday", new ZDate(2023, 9, 30), DeliveryDueDateCalculationHelper.GetNextDayOfWeek(tuesday, DayOfWeek.Saturday));
			AssertEquals("next sunday", new ZDate(2023, 10, 1), DeliveryDueDateCalculationHelper.GetNextDayOfWeek(tuesday, DayOfWeek.Sunday));
		}

		public void TestGetMatchedTimetable()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var tuesday9am = new ZDateTime(2023, 11, 7, 9, 0, 0);
			var tuesday1am = new ZDateTime(2023, 11, 7, 1, 0, 0);

			AssertNotNull("Match time within operating hours", DeliveryDueDateCalculationHelper.GetMatchedTimetable(tuesday9am, orgAddress, "DLV"));
			AssertNull("Not match time outside operating hours", DeliveryDueDateCalculationHelper.GetMatchedTimetable(tuesday1am, orgAddress, "DLV"));
			AssertNotNull("Match time outside operating hours if ignoring time", DeliveryDueDateCalculationHelper.GetMatchedTimetable(tuesday1am, orgAddress, "DLV", ignoreTime: true));
		}

		public void TestAdjustTimeToDeliverDueTime_DeliveryDueTimeAfterClosingHourAndNextDayIsNonWorkingDay_NotSkipDate()
		{
			var initialDateTime = new ZDateTime(2024, 10, 11, 17, 0, 0);
			var calculationLogBuilder = new ZStringBuilder();
			var deliveryDueTime = new DeliveryDueTime(new ZDateTime(1900, 1, 1, 20, 0, 0).TimeOfDay, DeliveryDueTimeSource.ServiceLevel);
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			AssertEquals(DayOfWeek.Saturday, initialDateTime.AddDays(1).DayOfWeek);

			var result = DeliveryDueDateCalculationHelper.AdjustTimeToDeliverDueTime(initialDateTime, calculationLogBuilder, deliveryDueTime, new CalendarDayTypeProvider(), deliveryAddress);
			AssertEquals(new ZDateTime(2024, 10, 11, 20, 0, 0), result);
		}

		public void TestAdjustTimeToDeliverDueTime_AdjustedTimeIsWeekendDay_ApplyNextMonday()
		{
			var initialDateTime = new ZDateTime(2024, 10, 12, 9, 0, 0);
			var calculationLogBuilder = new ZStringBuilder();
			var deliveryDueTime = new DeliveryDueTime(new ZDateTime(1900, 1, 1, 9, 0, 0).TimeOfDay, DeliveryDueTimeSource.ServiceLevel);
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			AssertEquals(DayOfWeek.Saturday, initialDateTime.DayOfWeek);

			var result = DeliveryDueDateCalculationHelper.AdjustTimeToDeliverDueTime(initialDateTime, calculationLogBuilder, deliveryDueTime, new CalendarDayTypeProvider(), deliveryAddress);
			AssertEquals(new ZDateTime(2024, 10, 14, 9, 0, 0), result);
			AssertEquals(@"Delivery Address Weekend Days/Public Holidays: Saturday 12-Oct-24; Sunday 13-Oct-24; 
12-Oct-24 09:00:00 adjusted to 14-Oct-24 09:00:00 because of Delivery Address Weekends/Public Holidays
Delivery due time adjusted from 12-Oct-24 09:00:00 to 14-Oct-24 09:00:00 based on ServiceLevel's configured delivery due time
", calculationLogBuilder.ToString());
		}

		public void TestAdjustTimeToDeliverDueTime_InitialTimeEqualFinalAdjustDateTime_NoLogAboutAdjust()
		{
			var initialDateTime = new ZDateTime(2024, 10, 10, 17, 0, 0);
			var calculationLogBuilder = new ZStringBuilder();
			var deliveryDueTime = new DeliveryDueTime(new ZDateTime(1900, 1, 1, 11, 0, 0).TimeOfDay, DeliveryDueTimeSource.ServiceLevel);
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			var result = DeliveryDueDateCalculationHelper.AdjustTimeToDeliverDueTime(initialDateTime, calculationLogBuilder, deliveryDueTime, new CalendarDayTypeProvider(), deliveryAddress);

			AssertEquals(new ZDateTime(2024, 10, 11, 11, 0, 0), result);
			AssertContains("Delivery due time adjusted from 10-Oct-24 17:00:00 to 11-Oct-24 11:00:00 based on ServiceLevel's configured delivery due time", calculationLogBuilder.ToString());

			calculationLogBuilder = new ZStringBuilder();
			deliveryDueTime = new DeliveryDueTime(new ZDateTime(1900, 1, 1, 17, 0, 0).TimeOfDay, DeliveryDueTimeSource.ServiceLevel);
			result = DeliveryDueDateCalculationHelper.AdjustTimeToDeliverDueTime(initialDateTime, calculationLogBuilder, deliveryDueTime, new CalendarDayTypeProvider(), deliveryAddress);

			AssertEquals(new ZDateTime(2024, 10, 10, 17, 0, 0), result);
			AssertNotContains("Delivery due time adjusted from 10-Oct-24 17:00:00 to 10-Oct-24 17:00:00 based on ServiceLevel's configured delivery due time", calculationLogBuilder.ToString());
			AssertEquals(ZString.Empty, calculationLogBuilder.ToString());
		}

		public void TestOrgAddressShouldNotSkipFindingOpeningHours_ForWeekday()
		{
			var friday9Am = new ZDateTime(2025, 2, 7, 9, 0, 0);
			var result = ExecuteOrgAddressShouldSkipFindingOpeningHours(friday9Am, false, false, false);
			AssertEquals("Should not skip finding opening hours for weekday", false, result);
		}

		public void TestOrgAddressShouldNotSkipFindingOpeningHours_ForWeekday_Pickup()
		{
			var friday9Am = new ZDateTime(2025, 2, 7, 9, 0, 0);
			var result = ExecuteOrgAddressShouldSkipFindingOpeningHours(friday9Am, false, false, false, isTypePickup: true);
			AssertEquals("Should not skip finding opening hours for weekday pickup", false, result);
		}

		public void TestOrgAddressShouldNotSkipFindingOpeningHours_ForWeekend_Pickup()
		{
			var saturday9Am = new ZDateTime(2025, 2, 1, 9, 0, 0);
			var result = ExecuteOrgAddressShouldSkipFindingOpeningHours(saturday9Am, false, false, false, isTypePickup: true);
			AssertEquals("Should not skip finding opening hours for weekend pickup", false, result);
		}

		public void TestOrgAddressShouldSkipFindingOpeningHours_WithArrivalTime_BeforeOpening()
		{
			var tuesday6Am = new ZDateTime(2025, 2, 4, 6, 0, 0);
			var result = ExecuteOrgAddressShouldSkipFindingOpeningHours(tuesday6Am, true, false, false);
			AssertEquals("Should skip finding opening hours when arrival time is set before CFS opening time", true, result);
		}

		public void TestOrgAddressShouldSkipFindingOpeningHours_WithArrivalTime_BeforeClosing()
		{
			var tuesday10Am = new ZDateTime(2025, 2, 4, 10, 0, 0);
			var result = ExecuteOrgAddressShouldSkipFindingOpeningHours(tuesday10Am, true, false, false);
			AssertEquals("Should skip finding opening hours when arrival time is set before CFS closing time", true, result);
		}

		public void TestOrgAddressShouldNotSkipFindingOpeningHours_WithArrivalTime_AfterClosing()
		{
			var tuesday8Pm = new ZDateTime(2025, 2, 4, 20, 0, 0);
			var result = ExecuteOrgAddressShouldSkipFindingOpeningHours(tuesday8Pm, true, false, false);
			AssertEquals("Should not skip finding opening hours when arrival time is set after CFS closing time", false, result);
		}

		public void TestOrgAddressShouldNotSkipFindingOpeningHours_WithArrivalTime_OnWeekend()
		{
			var saturday10Am = new ZDateTime(2025, 2, 1, 10, 0, 0);
			var result = ExecuteOrgAddressShouldSkipFindingOpeningHours(saturday10Am, true, false, false);
			AssertEquals("Should not skip finding opening hours when arrival time is set when CFS closed on weekend", false, result);
		}

		public void TestOrgAddressShouldNotSkipFindingOpeningHours_WithArrivalTime_OnPublicHoliday()
		{
			var holidays = new ZDateTime[] { new ZDateTime(2025, 2, 4) };
			var tuesday10Am = new ZDateTime(2025, 2, 4, 10, 0, 0);
			var result = ExecuteOrgAddressShouldSkipFindingOpeningHours(tuesday10Am, true, false, false, holidays: holidays);
			AssertEquals("Should not skip finding opening hours when arrival time is set on public holiday", false, result);
		}

		public void TestOrgAddressShouldNotSkipFindingOpeningHours_ForPublicHoliday()
		{
			var holidays = new ZDateTime[] { new ZDateTime(2025, 2, 7) };
			var friday9Am = new ZDateTime(2025, 2, 7, 9, 0, 0);
			var result = ExecuteOrgAddressShouldSkipFindingOpeningHours(friday9Am, false, false, false, holidays: holidays);
			AssertEquals("Should not skip finding opening hours for public holiday", false, result);
		}

		public void TestOrgAddressShouldNotSkipFindingOpeningHours_ForPublicHoliday_WithDeliverOnWeekend()
		{
			var holidays = new ZDateTime[] { new ZDateTime(2025, 2, 7) };
			var friday9Am = new ZDateTime(2025, 2, 7, 9, 0, 0);
			var result = ExecuteOrgAddressShouldSkipFindingOpeningHours(friday9Am, false, true, false, holidays: holidays);
			AssertEquals("Should not skip finding opening hours for public holiday when DeliverOnWeekend is set", false, result);
		}

		public void TestOrgAddressShouldNotSkipFindingOpeningHours_ForWeekend()
		{
			var saturday9Am = new ZDateTime(2025, 2, 1, 9, 0, 0);
			var result = ExecuteOrgAddressShouldSkipFindingOpeningHours(saturday9Am, false, false, false);
			AssertEquals("Should not skip finding opening hours for weekend", false, result);
		}

		public void TestOrgAddressShouldSkipFindingOpeningHours_ForWeekend_WithDeliverOnWeekend()
		{
			var saturday9Am = new ZDateTime(2025, 2, 1, 9, 0, 0);
			var result = ExecuteOrgAddressShouldSkipFindingOpeningHours(saturday9Am, false, true, false);
			AssertEquals("Should skip finding opening hours for weekend when DeliverOnWeekend is set", true, result);
		}

		public void TestOrgAddressShouldNotSkipFindingOpeningHours_WithIsXtoCFS_ForWeekend_WithDeliverOnWeekend()
		{
			var saturday9Am = new ZDateTime(2025, 2, 1, 9, 0, 0);
			var result = ExecuteOrgAddressShouldSkipFindingOpeningHours(saturday9Am, false, true, true);
			AssertEquals("Should not skip finding opening hours for weekend when DeliverOnWeekend is set", false, result);
		}

		public void TestOrgAddressShouldSkipFindingOpeningHours_WithArrivalTime_BeforeClosing_Weekday()
		{
			var tuesday10Am = new ZDateTime(2025, 2, 4, 10, 0, 0);
			(var orgAddress, var calendarDayTypeProvider) = CreateTestOrgAddress();

			orgAddress.SetTimetablesRangeType(OrgTimeTableRangeType.Weekday);

			orgAddress.Timetables.DeleteAll();
			var timetable = orgAddress.Timetables.AddNew();
			timetable.OTT_IsForAllWeekDays = true;
			timetable.OTT_Type = OrgTimetableType.Codes.Deliver;
			timetable.OTT_TimeFrom = new DateTime(1900, 1, 1, 9, 0, 0);
			timetable.OTT_TimeTo = new DateTime(1900, 1, 1, 14, 0, 0);

			var result = DeliveryDueDateCalculationHelper.ShouldSkipFindingOpeningHours(Factory, calendarDayTypeProvider, orgAddress, OrgTimetableType.Codes.Deliver, tuesday10Am, true, false, false, new ZStringBuilder());
			AssertEquals("Should skip finding opening hours when arrival time is set before CFS closing time", true, result);
		}

		public void TestOrgAddressShouldSkipFindingOpeningHours_WithArrivalTime_BeforeClosing_Advanced()
		{
			var tuesday10Am = new ZDateTime(2025, 2, 4, 10, 0, 0);
			(var orgAddress, var calendarDayTypeProvider) = CreateTestOrgAddress();

			orgAddress.Timetables.allowDeleteLastTimeTable = true;
			orgAddress.Timetables.DeleteAll();

			orgAddress.SetTimetablesRangeType(OrgTimeTableRangeType.Advanced);

			DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Deliver, 9, 0, 17, 0, isAdvanced: true, advancedDay: "TUE", processingTime: 60, default);

			var result = DeliveryDueDateCalculationHelper.ShouldSkipFindingOpeningHours(Factory, calendarDayTypeProvider, orgAddress, OrgTimetableType.Codes.Deliver, tuesday10Am, true, false, false, new ZStringBuilder());
			AssertEquals("Should skip finding opening hours when arrival time is set before CFS closing time", true, result);
		}

		public void TestOrgAddressShouldNotSkipFindingOpeningHours_WithArrivalTime_AfterClosing_Weekday()
		{
			var tuesday8Pm = new ZDateTime(2025, 2, 4, 16, 0, 0);
			(var orgAddress, var calendarDayTypeProvider) = CreateTestOrgAddress();

			orgAddress.SetTimetablesRangeType(OrgTimeTableRangeType.Weekday);

			orgAddress.Timetables.DeleteAll();

			// Delivery time is set to close at 1400
			var timetable = orgAddress.Timetables.AddNew();
			timetable.OTT_IsForAllWeekDays = true;
			timetable.OTT_Type = OrgTimetableType.Codes.Deliver;
			timetable.OTT_TimeFrom = new DateTime(1900, 1, 1, 9, 0, 0);
			timetable.OTT_TimeTo = new DateTime(1900, 1, 1, 14, 0, 0);

			// Pickup time is set to close at 1700
			timetable = orgAddress.Timetables.AddNew();
			timetable.OTT_IsForAllWeekDays = true;
			timetable.OTT_Type = OrgTimetableType.Codes.Pickup;
			timetable.OTT_TimeFrom = new DateTime(1900, 1, 1, 9, 0, 0);
			timetable.OTT_TimeTo = new DateTime(1900, 1, 1, 17, 0, 0);

			var result = DeliveryDueDateCalculationHelper.ShouldSkipFindingOpeningHours(Factory, calendarDayTypeProvider, orgAddress, OrgTimetableType.Codes.Deliver, tuesday8Pm, true, false, false, new ZStringBuilder());
			AssertEquals("Should not skip finding opening hours when arrival time is set after CFS closing time", false, result);
		}

		public void TestOrgAddressShouldNotSkipFindingOpeningHours_WithArrivalTime_AfterClosing_Advanced()
		{
			var tuesday8Pm = new ZDateTime(2025, 2, 4, 16, 0, 0);
			(var orgAddress, var calendarDayTypeProvider) = CreateTestOrgAddress();

			orgAddress.Timetables.allowDeleteLastTimeTable = true;
			orgAddress.Timetables.DeleteAll();

			orgAddress.SetTimetablesRangeType(OrgTimeTableRangeType.Advanced);

			DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Deliver, 9, 0, 14, 0, isAdvanced: true, advancedDay: "TUE", processingTime: 60, default);
			DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 9, 0, 17, 0, isAdvanced: true, advancedDay: "TUE", processingTime: 60, default);

			var result = DeliveryDueDateCalculationHelper.ShouldSkipFindingOpeningHours(Factory, calendarDayTypeProvider, orgAddress, OrgTimetableType.Codes.Deliver, tuesday8Pm, true, false, false, new ZStringBuilder());
			AssertEquals("Should not skip finding opening hours when arrival time is set after CFS closing time", false, result);
		}

		public void TestJobDocAddressShouldNotSkipFindingOpeningHours_ForWeekday()
		{
			var friday9Am = new ZDateTime(2025, 2, 7, 9, 0, 0);
			var result = ExecuteJobDocAddressShouldSkipFindingOpeningHours(friday9Am, false, false, false);
			AssertEquals("Should not skip finding opening hours for weekday", false, result);
		}

		public void TestJobAddressShouldSkipFindingOpeningHours_WithArrivalTime_BeforeClosing()
		{
			var tuesday10Am = new ZDateTime(2025, 2, 4, 10, 0, 0);
			var result = ExecuteJobDocAddressShouldSkipFindingOpeningHours(tuesday10Am, true, false, false);
			AssertEquals("Should skip finding opening hours when arrival time is set before CFS closing time", true, result);
		}

		public void TestJobAddressShouldNotSkipFindingOpeningHours_WithArrivalTime_AfterClosing()
		{
			var tuesday8Pm = new ZDateTime(2025, 2, 4, 20, 0, 0);
			var result = ExecuteJobDocAddressShouldSkipFindingOpeningHours(tuesday8Pm, true, false, false);
			AssertEquals("Should not skip finding opening hours when arrival time is set after CFS closing time", false, result);
		}

		public bool ExecuteOrgAddressShouldSkipFindingOpeningHours(ZDateTime date, bool useArrivalTime, bool deliverOnWeekend, bool isXtoCFS, bool isTypePickup = false, params ZDateTime[] holidays)
		{
			(var orgAddress, var calendarDayTypeProvider) = CreateTestOrgAddress(holidays);

			var operationType = isTypePickup ? OrgTimetableType.Codes.Pickup : OrgTimetableType.Codes.Deliver;
			var logger = new ZStringBuilder();
			return DeliveryDueDateCalculationHelper.ShouldSkipFindingOpeningHours(Factory, calendarDayTypeProvider, orgAddress, operationType, date, useArrivalTime, deliverOnWeekend, isXtoCFS, logger);
		}

		public bool ExecuteJobDocAddressShouldSkipFindingOpeningHours(ZDateTime date, bool useArrivalTime, bool deliverOnWeekend, bool isXtoCFS, bool isTypePickup = false, params ZDateTime[] holidays)
		{
			(var jobDocAddress, var calendarDayTypeProvider) = CreateTestJobDocAddress(holidays);

			var operationType = isTypePickup ? OrgTimetableType.Codes.Pickup : OrgTimetableType.Codes.Deliver;
			var logger = new ZStringBuilder();
			return DeliveryDueDateCalculationHelper.ShouldSkipFindingOpeningHours(Factory, calendarDayTypeProvider, jobDocAddress, operationType, date, useArrivalTime, deliverOnWeekend, isXtoCFS, logger);
		}

		(OrgAddress, CalendarDayTypeProvider) CreateTestOrgAddress(params ZDateTime[] holidays)
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "XX";
			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "YY";
			state.RW_RN_NKCountryCode = "XX";

			foreach (var holiday in holidays)
			{
				AddHoliday(state, holiday);
			}

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = org.Addresses[0];
			orgAddress.OA_RN_NKCountryCode = "XX";
			orgAddress.State = "YY";

			Factory.Save();

			return (orgAddress, new CalendarDayTypeProvider());
		}

		(JobDocAddress, CalendarDayTypeProvider) CreateTestJobDocAddress(params ZDateTime[] holidays)
		{
			(var orgAddress, var calendarDayTypeProvider) = CreateTestOrgAddress(holidays);

			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_OA_Address = orgAddress.PK;

			Factory.Save();

			return (jobDocAddress, calendarDayTypeProvider);
		}

		void AddHoliday(RefCountryStates state, ZDateTime holiday)
		{
			var glbHoliday = Factory.NewWithValidTestData<GlbHoliday>();
			glbHoliday.GH_Date = holiday;
			glbHoliday.GH_IsWorkingDay = false;
			glbHoliday.GH_Recurring = true;
			glbHoliday.GH_ParentID = state.PK;
			glbHoliday.GH_ParentTableCode = "RW";
			glbHoliday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;
			glbHoliday.GH_IsActive = true;

			Factory.Save();
		}
	}
}
