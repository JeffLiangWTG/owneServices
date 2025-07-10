using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Rateable;
using Moq;

namespace Enterprise.Rating.Business.Test
{
	public class ContainerTypeComparerTest : TestCaseWithFactory
	{
		public void TestGetSimilarity()
		{
			// Note, Freight rates match containers on freight class.
			// Origin rates match on handling class.
			var container20a = Factory.New<RefContainer>();
			container20a.RC_Code = "C2A";
			container20a.RC_FreightRateClass = "20FN";
			container20a.RC_HandlingRateClass = "20HN";

			var container20b = Factory.New<RefContainer>();
			container20b.RC_Code = "C2B";
			container20b.RC_FreightRateClass = "20FN";
			container20b.RC_HandlingRateClass = "20HN";

			Factory.Save();
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry20GP = rate.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "", "20GP");
			var line20GP = entry20GP.AddRateLine("ODOC");
			var entryNoContainerType = rate.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "", "");
			var lineNoContainerType = entryNoContainerType.AddRateLine("ODOC");
			var entry40GP = rate.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "", "40GP");
			entry40GP.TI_MatchContainerRateClass = true;
			var line40GP = entry40GP.AddRateLine("ODOC");

			var entry20a = rate.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "", "C2A");
			entry20a.TI_MatchContainerRateClass = true;
			var line20a = entry20a.AddRateLine("ODOC");

			var criteria = new TestRatingCriteria();

			var distinctContainerTypes = new List<Guid?>() {
				entry20GP.TI_RC.ToGuid(),
				entry40GP.TI_RC.ToGuid(),
				container20b.PK.ToGuid()
			};

			var measuresMock = new Mock<IDistinctPartRateDimensions>();
			measuresMock.Setup(x => x.GetDistinctContainerTypePKs()).Returns(distinctContainerTypes);
			var measures = measuresMock.Object;

			var containerPartListMock = new Mock<IHasPartDimensions>();
			containerPartListMock.Setup(x => x.HasContainerType).Returns(true);
			var containerParts = containerPartListMock.Object;

			var part20GP = CreateContainerPart(entry20GP.TI_RC);
			var part40GP = CreateContainerPart(entry40GP.TI_RC);
			var part20b = CreateContainerPart(container20b);

			var partEmptyMock = new Mock<IRateablePart>();
			partEmptyMock.Setup(x => x.ContainerTypePk).Returns((Guid?)null);
			var partEmpty = partEmptyMock.Object;

			var unitPartListMock = new Mock<IHasPartDimensions>();
			unitPartListMock.Setup(x => x.HasPackageType).Returns(true);
			var unitPartList = unitPartListMock.Object;
			var unitPartMock = new Mock<IRateablePart>();
			unitPartMock.Setup(x => x.PackageType).Returns("PLT");
			var unitPart = unitPartMock.Object;

			var lineProvider = new FastLineProvider(criteria);
			var fastLine20GP = lineProvider.GetOrCreate(line20GP);
			var fastLineNoContainerType = lineProvider.GetOrCreate(lineNoContainerType);
			var fastLine40GP = lineProvider.GetOrCreate(line40GP);
			var fastLine20a = lineProvider.GetOrCreate(line20a);

			var comparer = new ContainerTypeComparer();
			AssertEquals("same container type on line and part", Similarity.Exact, comparer.GetSimilarity(fastLine20GP, measures, containerParts, part20GP, null));
			AssertEquals("different container type on line and part", Similarity.None, comparer.GetSimilarity(fastLine40GP, measures, containerParts, part20GP, null));
			AssertEquals("different container type on line and part", Similarity.None, comparer.GetSimilarity(fastLine20GP, measures, containerParts, part40GP, null));
			AssertEquals("line with no container type vs 20GP", Similarity.Generic, comparer.GetSimilarity(fastLineNoContainerType, measures, containerParts, part20GP, null));
			AssertEquals("line with no container type vs 40GP", Similarity.Generic, comparer.GetSimilarity(fastLineNoContainerType, measures, containerParts, part40GP, null));
			AssertEquals("line 20a matches on class vs part with same class", Similarity.Generic, comparer.GetSimilarity(fastLine20a, measures, containerParts, part20b, null));
			AssertEquals("line 20a has no match on class vs part 20GP", Similarity.None, comparer.GetSimilarity(fastLine20a, measures, containerParts, part20GP, null));

			AssertEquals("line with container type vs part with no container type", Similarity.None, comparer.GetSimilarity(fastLine20GP, measures, containerParts, partEmpty, null));
			AssertEquals("line with no container type vs part with no container type", Similarity.Exact, comparer.GetSimilarity(fastLineNoContainerType, measures, containerParts, partEmpty, null));

			AssertEquals("line with no container type vs unit part with no container type", Similarity.Exact, comparer.GetSimilarity(fastLineNoContainerType, measures, unitPartList, unitPart, null));
			AssertEquals("line with container type found on another part list vs unit part with no container type", Similarity.Lowest, comparer.GetSimilarity(fastLine20GP, measures, unitPartList, unitPart, null));
		}

		public void TestGetSimilarity_JobWithoutContainerType()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry20GP = rate.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "", "20GP");
			var line20GP = entry20GP.AddRateLine("ODOC");
			var entryNoContainerType = rate.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "", "");
			var lineNoContainerType = entryNoContainerType.AddRateLine("ODOC");

			var criteria = new TestRatingCriteria();

			var measuresMock = new Mock<IDistinctPartRateDimensions>();
			var distinctContainerTypes = new List<Guid?>();
			measuresMock.Setup(x => x.GetDistinctContainerTypePKs()).Returns(distinctContainerTypes);
			var measures = measuresMock.Object;

			var unitPartListMock = new Mock<IHasPartDimensions>();
			unitPartListMock.Setup(x => x.HasPackageType).Returns(true);
			var partList = unitPartListMock.Object;
			var unitPartMock = new Mock<IRateablePart>();
			unitPartMock.Setup(x => x.PackageType).Returns("PLT");
			var part1 = unitPartMock.Object;

			var lineProvider = new FastLineProvider(criteria);
			var fastLine20GP = lineProvider.GetOrCreate(line20GP);
			var fastLineNoContainerType = lineProvider.GetOrCreate(lineNoContainerType);

			var comparer = new ContainerTypeComparer();
			AssertEquals("line with no container type", Similarity.Exact, comparer.GetSimilarity(fastLineNoContainerType, measures, partList, part1, null));
			AssertEquals("line with container type", Similarity.None, comparer.GetSimilarity(fastLine20GP, measures, partList, part1, null));
		}

		public void TestGetSimilarity_JobWithSingleContainerTypeOnAnotherPartList()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry20GP = rate.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "", "20GP");
			var line20GP = entry20GP.AddRateLine("ODOC");
			var entryNoContainerType = rate.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "", "");
			var lineNoContainerType = entryNoContainerType.AddRateLine("ODOC");

			var otherContainerTypePK = Guid.NewGuid();

			var criteria = new TestRatingCriteria();

			// Distinct measures with only 20GP container type
			var measures1Mock = new Mock<IDistinctPartRateDimensions>();
			var distinctContainerTypes = new List<Guid?>() {
				entry20GP.TI_RC.ToGuid(),
			};
			measures1Mock.Setup(x => x.GetDistinctContainerTypePKs()).Returns(distinctContainerTypes);
			var measures1 = measures1Mock.Object;

			// Distinct measures with 20GP and one other container type
			var measures2Mock = new Mock<IDistinctPartRateDimensions>();
			var distinctContainerTypes2 = new List<Guid?>() {
				entry20GP.TI_RC.ToGuid(),
				otherContainerTypePK,
			};
			measures2Mock.Setup(x => x.GetDistinctContainerTypePKs()).Returns(distinctContainerTypes2);
			var measures2 = measures2Mock.Object;

			// Distinct measures with one container type, not 20GP
			var measures3Mock = new Mock<IDistinctPartRateDimensions>();
			var distinctContainerTypes3 = new List<Guid?>() {
				otherContainerTypePK,
			};
			measures3Mock.Setup(x => x.GetDistinctContainerTypePKs()).Returns(distinctContainerTypes3);
			var measures3 = measures3Mock.Object;

			var unitPartListMock = new Mock<IHasPartDimensions>();
			unitPartListMock.Setup(x => x.HasPackageType).Returns(true);
			var partList = unitPartListMock.Object;
			var unitPartMock = new Mock<IRateablePart>();
			unitPartMock.Setup(x => x.PackageType).Returns("PLT");
			var part1 = unitPartMock.Object;

			var lineProvider = new FastLineProvider(criteria);
			var fastLine20GP = lineProvider.GetOrCreate(line20GP);
			var fastLineNoContainerType = lineProvider.GetOrCreate(lineNoContainerType);

			var comparer = new ContainerTypeComparer();
			AssertEquals("line with no container type vs part with no container type", Similarity.Exact, comparer.GetSimilarity(fastLineNoContainerType, measures1, partList, part1, null));
			AssertEquals("line with container type X, part with no container type, other part list has only X", Similarity.Exact, comparer.GetSimilarity(fastLine20GP, measures1, partList, part1, null));

			AssertEquals("line with container type X, part with no container type, other part list has only X and another", Similarity.Lowest, comparer.GetSimilarity(fastLine20GP, measures2, partList, part1, null));

			AssertEquals("line with container type X, part with no container type, other part list has only another type", Similarity.None, comparer.GetSimilarity(fastLine20GP, measures3, partList, part1, null));
		}

		static IRateablePart CreateContainerPart(RefContainer container)
			=> CreateContainerPart(container.PK);

		static IRateablePart CreateContainerPart(ZGuid containerPk)
		{
			var partMock = new Mock<IRateablePart>();
			partMock.Setup(x => x.ContainerTypePk).Returns(containerPk.ToGuid());
			return partMock.Object;
		}

		protected TestHelper Helper => testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;
	}
}
