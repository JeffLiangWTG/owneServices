using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class CusInBondBillCollectionSynchroniserTest : AMSSynchroniserTestCase
	{
		public void TestSynchronisation()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "SCAC";
			carrier.UI_ModeOfTransportation = "11";
			var carrier2 = Factory.New<USCarrierCombined>();
			carrier2.UI_Code = "OTT1";
			carrier2.UI_ModeOfTransportation = "11";
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var orgProxyCarrierCode = orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals(0, header.Bills.Count);
			AssertEquals("OTT1", orgProxyCarrierCode.OK_CustomsRegNo);
			var shipment1 = consol.Shipments.AddNew();
			AssertEquals(1, header.Bills.Count);
			shipment1.JS_HouseBill = "1234";
			AssertBill(header.Bills[0], "OTT1", "1234");
			synchroniser.SetEnabled(false, false);
			var bill1 = header.Bills[0];
			var bill2 = header.Bills.AddNew();
			bill2.B0_IssuerCode = "OTT1";
			bill2.B0_MasterBillNumber = "MB2";
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "5678";
			synchroniser.SetEnabled(true, false);
			AssertEquals(2, header.Bills.Count);
			AssertBill(header.Bills[0], bill1.PK, "OTT1", "1234");
			AssertBill(header.Bills[1], bill2.PK, "OTT1", "MB2");
			synchroniser.Synchronise(true);
			AssertEquals(2, header.Bills.Count);
			AssertBill(header.Bills[0], bill1.PK, "OTT1", "1234");
			var bill3 = header.Bills[1];
			AssertBill(bill3, "OTT1", "5678");
			AssertEquals(true, bill2.IsDeleted);
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "APLU";
			cusCode.OK_RN_NKCodeCountry = "US";
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_HouseBill = "BN1234";
			shipment3.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
			synchroniser.Synchronise(true);
			AssertEquals(3, header.Bills.Count);
			AssertBill(header.Bills[2], header.Bills[2].PK, "APLU", "BN1234");
			var org0 = Factory.NewWithValidTestData<OrgHeader>();
			var orgCarrierCode = org0.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SCAC", Core.Constants.CountryCodes.UnitedStates);
			consol.JK_OA_SendingForwarderAddress = org0.MainAddress.PK;
			var shipment4 = consol.Shipments.AddNew();
			shipment4.JS_HouseBill = "SCACBN001";
			synchroniser.Synchronise(true);
			AssertEquals(4, header.Bills.Count);
			AssertBill(header.Bills[3], header.Bills[3].PK, "SCAC", "BN001"); // "SCAC" is valid carrier code.
		}

		public void TestSynchronisation_DuplicatedHouseBill()
		{
			AssertEquals(0, header.Bills.Count);
			AssertEquals("OTT1", orgProxyCarrierCode.OK_CustomsRegNo);
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "AB1D1234";
			AssertEquals(1, header.Bills.Count);
			var bill1 = header.Bills[0];
			AssertBill(bill1, "OTT1", "AB1D1234");
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "AB1D1234";
			AssertEquals(2, header.Bills.Count);
			var bill2 = header.Bills[1];
			if (bill2 == bill1)
			{
				bill2 = header.Bills[0];
			}
			else
			{
				AssertEquals(bill1, header.Bills[0]);
			}

			AssertBill(bill1, "OTT1", "AB1D1234");
			AssertBill(bill2, "OTT1", "AB1D1234");
			shipment2.JS_HouseBill = "AB1D5678";
			AssertEquals(2, header.Bills.Count);
			AssertBill(bill1, "OTT1", "AB1D1234");
			AssertBill(bill2, "OTT1", "AB1D5678");
			shipment2.JS_HouseBill = "AB1D1234";
			AssertEquals(2, header.Bills.Count);
			AssertBill(bill1, "OTT1", "AB1D1234");
			AssertBill(bill2, "OTT1", "AB1D1234");
			synchroniser.SetEnabled(false, false);
			shipment1.Delete();
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_HouseBill = "AB1D1234";
			synchroniser.SetEnabled(true, false);
			AssertEquals(2, header.Bills.Count);
			AssertBill(bill1, "OTT1", "AB1D1234");
			AssertBill(bill2, "OTT1", "AB1D1234");
			synchroniser.Synchronise(true);
			AssertEquals("Is deleted as shipment1 is deleted", true, bill1.IsDeleted);
			AssertEquals(2, header.Bills.Count);
			var bill3 = header.Bills[1];
			if (bill3 == bill2)
			{
				bill3 = header.Bills[0];
			}
			else
			{
				AssertEquals(bill2, header.Bills[0]);
			}

			AssertBill(bill2, "OTT1", "AB1D1234");
			AssertBill(bill3, "OTT1", "AB1D1234");
		}

		public void TestSynchronisation_NotTrimSCACfromBillsDuringSync()
		{
			AssertEquals(0, header.Bills.Count);
			AssertEquals("OTT1", orgProxyCarrierCode.OK_CustomsRegNo);
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "AB1D1234";
			AssertEquals(1, header.Bills.Count);
			AssertBill(header.Bills[0], "OTT1", "AB1D1234");
			synchroniser.SetEnabled(false, false);
			var bill1 = header.Bills[0];
			var bill2 = header.Bills.AddNew();
			bill2.B0_IssuerCode = "OTT1";
			bill2.B0_MasterBillNumber = "MB2";
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "AB1D5678";
			synchroniser.SetEnabled(true, false);
			AssertEquals(2, header.Bills.Count);
			AssertBill(header.Bills[0], bill1.PK, "OTT1", "AB1D1234");
			AssertBill(header.Bills[1], bill2.PK, "OTT1", "MB2");
			synchroniser.Synchronise(true);
			AssertEquals(2, header.Bills.Count);
			AssertBill(header.Bills[0], bill1.PK, "OTT1", "AB1D1234");
			var bill3 = header.Bills[1];
			AssertBill(bill3, "OTT1", "AB1D5678");
			AssertEquals(true, bill2.IsDeleted);
		}

		public void TestSynchronisation_NotTrimSCACfromBillsDuringSync_CoLoad()
		{
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AssertEquals(0, header.Bills.Count);
			AssertEquals("OTT1", orgProxyCarrierCode.OK_CustomsRegNo);
			AssertEquals("ORG1", org1CarrierCode.OK_CustomsRegNo);
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "AB1D1234";
			AssertEquals(1, header.Bills.Count);
			AssertBill(header.Bills[0], "OTT1", "AB1D1234");
			synchroniser.SetEnabled(false, false);
			var bill1 = header.Bills[0];
			var bill2 = header.Bills.AddNew();
			bill2.B0_IssuerCode = "OTT1";
			bill2.B0_MasterBillNumber = "MB2";
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "AB1D5678";
			synchroniser.SetEnabled(true, false);
			AssertEquals(2, header.Bills.Count);
			AssertBill(header.Bills[0], bill1.PK, "OTT1", "AB1D1234");
			AssertBill(header.Bills[1], bill2.PK, "OTT1", "MB2");
			synchroniser.Synchronise(true);
			AssertEquals(2, header.Bills.Count);
			AssertBill(header.Bills[0], bill1.PK, "OTT1", "AB1D1234");
			var bill3 = header.Bills[1];
			AssertBill(bill3, "OTT1", "AB1D5678");
			AssertEquals(true, bill2.IsDeleted);
		}

		public void TestSynchronisationViaDataRefreshBus()
		{
#pragma warning disable
			((IBusinessObjectState)consol).UpdatedByDataRefreshIncludingChildren += new EventHandler(CusInBondBillCollectionSynchroniserTest_UpdatedByDataRefreshIncludingChildren);
#pragma warning restore
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "AB1DHB1";
			synchroniser.Synchronise(true);
			AssertEquals(1, header.Bills.Count);
			var bill = header.Bills[0];
			AssertEquals("OTT1", bill.B0_IssuerCode);
			AssertEquals("AB1DHB1", bill.B0_MasterBillNumber);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var shipmentInOtherFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			AssertEquals("AB1DHB1", shipmentInOtherFactory.JS_HouseBill);
			shipmentInOtherFactory.JS_HouseBill = "EFGHHB2";
			newFactory.Save();
			AssertEquals("EFGHHB2", shipment.JS_HouseBill);
			AssertEquals(1, header.Bills.Count);
			AssertEquals(false, bill.IsDeleted);
			AssertEquals(bill, header.Bills["OTT1", "EFGHHB2"]);
		}

		public void TestBillsAlreadyOnFileShouldNotBeDeletedBySynchronisation()
		{
			AssertEquals(0, header.Bills.Count);
			AssertEquals("OTT1", orgProxyCarrierCode.OK_CustomsRegNo);
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "AB1D1234";
			AssertEquals(1, header.Bills.Count);
			AssertBill(header.Bills[0], "OTT1", "AB1D1234");
			synchroniser.SetEnabled(false, false);
			var bill1 = header.Bills[0];
			var bill2 = header.Bills.AddNew();
			bill2.B0_IssuerCode = "OTT1";
			bill2.B0_MasterBillNumber = "MB2";
			bill2.MovementDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			synchroniser.Synchronise(true);
			AssertEquals("Should not be deleted as it's on Customs File", false, bill2.IsDeleted);
			AssertEquals(2, header.Bills.Count);
			AssertBill(header.Bills[0], bill1.PK, "OTT1", "AB1D1234");
			AssertBill(header.Bills[1], bill2.PK, "OTT1", "MB2");
			bill2.MovementDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.NotOnFile;
			synchroniser.Synchronise(true);
			AssertEquals("Should be deleted as it's not on Customs File", true, bill2.IsDeleted);
			AssertEquals(1, header.Bills.Count);
			AssertBill(header.Bills[0], bill1.PK, "OTT1", "AB1D1234");
		}

		public void TestSynchronisation_CoLoadMaster()
		{
			AssertEquals(0, header.Bills.Count);
			AssertEquals("OTT1", orgProxyCarrierCode.OK_CustomsRegNo);
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1";
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "AB1D1234";
			var shipment1Packline = shipment1.OuterPackLines.AddNew();
			shipment1Packline.SetContainer(consol, container);
			shipment1Packline.JL_MarksAndNumbers = "MARKS1";
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "AB1D5678";
			var shipment2Packline = shipment2.OuterPackLines.AddNew();
			shipment2Packline.SetContainer(consol, container);
			shipment2Packline.JL_MarksAndNumbers = "MARKS2";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment2.JS_JS_ColoadMasterShipment = shipment1.PK;
			AssertEquals(1, header.Bills.Count);
			var bill1 = header.Bills[0];
			AssertBill(bill1, "OTT1", "AB1D5678");
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			AssertEquals(2, header.Bills.Count);
			AssertBill(header.Bills[0], bill1.PK, "OTT1", "AB1D5678");
			var bill2 = header.Bills[1];
			AssertBill(bill2, "OTT1", "AB1D1234");
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_HouseBill = "AB1D9012";
			var shipment3Packline = shipment3.OuterPackLines.AddNew();
			shipment3Packline.SetContainer(consol, container);
			shipment3Packline.JL_MarksAndNumbers = "MARKS3";
			AssertEquals(3, header.Bills.Count);
			AssertBill(header.Bills[0], bill1.PK, "OTT1", "AB1D5678");
			AssertBill(header.Bills[1], bill2.PK, "OTT1", "AB1D1234");
			var bill3 = header.Bills[2];
			AssertBill(bill3, "OTT1", "AB1D9012");
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals(2, header.Bills.Count);
			AssertBill(header.Bills[0], bill1.PK, "OTT1", "AB1D5678");
			AssertBill(header.Bills[1], bill2.PK, "OTT1", "AB1D1234");
			AssertEquals(true, bill3.IsDeleted);
			var org1 = Factory.New<OrgHeader>();
			org1.OH_IsForwarder = true;
			org1.MiscServ.OM_FWDirectAMSReporter = true;
			var org2 = Factory.New<OrgHeader>();
			org2.OH_IsForwarder = true;
			org2.MiscServ.OM_FWDirectAMSReporter = false;
			shipment3.ConsignorPK = org1.PK;
			shipment2.JS_JS_ColoadMasterShipment = shipment3.PK;
			AssertEquals(1, header.Bills.Count);
			AssertBill(header.Bills[0], bill2.PK, "OTT1", "AB1D1234");
			AssertEquals(true, bill1.IsDeleted);
			shipment3.ConsignorPK = org2.PK;
			AssertEquals(2, header.Bills.Count);
			AssertBill(header.Bills[0], bill2.PK, "OTT1", "AB1D1234");
			bill3 = header.Bills[1];
			AssertBill(bill3, "OTT1", "AB1D5678");
			// AssemblyMaster should be like coload master
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			AssertEquals(2, header.Bills.Count);
			AssertBill(header.Bills[0], bill2.PK, "OTT1", "AB1D1234");
			AssertBill(header.Bills[1], bill3.PK, "OTT1", "AB1D5678");
		}

		void CusInBondBillCollectionSynchroniserTest_UpdatedByDataRefreshIncludingChildren(object sender, EventArgs e)
		{
			synchroniser.Synchronise();
		}

		CusInBondBillCollectionSynchroniser synchroniser;
		protected override void SetUp()
		{
			base.SetUp();
			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			synchroniser = new CusInBondBillCollectionSynchroniser(header);
		}
	}
}
