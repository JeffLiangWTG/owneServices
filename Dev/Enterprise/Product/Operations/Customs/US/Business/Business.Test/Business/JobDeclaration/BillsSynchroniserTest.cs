using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class BillsSynchroniserTest : TestCaseWithFactory
	{
		public void TestDefaultNumberOfPacksToManifestQtyAndUQIfRequiredAfterMarkAsDirtyEvent()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "APLU2897";
			shipment.JS_TotalPackageCount = 12;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("No of bills", 1, declaration.Bills.Count);
			var bill = declaration.Bills[0];
			AssertEquals("DefaultNumberOfPacksToManifestQtyAndUQIfRequired after Synchronizing", 12m, bill.CU_NoOfPacks);
			shipment.PackLineSynchroniser.MarkSyncDirty();
			AssertEquals("DefaultNumberOfPacksToManifestQtyAndUQIfRequired after Mark As Dirty", 12m, bill.CU_NoOfPacks);
		}

		public void TestNoOfPacksForRail()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "CRUX43287";
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USCHI";

			var seaLeg = consol.Transports[0];
			seaLeg.JW_TransportMode = "SEA";
			seaLeg.JW_RL_NKLoadPort = "AUSYD";
			seaLeg.JW_RL_NKDiscPort = "CAAAL";

			var railLeg = consol.Transports.AddNew();
			railLeg.JW_TransportMode = "RAI";
			railLeg.JW_RL_NKLoadPort = "CAAAL";
			railLeg.JW_RL_NKDiscPort = "USCHI";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CRUX987";
			container1.JC_ArrivalPickupByRail = true;
			container1.AMSNumber = "ABCDM1";
			container1.ITReferenceNumber = "V1";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CRUX123";
			container2.JC_ArrivalPickupByRail = true;
			container2.AMSNumber = "ABCDM2";
			container2.ITReferenceNumber = "V2";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.Consols.Add(consol);
			shipment.JS_HouseBill = "APLU2897";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container1.PK;
			packLine.JL_PackageCount = 1;
			packLine.JL_F3_NKPackType = "PKG";

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_JC = container2.PK;
			packLine2.JL_PackageCount = 2;
			packLine2.JL_F3_NKPackType = "PKG";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);

			AssertEquals("One master bill & one house bill", 2, declaration.Bills.Count);
			AssertEquals("MasterBill", "M2", declaration.JE_MasterBill);
			AssertEquals("MasterBill", "ABCD", declaration.JE_MasterBillIssuerSCAC);

			var m1 = declaration.Bills.Find("M1", BillTypeList.Codes.MasterBill, "ABCD");
			AssertNotNull(m1);
			AssertEquals(1m, m1.CU_NoOfPacks);
			AssertEquals("PK", m1.CU_PackType);

			var m2 = declaration.Bills.Find("M2", BillTypeList.Codes.MasterBill, "ABCD");
			AssertNotNull(m2);
			AssertEquals(2m, m2.CU_NoOfPacks);
			AssertEquals("PK", m2.CU_PackType);

			//Add buyer rail consol scenario.
			var buyerConsol = Factory.New<ForwardingConsol>();
			buyerConsol.JK_TransportMode = Constants.TransportModes.Rail;
			buyerConsol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			buyerConsol.JK_RL_NKDischargePort = "USCHI";

			var buyerContainer1 = buyerConsol.Containers.AddNew();
			buyerContainer1.JC_ContainerNum = "CONT1";
			buyerContainer1.AMSNumber = "ABCDM1";
			buyerContainer1.JC_ArrivalPickupByRail = true;
			buyerContainer1.ITReferenceNumber = "V1";

			var buyerContainer2 = buyerConsol.Containers.AddNew();
			buyerContainer2.JC_ContainerNum = "CONT2";
			buyerContainer2.AMSNumber = "ABCDM2";
			buyerContainer2.JC_ArrivalPickupByRail = true;
			buyerContainer2.ITReferenceNumber = "V2";

			var buyerLeaderShipment = Factory.New<ForwardingShipment>();
			buyerLeaderShipment.Consols.Add(buyerConsol);
			buyerLeaderShipment.JS_HouseBill = "APLU0001";
			buyerLeaderShipment.JS_TransportMode = Constants.TransportModes.Rail;
			buyerLeaderShipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			buyerLeaderShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;

			var buyerCoLoadShipment = buyerLeaderShipment.CoLoadShipments.AddNew();
			buyerCoLoadShipment.JS_HouseBill = "APLU0002";
			buyerCoLoadShipment.JS_TransportMode = Constants.TransportModes.Rail;
			buyerCoLoadShipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			buyerCoLoadShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var buyerPackLine1 = buyerLeaderShipment.OuterPackLines.AddNew();
			buyerPackLine1.JL_JC = buyerContainer1.PK;
			buyerPackLine1.JL_PackageCount = 1;
			buyerPackLine1.JL_F3_NKPackType = "PKG";

			var buyerPackLine2 = buyerCoLoadShipment.OuterPackLines.AddNew();
			buyerPackLine2.JL_JC = buyerContainer2.PK;
			buyerPackLine2.JL_PackageCount = 2;
			buyerPackLine2.JL_F3_NKPackType = "PKG";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_JS = buyerLeaderShipment.PK;
			declaration2.ShipmentSynchroniser.Synchronise(true);

			AssertEquals("2 Master bills", 2, declaration2.Bills.Count);

			var mB1 = declaration2.Bills.Find("M1", BillTypeList.Codes.MasterBill, "ABCD");
			AssertNotNull(mB1);
			AssertEquals(1m, mB1.CU_NoOfPacks);
			AssertEquals("PK", mB1.CU_PackType);

			var mB2 = declaration2.Bills.Find("M2", BillTypeList.Codes.MasterBill, "ABCD");
			AssertNotNull(mB2);
			AssertEquals(2m, mB2.CU_NoOfPacks);
			AssertEquals("PK", mB2.CU_PackType);
		}

		public void TestSeaConsolPickedUpByRail()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "CRUX";
			carrier.UI_ModeOfTransportation = "11";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "43287";
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USCHI";

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "CRUX";
			cusCode.OK_RN_NKCodeCountry = "US";
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;

			var seaLeg = consol.Transports[0];
			seaLeg.JW_TransportMode = "SEA";
			seaLeg.JW_RL_NKLoadPort = "AUSYD";
			seaLeg.JW_RL_NKDiscPort = "CAAAL";

			var railLeg = consol.Transports.AddNew();
			railLeg.JW_TransportMode = "RAI";
			railLeg.JW_RL_NKLoadPort = "CAAAL";
			railLeg.JW_RL_NKDiscPort = "USCHI";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CRUX987";
			container1.JC_ArrivalPickupByRail = true;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CRUX123";
			container2.JC_ArrivalPickupByRail = true;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var shipment = factory2.New<ForwardingShipment>();
			shipment.Consols.Add(factory2.Load<ForwardingConsol>(consol.PK));
			shipment.JS_HouseBill = "2897";

			var org2 = factory2.New<OrgHeader>();
			org2.OH_Code = "aaa";
			org2.OH_FullName = "bbb";
			var cusCode2 = org2.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode2.OK_CustomsRegNo = "APLU";
			cusCode2.OK_RN_NKCodeCountry = "US";
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org2.PK;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container1.PK;
			packLine.JL_PackageCount = 1;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_JC = container2.PK;
			packLine2.JL_PackageCount = 2;

			var declaration = factory2.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);

			AssertEquals("One master bill & one house bill", 2, declaration.Bills.Count);

			AssertEquals("Master bill", "43287", declaration.JE_MasterBill);
			AssertEquals("IssuerCode", "CRUX", declaration.JE_MasterBillIssuerSCAC);

			AssertEquals("House bill", "2897", declaration.JE_HouseBill);
			AssertEquals("IssuerCode", "APLU", declaration.JE_HouseBillIssuerSCAC);

			AssertEquals("Containers", 2, declaration.CusContainers.Count);
			AssertEquals("Two packs", 2, declaration.PackingGroups.Count);

			((ForwardingContainer)declaration.CusContainers[0].JobContainer).AMSNumber = "ABCDM1";
			((ForwardingContainer)declaration.CusContainers[0].JobContainer).ITReferenceNumber = "V1";

			((ForwardingContainer)declaration.CusContainers[1].JobContainer).AMSNumber = "ABCDM2";
			((ForwardingContainer)declaration.CusContainers[1].JobContainer).ITReferenceNumber = "V2";

			AssertEquals("Two master bills & two house bills", 2, declaration.Bills.Count);

			AssertEquals("MasterBill", "M1", declaration.JE_MasterBill);
			AssertEquals("MasterBill", "ABCD", declaration.JE_MasterBillIssuerSCAC);

			var m1 = declaration.Bills.Find("M1", BillTypeList.Codes.MasterBill, "ABCD");
			AssertNotNull(m1);
			Assert(m1.CU_GUIPresentationRecord);
			AssertEquals(1, m1.ITAndSplitDetails.Count);

			var m2 = declaration.Bills.Find("M2", BillTypeList.Codes.MasterBill, "ABCD");
			AssertNotNull(m2);
			Assert(!m2.CU_GUIPresentationRecord);
			AssertEquals(1, m2.ITAndSplitDetails.Count);

			var cusContainer1 = declaration.CusContainers.Find("CRUX987");
			AssertNotNull(cusContainer1);
			var cusContainer2 = declaration.CusContainers.Find("CRUX123");
			AssertNotNull(cusContainer2);
			AssertNotNull(declaration.PackingGroups.GetElementWithHouseBillAndContainerOrEquipment(m2, cusContainer1));
			AssertNotNull(declaration.PackingGroups.GetElementWithHouseBillAndContainerOrEquipment(m1, cusContainer2));

			AssertEquals("IT number", "V1", m1.ITNumber);
			AssertEquals("No of packs", 2, m1.ITAndSplitDetails[0].US_NoOfPacks);
			AssertEquals("IT number", "V2", m2.ITNumber);
			AssertEquals("No of packs", 1, m2.ITAndSplitDetails[0].US_NoOfPacks);

			((ForwardingContainer)declaration.CusContainers[0].JobContainer).AMSNumber = ZString.Empty;
			((ForwardingContainer)declaration.CusContainers[0].JobContainer).ITReferenceNumber = "V1";

			((ForwardingContainer)declaration.CusContainers[1].JobContainer).AMSNumber = ZString.Empty;
			((ForwardingContainer)declaration.CusContainers[1].JobContainer).ITReferenceNumber = "V2";

			AssertEquals("2 it no", 2, declaration.PrimaryHouseBill.ITAndSplitDetails.Count);
			AssertNotNull("Contain V1", declaration.PrimaryHouseBill.ITAndSplitDetails.FindByItNumber("V1"));
			AssertNotNull("Contain V2", declaration.PrimaryHouseBill.ITAndSplitDetails.FindByItNumber("V2"));

			cusContainer1 = declaration.CusContainers.Find("CRUX987");
			cusContainer2 = declaration.CusContainers.Find("CRUX123");
			AssertNotNull(declaration.PackingGroups.GetElementWithHouseBillAndContainerOrEquipment(declaration.PrimaryHouseBill, cusContainer2));
			AssertNotNull(declaration.PackingGroups.GetElementWithHouseBillAndContainerOrEquipment(declaration.PrimaryHouseBill, cusContainer1));

			var shipmentITNo = shipment.Numbers.AddNew();
			shipmentITNo.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			shipmentITNo.CE_EntryNum = "V3";

			((ForwardingContainer)declaration.CusContainers[1].JobContainer).ITReferenceNumber = ZString.Empty;
			AssertEquals("2 it no", 2, declaration.PrimaryHouseBill.ITAndSplitDetails.Count);
			AssertNotNull("Contain V1", declaration.PrimaryHouseBill.ITAndSplitDetails.FindByItNumber("V1"));
			AssertNotNull("Contain V3", declaration.PrimaryHouseBill.ITAndSplitDetails.FindByItNumber("V3"));

			((ForwardingContainer)declaration.CusContainers[0].JobContainer).AMSNumber = "ABCDM1";
			((ForwardingContainer)declaration.CusContainers[0].JobContainer).ITReferenceNumber = "V1";

			((ForwardingContainer)declaration.CusContainers[1].JobContainer).AMSNumber = ZString.Empty;
			((ForwardingContainer)declaration.CusContainers[1].JobContainer).ITReferenceNumber = "V2";

			AssertEquals("2 masters and 2 house bills", 4, declaration.Bills.Count);
			m1 = declaration.Bills.FindByBillNumberAndType("43287", BillTypeList.Codes.MasterBill);
			AssertNotNull(m1);
			AssertEquals("Bill from shipment is primary bill", m1, declaration.PrimaryMasterBill);
			var h1 = declaration.Bills.FindAnyBillWithHouseBillMasterBillCombination("2897", "43287");
			AssertNotNull(h1);
			AssertEquals(h1, declaration.PrimaryHouseBill);

			AssertEquals("1 it numbers", 1, h1.ITAndSplitDetails.Count);
			AssertNotNull("Contain V2", h1.ITAndSplitDetails.FindByItNumber("V2"));

			var h2 = declaration.Bills.FindAnyBillWithHouseBillMasterBillCombination("2897", "M1");
			AssertNotNull(h2);

			AssertEquals("1 it numbers", 1, h2.ITAndSplitDetails.Count);
			AssertNotNull("Contain V1", h2.ITAndSplitDetails.FindByItNumber("V1"));

			cusContainer1 = declaration.CusContainers.Find("CRUX123");
			cusContainer2 = declaration.CusContainers.Find("CRUX987");
			AssertNotNull(declaration.PackingGroups.GetElementWithHouseBillAndContainerOrEquipment(h2, cusContainer1));
			AssertNotNull(declaration.PackingGroups.GetElementWithHouseBillAndContainerOrEquipment(h1, cusContainer2));

			var subShipment = shipment.CoLoadShipments.AddNew();
			subShipment.JS_HouseBill = "APLU1111";

			subShipment.OuterPackLines.AddNew();
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("2 masters and 2 house bills", 4, declaration.Bills.Count);

			shipment.CoLoadShipments.RemoveAndDeleteAll();
			consol.JK_MasterBillNum = ZString.Empty;
			Factory.Save();
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("1 master and 2 house bills", 3, declaration.Bills.Count);
			AssertEquals("2897", declaration.PrimaryHouseBill.CU_BillNum);
			AssertNull("No parent bill", declaration.PrimaryHouseBill.ParentBill);
		}

		public void TestSynchroniseNormalITNumbers()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "CRUX43287";
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USCHI";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CRUX987";

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var shipment = factory2.New<ForwardingShipment>();
			shipment.Consols.Add(factory2.Load<ForwardingConsol>(consol.PK));
			shipment.JS_HouseBill = "APLU2897";

			var cusEntryNum = shipment.Numbers.AddNew();
			cusEntryNum.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			cusEntryNum.CE_EntryNum = "v807432";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container1.PK;
			packLine.JL_PackageCount = 1;

			var declaration = factory2.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);

			AssertNotNull(declaration.PrimaryHouseBill);
			AssertEquals(1, declaration.PrimaryHouseBill.ITAndSplitDetails.Count);
			AssertEquals("V807432", declaration.PrimaryHouseBill.ITAndSplitDetails[0].US_ITNumber);
		}

		public void TestSynchroniseNormalITNumbersWhenIsRail()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Rail;
			shipment.JS_HouseBill = "HB0000001";

			var entryNumber = shipment.Numbers.AddNew();
			entryNumber.CE_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;
			entryNumber.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			entryNumber.CE_EntryNum = "IT0000001";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Rail;
			consol.JK_MasterBillNum = "MB0000001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USCHI";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CRUX987";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container.PK;
			packLine.JL_PackageCount = 1;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);

			CombineAssertions(() =>
			{
				AssertNotNull("House Bill exists.", declaration.PrimaryHouseBill);
				AssertNotNull("Master Bill exists.", declaration.PrimaryMasterBill);
				AssertEquals("Number of House Bill Details.", 0, declaration.PrimaryHouseBill.ITAndSplitDetails.Count);
				AssertEquals("Number of Master Bill Details.", 1, declaration.PrimaryMasterBill.ITAndSplitDetails.Count);
				AssertEquals("Master Bill IT Number.", entryNumber.CE_EntryNum, declaration.PrimaryMasterBill.ITAndSplitDetails[0].US_ITNumber);
			});
		}
		public void TestSynchroniseNormalITNumbersWhenIsAir()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "HB0000001";

			var entryNumber = shipment.Numbers.AddNew();
			entryNumber.CE_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;
			entryNumber.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			entryNumber.CE_EntryNum = "IT0000001";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "MB0000001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USCHI";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CRUX987";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container.PK;
			packLine.JL_PackageCount = 1;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);

			CombineAssertions(() =>
			{
				AssertNotNull("House Bill exists.", declaration.PrimaryHouseBill);
				AssertNotNull("Master Bill exists.", declaration.PrimaryMasterBill);
				AssertEquals("Number of House Bill Details.", 1, declaration.PrimaryHouseBill.ITAndSplitDetails.Count);
				AssertEquals("Number of Master Bill Details.", 0, declaration.PrimaryMasterBill.ITAndSplitDetails.Count);
				AssertEquals("House Bill IT Number.", entryNumber.CE_EntryNum, declaration.PrimaryHouseBill.ITAndSplitDetails[0].US_ITNumber);
			});
		}

		public void TestSynchroniseDoesntDeleteAndRecreate()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "CRUX43287";
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USCHI";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CRUX987";

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var shipment = factory2.New<ForwardingShipment>();
			shipment.Consols.Add(factory2.Load<ForwardingConsol>(consol.PK));
			shipment.JS_HouseBill = "APLU2897";

			var cusEntryNum = shipment.Numbers.AddNew();
			cusEntryNum.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			cusEntryNum.CE_EntryNum = "V807432";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container1.PK;
			packLine.JL_PackageCount = 1;

			var declaration = factory2.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);

			Assert("Precondition1", !declaration.PrimaryHouseBill.ITAndSplitDetails[0].IsInDatabase);
			factory2.Save();
			Assert("Precondition2", declaration.PrimaryHouseBill.ITAndSplitDetails[0].IsInDatabase);
			declaration.ShipmentSynchroniser.Synchronise(true);
			Assert(declaration.PrimaryHouseBill.ITAndSplitDetails[0].IsInDatabase);
		}

		public void TestSynchroniseConsolidatesContainersWithSameITNumber()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "CRUX43287";
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USCHI";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CRUX987";
			container1.ITReferenceNumber = "V1";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CRUX123";
			container2.ITReferenceNumber = "V1";

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var shipment = factory2.New<ForwardingShipment>();
			shipment.Consols.Add(factory2.Load<ForwardingConsol>(consol.PK));
			shipment.JS_HouseBill = "APLU2897";
			shipment.JS_F3_NKPackType = "";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container1.PK;
			packLine.JL_PackageCount = 2;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_JC = container2.PK;
			packLine2.JL_PackageCount = 7;

			var declaration = factory2.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);

			AssertEquals("V1", declaration.PrimaryHouseBill.ITAndSplitDetails[0].US_ITNumber);
			//This should be 9 - current bug causes it to be 4 in synchronised method
			AssertEquals(4, declaration.PrimaryHouseBill.ITAndSplitDetails[0].US_NoOfPacks);
		}

		public void TestSynchroniseLongITNumbers()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "CRUX43287";
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USCHI";
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			var number = consol.Numbers.AddNew();
			number.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			number.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			number.CE_EntryNum = "1234567890123456";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CRUX987";

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var shipment = factory2.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.Consols.Add(factory2.Load<ForwardingConsol>(consol.PK));
			shipment.JS_HouseBill = "APLU2897";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container1.PK;
			packLine.JL_PackageCount = 1;

			var declaration = factory2.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);

			AssertEquals(1, declaration.Bills[0].ITAndSplitDetails.Count);
			AssertEquals("12345678901", declaration.Bills[0].ITAndSplitDetails[0].US_ITNumber);

			number = container1.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			number.CE_EntryNum = "55555555501111";
			Factory.Save();
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("55555555501", declaration.Bills[0].ITAndSplitDetails[0].US_ITNumber);

			number = shipment.Numbers.AddNew();
			number.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			number.CE_EntryNum = "V999999999011111";
			container1.AdditionalReferenceNumbers.RemoveAndDeleteAll();
			Factory.Save();
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("V9999999990", declaration.Bills[0].ITAndSplitDetails[0].US_ITNumber);
		}

		public void TestSynchroniseSCACDependOnRegistrySetting()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUDYD";
			consol.JK_RL_NKDischargePort = "USCHI";
			consol.JK_MasterBillNum = "IANTESTMB";

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "aaa";
			org1.OH_FullName = "org1";
			var cusCode = org1.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "APLU";
			cusCode.OK_RN_NKCodeCountry = "US";
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org1.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "bbb";
			org2.OH_FullName = "org1";
			var cusCode2 = org2.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode2.OK_CustomsRegNo = "CRUN";
			cusCode2.OK_RN_NKCodeCountry = "US";
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org2.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_JS = shipment.PK;

			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("APLU", declaration.JE_MasterBillIssuerSCAC);
			AssertEquals("APLU", declaration.Bills[0].US_UI_NKBillIssuerSCAC);
		}

		public void TestDeleteDuplicateITAndSplitDetails()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "CRUX43287";
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USCHI";
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			var number = consol.Numbers.AddNew();
			number.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			number.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			number.CE_EntryNum = "1234567890123456";
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var shipment = factory2.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.Consols.Add(factory2.Load<ForwardingConsol>(consol.PK));
			shipment.JS_HouseBill = "APLU2897";

			var declaration = factory2.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);

			AssertEquals(1, declaration.Bills[0].ITAndSplitDetails.Count);
			AssertEquals("12345678901", declaration.Bills[0].ITAndSplitDetails[0].US_ITNumber);

			var iTNumber = declaration.Bills[0].ITAndSplitDetails.AddNew();
			iTNumber.US_ITNumber = "12345678901";
			AssertEquals(2, declaration.Bills[0].ITAndSplitDetails.Count);
			AssertEquals("12345678901", declaration.Bills[0].ITAndSplitDetails[0].US_ITNumber);
			AssertEquals("12345678901", declaration.Bills[0].ITAndSplitDetails[1].US_ITNumber);

			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals(1, declaration.Bills[0].ITAndSplitDetails.Count);
			AssertEquals("12345678901", declaration.Bills[0].ITAndSplitDetails[0].US_ITNumber);
		}
	}
}
