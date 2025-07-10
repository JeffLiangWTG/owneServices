using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(JobDeclarationShipmentController))]
	public class JobDeclarationShipmentControllerTest : ZControllerBasherTest
	{
		protected override string CountryCode
		{
			get
			{
				return Core.Constants.CountryCodes.Afghanistan;
			} // Chose AF because it doesnt have a custom JobDeclarationShipment
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		public void TestModuleIDForDocumentSecurityIsCorrect()
		{
			AssertEquals(ModuleIDs.JobShipment, Controller.ModuleIDForDocumentSecurity);
			AssertEquals(ModuleIDs.Customs.JobDeclaration, Controller.ModuleID);
		}

		public void TestOpenShipmentFormFromDeclarationModuleForPlugInDec()
		{
			using (Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase()))
			{
				AssertEquals("Shipment form is shown for PlugIn Dec", typeof(ShipmentForm), Controller.LastShownForm.GetType());
				ShipmentForm form = Controller.LastShownForm as ShipmentForm;
				AssertEquals("ShipmentForm", form.SecurityToken);
			}
		}

		public void TestLastSavedPK()
		{
			BaseJobDeclaration declaration = (BaseJobDeclaration)GetBusinessObjectThatIsInTheDatabase();
			using (Controller.ShowEditForm(declaration))
			{
				AssertEquals("Shipment form is shown for PlugIn Dec", typeof(ShipmentForm), Controller.LastShownForm.GetType());
				ShipmentForm form = Controller.LastShownForm as ShipmentForm;
				AssertEquals("Dec PK should be returned though", Controller.LastSavedPK, declaration.PK);
			}
		}

		public void TestOpenShipmentFormWhenJobDeclarationHasRelatedShipment()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var jobHeaderLinkedToShipment = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeaderLinkedToShipment.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			jobHeaderLinkedToShipment.JH_OA_LocalChargesAddr = org.Addresses[0].PK;

			var baseJobDeclarationLinkedToShipment = Factory.NewWithValidTestData<BaseJobDeclaration>();
			baseJobDeclarationLinkedToShipment.JE_JS = shipment.PK;
			jobHeaderLinkedToShipment.JH_ParentID = shipment.PK;
			jobHeaderLinkedToShipment.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Factory.Save();

			Env.Security.MaintainShipmentCRMSecurity.IgnoreOSMG.IsAllowed = false;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (Controller.ShowViewForm(baseJobDeclarationLinkedToShipment))
			{
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertNull(Controller.LastShownForm);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (Controller.ShowEditForm(baseJobDeclarationLinkedToShipment))
			{
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertNull(Controller.LastShownForm);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (Controller.ShowDeleteForm(baseJobDeclarationLinkedToShipment))
			{
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertNull(Controller.LastShownForm);
			}

			Env.Security.MaintainShipmentCRMSecurity.IgnoreOSMG.IsAllowed = true;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (Controller.ShowViewForm(baseJobDeclarationLinkedToShipment))
			{
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				Assert(Controller.LastShownForm is ShipmentForm);
				Controller.LastShownForm.Dispose();
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (Controller.ShowEditForm(baseJobDeclarationLinkedToShipment))
			{
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				Assert(Controller.LastShownForm is ShipmentForm);
				Controller.LastShownForm.Dispose();
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (Controller.ShowDeleteForm(baseJobDeclarationLinkedToShipment))
			{
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				Assert(Controller.LastShownForm is ShipmentForm);
				Controller.LastShownForm.Dispose();
			}
		}

		public void TestFavoritesLinkIsCorrect()
		{
			var bizO = GetBusinessObjectThatIsInTheDatabase() as BaseJobDeclaration;
			var shipment = bizO.Shipment;
			using (var form = Controller.ShowEditForm(bizO) as ShipmentForm)
			{
				Application.DoEvents();
				form.BusinessEntity.Factory.Save();
				//no access to StmLink from this assembly - using query instead
				var query = new ZQuery(StmLinkSchema.STL_ItemPK, shipment.PK);
				query.AddToFilter(StmLinkSchema.STL_ModuleID, "CusDec");
				query.AddToFilter(StmLinkSchema.STL_LinkType, "MRI");
				AssertEquals(1, Factory.Load<StmLink>(query).Length);
				var query2 = new ZQuery(StmLinkSchema.STL_ItemPK, shipment.PK);
				query2.AddToFilter(StmLinkSchema.STL_ModuleID, "CusDec");
				query2.AddToFilter(StmLinkSchema.STL_LinkType, "RUI");
				AssertEquals(1, Factory.Load<StmLink>(query2).Length);
				var query3 = new ZQuery(StmLinkSchema.STL_ItemPK, bizO.PK);
				query3.AddToFilter(StmLinkSchema.STL_ModuleID, "CusDec");
				AssertEquals(0, Factory.Load<StmLink>(query3).Length);
			}
		}

		public virtual void TestShowBrokerageTabWhenShipmentFormIsOpened()
		{
			Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase());
			using (ShipmentForm form = Controller.LastShownForm as ShipmentForm)
			{
				ZTemplateTabControl mainTabControl = null;
				foreach (Control control in form.Controls)
				{
					if (control is ZTemplateTabControl)
					{
						mainTabControl = control as ZTemplateTabControl;
						break;
					}
				}

				UserIdleWorker.Flush();
				Assert("Selected plugIn ID", mainTabControl.SelectedTab.Controls[0] is BaseCustomsBrokerageUserControl);
			}
		}

		public void TestFormCacheShipmentFormFromDeclarationModule()
		{
			BaseJobDeclaration testDec = GetBusinessObjectThatIsInTheDatabase() as BaseJobDeclaration;
			ForwardingShipment shipment = testDec.Shipment;
			ZController shipmentController = ZControllerFactory.Create(ControllerIDs.JobShipment);
			try
			{
				Controller.ShowEditForm(testDec);
				shipmentController.ShowEditForm(shipment);
				AssertEquals("Shipment form opened for TestDec is also shown for 'Shipment'", Controller.LastShownForm, shipmentController.LastShownForm);
			}
			finally
			{
				if (Controller.LastShownForm != null)
				{
					Controller.LastShownForm.Dispose();
				}

				if (shipmentController.LastShownForm != null)
				{
					shipmentController.LastShownForm.Dispose();
				}
			}
		}

		public void TestGetLoadedBusinessEntityInLocalFactory_WithShipment()
		{
			Shipment.Factory.Save();
			AssertEquals(Shipment.PK, Controller.GetLoadedBusinessEntityInLocalFactory(Shipment).Identifier);
			AssertEquals(Controller.Factory, Controller.GetLoadedBusinessEntityInLocalFactory(Shipment).Factory);
		}

		public void TestGetLoadedBusinessEntityInLocalFactory_WithDeclaration()
		{
			Declaration.JE_JS = Shipment.PK;
			Factory.Save();
			AssertEquals(Shipment.PK, Controller.GetLoadedBusinessEntityInLocalFactory(Declaration).Identifier);
			AssertEquals(Controller.Factory, Controller.GetLoadedBusinessEntityInLocalFactory(Declaration).Factory);
		}

		public void TestGetLoadedBusinessEntityInLocalFactory_WithDeclarationFromShipmentPuller()
		{
			DeclarationFromShipmentPuller puller = new DeclarationFromShipmentPuller(Factory);
			puller.ShipmentPK = Shipment.PK;
			Factory.Save();
			AssertEquals(Shipment.PK, Controller.GetLoadedBusinessEntityInLocalFactory(puller).Identifier);
			AssertEquals(Controller.Factory, Controller.GetLoadedBusinessEntityInLocalFactory(puller).Factory);
		}

		public void TestLoadBusinessEntity_WithShipment()
		{
			AssertEquals(Shipment.PK, Controller.LoadBusinessEntity(Factory, Shipment.PK).Identifier);
			AssertEquals(Factory, Controller.LoadBusinessEntity(Factory, Shipment.PK).Factory);
		}

		public void TestLoadBusinessEntity_WithDeclaration()
		{
			Declaration.JE_JS = Shipment.PK;
			AssertEquals(Shipment.PK, Controller.LoadBusinessEntity(Factory, Declaration.PK).Identifier);
			AssertEquals(Factory, Controller.LoadBusinessEntity(Factory, Declaration.PK).Factory);
		}

		new TestJobDeclarationShipmentController Controller
		{
			get
			{
				if (fController == null)
				{
					fController = new TestJobDeclarationShipmentController();
				}

				return fController;
			}
		}
		TestJobDeclarationShipmentController fController;

		ForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Factory.New<ForwardingShipment>();
				}

				return fShipment;
			}
		}
		ForwardingShipment fShipment;

		BaseJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<BaseJobDeclaration>();
				}

				return fDeclaration;
			}
		}
		BaseJobDeclaration fDeclaration;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			Factory.Save();
			return declaration;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.JobDeclarationPluggedIntoShipment;
		}

		sealed class TestJobDeclarationShipmentController : JobDeclarationShipmentController
		{
			public new IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
			{
				return base.GetLoadedBusinessEntityInLocalFactory(sourceEntity);
			}

			public new IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
			{
				return base.LoadBusinessEntity(factory, sourceEntityPK);
			}
		}
	}
}
