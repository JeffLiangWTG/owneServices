using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(BillContainersController))]
	internal class BillContainersControllerBasherTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			ZController controller = ZControllerFactory.Create(ControllerIDs.AgencyBillContainers);
			AssertEquals(ModuleIDs.AgencyBillContainers, controller.ModuleID);
		}

		public void TestReUseExistingBillOfLadingForms()
		{
			BillOfLading shipment = Factory.New<BillOfLading>();
			BillOfLadingContainer container = shipment.RealContainers.AddNew();
			Factory.Save();
			ZController billController = ZControllerFactory.Create(ControllerIDs.AgencyBillOfLading);
			ZController detentionController = ZControllerFactory.Create(ControllerIDs.AgencyBillContainers);
			using (IZForm form1 = billController.ShowEditForm(shipment))
			using (IZForm form2 = detentionController.ShowEditForm(container))
			{
				AssertSame("Should be re-using the same form", form1, form2);
			}
		}

		public void TestBillOfLadingFormShouldHaveNewButtonDisabled()
		{
			BillOfLading shipment = Factory.New<BillOfLading>();
			BillOfLadingContainer container = shipment.RealContainers.AddNew();
			Factory.Save();
			ZController controller = ZControllerFactory.Create(ControllerIDs.AgencyBillContainers);
			using (IZForm form = controller.ShowEditForm(container))
			{
				AssertEquals("Form should have NEW button disabled", ODisplayMode.NewSaved, ((ZForm)form).DisplayMode);
			}
		}

		public void TestReloadReturnsBillOfLadingForm()
		{
			BillOfLading shipment = Factory.New<BillOfLading>();
			BillOfLadingContainer container = shipment.RealContainers.AddNew();
			Factory.Save();
			ZController controller = ZControllerFactory.Create(ControllerIDs.AgencyBillContainers);
			using (var editForm = controller.ShowEditForm(container))
			{
				try
				{
					using (var reloadForm = controller.ShowFormOfGivenDisplayType(editForm.BusinessEntityForPersistingForm as BusinessObject, editForm.DisplayMode))
					{
						Assert(reloadForm is BillOfLadingForm);
					}
				}
				catch (ModuleGuiNotSupportedException)
				{
					Fail("Reload should not cause exception");
				}
			}
		}

		public void TestShowNewFormDenied()
		{
			ShowFormDelegate method = (AgencyShipmentContainer container, ZController controller) =>
			{
				return (ZForm)controller.ShowFormForNewEntity(container);
			};
			GenericShowFormDeniedTest(method);
		}

		public void TestShowTemplateCopyFormDenied()
		{
			ShowFormDelegate method = (AgencyShipmentContainer container, ZController controller) =>
			{
				return (ZForm)controller.ShowTemplateCopyForm(container);
			};
			GenericShowFormDeniedTest(method);
		}

		public void TestShowDeleteFormDenied()
		{
			ShowFormDelegate method = (AgencyShipmentContainer container, ZController controller) =>
			{
				return (ZForm)controller.ShowDeleteForm(container);
			};
			GenericShowFormDeniedTest(method);
		}

		public void TestShowEditFormPrincipalCheck()
		{
			ShowFormDelegate method = (AgencyShipmentContainer container, ZController controller) =>
			{
				return (ZForm)controller.ShowEditForm(container);
			};
			GenericShowFormPrincipalCheckTest(method);
		}

		public void TestShowViewFormPrincipalCheck()
		{
			ShowFormDelegate method = (AgencyShipmentContainer container, ZController controller) =>
			{
				return (ZForm)controller.ShowViewForm(container);
			};
			GenericShowFormPrincipalCheckTest(method);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			ZController controller = ZControllerFactory.Create(GetControllerID());
			BusinessObject bo = GetBusinessObjectThatIsInTheDatabase();
			AssertEquals(bo.GetType(), controller.TypeOfTopLevelBusinessObject);
		}

		#region Implementation
		OrgHeader goodPrincipal;
		OrgHeader badPrincipal;
		delegate ZForm ShowFormDelegate(AgencyShipmentContainer container, ZController controller);
		void GenericShowFormPrincipalCheckTest(ShowFormDelegate method)
		{
			Env.Security.AgencyPrincipalAccess.IsAllowed = false;
			SetupPrincipals();
			AgencyShipmentContainer goodContainer = Factory.New<AgencyShipment>().RealContainers.AddNew();
			goodContainer.Booking.JS_OH_DeliveryAgent = goodPrincipal.PK;
			AgencyShipmentContainer badContainer = Factory.New<AgencyShipment>().RealContainers.AddNew();
			badContainer.Booking.JS_OH_DeliveryAgent = badPrincipal.PK;
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (ZForm form = method(goodContainer, Controller))
			{
				AssertNotNull("Should have shown a form.", form);
				AssertEquals("Should not have shown an error", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (ZForm form = method(badContainer, Controller))
			{
				AssertNull("Should not have shown a form", form);
				AssertEquals("Should show an error", "Error You are not authorized to access shipments for this principal", UnitTestUserNotification.Instance.LastMessage.ToString());
			}

			Env.Security.AgencyPrincipalAccess.IsAllowed = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (ZForm form = method(badContainer, Controller))
			{
				AssertNotNull("Should have shown a form.", form);
				AssertEquals("Should not have shown an error", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		void GenericShowFormDeniedTest(ShowFormDelegate method)
		{
			AgencyShipmentContainer container = Factory.New<AgencyShipment>().RealContainers.AddNew();
			Factory.Save();
			try
			{
				ZForm form = method(container, Controller);
				if (form != null)
				{
					form.Dispose();
				}

				Fail("Should have thrown a ModuleGuiNotSupportedException");
			}
			catch (ModuleGuiNotSupportedException)
			{
				Assert(true);
			}
		}

		void SetupPrincipals()
		{
			goodPrincipal = CreatePrincipal("GOOD");
			badPrincipal = CreatePrincipal("BAD");
			Factory.Save();
			((IOrgsAndWarehousesAccessProvider)GlbStaff.CurrentUser).AddSecurityToAccessOrgOrWarehouse("GOOD");
			AssertEquals("precondition: (GOOD)", true, ShipsAgencyPrincipalCollectionWithSecurityCheck.AllowedAccessTo(goodPrincipal));
			AssertEquals("precondition: (BAD)", false, ShipsAgencyPrincipalCollectionWithSecurityCheck.AllowedAccessTo(badPrincipal));
		}

		OrgHeader CreatePrincipal(string code)
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			result.OH_Code = code;
			result.OH_IsShippingProvider = true;
			result.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			return result;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AgencyBillContainers;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BillOfLading bill = Factory.New<BillOfLading>();
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			Factory.Save();
			return container;
		}
		#endregion
	}
}
