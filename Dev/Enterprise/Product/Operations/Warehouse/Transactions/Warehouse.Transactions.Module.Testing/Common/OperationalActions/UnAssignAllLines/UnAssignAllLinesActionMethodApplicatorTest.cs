using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public abstract class UnAssignAllLinesActionMethodApplicatorTest<TApplicator, TMasterAssigner> : LinesAssignerActionMethodApplicatorTest<TApplicator, TMasterAssigner>
		where TMasterAssigner : BusinessObject, IMasterStaffAssigner
		where TApplicator : UnAssignAllLinesActionMethodApplicator<TMasterAssigner>
	{
		#region Test cases

		public void TestOperationalAction_Unassign_PickCancelled()
		{
			var pickLines = OperationalActionSampleData();

			AssertNotNull("CreatedDocket cannot be null", CreatedDocket);

			AssertEquals("Precondition: Pick lines are assigned", true, pickLines.All(pl => pl.AssignedTo == SelectedUser));

			SetDocketStatus(CancelledStatus);

			var logText = $"WARNING: {OperationName} [HL {JobNo}] - No Lines were un-assigned because the {OperationName} is Canceled.";

			ApplyApplicator(new TMasterAssigner[] { CreatedDocket }, logText);

			AssertEquals("Pick lines cannot be un-assigned.", true, pickLines.All(pl => pl.AssignedTo == SelectedUser));
		}

		public void TestOperationalAction_Unassign_PickFinalised()
		{
			var pickLines = OperationalActionSampleData();

			AssertNotNull("CreatedDocket cannot be null", CreatedDocket);

			AssertEquals("Precondition: Pick lines are assigned", true, pickLines.All(pl => pl.AssignedTo == SelectedUser));

			SetDocketStatus(FinaliseStatus);
			foreach (var line in pickLines)
			{
				line.WZ_PickedDateTime = ZDateTimeOffset.Now;
			}

			var logText = $"WARNING: {OperationName} [HL {JobNo}] - No Lines were un-assigned because the {OperationName} is Finalized.";

			ApplyApplicator(new TMasterAssigner[] { CreatedDocket }, logText);

			AssertEquals("Pick lines cannot be un-assigned.", true, pickLines.All(pl => pl.AssignedTo == SelectedUser));
		}

		public void TestOperationalAction_Unassign_PickFromSourceLocation()
		{
			var pickLines = OperationalActionSampleData();

			AssertNotNull("CreatedDocket cannot be null", CreatedDocket);

			AssertEquals("Precondition: Pick lines are assigned", true, pickLines.All(pl => pl.AssignedTo == SelectedUser));

			foreach (var line in pickLines)
			{
				line.WZ_PickedDateTime = ZDateTimeOffset.Now;
			}

			foreach (var line in pickLines)
			{
				AssertEquals("Stock Picked already", true, line.IsPickedFromPutawayLocation);
			}

			var logText = $"INFO: {OperationName} [HL {JobNo}] - No Lines were un-assigned because there are no assigned lines, or Picking has already commenced.";

			ApplyApplicator(new TMasterAssigner[] { CreatedDocket }, logText);

			AssertEquals("Pick lines cannot be un-assigned.", true, pickLines.All(pl => pl.AssignedTo == SelectedUser));
		}

		public void TestOperationalAction_Unassign_AllLinesIsPicking()
		{
			var pickLines = OperationalActionSampleData();

			AssertNotNull("CreatedDocket cannot be null", CreatedDocket);

			AssertEquals("Precondition: Pick lines are assigned", true, pickLines.All(pl => pl.AssignedTo == SelectedUser));

			foreach (var line in pickLines)
			{
				line.WZ_IsPicking = true;
			}

			var logText = $"INFO: {OperationName} [HL {JobNo}] - No Lines were un-assigned because there are no assigned lines, or Picking has already commenced.";

			ApplyApplicator(new TMasterAssigner[] { CreatedDocket }, logText);

			AssertEquals("Pick lines cannot be un-assigned.", true, pickLines.All(pl => pl.AssignedTo == SelectedUser));
		}

		public void TestOperationalAction_Unassign_Partial_IsPicking()
		{
			var pickLines = OperationalActionSampleData();

			AssertNotNull("CreatedDocket cannot be null", CreatedDocket);

			AssertEquals("Precondition: Pick lines are assigned", true, pickLines.All(pl => pl.AssignedTo == SelectedUser));

			pickLines.Single(p => p.ProductCode == "P1").WZ_IsPicking = true;

			var logText = $"INFO: {OperationName} [HL {JobNo}] - All lines have been un-assigned successfully.";

			ApplyApplicator(new TMasterAssigner[] { CreatedDocket }, logText);

			AssertEquals("Pick lines cannot be un-assigned.", true, pickLines.Single(p => p.ProductCode == "P1").AssignedTo == SelectedUser);
			AssertEquals("Pick lines have been un-assigned", true, pickLines.Single(p => p.ProductCode == "P2").AssignedTo == null);
		}

		public void TestOperationalAction_Unassign_Partial_PickFromSourceLocation()
		{
			var pickLines = OperationalActionSampleData();

			AssertNotNull("CreatedDocket cannot be null", CreatedDocket);

			AssertEquals("Precondition: Pick lines are assigned", true, pickLines.All(pl => pl.AssignedTo == SelectedUser));

			pickLines.Single(p => p.ProductCode == "P1").WZ_PickedDateTime = ZDateTimeOffset.Now;

			var logText = $"INFO: {OperationName} [HL {JobNo}] - All lines have been un-assigned successfully.";

			ApplyApplicator(new TMasterAssigner[] { CreatedDocket }, logText);

			AssertEquals("Pick lines cannot be un-assigned.", true, pickLines.Single(p => p.ProductCode == "P1").AssignedTo == SelectedUser);
			AssertEquals("Pick lines have been un-assigned.", true, pickLines.Single(p => p.ProductCode == "P2").AssignedTo == null);
		}

		public void TestOperationalAction_Unassign()
		{
			var pickLines = OperationalActionSampleData();

			AssertNotNull("CreatedDocket cannot be null", CreatedDocket);

			AssertEquals("Precondition: Pick lines are assigned", true, pickLines.All(pl => pl.AssignedTo == SelectedUser));

			var logText = $"INFO: {OperationName} [HL {JobNo}] - All lines have been un-assigned successfully.";

			ApplyApplicator(new TMasterAssigner[] { CreatedDocket }, logText);

			AssertEquals("Pick lines have been un-assigned", true, pickLines.All(pl => pl.AssignedTo == null));
		}

		public void TestOperationalAction_Unassign_SelectedUser_ExistInPickLines()
		{
			var pickLines = OperationalActionSampleData();

			var user2 = Helper.CreateGlbStaff("CHA", "CHA");

			AssertNotNull("CreatedDocket cannot be null", CreatedDocket);

			AssertEquals("Precondition: Pick lines are assigned", true, pickLines.All(pl => pl.AssignedTo == SelectedUser));

			pickLines.Single(p => p.ProductCode == "P1").WZ_GS_NKAssignedTo = user2.GS_Code;

			Applicator.SelectedUser = user2.GS_Code;

			AssertEquals("Precondition: Pick line assigned to user CHA", true, pickLines.Single(p => p.ProductCode == "P1").AssignedTo == user2);
			AssertEquals("Precondition: Pick line assigned to user XYZ", true, pickLines.Single(p => p.ProductCode == "P2").AssignedTo == SelectedUser);

			var logText = $"INFO: {OperationName} [HL {JobNo}] - All lines have been un-assigned successfully.";

			ApplyApplicator(new TMasterAssigner[] { CreatedDocket }, logText);

			AssertEquals("Pick lines have been un-assigned.", true, pickLines.Single(p => p.ProductCode == "P1").AssignedTo == null);
			AssertEquals("Pick lines cannot be un-assigned.", true, pickLines.Single(p => p.ProductCode == "P2").AssignedTo == SelectedUser);
		}

		public void TestOperationalAction_Unassign_SelectedUser_NotExistInPickLines()
		{
			var pickLines = OperationalActionSampleData();

			var user2 = Helper.CreateGlbStaff("CHA", "CHA");

			AssertNotNull("CreatedDocket cannot be null", CreatedDocket);

			AssertEquals("Precondition: Pick lines are assigned", true, pickLines.All(pl => pl.AssignedTo == SelectedUser));

			Applicator.SelectedUser = user2.GS_Code;

			var logText = $"INFO: {OperationName} [HL {JobNo}] - All lines have been un-assigned successfully.";

			ApplyApplicator(new TMasterAssigner[] { CreatedDocket }, logText);

			AssertEquals("Pick lines cannot be un-assigned", true, pickLines.All(pl => pl.AssignedTo == SelectedUser));
		}

		#endregion

		#region Implementation

		protected abstract IEnumerable<WhsPickLine> OperationalActionSampleData();

		protected override Type ExpectedApplicatorValidationType => typeof(UnassignAllLinesApplicatorValidation<TMasterAssigner>);

		protected abstract string OperationName { get; }

		protected TMasterAssigner CreatedDocket { get; set; }

		protected abstract string JobNo { get; }

		protected abstract string FinaliseStatus { get; }

		protected abstract string CancelledStatus { get; }

		protected abstract void SetDocketStatus(string status);

		#endregion

		new TApplicator Applicator => (TApplicator)base.Applicator;
	}
}
