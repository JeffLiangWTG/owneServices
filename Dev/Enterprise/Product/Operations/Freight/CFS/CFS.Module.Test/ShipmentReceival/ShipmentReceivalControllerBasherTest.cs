using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.CFS.GUI;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Module.Testing
{
	[TestedType(typeof(ShipmentReceivalController))]
	sealed class ShipmentReceivalControllerBasherTest : ZControllerBasherTest
	{
		public void TestCFSContextService()
		{
			AssertEquals("CFS controllers must set CFSContextService on factory", FreightDomainContext.CFS, Controller.Factory.GetFreightDomainContext());
		}

		public void TestIsRoot()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			CFSShipment shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment2.JS_JS_ColoadMasterShipment = shipment.PK;

			CFSPackLine packLine = shipment2.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 99;
			packLine.JL_ActualWeight = 111.111m;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			packLine.JL_ActualVolume = 222.222m;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;

			Factory.Save();

			ShipmentReceivalController controller = new ShipmentReceivalController();
			CFSShipment shipment_FreshFactory = new BusinessObjectFactory().Load<CFSShipment>(shipment.PK);
			using (ShipmentReceivalForm shownForm = (ShipmentReceivalForm)controller.ShowEditForm(shipment_FreshFactory))
			{
				Assert("packlines should not be readonly", !((CFSShipment)shownForm.BusinessEntity).OuterPackLines[0].ReadOnly);
			}

			controller = new ShipmentReceivalController();
			shipment_FreshFactory = new BusinessObjectFactory().Load<CFSShipment>(shipment2.PK);
			using (ShipmentReceivalForm shownForm = (ShipmentReceivalForm)controller.ShowEditForm(shipment_FreshFactory))
			{
				Assert("packlines should not be readonly", !((CFSShipment)shownForm.BusinessEntity).OuterPackLines[0].ReadOnly);
			}
		}

		public void TestChildEditableServiceState()
		{
			var shipment = Factory.New<CFSShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			Factory.Save();

			var controller = new ShipmentReceivalController();
			var shipment_FreshFactory = NewFactory().Load<CFSShipment>(shipment.PK);
			using (var shownForm = (ShipmentReceivalForm)controller.ShowEditForm(shipment_FreshFactory))
			{
				AssertEquals(ChildEditableServiceStates.Shipment, ChildEditableService.GetState(controller.Factory));
			}
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			CFSShipment receival = Factory.New<CFSShipment>();
			Factory.Save();
			return receival;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ShipmentReceival;
		}

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var shipment = base.GetBusinessObjectWithoutValidationErrors() as CFSShipment;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.DocAddresses.RemoveAndDeleteAll();
			return shipment;
		}

		#endregion
	}
}
