using Enterprise.Freight.Forwarding.GUI.DangerousGoods;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing.DangerousGoods
{
	[TestedType(typeof(DangerousGoodsController))]
	public class DangerousGoodsControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.DangerousGoodsPlugin;
	}
}
