using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AllocationAdjustmentDetailsTest : BaseAgencyTest
	{
		public void TestAllocated_TEU()
		{
			Allocation.SetAspect(AllocationAspectTypes.TEU, 10);
			AssertEquals(10m, Details.Allocated_TEU);
			Allocation.SetAspect(AllocationAspectTypes.TEU, 20);
			AssertEquals(20m, Details.Allocated_TEU);
		}

		public void TestAllocated_PowerPoints()
		{
			Allocation.SetAspect(AllocationAspectTypes.PowerPoints, 10);
			AssertEquals(10m, Details.Allocated_PowerPoints);
			Allocation.SetAspect(AllocationAspectTypes.PowerPoints, 20);
			AssertEquals(20m, Details.Allocated_PowerPoints);
		}

		public void TestAllocated_Tonnes()
		{
			Allocation.SetAspect(AllocationAspectTypes.Tonnes, 10);
			AssertEquals(10m, Details.Allocated_Tonnes);
			Allocation.SetAspect(AllocationAspectTypes.Tonnes, 20);
			AssertEquals(20m, Details.Allocated_Tonnes);
		}

		public void TestAllocated_Volume()
		{
			Allocation.SetAspect(AllocationAspectTypes.Volume, 10);
			AssertEquals(10m, Details.Allocated_Volume);
			Allocation.SetAspect(AllocationAspectTypes.Volume, 20);
			AssertEquals(20m, Details.Allocated_Volume);
		}

		public void TestAllocated_Area()
		{
			Allocation.SetAspect(AllocationAspectTypes.Area, 10);
			AssertEquals(10m, Details.Allocated_Area);
			Allocation.SetAspect(AllocationAspectTypes.Area, 20);
			AssertEquals(20m, Details.Allocated_Area);
		}

		public void TestTotalRequired_TEU()
		{
			AssertEquals(20m, Details.TotalRequired_TEU);
			required = new AllocationUsage(15, 0, 0, 0, 0, 0);
			details = null;
			AssertEquals(15m, Details.TotalRequired_TEU);
		}

		public void TestTotalRequired_PowerPoints()
		{
			AssertEquals(10, Details.TotalRequired_PowerPoints);
			required = new AllocationUsage(0, 0, 15, 0, 0, 0);
			details = null;
			AssertEquals(15, Details.TotalRequired_PowerPoints);
		}

		public void TestTotalRequired_Tonnes()
		{
			AssertEquals(20m, Details.TotalRequired_Tonnes);
			required = new AllocationUsage(0, 0, 0, 15, 0, 0);
			details = null;
			AssertEquals(15m, Details.TotalRequired_Tonnes);
		}

		public void TestTotalRequired_Volume()
		{
			AssertEquals(20m, Details.TotalRequired_Volume);
			required = new AllocationUsage(0, 0, 0, 0, 15, 0);
			details = null;
			AssertEquals(15m, Details.TotalRequired_Volume);
		}

		public void TestTotalRequired_Area()
		{
			AssertEquals(15m, Details.TotalRequired_Area);
			required = new AllocationUsage(0, 0, 0, 0, 0, 20);
			details = null;
			AssertEquals(20m, Details.TotalRequired_Area);
		}

		public void TestOldOverAllocation_TEU()
		{
			Allocation.SetAspect(AllocationAspectTypes.TEU, 10);
			AssertEquals(11m, Details.OldOverAllocation_TEU);
			Allocation.E0_OverAllocationPercent = 50;
			AssertEquals(15m, Details.OldOverAllocation_TEU);
		}

		public void OldOverAllocation_PowerPoints()
		{
			Allocation.SetAspect(AllocationAspectTypes.PowerPoints, 10);
			AssertEquals(11m, Details.OldOverAllocation_PowerPoints);
			Allocation.E0_OverAllocationPercent = 50;
			AssertEquals(15m, Details.OldOverAllocation_PowerPoints);
		}

		public void TestOldOverAllocation_Tonnes()
		{
			Allocation.SetAspect(AllocationAspectTypes.Tonnes, 20);
			AssertEquals(22m, Details.OldOverAllocation_Tonnes);
			Allocation.E0_OverAllocationPercent = 50;
			AssertEquals(30m, Details.OldOverAllocation_Tonnes);
		}

		public void TestOldOverAllocation_Volume()
		{
			Allocation.SetAspect(AllocationAspectTypes.Volume, 20);
			AssertEquals(22m, Details.OldOverAllocation_Volume);
			Allocation.E0_OverAllocationPercent = 50;
			AssertEquals(30m, Details.OldOverAllocation_Volume);
		}

		public void TestOldOverAllocation_Area()
		{
			Allocation.SetAspect(AllocationAspectTypes.Area, 20);
			AssertEquals(22m, Details.OldOverAllocation_Area);
			Allocation.E0_OverAllocationPercent = 50;
			AssertEquals(30m, Details.OldOverAllocation_Area);
		}

		public void TestOldOverAllocationPercent()
		{
			AssertEquals(10m, Details.OldOverAllocationPercent);
			Allocation.E0_OverAllocationPercent = 50;
			AssertEquals(50m, Details.OldOverAllocationPercent);
		}

		public void TestOldPercentLabelText()
		{
			AssertEquals("  +10%", Details.OldPercentLabelText);
			Allocation.E0_OverAllocationPercent = 50;
			AssertEquals("  +50%", Details.OldPercentLabelText);
		}

		public void TestNewOverAllocation_TEU()
		{
			AssertEquals(0m, Details.NewOverAllocationPercent);
			AssertEquals(0m, Details.NewOverAllocation_TEU);
			Allocation.SetAspect(AllocationAspectTypes.TEU, 10);
			required = new AllocationUsage(12m, 0m, 0, 0m, 0m, 0m);
			details = null;
			AssertEquals(20m, Details.NewOverAllocationPercent);
			AssertEquals(12m, Details.NewOverAllocation_TEU);
			required = new AllocationUsage(14.9m, 0m, 0, 0m, 0m, 0m);
			details = null;
			AssertEquals(49m, Details.NewOverAllocationPercent);
			AssertEquals(14.9m, Details.NewOverAllocation_TEU);
			required = new AllocationUsage(14.901m, 0m, 0, 0m, 0m, 0m);
			details = null;
			AssertEquals(50m, Details.NewOverAllocationPercent);
			AssertEquals(15m, Details.NewOverAllocation_TEU);
		}

		public void TestNewOverAllocation_PowerPoints()
		{
			AssertEquals(0m, Details.NewOverAllocationPercent);
			AssertEquals(0m, Details.NewOverAllocation_PowerPoints);
			Allocation.SetAspect(AllocationAspectTypes.PowerPoints, 10);
			required = new AllocationUsage(0m, 0m, 12, 0m, 0m, 0m);
			details = null;
			AssertEquals(20m, Details.NewOverAllocationPercent);
			AssertEquals(12m, Details.NewOverAllocation_PowerPoints);
			required = new AllocationUsage(0m, 0m, 14, 0m, 0m, 0m);
			details = null;
			AssertEquals(40m, Details.NewOverAllocationPercent);
			AssertEquals(14m, Details.NewOverAllocation_PowerPoints);
		}

		public void TestNewOverAllocation_Tonnes()
		{
			AssertEquals(0m, Details.NewOverAllocationPercent);
			AssertEquals(0m, Details.NewOverAllocation_Tonnes);
			Allocation.SetAspect(AllocationAspectTypes.Tonnes, 10);
			required = new AllocationUsage(0m, 0m, 0, 12m, 0m, 0m);
			details = null;
			AssertEquals(20m, Details.NewOverAllocationPercent);
			AssertEquals(12m, Details.NewOverAllocation_Tonnes);
			required = new AllocationUsage(0m, 0m, 0, 14.9m, 0m, 0m);
			details = null;
			AssertEquals(49m, Details.NewOverAllocationPercent);
			AssertEquals(14.9m, Details.NewOverAllocation_Tonnes);
			required = new AllocationUsage(0m, 0m, 0, 14.901m, 0m, 0m);
			details = null;
			AssertEquals(50m, Details.NewOverAllocationPercent);
			AssertEquals(15m, Details.NewOverAllocation_Tonnes);
		}

		public void TestNewOverAllocation_Volume()
		{
			AssertEquals(0m, Details.NewOverAllocationPercent);
			AssertEquals(0m, Details.NewOverAllocation_Volume);
			Allocation.SetAspect(AllocationAspectTypes.Volume, 10);
			required = new AllocationUsage(0m, 0m, 0, 0m, 12m, 0m);
			details = null;
			AssertEquals(20m, Details.NewOverAllocationPercent);
			AssertEquals(12m, Details.NewOverAllocation_Volume);
			required = new AllocationUsage(0m, 0m, 0, 0m, 14.9m, 0m);
			details = null;
			AssertEquals(49m, Details.NewOverAllocationPercent);
			AssertEquals(14.9m, Details.NewOverAllocation_Volume);
			required = new AllocationUsage(0m, 0m, 0, 0m, 14.901m, 0m);
			details = null;
			AssertEquals(50m, Details.NewOverAllocationPercent);
			AssertEquals(15m, Details.NewOverAllocation_Volume);
		}

		public void TestNewOverAllocation_Area()
		{
			AssertEquals(0m, Details.NewOverAllocationPercent);
			AssertEquals(0m, Details.NewOverAllocation_Area);
			Allocation.SetAspect(AllocationAspectTypes.Area, 10);
			required = new AllocationUsage(0m, 0m, 0, 0m, 0m, 12m);
			details = null;
			AssertEquals(20m, Details.NewOverAllocationPercent);
			AssertEquals(12m, Details.NewOverAllocation_Area);
			required = new AllocationUsage(0m, 0m, 0, 0m, 0m, 14.9m);
			details = null;
			AssertEquals(49m, Details.NewOverAllocationPercent);
			AssertEquals(14.9m, Details.NewOverAllocation_Area);
			required = new AllocationUsage(0m, 0m, 0, 0m, 0m, 14.901m);
			details = null;
			AssertEquals(50m, Details.NewOverAllocationPercent);
			AssertEquals(15m, Details.NewOverAllocation_Area);
		}

		public void TestNewPercentLabelText()
		{
			AssertEquals("  +0%", Details.NewPercentLabelText);
			Allocation.SetAspect(AllocationAspectTypes.TEU, 10);
			required = new AllocationUsage(12, 0, 0, 0, 0, 0);
			details = null;
			AssertEquals("  +20%", Details.NewPercentLabelText);
		}

		public void TestAcceptNewOverAllocationPercent()
		{
			GlbStaff bob = Factory.New<GlbStaff>();
			bob.GS_LoginName = "some.random.guy";
			bob.GS_Code = "BOB";
			JobVoyage voyage = Factory.New<JobVoyage>();
			allocation = voyage.Countries.GetCountry("AU", true).SlotAllocations.GetAllocation(ZGuid.Empty);
			Allocation.E0_UseDefaultOverAllocation = true;
			Allocation.SetAspect(AllocationAspectTypes.TEU, 10);
			required = new AllocationUsage(12, 0, 0, 0, 0, 0);
			StmALog[] logsBefoure = voyage.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Authorised.Code));
			AssertEquals("precondition: Should not have any ActionAuthorised logs yet", 0, logsBefoure.Length);
			Details.Login = bob.GS_LoginName;
			Details.AcceptNewOverAllocation();
			AssertEquals("Should now be overriden", false, Allocation.E0_UseDefaultOverAllocation);
			AssertEquals("Should now be set at 20%", 20m, Allocation.E0_OverAllocationPercent);
			StmALog[] logsAfter = voyage.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Authorised.Code));
			AssertEquals("Should have an ActionAuthorised log now", 1, logsAfter.Length);
			AssertEquals("Should reference correct staff member", bob.GS_Code, logsAfter[0].SL_GS_NKUser);
		}

		public void TestLogin()
		{
			string login = GlbStaff.CurrentUser.GS_LoginName;
			AssertNotEquals("precondition: ", "", login);
			Env.Security.SailingScheduleAllocationEdit.IsAllowed = true;
			AssertEquals("Default the login name if the current user has permissions", login, Details.Login);
			Env.Security.SailingScheduleAllocationEdit.IsAllowed = false;
			details = null;
			AssertEquals("Dont default the login name if the current user does not have permissions", "", Details.Login);
		}

		#region Implementation
		AllocationAdjustmentDetails Details
		{
			get
			{
				if (details == null)
				{
					details = new AllocationAdjustmentDetails(Allocation, Required);
				}

				return details;
			}
		}

		AllocationUsage Required
		{
			get
			{
				if (required == null)
				{
					required = new AllocationUsage(10m, 10m, 10, 20m, 20m, 15);
				}

				return required;
			}
		}

		SlotAllocation Allocation
		{
			get
			{
				if (allocation == null)
				{
					allocation = Factory.New<SlotAllocation>();
					allocation.E0_UseDefaultOverAllocation = false;
					allocation.E0_OverAllocationPercent = 10;
				}

				return allocation;
			}
		}

		AllocationAdjustmentDetails details;
		AllocationUsage required;
		SlotAllocation allocation;
		#endregion
	}
}
