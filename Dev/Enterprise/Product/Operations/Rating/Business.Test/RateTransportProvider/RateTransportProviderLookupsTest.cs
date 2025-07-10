using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.Testing
{
	public class RateTransportProviderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestZoneTypes()
		{
			var expected = new ZString[]
				{
					RatingConstants.RatingZoneTypes.All,
					RatingConstants.RatingZoneTypes.Rating,
					RatingConstants.RatingZoneTypes.Operations,
					RatingConstants.RatingZoneTypes.Reporting
				};

			var transportZoneSet = Factory.New<RateTransportProvider>();
			var actual = transportZoneSet.Lookups.ZoneTypes.Cast<CodeDescriptionPair>().Select(x => x.Code).ToArray();

			AssertEquals(expected.Length, actual.Length);
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestZoneModes()
		{
			var expectedZones = new CodeDescriptionPairList();
			var transportZoneSet = Factory.New<RateTransportProvider>();

			expectedZones = new CodeDescriptionPairList(OLookUpEditType.RateModes);
			transportZoneSet.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			var actual = transportZoneSet.Lookups.ZoneModes.Cast<CodeDescriptionPair>().Select(x => x.Code).ToArray();
			AssertContainsExactElementsInAnyOrder(expectedZones.GetAllCodes(), actual);

			expectedZones.Clear();
			expectedZones.AddPair(Core.Constants.RateMode.ALL, Core.Constants.RateMode.ALL);
			transportZoneSet.TP_ZoneType = RatingConstants.RatingZoneTypes.Reporting;
			actual = transportZoneSet.Lookups.ZoneModes.Cast<CodeDescriptionPair>().Select(x => x.Code).ToArray();
			AssertContainsExactElementsInAnyOrder(expectedZones.GetAllCodes(), actual);
		}
	}
}
