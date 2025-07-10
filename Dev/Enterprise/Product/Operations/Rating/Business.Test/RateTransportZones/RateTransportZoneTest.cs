using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateTransportZone))]
	public class RateTransportZoneTest : EnterpriseBusinessObjectTestCase
	{
		#region Related Business Objects

		public void TestTransportProvider()
		{
			var provider = Factory.New<RateTransportProvider>();
			var zone = provider.Zones.AddNew();
			AssertEquals("Transport provider should return the dependent master", provider, zone.TransportProvider);
		}

		public void TestTZ_IsActive_ReadOnly()
		{
			var provider = Factory.New<RateTransportProvider>();
			var zone = provider.Zones.AddNew();
			provider.TP_IsActive = true;
			Assert(!zone.TZ_IsActive_ReadOnly);

			provider.TP_IsActive = false;
			Assert(zone.TZ_IsActive_ReadOnly);
		}

		public void TestItems()
		{
			var zone = Factory.New<RateTransportZone>();
			AssertEquals("Items not null and registered child editable", true, zone.IsRegisteredEditableChildObject(zone.Items));
		}

		#endregion

		#region Rate Updates

		public void TestRenameUpdatesRates()
		{
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			provider.TP_OH_RelatedParty = supplier.PK;
			provider.TP_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			var zone = provider.Zones.AddNew();
			zone.TZ_ZoneName = "Dom Zone";
			Factory.Save();

			var rate = Factory.NewWithValidTestData<ClientRate>();
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.SEA, "AUSYD", "USLAX");
			entry.TI_OH_Supplier = supplier.PK;

			var line = entry.AddRateLine("FRT", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			var calc = line.GetCalculator<CartageZoneDistanceCalculator>();

			AssertEquals("Standard + Dom Zone", 2, calc.CartageZones.Count);

			line.Calculator.AddRateLineItem("MAX", 0m, 500m);
			line.Calculator.AddRateLineItemWithZone("UNT", 0m, 30m, ZGuid.Empty);
			line.Calculator.AddRateLineItemWithZone("UNT", 0m, 50m, zone.PK);
			Factory.Save();

			zone.TZ_ZoneName = "Renamed Dom Zone";
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var reloadedLine = factory2.Load<RateLine>(line.PK);
			var reloadedCalc = reloadedLine.GetCalculator<CartageZoneDistanceCalculator>();

			AssertEquals("After Save, Standard + Dom Zone", 2, reloadedCalc.CartageZones.Count);
			AssertEquals("Should have a standard zone", "", reloadedCalc.CartageZones[0].ZoneName);
			AssertEquals("Should have a renamed zone", "Renamed Dom Zone", reloadedCalc.CartageZones[1].ZoneName);
		}

		#endregion

		#region Delete transport zone

		[TestDate(2015, 2, 13)]
		public void TestDeleteRateTransportZone_ReferencesRateEntry()
		{
			var orgHeader = Helper.NewOrgHeader();

			var zoneSetHeader = Helper.CreateRateTransportZoneSet(orgHeader, CountryCodes.Australia);

			var rateTransportZone = zoneSetHeader.CreateRateTransportZoneForTest("Zone 1");
			rateTransportZone.CreateRateTransportZoneItemForTest(0, 10);

			Helper.ChargeCodes.New("CART", "Cartage", CartageZoneDistanceCalculator.Code, ChargeCodeGroupList.Codes.Transport);

			Factory.Save();

			var clientRate = Helper.NewClientRate(orgHeader);
			var entry = clientRate.AddRateEntry("TRN", "LRO", "AU", "");
			entry.TI_RateStartDate = ZDate.Today.AddDays(-20);
			entry.TI_TZ_OriginZone = rateTransportZone.PK;

			entry.RateLines.RemoveAndDeleteAll();

			var line = entry.AddRateLine("CART", FlatCalculator.Code);
			line.GetCalculator<FlatCalculator>().BaseRate = 10;

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Assert(!rateTransportZone.CanDelete);

			const string errorMessage = "Transport zone Zone 1 is used by Client Rate's (TESTORG1) Rate Entry with Location: AU, Start date: 24/01/2015 and End Date: 13/08/2015.";
			AssertEquals(errorMessage, rateTransportZone.ReasonForNotAbleToDelete);
		}

		[TestDate(2015, 2, 13)]
		public void TestDeleteRateTransportZone_ReferencesRateLineItem()
		{
			var orgHeader = Helper.NewOrgHeader();

			var zoneSetHeader = Helper.CreateRateTransportZoneSet(orgHeader, CountryCodes.Australia);

			var rateTransportZone = zoneSetHeader.CreateRateTransportZoneForTest("Zone 1");
			rateTransportZone.CreateRateTransportZoneItemForTest(0, 10);

			Helper.ChargeCodes.New("CART", "Cartage", CartageZoneDistanceCalculator.Code, ChargeCodeGroupList.Codes.Transport);

			Factory.Save();

			var clientRate = Helper.NewClientRate(orgHeader);
			var entry = clientRate.AddRateEntry("TRN", "LRO", "AU", "");
			entry.TI_RateStartDate = ZDate.Today.AddDays(-20);

			entry.RateLines.RemoveAndDeleteAll();

			var line = entry.AddRateLine("CART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			line.ConversionFactor = new ConversionFactor(194m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			line.TL_RX_NKCurrency = "AUD";
			line.TL_WeightVolume = "KG";
			line.Calculator.EquipmentType = "PSL";
			line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0m, 10m, ZGuid.Empty);
			line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 20m, rateTransportZone.PK);

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Assert(!rateTransportZone.CanDelete);

			const string errorMessage = "Transport zone Zone 1 is used by CTZ calculator in Client Rate's (TESTORG1) Rate Entry with Location: AU, Start date: 24/01/2015 and End Date: 13/08/2015.";
			AssertEquals(errorMessage, rateTransportZone.ReasonForNotAbleToDelete);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<RateTransportProvider>().Zones.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var prov = factory.NewWithValidTestData<RateTransportProvider>();
			var zone = prov.Zones.AddNew();
			var item = zone.Items.AddNew();
			item.TQ_ToDistance = 10;

			return zone;
		}

		protected TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}
		TestHelper helper;

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		#endregion
	}
}
