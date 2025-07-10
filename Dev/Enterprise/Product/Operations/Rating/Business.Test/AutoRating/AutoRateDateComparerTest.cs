using System.Linq;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	public class AutoRateDateComparerTest : RatingTestCase
	{
		public void TestJobDateTypeComparerLocationSpecificity()
		{
			var zone = Helper.NewInternationalZone("AUXX", null, "AU", "US", "AUSYD");
			Factory.Save();

			var autoRateDate1 = new AutoRateDate { Location = "AUSYD" };
			var autoRateDate2 = new AutoRateDate { Location = "AU" };
			var autoRateDate3 = new AutoRateDate { Location = zone.Code };
			var autoRateDate4 = new AutoRateDate();

			var resultsInPseudoRandomOrder = new[]
			{
				autoRateDate2,
				autoRateDate3,
				autoRateDate4,
				autoRateDate1,
			};

			resultsInPseudoRandomOrder = resultsInPseudoRandomOrder.OrderByDescending(x => x, new AutoRateDateComparer(Factory)).ToArray();

			AssertEquals("First result should be the most specific", autoRateDate1, resultsInPseudoRandomOrder[0]);
			AssertEquals("Second result should be country", autoRateDate2, resultsInPseudoRandomOrder[1]);
			AssertEquals("third one should be international zone", autoRateDate3, resultsInPseudoRandomOrder[2]);
			AssertEquals("last one should be empty location", autoRateDate4, resultsInPseudoRandomOrder[3]);

			resultsInPseudoRandomOrder = new[]
			{
				autoRateDate2,
				autoRateDate1
			};

			resultsInPseudoRandomOrder = resultsInPseudoRandomOrder.OrderByDescending(x => x, new AutoRateDateComparer(Factory)).ToArray();

			AssertEquals("First result should be the most specific", autoRateDate1, resultsInPseudoRandomOrder[0]);
			AssertEquals("Second result should be country", autoRateDate2, resultsInPseudoRandomOrder[1]);
		}

		public void TestJobDateTypeComparerParameterSpecificity()
		{
			var zone = Helper.NewInternationalZone("AUXX", null, "AU", "US", "AUSYD");
			Factory.Save();

			var autoRateDate1 = new AutoRateDate
			{
				RateType = JobRateTypes.Codes.Revenue,
				Location = "AUSYD",
				ContainerMode = Constants.ContainerModes.FCL,
				Mode = Constants.TransportModes.Air,
				DirectionCode = Constants.FreightShipmentDirection.Code.Export,
				JobType = "SHP"
			};
			var autoRateDate2 = new AutoRateDate
			{
				RateType = JobRateTypes.Codes.All,
				Location = "AUSYD",
				ContainerMode = Constants.ContainerModes.FCL,
				Mode = Constants.TransportModes.Air,
				DirectionCode = Constants.FreightShipmentDirection.Code.Export,
				JobType = "SHP"
			};
			var autoRateDate3 = new AutoRateDate
			{
				RateType = JobRateTypes.Codes.All,
				Location = "All",
				ContainerMode = Constants.ContainerModes.FCL,
				Mode = Constants.TransportModes.Air,
				DirectionCode = Constants.FreightShipmentDirection.Code.Export,
				JobType = "SHP"
			};
			var autoRateDate4 = new AutoRateDate
			{
				RateType = JobRateTypes.Codes.All,
				Location = "All",
				ContainerMode = Constants.ContainerModes.All,
				Mode = Constants.TransportModes.Air,
				DirectionCode = Constants.FreightShipmentDirection.Code.Export,
				JobType = "SHP"
			};
			var autoRateDate5 = new AutoRateDate
			{
				RateType = JobRateTypes.Codes.All,
				Location = "All",
				ContainerMode = Constants.ContainerModes.All,
				Mode = Constants.TransportModes.All,
				DirectionCode = Constants.FreightShipmentDirection.Code.Export,
				JobType = "SHP"
			};
			var autoRateDate6 = new AutoRateDate
			{
				RateType = JobRateTypes.Codes.All,
				Location = "All",
				ContainerMode = Constants.ContainerModes.All,
				Mode = Constants.TransportModes.All,
				DirectionCode = Constants.FreightShipmentDirection.Code.All,
				JobType = "SHP"
			};
			var autoRateDate7 = new AutoRateDate
			{
				RateType = JobRateTypes.Codes.All,
				Location = "All",
				ContainerMode = Constants.ContainerModes.All,
				Mode = Constants.TransportModes.All,
				DirectionCode = Constants.FreightShipmentDirection.Code.All,
				JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All
			};

			var resultsInPseudoRandomOrder = new[]
			{
				autoRateDate2,
				autoRateDate1,
				autoRateDate3,
				autoRateDate7,
				autoRateDate6,
				autoRateDate5,
				autoRateDate4,
			};

			AssertContainsExactElementsInExactOrder(
				"Should be sorted in descending order of specificity",
				new[]
				{
					autoRateDate1,
					autoRateDate2,
					autoRateDate3,
					autoRateDate4,
					autoRateDate5,
					autoRateDate6,
					autoRateDate7,
				},
				resultsInPseudoRandomOrder.OrderByDescending(x => x, new AutoRateDateComparer(Factory)));
		}
	}
}
