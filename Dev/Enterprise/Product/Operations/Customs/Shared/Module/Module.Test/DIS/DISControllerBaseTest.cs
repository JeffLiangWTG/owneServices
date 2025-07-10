using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Customs.Module.Testing
{
	public abstract class DISControllerBaseTest : ZControllerBasherTest
	{
		public override void TestNewForm()
		{
			Assert("this involves Factory.New where the top biz object for this controller is non-persistent", true);
		}

		public abstract void TestSecurity();

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.DocumentImageSystem;
	}
}
