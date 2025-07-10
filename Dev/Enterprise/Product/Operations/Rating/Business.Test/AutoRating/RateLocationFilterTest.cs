using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Rating.Business.AutoRating.Testing
{
	public class RateLocationFilterTest : RatingTestCase
	{
		[ExpectNoExceptions]
		public void TestAddressIsWithinZoneCheckAddressForNull()
		{
			var ratingHeader = Factory.NewWithValidTestData<ClientRate>();
			var rateEnry = ratingHeader.ORGRateEntriesForBinding.AddNew();
			rateEnry.TI_TZ_OriginZone = ZGuid.NewZGuid();

			var criteria = new TestRatingCriteria();
			AssertNull("Prerequisite: ConsignorPickupAddress should be null", criteria.PickupAddress);

			RateLocationFilter.Apply(new List<IRateEntry> { rateEnry }, criteria);
		}

		[ExpectNoExceptions]
		public void TestAddressIsWithinSuburbCheckAddressForNull()
		{
			var ratingHeader = Factory.NewWithValidTestData<ClientRate>();
			var rateEntry = ratingHeader.ORGRateEntriesForBinding.AddNew();
			var suburb = Factory.New<RefCityTown>();
			rateEntry.OriginSuburbPK = suburb.PK;

			var criteria = new TestRatingCriteria();
			AssertNull("Prerequisite: ConsignorPickupAddress should be null", criteria.PickupAddress);
			AssertNull("Prerequisite: ConsignorDeliveryAddress should be null", criteria.DeliveryAddress);

			RateLocationFilter.Apply(new List<IRateEntry> { rateEntry }, criteria);
		}

		[ExpectNoExceptions]
		public void TestAllZonesAreActive()
		{
			var ratingHeader = Factory.NewWithValidTestData<ClientRate>();
			var rateTransportProvider = Factory.NewWithValidTestData<RateTransportProvider>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			rateTransportProvider.TP_OH_RelatedParty = orgHeader.PK;
			rateTransportProvider.TP_ZoneType = RatingConstants.RatingZoneTypes.All;

			var zoneFrom = rateTransportProvider.Zones.AddNew();
			zoneFrom.TZ_ZoneName = "ZoneFrom";
			var zoneTo = rateTransportProvider.Zones.AddNew();
			zoneTo.TZ_ZoneName = "ZoneTo";

			var tbcRateEntry = ratingHeader.TBCRateEntriesForBinding.AddNew();
			tbcRateEntry.TI_OH_TransportProvider = orgHeader.PK;
			tbcRateEntry.TI_TZ_OriginZone = zoneFrom.PK;
			tbcRateEntry.TI_TZ_DestinationZone = zoneTo.PK;

			var zoneOrigin = Factory.New<RefZoneHeader>();
			zoneOrigin.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.All;
			zoneOrigin.FZ_Code = "XUZH";
			zoneOrigin.FZ_Description = "XUZH";

			var zoneDestination = Factory.New<RefZoneHeader>();
			zoneDestination.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.All;
			zoneDestination.FZ_Code = "PIZH";
			zoneDestination.FZ_Description = "PIZH";

			var airRateEntry = ratingHeader.SummaryRateEntries.AddNew();
			airRateEntry.TI_RateCategory = RatingConstants.RateCategory.AIR;
			airRateEntry.TI_OriginLRC = "XUZH";
			airRateEntry.TI_DestinationLRC = "PIZH";

			var entries = new List<IRateEntry> { tbcRateEntry, airRateEntry };

			var ratingCriteria = new TestRatingCriteria();
			RateLocationFilter.Apply(entries, ratingCriteria);
			AssertContainsExactElementsInAnyOrder(new[] { airRateEntry, tbcRateEntry }, entries);

			zoneOrigin.FZ_IsActive = false;
			RateLocationFilter.Apply(entries, ratingCriteria);
			AssertContainsExactElementsInAnyOrder(new[] { tbcRateEntry }, entries);

			entries.Add(airRateEntry);
			AssertContainsExactElementsInAnyOrder(new[] { airRateEntry, tbcRateEntry }, entries);

			zoneOrigin.FZ_IsActive = true;
			zoneDestination.FZ_IsActive = false;
			RateLocationFilter.Apply(entries, ratingCriteria);
			AssertContainsExactElementsInAnyOrder(new[] { tbcRateEntry }, entries);

			entries.Add(airRateEntry);
			AssertContainsExactElementsInAnyOrder(new[] { airRateEntry, tbcRateEntry }, entries);

			zoneOrigin.FZ_IsActive = true;
			zoneDestination.FZ_IsActive = true;
			zoneFrom.TZ_IsActive = false;
			RateLocationFilter.Apply(entries, ratingCriteria);
			AssertContainsExactElementsInAnyOrder(new[] { airRateEntry }, entries);

			entries.Add(tbcRateEntry);
			AssertContainsExactElementsInAnyOrder(new[] { airRateEntry, tbcRateEntry }, entries);

			zoneFrom.TZ_IsActive = true;
			zoneTo.TZ_IsActive = false;
			RateLocationFilter.Apply(entries, ratingCriteria);
			AssertContainsExactElementsInAnyOrder(new[] { airRateEntry }, entries);

			entries.Add(tbcRateEntry);
			AssertContainsExactElementsInAnyOrder(new[] { airRateEntry, tbcRateEntry }, entries);

			zoneOrigin.FZ_IsActive = false;
			zoneFrom.TZ_IsActive = false;
			RateLocationFilter.Apply(entries, ratingCriteria);
			AssertEquals(0, entries.Count);
		}

		IDocAddress GetAddress(string postcode)
		{
			var mock = new Mock<IDocAddress>();
			mock.SetupGet(x => x.E2_City).Returns("potato");
			mock.SetupGet(x => x.E2_State).Returns("potato");
			mock.SetupGet(x => x.E2_Postcode).Returns(postcode);

			return mock.Object;
		}

		public void TestFilterByZones()
		{
			var ratingHeader = Factory.NewWithValidTestData<ClientRate>();
			var rateTransportProvider = Factory.NewWithValidTestData<RateTransportProvider>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			rateTransportProvider.TP_OH_RelatedParty = orgHeader.PK;
			rateTransportProvider.TP_ZoneType = RatingConstants.RatingZoneTypes.All;

			var zoneFrom = rateTransportProvider.Zones.AddNew();
			zoneFrom.TZ_ZoneName = "ZoneFrom";

			var zoneTo = rateTransportProvider.Zones.AddNew();
			zoneTo.TZ_ZoneName = "ZoneTo";

			var incorrectZone = rateTransportProvider.Zones.AddNew();
			incorrectZone.TZ_ZoneName = "IncorrectZone";

			var zoneitem = zoneFrom.Items.AddNew();
			zoneitem.TQ_FromPostCode = "1";
			zoneitem.TQ_ToPostCode = "5";

			var zoneitem2 = zoneTo.Items.AddNew();
			zoneitem2.TQ_FromPostCode = "1";
			zoneitem2.TQ_ToPostCode = "5";

			var cityTown = Factory.New<RefCityTown>();
			cityTown.R9_InternationalName = "notpotato";

			var correctZoneEntry = ratingHeader.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "JM", "WAR", 100);
			correctZoneEntry.TI_TZ_OriginZone = zoneFrom.PK;
			correctZoneEntry.TI_TZ_DestinationZone = zoneTo.PK;

			var incorrectOriginZoneEntry = ratingHeader.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "JM", "FRT", 101);
			incorrectOriginZoneEntry.TI_TZ_OriginZone = incorrectZone.PK;
			incorrectOriginZoneEntry.TI_TZ_DestinationZone = zoneTo.PK;

			var incorrectDestinationZoneEntry = ratingHeader.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "JM", "FRT", 101);
			incorrectDestinationZoneEntry.TI_TZ_OriginZone = zoneFrom.PK;
			incorrectDestinationZoneEntry.TI_TZ_DestinationZone = incorrectZone.PK;

			var criteria = new TestRatingCriteria();
			criteria.PickupAddress = GetAddress(postcode: "2");
			criteria.DeliveryAddress = GetAddress(postcode: "3");

			var entrys = new List<IRateEntry> { correctZoneEntry, incorrectOriginZoneEntry, incorrectDestinationZoneEntry };
			RateLocationFilter.FilterRateEntriesByOriginAndDestinationZones(entrys, criteria);
			AssertEquals("Rate with incorrect origin zone is filtered", 1, entrys.Count);
			AssertEquals("Should be correct rate left", correctZoneEntry.PK, entrys[0].PK);
		}

		public void TestFilterOrderOfOperations()
		{
			var ratingHeader = Factory.NewWithValidTestData<ClientRate>();
			var rateTransportProvider = Factory.NewWithValidTestData<RateTransportProvider>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			rateTransportProvider.TP_OH_RelatedParty = orgHeader.PK;
			rateTransportProvider.TP_ZoneType = RatingConstants.RatingZoneTypes.All;

			var zoneFrom = rateTransportProvider.Zones.AddNew();
			zoneFrom.TZ_ZoneName = "ZoneFrom";

			var zoneTo = rateTransportProvider.Zones.AddNew();
			zoneTo.TZ_ZoneName = "ZoneTo";

			var incorrectZone = rateTransportProvider.Zones.AddNew();
			incorrectZone.TZ_ZoneName = "IncorrectZone";

			var zoneitem = zoneFrom.Items.AddNew();
			zoneitem.TQ_FromPostCode = "1";
			zoneitem.TQ_ToPostCode = "5";

			var zoneitem2 = zoneTo.Items.AddNew();
			zoneitem2.TQ_FromPostCode = "1";
			zoneitem2.TQ_ToPostCode = "5";

			var cityTown = Factory.New<RefCityTown>();
			cityTown.R9_InternationalName = "notpotato";

			var rateEntry = ratingHeader.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "JM", "FRT", 100);
			rateEntry.TI_TZ_OriginZone = zoneFrom.PK;
			rateEntry.TI_TZ_DestinationZone = zoneTo.PK;
			rateEntry.OriginSuburbPK = cityTown.PK;

			var rateEntry2 = ratingHeader.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "JM", "WAR", 100);
			rateEntry2.TI_TZ_OriginZone = zoneFrom.PK;
			rateEntry2.TI_TZ_DestinationZone = zoneTo.PK;

			var rateEntry3 = ratingHeader.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "JM", "FRT", 101);
			rateEntry3.TI_TZ_OriginZone = incorrectZone.PK;
			rateEntry3.TI_TZ_DestinationZone = zoneTo.PK;

			var rateEntry4 = ratingHeader.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "JM", "WAR", 101);
			rateEntry4.TI_TZ_OriginZone = zoneFrom.PK;
			rateEntry4.TI_TZ_DestinationZone = incorrectZone.PK;

			var rateEntry5 = ratingHeader.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "JM", "WAR", 101);

			var criteria = new TestRatingCriteria();
			criteria.PickupAddress = GetAddress(postcode: "2");
			criteria.DeliveryAddress = GetAddress(postcode: "3");

			var entrys = new List<IRateEntry> { rateEntry, rateEntry2, rateEntry3, rateEntry4, rateEntry5 };
			RateLocationFilter.Apply(entrys, criteria);
			AssertEquals("Remaining rates are correct zone rate and empty rate", 2, entrys.Count);
		}
	}
}
