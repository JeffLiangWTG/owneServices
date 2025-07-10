using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Rateable;
using Moq;

namespace Enterprise.Rating.Business.Test
{
	public class ContainerIsNonOperatingReeferComparerTest : TestCaseWithFactory
	{
		public void TestGetSimilarity()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entryYes20GP = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "20GP");
			entryYes20GP.TI_IsNonOperatedReefer = "Y";
			var entryNo20GP = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "20GP");
			entryNo20GP.TI_IsNonOperatedReefer = "N";
			var entryBlank40GP = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "40GP");
			entryBlank40GP.TI_IsNonOperatedReefer = "";
			var entryNo40GP = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "40GP");
			entryNo40GP.TI_IsNonOperatedReefer = "N";

			var criteria = new TestRatingCriteria();
			var containerTypes = new List<Guid?>() {
				entryYes20GP.TI_RC.ToGuid(),
				entryBlank40GP.TI_RC.ToGuid(),
			};
			var isNonOperatedReeferValues = new List<bool>() { true, false };

			var measuresMock = new Mock<IDistinctPartRateDimensions>();
			measuresMock.Setup(x => x.GetDistinctContainerTypePKs()).Returns(containerTypes);
			measuresMock.Setup(x => x.GetDistinctContainerIsNonOperatingReefers()).Returns(isNonOperatedReeferValues);
			var measures = measuresMock.Object;

			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasContainerType).Returns(true);
			var partList = partListMock.Object;

			var partYesNonOperatedReefer20GP = CreatePart(true, entryYes20GP.TI_RC.ToGuid());
			var partNoNonOperatedReefer20GP = CreatePart(false, entryNo20GP.TI_RC.ToGuid());
			var partYesNonOperatedReefer40GP = CreatePart(true, entryNo40GP.TI_RC.ToGuid());

			var lineProvider = new FastLineProvider(criteria);
			var lineYes20GP = lineProvider.GetOrCreate(entryYes20GP.RateLines[0]);
			var lineNo20GP = lineProvider.GetOrCreate(entryNo20GP.RateLines[0]);
			var lineBlank40GP = lineProvider.GetOrCreate(entryBlank40GP.RateLines[0]);
			var lineNo40GP = lineProvider.GetOrCreate(entryNo40GP.RateLines[0]);

			var comparer = new ContainerIsNonOperatingReeferComparer();

			// For the below four groups of three tests, keep in mind that the
			// comparer is only looking a the reefer flag. It is not caring
			// about the container type itself, but it does care that there is a
			// container
			AssertEquals(Similarity.Exact, comparer.GetSimilarity(lineYes20GP, measures, partList, partYesNonOperatedReefer20GP, null));
			AssertEquals(Similarity.None, comparer.GetSimilarity(lineYes20GP, measures, partList, partNoNonOperatedReefer20GP, null));
			AssertEquals(Similarity.Exact, comparer.GetSimilarity(lineYes20GP, measures, partList, partYesNonOperatedReefer40GP, null));

			AssertEquals(Similarity.None, comparer.GetSimilarity(lineNo20GP, measures, partList, partYesNonOperatedReefer20GP, null));
			AssertEquals(Similarity.Exact, comparer.GetSimilarity(lineNo20GP, measures, partList, partNoNonOperatedReefer20GP, null));
			AssertEquals(Similarity.None, comparer.GetSimilarity(lineNo20GP, measures, partList, partYesNonOperatedReefer40GP, null));

			AssertEquals(Similarity.Generic, comparer.GetSimilarity(lineBlank40GP, measures, partList, partYesNonOperatedReefer20GP, null));
			AssertEquals(Similarity.Generic, comparer.GetSimilarity(lineBlank40GP, measures, partList, partNoNonOperatedReefer20GP, null));
			AssertEquals(Similarity.Generic, comparer.GetSimilarity(lineBlank40GP, measures, partList, partYesNonOperatedReefer40GP, null));

			AssertEquals(Similarity.None, comparer.GetSimilarity(lineNo40GP, measures, partList, partYesNonOperatedReefer20GP, null));
			AssertEquals(Similarity.Exact, comparer.GetSimilarity(lineNo40GP, measures, partList, partNoNonOperatedReefer20GP, null));
			AssertEquals(Similarity.None, comparer.GetSimilarity(lineNo40GP, measures, partList, partYesNonOperatedReefer40GP, null));
		}

		static IRateablePart CreatePart(bool isNonOperatedReefer, Guid containerPK)
		{
			var partMock = new Mock<IRateablePart>();
			partMock.Setup(x => x.ContainerPK).Returns(containerPK);
			partMock.Setup(x => x.ContainerIsNonOperatingReefer).Returns(isNonOperatedReefer);
			return partMock.Object;
		}

		protected TestHelper Helper => testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;
	}
}
