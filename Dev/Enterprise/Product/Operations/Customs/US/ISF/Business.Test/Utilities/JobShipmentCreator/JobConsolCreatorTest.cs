using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using ForwardingConsol = Enterprise.Freight.Forwarding.Business.ForwardingConsol;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class JobConsolCreatorTest : TestCaseWithFactory
	{
		public void TestSeaConsolIsCreatedByDefault()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_TransportMode = TransportModeCodes.Codes.OceanVesselNonContainerized;
			header.BF_OwnerReference = "OWNREF123";
			CusISFBill masterBill1 = AddBill(header, "MB1", BillTypeList.Codes.MasterBillOfLading);
			CusISFBill houseBill1 = AddBill(header, "HB1", BillTypeList.Codes.HouseBillOfLading);
			header.Transports.RemoveAndDeleteAll();
			Factory.Save();
			ISFHeaderRow row = new ISFHeaderRow(header);
			row.MasterBillPK = masterBill1.PK;
			JobConsolCreator creator = new JobConsolCreator(row);
			ForwardingConsol consol = creator.GetConsol(new BusinessObjectFactory());
			AssertEquals(Core.Constants.TransportModes.Sea, consol.JK_TransportMode);
		}

		public void TestCorrectContainerModeIsCreated()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_TransportMode = TransportModeCodes.Codes.OceanVesselNonContainerized;
			var masterBill1 = AddBill(header, "MB1", BillTypeList.Codes.MasterBillOfLading);
			var houseBill1 = AddBill(header, "HB1", BillTypeList.Codes.HouseBillOfLading);
			header.Transports.RemoveAndDeleteAll();
			Factory.Save();
			var row = new ISFHeaderRow(header);
			row.MasterBillPK = masterBill1.PK;
			var creator = new JobConsolCreator(row);
			var consol = creator.GetConsol(new BusinessObjectFactory());
			AssertEquals(Core.Constants.ContainerModes.BreakBulk, consol.JK_ConsolMode);
			header.BF_TransportMode = TransportModeCodes.Codes.OceanVesselContainerized;
			Factory.Save();
			consol = creator.GetConsol(new BusinessObjectFactory());
			AssertEquals(Core.Constants.ContainerModes.FCL, consol.JK_ConsolMode);
		}

		public void TestGetConsol()
		{
			#region Setup data
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_TransportMode = TransportModeCodes.Codes.OceanVesselNonContainerized;
			header.BF_OwnerReference = "OWNREF123";
			CusISFBill masterBill1 = AddBill(header, "MB1", BillTypeList.Codes.MasterBillOfLading);
			CusISFBill houseBill1 = AddBill(header, "HB1", BillTypeList.Codes.HouseBillOfLading);
			CusISFBill masterBill2 = AddBill(header, "MB2", BillTypeList.Codes.MasterBillOfLading);
			CusISFBill houseBill2 = AddBill(header, "HB2", BillTypeList.Codes.HouseBillOfLading);
			RefContainer containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "!Z1";
			containerType.RC_ISOType = "21ZZ";
			var containerMap = containerType.CodeMapCollection.AddNew();
			containerMap.RCM_RN_NKCountry = "US";
			containerMap.RCM_Code = "2Z";
			CusISFEquip container1 = header.Equipments.AddNew();
			container1.BE_ContainerNum = "TURE1";
			container1.BE_EquipCode = "2Z";
			container1.BE_ContainerISO = "21ZZ";
			CusISFEquip container2 = header.Equipments.AddNew();
			container2.BE_ContainerNum = "TURE2";
			container2.BE_EquipCode = "2Z";
			container2.BE_ContainerISO = "";
			CusISFEquip container3 = header.Equipments.AddNew();
			container3.BE_ContainerNum = "TURE3";
			container3.BE_EquipCode = "";
			container3.BE_ContainerISO = "21ZZ";
			CusISFLine line1 = header.Lines.AddNew();
			line1.BL_HarmonisedNum = "10.10.8120";
			line1.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			CusISFLine line2 = header.Lines.AddNew();
			line2.BL_HarmonisedNum = "20.20.8220";
			line2.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			OrgHeader carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "CARRIER COMPANY";
			carrier.OH_RL_NKClosestPort = "USCHI";
			carrier.MainAddress.OA_Address1 = "CARRIER ADDRESS 1";
			carrier.MainAddress.OA_Address2 = "CARRIER ADDRESS 2";
			carrier.MainAddress.OA_City = "CHICAGO";
			carrier.MainAddress.OA_State = "IL";
			Transport transport1 = header.Transports.AddNew();
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;
			transport1.JW_TransportMode = Core.Constants.TransportModes.Road;
			transport1.JW_VoyageFlight = "111";
			transport1.JW_RL_NKLoadPort = "AUMEL";
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_ETD = new ZDateTime(2009, 3, 1);
			transport1.JW_ETA = new ZDateTime(2009, 3, 2);
			Transport transport2 = header.Transports.AddNew();
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_Vessel = "APL VESSEL 2";
			transport2.JW_VoyageFlight = "222";
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_ETD = new ZDateTime(2009, 3, 3);
			transport2.JW_ETA = new ZDateTime(2009, 4, 1);
			transport2.CarrierPK = carrier.PK;
			Transport transport3 = header.Transports.AddNew();
			transport3.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;
			transport3.JW_TransportMode = Core.Constants.TransportModes.Rail;
			transport3.JW_VoyageFlight = "333";
			transport3.JW_RL_NKLoadPort = "USLAX";
			transport3.JW_RL_NKDiscPort = "CATOR";
			transport3.JW_ETD = new ZDateTime(2009, 4, 2);
			transport3.JW_ETA = new ZDateTime(2009, 4, 5);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Other;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKLastForeignPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "USCHI";
			consol.JK_MasterBillNum = "MB3";
			ForwardingContainer consolContainer1 = consol.Containers.AddNew();
			consolContainer1.JC_ContainerNum = "TURE1";
			consolContainer1.JC_RC = ZGuid.Empty;
			ForwardingContainer consolContainer2 = consol.Containers.AddNew();
			consolContainer2.JC_ContainerNum = "TURE3";
			consolContainer2.JC_RC = containerType.PK;
			Factory.Save();
			#endregion
			ISFHeaderRow row = new ISFHeaderRow(header);
			row.MasterBillPK = masterBill1.PK;
			AssertEquals(3, row.Containers.Count);
			var containerRow1 = row.Containers[0];
			AssertEquals("TURE1", containerRow1.ContainerNumber);
			var containerRow2 = row.Containers[1];
			AssertEquals("TURE2", containerRow2.ContainerNumber);
			var containerRow3 = row.Containers[2];
			AssertEquals("TURE3", containerRow3.ContainerNumber);
			AssertEquals(2, row.Bills.Count);
			ISFBillRow billRow1 = row.Bills[0];
			AssertEquals(2, billRow1.Lines.Count);
			billRow1.Lines[0].ContainerPK = containerRow1.PK;
			billRow1.Lines[1].ContainerPK = containerRow2.PK;
			ISFBillRow billRow2 = row.Bills[1];
			AssertEquals(2, billRow2.Lines.Count);
			billRow2.Lines[0].ContainerPK = containerRow3.PK;
			billRow2.Lines[1].ContainerPK = ZGuid.Empty;
			JobConsolCreator creator = new JobConsolCreator(row);
			ForwardingConsol consol1 = creator.GetConsol(new BusinessObjectFactory());
			AssertNotEquals(Factory, consol1.Factory);
			AssertConsolData(consol1, masterBill1.BB_BillNum, Core.Constants.TransportModes.Sea, "AUSYD", "USLAX", "AUSYD");
			AssertEquals(3, consol1.Transports.Count);
			AssertTransport(consol1.Transports.FindByLoadPort("AUMEL"), transport1.JW_TransportType, transport1.JW_TransportMode, transport1.JW_Vessel, transport1.JW_VoyageFlight, transport1.JW_RL_NKLoadPort, transport1.JW_RL_NKDiscPort, transport1.JW_ETD, transport1.JW_ETA, transport1.JW_OA_CarrierAddress, consol1.PK);
			AssertTransport(consol1.Transports.FindByLoadPort("AUSYD"), transport2.JW_TransportType, transport2.JW_TransportMode, transport2.JW_Vessel, transport2.JW_VoyageFlight, transport2.JW_RL_NKLoadPort, transport2.JW_RL_NKDiscPort, transport2.JW_ETD, transport2.JW_ETA, transport2.JW_OA_CarrierAddress, consol1.PK);
			AssertTransport(consol1.Transports.FindByLoadPort("USLAX"), transport3.JW_TransportType, transport3.JW_TransportMode, transport3.JW_Vessel, transport3.JW_VoyageFlight, transport3.JW_RL_NKLoadPort, transport3.JW_RL_NKDiscPort, transport3.JW_ETD, transport3.JW_ETA, transport3.JW_OA_CarrierAddress, consol1.PK);
			// Test all new objects have been created with the same factory
			consol1.Factory.Save();
			consol1 = new BusinessObjectFactory().Load<ForwardingConsol>(consol1.PK);
			AssertConsolData(consol1, masterBill1.BB_BillNum, Core.Constants.TransportModes.Sea, "AUSYD", "USLAX", "AUSYD");
			AssertEquals(3, consol1.Transports.Count);
			AssertTransport(consol1.Transports.FindByLoadPort("AUMEL"), transport1.JW_TransportType, transport1.JW_TransportMode, transport1.JW_Vessel, transport1.JW_VoyageFlight, transport1.JW_RL_NKLoadPort, transport1.JW_RL_NKDiscPort, transport1.JW_ETD, transport1.JW_ETA, transport1.JW_OA_CarrierAddress, consol1.PK);
			AssertTransport(consol1.Transports.FindByLoadPort("AUSYD"), transport2.JW_TransportType, transport2.JW_TransportMode, transport2.JW_Vessel, transport2.JW_VoyageFlight, transport2.JW_RL_NKLoadPort, transport2.JW_RL_NKDiscPort, transport2.JW_ETD, transport2.JW_ETA, transport2.JW_OA_CarrierAddress, consol1.PK);
			AssertTransport(consol1.Transports.FindByLoadPort("USLAX"), transport3.JW_TransportType, transport3.JW_TransportMode, transport3.JW_Vessel, transport3.JW_VoyageFlight, transport3.JW_RL_NKLoadPort, transport3.JW_RL_NKDiscPort, transport3.JW_ETD, transport3.JW_ETA, transport3.JW_OA_CarrierAddress, consol1.PK);
			AssertEquals(3, consol1.Containers.Count);
			AsserContainer((ForwardingContainer)consol1.Containers.FindAnyByContainerNumber("TURE1"), "TURE1", containerType.PK);
			AsserContainer((ForwardingContainer)consol1.Containers.FindAnyByContainerNumber("TURE2"), "TURE2", ZGuid.Empty);
			AsserContainer((ForwardingContainer)consol1.Containers.FindAnyByContainerNumber("TURE3"), "TURE3", ZGuid.Empty);
			row.ConsolPK = consol.PK;
			ForwardingConsol consol2 = creator.GetConsol(new BusinessObjectFactory());
			AssertNotEquals(Factory, consol2.Factory);
			AssertNotEquals(consol1.Factory, consol2.Factory);
			AssertEquals(consol.PK, consol2.PK);
			AssertConsolData(consol2, consol.JK_MasterBillNum, consol.JK_TransportMode, consol.JK_RL_NKLoadPort, consol.JK_RL_NKDischargePort, consol.JK_RL_NKLastForeignPort);
			AssertEquals(1, consol2.Transports.Count);
			AssertEquals(3, consol2.Containers.Count);
			AsserContainer((ForwardingContainer)consol2.Containers.FindByPK(consolContainer1.PK), consolContainer1.JC_ContainerNum, containerType.PK);
			AsserContainer((ForwardingContainer)consol2.Containers.FindByPK(consolContainer2.PK), consolContainer2.JC_ContainerNum, containerType.PK);
			var consol2Container3 = consol2.Containers.FindAnyByContainerNumber("TURE2");
			AsserContainer((ForwardingContainer)consol2Container3, "TURE2", ZGuid.Empty);
			consol2Container3.Delete();
			consol2.Factory.Save();
			containerRow2.ShouldCopy = false;
			billRow1.Lines[1].ShouldCopy = false;
			ForwardingConsol consol3 = creator.GetConsol(new BusinessObjectFactory());
			AssertNotEquals(Factory, consol3.Factory);
			AssertNotEquals(consol1.Factory, consol3.Factory);
			AssertEquals(consol.PK, consol3.PK);
			AssertConsolData(consol3, consol.JK_MasterBillNum, consol.JK_TransportMode, consol.JK_RL_NKLoadPort, consol.JK_RL_NKDischargePort, consol.JK_RL_NKLastForeignPort);
			AssertEquals(1, consol3.Transports.Count);
			AssertEquals(2, consol3.Containers.Count);
			AsserContainer((ForwardingContainer)consol3.Containers.FindByPK(consolContainer1.PK), consolContainer1.JC_ContainerNum, containerType.PK);
			AsserContainer((ForwardingContainer)consol3.Containers.FindByPK(consolContainer2.PK), consolContainer2.JC_ContainerNum, containerType.PK);
		}

		void AssertTransport(Transport transport, ZString transportType, ZString transportMode, ZString vessel, ZString voyageFlight, ZString loadPort, ZString discPort, ZDateTime etd, ZDateTime eta, ZGuid carrierAddressPK, ZGuid parentPK)
		{
			AssertEquals(transportType, transport.JW_TransportType);
			AssertEquals(transportMode, transport.JW_TransportMode);
			AssertEquals(vessel, transport.JW_Vessel);
			AssertEquals(voyageFlight, transport.JW_VoyageFlight);
			AssertEquals(loadPort, transport.JW_RL_NKLoadPort);
			AssertEquals(discPort, transport.JW_RL_NKDiscPort);
			AssertEquals(etd, transport.JW_ETD);
			AssertEquals(eta, transport.JW_ETA);
			AssertEquals(carrierAddressPK, transport.JW_OA_CarrierAddress);
			AssertEquals(parentPK, transport.JW_ParentGUID);
			AssertEquals(Core.Constants.TransportParentTypes.Consol, transport.JW_ParentType);
		}

		void AssertConsolData(ForwardingConsol consol, ZString masterBill, ZString transportMode, ZString loadPort, ZString discPort, ZString lastForeignPort)
		{
			AssertEquals(masterBill, consol.JK_MasterBillNum);
			AssertEquals(transportMode, consol.JK_TransportMode);
			AssertEquals(loadPort, consol.JK_RL_NKLoadPort);
			AssertEquals(discPort, consol.JK_RL_NKDischargePort);
			AssertEquals(lastForeignPort, consol.JK_RL_NKLastForeignPort);
		}

		void AsserContainer(ForwardingContainer container, ZString containerNumber, ZGuid containerTypePK)
		{
			AssertEquals(containerNumber, container.JC_ContainerNum);
			AssertEquals(containerTypePK, container.JC_RC);
		}

		CusISFBill AddBill(CusISFHeader header, ZString billNum, ZString billType)
		{
			CusISFBill bill = header.ReferenceDatas.AddNew();
			bill.BB_BillNum = billNum;
			bill.BB_BillType = billType;
			return bill;
		}
	}
}
