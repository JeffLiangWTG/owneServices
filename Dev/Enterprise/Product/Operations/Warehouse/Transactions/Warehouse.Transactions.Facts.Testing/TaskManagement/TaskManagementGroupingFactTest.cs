using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	class TaskManagementGroupingFactTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var groupingFact = new TaskManagementGroupingFact(ZGuid.BrettsGuid.ToGuid(), "KG", "M3");
			AssertEquals(nameof(groupingFact.PK), ZGuid.BrettsGuid.ToGuid(), groupingFact.PK);
			AssertEquals(nameof(groupingFact.WeightUQ), "KG", groupingFact.WeightUQ);
			AssertEquals(nameof(groupingFact.VolumeUQ), "M3", groupingFact.VolumeUQ);
		}

		public void TestConstructor_OptionalArguments()
		{
			var groupingFact = new TaskManagementGroupingFact(ZGuid.BrettsGuid.ToGuid());
			AssertEquals(nameof(groupingFact.PK), ZGuid.BrettsGuid.ToGuid(), groupingFact.PK);
			AssertEquals(nameof(groupingFact.WeightUQ), string.Empty, groupingFact.WeightUQ);
			AssertEquals(nameof(groupingFact.VolumeUQ), string.Empty, groupingFact.VolumeUQ);
		}

		public void TestConstructor_NullWeightUQ_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementGroupingFact(ZGuid.BrettsGuid.ToGuid(), null, "M3"));
		}

		public void TestConstructor_NullVolumeUQ_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementGroupingFact(ZGuid.BrettsGuid.ToGuid(), "KG", null));
		}
	}
}
