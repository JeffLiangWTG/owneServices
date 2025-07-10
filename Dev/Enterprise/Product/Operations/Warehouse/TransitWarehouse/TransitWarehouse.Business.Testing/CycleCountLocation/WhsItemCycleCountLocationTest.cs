using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemCycleCountLocation))]
	class WhsItemCycleCountLocationTest : EnterpriseBusinessObjectTestCase
	{
		#region TestNoteTypes

		public void TestNoteTypes()
		{
			var cycleCountLocation = Factory.New<WhsItemCycleCountLocation>();
			AssertContainsExactElementsInAnyOrder(new PredefinedNoteType[] {
				PredefinedNoteTypes.Instance.CycleCountNotes }, cycleCountLocation.NoteTypes);
		}

		#endregion

		#region INumberFountain

		public void TestINumberFountainConsumer()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateCycleCountLocation(location1, CycleCountLocationStatuses.Codes.Completed, now, now.AddHours(1), now.AddHours(1), "TTT");
			AssertEquals("WhsCycleCountLocation uses correct Fountain.", Env.NumberFountains.TransitWarehouseCycleCountID, ((INumberFountainConsumer)cycleCount).Fountain);

			cycleCount.WIC_JobID = "CC00000001";
			AssertEquals("ID refers to correct Field.", "CC00000001", ((INumberFountainConsumer)cycleCount).ID);

			((INumberFountainConsumer)cycleCount).ID = "CC00000002";
			AssertEquals("ID refers to correct Field.", "CC00000002", cycleCount.WIC_JobID);
		}

		public void TestINumberFountainConsumer_SavingSetsJobID()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateCycleCountLocation(location1, CycleCountLocationStatuses.Codes.Completed, now, now.AddHours(1), now.AddHours(1), "TTT");
			AssertEquals("Precondition: Cycle Count Location Job ID is empty.", "", cycleCount.WIC_JobID);

			Factory.Save();
			AssertEquals("Cycle Count Location Job ID was set correctly.", "CC00000001", cycleCount.WIC_JobID);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var warehouse = Helper.CreateWarehouse("TTT", "A", 2, 1);
			factory.Save();

			return Helper.CreateCycleCountLocation(warehouse.DefaultLocation, CycleCountLocationStatuses.Codes.NotStarted, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty);
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		#endregion
	}

	#region Triggers_PreventCreatingCycleCountLocationsWithOpenVariance

	[TestedType(typeof(WhsItemCycleCountLocation))]
	public class Triggers_PreventCreateCycleCountLocationsWithOpenVariance : DeferrableTriggerTestCase<WhsItemCycleCountLocation>
	{
		#region TestTG_PreventCreateCycleCountLocationsWithOpenVariance

		[ExpectNoExceptions]
		public void TestTG_PreventCreateCycleCountLocationsWithOpenVariance_Insert()
		{
			var now = DateTimeOffset.Now;
			var warehouse = Helper.CreateWarehouse("TTT", "A", 2, 1);
			Factory.Save();

			var cycleCount1 = Helper.CreateCycleCountLocation(warehouse.DefaultLocation, CycleCountLocationStatuses.Codes.Completed, now, now.AddHours(1), now.AddHours(1), "WWW");
			var openVariance = Helper.CreateCycleCountLocationVariance(cycleCount1, packageNotInWhsID: "P1");
			Factory.Save();

			var cycleCount2 = Helper.CreateCycleCountLocation(warehouse.DefaultLocation, CycleCountLocationStatuses.Codes.Completed, now, now.AddHours(1), now.AddHours(1), "WWW");
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsItemCycleCountLocation.PreventCreateIfLocationHasOpenVarianceTriggerError, true), "Trigger should prevent inserting cycle count location for same location with open variance.");
		}

		#endregion

		#region Implementation

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		#endregion
	}

	#endregion
}
