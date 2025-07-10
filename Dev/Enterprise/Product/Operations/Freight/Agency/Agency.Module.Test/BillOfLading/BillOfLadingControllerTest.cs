using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class BillOfLadingControllerTest : BaseAgencyTest
	{
		#region Test ShowLoadedForm
		public void TestShowLoadedForm_Shipment_View()
		{
			GenericShowLoadedFormTest(FormAction.View);
		}

		public void TestShowLoadedForm_Shipment_Edit()
		{
			GenericShowLoadedFormTest(FormAction.Edit);
		}

		public void TestShowLoadedForm_Shipment_Delete()
		{
			GenericShowLoadedFormTest(FormAction.Delete);
		}

		protected void GenericShowLoadedFormTest(FormAction action)
		{
			BillOfLading shipment = Factory.New<BillOfLading>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			Factory.Save();
			DummyController controller = new DummyController();
			ODisplayMode expectedDisplayMode;
			switch (action)
			{
				case FormAction.Edit:
					expectedDisplayMode = ODisplayMode.Browse;
					break;
				case FormAction.Delete:
					expectedDisplayMode = ODisplayMode.Delete;
					break;
				case FormAction.View:
					expectedDisplayMode = ODisplayMode.ReadOnly;
					break;
				default:
					expectedDisplayMode = ODisplayMode.ReadOnly;
					break;
			}

			controller.ShowLoadedForm(shipment, action);
			using (ZForm form = (ZForm)controller.LastShownForm)
			{
				AssertFormShown(form, expectedDisplayMode);
				if (form.DisplayMode != ODisplayMode.Delete)
				{
					AssertShipmentUnchanged((AgencyShipment)form.BusinessEntity);
				}
				else
				{
					AssertShipmentConverted((AgencyShipment)form.BusinessEntity);
					AssertEquals("BO active/inactive status should have changes", true, ((AgencyShipment)form.BusinessEntity).JS_IsCancelledInfo.HasChanges);
				}

				AssertEquals("The original shipment should not have changed", false, shipment.HasChanges);
			}
		}

		void AssertShipmentConverted(AgencyShipment shipment)
		{
			AssertEquals("BusinessObject should be a BillOfLading", typeof(BillOfLading), shipment.GetType());
			AssertEquals("BillOfLading should be confirmed", true, shipment.IsBillOfLadingStage);
			AssertEquals("BillOfLading should have changes", true, shipment.HasChanges);
		}

		void AssertShipmentUnchanged(AgencyShipment shipment)
		{
			AssertEquals("BusinessObject should be a BillOfLading", typeof(BillOfLading), shipment.GetType());
			AssertEquals("BillOfLading should be confirmed", true, shipment.IsBillOfLadingStage);
			AssertEquals("BillOfLading should not have changes", false, shipment.HasChanges);
		}

		void AssertFormShown(ZForm form, ODisplayMode expectedDisplayMode)
		{
			AssertNotNull("Form should be shown.", form);
			AssertEquals("Correct form", typeof(BillOfLadingForm), form.GetType());
			AssertEquals("Correct DisplayMode", expectedDisplayMode, form.DisplayMode);
		}

		#endregion
		#region Test ShowCopyForm
		public void TestShowCopyForm_BillOfLading()
		{
			var billOfLading = Factory.New<BillOfLading>();
			Factory.Save();
			var controller = ZControllerFactory.Create(ControllerIDs.AgencyBillOfLading);
			using (IZForm form = controller.ShowTemplateCopyForm(billOfLading))
			{
				AssertNotNull("Should have returned a form", form);
				AssertEquals("Should have returned the correct form", typeof(BillOfLadingForm), form.GetType());
			}

			using (IZForm form = controller.ShowCopyAndReverseForm(billOfLading))
			{
				AssertNotNull("Should have returned a form", form);
				AssertEquals("Should have returned the correct form", typeof(BillOfLadingForm), form.GetType());
			}
		}

		#endregion
		public void TestCRMSecurityCheckpoints()
		{
			var bizObj = Factory.NewWithValidTestData<BillOfLading>();
			CRMSecurityProviderTest<BillOfLading>.AssertController(new BillOfLadingController(), bizObj, Env.Security.AgencyBillOfLadingCRMSecurity);
		}

		#region Implementation
		class DummyController : BillOfLadingController
		{
			public new IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
			{
				return base.GetLoadedBusinessEntityInLocalFactory(sourceEntity);
			}

			public new void ShowLoadedForm(IBusiness sourceEntity, FormAction action)
			{
				base.ShowLoadedForm(sourceEntity, action);
			}
		}
		#endregion
	}
}
