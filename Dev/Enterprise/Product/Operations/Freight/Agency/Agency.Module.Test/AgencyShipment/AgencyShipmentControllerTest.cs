using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Freight.Agency.Module.Testing
{
	public abstract class AgencyShipmentControllerTest : ZControllerBasherTest
	{
		#region TestShowDeleteFormPrincipalCheck

		public void TestShowDeleteFormPrincipalCheck()
		{
			ShowFormDelegate method = delegate(AgencyShipment shipment, ZController controller)
			{
				return (ZForm)controller.ShowDeleteForm(shipment);
			};
			GenericShowFormPrincipalCheckTest(method);
		}

		#endregion

		#region TestShowEditFormPrincipalCheck

		public void TestShowEditFormPrincipalCheck()
		{
			ShowFormDelegate method = delegate(AgencyShipment shipment, ZController controller)
			{
				return (ZForm)controller.ShowEditForm(shipment);
			};
			GenericShowFormPrincipalCheckTest(method);
		}

		#endregion

		#region TestShowTemplateCopyFormPrincipalCheck

		public void TestShowTemplateCopyFormPrincipalCheck()
		{
			ShowFormDelegate method = delegate(AgencyShipment shipment, ZController controller)
			{
				return (ZForm)controller.ShowTemplateCopyForm(shipment);
			};
			GenericShowFormPrincipalCheckTest(method);
		}

		#endregion

		#region TestShowViewFormPrincipalCheck

		public void TestShowViewFormPrincipalCheck()
		{
			ShowFormDelegate method = delegate(AgencyShipment shipment, ZController controller)
			{
				return (ZForm)controller.ShowViewForm(shipment);
			};
			GenericShowFormPrincipalCheckTest(method);
		}

		#endregion

		OrgHeader goodPrincipal;
		OrgHeader badPrincipal;

		#region Implementation

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		delegate ZForm ShowFormDelegate(AgencyShipment shipment, ZController controller);
		void GenericShowFormPrincipalCheckTest(ShowFormDelegate method)
		{
			Env.Security.AgencyPrincipalAccess.IsAllowed = false;

			SetupPrincipals();

			AgencyShipment goodShipment = (AgencyShipment)Factory.New(Controller.TypeOfTopLevelBusinessObject);
			goodShipment.JS_OH_DeliveryAgent = goodPrincipal.PK;

			AgencyShipment badShipment = (AgencyShipment)Factory.New(Controller.TypeOfTopLevelBusinessObject);
			badShipment.JS_OH_DeliveryAgent = badPrincipal.PK;

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (ZForm form = method(goodShipment, Controller))
			{
				AssertNotNull("Should have shown a form.", form);
				AssertEquals("Should not have shown an error", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (ZForm form = method(badShipment, Controller))
			{
				AssertNull("Should not have shown a form", form);
				AssertEquals("Should show an error", "Error You are not authorized to access shipments for this principal", UnitTestUserNotification.Instance.LastMessage.ToString());
			}

			Env.Security.AgencyPrincipalAccess.IsAllowed = true;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (ZForm form = method(badShipment, Controller))
			{
				AssertNotNull("Should have shown a form.", form);
				AssertEquals("Should not have shown an error", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
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

		#endregion
	}
}
