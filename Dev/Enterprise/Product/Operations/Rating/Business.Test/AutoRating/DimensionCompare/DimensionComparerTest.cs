using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Moq;

namespace Enterprise.Rating.Business.Test
{
	public class DimensionComparerTest : TestCaseWithFactory
	{
		public void TestGetSimilarity_Logging()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry20GP = rate.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "", "20GP");
			var line20GP = entry20GP.AddRateLine("ODOC");

			var criteria = new TestRatingCriteria();
			var measures = criteria.RateableMeasures;
			measures.AddPackageUnit(new MeasureInfo.ContainerInfo(), "PLT");

			var partList = measures.GetPartList(MeasureType.Unit);
			var part1 = partList[0];
			var lineProvider = new FastLineProvider(criteria);
			var fastLine20GP = lineProvider.GetOrCreate(line20GP);
			var lineDisplayInfo = line20GP.DisplayInfo();

			var mockLog = new Mock<IPointMatchingLog>();
			mockLog.Setup(x => x.GetPartName(partList, part1)).Returns("PartName").Verifiable();
			mockLog.Setup(x => x.LogPartDidNotMatchLine(lineDisplayInfo, "PartName", "Dimension", "PART", "LINE")).Verifiable();

			var comparer = new DimensionComparerForTest("LINE", "PART", Similarity.None);
			comparer.GetSimilarity(fastLine20GP, measures, partList, part1, mockLog.Object);

			mockLog.Verify();
			Assert("Verify OK", true);
		}

		public void TestGetSimilarity_DoesNotLogWhenSimilar()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry20GP = rate.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "", "20GP");
			var line20GP = entry20GP.AddRateLine("ODOC");

			var criteria = new TestRatingCriteria();
			var measures = criteria.RateableMeasures;
			measures.AddPackageUnit(new MeasureInfo.ContainerInfo(), "PLT");

			var partList = measures.GetPartList(MeasureType.Unit);
			var part1 = partList[0];
			var lineProvider = new FastLineProvider(criteria);
			var fastLine20GP = lineProvider.GetOrCreate(line20GP);

			var mockLog = new Mock<IPointMatchingLog>(MockBehavior.Strict);

			var comparer = new DimensionComparerForTest("TEST", "TEST", Similarity.Exact);
			comparer.GetSimilarity(fastLine20GP, measures, partList, part1, mockLog.Object);

			mockLog.Verify();
			Assert("Verify OK", true);
		}

		protected TestHelper Helper => testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;

		class DimensionComparerForTest : DimensionComparer<string>
		{
			readonly string lineValue;
			readonly string partValue;
			readonly Similarity similarity;

			public DimensionComparerForTest(string lineValue, string partValue, Similarity similarity)
			{
				this.lineValue = lineValue;
				this.partValue = partValue;
				this.similarity = similarity;
			}

			protected override string DimensionReadableNameForLogging => "Dimension";

			public override bool AreRatelinesCompatibleForSimultaneousUsage(Rateable.RateableMeasureSet measures, FastLine line1, FastLine line2)
				=> true;

			public override bool HasDimension(Rateable.IHasPartDimensions partList)
				=> true;

			protected override string GetLineValueForLogging(FastLine line, MeasureType currentMeasureType)
				=> lineValue;

			protected override string GetPartValue(Rateable.IRateablePart part)
				=> partValue;

			protected override string GetPartValueForLogging(BusinessObjectFactory factory, Rateable.IRateablePart part, MeasureType currentMeasureType)
				=> GetPartValue(part);

			protected internal override Similarity GetValueSimilarity(FastLine line, string partValue)
				=> similarity;
		}
	}
}
