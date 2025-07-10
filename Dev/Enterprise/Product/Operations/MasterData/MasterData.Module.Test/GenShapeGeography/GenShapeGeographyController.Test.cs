using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterData.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.Module.Tests
{
	[TestedType(typeof(GenShapeGeographyController))]
	public class GenShapeGeographyControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.GenShapeGeography, new GenShapeGeographyController().ModuleID);
		}

		public void TestControllerID()
		{
			AssertEquals("ControllerID", ControllerIDs.GenShapeGeography, new GenShapeGeographyController().ID);
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new TestGenShapeGeographyController();
			AssertType<GenShapeGeographyNewSecurityCheckpoint>("For New", controller.CheckPointForNew_Exposed);
			AssertEquals("For View", Env.Security.GeographyView, controller.CheckPointForView_Exposed);
			AssertEquals("For Edit", Env.Security.GeographyModify, controller.CheckPointForEdit_Exposed);
			AssertEquals("For Delete", Env.Security.GeographyDelete, controller.CheckPointForDelete_Exposed);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var shapeGeography = Factory.NewWithValidTestData<GenShapeGeography>();
			Factory.Save();
			return shapeGeography;
		}

		#region Overrides

		public override void TestNewForm()
		{
			AssertExceptionThrown<SecurityAccessDeniedException>("Show new form is not supported.", () =>
			{
				Controller.ShowNewForm();
			});
		}

		#endregion

		#region Implementation

		protected override ControllerID GetControllerID() => ControllerIDs.GenShapeGeography;

		class TestGenShapeGeographyController : GenShapeGeographyController
		{
			public SecurityCheckpoint CheckPointForNew_Exposed => base.CheckPointForNew;

			public SecurityCheckpoint CheckPointForView_Exposed => base.CheckPointForView;

			public SecurityCheckpoint CheckPointForEdit_Exposed => base.CheckPointForEdit;

			public SecurityCheckpoint CheckPointForDelete_Exposed => base.CheckPointForDelete;
		}

		#endregion
	}
}

