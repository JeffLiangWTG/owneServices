using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class PackLineSynchronisersTest : TestCaseWithFactory
	{
		public void TestSynchronise()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HBL1";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "KRBUS";
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.Shipments.Add(shipment);
			consol.JK_MasterBillNum = "M1";
			var forwardingContainer1 = consol.Containers.AddNew();
			forwardingContainer1.JC_ContainerNum = "C1";
			var forwardingContainer2 = consol.Containers.AddNew();
			forwardingContainer2.JC_ContainerNum = "C2";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_JC = forwardingContainer1.PK;

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;

			declaration.ShipmentSynchroniser.SetEnabled(true, false);
			declaration.ShipmentSynchroniser.Synchronise();

			AssertEquals("Bill is synchronised", 2, declaration.Bills.Count);
			AssertEquals("Bill is read only", true, declaration.Bills[0].CU_BillNumInfo.ReadOnly);
			AssertEquals("1 container", 1, declaration.CusContainers.Count);
			AssertEquals("Container is read only", true, declaration.CusContainers[0].CO_ContainerNumberInfo.ReadOnly);
			Factory.Save();

			declaration.JE_OverrideFreightDefaults = true;
			AssertEquals("Bill is not read only", false, declaration.Bills[0].CU_BillNumInfo.ReadOnly);
			AssertEquals("Container is not read only", false, declaration.CusContainers[0].CO_ContainerNumberInfo.ReadOnly);

			var factory2 = new BusinessObjectFactory();
			var declarationLoaded = factory2.Load<BaseJobDeclaration>(declaration.PK);
			declarationLoaded.ShipmentSynchroniser.SetEnabled(true, false);
			declarationLoaded.ShipmentSynchroniser.Synchronise();

			var packLine2 = declarationLoaded.Shipment.OuterPackLines.AddNew();
			packLine2.JL_JC = forwardingContainer2.PK;
			packLine2.JL_F3_NKPackType = Core.Constants.PkgUnit.Basket;

			AssertEquals("2 container", 2, declarationLoaded.CusContainers.Count);
			AssertNotNull("All linked to container", declarationLoaded.PackingGroups[0].Container);
			AssertNotNull("All linked to container", declarationLoaded.PackingGroups[1].Container);
		}
	}
}
