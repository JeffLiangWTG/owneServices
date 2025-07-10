using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using WTG.ProductionRules.Business.ProductWarehouseCycleCountTaskCreation;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	public class CycleCountLocationCoreFactTest : TestCaseWithFactory
	{
		public void TestNullLocationTypeCode_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CycleCountLocationCoreFact(Guid.NewGuid(), ZGuid.BrettsGuid.ToGuid(), null, "CLASS1", "AREA1", "ROW1", 1, 1, 1, "ROW1-1-1", "STATUS1", "CAR", false, 0, 0, 0, null, null, 0m, false, "PWA", 1));
		}

		public void TestNullLocationClass_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CycleCountLocationCoreFact(Guid.NewGuid(), ZGuid.BrettsGuid.ToGuid(), "Type1", null, "AREA1", "ROW1", 1, 1, 1, "ROW1-1-1", "STATUS1", "CAR", false, 0, 0, 0, null, null, 0m, false, "PWA", 1));
		}

		public void TestNullAreaName_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CycleCountLocationCoreFact(Guid.NewGuid(), ZGuid.BrettsGuid.ToGuid(), "Type1", "CLASS1", null, "ROW1", 1, 1, 1, "ROW1-1-1", "STATUS1", "CAR", false, 0, 0, 0, null, null, 0m, false, "PWA", 1));
		}

		public void TestNullRowName_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CycleCountLocationCoreFact(Guid.NewGuid(), ZGuid.BrettsGuid.ToGuid(), "Type1", "CLASS1", "AREA1", null, 1, 1, 1, "ROW1-1-1", "STATUS1", "CAR", false, 0, 0, 0, null, null, 0m, false, "PWA", 1));
		}

		public void TestNullLocationStatus_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CycleCountLocationCoreFact(Guid.NewGuid(), ZGuid.BrettsGuid.ToGuid(), "Type1", "CLASS1", "AREA1", "ROW1", 1, 1, 1, "ROW1-1-1", null, "CAR", false, 0, 0, 0, null, null, 0m, false, "PWA", 1));
		}

		public void TestNullPickMethod_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CycleCountLocationCoreFact(Guid.NewGuid(), ZGuid.BrettsGuid.ToGuid(), "Type1", "CLASS1", "AREA1", "ROW1", 1, 1, 1, "ROW1-1-1", "STATUS1", null, false, 0, 0, 0, null, null, 0m, false, "PWA", 1));
		}

		public void TestNullGranularity_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CycleCountLocationCoreFact(Guid.NewGuid(), ZGuid.BrettsGuid.ToGuid(), "Type1", "CLASS1", "AREA1", "ROW1", 1, 1, 1, "ROW1-1-1", "STATUS1", "CAR", false, 0, 0, 0, null, null, 0m, false, null, 1));
		}

		public void TestFields()
		{
			var pk = Guid.NewGuid();
			var entityPK = Guid.NewGuid();
			var date1 = DateTime.Now;
			var date2 = DateTime.Now;
			var fact = new CycleCountLocationCoreFact(pk, entityPK, "Type1", "CLASS1", "AREA1", "ROW1", 9, 8, 7, "ROW1-1-1", "STATUS1", "CAR", true, 1, 2, 3, date1, date2, 4m, true, "PWA", 7);

			AssertEquals(nameof(CycleCountLocationCoreFact.PK), pk, fact.PK);
			AssertEquals(nameof(ICycleCountLocationCoreFact.PK), pk, ((ICycleCountLocationCoreFact)fact).PK);
			AssertEquals(nameof(CycleCountLocationCoreFact.PK), entityPK, fact.EntityPK);
			AssertEquals(nameof(ITaskManagementGroupingFact.PK), entityPK, ((ITaskManagementGroupingFact)fact).PK);
			AssertEquals(nameof(CycleCountLocationCoreFact.LocationTypeCode), "Type1", fact.LocationTypeCode);
			AssertEquals(nameof(CycleCountLocationCoreFact.LocationTypeCode), "Type1", ((ICycleCountLocationCoreFact)fact).LocationTypeCode);
			AssertEquals(nameof(CycleCountLocationCoreFact.LocationClass), "CLASS1", fact.LocationClass);
			AssertEquals(nameof(CycleCountLocationCoreFact.LocationClass), "CLASS1", ((ICycleCountLocationCoreFact)fact).LocationClass);
			AssertEquals(nameof(CycleCountLocationCoreFact.AreaName), "AREA1", fact.AreaName);
			AssertEquals(nameof(CycleCountLocationCoreFact.AreaName), "AREA1", ((ICycleCountLocationCoreFact)fact).AreaName);
			AssertEquals(nameof(CycleCountLocationCoreFact.RowName), "ROW1", fact.RowName);
			AssertEquals(nameof(CycleCountLocationCoreFact.RowName), "ROW1", ((ICycleCountLocationCoreFact)fact).RowName);
			AssertEquals(nameof(CycleCountLocationCoreFact.Column), 9, fact.Column);
			AssertEquals(nameof(CycleCountLocationCoreFact.Column), 9, ((ICycleCountLocationCoreFact)fact).Column);
			AssertEquals(nameof(CycleCountLocationCoreFact.Level), 8, fact.Level);
			AssertEquals(nameof(CycleCountLocationCoreFact.Level), 8, ((ICycleCountLocationCoreFact)fact).Level);
			AssertEquals(nameof(CycleCountLocationCoreFact.Tray), 7, fact.Tray);
			AssertEquals(nameof(CycleCountLocationCoreFact.Tray), 7, ((ICycleCountLocationCoreFact)fact).Tray);
			AssertEquals(nameof(CycleCountLocationCoreFact.LocationString), "ROW1-1-1", fact.LocationString);
			AssertEquals(nameof(CycleCountLocationCoreFact.LocationString), "ROW1-1-1", ((ICycleCountLocationCoreFact)fact).LocationString);
			AssertEquals(nameof(CycleCountLocationCoreFact.LocationStatus), "STATUS1", fact.LocationStatus);
			AssertEquals(nameof(CycleCountLocationCoreFact.LocationStatus), "STATUS1", ((ICycleCountLocationCoreFact)fact).LocationStatus);
			AssertEquals(nameof(CycleCountLocationCoreFact.PickMethod), "CAR", fact.PickMethod);
			AssertEquals(nameof(CycleCountLocationCoreFact.PickMethod), "CAR", ((ICycleCountLocationCoreFact)fact).PickMethod);
			AssertEquals(nameof(CycleCountLocationCoreFact.CycleCountTaskExists), true, fact.CycleCountTaskExists);
			AssertEquals(nameof(CycleCountLocationCoreFact.CycleCountTaskExists), true, ((ICycleCountLocationCoreFact)fact).CycleCountTaskExists);
			AssertEquals(nameof(CycleCountLocationCoreFact.CycleCountPathSequence), 1, fact.CycleCountPathSequence);
			AssertEquals(nameof(CycleCountLocationCoreFact.CycleCountPathSequence), 1, ((ICycleCountLocationCoreFact)fact).CycleCountPathSequence);
			AssertEquals(nameof(CycleCountLocationCoreFact.RowPathSequence), 2, fact.RowPathSequence);
			AssertEquals(nameof(CycleCountLocationCoreFact.RowPathSequence), 2, ((ICycleCountLocationCoreFact)fact).RowPathSequence);
			AssertEquals(nameof(CycleCountLocationCoreFact.LocationStringSortIndex), 3, fact.LocationStringSortIndex);
			AssertEquals(nameof(CycleCountLocationCoreFact.LocationStringSortIndex), 3, ((ICycleCountLocationCoreFact)fact).LocationStringSortIndex);
			AssertEquals(nameof(CycleCountLocationCoreFact.InventoryLastChangedDate), date1, fact.InventoryLastChangedDate);
			AssertEquals(nameof(CycleCountLocationCoreFact.InventoryLastChangedDate), date1, ((ICycleCountLocationCoreFact)fact).InventoryLastChangedDate);
			AssertEquals(nameof(CycleCountLocationCoreFact.CycleCountLastPerformedDate), date2, fact.CycleCountLastPerformedDate);
			AssertEquals(nameof(CycleCountLocationCoreFact.CycleCountLastPerformedDate), date2, ((ICycleCountLocationCoreFact)fact).CycleCountLastPerformedDate);
			AssertEquals(nameof(CycleCountLocationCoreFact.StockOnHand), 4m, fact.StockOnHand);
			AssertEquals(nameof(CycleCountLocationCoreFact.StockOnHand), 4m, ((ICycleCountLocationCoreFact)fact).StockOnHand);
			AssertEquals(nameof(CycleCountLocationCoreFact.HasCommittedStock), true, fact.HasCommittedStock);
			AssertEquals(nameof(CycleCountLocationCoreFact.HasCommittedStock), true, ((ICycleCountLocationCoreFact)fact).HasCommittedStock);
			AssertEquals(nameof(CycleCountLocationCoreFact.Priority), 7, fact.Priority);
			AssertEquals(nameof(CycleCountLocationCoreFact.Granularity), "PWA", fact.Granularity);
			AssertEquals(nameof(ITaskManagementGroupingFact.NumberOfLines), 1, ((ITaskManagementGroupingFact)fact).NumberOfLines);
			AssertEquals(nameof(ITaskManagementGroupingFact.NumberOfUnits), 0, ((ITaskManagementGroupingFact)fact).NumberOfUnits);
			AssertEquals(nameof(ITaskManagementGroupingFact.NumberOfPacks), 0, ((ITaskManagementGroupingFact)fact).NumberOfPacks);
			AssertEquals(nameof(ITaskManagementGroupingFact.Weight), 0m, ((ITaskManagementGroupingFact)fact).Weight);
			AssertEquals(nameof(ITaskManagementGroupingFact.WeightUQ), string.Empty, ((ITaskManagementGroupingFact)fact).WeightUQ);
			AssertEquals(nameof(ITaskManagementGroupingFact.Volume), 0m, ((ITaskManagementGroupingFact)fact).Volume);
			AssertEquals(nameof(ITaskManagementGroupingFact.VolumeUQ), string.Empty, ((ITaskManagementGroupingFact)fact).VolumeUQ);
		}
	}
}
