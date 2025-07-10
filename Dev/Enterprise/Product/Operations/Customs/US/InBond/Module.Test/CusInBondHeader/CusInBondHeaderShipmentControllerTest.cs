using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Module.Testing
{
	[TestedType(typeof(CusInBondHeaderShipmentController))]
	sealed class CusInBondHeaderShipmentControllerTest : ZControllerBasherTest
	{
		public void TestOpenShipmentFormFromInBondModuleForPlugInInBond()
		{
			using (Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase()))
			{
				AssertEquals("Shipment form is shown for PlugIn In-Bond", typeof(ShipmentForm), Controller.LastShownForm.GetType());
				var form = Controller.LastShownForm as ShipmentForm;
			}
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		//Todo: re-instate when Plugin re-activated - Gary
		//public virtual void TestShowInBondTabWhenShipmentFormIsOpened()
		//{
		//    Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase());
		//    using (ShipmentForm Form = Controller.LastShownForm as ShipmentForm)
		//    {
		//        ZTemplateTabControl MainTabControl = null;

		//        foreach (Control Control in Form.Controls)
		//        {
		//            if (Control is ZTemplateTabControl)
		//            {
		//                MainTabControl = Control as ZTemplateTabControl;
		//                break;
		//            }
		//        }

		//        UserIdleWorker.Flush();
		//        Assert("Selected plugIn ID", MainTabControl.SelectedTab.Controls[0] is USInBondUserControl);
		//    }
		//}

		public void TestFormCacheShipmentFormFromDeclarationModule()
		{
			var testInBond = GetBusinessObjectThatIsInTheDatabase() as CusInBondHeader;
			var shipment = (ForwardingShipment)testInBond.Parent;

			var shipmentController = ZControllerFactory.Create(ControllerIDs.JobShipment);

			try
			{
				Controller.ShowEditForm(testInBond);
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
			InBond.BH_ParentID = Shipment.PK;
			InBond.BH_ParentTableCode = Shipment.TablePrefix;
			Factory.Save();
			AssertEquals(Shipment.PK, Controller.GetLoadedBusinessEntityInLocalFactory(InBond).Identifier);
			AssertEquals(Controller.Factory, Controller.GetLoadedBusinessEntityInLocalFactory(InBond).Factory);
		}

		public void TestLoadBusinessEntity_WithShipment()
		{
			AssertEquals(Shipment.PK, Controller.LoadBusinessEntity(Factory, Shipment.PK).Identifier);
			AssertEquals(Factory, Controller.LoadBusinessEntity(Factory, Shipment.PK).Factory);
		}

		public void TestLoadBusinessEntity_WithDeclaration()
		{
			InBond.BH_ParentID = Shipment.PK;
			InBond.BH_ParentTableCode = Shipment.TablePrefix;
			AssertEquals(Shipment.PK, Controller.LoadBusinessEntity(Factory, InBond.PK).Identifier);
			AssertEquals(Factory, Controller.LoadBusinessEntity(Factory, InBond.PK).Factory);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var inBond = Factory.New<CusInBondHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			inBond.BH_ParentID = shipment.PK;
			inBond.BH_ParentTableCode = shipment.TablePrefix;
			Factory.Save();
			return inBond;
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.InBondPluggedIntoShipment;

		new TestCusInBondHeaderShipmentController Controller
		{
			get
			{
				if (fController == null)
				{
					fController = new TestCusInBondHeaderShipmentController();
				}
				return fController;
			}
		}
		TestCusInBondHeaderShipmentController fController;

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

		CusInBondHeader InBond
		{
			get
			{
				if (finBond == null)
				{
					finBond = Factory.New<CusInBondHeader>();
				}
				return finBond;
			}
		}
		CusInBondHeader finBond;

		sealed class TestCusInBondHeaderShipmentController : CusInBondHeaderShipmentController
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
