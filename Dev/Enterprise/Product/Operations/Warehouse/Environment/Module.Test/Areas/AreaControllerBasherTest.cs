using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(AreaController))]
	class AreaControllerBasherTest : WhsControllerBaseBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsConfigArea;
		}
	}
}
