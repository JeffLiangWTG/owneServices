using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(CycleCountWaveController))]
	public class CycleCountWaveControllerTest : ZControllerBasherTest
	{
		#region TestTypeOfTopLevelBusinessObject

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(typeof(WhsCycleCountWave), controller.TypeOfTopLevelBusinessObject);
		}

		#endregion

		#region TestCheckPointForView

		public void TestCheckPointForView()
		{
			var wave = Factory.New<WhsCycleCountWave>();
			var controller = ZControllerFactory.Create(GetControllerID());

			AssertEquals(Env.Security.Warehouse, controller.GetCheckPointForView(wave));
		}

		#endregion

		#region TestCheckPointForEdit

		public void TestCheckPointForEdit()
		{
			var wave = Factory.New<WhsCycleCountWave>();
			var controller = ZControllerFactory.Create(GetControllerID());

			AssertEquals(Env.Security.Warehouse, controller.GetCheckPointForEdit(wave));
		}

		#endregion

		#region TestCheckPointForNew

		public void TestCheckPointForNew()
		{
			var wave = Factory.New<WhsCycleCountWave>();
			var controller = ZControllerFactory.Create(GetControllerID());

			AssertEquals(Env.Security.Warehouse, controller.GetCheckPointForNew(wave));
		}

		#endregion

		#region TestCheckPointForDelete

		public void TestCheckPointForDelete()
		{
			var wave = Factory.New<WhsCycleCountWave>();
			var controller = ZControllerFactory.Create(GetControllerID());

			AssertEquals(Env.Security.Warehouse, controller.GetCheckPointForDelete(wave));
		}

		#endregion

		#region Implementation

		protected override ControllerID GetControllerID() => ControllerIDs.WhsCycleCountWave;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var wave = Factory.New<WhsCycleCountWave>();
			Factory.Save();

			return wave;
		}

		#endregion
	}
}
