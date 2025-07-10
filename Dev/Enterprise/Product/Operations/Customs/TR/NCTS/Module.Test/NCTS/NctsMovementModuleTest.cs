using CargoWise.EntityFramework;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Module.Testing
{
	[TestedType(typeof(NctsMovementModule))]
	sealed class NctsMovementModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.EU.NctsMovementModule;
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Turkey;

		public void TestFilterBusinessObject()
		{
			using (var nctsMovementModule = new NctsMovementModule())
			{
				AssertType<NctsMovementFilterStripBusinessObject>(nctsMovementModule.FilterBusinessObject);
			}
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			collection.Add(header);
		}

		public void TestGetNewFilterControl()
		{
			using (var nctsMovementModule = new NctsMovementModuleForTest())
			{
				var nctsMovementFilterControl = nctsMovementModule.GetNewFilterControl_Exposed();
				AssertType<NctsMovementFilterControl>(nctsMovementFilterControl);
				nctsMovementFilterControl.Dispose();
			}
		}

		public override void TestExceptionsFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		public override void TestMilestonesFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		public override void TestAutoAddedMilestoneDateFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		public override void TestAutoAddedTaskStatusFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		public override void TestTasksFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		public override void TestTriggersFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		protected override bool HasController() => true;

		protected override bool ShouldTestFormIsFullyTranslatable => false;
	}

	class NctsMovementModuleForTest : NctsMovementModule
	{
		public IFilterControl GetNewFilterControl_Exposed()
		{
			return base.GetNewFilterControl();
		}
	}
}
