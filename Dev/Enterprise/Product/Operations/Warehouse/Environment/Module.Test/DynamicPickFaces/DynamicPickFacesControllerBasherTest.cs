using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(DynamicPickFacesController))]
	class DynamicPickFacesControllerBasherTest : WhsControllerBaseBasherTest
	{
		#region TestFormProperties

		public void TestID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ControllerIDs.WhsConfigDynamicPickFaces, controller.ID);
		}

		public void TestModuleID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ModuleIDs.WhsConfigDynamicPickFaces, controller.ModuleID);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(typeof(WhsDynamicPickFaceView), controller.TypeOfTopLevelBusinessObject);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsConfigDynamicPickFaces;
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

		public override void TestViewForm() // DPF Location with assigned product without stock should show form
		{
			var data = new DynamicPickFaceTestHelper(Factory);
			data.CreateAndBindDynamicPickingArea();
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertControllerNotNull();
			OrgSupplierPartForm testForm;

			try
			{
				var wdpBO = Factory.LoadTop1<WhsDynamicPickFaceView>(new ZQuery(WhsDynamicPickFaceViewSchema.WDP_LocationString, data.Location.ToLocationString()));
				testForm = (OrgSupplierPartForm)Controller.ShowViewForm(wdpBO);
				AssertNotNull(testForm);

				Application.DoEvents();
				AssertEquals("Warehouse tab on Details tab is selected", expected: true, testForm.IsWarehouseTabPageSelected());
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}

			AssertEquals("Expect no message to be shown.", 1, UnitTestUserNotification.Instance.PreviousMessages.Length);
			Assert("Expect no message to be shown.", UnitTestUserNotification.Instance.PreviousMessages[0].WasNone);
		}

		public void TestViewForm_DPFLocation_NoStock_NoAssignedProduct_ShouldShowAlert()
		{
			var data = new DynamicPickFaceTestHelper(Factory);
			data.Helper.CreateDynamicPF(data.Whs, data.Location);
			Factory.Save();

			AssertControllerNotNull();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var wdpBO = Factory.LoadTop1<WhsDynamicPickFaceView>(new ZQuery());
			var testForm = Controller.ShowViewForm(wdpBO);
			AssertNull(testForm);
			AssertMultilineASCIIEquals("Expect a message to the user.", "Use the product module to assign products to this dynamic pick face.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestViewForm_DPFLocationWithUnassignedStock_ShouldShowForm()
		{
			var data = new DynamicPickFaceTestHelper(Factory);
			data.Helper.CreateDynamicPF(data.Whs, data.Location);

			var receive = data.TransactionHelper.CreateWhsReceive(data.Client.PK, data.Whs.PK, "R1", new TestNotificationBuffer());
			data.TransactionHelper.CreateWhsReceiveInventoryLine(receive, data.Part.PK, 8m, data.Location.PK);
			Factory.Save();

			// Unassigned pick face with stock or open/pending transactions.
			var wdpBO = Factory.LoadTop1<WhsDynamicPickFaceView>(new ZQuery());
			AssertEquals("Precondition: Product", data.Part.OP_PartNum, wdpBO["WDP_ProductCode"]);
			AssertEquals("Precondition: Location", data.Location.ToLocationString(), wdpBO["WDP_LocationString"]);
			AssertEquals("Precondition: IsAssigned should be false", expected: false, wdpBO["WDP_IsAssigned"]);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertControllerNotNull();

			var testForm = (OrgSupplierPartForm)Controller.ShowViewForm(wdpBO);
			AssertNotNull(testForm);

			Application.DoEvents();
			AssertEquals("Warehouse tab on Details tab is selected", expected: true, testForm.IsWarehouseTabPageSelected());
			AssertEquals("Expect no message to be shown.", 1, UnitTestUserNotification.Instance.PreviousMessages.Length);
			Assert("Expect no message to be shown.", UnitTestUserNotification.Instance.PreviousMessages[0].WasNone);
		}

		public void TestViewForm_DPFLocationWithAssignedStock_ShouldShowForm()
		{
			var data = new DynamicPickFaceTestHelper(Factory);
			data.CreateAndBindDynamicPickingArea();

			var receive = data.TransactionHelper.CreateWhsReceive(data.Client.PK, data.Whs.PK, "R1", new TestNotificationBuffer());
			data.TransactionHelper.CreateWhsReceiveInventoryLine(receive, data.Part.PK, 8m, data.Location.PK);
			Factory.Save();

			// Unassigned pick face with stock or open/pending transactions.
			var wdpBO = Factory.LoadTop1<WhsDynamicPickFaceView>(new ZQuery());
			AssertEquals("Precondition: Product", data.Part.OP_PartNum, wdpBO["WDP_ProductCode"]);
			AssertEquals("Precondition: Location", data.Location.ToLocationString(), wdpBO["WDP_LocationString"]);
			AssertEquals("Precondition: IsAssigned should be true", expected: true, wdpBO["WDP_IsAssigned"]);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertControllerNotNull();

			var testForm = (OrgSupplierPartForm)Controller.ShowViewForm(wdpBO);
			AssertNotNull(testForm);

			Application.DoEvents();
			AssertEquals("Warehouse tab on Details tab is selected", expected: true, testForm.IsWarehouseTabPageSelected());
			AssertEquals("Expect no message to be shown.", 1, UnitTestUserNotification.Instance.PreviousMessages.Length);
			Assert("Expect no message to be shown.", UnitTestUserNotification.Instance.PreviousMessages[0].WasNone);
		}

		public void TestViewForm_DoesNotOverrideControllerID()
		{
			var data = new DynamicPickFaceTestHelper(Factory);
			data.CreateAndBindDynamicPickingArea();
			Factory.Save();

			var wdpBO = Factory.LoadTop1<WhsDynamicPickFaceView>(new ZQuery());
			var form = Controller.ShowViewForm(wdpBO);
			AssertEquals(ControllerIDs.WhsConfigProduct, form.ControllerID);
		}

		#endregion

		#region TestEditForm

		public override void TestEditForm() // DPF Location with assigned product without stock should show form
		{
			var data = new DynamicPickFaceTestHelper(Factory);
			data.CreateAndBindDynamicPickingArea();
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			AssertControllerNotNull();
			OrgSupplierPartForm testForm = null;

			try
			{
				var wdpBO = Factory.LoadTop1<WhsDynamicPickFaceView>(new ZQuery());
				testForm = (OrgSupplierPartForm)Controller.ShowEditForm(wdpBO);
				AssertNotNull(testForm);

				Application.DoEvents();
				AssertEquals("Warehouse tab on Details tab is selected", true, testForm.IsWarehouseTabPageSelected());
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

		public void TestEditForm_DPFLocation_NoStock_NoAssignedProduct_ShouldShowAlert()
		{
			var data = new DynamicPickFaceTestHelper(Factory);
			data.Helper.CreateDynamicPF(data.Whs, data.Location);
			Factory.Save();

			AssertControllerNotNull();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var wdpBO = Factory.LoadTop1<WhsDynamicPickFaceView>(new ZQuery());
			var testForm = Controller.ShowEditForm(wdpBO);
			AssertNull(testForm);
			AssertMultilineASCIIEquals("Expect a message to the user.", "Use the product module to assign products to this dynamic pick face.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region TestSecurityCheckpoints

		public void TestSecurityCheckpoints()
		{
			var wdpView = Factory.New<WhsDynamicPickFaceView>();
			var controller = new DynamicPickFacesController();
			AssertEquals(Env.Security.WhsConfigDynamicPickFacesEdit, controller.GetCheckPointForEdit(wdpView));
			AssertEquals(Env.Security.WhsConfigDynamicPickFacesView, controller.GetCheckPointForView(wdpView));
			AssertExceptionThrown<NotSupportedException>(() => controller.GetCheckPointForNew(wdpView));
			AssertExceptionThrown<NotSupportedException>(() => controller.GetCheckPointForDelete(wdpView));
		}

		#endregion

		#region TestOverrides

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var data = new DynamicPickFaceTestHelper(Factory);
			data.Helper.CreateDynamicPF(data.Whs, data.Location);
			Factory.Save();

			return Factory.LoadTop1<WhsDynamicPickFaceView>(new ZQuery());
		}

		#endregion
	}
}
