using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business.Testing
{
	partial class AgencyShipmentTest
	{
		public void TestPortDefaultingFromBuyerConsignorRelationship()
		{
			OrgHeader buyer = Factory.New<OrgHeader>();
			buyer.OH_Code = "AUSBUYER";
			buyer.OH_RL_NKClosestPort = "AUSYD";
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "NZSUPPLIER";
			supplier.OH_RL_NKClosestPort = "NZAKL";

			OrgSupplierBuyerLink link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = buyer.PK;
			link.OL_OH_Supplier = supplier.PK;
			OrgSupBuyLinkTrnMode linkTrnMode = link.OrgSupBuyLinkTrnModes[0];
			linkTrnMode.PF_OL = link.PK;
			linkTrnMode.PF_TransportMode = Core.Constants.TransportModes.Sea;
			linkTrnMode.PF_ContainerMode = Core.Constants.ContainerModes.FCL;
			linkTrnMode.PF_RL_NKLoadPort = "NZWLG";
			linkTrnMode.PF_RL_NKDischargePort = "AUBNE";

			Factory.Save();

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.ConsigneePK = buyer.PK;
			shipment.ConsignorPK = supplier.PK;

			AssertEquals("Load should default from SupplierBuyerLink", "NZWLG", shipment.JS_NKLoadPort);
			AssertEquals("Discharge should default from SupplierBuyerLink", "AUBNE", shipment.JS_NKDischargePort);

			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "GBLON";
			VoyageDestination destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			JobSailing sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			AgencyShipment shipment2 = Factory.New<AgencyShipment>();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment2.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment2.JS_JX = sailing.PK;
			shipment2.ConsigneePK = buyer.PK;
			shipment2.ConsignorPK = supplier.PK;

			AssertEquals("Sailing attached to shipment, load should not default from SupplierBuyerLink", "GBLON", shipment2.JS_NKLoadPort);
			AssertEquals("Sailing attached to shipment, discharge should not default from SupplierBuyerLink", "USLAX", shipment2.JS_NKDischargePort);
		}

		public void TestBSConsumer_BuyerSupplierDefaultingFromConsignor()
		{
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";

			OrgSupplierBuyerLink link = consignor.BuyerLinks.AddNew();
			link.OL_OH_Buyer = consignee.PK;
			link.OL_RN_NKImporterCountry = "AU";

			OrgSupBuyLinkTrnMode mode = link.OrgSupBuyLinkTrnModes[0];
			mode.PF_TransportMode = Constants.TransportModes.Sea;
			mode.PF_ContainerMode = Constants.ContainerModes.FCL;
			mode.PF_GoodsDescription = "Super Magic!";
			mode.PF_RS_NKDefaultServiceLevel = "URG";

			Factory.Save();

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKDestination = "AUBNE";

			AssertEquals("Pre-Condition: ConsigneePK", ZGuid.Empty, shipment.ConsigneePK);
			AssertEquals("Pre-Condition: Goods Description", "", shipment.JS_GoodsDescription);
			AssertEquals("Pre-Condition: Service Level", "STD", shipment.JS_RS_NKServiceLevel);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			AssertEquals("ConsigneePK", consignee.PK, shipment.ConsigneePK);
			AssertEquals("Goods Description", "Super Magic!", shipment.JS_GoodsDescription);
			AssertEquals("Service Level", "URG", shipment.JS_RS_NKServiceLevel);
		}

		public void TestRestoreNumberOfBills()
		{
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";

			Enterprise.Registry.Business.ReleaseTypes newValue = new Enterprise.Registry.Business.ReleaseTypes();
			newValue.OriginalsNumber = 10;
			newValue.CopiesNumber = 11;
			Enterprise.Registry.Business.ReleaseType value1 = new Enterprise.Registry.Business.ReleaseType();
			value1.Code = "AAA";
			value1.Description = (NoResString)"blah blah blah";
			value1.OriginalsNumber = 12;
			value1.CopiesNumber = 13;

			Enterprise.Registry.Business.ReleaseType value2 = new Enterprise.Registry.Business.ReleaseType();
			value2.Code = "BBB";
			value2.Description = (NoResString)"blah blah blah";
			value2.OriginalsNumber = 14;
			value2.CopiesNumber = 15;

			newValue.Types.Add(value1);
			newValue.Types.Add(value2);

			AgencyRegistry.Instance.ReleaseTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

			consignee.MiscServ.OM_IMOriginalSeaBills = 0;
			consignee.MiscServ.OM_IMCopySeaBills = 0;

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_ReleaseType = "AAA";
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			shipment.BuyerSupplierLinksHelper.RestoreNumberOfBills();
			AssertEquals("Default from Registry", (ZByte)12, shipment.JS_NoOriginalBills);
			AssertEquals("Default from Registry", (ZByte)13, shipment.JS_NoCopyBills);

			shipment.JS_ReleaseType = "BBB";
			AssertEquals("Default from Registry", (ZByte)14, shipment.JS_NoOriginalBills);
			AssertEquals("Default from Registry", (ZByte)15, shipment.JS_NoCopyBills);

			consignee.MiscServ.OM_IMOriginalSeaBills = 16;
			consignee.MiscServ.OM_IMCopySeaBills = 17;
			shipment.BuyerSupplierLinksHelper.RestoreNumberOfBills();
			AssertEquals("Default from Consignee", (ZByte)16, shipment.JS_NoOriginalBills);
			AssertEquals("Default from Consignee", (ZByte)17, shipment.JS_NoCopyBills);

			shipment.JS_ReleaseType = "AAA";
			AssertEquals("Default from Consignee", (ZByte)16, shipment.JS_NoOriginalBills);
			AssertEquals("Default from Consignee", (ZByte)17, shipment.JS_NoCopyBills);

			OrgSupplierBuyerLink link = consignor.BuyerLinks.AddNew();
			link.OL_OH_Buyer = consignee.PK;
			link.OL_RN_NKImporterCountry = "AU";

			OrgSupBuyLinkTrnMode mode = link.OrgSupBuyLinkTrnModes[0];
			mode.PF_TransportMode = Constants.TransportModes.Sea;
			mode.PF_ContainerMode = Constants.ContainerModes.FCL;
			mode.PF_GoodsDescription = "Super Magic!";
			mode.PF_NoOfOriginalBills = 18;
			mode.PF_NoOfCopyBills = 19;
			shipment.BuyerSupplierLinksHelper.RestoreNumberOfBills();
			AssertEquals("Default from link", (ZByte)18, shipment.JS_NoOriginalBills);
			AssertEquals("Default from link", (ZByte)19, shipment.JS_NoCopyBills);

			shipment.JS_ReleaseType = "BBB";
			AssertEquals("Default from link", (ZByte)18, shipment.JS_NoOriginalBills);
			AssertEquals("Default from link", (ZByte)19, shipment.JS_NoCopyBills);

			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.ExpressBofL;
			AssertEquals("Default from Registry for Release Type EBL", (ZByte)0, shipment.JS_NoOriginalBills);
			AssertEquals("Default from Registry for Release Type EBL", (ZByte)1, shipment.JS_NoCopyBills);
		}

		public void TestBSConsumer_BuyerSupplierDefaultingFromConsignee()
		{
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";

			OrgSupplierBuyerLink link = consignor.BuyerLinks.AddNew();
			link.OL_OH_Buyer = consignee.PK;
			link.OL_RN_NKImporterCountry = "AU";

			OrgSupBuyLinkTrnMode mode = link.OrgSupBuyLinkTrnModes[0];
			mode.PF_TransportMode = Constants.TransportModes.Sea;
			mode.PF_ContainerMode = Constants.ContainerModes.FCL;
			mode.PF_GoodsDescription = "Super Magic!";

			Factory.Save();

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKDestination = "AUBNE";

			AssertEquals("Pre-Condition: ConsignorPK", ZGuid.Empty, shipment.ConsignorPK);
			AssertEquals("Pre-Condition: Goods Description", "", shipment.JS_GoodsDescription);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			AssertEquals("ConsignorPK", consignor.PK, shipment.ConsignorPK);
			AssertEquals("Goods Description", "Super Magic!", shipment.JS_GoodsDescription);
		}

		public void TestBSConsumer_OriginAndDestination()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NLAMS";

			IBuyerSupplierRelationshipConsumer consumer = shipment;
			AssertEquals("Get Origin", "AUBNE", consumer.Origin);
			AssertEquals("Get Destination", "NLAMS", consumer.Destination);

			consumer.Origin = "AUSYD";
			consumer.Destination = "GBLON";
			AssertEquals("Set Origin", "AUSYD", shipment.JS_RL_NKOrigin);
			AssertEquals("Set Destination", "GBLON", shipment.JS_RL_NKDestination);
		}

		public void TestBSConsumer_ContainerMode()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			IBuyerSupplierRelationshipConsumer consumer = shipment;
			AssertEquals("Get Container Mode", Constants.ContainerModes.FCL, consumer.ContainerMode);

			consumer.ContainerMode = Constants.ContainerModes.BreakBulk;
			AssertEquals("Set Container Mode", Constants.ContainerModes.BreakBulk, shipment.JS_PackingMode);
		}

		public void TestBSConsumer_ShouldPromptToSaveBuyerSupplierRelationship()
		{
			IBuyerSupplierRelationshipConsumer consumer = Factory.New<AgencyShipment>();

			Env.Registry.PromptToSaveBuyerSupplier = true;
			AssertEquals(true, consumer.ShouldPromptToSaveBuyerSupplierRelationship);

			Env.Registry.PromptToSaveBuyerSupplier = false;
			AssertEquals(false, consumer.ShouldPromptToSaveBuyerSupplierRelationship);
		}

		public void TestBSConsumer_BillCounts()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 4;

			IBuyerSupplierRelationshipConsumer consumer = shipment;
			AssertEquals("Get Original Bills", (byte)2, consumer.NoOriginalBills);
			AssertEquals("Get Copy Bills", (byte)4, consumer.NoCopyBills);

			consumer.NoOriginalBills = 1;
			consumer.NoCopyBills = 2;
			AssertEquals("Set Original Bills", (byte)1, shipment.JS_NoOriginalBills);
			AssertEquals("Set Copy Bills", (byte)2, shipment.JS_NoCopyBills);
		}

		public void TestBSConsumer_PaymentTerm()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_INCO = Constants.DomesticPaymentTerms.Prepaid;

			IBuyerSupplierRelationshipConsumer consumer = shipment;
			AssertEquals("Get Payment Term", Constants.DomesticPaymentTerms.Prepaid, consumer.PaymentTerms);
			AssertEquals("Get Should Restore Payment Term", true, consumer.ShouldRestorePaymentTerm);

			consumer.PaymentTerms = Constants.DomesticPaymentTerms.Collect;
			AssertEquals("Set Payment Term", Constants.DomesticPaymentTerms.Collect, shipment.JS_INCO);
		}

		public void TestBSConsumer_ReleaseType()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_ReleaseType = "SWB";

			IBuyerSupplierRelationshipConsumer consumer = shipment;
			AssertEquals("Get Release Type", "SWB", consumer.ReleaseType);
		}

		public void TestBSConsumer_ServiceLevel()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_RS_NKServiceLevel = "D2D";

			IBuyerSupplierRelationshipConsumer consumer = shipment;
			AssertEquals("Get Service Level", "D2D", consumer.ServiceLevel);

			consumer.ServiceLevel = "DEP";
			AssertEquals("Set Service Level", "DEP", shipment.JS_RS_NKServiceLevel);
		}
	}
}
