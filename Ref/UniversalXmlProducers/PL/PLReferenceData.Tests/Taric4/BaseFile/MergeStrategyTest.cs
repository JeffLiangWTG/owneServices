using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Taric4;

[TestFixture]
sealed class MergeStrategyTest
{
	[Test]
	public void TestMerge_AddsNewDataPointFromUpdateDataGroup()
	{
		var mergeStrategy = new MergeStrategy<TestDataGroup, TestDataPoint>(originType.N);
		var baseDataGroup = new TestDataGroup
		{
			DataPoints = [new() { hjid = 1, metainfo = new() { opType = OpType.C, origin = originType.N } }]
		};
		var updateDataGroup = new TestDataGroup
		{
			DataPoints = [new() { hjid = 2, metainfo = new() { opType = OpType.C, origin = originType.N } }]
		};

		var mergeDataGroup = mergeStrategy.Merge(baseDataGroup, updateDataGroup);

		Assert.Multiple(() =>
		{
			Assert.That(mergeDataGroup, Is.Not.SameAs(baseDataGroup).And.Not.SameAs(updateDataGroup));
			Assert.That(mergeDataGroup.DataPoints, Is.EquivalentTo(new[] { baseDataGroup.DataPoints[0], updateDataGroup.DataPoints[0] }));
		});
	}

	[Test]
	public void TestMerge_UpdatesBaseDataPointFromUpdateDataGroup()
	{
		var mergeStrategy = new MergeStrategy<TestDataGroup, TestDataPoint>(originType.N);
		var baseDataGroup = new TestDataGroup
		{
			DataPoints =[new() { hjid = 1, metainfo = new() { opType = OpType.C, origin = originType.N } }]
		};
		var updateDataGroup = new TestDataGroup
		{
			DataPoints = [new() { hjid = 1, metainfo = new() { opType = OpType.U, origin = originType.N } }]
		};

		var mergeDataGroup = mergeStrategy.Merge(baseDataGroup, updateDataGroup);

		Assert.Multiple(() =>
		{
			Assert.That(mergeDataGroup, Is.Not.SameAs(baseDataGroup).And.Not.SameAs(updateDataGroup));
			Assert.That(mergeDataGroup.DataPoints, Is.EquivalentTo(new[] { updateDataGroup.DataPoints[0] }));
		});
	}

	[Test]
	public void TestMerge_RemovesBaseDataPointFromUpdateDataGroup()
	{
		var mergeStrategy = new MergeStrategy<TestDataGroup, TestDataPoint>(originType.T);
		var baseDataGroup = new TestDataGroup
		{
			DataPoints = [new() { hjid = 1, metainfo = new() { opType = OpType.C, origin = originType.T } }]
		};
		var updateDataGroup = new TestDataGroup
		{
			DataPoints = [new() { hjid = 1, metainfo = new() { opType = OpType.D, origin = originType.T } }]
		};

		var mergeDataGroup = mergeStrategy.Merge(baseDataGroup, updateDataGroup);

		Assert.Multiple(() =>
		{
			Assert.That(mergeDataGroup, Is.Not.SameAs(baseDataGroup).And.Not.SameAs(updateDataGroup));
			Assert.That(mergeDataGroup.DataPoints, Is.Empty);
		});
	}

	[Test]
	public void TestMerge_RemovesBaseDataPointFromBaseDataGroup()
	{
		var mergeStrategy = new MergeStrategy<TestDataGroup, TestDataPoint>(originType.T);
		var baseDataGroup = new TestDataGroup
		{
			DataPoints =
			[
				new() { hjid = 1, metainfo = new() { opType = OpType.C, origin = originType.T } },
				new() { hjid = 1, metainfo = new() { opType = OpType.U, origin = originType.T } },
				new() { hjid = 1, metainfo = new() { opType = OpType.D, origin = originType.T } }
			]
		};
		var updateDataGroup = new TestDataGroup
		{
			DataPoints = []
		};

		var mergeDataGroup = mergeStrategy.Merge(baseDataGroup, updateDataGroup);

		Assert.Multiple(() =>
		{
			Assert.That(mergeDataGroup, Is.Not.SameAs(baseDataGroup).And.Not.SameAs(updateDataGroup));
			Assert.That(mergeDataGroup.DataPoints, Is.Empty);
		});
	}


