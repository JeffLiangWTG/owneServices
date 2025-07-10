using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using FluentAssertions;

namespace Enterprise.MasterFiles.Business.Rating.Test
{
	public class RatingZoneRetrieverTest : TestCaseWithFactory
	{
		public void TestGetApplicableRatingZones()
		{
			var allZones = zones.ToArray();
			AssertContainsExactElementsInAnyOrder("organization1 zone owner",
				new[] { allZoneNoCarrier.FZ_Code, allZoneCarrier1Matching.FZ_Code },
				RatingZoneRetriever.GetApplicableRatingZones(allZones, organisation1));

			AssertContainsExactElementsInAnyOrder("organization2 zone owner",
				new[] { allZoneNoCarrier.FZ_Code, allZoneCarrier2Matching.FZ_Code },
				RatingZoneRetriever.GetApplicableRatingZones(allZones, organisation2));

			Assert("Should not contain tax zone", RatingZoneRetriever.GetApplicableRatingZones(allZones).All(x => x != taxZone.FZ_Code));
			Assert("Should not contain reporting zone", RatingZoneRetriever.GetApplicableRatingZones(allZones).All(x => x != reportingZone.FZ_Code));
			Assert("Should not contain wise rates zone", RatingZoneRetriever.GetApplicableRatingZones(allZones).All(x => x != wrsZone.FZ_Code));
			Assert("Should not contain transit warehouse zone", RatingZoneRetriever.GetApplicableRatingZones(allZones).All(x => x != transitWarehouseZone.FZ_Code));
		}

		public void TestGetApplicableWiseRatesZones()
		{
			AssertContainsExactElementsInAnyOrder("organization1 zone owner",
				new[] { wrsZone.FZ_Code, wrsZoneWithOrg.FZ_Code },
				RatingZoneRetriever.GetApplicableWiseRatesZones(zones.ToArray(), organisation1));
		}

		public void TestGetApplicableLocationCodes()
		{
			var aubne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");
			var australia = aubne.Country;
			allZoneCarrier1Matching.Countries.Add(australia);
			var zoneCode = allZoneCarrier1Matching.FZ_Code;

			var result = RatingZoneRetriever.GetApplicableLocationCodes(aubne, new[] { organisation1 });
			result.Should().BeEquivalentTo(
				new ZString[]
				{
					"AUBNE",		// UNLOCO
					"BNE",			// IATA
					"AU",			// Country
					"AUEC",			// Default setup zone
					zoneCode,		// AU is added to the zone earlier
					string.Empty
				});

			result = RatingZoneRetriever.GetApplicableLocationCodes(aubne, new[] { organisation2 });
			result.Should().BeEquivalentTo(
				new ZString[] { "AUBNE", "BNE", "AU", "AUEC", string.Empty },
				"AUBNE is not in the zones covered by organisation2. It's zones should not be in the result.");

			Assert("FluentAssertions is used.", true);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			organisation1 = Factory.New<OrgHeader>();
			organisation1.OH_FullName = "Carrier 1";

			organisation2 = Factory.New<OrgHeader>();
			organisation2.OH_FullName = "Carrier 2";

			zones = new List<RefZoneHeader>();

			allZoneNoCarrier = CreateZone("ZON1", RefZoneHeaderLookups.ZoneTypeCodes.All);
			allZoneCarrier1Matching = CreateZone("ZON2", RefZoneHeaderLookups.ZoneTypeCodes.All, organisation1);
			allZoneCarrier2Matching = CreateZone("ZON3", RefZoneHeaderLookups.ZoneTypeCodes.All, organisation2);
			taxZone = CreateZone("ZON4", RefZoneHeaderLookups.ZoneTypeCodes.Tax);
			reportingZone = CreateZone("ZON5", RefZoneHeaderLookups.ZoneTypeCodes.Reporting);
			wrsZone = CreateZone("ZON6", RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean);
			transitWarehouseZone = CreateZone("ZON7", RefZoneHeaderLookups.ZoneTypeCodes.TransitWarehouse);
			wrsZoneWithOrg = CreateZone("ZON8", RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean, organisation1);
		}

		RefZoneHeader CreateZone(string zoneCode, string zoneType, OrgHeader carrier = null)
		{
			var zone = Factory.New<RefZoneHeader>();
			zone.FZ_Code = zoneCode;
			zone.FZ_Description = zoneCode;
			zone.FZ_ZoneType = zoneType;
			if (carrier != null)
			{
				zone.FZ_OH_RelatedParty = carrier.PK;
			}

			zones.Add(zone);

			return zone;
		}

		List<RefZoneHeader> zones;

		RefZoneHeader allZoneNoCarrier;
		RefZoneHeader allZoneCarrier1Matching;
		RefZoneHeader allZoneCarrier2Matching;
		RefZoneHeader taxZone;
		RefZoneHeader reportingZone;
		RefZoneHeader wrsZone;
		RefZoneHeader wrsZoneWithOrg;
		RefZoneHeader transitWarehouseZone;

		OrgHeader organisation1;
		OrgHeader organisation2;

		#endregion
	}
}
