using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDReceiveAdviceController))]
	public class CYDReceiveAdviceControllerTest : CYDControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CYDReceiveAdvice;
		}
	}
}
