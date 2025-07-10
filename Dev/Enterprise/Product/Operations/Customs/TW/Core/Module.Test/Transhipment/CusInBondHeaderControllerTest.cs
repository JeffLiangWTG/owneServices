using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Transhipment.Module.Test
{
	[TestedType(typeof(CusInBondHeaderController))]
	public sealed class CusInBondHeaderControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		public void TestTabText()
		{
			var controller = new CusInBondHeaderController();
			AssertEquals("Transhipment", controller.PluginTabPageCaption.Caption);
		}

		#region Implementation
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var testInBond = Factory.New<CusInBondHeader>();
			Factory.Save();
			return testInBond;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.TW.Transhipment;
		}
		#endregion
	}
}
