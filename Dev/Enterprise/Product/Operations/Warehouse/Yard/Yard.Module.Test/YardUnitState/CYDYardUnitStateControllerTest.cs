using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDYardUnitStateController))]
	public class CYDYardUnitStateControllerTest : CYDControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CYDYardUnitState;
		}
	}
}
