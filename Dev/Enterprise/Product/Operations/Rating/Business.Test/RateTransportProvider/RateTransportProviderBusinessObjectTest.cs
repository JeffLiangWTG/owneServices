using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateTransportProvider))]
	public class RateTransportProviderBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<RateTransportProvider>();
		}

		public void TestIsZoneSetOwnerFieldVisible()
		{
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_OH_RelatedParty = GlbCompany.CurrentCompany.PK;
			provider.TP_ZoneType = RatingConstants.RatingZoneTypes.All;
			Assert("Owner field should be visible when zone type is not reporting", provider.IsZoneSetOwnerFieldVisible);
			provider.TP_ZoneType = RatingConstants.RatingZoneTypes.Operations;
			Assert("Owner field should be visible when zone type is not reporting", provider.IsZoneSetOwnerFieldVisible);
			provider.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			Assert("Owner field should be visible when zone type is not reporting", provider.IsZoneSetOwnerFieldVisible);
			provider.TP_ZoneType = RatingConstants.RatingZoneTypes.Reporting;
			Assert("Owner field should be invisible when zone type is reporting", !provider.IsZoneSetOwnerFieldVisible);
			Assert("Owner should set to empty when zone type is reporting", provider.TP_OH_RelatedParty.IsEmpty);
		}

		public void TestCityTownResetsCountry()
		{
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_RN_NKCountry = Core.Constants.CountryCodes.China;

			AssertEquals(Core.Constants.CountryCodes.China, provider.TP_RN_NKCountry);

			var city = Factory.NewWithValidTestData<RefCityTown>();
			city.R9_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;
			AssertNotEquals("Pre-condition 1: City/Town is not set to the provider.", city.PK, provider.TP_R9_ZoneHubLocation);

			provider.TP_R9_ZoneHubLocation = city.PK;

			AssertEquals("City/Town should have reset country", ZString.Empty, provider.TP_RN_NKCountry);
			AssertEquals("Provider's country should now default from town", Core.Constants.CountryCodes.UnitedKingdom, provider.CountryCode);

			city.R9_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("Pre-condition 2: The PK of the provider's ZoneHubLocation(City/Town) is not changed.", city.PK, provider.TP_R9_ZoneHubLocation);

			provider.TP_R9_ZoneHubLocation = city.PK;

			AssertEquals("City/Town should have reset country when it has country changed.", ZString.Empty, provider.TP_RN_NKCountry);
			AssertEquals("Provider's country should change when same City/Town has updated the country.", Core.Constants.CountryCodes.UnitedStates, provider.CountryCode);
		}

		public void TestState()
		{
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			var city = Factory.NewWithValidTestData<RefCityTown>();
			city.R9_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			city.R9_RW_NKState = "NSW";
			provider.TP_R9_ZoneHubLocation = city.PK;

			AssertEquals("NSW", provider.ZoneHubLocation.R9_RW_NKState);

			city.R9_RW_NKState = "ACT";

			AssertEquals("ACT", provider.ZoneHubLocation.R9_RW_NKState);
		}

		public void TestUniqueCode()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "GOLD";

			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_OH_RelatedParty = org.PK;
			provider.TP_ZoneType = RatingConstants.RatingZoneTypes.All;
			provider.TP_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			AssertEquals("GOLD-ALL-ALL-AU", provider.UniqueCode);

			provider.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;

			AssertEquals("GOLD-RAT-ALL-AU", provider.UniqueCode);

			var city = Factory.NewWithValidTestData<RefCityTown>();
			city.R9_RN_NKCountry = Core.Constants.CountryCodes.Ukraine;
			city.R9_InternationalName = "Lion";
			provider.TP_R9_ZoneHubLocation = city.PK;

			AssertEquals("GOLD-RAT-ALL-UA-LION", provider.UniqueCode);

			provider.TP_R9_ZoneHubLocation = ZGuid.Empty;

			AssertEquals("Expected to remove city/town name and remember last country", "GOLD-RAT-ALL-UA", provider.UniqueCode);
		}

		public void TestChildZonesReadOnlyAndActiveStatus()
		{
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_OH_RelatedParty = GlbCompany.CurrentCompany.PK;
			provider.TP_ZoneType = RatingConstants.RatingZoneTypes.All;

			var zone1 = provider.Zones.AddNew();
			var zone2 = provider.Zones.AddNew();
			Assert(zone1.TZ_IsActive);
			Assert(zone2.TZ_IsActive);
			Assert(!zone1.TZ_IsActive_ReadOnly);
			Assert(!zone2.TZ_IsActive_ReadOnly);
			Assert(!provider.Zones.ReadOnly);

			provider.TP_IsActive = false;
			Assert(!zone1.TZ_IsActive);
			Assert(!zone2.TZ_IsActive);
			Assert(zone1.TZ_IsActive_ReadOnly);
			Assert(provider.Zones.ReadOnly);
			Assert(zone2.TZ_IsActive_ReadOnly);
		}

		public void TestChildZoneItemsCountryAreDefaultedToTP_RN_NKCountry()
		{
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_OH_RelatedParty = GlbCompany.CurrentCompany.PK;
			provider.TP_ZoneType = RatingConstants.RatingZoneTypes.All;
			AssertEquals("AU", provider.TP_RN_NKCountry);

			var zoneItem1 = provider.Zones.AddNew().Items.AddNew();
			var zoneItem2 = provider.Zones.AddNew().Items.AddNew();
			AssertEquals("AU", zoneItem1.TQ_RN_NKCountry);
			AssertEquals("AU", zoneItem2.TQ_RN_NKCountry);
		}

		public void TestDefaultDeliveryDueTime()
		{
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_DefaultDeliveryDueTime = new ZDateTime(2015, 1, 1, 14, 0, 0);
			AssertEquals((ZDateTime)TimeSpan.FromHours(14), provider.TP_DefaultDeliveryDueTime);
		}

		public void TestDefaultHoldForPickupTime()
		{
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_DefaultHoldForPickupTime = new ZDateTime(2015, 1, 1, 14, 0, 0);
			AssertEquals((ZDateTime)TimeSpan.FromHours(14), provider.TP_DefaultHoldForPickupTime);
		}
	}
}
