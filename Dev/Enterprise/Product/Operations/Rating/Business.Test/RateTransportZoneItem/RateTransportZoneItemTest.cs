using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateTransportZoneItem))]
	public class RateTransportZoneItemTest : EnterpriseBusinessObjectTestCase
	{
		#region Business logic

		public void TestFromPostCode_PreferActiveOverInactive()
		{
			var postCode1 = Factory.New<RefPostCode>();
			const string testPostCode = "XYZZY";
			postCode1.RK_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			postCode1.RK_CityTownPostCode = testPostCode;
			postCode1.RK_IsActive = true;

			var postCode2 = Factory.New<RefPostCode>();
			postCode2.RK_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			postCode2.RK_CityTownPostCode = testPostCode;
			postCode2.RK_IsActive = false;

			var prov = Factory.New<RateTransportProvider>();
			var zone = prov.Zones.AddNew();
			var zoneItem = zone.Items.AddNew();
			zoneItem.TQ_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			zoneItem.TQ_FromPostCode = testPostCode;
			var actualPostCode = zoneItem.FromPostCode;
			AssertEquals("prefer the active post code", true, actualPostCode.RK_IsActive);
			AssertEquals("prefer the active post code", postCode1.PK, actualPostCode.PK);

			// swap active
			postCode1.RK_IsActive = false;
			postCode2.RK_IsActive = true;
			actualPostCode = zoneItem.FromPostCode;
			AssertEquals("prefer the active post code", true, actualPostCode.RK_IsActive);
			AssertEquals("prefer the active post code", postCode2.PK, actualPostCode.PK);

			postCode2.RK_RN_NKCountry = Core.Constants.CountryCodes.NewZealand;
			actualPostCode = zoneItem.FromPostCode;
			AssertEquals("pick inactive if there is no active", false, actualPostCode.RK_IsActive);
			AssertEquals("pick inactive if there is no active", postCode1.PK, actualPostCode.PK);
		}

		public void TestPostCodeAndCityTownControlType()
		{
			var prov = Factory.New<RateTransportProvider>();
			prov.TP_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			var zone = prov.Zones.AddNew();
			var zoneItem = zone.Items.AddNew();
			var postCode = Factory.New<RefPostCode>();
			postCode.RK_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			postCode.RK_CityTownPostCode = "TEST2000";
			var cityTown = Factory.New<RefCityTown>();
			cityTown.R9_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			Assert(zoneItem.TQ_R9_CityTown.IsEmpty);
			Assert(zoneItem.TQ_FromPostCode.IsEmpty);
			AssertEquals(nameof(FieldType.TextCodeFindBox), zoneItem.PostCodeControlType);
			AssertEquals(nameof(FieldType.Guid), zoneItem.CityTownControlType);

			zoneItem.TQ_R9_CityTown = cityTown.PK;
			AssertEquals(nameof(FieldType.TextCodeFindBox), zoneItem.PostCodeControlType);
			zoneItem.TQ_FromPostCode = "TEST2000";
			AssertNotNull(zoneItem.FromPostCode);
			AssertEquals(nameof(FieldType.Guid), zoneItem.CityTownControlType);

			var pivot = Factory.New<RefCityPCodePivot>();
			pivot.R0_R9 = cityTown.PK;
			pivot.R0_RK = postCode.PK;
			AssertEquals(nameof(FieldType.TextDropEdit), zoneItem.PostCodeControlType);
			AssertEquals(nameof(FieldType.GuidDropEdit), zoneItem.CityTownControlType);
		}

		public void TestPostCodeAndCityTownReadOnly()
		{
			var prov = Factory.New<RateTransportProvider>();
			var zone = prov.Zones.AddNew();
			var zoneItem = zone.Items.AddNew();

			Assert(zoneItem.TQ_R9_CityTown.IsEmpty);
			Assert("ToPostCode is not readonly when city/town is not specified", !zoneItem.TQ_ToPostCode_ReadOnly);
			zoneItem.TQ_R9_CityTown = ZGuid.NewZGuid();
			Assert("ToPostCode is readonly when city/town is specified", zoneItem.TQ_ToPostCode_ReadOnly);

			Assert("City/Town is not readonly.", !zoneItem.TQ_R9_CityTown_ReadOnly);
			zoneItem.TQ_FromPostCode = "2000";
			Assert("City/Town is not readonly.", !zoneItem.TQ_R9_CityTown_ReadOnly);
			zoneItem.TQ_ToPostCode = "2000";
			Assert("City/Town is readonly when both from post code and to post code are specified.", zoneItem.TQ_R9_CityTown_ReadOnly);
		}

		public void TestPropertySynced_TQ_RN_NKCountry()
		{
			var prov = Factory.New<RateTransportProvider>();
			var zone = prov.Zones.AddNew();
			AssertEquals(ZString.Empty, prov.TP_RN_NKCountry);
			AssertNull(prov.ZoneHubLocation);

			var zoneItem = Factory.New<RateTransportZoneItem>();
			zoneItem.TQ_TZ_DomesticZone = zone.PK;
			AssertEquals(ZString.Empty, zoneItem.TQ_RN_NKCountry);

			prov = Factory.New<RateTransportProvider>();
			prov.TP_RN_NKCountry = "AU";
			zone = prov.Zones.AddNew();
			AssertNull(prov.ZoneHubLocation);

			zoneItem = Factory.New<RateTransportZoneItem>();
			zoneItem.TQ_TZ_DomesticZone = zone.PK;
			AssertEquals("AU", zoneItem.TQ_RN_NKCountry);

			var cityTown = Factory.New<RefCityTown>();
			cityTown.R9_RN_NKCountry = "ZZ";
			prov.TP_R9_ZoneHubLocation = cityTown.PK;

			zoneItem = Factory.New<RateTransportZoneItem>();
			zoneItem.TQ_TZ_DomesticZone = zone.PK;
			AssertEquals("ZZ", zoneItem.TQ_RN_NKCountry);

			prov = Factory.New<RateTransportProvider>();
			prov.TP_R9_ZoneHubLocation = cityTown.PK;
			zone = prov.Zones.AddNew();
			AssertEquals(ZString.Empty, prov.TP_RN_NKCountry);
			AssertNotNull(prov.ZoneHubLocation);

			zoneItem = Factory.New<RateTransportZoneItem>();
			zoneItem.TQ_TZ_DomesticZone = zone.PK;
			AssertEquals("ZZ", zoneItem.TQ_RN_NKCountry);
		}

		public void TestBeyondDaysAndHours_ShouldBeZero_WhenIsBeyondIsFalse()
		{
			var transportZoneItem = Factory.NewWithValidTestData<RateTransportZoneItem>();
			transportZoneItem.BeyondDays = 2;
			transportZoneItem.BeyondHours = 18;
			transportZoneItem.TQ_FromPostCode = "2234";
			transportZoneItem.TQ_FromDistance = 0;
			transportZoneItem.TQ_ToDistance = 0;
			transportZoneItem.TQ_IsBeyond = false;
			AssertEquals(0, transportZoneItem.TQ_BeyondHours);
			AssertEquals(0, transportZoneItem.BeyondHours);
			AssertEquals(0, transportZoneItem.BeyondDays);

			transportZoneItem.BeyondDays = 2;
			transportZoneItem.BeyondHours = 18;
			transportZoneItem.TQ_IsBeyond = true;
			AssertEquals(66, transportZoneItem.TQ_BeyondHours);
			AssertEquals(18, transportZoneItem.BeyondHours);
			AssertEquals(2, transportZoneItem.BeyondDays);

			transportZoneItem.TQ_IsBeyond = false;
			Factory.Save();

			transportZoneItem = new BusinessObjectFactory().Load<RateTransportZoneItem>(transportZoneItem.PK);
			AssertEquals(0, transportZoneItem.BeyondDays);
			AssertEquals(0, transportZoneItem.BeyondHours);
		}

		public void TestBeyondDaysAndHoursReadOnlyness()
		{
			var provider = Factory.New<RateTransportProvider>();
			provider.TP_RN_NKCountry = Core.Constants.CountryCodes.Indonesia;
			var zone = provider.Zones.AddNew();
			var zoneItem = zone.Items.AddNew();

			zoneItem.BeyondDays = 2;
			zoneItem.BeyondHours = 18;
			zoneItem.TQ_FromPostCode = "2234";
			zoneItem.TQ_FromDistance = 0;
			zoneItem.TQ_ToDistance = 0;
			zoneItem.TQ_IsBeyond = false;

			Assert(zoneItem.BeyondDaysInfo.ReadOnly);
			Assert(zoneItem.BeyondHoursInfo.ReadOnly);

			zoneItem.TQ_IsBeyond = true;

			Assert(!zoneItem.BeyondDaysInfo.ReadOnly);
			Assert(!zoneItem.BeyondHoursInfo.ReadOnly);
		}

		public void TestBeyondDaysAndHours()
		{
			var transportZoneItem = Factory.NewWithValidTestData<RateTransportZoneItem>();
			transportZoneItem.BeyondDays = 2;
			transportZoneItem.BeyondHours = 18;
			transportZoneItem.TQ_FromPostCode = "2234";
			transportZoneItem.TQ_FromDistance = 0;
			transportZoneItem.TQ_ToDistance = 0;

			AssertEquals(66, transportZoneItem.TQ_BeyondHours);

			transportZoneItem.TQ_BeyondHours = 83;
			Factory.Save();

			transportZoneItem = new BusinessObjectFactory().Load<RateTransportZoneItem>(transportZoneItem.PK);
			AssertEquals(3, transportZoneItem.BeyondDays);
			AssertEquals(11, transportZoneItem.BeyondHours);
		}

		public void TestTransitTimeFormatted()
		{
			var transportZoneItem = Factory.New<RateTransportZoneItem>();
			transportZoneItem.BeyondDays = 1;
			transportZoneItem.BeyondHours = 15;

			AssertEquals("1 day 15 hours", transportZoneItem.BeyondTimeFormatted);

			transportZoneItem.BeyondDays = 4;
			transportZoneItem.BeyondHours = 1;

			AssertEquals("4 days 1 hour", transportZoneItem.BeyondTimeFormatted);

			transportZoneItem.BeyondDays = 4;
			transportZoneItem.BeyondHours = 2;

			AssertEquals("4 days 2 hours", transportZoneItem.BeyondTimeFormatted);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public void TestBeyondDaysAndHours_WhenCalculatedValueOverflowed()
		{
			var transportZoneItem = Factory.NewWithValidTestData<RateTransportZoneItem>();

			Test(int.MaxValue, int.MaxValue, int.MaxValue);
			Test(int.MinValue, int.MinValue, int.MinValue);

			void Test(int beyondDays, int beyondHours, int expectedTQBeyondHours)
			{
				transportZoneItem.BeyondDays = beyondDays;
				transportZoneItem.BeyondHours = beyondHours;
				AssertEquals(expectedTQBeyondHours, transportZoneItem.TQ_BeyondHours);
			}
		}

		public override (ZDateTime AssignValue, ZDateTime AssertValue)[] GetValidZDateTimes(string propertyName)
		{
			if (propertyName == "TQ_DeliveryDueTime")
			{
				return new (ZDateTime AssignValue, ZDateTime AssertValue)[]
				{
					(new ZDateTime(1900, 1, 1, 8, 30, 0), new ZDateTime(1900, 1, 1, 8, 30, 0)),
					(new ZDateTime(1900, 1, 1, 23, 30, 0), new ZDateTime(1900, 1, 1, 23, 30, 0)),
					(new ZDateTime(1900, 1, 1, 0, 0, 0), new ZDateTime(1900, 1, 1, 0, 0, 0)),

					(ZDateTime.Invalid, ZDateTime.Invalid),
					(new ZDateTime(DateTime.MaxValue), new ZDateTime(1900, 1, 1, 23, 59, 59)),
					(new ZDateTime(DateTime.MinValue), new ZDateTime(DateTime.MinValue)),
					(ZDateTime.Empty, ZDateTime.Empty),
				};
			}
			else
			{
				return base.GetValidZDateTimes(propertyName);
			}
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var prov = Factory.NewWithValidTestData<RateTransportProvider>();
			var zone = prov.Zones.AddNew();
			return zone.Items.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var prov = factory.NewWithValidTestData<RateTransportProvider>();
			var zone = prov.Zones.AddNew();

			var item = zone.Items.AddNew();
			item.FillWithValidTestData();
			item.TQ_ToDistance = 10;

			return item;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var prov = Factory.NewWithValidTestData<RateTransportProvider>();
			var zone = prov.Zones.AddNew();
			var item = zone.Items.AddNew();
			item.FillWithValidTestData();
			item.TQ_ToDistance = 10;

			return item;
		}

		#endregion
	}
}
