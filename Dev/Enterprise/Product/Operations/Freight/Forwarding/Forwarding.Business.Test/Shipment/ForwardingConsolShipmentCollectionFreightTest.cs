using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingConsolShipmentCollectionFreightTest : BaseFreightTest
	{
		public void TestOnlyForwardRegisteredShipmentAppearInList()
		{
			GlbBranch.CurrentBranch.OrgProxy.OH_IsUnpackDepot = true;
			AssertEquals("Preconditions: CurrentBranch HomePort should be in CurrentBranch Country.", true, GlbBranch.CurrentBranch.Country.ContainsUNLOCO(GlbBranch.CurrentBranch.HomePort));

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.HomePort.RL_Code;
			AssertEquals("Preconditions: Consol should be discharging locally", true, ImportExportHelper.IsBranchCountry(consol.JK_RL_NKDischargePort));

			Transport transport = consol.Transports[0];
			transport.JW_JX = ImportSailing.PK;
			consol.JK_OA_UnpackDepotAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;

			consol.Shipments.AddNew();
			Factory.Save();

			BusinessObjectFactory cFSFactory = new BusinessObjectFactory();

			CommonConsol loadList = (CommonConsol)cFSFactory.Load<Integration.CFS.ICFSLoadListConsol>(consol.PK);
			CommonShipment cFSShipment1 = loadList.Shipments.AddNew();
			CommonShipment cFSShipment2 = loadList.Shipments.AddNew();

			loadList.Shipments.Add(cFSShipment1);
			loadList.Shipments.Add(cFSShipment2);

			AssertEquals("CFS Shipment should not be forward registered", false, cFSShipment1.JS_IsForwardRegistered);
			AssertEquals("CFS Shipment should not be forward registered", false, cFSShipment2.JS_IsForwardRegistered);
			AssertEquals("Load list should show 2 shipments", 3, loadList.Shipments.Count);

			cFSShipment2.JS_IsBooking = true;
			cFSFactory.Save();

			BusinessObjectFactory loadFactory = new BusinessObjectFactory();
			ForwardingConsol loadedConsol = loadFactory.Load<ForwardingConsol>(loadList.PK);

			AssertEquals("CFS Shipment should not be forward registered after saved.", false, cFSShipment1.JS_IsForwardRegistered);
			AssertEquals("CFS Shipment should be forward registered after saved.", true, cFSShipment2.JS_IsForwardRegistered);
			AssertEquals("Forwarding List should contains all forward registered shipments", 2, loadedConsol.Shipments.Count);
			AssertEquals("Data Refresh should be filtering collection", 2, consol.Shipments.Count);
		}
	}
}