	[Test]
	public void TestMerge_DeduplicatesDataPointsFromBaseDataGroup()
	{
		var mergeStrategy = new MergeStrategy<TestDataGroup, TestDataPoint>(originType.N);
		var baseDataGroup = new TestDataGroup
		{
			DataPoints =
			[
				new() { hjid = 1, metainfo = new() { opType = OpType.C, origin = originType.N } },
				new() { hjid = 1, metainfo = new() { opType = OpType.U, origin = originType.N } }
			]
		};
		var updateDataGroup = new TestDataGroup
		{
			DataPoints = []
		};

		var mergeDataGroup = mergeStrategy.Merge(baseDataGroup, updateDataGroup);

		Assert.Multiple(() =>
		{
			Assert.That(mergeDataGroup, Is.Not.SameAs(baseDataGroup).And.Not.SameAs(updateDataGroup));
			Assert.That(mergeDataGroup.DataPoints, Is.EquivalentTo(new[] { baseDataGroup.DataPoints[1] }));
		});
	}

	[Test]
	public void TestMerge_DeduplicatesDataPointsFromUpdateDataGroup()
	{
		var mergeStrategy = new MergeStrategy<TestDataGroup, TestDataPoint>(originType.N);
		var baseDataGroup = new TestDataGroup
		{
			DataPoints = []
		};
		var updateDataGroup = new TestDataGroup
		{
			DataPoints =
			[
				new() { hjid = 1, metainfo = new() { opType = OpType.C, origin = originType.N } },
				new() { hjid = 1, metainfo = new() { opType = OpType.U, origin = originType.N } },
			]
		};

		var mergeDataGroup = mergeStrategy.Merge(baseDataGroup, updateDataGroup);

		Assert.Multiple(() =>
		{
			Assert.That(mergeDataGroup, Is.Not.SameAs(baseDataGroup).And.Not.SameAs(updateDataGroup));
			Assert.That(mergeDataGroup.DataPoints, Is.EquivalentTo(new[] { updateDataGroup.DataPoints[1] }));
		});
	}

	[Test]
	public void TestMerge_FiltersAllDataPointsByOriginType()
	{
		var mergeStrategy = new MergeStrategy<TestDataGroup, TestDataPoint>(originType.N);
		var baseDataGroup = new TestDataGroup
		{
			DataPoints =
			[
				new() { hjid = 1, metainfo = new() { opType = OpType.C, origin = originType.N } },
				new() { hjid = 2, metainfo = new() { opType = OpType.C, origin = originType.T } }
			]
		};
		var updateDataGroup = new TestDataGroup
		{
			DataPoints =
			[
				new() { hjid = 1, metainfo = new() { opType = OpType.U, origin = originType.N } },
				new() { hjid = 2, metainfo = new() { opType = OpType.U, origin = originType.T } }
			]
		};
		var mergeDataGroup = mergeStrategy.Merge(baseDataGroup, updateDataGroup);

		Assert.Multiple(() =>
		{
			Assert.That(mergeDataGroup, Is.Not.SameAs(baseDataGroup).And.Not.SameAs(updateDataGroup));
			Assert.That(mergeDataGroup.DataPoints, Is.EquivalentTo(new[] { updateDataGroup.DataPoints[0] }));
		});
	}

	[Test]
	public void TestMerge_BaseDataGroupHasNoDataPoints()
	{
		var mergeStrategy = new MergeStrategy<TestDataGroup, TestDataPoint>(originType.N);
		var baseDataGroup = new TestDataGroup
		{
			DataPoints = null
		};
		var updateDataGroup = new TestDataGroup
		{
			DataPoints = [new() { hjid = 1, metainfo = new() { opType = OpType.C, origin = originType.N } }]
		};
		var mergeDataGroup = mergeStrategy.Merge(baseDataGroup, updateDataGroup);

		Assert.Multiple(() =>
		{
			Assert.That(mergeDataGroup, Is.Not.SameAs(baseDataGroup).And.Not.SameAs(updateDataGroup));
			Assert.That(mergeDataGroup.DataPoints, Is.EquivalentTo(new[] { updateDataGroup.DataPoints[0] }));
		});
	}

	[Test]
	public void TestMerge_UpdateDataGroupHasNoDataPoints()
	{
		var mergeStrategy = new MergeStrategy<TestDataGroup, TestDataPoint>(originType.N);
		var baseDataGroup = new TestDataGroup
		{
			DataPoints = [new() { hjid = 1, metainfo = new() { opType = OpType.C, origin = originType.N } }]
		};
		var updateDataGroup = new TestDataGroup
		{
			DataPoints = null
		};
		var mergeDataGroup = mergeStrategy.Merge(baseDataGroup, updateDataGroup);

		Assert.Multiple(() =>
		{
			Assert.That(mergeDataGroup, Is.Not.SameAs(baseDataGroup).And.Not.SameAs(updateDataGroup));
			Assert.That(mergeDataGroup.DataPoints, Is.EquivalentTo(new[] { baseDataGroup.DataPoints[0] }));
		});
	}
}
