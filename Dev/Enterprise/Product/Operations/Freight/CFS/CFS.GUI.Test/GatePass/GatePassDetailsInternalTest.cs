using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.CFS.GUI
{
	sealed class GatePassDetailsInternalTest : BaseFreightTest
	{
		public void TestTransportCoStackOverflow()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			SetupLocalBranchAsDepot();
			SetupLocalBranchAsLocalCartage();

			GatePassShipment shipment = (GatePassShipment)GetImportShipment(typeof(GatePassShipment));

			shipment.ConsigneeDeliveryAddress.E2_OA_Address = shipment.Consignee.MainAddress.PK;
			CFSPackLine line = shipment.OuterPackLines.AddNew();
			line.JL_PackageCount = 20;
			line.JL_ActualVolume = 20m;
			line.JL_ActualWeight = 20000m;

			CFSLoadListConsol loadList = shipment.Consols.AddNew();
			loadList.JK_OA_UnpackDepotAddress = LocalDepot.MainAddress.PK;
			loadList.Containers.AddNew();
			line.Containers.Add(loadList.Containers[0]);

			loadList.Containers[0].JC_LCLUnpack = ZDateTime.Today;
			line.JL_Outturn = 20;

			Factory.Save();

			BusinessObjectFactory gatePassFactory = new BusinessObjectFactory();
			GatePassShipment gatePass = gatePassFactory.Load<GatePassShipment>(shipment.PK);
			CommonPickupDeliveryConfirm containerLeg = gatePass.DestinationCFSDepartures.AddNew();
			containerLeg.EU_VehicleRegistration = "ASD";
			containerLeg.EU_PickupDeliveryTime = ZDateTime.Now;

			using (ShipmentGatePassForm form = new ShipmentGatePassForm(gatePass))
			{
				using (GatePassDetails detailsControl = form.GatePassDetailsUserControl)
				{
					form.Show();
					detailsControl.JU_DriversLicenseTextBoxInternal.Focus();
					AssertNotNull(detailsControl);
				}
			}
		}
	}
}
