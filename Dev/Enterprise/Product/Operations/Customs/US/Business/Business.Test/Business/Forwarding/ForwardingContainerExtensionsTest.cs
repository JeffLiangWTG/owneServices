using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ForwardingContainerExtensionsTest : TestCaseWithFactory
	{
		public void TestGetEffectiveITNumber()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolITNo = consol.Numbers.AddNew();
			consolITNo.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			consolITNo.CE_EntryNum = "V1";
			var shipment = Factory.New<ForwardingShipment>();
			var shipmentITNo = shipment.Numbers.AddNew();
			shipmentITNo.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			shipmentITNo.CE_EntryNum = "V2";
			var container = consol.Containers.AddNew();
			container.ITReferenceNumber = "V3";
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container.PK;
			packLine.JL_PackageCount = 1;
			shipment.Consols.Add(consol);
			AssertEquals("Taken from container", "V3", container.GetEffectiveITNumber(shipment, consol));
			container.ITReferenceNumber = ZString.Empty;
			AssertEquals("Taken from (effective) shipment", "V2", container.GetEffectiveITNumber(shipment, consol));
		}

		public void TestSynchroniseManifestQtyFromBCNShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USCHI";
			consol.JK_MasterBillNum = "IANTESTMB";
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "MAEU5287800";
			container1.ITReferenceNumber = "V3832468001";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "MAEU5287811";
			container2.ITReferenceNumber = "V3832468002";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			shipment.JS_HouseBill = "HBOL201702280";
			var package1InBCN = shipment.OuterPackLines.AddNew();
			package1InBCN.JL_PackageCount = 100;
			package1InBCN.JL_JC = container1.PK;
			var package2InBCN = shipment.OuterPackLines.AddNew();
			package2InBCN.JL_PackageCount = 200;
			package2InBCN.JL_JC = container2.PK;
			shipment.JS_OuterPacks = 300;
			var subShipment = shipment.CoLoadShipments.AddNew();
			subShipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			subShipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			subShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			subShipment.JS_HouseBill = "HBOL201702281";
			var package1InSub = subShipment.OuterPackLines.AddNew();
			package1InSub.JL_PackageCount = 1000;
			package1InSub.JL_JC = container1.PK;
			var package2InSub = subShipment.OuterPackLines.AddNew();
			package2InSub.JL_PackageCount = 2000;
			package2InSub.JL_JC = container2.PK;
			subShipment.JS_OuterPacks = 2100;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("3 bills should be generated.", 3, declaration.Bills.Count);
			var matchedBillForMasterShipment = declaration.Bills.OfType<Bill>().FirstOrDefault(x => x.CU_BillNum.Contains("201702280"));
			AssertNotNull("Found matched bill which has same bill number with master shipment", matchedBillForMasterShipment);
			AssertEquals("Bill quantity equals to 300", 300m, matchedBillForMasterShipment.CU_NoOfPacks);
			AssertEquals("There are two IT Numbers generated", 2, matchedBillForMasterShipment.ITAndSplitDetails.Count);
			AssertEquals("Quantity in IT Number equals to 100", 100, matchedBillForMasterShipment.ITAndSplitDetails.FindByItNumber("V3832468001").FirstOrDefault().US_NoOfPacks);
			AssertEquals("Quantity in IT Number equals to 200", 200, matchedBillForMasterShipment.ITAndSplitDetails.FindByItNumber("V3832468002").FirstOrDefault().US_NoOfPacks);
			var matchedBillForSubShipment = declaration.Bills.OfType<Bill>().FirstOrDefault(x => x.CU_BillNum.Contains("201702281"));
			AssertNotNull("Found matched bill which has same bill number with sub shipment", matchedBillForSubShipment);
			AssertEquals("Bill quantity equals to 2100", 2100m, matchedBillForSubShipment.CU_NoOfPacks);
			AssertEquals("There are two IT Numbers generated", 2, matchedBillForSubShipment.ITAndSplitDetails.Count);
			AssertEquals("Quantity in IT Number equals to 1000", 1000, matchedBillForSubShipment.ITAndSplitDetails.FindByItNumber("V3832468001").FirstOrDefault().US_NoOfPacks);
			AssertEquals("Quantity in IT Number equals to 2000", 2000, matchedBillForSubShipment.ITAndSplitDetails.FindByItNumber("V3832468002").FirstOrDefault().US_NoOfPacks);
			container1.AMSNumber = "CNRUMSTNEW001";
			container2.AMSNumber = "CNRUMSTNEW002";
			declaration.Bills.RemoveAndDeleteAll();
			declaration.Packages.RemoveAndDeleteAll();
			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("2 bills should be generated.", 2, declaration.Bills.Count);
			var matchedMasterBill1 = declaration.Bills.FindByBillNumberAndType("MSTNEW001", "MB");
			AssertNotNull("Found matched master bill with AMS number", matchedMasterBill1);
			AssertEquals("Bill quantity equals to 1100", 1100m, matchedMasterBill1.CU_NoOfPacks);
			AssertEquals("There is only one IT Numbers generated", 1, matchedMasterBill1.ITAndSplitDetails.Count);
			AssertEquals("Quantity in IT Number equals to 1100", 1100, matchedMasterBill1.ITAndSplitDetails.FindByItNumber("V3832468001").FirstOrDefault().US_NoOfPacks);
			var matchedMasterBill2 = declaration.Bills.FindByBillNumberAndType("MSTNEW002", "MB");
			AssertNotNull("Found matched master bill with AMS number", matchedMasterBill2);
			AssertEquals("Bill quantity equals to 2200", 2200m, matchedMasterBill2.CU_NoOfPacks);
			AssertEquals("There are two IT Numbers generated", 1, matchedMasterBill2.ITAndSplitDetails.Count);
			AssertEquals("Quantity in IT Number equals to 2200", 2200, matchedMasterBill2.ITAndSplitDetails.FindByItNumber("V3832468002").FirstOrDefault().US_NoOfPacks);
		}
	}
}
