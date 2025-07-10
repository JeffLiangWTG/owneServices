using System;
using CargoWise.EntityFramework.Testing;
using WTG.ProductionRules.Business.TransitWarehouseCycleCountAutomation;

namespace Enterprise.Warehouse.Transit.Facts.Testing
{
	public class TransitCycleCountLocationCoreFactTest : TestCaseWithFactory
	{
		public void TestNullLocationTypeCode_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TransitCycleCountLocationCoreFact(Guid.NewGuid(), null, "CLASS1", "AREA1", "ROW1", 1, 1, 1, "ROW1-1-1", "STATUS1", false, 0, 0, 0, null, null));
		}

		public void TestNullLocationClass_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TransitCycleCountLocationCoreFact(Guid.NewGuid(), "Type1", null, "AREA1", "ROW1", 1, 1, 1, "ROW1-1-1", "STATUS1", false, 0, 0, 0, null, null));
		}

		#region TestInventoryChanged

		public void TestInventoryChanged_InventoryLastChangedDateNull_CycleCountLastPerformedDateNull_ExpectFalse() => TestInventoryChanged(null, null, false);
		public void TestInventoryChanged_InventoryLastChangedDateNull_CycleCountLastPerformedDateNotNull_ExpectFalse() => TestInventoryChanged(null, DateTime.Now, false);
		public void TestInventoryChanged_InventoryLastChangedDateNotNull_CycleCountLastPerformedDateNull_ExpectTrue() => TestInventoryChanged(DateTime.Now, null, true);
		public void TestInventoryChanged_InventoryLastChangedDateNotNull_CycleCountLastPerformedDateNotNull_InventoryLastChangedDateGreaterThanCycleCountLastPerformedDate_ExpectTrue() => TestInventoryChanged(DateTime.Now, DateTime.Now.AddMinutes(-1), true);
		public void TestInventoryChanged_InventoryLastChangedDateNotNull_CycleCountLastPerformedDateNotNull_InventoryLastChangedDateLessThanCycleCountLastPerformedDate_ExpectFalse() => TestInventoryChanged(DateTime.Now, DateTime.Now.AddMinutes(1), false);

		void TestInventoryChanged(DateTime? inventoryLastChangedDate, DateTime? cycleCountLastPerformedDate, bool expect)
		{
			var fact = new TransitCycleCountLocationCoreFact(Guid.NewGuid(), "Type1", "CLASS1", "AREA1", "ROW1", 1, 1, 1, "ROW1-1-1", "STATUS1", false, 0, 0, 0, inventoryLastChangedDate, cycleCountLastPerformedDate);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.InventoryChanged), expect, fact.InventoryChanged);
		}

		#endregion

		public void TestNullAreaName_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TransitCycleCountLocationCoreFact(Guid.NewGuid(), "Type1", "CLASS1", null, "ROW1", 1, 1, 1, "ROW1-1-1", "STATUS1", false, 0, 0, 0, null, null));
		}

		public void TestNullRowName_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TransitCycleCountLocationCoreFact(Guid.NewGuid(), "Type1", "CLASS1", "AREA1", null, 1, 1, 1, "ROW1-1-1", "STATUS1", false, 0, 0, 0, null, null));
		}

		public void TestNullLocationStatus_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TransitCycleCountLocationCoreFact(Guid.NewGuid(), "Type1", "CLASS1", "AREA1", "ROW1", 1, 1, 1, "ROW1-1-1", null, false, 0, 0, 0, null, null));
		}

		public void TestFields()
		{
			var pk = Guid.NewGuid();
			var date1 = DateTime.Now;
			var date2 = DateTime.Now;
			var fact = new TransitCycleCountLocationCoreFact(pk, "Type1", "CLASS1", "AREA1", "ROW1", 9, 8, 7, "ROW1-1-1", "STATUS1", true, 1, 2, 3, date1, date2);

			AssertEquals(nameof(TransitCycleCountLocationCoreFact.PK), pk, fact.PK);
			AssertEquals(nameof(ITransitCycleCountLocationCoreFact.PK), pk, ((ITransitCycleCountLocationCoreFact)fact).PK);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.LocationTypeCode), "Type1", fact.LocationTypeCode);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.LocationTypeCode), "Type1", ((ITransitCycleCountLocationCoreFact)fact).LocationTypeCode);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.LocationClass), "CLASS1", fact.LocationClass);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.LocationClass), "CLASS1", ((ITransitCycleCountLocationCoreFact)fact).LocationClass);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.AreaName), "AREA1", fact.AreaName);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.AreaName), "AREA1", ((ITransitCycleCountLocationCoreFact)fact).AreaName);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.RowName), "ROW1", fact.RowName);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.RowName), "ROW1", ((ITransitCycleCountLocationCoreFact)fact).RowName);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.Column), 9, fact.Column);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.Column), 9, ((ITransitCycleCountLocationCoreFact)fact).Column);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.Level), 8, fact.Level);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.Level), 8, ((ITransitCycleCountLocationCoreFact)fact).Level);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.Tray), 7, fact.Tray);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.Tray), 7, ((ITransitCycleCountLocationCoreFact)fact).Tray);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.LocationString), "ROW1-1-1", fact.LocationString);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.LocationString), "ROW1-1-1", ((ITransitCycleCountLocationCoreFact)fact).LocationString);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.LocationStatus), "STATUS1", fact.LocationStatus);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.LocationStatus), "STATUS1", ((ITransitCycleCountLocationCoreFact)fact).LocationStatus);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.CycleCountTaskExists), true, fact.CycleCountTaskExists);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.CycleCountTaskExists), true, ((ITransitCycleCountLocationCoreFact)fact).CycleCountTaskExists);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.CycleCountPathSequence), 1, fact.CycleCountPathSequence);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.CycleCountPathSequence), 1, ((ITransitCycleCountLocationCoreFact)fact).CycleCountPathSequence);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.RowPathSequence), 2, fact.RowPathSequence);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.RowPathSequence), 2, ((ITransitCycleCountLocationCoreFact)fact).RowPathSequence);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.LocationStringSortIndex), 3, fact.LocationStringSortIndex);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.LocationStringSortIndex), 3, ((ITransitCycleCountLocationCoreFact)fact).LocationStringSortIndex);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.InventoryLastChangedDate), date1, fact.InventoryLastChangedDate);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.InventoryLastChangedDate), date1, ((ITransitCycleCountLocationCoreFact)fact).InventoryLastChangedDate);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.CycleCountLastPerformedDate), date2, fact.CycleCountLastPerformedDate);
			AssertEquals(nameof(TransitCycleCountLocationCoreFact.CycleCountLastPerformedDate), date2, ((ITransitCycleCountLocationCoreFact)fact).CycleCountLastPerformedDate);
		}
	}
}
