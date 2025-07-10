using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Rating.Integration;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class RatingConstantsHelperTest : TestCase
	{
		public void TestGetRateModesFromTransportMode()
		{
			IRatingConstantsHelper ratingHelper = new RatingConstantsHelper();
			IEnumerable<ZString> result1 = ratingHelper.GetRateModes(Core.Constants.ContainerModes.All);
			AssertArrayEqualsByElements(new ZString[] { Core.Constants.RateMode.ALL }, result1.ToArray());

			IEnumerable<ZString> result2 = ratingHelper.GetRateModes(Core.Constants.ContainerModes.FCL);
			AssertArrayEqualsByElements(new ZString[] { Core.Constants.RateMode.ALL, Core.Constants.RateMode.SEA, Core.Constants.RateMode.FCL }, result2.ToArray());

			IEnumerable<ZString> result3 = ratingHelper.GetRateModes(Core.Constants.TransportModes.Sea);
			AssertArrayEqualsByElements(new ZString[] { Core.Constants.RateMode.ALL, Core.Constants.RateMode.SEA, Core.Constants.RateMode.LCL, Core.Constants.RateMode.FCL }, result3.ToArray());

			IEnumerable<ZString> result4 = ratingHelper.GetRateModes(Core.Constants.ContainerModes.Loose);
			AssertArrayEqualsByElements(new ZString[] { Core.Constants.RateMode.ALL, Core.Constants.RateMode.AIR, Core.Constants.RateMode.LSE }, result4.ToArray());

			IEnumerable<ZString> result5 = ratingHelper.GetRateModes(Core.Constants.ContainerModes.AIR);
			AssertArrayEqualsByElements(new ZString[] { Core.Constants.RateMode.ALL, Core.Constants.RateMode.AIR, Core.Constants.RateMode.ULD, Core.Constants.RateMode.LSE }, result5.ToArray());
		}
	}
}
