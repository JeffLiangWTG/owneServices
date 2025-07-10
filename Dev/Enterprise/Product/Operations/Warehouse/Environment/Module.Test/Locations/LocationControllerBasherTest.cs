using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(LocationController))]
	class LocationControllerBasherTest : WhsControllerBaseBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsConfigLocation;
		}
	}
}
