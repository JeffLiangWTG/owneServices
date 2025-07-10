using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	public class OriginDestinationComparerTest : RatingTestCase
	{
		public void TestIsOriginDestinationOverriden()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());

			var zone1 = Helper.NewInternationalZone("USZO", null, "US");
			var zone2 = Helper.NewInternationalZone("AUUN", null, "AUMEL", "AUSYD");

			var oRGLine1 = quote.AddRateEntry("ORG", "AIR", "AU", "").AddRateLine("ODOC", FlatCalculator.Code);
			var oRGLine2 = quote.AddRateEntry("ORG", "AIR", "AUSYD", "").AddRateLine("ODOC", FlatCalculator.Code);
			var oRGLine3 = quote.AddRateEntry("ORG", "AIR", "AUSYD", "US").AddRateLine("ODOC", FlatCalculator.Code);
			var oRGLine4 = quote.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX").AddRateLine("ODOC", FlatCalculator.Code);
			var oRGLine5 = quote.AddRateEntry("ORG", "AIR", "AUMEL", "USLAX").AddRateLine("ODOC", FlatCalculator.Code);
			var oRGLine6 = quote.AddRateEntry("ORG", "AIR", "AUSYD", "USZO").AddRateLine("ODOC", FlatCalculator.Code);
			var oRGLine7 = quote.AddRateEntry("ORG", "AIR", "AUUN", "").AddRateLine("ODOC", FlatCalculator.Code);

			var dSTLine1 = quote.AddRateEntry("DST", "AIR", "", "USCA").AddRateLine("DDOC", FlatCalculator.Code);
			var dSTLine2 = quote.AddRateEntry("DST", "AIR", "", "USLAX").AddRateLine("DDOC", FlatCalculator.Code);
			var dSTLine3 = quote.AddRateEntry("DST", "AIR", "AUEC", "USLAX").AddRateLine("DDOC", FlatCalculator.Code);
			var dSTLine4 = quote.AddRateEntry("DST", "AIR", "AUSYD", "USLAX").AddRateLine("DDOC", FlatCalculator.Code);
			var dSTLine5 = quote.AddRateEntry("DST", "AIR", "AUSYD", "USSFO").AddRateLine("DDOC", FlatCalculator.Code);

			var fRTLine1 = quote.AddRateEntry("AIR", "LSE", "AU", "US").RateLines[0];
			var fRTLine2 = quote.AddRateEntry("AIR", "LSE", "AU", "USLAX").RateLines[0];
			var fRTLine3 = quote.AddRateEntry("AIR", "LSE", "AUSYD", "US").RateLines[0];
			var fRTLine4 = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			var fRTLine5 = quote.AddRateEntry("AIR", "LSE", "AUMEL", "USSFO").RateLines[0];
			var fRTLine6 = quote.AddRateEntry("AIR", "LSE", "AUMEL", "").RateLines[0];
			var fRTLine7 = quote.AddRateEntry("AIR", "LSE", "", "USSFO").RateLines[0];

			AssertEquals(4, OriginDestinationComparer.IsOriginDestinationOverridden(oRGLine1, oRGLine2, null));
			AssertEquals(2, OriginDestinationComparer.IsOriginDestinationOverridden(oRGLine2, oRGLine3, null));
			AssertEquals(2, OriginDestinationComparer.IsOriginDestinationOverridden(oRGLine3, oRGLine4, null));
			AssertEquals(0, OriginDestinationComparer.IsOriginDestinationOverridden(oRGLine4, oRGLine5, null));

			AssertEquals(0, OriginDestinationComparer.IsOriginDestinationOverridden(oRGLine3, oRGLine6, null));
			AssertEquals(2, OriginDestinationComparer.IsOriginDestinationOverridden(oRGLine6, oRGLine3, null));
			AssertEquals(0, OriginDestinationComparer.IsOriginDestinationOverridden(oRGLine7, oRGLine1, null));
			AssertEquals(4, OriginDestinationComparer.IsOriginDestinationOverridden(oRGLine1, oRGLine7, null));
			AssertEquals(4, OriginDestinationComparer.IsOriginDestinationOverridden(oRGLine7, oRGLine2, null));
			AssertEquals(0, OriginDestinationComparer.IsOriginDestinationOverridden(oRGLine2, oRGLine7, null));

			var criteria = new TestRatingCriteria();
			criteria.Origin = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			zone2.Countries.Add((RefCountry)LocationHelper.GetLocationFromString("AU", Factory));
			AssertEquals(0, OriginDestinationComparer.IsOriginDestinationOverridden(oRGLine1, oRGLine7, criteria));

			AssertEquals(4, OriginDestinationComparer.IsOriginDestinationOverridden(dSTLine1, dSTLine2, null));
			AssertEquals(2, OriginDestinationComparer.IsOriginDestinationOverridden(dSTLine2, dSTLine3, null));
			AssertEquals(2, OriginDestinationComparer.IsOriginDestinationOverridden(dSTLine3, dSTLine4, null));
			AssertEquals(0, OriginDestinationComparer.IsOriginDestinationOverridden(dSTLine4, dSTLine5, null));

			AssertEquals(2, OriginDestinationComparer.IsOriginDestinationOverridden(fRTLine1, fRTLine2, null));
			AssertEquals(4, OriginDestinationComparer.IsOriginDestinationOverridden(fRTLine1, fRTLine3, null));
			AssertEquals(4, OriginDestinationComparer.IsOriginDestinationOverridden(fRTLine1, fRTLine4, null));
			AssertEquals(4, OriginDestinationComparer.IsOriginDestinationOverridden(fRTLine2, fRTLine4, null));
			AssertEquals(2, OriginDestinationComparer.IsOriginDestinationOverridden(fRTLine3, fRTLine4, null));
			AssertEquals(0, OriginDestinationComparer.IsOriginDestinationOverridden(fRTLine4, fRTLine5, null));
			AssertEquals(2, OriginDestinationComparer.IsOriginDestinationOverridden(fRTLine6, fRTLine5, null));
			AssertEquals(4, OriginDestinationComparer.IsOriginDestinationOverridden(fRTLine7, fRTLine5, null));
		}

		public void TestIsOriginDestinationOverriden_WithCriteria()
		{
			var zone1 = Helper.NewInternationalZone("ZUB1", null, "AUMEL", "AUSYD");
			var zone2 = Helper.NewInternationalZone("ZUB2", null, "AU", "AUPER");

			var quote = Helper.NewQuote(Helper.NewOrgHeader());

			var dSTLine1 = quote.AddRateEntry("DST", "AIR", "", "ZUB1").AddRateLine("DDOC", FlatCalculator.Code);
			var dSTLine2 = quote.AddRateEntry("DST", "AIR", "", "ZUB2").AddRateLine("DDOC", FlatCalculator.Code);

			Factory.Save();

			AssertEquals(4, OriginDestinationComparer.IsOriginDestinationOverridden(dSTLine2, dSTLine1, null));
			AssertEquals(0, OriginDestinationComparer.IsOriginDestinationOverridden(dSTLine1, dSTLine2, null));

			var criteria = new TestRatingCriteria();

			criteria.Destination = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			AssertEquals(4, OriginDestinationComparer.IsOriginDestinationOverridden(dSTLine2, dSTLine1, criteria));
			AssertEquals(0, OriginDestinationComparer.IsOriginDestinationOverridden(dSTLine1, dSTLine2, criteria));
		}

		public void TestIsOriginDestinationOverriden_WithCriteria_CarrierAndClientZones()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;

			var zone1 = Helper.NewInternationalZone("ZUB1", carrier, "AUMEL");
			var zone2 = Helper.NewInternationalZone("ZUB2", Factory.New<OrgHeader>(), "AUMEL");

			var quote = Helper.NewQuote(Helper.NewOrgHeader());

			var dSTLine1 = quote.AddRateEntry("DST", "AIR", "", "ZUB1").AddRateLine("DDOC", FlatCalculator.Code);
			var dSTLine2 = quote.AddRateEntry("DST", "AIR", "", "ZUB2").AddRateLine("DDOC", FlatCalculator.Code);

			var criteria = new TestRatingCriteria();
			criteria.Destination = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
			AssertEquals(0, OriginDestinationComparer.IsOriginDestinationOverridden(dSTLine2, dSTLine1, criteria));
			AssertEquals("Client overrides Carrier", 4, OriginDestinationComparer.IsOriginDestinationOverridden(dSTLine1, dSTLine2, criteria));
		}

		public void TestComparer_SameLocationTypes_RateEntryMatchingCriteriaOriginDestinationIsPreferred()
		{
			var comparer = CreateComparer("AUSYD", "CNSHA", "IDAEG", "CNAAT");

			var rateLine1 = CreateRate(comparer.Criteria, "LCL", "AUSYD", "CNSHA");
			var rateLine2 = CreateRate(comparer.Criteria, "LCL", "IDAEG", "CNAAT");
			Assert("rateLine1 is prefered because it matches Criteria Origin/Destination", comparer.Compare(rateLine1, rateLine2) > 0);

			rateLine1 = CreateRate(comparer.Criteria, "LCL", "AU", "CN");
			rateLine2 = CreateRate(comparer.Criteria, "LCL", "ID", "CN");
			Assert("rateLine1 is prefered because it matches Criteria Origin/Destination", comparer.Compare(rateLine1, rateLine2) > 0);

			rateLine1 = CreateRate(comparer.Criteria, "LCL", "AU", "");
			rateLine2 = CreateRate(comparer.Criteria, "LCL", "ID", "");
			Assert("rateLine1 is prefered because it matches Criteria Origin/Destination", comparer.Compare(rateLine1, rateLine2) > 0);

			rateLine1 = CreateRate(comparer.Criteria, "LCL", "", "CNSHA");
			rateLine2 = CreateRate(comparer.Criteria, "LCL", "", "CNAAT");
			Assert("rateLine1 is prefered because it matches Criteria Origin/Destination", comparer.Compare(rateLine1, rateLine2) > 0);
		}

		public void TestCompare_DifferentLocationTypes_MoreSpecificLocationIsPreferred()
		{
			var comparer = CreateComparer("AUSYD", "CNSHA", "IDAEG", "CNAAT");

			var rateLine1 = CreateRate(comparer.Criteria, "LCL", "AU", "CNSHA");
			var rateLine2 = CreateRate(comparer.Criteria, "LCL", "IDAEG", "CNAAT");
			AssertEquals("rateLine2 has more specific Origin and Destination", true, comparer.Compare(rateLine1, rateLine2) < 0);

			rateLine1 = CreateRate(comparer.Criteria, "LCL", "AU", "CN");
			rateLine2 = CreateRate(comparer.Criteria, "LCL", "ID", "");
			AssertEquals("rateLine1 has more specific Origin and Destination", true, comparer.Compare(rateLine1, rateLine2) > 0);

			rateLine1 = CreateRate(comparer.Criteria, "LCL", "", "CN");
			rateLine2 = CreateRate(comparer.Criteria, "LCL", "ID", "CN");
			AssertEquals("rateLine2 has more specific Origin and Destination", true, comparer.Compare(rateLine1, rateLine2) < 0);
		}

		public void TestComparer_FreightEntries_MoreSpecificOriginOnRateEntryIsPreferred()
		{
			var comparer = CreateComparer("AUSYD", "CNSHA", "IDAEG", "CNAAT");

			var rateLine1 = CreateRate(comparer.Criteria, "LCL", "AU", "CNSHA");
			var rateLine2 = CreateRate(comparer.Criteria, "LCL", "IDAEG", "CN");
			AssertEquals(
				"rateLine2 should be selected as IDAEG (UNLOCO) is more specific location than AU (Country)",
				true,
				comparer.Compare(rateLine1, rateLine2) < 0
			);

			rateLine1 = CreateRate(comparer.Criteria, "LCL", "", "CNSHA");
			rateLine2 = CreateRate(comparer.Criteria, "LCL", "IDAEG", "");
			AssertEquals(
				"rateLine2 should be selected as IDAEG (UNLOCO) is more specific location than empty origin on rateLine1",
				true,
				comparer.Compare(rateLine1, rateLine2) < 0
			);

			rateLine1 = CreateRate(comparer.Criteria, "LCL", "AUSYD", "");
			rateLine2 = CreateRate(comparer.Criteria, "LCL", "ID", "");
			AssertEquals(
				"rateLine1 should be selected as AUSYD (UNLOCO) is more specific location than ID (Country)",
				true,
				comparer.Compare(rateLine1, rateLine2) > 0
			);

			rateLine1 = CreateRate(comparer.Criteria, "LCL", "AUSYD", "");
			rateLine2 = CreateRate(comparer.Criteria, "LCL", "ID", "CNAAT");
			AssertEquals(
				"rateLine1 should be selected as AUSYD (UNLOCO) is more specific location than ID (Country)",
				true,
				comparer.Compare(rateLine1, rateLine2) > 0
			);
		}

		public void TestComparer_OriginEntries_MoreSpecificOriginOnRateEntryIsPreferred()
		{
			var comparer = CreateComparer("AUSYD", "CNSHA", "IDAEG", "CNAAT");

			var rateLine1 = CreateRate(comparer.Criteria, "ORG", "AUSYD", "CN");
			var rateLine2 = CreateRate(comparer.Criteria, "ORG", "ID", "CN");
			var comparisonResult = comparer.Compare(rateLine1, rateLine2);
			AssertEquals(
				"rateLine1 should be selected as AUSYD (UNLOCO) is more specific location than ID (Country)",
				1, // Positive value expected
				Math.Sign(comparisonResult)
			);

			rateLine1 = CreateRate(comparer.Criteria, "ORG", "", "CN");
			rateLine2 = CreateRate(comparer.Criteria, "ORG", "ID", "CN");
			comparisonResult = comparer.Compare(rateLine1, rateLine2);
			AssertEquals(
				"rateLine2 should be selected as ID (Country) is more specific location than empty origin location for rateLine1",
				-1, // Negative value expected
				Math.Sign(comparisonResult)
			);
		}

		public void TestComparer_DestinationEntries_MoreSpecificDestinationOnRateEntryIsPreferred()
		{
			var comparer = CreateComparer("AUSYD", "CNSHA", "IDAEG", "CNAAT");

			var rateLine1 = CreateRate(comparer.Criteria, "DST", "AUSYD", "CN");
			var rateLine2 = CreateRate(comparer.Criteria, "DST", "ID", "CNAAT");
			AssertLessThan(
				"rateLine2 should be selected as CNAAT (UNLOCO) is more specific location than CN (Country)",
				comparer.Compare(rateLine1, rateLine2),
				0);

			rateLine1 = CreateRate(comparer.Criteria, "DST", "AU", "CN");
			rateLine2 = CreateRate(comparer.Criteria, "DST", "", "CNAAT");
			AssertLessThan(
				"rateLine2 should be selected as CNAAT (UNLOCO) is more specific location than CN (Country)",
				comparer.Compare(rateLine1, rateLine2),
				0
			);
		}

		OriginDestinationComparer CreateComparer(string origin, string destination, string rateOrigin, string rateDestination)
		{
			var ratingCriteria = new TestRatingCriteria(origin, destination, 0, null, null);
			if (!string.IsNullOrWhiteSpace(rateOrigin))
			{
				ratingCriteria.RateOrigin = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, rateOrigin);
			}
			if (!string.IsNullOrWhiteSpace(rateDestination))
			{
				ratingCriteria.RateDestination = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, rateDestination);
			}

			return new OriginDestinationComparer(ratingCriteria);
		}

		FastLine CreateRate(RatingCriteria criteria, string rateCategory, string origin, string destination)
		{
			return criteria.Cache.GetOrCreateFastLine(CreateRate(rateCategory, origin, destination));
		}

		RateLine CreateRate(string rateCategory, string origin, string destination)
		{
			return clientRate.AddRateEntryWithFlatRateLine(rateCategory, "LCL", origin, destination, "FRT", 50).RateLines[0];
		}

		protected override void SetUp()
		{
			base.SetUp();
			clientRate = Helper.NewClientRate(NewClient);
		}

		ClientRate clientRate;
	}
}
