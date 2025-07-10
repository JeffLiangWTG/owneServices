using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TransitTimeServiceLevelCombinationCollection))]
	public class TransitTimeServiceLevelCombinationCollectionTest : ActiveBusinessObjectCollectionTestCase<TransitTimeServiceLevelCombinationCollection>
	{
		public override void TestAddNew()
		{
			int initialCount = Collection.Count;
			TransitTimeServiceLevelCombinationView bizO1 = Collection.AddNew();
			TransitTimeServiceLevelCombinationView bizO2 = Collection.AddNew();
			bizO1.TSC_TransitHours = 10;
			bizO2.TSC_TransitHours = 1;

			AssertEquals("Collection count", initialCount + 2, Collection.Count);
			Assert("Contains new elements", Collection.Contains(bizO1));
			Assert("Contains new elements", Collection.Contains(bizO2));
		}

		public void TestFilterBusinessObjectDefaults()
		{
			var originZoneOwner = Factory.NewWithValidTestData<OrgHeader>();
			var destinationZoneOwner = Factory.NewWithValidTestData<OrgHeader>();

			var collection = new TransitTimeServiceLevelCombinationCollection(Factory, originZoneOwner, destinationZoneOwner, null, null, null);
			AssertEquals(ZString.Empty, collection.FilterBusinessObjectDefaults["Service Level:Property"].Value);
			AssertEquals(ZGuid.Empty, collection.FilterBusinessObjectDefaults["Origin Zone:Property"].Value);
			AssertEquals(ZGuid.Empty, collection.FilterBusinessObjectDefaults["Destination Zone:Property"].Value);
			AssertEquals(ZString.Empty, collection.FilterBusinessObjectDefaults["Mode:Property"].Value);
			AssertEquals(originZoneOwner.PK, collection.FilterBusinessObjectDefaults["Origin Zone Owner/Carrier:Property"].Value);
			AssertEquals(destinationZoneOwner.PK, collection.FilterBusinessObjectDefaults["Destination Zone Owner/Carrier:Property"].Value);

			collection = new TransitTimeServiceLevelCombinationCollection(Factory, null, null, null, null, null);
			AssertEquals(ZGuid.Empty, collection.FilterBusinessObjectDefaults["Origin Zone Owner/Carrier:Property"].Value);
			AssertEquals(ZGuid.Empty, collection.FilterBusinessObjectDefaults["Destination Zone Owner/Carrier:Property"].Value);
		}

		public void TestFilterByAddress()
		{
			var pickupCityTown = Factory.NewWithValidTestData<RefCityTown>();
			pickupCityTown.R9_InternationalName = "Perth";
			pickupCityTown.R9_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			var pickupAddress = Factory.NewWithValidTestData<JobDocAddress>();
			pickupAddress.E2_City = pickupCityTown.R9_InternationalName;
			pickupAddress.E2_RN_NKCountryCode = pickupCityTown.R9_RN_NKCountry;
			pickupAddress.E2_Postcode = ZString.Empty;

			var deliveryCityTown = Factory.NewWithValidTestData<RefCityTown>();
			deliveryCityTown.R9_InternationalName = "ALEXANDRIA";
			deliveryCityTown.R9_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			var deliveryAddress = Factory.NewWithValidTestData<JobDocAddress>();
			deliveryAddress.E2_City = deliveryCityTown.R9_InternationalName;
			deliveryAddress.E2_RN_NKCountryCode = deliveryCityTown.R9_RN_NKCountry;
			deliveryAddress.E2_Postcode = ZString.Empty;

			var collection = new TransitTimeServiceLevelCombinationCollection(Factory, null, null, pickupAddress, deliveryAddress, ZString.Empty);
			AssertEquals("Should not find any transit time.", 0, collection.Count);

			pickupAddress.E2_Postcode = "6006";
			deliveryAddress.E2_Postcode = "2015";

			var exportReceivingDepot = Factory.NewWithValidTestData<OrgHeader>();
			var originZone1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			((BusinessObject)((IRateTransportZone)originZone1).TransportProvider)[RateTransportProviderSchema.TP_OH_RelatedParty] = exportReceivingDepot.PK;

			var originZoneItem1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZoneItem>());
			originZoneItem1[RateTransportZoneItemSchema.TQ_TZ_DomesticZone] = originZone1.PK;
			originZoneItem1[RateTransportZoneItemSchema.TQ_R9_CityTown] = ZGuid.Empty;
			originZoneItem1[RateTransportZoneItemSchema.TQ_RN_NKCountry] = pickupCityTown.R9_RN_NKCountry;
			originZoneItem1[RateTransportZoneItemSchema.TQ_FromPostCode] = "6005";
			originZoneItem1[RateTransportZoneItemSchema.TQ_ToPostCode] = "6007";

			var originZone2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			var originZoneItem2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZoneItem>());
			originZoneItem2[RateTransportZoneItemSchema.TQ_TZ_DomesticZone] = originZone2.PK;
			originZoneItem2[RateTransportZoneItemSchema.TQ_R9_CityTown] = ZGuid.Empty;
			originZoneItem2[RateTransportZoneItemSchema.TQ_RN_NKCountry] = pickupCityTown.R9_RN_NKCountry;
			originZoneItem2[RateTransportZoneItemSchema.TQ_FromPostCode] = "6004";
			originZoneItem2[RateTransportZoneItemSchema.TQ_ToPostCode] = "6008";

			var originZone3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			var originZoneItem3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZoneItem>());
			originZoneItem3[RateTransportZoneItemSchema.TQ_TZ_DomesticZone] = originZone3.PK;
			originZoneItem3[RateTransportZoneItemSchema.TQ_R9_CityTown] = ZGuid.Empty;
			originZoneItem3[RateTransportZoneItemSchema.TQ_RN_NKCountry] = pickupCityTown.R9_RN_NKCountry;
			originZoneItem3[RateTransportZoneItemSchema.TQ_FromPostCode] = "6007";
			originZoneItem3[RateTransportZoneItemSchema.TQ_ToPostCode] = "6008";

			var importReleaseDepot = Factory.NewWithValidTestData<OrgHeader>();
			var destinationZone1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			((BusinessObject)((IRateTransportZone)destinationZone1).TransportProvider)[RateTransportProviderSchema.TP_OH_RelatedParty] = importReleaseDepot.PK;

			var destinationZoneItem1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZoneItem>());
			destinationZoneItem1[RateTransportZoneItemSchema.TQ_TZ_DomesticZone] = destinationZone1.PK;
			destinationZoneItem1[RateTransportZoneItemSchema.TQ_R9_CityTown] = deliveryCityTown.PK;
			destinationZoneItem1[RateTransportZoneItemSchema.TQ_RN_NKCountry] = deliveryCityTown.R9_RN_NKCountry;
			destinationZoneItem1[RateTransportZoneItemSchema.TQ_FromPostCode] = deliveryAddress.E2_Postcode;
			destinationZoneItem1[RateTransportZoneItemSchema.TQ_ToPostCode] = ZString.Empty;

			var destinationZone2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			var destinationZoneItem2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZoneItem>());
			destinationZoneItem2[RateTransportZoneItemSchema.TQ_TZ_DomesticZone] = destinationZone2.PK;
			destinationZoneItem2[RateTransportZoneItemSchema.TQ_R9_CityTown] = deliveryCityTown.PK;
			destinationZoneItem2[RateTransportZoneItemSchema.TQ_RN_NKCountry] = deliveryCityTown.R9_RN_NKCountry;
			destinationZoneItem2[RateTransportZoneItemSchema.TQ_FromPostCode] = deliveryAddress.E2_Postcode;
			destinationZoneItem2[RateTransportZoneItemSchema.TQ_ToPostCode] = ZString.Empty;

			var destinationZone3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			var destinationZoneItem3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZoneItem>());
			destinationZoneItem3[RateTransportZoneItemSchema.TQ_TZ_DomesticZone] = destinationZone3.PK;
			destinationZoneItem3[RateTransportZoneItemSchema.TQ_R9_CityTown] = deliveryCityTown.PK;
			destinationZoneItem3[RateTransportZoneItemSchema.TQ_RN_NKCountry] = deliveryCityTown.R9_RN_NKCountry;
			destinationZoneItem3[RateTransportZoneItemSchema.TQ_FromPostCode] = "6009";
			destinationZoneItem3[RateTransportZoneItemSchema.TQ_ToPostCode] = ZString.Empty;

			var transitTime1 = GetValidTransitTimeWithoutZones();
			transitTime1.RTT_TZ_OriginDomesticZone = originZone1.PK;
			transitTime1.RTT_TZ_DestinationDomesticZone = destinationZone1.PK;
			transitTime1.RTT_Mode = Core.Constants.RateMode.AIR;
			transitTime1.RTT_RS_NKServiceLevel = "STD";

			var transitTime2 = GetValidTransitTimeWithoutZones();
			transitTime2.RTT_TZ_OriginDomesticZone = originZone1.PK;
			transitTime2.RTT_TZ_DestinationDomesticZone = destinationZone2.PK;
			transitTime2.RTT_Mode = Core.Constants.RateMode.ALL;
			transitTime2.RTT_RS_NKServiceLevel = "STD";

			var transitTime3 = GetValidTransitTimeWithoutZones();
			transitTime3.RTT_TZ_OriginDomesticZone = originZone2.PK;
			transitTime3.RTT_TZ_DestinationDomesticZone = destinationZone1.PK;
			transitTime3.RTT_Mode = Core.Constants.RateMode.LSE;
			transitTime3.RTT_RS_NKServiceLevel = "STD";

			var transitTime4 = GetValidTransitTimeWithoutZones();
			transitTime4.RTT_TZ_OriginDomesticZone = originZone2.PK;
			transitTime4.RTT_TZ_DestinationDomesticZone = destinationZone2.PK;
			transitTime4.RTT_Mode = Core.Constants.RateMode.SEA;
			transitTime4.RTT_RS_NKServiceLevel = "STD";

			var transitTime5 = GetValidTransitTimeWithoutZones();
			transitTime5.RTT_TZ_OriginDomesticZone = originZone3.PK;
			transitTime5.RTT_TZ_DestinationDomesticZone = destinationZone3.PK;
			transitTime5.RTT_Mode = Core.Constants.RateMode.AIR;
			transitTime5.RTT_RS_NKServiceLevel = "STD";

			var serviceLevel1 = Factory.New<RefServiceLevel>();
			serviceLevel1.RS_Code = "SL1";
			serviceLevel1.RS_DefaultTransitHours = 10;

			var serviceLevel2 = Factory.New<RefServiceLevel>();
			serviceLevel2.RS_Code = "SL2";

			Factory.Save();

			collection = new TransitTimeServiceLevelCombinationCollection(Factory, null, null, pickupAddress, deliveryAddress, Core.Constants.TransportModes.Air);
			AssertContainsSameElementsInView("Should filter by all matching zones in the background.", new[] { transitTime1, transitTime2, transitTime3 }, collection);
			AssertCollectionContains("SL1", collection.Select(v => v.TSC_Code));
			AssertCollectionNotContains("SL2", collection.Select(v => v.TSC_Code));

			collection = new TransitTimeServiceLevelCombinationCollection(Factory, null, null, pickupAddress, deliveryAddress, Core.Constants.TransportModes.Sea);
			AssertContainsSameElementsInView("Should filter by all matching zones in the background.", new[] { transitTime2, transitTime4 }, collection);
			AssertCollectionContains("SL1", collection.Select(v => v.TSC_Code));
			AssertCollectionNotContains("SL2", collection.Select(v => v.TSC_Code));

			collection = new TransitTimeServiceLevelCombinationCollection(Factory, null, null, pickupAddress, deliveryAddress, Core.Constants.TransportModes.Road);
			AssertContainsSameElementsInView("Should filter by all matching zones in the background.", new[] { transitTime2 }, collection);
			AssertCollectionContains("SL1", collection.Select(v => v.TSC_Code));
			AssertCollectionNotContains("SL2", collection.Select(v => v.TSC_Code));

			collection = new TransitTimeServiceLevelCombinationCollection(Factory, exportReceivingDepot, importReleaseDepot, pickupAddress, deliveryAddress, Core.Constants.TransportModes.Air);
			AssertContainsSameElementsInView("Should only filter by zones that match the address when CFS is blank.", new[] { transitTime1 }, collection);
			AssertCollectionContains("SL1", collection.Select(v => v.TSC_Code));
			AssertCollectionNotContains("SL2", collection.Select(v => v.TSC_Code));
		}

		public void TestNotifications()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var domesticZone1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			((BusinessObject)((IRateTransportZone)domesticZone1).TransportProvider)[RateTransportProviderSchema.TP_OH_RelatedParty] = org1.PK;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var domesticZone2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			((BusinessObject)((IRateTransportZone)domesticZone2).TransportProvider)[RateTransportProviderSchema.TP_OH_RelatedParty] = org2.PK;

			var transitTime1 = GetValidTransitTimeWithoutZones();
			transitTime1.RTT_TZ_OriginDomesticZone = domesticZone2.PK;
			transitTime1.RTT_TZ_DestinationDomesticZone = domesticZone2.PK;
			transitTime1.RTT_RS_NKServiceLevel = "STD";

			var transitTime2 = GetValidTransitTimeWithoutZones();
			transitTime2.RTT_TZ_OriginDomesticZone = domesticZone1.PK;
			transitTime2.RTT_TZ_DestinationDomesticZone = domesticZone1.PK;
			transitTime2.RTT_RS_NKServiceLevel = "STD";

			var transitTime3 = GetValidTransitTimeWithoutZones();
			transitTime3.RTT_TZ_OriginDomesticZone = domesticZone2.PK;
			transitTime3.RTT_TZ_DestinationDomesticZone = domesticZone1.PK;
			transitTime3.RTT_RS_NKServiceLevel = "STD";

			var serviceLevel = Factory.New<RefServiceLevel>();
			serviceLevel.RS_Code = "SL1";
			serviceLevel.RS_DefaultTransitHours = 10;

			Factory.Save();

			var transitTimeView1 = Factory.LoadTop1<TransitTimeServiceLevelCombinationView>(new ZQuery(TransitTimeServiceLevelCombinationViewSchema.TSC_RTT, SQLComparisonOperator.Equal, transitTime1.PK));
			var transitTimeView2 = Factory.LoadTop1<TransitTimeServiceLevelCombinationView>(new ZQuery(TransitTimeServiceLevelCombinationViewSchema.TSC_RTT, SQLComparisonOperator.Equal, transitTime2.PK));
			var transitTimeView3 = Factory.LoadTop1<TransitTimeServiceLevelCombinationView>(new ZQuery(TransitTimeServiceLevelCombinationViewSchema.TSC_RTT, SQLComparisonOperator.Equal, transitTime3.PK));
			var transitTimeView4 = Factory.LoadTop1<TransitTimeServiceLevelCombinationView>(new ZQuery(TransitTimeServiceLevelCombinationViewSchema.TSC_Code, SQLComparisonOperator.Equal, "SL1"));

			var collection = (IActiveBusinessObjectCollection)new TransitTimeServiceLevelCombinationCollection(Factory, org1, org2, null, null, null);
			AssertContains($"A Service Level selected from here must have an Origin Owner/Carrier ({org1.OH_Code}).", collection.GetAllNotificationsWhenAdditionalFilterNotMet(transitTimeView1));
			AssertContains($"A Service Level selected from here must have a Destination Owner/Carrier ({org2.OH_Code}).", collection.GetAllNotificationsWhenAdditionalFilterNotMet(transitTimeView2));
			AssertContains($@"A Service Level selected from here must have an Origin Owner/Carrier ({org1.OH_Code}).
A Service Level selected from here must have a Destination Owner/Carrier ({org2.OH_Code}).", collection.GetAllNotificationsWhenAdditionalFilterNotMet(transitTimeView3));
			AssertNotContains($"A Service Level selected from here must have an Origin Owner/Carrier", collection.GetAllNotificationsWhenAdditionalFilterNotMet(transitTimeView4));
		}

		public void Test_ThereShouldBeAnErrorIf_TheUserTriesToSelectAGenericServiceLevel_WhenATransitTimeBetweenTheOriginAndDestinationZonesExists()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var domesticZone1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			((BusinessObject)((IRateTransportZone)domesticZone1).TransportProvider)[RateTransportProviderSchema.TP_OH_RelatedParty] = org1.PK;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var domesticZone2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			((BusinessObject)((IRateTransportZone)domesticZone2).TransportProvider)[RateTransportProviderSchema.TP_OH_RelatedParty] = org2.PK;

			var transitTime = GetValidTransitTimeWithoutZones();
			transitTime.RTT_TZ_OriginDomesticZone = domesticZone1.PK;
			transitTime.RTT_TZ_DestinationDomesticZone = domesticZone2.PK;
			transitTime.RTT_RS_NKServiceLevel = "BBB";

			var serviceLevel = Factory.New<RefServiceLevel>();
			serviceLevel.RS_Code = "BBB";
			serviceLevel.RS_DefaultTransitHours = 10;

			Factory.Save();

			var collection = new TransitTimeServiceLevelCombinationCollection(Factory, org1, org2, null, null, null);
			Assert("TransitTimeServiceLevelCombinationCollection should implement IFilterModuleExtraNotificationProvider", collection is IFilterModuleExtraNotificationProvider);

			var genericServiceLevelRow = collection.FirstOrDefault(v => v.PK == serviceLevel.PK);
			var notification = ((IFilterModuleExtraNotificationProvider)collection).GetExtraNotification(genericServiceLevelRow);

			AssertNotNull("An error should be returned", notification);
			AssertContains("This generic service level cannot be selected since a transit time between the origin and destination zones exists.", notification.Message);
		}

		public void Test_ThereShouldBeNoErrorsIf_TheUserTriesToSelectAGenericServiceLevel_AndATransitTimeWithUnrelatedOriginAndDestinationExists()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var domesticZone1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			((BusinessObject)((IRateTransportZone)domesticZone1).TransportProvider)[RateTransportProviderSchema.TP_OH_RelatedParty] = org1.PK;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var domesticZone2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			((BusinessObject)((IRateTransportZone)domesticZone2).TransportProvider)[RateTransportProviderSchema.TP_OH_RelatedParty] = org2.PK;

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var domesticZone3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			((BusinessObject)((IRateTransportZone)domesticZone3).TransportProvider)[RateTransportProviderSchema.TP_OH_RelatedParty] = org3.PK;

			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			var domesticZone4 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			((BusinessObject)((IRateTransportZone)domesticZone4).TransportProvider)[RateTransportProviderSchema.TP_OH_RelatedParty] = org4.PK;

			var transitTime = GetValidTransitTimeWithoutZones();
			transitTime.RTT_TZ_OriginDomesticZone = domesticZone1.PK;
			transitTime.RTT_TZ_DestinationDomesticZone = domesticZone2.PK;
			transitTime.RTT_RS_NKServiceLevel = "BBB";

			var serviceLevel = Factory.New<RefServiceLevel>();
			serviceLevel.RS_Code = "BBB";
			serviceLevel.RS_DefaultTransitHours = 10;

			Factory.Save();

			var collection = new TransitTimeServiceLevelCombinationCollection(Factory, org3, org4, null, null, null);
			var genericServiceLevelRow = collection.FirstOrDefault(v => v.PK == serviceLevel.PK);
			var notification = ((IFilterModuleExtraNotificationProvider)collection).GetExtraNotification(genericServiceLevelRow);

			AssertNull("No errors should be returned", notification);
		}

		#region Implementation

		protected override TransitTimeServiceLevelCombinationCollection GetCollectionToTest()
		{
			return new TransitTimeServiceLevelCombinationCollection(Factory, null, null, null, null, null);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var transitTimeServiceLevelCombinationView = Factory.New<TransitTimeServiceLevelCombinationView>();
			transitTimeServiceLevelCombinationView.TSC_TransitHours = 10;
			return transitTimeServiceLevelCombinationView;
		}

		void AssertContainsSameElementsInView(string message, RefTransitTime[] transitTimes, TransitTimeServiceLevelCombinationCollection views)
		{
			AssertContainsExactElementsInAnyOrder(message, transitTimes.Select(t => t.PK), views.ToArray().Where(v => !v.TSC_RTT.IsEmpty).Select(v => v.TSC_RTT));
		}

		RefTransitTime GetValidTransitTimeWithoutZones()
		{
			var transitTime = Factory.NewWithValidTestData<RefTransitTime>();
			transitTime.RTT_TZ_OriginDomesticZone = ZGuid.Empty;
			transitTime.RTT_FZ_OriginInternationalZone = ZGuid.Empty;
			transitTime.RTT_TZ_DestinationDomesticZone = ZGuid.Empty;
			transitTime.RTT_FZ_DestinationInternationalZone = ZGuid.Empty;
			return transitTime;
		}

		#endregion
	}
}
