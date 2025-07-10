using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.CFS.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Module.Testing
{
	[TestedType(typeof(ShipmentGatePassController))]
	sealed class ShipmentGatePassControllerBasherTest : ZControllerBasherTest
	{
		public void TestCFSContextService()
		{
			AssertEquals("CFS controllers must set CFSContextService on factory", FreightDomainContext.CFS, Controller.Factory.GetFreightDomainContext());
		}

		public void TestIsRoot()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			GatePassShipment shipment2 = Factory.New<GatePassShipment>();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment2.JS_JS_ColoadMasterShipment = shipment.PK;

			GatePassPackLine packLine = shipment2.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 99;
			packLine.JL_ActualWeight = 111.111m;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			packLine.JL_ActualVolume = 222.222m;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;

			Factory.Save();

			ShipmentGatePassController controller = new ShipmentGatePassController();
			GatePassShipment shipment_FreshFactory = new BusinessObjectFactory().Load<GatePassShipment>(shipment.PK);
			using (ShipmentGatePassForm shownForm = (ShipmentGatePassForm)controller.ShowEditForm(shipment_FreshFactory))
			{
				Assert("packlines should not be readonly", !((GatePassShipment)shownForm.BusinessEntity).OuterPackLines[0].ReadOnly);
			}

			controller = new ShipmentGatePassController();
			shipment_FreshFactory = new BusinessObjectFactory().Load<GatePassShipment>(shipment2.PK);
			using (ShipmentGatePassForm shownForm = (ShipmentGatePassForm)controller.ShowEditForm(shipment_FreshFactory))
			{
				Assert("packlines should not be readonly", !((GatePassShipment)shownForm.BusinessEntity).OuterPackLines[0].ReadOnly);
			}
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			GatePassShipment gatePass = Factory.New<GatePassShipment>();
			Factory.Save();
			return gatePass;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ShipmentGatePass;
		}

		#endregion
	}
}
