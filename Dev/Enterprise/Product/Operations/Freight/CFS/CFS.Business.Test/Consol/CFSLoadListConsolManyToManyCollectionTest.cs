using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSLoadListConsolManyToManyCollectionTest : BaseFreightTest
	{
		public void TestShipmentIsNotDeletedWhenItIsTopLevelBusinessObject()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			Factory.Save();

			CFSShipment shipment = Factory.New<CFSShipment>();
			Factory.Save();
			shipment.Consols.Add(consol);
			shipment.Consols.Remove(consol);
			AssertEquals("Shipment should not be deleted", false, shipment.IsDeleted);
		}

		public void TestLoadListCollectionDoesContainOnlyCFSLoadLists()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S10001000";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUMEL";
			shipment.JS_IsCFSRegistered = true;

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.Shipments.Add(shipment);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_RL_NKLoadPort = HomePort;
			consol2.JK_RL_NKDischargePort = AlternateHomePort;
			consol2.JK_OA_PackDepotAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol2.Shipments.Add(shipment);

			Factory.Save();
			ReleaseFactory();

			var loadConsol1 = Factory.Load<CommonConsol>(consol1.PK);
			AssertEquals("Prerequisite", false, loadConsol1.JK_IsCFS);

			var loadConsol2 = Factory.Load<CommonConsol>(consol2.PK);
			AssertEquals("Prerequisite", true, loadConsol2.JK_IsCFS);

			var cfsShipment = Factory.Load<CFSShipment>(shipment.PK);
			Factory.Save();

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_RL_NKLoadPort = "AUPER";
			cfsShipment.Consols.Add(loadList);

			Factory.Save();
			ReleaseFactory();

			cfsShipment = Factory.Load<CFSShipment>(shipment.PK);

			AssertContainsExactElementsInAnyOrder("consols collection on cfs shipment contains only load lists",
				  new[] { consol2.JK_UniqueConsignRef, loadList.JK_UniqueConsignRef },
				  cfsShipment.Consols.Cast<CFSLoadListConsol>().Select(c => c.JK_UniqueConsignRef));
		}
	}
}
