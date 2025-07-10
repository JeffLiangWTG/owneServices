using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Module.Testing;

[TestedType(typeof(NctsMovementModule))]
sealed class NctsMovementModuleTest : ZModuleBasherTest
{
	public void TestFilterBusinessObject()
	{
		using (var module = new NctsMovementModule())
		{
			AssertType(typeof(NctsMovementFilterStripBusinessObject), module.FilterBusinessObject);
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

	protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.NctsMovementModule;

	protected override string CountryCode => Core.Constants.CountryCodes.Netherlands;
}
