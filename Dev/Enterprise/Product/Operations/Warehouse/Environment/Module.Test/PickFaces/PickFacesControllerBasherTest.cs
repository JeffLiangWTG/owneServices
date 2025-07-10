using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Environment.GUI.PickFaces;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(PickFacesController))]
	class PickFacesControllerBasherTest : WhsControllerBaseBasherTest
	{
		#region TestFormProperties

		public void TestID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ControllerIDs.WhsConfigPickFaces, controller.ID);
		}

		public void TestModuleID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ModuleIDs.WhsConfigPickFaces, controller.ModuleID);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(typeof(WhsPickFaceView), controller.TypeOfTopLevelBusinessObject);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsConfigPickFaces;
		}

		#endregion

		#region TestNewForm

		public override void TestNewForm()
		{
			Assert("Actions not supported", true);
		}

		#endregion

		#region TestDeleteForm

		public override void TestDeleteForm()
		{
			Assert("Actions not supported", true);
		}

		#endregion

		#region TestViewForm

		public override void TestViewForm()
		{
			var data = new DataSetupHelper(Factory);
			var pickFace = data.Helper.CreateProductPickFace(data.Part, data.Client, data.Location);
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertControllerNotNull();
			PickFaceForm testForm = null;

			try
			{
				var pickFaceViewBO = Factory.LoadTop1<WhsPickFaceView>(new ZQuery(WhsPickFaceViewSchema.WPV_WL, data.Location.PK));
				testForm = (PickFaceForm)Controller.ShowViewForm(pickFaceViewBO);
				AssertNotNull(testForm);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}

			AssertEquals("Expect no message to be shown.", 1, UnitTestUserNotification.Instance.PreviousMessages.Length);
			Assert("Expect no message to be shown.", UnitTestUserNotification.Instance.PreviousMessages[0].WasNone);
		}

		public void TestViewForm_DoesNotOverrideControllerID()
		{
			var data = new DataSetupHelper(Factory);
			var pickFace = data.Helper.CreateProductPickFace(data.Part, data.Client, data.Location);
			Factory.Save();

			var pickFaceBizO = Factory.LoadTop1<WhsPickFaceView>(new ZQuery());
			var form = Controller.ShowViewForm(pickFaceBizO);
			AssertEquals(ControllerIDs.WhsConfigPickFaces, form.ControllerID);
		}

		#endregion

		#region TestEditForm

		public override void TestEditForm()
		{
			var data = new DataSetupHelper(Factory);
			data.Helper.CreateProductPickFace(data.Part, data.Client, data.Location);
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			AssertControllerNotNull();
			PickFaceForm testForm = null;

			try
			{
				var pickFaceViewBO = Factory.LoadTop1<WhsPickFaceView>(new ZQuery(WhsPickFaceViewSchema.WPV_WL, data.Location.PK));
				testForm = (PickFaceForm)Controller.ShowEditForm(pickFaceViewBO);
				AssertNotNull(testForm);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
			finally
			{
				var testZForm = (ZForm)testForm;
				testZForm?.Close();
			}

			AssertEquals("Expect no message to be shown.", 1, UnitTestUserNotification.Instance.PreviousMessages.Length);
			Assert("Expect no message to be shown.", UnitTestUserNotification.Instance.PreviousMessages[0].WasNone);
		}

		#endregion

		#region TestSecurityCheckpoints

		public void TestSecurityCheckpoints()
		{
			var pickFace = Factory.New<WhsPickFaceView>();
			var controller = new PickFacesController();
			AssertEquals(Env.Security.WhsConfigPickFacesEdit, controller.GetCheckPointForEdit(pickFace));
			AssertEquals(Env.Security.WhsConfigPickFacesView, controller.GetCheckPointForView(pickFace));
			AssertExceptionThrown<NotSupportedException>(() => controller.GetCheckPointForNew(pickFace));
			AssertExceptionThrown<NotSupportedException>(() => controller.GetCheckPointForDelete(pickFace));
		}

		#endregion

		#region TestOverrides

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var warehouse = helper.CreateWarehouse("1", "A");
			var locationType = helper.CreateLocationType("XXX", "XXX Test", false, 1, LocationClasses.Codes.FIX);
			warehouse.DefaultLocation.WLV_WLT_LocationType = locationType.PK;
			Factory.Save();

			return Factory.LoadTop1<WhsPickFaceView>(new ZQuery());
		}

		#endregion

		#region TestFormWithNoAssignedProduct

		public void TestFormWithNoAssignedProduct()
		{
			var data = new DataSetupHelper(Factory);
			var locationType = data.Helper.CreateLocationType("L0", "L01 Test", false, 1, LocationClasses.Codes.FIX);
			data.Location.WLV_WLT_LocationType = locationType.PK;
			Factory.Save();

			AssertControllerNotNull();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var pickFaceViewBO = Factory.LoadTop1<WhsPickFaceView>(new ZQuery(WhsPickFaceViewSchema.WPV_WL, data.Location.PK));
			var testForm = Controller.ShowViewForm(pickFaceViewBO);
			AssertNull(testForm);
			AssertMultilineASCIIEquals("Expect a message to the user.", "Use the product module to assign products to this pick face.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region DataSetupHelper

		class DataSetupHelper
		{
			public DataSetupHelper(BusinessObjectFactory factory)
			{
				Helper = new WhsTestHelperFunctionsEnv(factory);
				Whs = Helper.CreateWarehouse("1");
				Location = Helper.CreateRowAndGenerateLocations(Whs, "A", 1, 1).Locations.Single();
				Client = Helper.CreateClient();
				Part = Helper.CreateProduct(Client, "P1");
			}

			public WhsWarehouse Whs { get; }
			public WhsLocation Location { get; }
			public OrgHeader Client { get; }
			public OrgSupplierPart Part { get; }
			public WhsTestHelperFunctionsEnv Helper { get; }
		}

		#endregion
	}
}
