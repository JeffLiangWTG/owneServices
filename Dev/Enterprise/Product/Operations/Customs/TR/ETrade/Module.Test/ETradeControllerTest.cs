using Enterprise.Customs.TR.ETrade.Business;
using Enterprise.Customs.TR.ETrade.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.Module.Testing
{
	[TestedType(typeof(ETradeController))]
	public class ETradeControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.TR.ETrade;
		}

		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.Turkey; }
		}

		public void TestGetForm()
		{
			AssertEquals(typeof(ETradeForm), Controller.ShowNewForm().GetType());
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			ETradeController controller = new ETradeController();
			AssertEquals("Should return a AsycudaManifestHeader type", typeof(AsycudaManifestHeader), controller.TypeOfTopLevelBusinessObject);
		}

		public void TestCheckPointForView()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var controller = new ETradeController();
			AssertEquals(Environment.Env.Security.TRETrade, controller.GetCheckPointForView(header));
		}

		public void TestCheckPointForEdit()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var controller = new ETradeController();
			AssertEquals(Environment.Env.Security.TRETrade, controller.GetCheckPointForEdit(header));
		}

		public void TestCheckPointForNew()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var controller = new ETradeController();
			AssertEquals(Environment.Env.Security.TRETrade, controller.GetCheckPointForNew(header));
		}

		public void TestCheckPointForDelete()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var controller = new ETradeController();
			AssertEquals(Environment.Env.Security.TRETrade, controller.GetCheckPointForDelete(header));
		}
	}
}
