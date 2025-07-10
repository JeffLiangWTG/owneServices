using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class BuyerSupplierLinksHelperTest : TestCaseWithFactory
	{
		public void TestCarrierNotDefaultedOnNewLink()
		{
			ShipmentForTest shipment = Factory.New<ShipmentForTest>();
			BuyerSupplierLinksHelper<ShipmentForTest> linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			((IBuyerSupplierRelationshipConsumer)shipment).ShippingLinePK = shippingLine.PK;
			linksHelper.AddNewBuyerSupplierLinkGivenOrgs(consignee, consignor);
			AssertNotEquals(shippingLine.PK, consignee.SupplierLinks[0].OrgSupBuyLinkTrnModes[0].PF_OH_CarrierLine);
		}

		public void TestAddNewRestoreBuyerSupplierLink()
		{
			RefUNLOCO currentPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			ZQuery otherCountryFilter = new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			RefCountry otherCountry = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			RefUNLOCO otherUnloco1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry.RN_Code));
			otherCountryFilter.AddToFilter(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, otherCountry.RN_Code);
			RefCountry otherCountry2 = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			RefUNLOCO otherUnloco2 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry2.RN_Code));

			ShipmentForTest shipment = Factory.New<ShipmentForTest>();

			OrgHeader deliveryCartage = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader pickupCartage = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader importBroker = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = otherUnloco1.RL_Code;

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.JS_RL_NKDestination = currentPort.RL_Code;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			((IBuyerSupplierRelationshipConsumer)shipment).ImportBrokerPK = importBroker.PK;
			((IBuyerSupplierRelationshipConsumer)shipment).GoodsCurrency = "UAH";
			((IBuyerSupplierRelationshipConsumer)shipment).PaymentTerms = "FOB";
			shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			((IBuyerSupplierRelationshipConsumer)shipment).NoOriginalBills = (ZByte)3;
			((IBuyerSupplierRelationshipConsumer)shipment).NoCopyBills = (ZByte)3;

			BuyerSupplierLinksHelper<ShipmentForTest> linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();
			linksHelper.AddNewBuyerSupplierLink();

			Factory.Save();

			ZQuery filter = new ZQuery(OrgSupplierBuyerLinkSchema.OL_OH_Buyer, consignee.PK);
			filter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Supplier, consignor.PK);
			OrgSupplierBuyerLink[] links = Factory.Load<OrgSupplierBuyerLink>(filter);
			AssertEquals("Supplier Buyer Link not saved.", 1, links.Length);

			OrgSupplierBuyerLink link = links[0];

			AssertEquals("Values copied to buyer supplier link", consignee.PK, link.OL_OH_Buyer);
			AssertEquals("Values copied to buyer supplier link", consignor.PK, link.OL_OH_Supplier);
			AssertEquals("Values copied to buyer supplier link", importBroker.PK, link.OL_OH_ImportBroker);
			AssertEquals("Values copied to buyer supplier link", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, link.OL_RN_NKImporterCountry);
			AssertEquals("Values copied to buyer supplier link", "UAH", link.OL_RX_NKDefaultCurrency);
			AssertEquals("Values copied to buyer supplier link", "FOB", link.OrgSupBuyLinkTrnModes[0].PF_IncoTerm);
			AssertEquals("Values copied to buyer supplier link", Core.Constants.TransportModes.Air, link.OrgSupBuyLinkTrnModes[0].PF_TransportMode);
			AssertEquals("Values copied to buyer supplier link", Core.Constants.ContainerModes.Loose, link.OrgSupBuyLinkTrnModes[0].PF_ContainerMode);
			AssertEquals("Values copied to buyer supplier link", (ZByte)3, link.OrgSupBuyLinkTrnModes[0].PF_NoOfCopyBills);
			AssertEquals("Values copied to buyer supplier link", (ZByte)3, link.OrgSupBuyLinkTrnModes[0].PF_NoOfOriginalBills);

			shipment.JS_RL_NKDestination = otherUnloco1.RL_Code;
			((IBuyerSupplierRelationshipConsumer)shipment).GoodsCurrency = "USD";
			((IBuyerSupplierRelationshipConsumer)shipment).PaymentTerms = "CIF";
			linksHelper.AddNewBuyerSupplierLink();
			Factory.Save();

			filter.AddToFilter(OrgSupplierBuyerLinkSchema.PK, SQLComparisonOperator.NotEqual, link.PK);
			links = Factory.Load<OrgSupplierBuyerLink>(filter);
			AssertEquals(1, links.Length);
			OrgSupplierBuyerLink link2 = links[0];

			AssertEquals("Values copied to buyer supplier link", consignee.PK, link2.OL_OH_Buyer);
			AssertEquals("Values copied to buyer supplier link", consignor.PK, link2.OL_OH_Supplier);
			AssertEquals("Values copied to buyer supplier link", importBroker.PK, link2.OL_OH_ImportBroker);
			AssertEquals("Values copied to buyer supplier link", otherCountry.Code, link2.OL_RN_NKImporterCountry);
			AssertEquals("Values copied to buyer supplier link", "USD", link2.OL_RX_NKDefaultCurrency);
			AssertEquals("Values copied to buyer supplier link", "CIF", link2.OrgSupBuyLinkTrnModes[0].PF_IncoTerm);
			AssertEquals("Values copied to buyer supplier link", Core.Constants.TransportModes.Air, link2.OrgSupBuyLinkTrnModes[0].PF_TransportMode);
			AssertEquals("Values copied to buyer supplier link", Core.Constants.ContainerModes.Loose, link2.OrgSupBuyLinkTrnModes[0].PF_ContainerMode);
			AssertEquals("Values copied to buyer supplier link", (ZByte)3, link2.OrgSupBuyLinkTrnModes[0].PF_NoOfCopyBills);
			AssertEquals("Values copied to buyer supplier link", (ZByte)3, link2.OrgSupBuyLinkTrnModes[0].PF_NoOfOriginalBills);

			ShipmentForTest shipmentNew = Factory.New<ShipmentForTest>();
			BuyerSupplierLinksHelper<ShipmentForTest> linksHelperNew = new BuyerSupplierLinksHelper<ShipmentForTest>(shipmentNew);
			linksHelperNew.Register();
			link.Delete();
			shipmentNew.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			AssertEquals("Values restored from buyer supplier link", consignee.PK, shipmentNew.Consignee.PK);
			AssertEquals("Values restored from buyer supplier link", consignor.PK, shipmentNew.Consignor.PK);
			AssertEquals("Values restored from buyer supplier link", importBroker.PK, ((IBuyerSupplierRelationshipConsumer)shipmentNew).ImportBrokerPK);
			AssertEquals("Values restored from buyer supplier link", "USD", ((IBuyerSupplierRelationshipConsumer)shipmentNew).GoodsCurrency);
			AssertEquals("Values restored from buyer supplier link", "CIF", ((IBuyerSupplierRelationshipConsumer)shipmentNew).PaymentTerms);
			AssertEquals("Values restored from buyer supplier link", (ZByte)3, ((IBuyerSupplierRelationshipConsumer)shipmentNew).NoCopyBills);
			AssertEquals("Values restored from buyer supplier link", (ZByte)3, ((IBuyerSupplierRelationshipConsumer)shipmentNew).NoOriginalBills);
		}

		public void TestBuyerSupplierFieldsToExcludeRegistry()
		{
			var consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			var consignor1 = Factory.NewWithValidTestData<OrgHeader>();
			var importBroker = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.New<ShipmentForTest>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee1.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor1.PK;
			((IBuyerSupplierRelationshipConsumer)shipment).ImportBrokerPK = importBroker.PK;
			((IBuyerSupplierRelationshipConsumer)shipment).GoodsCurrency = "UAH";
			((IBuyerSupplierRelationshipConsumer)shipment).PaymentTerms = "FCA";
			shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			((IBuyerSupplierRelationshipConsumer)shipment).NoOriginalBills = (ZByte)3;
			((IBuyerSupplierRelationshipConsumer)shipment).NoCopyBills = (ZByte)3;

			var fieldsToExclude_None = new CodeDescriptionBoolCollection
			{
				{ OrganisationsDataRegistry.BuyerSupplierRelationshipFields.ImportBroker, (NoResString)"Import Broker", false },
				{ OrganisationsDataRegistry.BuyerSupplierRelationshipFields.DefaultCurrency, (NoResString)"Default Currency", false },
				{ OrganisationsDataRegistry.BuyerSupplierRelationshipFields.Incoterm, (NoResString)"Incoterm", false },
				{ OrganisationsDataRegistry.BuyerSupplierRelationshipFields.ContainerMode, (NoResString)"Container Mode", false },
				{ OrganisationsDataRegistry.BuyerSupplierRelationshipFields.OriginalBills, (NoResString)"Original Bills", false },
				{ OrganisationsDataRegistry.BuyerSupplierRelationshipFields.CopyBills, (NoResString)"Copy Bills", false },
			};

			OrganisationsDataRegistry.Instance.BuyerSupplierRelationshipFieldsToExclude.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fieldsToExclude_None);

			BuyerSupplierLinksHelper<ShipmentForTest> linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();
			linksHelper.AddNewBuyerSupplierLink();

			Factory.Save();

			var filter = new ZQuery(OrgSupplierBuyerLinkSchema.OL_OH_Buyer, consignee1.PK);
			filter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Supplier, consignor1.PK);

			OrgSupplierBuyerLink[] links = Factory.Load<OrgSupplierBuyerLink>(filter);
			AssertEquals(1, links.Length);

			var link = links[0];
			AssertEquals("Values copied to buyer supplier link", importBroker.PK, link.OL_OH_ImportBroker);
			AssertEquals("Values copied to buyer supplier link", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, link.OL_RN_NKImporterCountry);
			AssertEquals("Values copied to buyer supplier link", "UAH", link.OL_RX_NKDefaultCurrency);
			AssertEquals("Values copied to buyer supplier link", "FCA", link.OrgSupBuyLinkTrnModes[0].PF_IncoTerm);
			AssertEquals("Values copied to buyer supplier link", Core.Constants.ContainerModes.Loose, link.OrgSupBuyLinkTrnModes[0].PF_ContainerMode);
			AssertEquals("Values copied to buyer supplier link", (ZByte)3, link.OrgSupBuyLinkTrnModes[0].PF_NoOfCopyBills);
			AssertEquals("Values copied to buyer supplier link", (ZByte)3, link.OrgSupBuyLinkTrnModes[0].PF_NoOfOriginalBills);

			var consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			var consignor2 = Factory.NewWithValidTestData<OrgHeader>();

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee2.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor2.PK;

			var fieldsToExclude_All = new CodeDescriptionBoolCollection
			{
				{ OrganisationsDataRegistry.BuyerSupplierRelationshipFields.ImportBroker, (NoResString)"Import Broker", true },
				{ OrganisationsDataRegistry.BuyerSupplierRelationshipFields.DefaultCurrency, (NoResString)"Default Currency", true },
				{ OrganisationsDataRegistry.BuyerSupplierRelationshipFields.Incoterm, (NoResString)"Incoterm", true },
				{ OrganisationsDataRegistry.BuyerSupplierRelationshipFields.ContainerMode, (NoResString)"Container Mode", true },
				{ OrganisationsDataRegistry.BuyerSupplierRelationshipFields.OriginalBills, (NoResString)"Original Bills", true },
				{ OrganisationsDataRegistry.BuyerSupplierRelationshipFields.CopyBills, (NoResString)"Copy Bills", true },
			};

			OrganisationsDataRegistry.Instance.BuyerSupplierRelationshipFieldsToExclude.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fieldsToExclude_All);

			linksHelper.AddNewBuyerSupplierLink();

			Factory.Save();

			filter = new ZQuery(OrgSupplierBuyerLinkSchema.OL_OH_Buyer, consignee2.PK);
			filter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Supplier, consignor2.PK);
			links = Factory.Load<OrgSupplierBuyerLink>(filter);
			AssertEquals(1, links.Length);

			link = links[0];
			AssertEquals("Values NOT copied to buyer supplier link", ZGuid.Empty, link.OL_OH_ImportBroker);
			AssertEquals("Values NOT copied to buyer supplier link", ZString.Empty, link.OL_RX_NKDefaultCurrency);
			AssertEquals("Values NOT copied to buyer supplier link", ZString.Empty, link.OrgSupBuyLinkTrnModes[0].PF_IncoTerm);
			AssertEquals("Values NOT copied to buyer supplier link", ZString.Empty, link.OrgSupBuyLinkTrnModes[0].PF_ContainerMode);
			AssertEquals("Values NOT copied to buyer supplier link", (ZByte)0, link.OrgSupBuyLinkTrnModes[0].PF_NoOfCopyBills);
			AssertEquals("Values NOT copied to buyer supplier link", (ZByte)0, link.OrgSupBuyLinkTrnModes[0].PF_NoOfOriginalBills);

			link.Validation.ValidateAll();
			AssertNoErrors("Values excluded from the relationship as per registry must be set to an empty value!", link);
		}

		public void TestBlankCurrency()
		{
			ShipmentForTest shipment = Factory.New<ShipmentForTest>();
			BuyerSupplierLinksHelper<ShipmentForTest> linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.JS_RX_NKGoodsValueCurr = "";
			linksHelper.AddNewBuyerSupplierLinkGivenOrgs(consignee, consignor);
			Assert(consignee.SupplierLinks[0].OL_RX_NKDefaultCurrency == "AUD");

			ShipmentForTest shipment2 = Factory.New<ShipmentForTest>();
			BuyerSupplierLinksHelper<ShipmentForTest> linksHelper2 = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment2);
			OrgHeader consignor2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			shipment2.JS_RX_NKGoodsValueCurr = "EUR";
			linksHelper2.AddNewBuyerSupplierLinkGivenOrgs(consignee2, consignor2);
			AssertEquals(consignee2.SupplierLinks[0].OL_RX_NKDefaultCurrency, "EUR");
		}

		public void TestInvalidTransportMode()
		{
			ShipmentForTest shipment = Factory.New<ShipmentForTest>();
			BuyerSupplierLinksHelper<ShipmentForTest> linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.AirSea;
			linksHelper.AddNewBuyerSupplierLinkGivenOrgs(consignee, consignor);
			AssertEquals(consignee.SupplierLinks[0].OrgSupBuyLinkTrnModes[0].PF_TransportMode, Core.Constants.TransportModes.All);

			shipment = Factory.New<ShipmentForTest>();
			linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();
			consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.SeaAir;
			linksHelper.AddNewBuyerSupplierLinkGivenOrgs(consignee, consignor);
			AssertEquals(consignee.SupplierLinks[0].OrgSupBuyLinkTrnModes[0].PF_TransportMode, Core.Constants.TransportModes.All);

			shipment = Factory.New<ShipmentForTest>();
			linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();
			consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			linksHelper.AddNewBuyerSupplierLinkGivenOrgs(consignee, consignor);
			AssertEquals(consignee.SupplierLinks[0].OrgSupBuyLinkTrnModes[0].PF_TransportMode, Core.Constants.TransportModes.Air);
		}

		public void TestBlankPaymentTerm()
		{
			ShipmentForTest shipment = Factory.New<ShipmentForTest>();
			BuyerSupplierLinksHelper<ShipmentForTest> linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.JS_INCO = "";
			shipment.JS_TransportMode = "AIR";
			linksHelper.AddNewBuyerSupplierLinkGivenOrgs(consignee, consignor);
			shipment.JS_INCO = "DDP";

			shipment.JS_TransportMode = "SEA";
			shipment.JS_TransportMode = "AIR";
			AssertEquals("DDP", shipment.JS_INCO);
		}

		public void TestServiceLevelFromOrg()
		{
			ShipmentForTest shipment = Factory.New<ShipmentForTest>();
			new BuyerSupplierLinksHelper<ShipmentForTest>(shipment).Register();

			AssertEquals(false, shipment.ServiceLevelFallBackSet);
			shipment.JS_TransportMode = "AIR";
			AssertEquals(true, shipment.ServiceLevelFallBackSet);
		}

		public void TestShowRelatedOrgsProperties()
		{
			ShipmentForTest shipment = Factory.New<ShipmentForTest>();
			shipment.JS_UniqueConsignRef = "S1010";

			BuyerSupplierLinksHelper<ShipmentForTest> linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			OrgHeader consignor1 = Factory.New<OrgHeader>();
			consignor1.OH_Code = "SUPP-ONE";
			consignor1.OH_FullName = "Supplier One";
			consignor1.MainAddress.OA_Address1 = "Supplier Address One";

			OrgHeader consignor2 = Factory.New<OrgHeader>();
			consignor2.OH_Code = "SUPP-TWO";
			consignor2.OH_FullName = "Supplier Two";
			consignor2.MainAddress.OA_Address1 = "Supplier Address Two";

			OrgHeader consignor3 = Factory.New<OrgHeader>();
			consignor3.OH_Code = "SUPP-THREE";
			consignor3.OH_FullName = "Supplier Three";
			consignor3.MainAddress.OA_Address1 = "Supplier Address Three";

			OrgHeader consignee1 = Factory.New<OrgHeader>();
			consignee1.OH_Code = "BUY-ONE";
			consignee1.OH_FullName = "Buyer One";
			consignee1.MainAddress.OA_Address1 = "Buyer Address One";

			OrgHeader consignee2 = Factory.New<OrgHeader>();
			consignee2.OH_Code = "BUY-TWO";
			consignee2.OH_FullName = "Buyer Two";
			consignee2.MainAddress.OA_Address1 = "Buyer Address Two";

			OrgHeader consignee3 = Factory.New<OrgHeader>();
			consignee3.OH_Code = "BUY-THREE";
			consignee3.OH_FullName = "Buyer Three";
			consignee3.MainAddress.OA_Address1 = "Buyer Address Three";

			//Link Consignor1 <--> Consignee1
			OrgSupplierBuyerLink link1 = consignor1.BuyerLinks.AddNew();
			link1.OL_OH_Buyer = consignee1.PK;

			//Link Consignor2 <--> Consignee1
			OrgSupplierBuyerLink link2 = consignor2.BuyerLinks.AddNew();
			link2.OL_OH_Buyer = consignee1.PK;

			//Link Consignor2 <--> Consignee2
			OrgSupplierBuyerLink link3 = consignor2.BuyerLinks.AddNew();
			link3.OL_OH_Buyer = consignee2.PK;

			Factory.Save();

			shipment.ConsignorPK = ZGuid.NewZGuid();
			shipment.ConsigneePK = ZGuid.NewZGuid();
			Assert("Consignor already entered, don't show related Consignors.", !linksHelper.ShouldShowRelatedConsignors);
			Assert("Consignee already entered, don't show related Consignees.", !linksHelper.ShouldShowRelatedConsignees);

			shipment.ConsignorPK = ZGuid.Empty;
			shipment.ConsigneePK = ZGuid.Empty;
			Assert("No Consignor, can't show related Consignees.", !linksHelper.ShouldShowRelatedConsignees);
			Assert("No Consignee, can't show related Consignors.", !linksHelper.ShouldShowRelatedConsignors);

			shipment.ConsignorPK = consignor1.PK;
			Assert("Only 1 related Consignee, don't show dialog.", !linksHelper.ShouldShowRelatedConsignees);

			shipment.ConsignorPK = consignor2.PK;
			shipment.ConsigneePK = ZGuid.Empty;
			Assert("Multiple related Consignees, show dialog.", linksHelper.ShouldShowRelatedConsignees);

			shipment.ConsignorPK = consignor3.PK;
			Assert("No related Consignees, don't show dialog.", !linksHelper.ShouldShowRelatedConsignees);

			shipment.ConsignorPK = ZGuid.Empty;
			shipment.ConsigneePK = consignee2.PK;
			Assert("Only 1 related Consignor, don't show dialog.", !linksHelper.ShouldShowRelatedConsignors);
		}

		public void TestDoNotShowRelatedOrgsPropertiesWhenRegistryIsOff()
		{
			Env.Registry.UseBuyerSupplierRelationships = false;
			ShipmentForTest shipment = Factory.New<ShipmentForTest>();
			shipment.JS_UniqueConsignRef = "S1010";

			BuyerSupplierLinksHelper<ShipmentForTest> linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			OrgHeader consignor1 = Factory.New<OrgHeader>();
			consignor1.OH_Code = "SUPP-ONE";
			consignor1.OH_FullName = "Supplier One";
			consignor1.MainAddress.OA_Address1 = "Supplier Address One";

			OrgHeader consignor2 = Factory.New<OrgHeader>();
			consignor2.OH_Code = "SUPP-TWO";
			consignor2.OH_FullName = "Supplier Two";
			consignor2.MainAddress.OA_Address1 = "Supplier Address Two";

			OrgHeader consignor3 = Factory.New<OrgHeader>();
			consignor3.OH_Code = "SUPP-THREE";
			consignor3.OH_FullName = "Supplier Three";
			consignor3.MainAddress.OA_Address1 = "Supplier Address Three";

			OrgHeader consignee1 = Factory.New<OrgHeader>();
			consignee1.OH_Code = "BUY-ONE";
			consignee1.OH_FullName = "Buyer One";
			consignee1.MainAddress.OA_Address1 = "Buyer Address One";

			OrgHeader consignee2 = Factory.New<OrgHeader>();
			consignee2.OH_Code = "BUY-TWO";
			consignee2.OH_FullName = "Buyer Two";
			consignee2.MainAddress.OA_Address1 = "Buyer Address Two";

			OrgHeader consignee3 = Factory.New<OrgHeader>();
			consignee3.OH_Code = "BUY-THREE";
			consignee3.OH_FullName = "Buyer Three";
			consignee3.MainAddress.OA_Address1 = "Buyer Address Three";

			//Link Consignor1 <--> Consignee1
			OrgSupplierBuyerLink link1 = consignor1.BuyerLinks.AddNew();
			link1.OL_OH_Buyer = consignee1.PK;

			//Link Consignor2 <--> Consignee1
			OrgSupplierBuyerLink link2 = consignor2.BuyerLinks.AddNew();
			link2.OL_OH_Buyer = consignee1.PK;

			//Link Consignor2 <--> Consignee2
			OrgSupplierBuyerLink link3 = consignor2.BuyerLinks.AddNew();
			link3.OL_OH_Buyer = consignee2.PK;

			Factory.Save();

			shipment.ConsignorPK = ZGuid.NewZGuid();
			shipment.ConsigneePK = ZGuid.NewZGuid();
			Assert("Registry is off: Do Not show dialog.", !linksHelper.ShouldShowRelatedConsignors);
			Assert("Registry is off: Do Not show dialog.", !linksHelper.ShouldShowRelatedConsignees);

			shipment.ConsignorPK = ZGuid.Empty;
			shipment.ConsigneePK = ZGuid.Empty;
			Assert("Registry is off: Do Not show dialog.", !linksHelper.ShouldShowRelatedConsignees);
			Assert("Registry is off: Do Not show dialog.", !linksHelper.ShouldShowRelatedConsignors);

			shipment.ConsignorPK = consignor1.PK;
			Assert("Registry is off: Do Not show dialog.", !linksHelper.ShouldShowRelatedConsignees);

			shipment.ConsignorPK = consignor2.PK;
			shipment.ConsigneePK = ZGuid.Empty;
			Assert("Multiple related Consignees, but registry is off: Do Not show dialog.", !linksHelper.ShouldShowRelatedConsignees);
		}

		public void TestBuyerDefaultedFromSupplier()
		{
			OrgHeader supplier = Factory.New<OrgHeader>();
			OrgHeader buyer1 = Factory.New<OrgHeader>();
			OrgHeader buyer2 = Factory.New<OrgHeader>();

			ShipmentForTest shipment = Factory.New<ShipmentForTest>();
			BuyerSupplierLinksHelper<ShipmentForTest> linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			shipment.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;
			AssertEquals("precondition: should not default when there are no links", ZGuid.Empty, shipment.ConsigneePK);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			supplier.BuyerLinks.AddNew(buyer1);
			shipment.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;
			AssertEquals("should default when there is exactly one link", buyer1.PK, shipment.ConsigneePK);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			supplier.BuyerLinks.AddNew(buyer2);
			shipment.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;
			AssertEquals("should not default when there is more than 1 link as the gui will deal with it", ZGuid.Empty, shipment.ConsigneePK);
		}

		public void TestBuyerDoesNotDefaultFromSupplierWhenRegistryIsOff()
		{
			Env.Registry.UseBuyerSupplierRelationships = false;
			OrgHeader supplier = Factory.New<OrgHeader>();
			OrgHeader buyer1 = Factory.New<OrgHeader>();

			ShipmentForTest shipment = Factory.New<ShipmentForTest>();
			BuyerSupplierLinksHelper<ShipmentForTest> linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			shipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			supplier.BuyerLinks.AddNew(buyer1);
			shipment.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;
			AssertEquals("should NOT default when the registry is off even if there is exactly one link", ZGuid.Empty, shipment.ConsigneePK);
		}

		public void TestBuyerDefaultedFromSupplier_OverrideBuyer()
		{
			OrgHeader supplier = Factory.New<OrgHeader>();
			OrgHeader buyer1 = Factory.New<OrgHeader>();
			supplier.BuyerLinks.AddNew(buyer1);

			ShipmentForTest shipment = Factory.New<ShipmentForTest>();
			BuyerSupplierLinksHelper<ShipmentForTest> linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_Address1 = "Override";
			shipment.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;
			AssertNotEquals("should NOT default when there is an overridden consignor address", buyer1.PK, shipment.ConsigneePK);
		}

		public void TestSupplierDefaultedFromBuyer()
		{
			OrgHeader buyer = Factory.New<OrgHeader>();
			OrgHeader supplier1 = Factory.New<OrgHeader>();
			OrgHeader supplier2 = Factory.New<OrgHeader>();

			ShipmentForTest shipment = Factory.New<ShipmentForTest>();
			BuyerSupplierLinksHelper<ShipmentForTest> linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = buyer.PK;
			AssertEquals("precondition: should not default when there are no links", ZGuid.Empty, shipment.ConsignorPK);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			buyer.SupplierLinks.AddNew(supplier1);
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = buyer.PK;
			AssertEquals("should default when there is exactly one link", supplier1.PK, shipment.ConsignorPK);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			buyer.SupplierLinks.AddNew(supplier2);
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = buyer.PK;
			AssertEquals("should not default when there is more than 1 link as the gui will deal with it", ZGuid.Empty, shipment.ConsignorPK);
		}

		public void TestSupplierDoesNotDefaultFromBuyerWhenRegistryIsOff()
		{
			Env.Registry.UseBuyerSupplierRelationships = false;
			OrgHeader buyer = Factory.New<OrgHeader>();
			OrgHeader supplier1 = Factory.New<OrgHeader>();

			ShipmentForTest shipment = Factory.New<ShipmentForTest>();
			BuyerSupplierLinksHelper<ShipmentForTest> linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			buyer.SupplierLinks.AddNew(supplier1);
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = buyer.PK;
			AssertEquals("should not default when the registry is off even if there is exactly one link", ZGuid.Empty, shipment.ConsignorPK);
		}

		public void TestSupplierDefaultedFromBuyer_OverrideSupplier()
		{
			OrgHeader buyer = Factory.New<OrgHeader>();
			OrgHeader supplier1 = Factory.New<OrgHeader>();
			buyer.SupplierLinks.AddNew(supplier1);

			ShipmentForTest shipment = Factory.New<ShipmentForTest>();
			BuyerSupplierLinksHelper<ShipmentForTest> linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_Address1 = "Override";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = buyer.PK;
			AssertNotEquals("should NOT default when there is an overridden consignor address", supplier1.PK, shipment.ConsignorPK);
		}

		public void TestRestoreOverrideNotifyPartyAddressWhenConsigneeChanges()
		{
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_RL_NKClosestPort = "AUSYD";

			var supplier = Factory.New<OrgHeader>();
			var link = buyer.SupplierLinks.AddNew(supplier);
			link.OL_RN_NKImporterCountry = "AU";

			var notifyParty = Factory.New<OrgHeader>();
			var mode = link.OrgSupBuyLinkTrnModes.AddNew();
			mode.PF_TransportMode = Constants.TransportModes.Air;
			mode.PF_ContainerMode = Constants.ContainerModes.Loose;
			mode.PF_OA_OverrideNotifyPartyAddress = notifyParty.MainAddress.PK;

			var shipment = Factory.New<ShipmentForTest>();
			var linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			AssertEquals(ZGuid.Empty, shipment.NotifyPartyDocumentaryAddress.E2_OA_Address);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = buyer.PK;
			AssertEquals(notifyParty.MainAddress.PK, shipment.NotifyPartyDocumentaryAddress.E2_OA_Address);
		}

		public void TestRestoreOverrideNotifyPartyAddressWhenConsignorChanges()
		{
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_RL_NKClosestPort = "AUSYD";

			var supplier = Factory.New<OrgHeader>();
			var link = buyer.SupplierLinks.AddNew(supplier);
			link.OL_RN_NKImporterCountry = "AU";

			var notifyParty = Factory.New<OrgHeader>();
			var mode = link.OrgSupBuyLinkTrnModes.AddNew();
			mode.PF_TransportMode = Constants.TransportModes.Air;
			mode.PF_ContainerMode = Constants.ContainerModes.Loose;
			mode.PF_OA_OverrideNotifyPartyAddress = notifyParty.MainAddress.PK;

			var shipment = Factory.New<ShipmentForTest>();
			var linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			AssertEquals(ZGuid.Empty, shipment.NotifyPartyDocumentaryAddress.E2_OA_Address);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;
			AssertEquals(notifyParty.MainAddress.PK, shipment.NotifyPartyDocumentaryAddress.E2_OA_Address);
		}

		#region TransportModeRestore

		[NUnit.Framework.ExpectNoExceptions()]
		public void TestRestoresTransportModeFor_NoLink()
		{
			OrgHeader buyer;
			OrgHeader supplier;
			ShipmentForTest bO;

			SetupShipmentWithLink(out buyer, out supplier, out bO, "SEA");

			bO.ConsigneeDocumentaryAddress.OrganisationPK = buyer.PK;
			bO.ConsignorDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
		}

		public void TestRestoresTransportModeFor_MultipleLink()
		{
			OrgHeader buyer1;
			OrgHeader buyer2;
			OrgHeader supplier1;

			ShipmentForTest bO;

			SetupShipmentWithLink(out buyer1, out supplier1, out bO, "SEA");

			buyer2 = CreateNewOrg(Factory, "buy");
			buyer2.OH_RL_NKClosestPort = "AUSYD";

			SetupBuyerLinkForSupplier(buyer2, supplier1, "AIR", true);

			bO.ConsigneeDocumentaryAddress.OrganisationPK = buyer2.PK;
			bO.ConsignorDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			bO.ConsignorDocumentaryAddress.OrganisationPK = supplier1.PK;

			AssertEquals("Should not change transport mode with multiple links", ZString.Empty, bO.TransportMode);
		}

		public void TestRestoresTransportModeFor_ExistingValueBlank()
		{
			OrgHeader buyer;
			OrgHeader supplier;
			ShipmentForTest bO;

			SetupShipmentWithLink(out buyer, out supplier, out bO, "SEA");

			bO.ConsigneeDocumentaryAddress.OrganisationPK = buyer.PK;
			bO.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;

			AssertEquals("Should set transport mode on shipment", "SEA", bO.JS_TransportMode);
		}

		public void TestRestoresTransportModeFor_ExistingValueSet()
		{
			OrgHeader buyer;
			OrgHeader supplier;
			ShipmentForTest bO;

			SetupShipmentWithLink(out buyer, out supplier, out bO, "AIR");

			((IBuyerSupplierRelationshipConsumer)bO).TransportMode = "SEA";

			bO.ConsigneeDocumentaryAddress.OrganisationPK = buyer.PK;
			bO.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;

			AssertEquals("Should not set transport mode on shipment", "SEA", bO.JS_TransportMode);
		}

		public void TestRestoresTransportModeFor_ALL()
		{
			OrgHeader buyer;
			OrgHeader supplier;
			ShipmentForTest bO;

			SetupShipmentWithLink(out buyer, out supplier, out bO, "ALL");

			bO.ConsigneeDocumentaryAddress.OrganisationPK = buyer.PK;
			bO.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;

			AssertEquals("Should not set transport mode on CommonShipment to ALL", "", bO.JS_TransportMode);
		}

		public void TestRestoresTransportModeFor_ShouldDefaultContainerModeAndIsContainerisedIsFalse()
		{
			OrgHeader buyer;
			OrgHeader supplier;
			ShipmentForTest shipment;

			SetupShipmentWithLink(out buyer, out supplier, out shipment,"AIR", "FCL");

			shipment.JS_PackingMode = "";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = buyer.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;

			AssertEquals("Should set transport mode on shipment", "AIR", shipment.JS_TransportMode);
			AssertEquals("Should not set packing mode on shipment", "", shipment.JS_PackingMode);
		}

		public void TestRestoresTransportModeFor_ShouldDefaultContainerModeAndIsContainerisedIsTrue()
		{
			OrgHeader buyer;
			OrgHeader supplier;
			ShipmentForTest shipment;

			SetupShipmentWithLink(out buyer, out supplier, out shipment, "AIR", "LCL");

			shipment.JS_PackingMode = "";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = buyer.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;

			AssertEquals("Should set transport mode on shipment", "SEA", shipment.JS_TransportMode);
			AssertEquals("Should set packing mode on shipment", "LCL", shipment.JS_PackingMode);
		}

		public void TestTransportModeRestorationSuspender()
		{
			OrgHeader buyer;
			OrgHeader supplier;
			ShipmentForTest bO;

			var link = SetupShipmentWithLink(out buyer, out supplier, out bO, "SEA");
			bO.ConsigneeDocumentaryAddress.OrganisationPK = buyer.PK;
			bO.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;

			AssertEquals("Should set transport mode on shipment", "SEA", bO.JS_TransportMode);
			using (link.SuspendTransportModeRestoration())
			{
				bO.JS_TransportMode = ZString.Empty;
			}
			AssertEquals("No change due to suspender", ZString.Empty, bO.JS_TransportMode);
		}

		#endregion

		#region TurningRestoringOFFForOrgsOnWeb

		public void TestTurningRestoringOffForOrgsOnWeb()
		{
			bool originalGlobalsIsWeb = Globals.IsWeb;

			try
			{
				ShipmentForTest shipment = Factory.New<ShipmentForTest>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

				OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
				OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
				consignee.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				IBuyerSupplierRelationshipConsumer bSRConsumer = shipment;
				bSRConsumer.Consignor = consignor;
				bSRConsumer.Consignee = consignee;

				OrgSupplierBuyerLink link = consignee.SupplierLinks.AddNew(consignor);

				SetOfOrgsForTest defaultOrgs = new SetOfOrgsForTest(Factory);
				SetOfOrgsForTest otherOrgs = new SetOfOrgsForTest(Factory);

				OrgSupBuyLinkTrnMode linkDetails = link.OrgSupBuyLinkTrnModes[0];
				linkDetails.PF_OH_SendingAgent = defaultOrgs.SendingAgent.PK;
				linkDetails.PF_OH_ReceivingAgent = defaultOrgs.ReceivingAgent.PK;
				linkDetails.PF_OH_CarrierLine = defaultOrgs.Carrier.PK;
				linkDetails.PF_OH_ImportCustomsAgent = defaultOrgs.ImportBroker.PK;
				linkDetails.PF_OH_PickupCartageContractor = defaultOrgs.PickupCartageContractor.PK;
				linkDetails.PF_OH_DeliveryCartageContractor = defaultOrgs.DeliveryCartageContractor.PK;

				BuyerSupplierLinksHelper<ShipmentForTest> linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
				linksHelper.Register();

				SetupConsumerToMakeOrgsDifferFromDefaults(bSRConsumer, otherOrgs);
				Globals.IsWeb = false;
				shipment.IsInDatabaseSetter = false;
				shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
				AssertOrganisationsAreSetCorrectly("Org is defaulted for Enterprise application, while creating", bSRConsumer, defaultOrgs);

				SetupConsumerToMakeOrgsDifferFromDefaults(bSRConsumer, otherOrgs);
				Globals.IsWeb = false;
				shipment.IsInDatabaseSetter = true;
				shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
				AssertOrganisationsAreSetCorrectly("Org is defaulted for Enterprise application while editing", bSRConsumer, defaultOrgs);

				SetupConsumerToMakeOrgsDifferFromDefaults(bSRConsumer, otherOrgs);
				Globals.IsWeb = true;
				shipment.IsInDatabaseSetter = false;
				shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
				AssertOrganisationsAreSetCorrectly("Org is defaulted for Web application while creating", bSRConsumer, defaultOrgs);

				SetupConsumerToMakeOrgsDifferFromDefaults(bSRConsumer, otherOrgs);
				Globals.IsWeb = true;
				shipment.IsInDatabaseSetter = true;
				shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
				AssertOrganisationsAreSetCorrectly("Org is preserved for Web application while editing", bSRConsumer, otherOrgs);
			}
			finally
			{
				Globals.IsWeb = originalGlobalsIsWeb;
			}
		}

		void SetupConsumerToMakeOrgsDifferFromDefaults(IBuyerSupplierRelationshipConsumer bSRConsumer, SetOfOrgsForTest setOfOrgs)
		{
			bSRConsumer.SendingAgentPK = setOfOrgs.SendingAgent.PK;
			bSRConsumer.ReceivingAgentPK = setOfOrgs.ReceivingAgent.PK;
			bSRConsumer.ShippingLinePK = setOfOrgs.Carrier.PK;
			bSRConsumer.ImportBrokerPK = setOfOrgs.ImportBroker.PK;
			bSRConsumer.PickupCartageCoPK = setOfOrgs.PickupCartageContractor.PK;
			bSRConsumer.DeliveryCartageCoPK = setOfOrgs.DeliveryCartageContractor.PK;
		}

		void AssertOrganisationsAreSetCorrectly(string message, IBuyerSupplierRelationshipConsumer bSRConsumer, SetOfOrgsForTest setOfOrgs)
		{
			AssertEquals("[Sending Agent] " + message, bSRConsumer.SendingAgentPK, setOfOrgs.SendingAgent.PK);
			AssertEquals("[ReceivingAgent] " + message, bSRConsumer.ReceivingAgentPK, setOfOrgs.ReceivingAgent.PK);
			AssertEquals("[Carrier] " + message, bSRConsumer.ShippingLinePK, setOfOrgs.Carrier.PK);
			AssertEquals("[ImportBroker]" + message, bSRConsumer.ImportBrokerPK, setOfOrgs.ImportBroker.PK);
			AssertEquals("[PickupCartageContractor] " + message, bSRConsumer.PickupCartageCoPK, setOfOrgs.PickupCartageContractor.PK);
			AssertEquals("[DeliveryCartageContractor] " + message, bSRConsumer.DeliveryCartageCoPK, setOfOrgs.DeliveryCartageContractor.PK);
		}

		#endregion

		public void TestRestorePaymentTerms()
		{
			OrgHeader buyer1;
			OrgHeader supplier;
			ShipmentForTest bO;

			SetupShipmentWithLink(out buyer1, out supplier, out bO, "ALL");
			var bsrConsumer = (IBuyerSupplierRelationshipConsumer)bO;
			AssertEquals("precondition: should be blank", ZString.Empty, bsrConsumer.PaymentTerms);

			bO.ConsigneeDocumentaryAddress.OrganisationPK = buyer1.PK;
			bO.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;
			AssertEquals("Should have value", "FOB", bsrConsumer.PaymentTerms);

			bO.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			bO.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			bsrConsumer.PaymentTerms = ZString.Empty;
			buyer1.SupplierLinks[0].OrgSupBuyLinkTrnModes[0].PF_IncoTerm = "CPT";
			bO.ConsigneeDocumentaryAddress.OrganisationPK = buyer1.PK;
			bO.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;
			AssertEquals("Should have value", "CPT", bsrConsumer.PaymentTerms);
		}

		public void TestShouldPromptToSaveSupplierBuyerRelationship()
		{
			ShipmentForTest shipment = Factory.New<ShipmentForTest>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			BuyerSupplierLinksHelper<ShipmentForTest> linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			Assert(linksHelper.ShouldPromptToSaveSupplierBuyerRelationship);

			Factory.Save();
			Assert(!linksHelper.ShouldPromptToSaveSupplierBuyerRelationship);

			consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			Assert(linksHelper.ShouldPromptToSaveSupplierBuyerRelationship);

			Factory.Save();
			shipment.ConsignorDocumentaryAddress.HasChanges = true;
			Assert(!linksHelper.ShouldPromptToSaveSupplierBuyerRelationship);
		}

		public void TestGetImportBrokerFromBuyerSupplierLink()
		{
			var shipment = Factory.New<ShipmentForTest>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			IBuyerSupplierRelationshipConsumer bSRConsumer = shipment;
			bSRConsumer.Consignor = consignor;
			bSRConsumer.Consignee = consignee;

			var link = consignee.SupplierLinks.AddNew(consignor);
			var defaultOrgs = new SetOfOrgsForTest(Factory);

			var linkDetails = link.OrgSupBuyLinkTrnModes[0];
			linkDetails.PF_OH_ImportCustomsAgent = defaultOrgs.ImportBroker.PK;

			var linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			AssertEquals(defaultOrgs.ImportBroker.PK, linkDetails.PF_OH_ImportCustomsAgent);
			AssertEquals(ZGuid.Empty, link.OL_OH_ImportBroker);
			AssertEquals("from SupplierBuyerLinkDetails.PF_OH_ImportCustomsAgent", linkDetails.PF_OH_ImportCustomsAgent, linksHelper.GetImportBrokerFromBuyerSupplierLink());

			linkDetails.PF_OH_ImportCustomsAgent = ZGuid.Empty;
			link.OL_OH_ImportBroker = defaultOrgs.ImportBroker.PK;

			AssertEquals("from SupplierBuyerLink.OL_OH_ImportBroker", link.OL_OH_ImportBroker, linksHelper.GetImportBrokerFromBuyerSupplierLink());

			link.OL_OH_ImportBroker = ZGuid.Empty;

			AssertNull("default as null", linksHelper.GetImportBrokerFromBuyerSupplierLink());
		}

		public void TestShouldPromptToSaveSupplierBuyerRelationship_SystemDefinedOrganisationsAreIgnored()
		{
			var systemDefinedOrganisation = Factory.Load<OrgHeader>(OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation);
			Assert("Precondition", systemDefinedOrganisation.IsSystemDefinedOrganisation);

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.New<ShipmentForTest>();
			var linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = systemDefinedOrganisation.PK;
			AssertEquals(false, linksHelper.ShouldPromptToSaveSupplierBuyerRelationship);

			shipment.ConsignorPK = systemDefinedOrganisation.PK;
			shipment.ConsigneePK = consignee.PK;
			AssertEquals(false, linksHelper.ShouldPromptToSaveSupplierBuyerRelationship);

			shipment.ConsignorPK = ZGuid.Empty;
			shipment.ConsigneePK = ZGuid.Empty;
			AssertEquals(false, linksHelper.ShouldPromptToSaveSupplierBuyerRelationship);

			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			AssertEquals(true, linksHelper.ShouldPromptToSaveSupplierBuyerRelationship);
		}

		public void TestAddNewBuyerSupplierLink_NullsAndSystemDefinedOrganisationsAreIgnored()
		{
			var systemDefinedOrganisation = Factory.Load<OrgHeader>(OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation);
			Assert("Precondition", systemDefinedOrganisation.IsSystemDefinedOrganisation);

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.New<ShipmentForTest>();
			var linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			shipment.ConsignorPK = systemDefinedOrganisation.PK;
			shipment.ConsigneePK = consignee.PK;

			linksHelper.AddNewBuyerSupplierLink();
			AssertEquals(0, consignee.SupplierLinks.Count);

			linksHelper.AddNewBuyerSupplierLinkGivenOrgs(systemDefinedOrganisation, consignee);
			AssertEquals(0, consignee.SupplierLinks.Count);

			shipment.ConsignorPK = ZGuid.Empty;
			shipment.ConsigneePK = consignee.PK;

			linksHelper.AddNewBuyerSupplierLink();
			AssertEquals(0, consignee.SupplierLinks.Count);

			linksHelper.AddNewBuyerSupplierLinkGivenOrgs(null, consignee);
			AssertEquals(0, consignee.SupplierLinks.Count);

			AssertNoExceptionThrown(() =>
				{
					shipment.ConsignorPK = ZGuid.Empty;
					shipment.ConsigneePK = ZGuid.Empty;

					linksHelper.AddNewBuyerSupplierLink();
					linksHelper.AddNewBuyerSupplierLinkGivenOrgs(null, null);
				});
		}

		public void TestRestoreNumberOfBills()
		{
			ShipmentForTest shipment;
			OrgHeader supplier;
			OrgHeader buyer;
			SetupShipmentWithLink(out buyer, out supplier, out shipment, Core.Constants.TransportModes.Sea);
			((ISupportDataImporting)shipment).IsImportingData = false;

			shipment.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;

			var mode = supplier.BuyerLinks[0].OrgSupBuyLinkTrnModes[0];

			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.ExpressBofL;
			shipment.JS_NoOriginalBills = 10;
			shipment.JS_NoCopyBills = 11;
			shipment.NoOriginalBillsForFallback = 12;
			shipment.NoCopyBillsForFallback = 13;
			mode.PF_NoOfOriginalBills = 0;
			mode.PF_NoOfCopyBills = 0;

			LinksHelper.RestoreNumberOfBills();
			AssertEquals("Restore BOL from FallBack for Release Type EBL", (ZByte)12, shipment.JS_NoOriginalBills);
			AssertEquals("Restore BOL from FallBack for Release Type EBL", (ZByte)13, shipment.JS_NoCopyBills);

			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.BankLetterOfCredit;
			shipment.JS_NoOriginalBills = 10;
			shipment.JS_NoCopyBills = 11;
			LinksHelper.RestoreNumberOfBills();
			AssertEquals("Restore BOL from FallBack for link values equal to 0", (ZByte)12, shipment.JS_NoOriginalBills);
			AssertEquals("Restore BOL from FallBack for link values equal to 0", (ZByte)13, shipment.JS_NoCopyBills);

			mode.PF_NoOfOriginalBills = 2;
			mode.PF_NoOfCopyBills = 0;
			shipment.JS_NoOriginalBills = 10;
			shipment.JS_NoCopyBills = 11;
			LinksHelper.RestoreNumberOfBills();
			AssertEquals("Restore BOL from link", (ZByte)2, shipment.JS_NoOriginalBills);
			AssertEquals("Restore BOL from link", (ZByte)0, shipment.JS_NoCopyBills);
		}

		public void TestControllingCustomer()
		{
			OrgHeader consignee;
			OrgHeader consignor;
			ShipmentForTest shipment;
			var controllingCustomerAll = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomerAirLSE = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomerAirUTL = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomerSea = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomerAir = Factory.NewWithValidTestData<OrgHeader>();

			SetupShipmentWithLink(out consignee, out consignor, out shipment, "AIR");

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_PackingMode = "LSE";
			var supplierBuyerLink = consignee.SupplierLinks[0];
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode = "ALL";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = "";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_OH_ControllingCustomer = controllingCustomerAll.PK;

			var linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			AssertEquals(controllingCustomerAll.PK, linksHelper.GetControllingCustomer().PK);
			shipment.JS_TransportMode = "AIR";
			supplierBuyerLink.OrgSupBuyLinkTrnModes.AddNew();
			supplierBuyerLink.OrgSupBuyLinkTrnModes[1].PF_TransportMode = "AIR";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[1].PF_ContainerMode = "";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[1].PF_OH_ControllingCustomer = controllingCustomerAir.PK;

			supplierBuyerLink.OrgSupBuyLinkTrnModes.AddNew();
			supplierBuyerLink.OrgSupBuyLinkTrnModes[2].PF_TransportMode = "AIR";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[2].PF_ContainerMode = "UTL";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[2].PF_OH_ControllingCustomer = controllingCustomerAirUTL.PK;

			supplierBuyerLink.OrgSupBuyLinkTrnModes.AddNew();
			supplierBuyerLink.OrgSupBuyLinkTrnModes[3].PF_TransportMode = "SEA";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[3].PF_ContainerMode = "FCL";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[3].PF_OH_ControllingCustomer = controllingCustomerSea.PK;

			AssertEquals(controllingCustomerAir.PK, linksHelper.GetControllingCustomer().PK);

			supplierBuyerLink.OrgSupBuyLinkTrnModes.AddNew();
			supplierBuyerLink.OrgSupBuyLinkTrnModes[4].PF_TransportMode = "AIR";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[4].PF_ContainerMode = "LSE";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[4].PF_OH_ControllingCustomer = controllingCustomerAirLSE.PK;

			AssertEquals(controllingCustomerAirLSE.PK, linksHelper.GetControllingCustomer().PK);
		}

		OrgSupBuyLinkTrnMode AddControllerCustomerTransportContainerOverride(OrgSupBuyLinkTrnMode linkTransportOverride, ZString transportMode, ZString containerMode, ZGuid pK)
		{
			linkTransportOverride.PF_TransportMode = transportMode;
			linkTransportOverride.PF_ContainerMode = containerMode;
			linkTransportOverride.PF_OH_ControllingCustomer = pK;
			return linkTransportOverride;
		}

		#region ControllingCustomerOverrides

		public void TestControllingCustomer_SameTransportDifferentContainerOverrideIsNotSelected()
		{
			OrgHeader consignee;
			OrgHeader consignor;
			ShipmentForTest shipment;
			SetupShipmentWithLink(out consignee, out consignor, out shipment, "SEA");
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_PackingMode = "LSE";
			var linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			var linkControllingCustomerOrg = Factory.NewWithValidTestData<OrgHeader>();
			var linkControllingCustomerBlkOverrideOrg = Factory.NewWithValidTestData<OrgHeader>();
			var linkControllingCustomerLseOverrideOrg = Factory.NewWithValidTestData<OrgHeader>();

			var supplierBuyerLink = consignee.SupplierLinks[0];
			supplierBuyerLink.OL_OH_ControllingCustomer = linkControllingCustomerOrg.PK;

			var firstTransportModeOverride = supplierBuyerLink.OrgSupBuyLinkTrnModes[0];
			AddControllerCustomerTransportContainerOverride(firstTransportModeOverride, "SEA", "BLK", linkControllingCustomerBlkOverrideOrg.PK);
			AssertEquals("the SEA/BLK override doesn't match because the shipment uses SEA/LSE", linkControllingCustomerOrg.PK, linksHelper.GetControllingCustomer().PK);
			AddControllerCustomerTransportContainerOverride(supplierBuyerLink.OrgSupBuyLinkTrnModes.AddNew(), "SEA", "LSE", linkControllingCustomerLseOverrideOrg.PK);
			AssertEquals("the SEA/LSE override matches the shipments SEA/LSE configuration", linkControllingCustomerLseOverrideOrg.PK, linksHelper.GetControllingCustomer().PK);
		}

		public void TestControllingCustomer_AllTransportDifferentContainerOverrideIsNotSelected()
		{
			OrgHeader consignee;
			OrgHeader consignor;
			ShipmentForTest shipment;
			SetupShipmentWithLink(out consignee, out consignor, out shipment, "SEA");
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_PackingMode = "LSE";
			var linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			var linkControllingCustomerOrg = Factory.NewWithValidTestData<OrgHeader>();
			var linkControllingCustomerBlkOverrideOrg = Factory.NewWithValidTestData<OrgHeader>();
			var linkControllingCustomerLseOverrideOrg = Factory.NewWithValidTestData<OrgHeader>();

			var supplierBuyerLink = consignee.SupplierLinks[0];
			supplierBuyerLink.OL_OH_ControllingCustomer = linkControllingCustomerOrg.PK;

			var firstTransportModeOverride = supplierBuyerLink.OrgSupBuyLinkTrnModes[0];
			AddControllerCustomerTransportContainerOverride(firstTransportModeOverride, "ALL", "BLK", linkControllingCustomerBlkOverrideOrg.PK);
			AssertEquals("the ALL/BLK override doesn't match because the shipment uses SEA/LSE", linkControllingCustomerOrg.PK, linksHelper.GetControllingCustomer().PK);
			AddControllerCustomerTransportContainerOverride(supplierBuyerLink.OrgSupBuyLinkTrnModes.AddNew(), "ALL", "LSE", linkControllingCustomerLseOverrideOrg.PK);
			AssertEquals("the ALL/LSE override matches the shipments SEA/LSE configuration", linkControllingCustomerLseOverrideOrg.PK, linksHelper.GetControllingCustomer().PK);
		}

		public void TestControllingCustomer_AllTransportEmptyContainerOverrideIsSelected()
		{
			OrgHeader consignee;
			OrgHeader consignor;
			ShipmentForTest shipment;
			SetupShipmentWithLink(out consignee, out consignor, out shipment, "SEA");
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_PackingMode = "LSE";
			var linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			var linkControllingCustomerOrg = Factory.NewWithValidTestData<OrgHeader>();
			var linkControllingCustomerAllOverrideOrg = Factory.NewWithValidTestData<OrgHeader>();

			var supplierBuyerLink = consignee.SupplierLinks[0];
			supplierBuyerLink.OL_OH_ControllingCustomer = linkControllingCustomerOrg.PK;

			var firstTransportModeOverride = supplierBuyerLink.OrgSupBuyLinkTrnModes[0];
			AddControllerCustomerTransportContainerOverride(firstTransportModeOverride, "ALL", "", linkControllingCustomerAllOverrideOrg.PK);
			AssertEquals("the ALL/empty override matches SEA/LSE", linkControllingCustomerAllOverrideOrg.PK, linksHelper.GetControllingCustomer().PK);
		}

		#endregion

		public void TestControllingCustomer_UseSupplierBuyerLinkControllingCustomer()
		{
			OrgHeader consignee;
			OrgHeader consignor;
			ShipmentForTest shipment;
			var controllingCustomerAll = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomerAirUTL = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomerSea = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomerLink = Factory.NewWithValidTestData<OrgHeader>();

			SetupShipmentWithLink(out consignee, out consignor, out shipment, "AIR");

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_PackingMode = "LSE";

			var supplierBuyerLink = consignee.SupplierLinks[0];
			supplierBuyerLink.OL_OH_ControllingCustomer = controllingCustomerLink.PK;
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode = "ALL";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = "";

			var linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			AssertEquals("no overrides match", controllingCustomerLink.PK, linksHelper.GetControllingCustomer().PK);

			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_OH_ControllingCustomer = controllingCustomerAll.PK;
			AssertEquals("the ALL/empty override matches", controllingCustomerAll.PK, linksHelper.GetControllingCustomer().PK);

			supplierBuyerLink.OrgSupBuyLinkTrnModes.AddNew();
			supplierBuyerLink.OrgSupBuyLinkTrnModes[1].PF_TransportMode = "AIR";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[1].PF_ContainerMode = "LSE";
			AssertEquals(Guid.Empty, supplierBuyerLink.OrgSupBuyLinkTrnModes[1].PF_OH_ControllingCustomer);

			supplierBuyerLink.OrgSupBuyLinkTrnModes.AddNew();
			supplierBuyerLink.OrgSupBuyLinkTrnModes[2].PF_TransportMode = "AIR";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[2].PF_ContainerMode = "UTL";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[2].PF_OH_ControllingCustomer = controllingCustomerAirUTL.PK;

			supplierBuyerLink.OrgSupBuyLinkTrnModes.AddNew();
			supplierBuyerLink.OrgSupBuyLinkTrnModes[3].PF_TransportMode = "SEA";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[3].PF_ContainerMode = "FCL";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[3].PF_OH_ControllingCustomer = controllingCustomerSea.PK;

			AssertEquals("AIR/UTL, AIR/LSE and SEA/FCL do not match the shipments AIR/LSE, so AIR/empty is the best match", controllingCustomerAll.PK, linksHelper.GetControllingCustomer().PK);
		}

		public void TestControllingCustomer_ShouldNotTouchOrgHeaderTable()
		{
			var creationFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var consignee = creationFactory.NewWithValidTestData<OrgHeader>();
			var consignor = creationFactory.NewWithValidTestData<OrgHeader>();

			consignee.SupplierLinks.AddNew().OL_OH_Supplier = creationFactory.NewWithValidTestData<OrgHeader>().PK;
			consignee.SupplierLinks.AddNew().OL_OH_Supplier = creationFactory.NewWithValidTestData<OrgHeader>().PK;

			creationFactory.Save();

			ShipmentForTest shipment = Factory.New<ShipmentForTest>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;

			var linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			int orgHeaderHitCountBefore = Factory.GetTableHitCount(OrgHeaderSchema.Constants.TableName);

			var loaded = linksHelper.GetControllingCustomer();
			AssertEquals("OrgHeader table was not touched", orgHeaderHitCountBefore, Factory.GetTableHitCount(OrgHeaderSchema.Constants.TableName));
		}

		public void TestRestoringHandlingInformation_RestoringHandlingInformationIsNotAllowed_DoNotCreateNote()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.MiscServ.OM_EXHandlingInstuctions = "MCLAREN";

			var shipment = Factory.New<ShipmentForTest>();
			shipment.ShouldRestoreHandlingInformation = false;

			var linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			// To force link updating
			shipment.ConsignorPK = consignor.PK;

			var notes = shipment.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
			AssertEquals("Notes count", 0, notes.Length);
		}

		public void TestRestoringHandlingInformation_RestoringHandlingInformationIsAllowed_CreateNote()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.MiscServ.OM_EXHandlingInstuctions = "MCLAREN";

			var shipment = Factory.New<ShipmentForTest>();
			shipment.ShouldRestoreHandlingInformation = true;

			var linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);
			linksHelper.Register();

			// To force link updating
			shipment.ConsignorPK = consignor.PK;

			var notes = shipment.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
			AssertEquals("Notes count", 1, notes.Length);
			AssertEquals("Notes count", "MCLAREN", notes[0].ST_NoteDataAsText);
		}

		#region Implementation

		class ShipmentForTest : CommonShipment, IBuyerSupplierRelationshipConsumer
		{
			public ShipmentForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public static new ShipmentForTest New(BusinessObjectFactory factory)
			{
				return factory.New<ShipmentForTest>();
			}

			#region IBuyerSupplierRelationshipConsumer Members

			event EventHandler IBuyerSupplierRelationshipConsumer.ModesChanged
			{
				add
				{
					JS_TransportModeInfo.ValueChanged += value;
					JS_PackingModeInfo.ValueChanged += value;
				}
				remove
				{
					JS_TransportModeInfo.ValueChanged -= value;
					JS_PackingModeInfo.ValueChanged -= value;
				}
			}

			void IBuyerSupplierRelationshipConsumer.RestoreSendingAgentFallback()
			{
			}

			void IBuyerSupplierRelationshipConsumer.RestoreReceivingAgentFallback()
			{
			}

			event EventHandler IBuyerSupplierRelationshipConsumer.ConsigneeChanged
			{
				add { ConsigneeDocumentaryAddress.E2_OA_AddressInfo.ValueChanged += value; }
				remove { ConsigneeDocumentaryAddress.E2_OA_AddressInfo.ValueChanged -= value; }
			}

			event EventHandler IBuyerSupplierRelationshipConsumer.ConsignorChanged
			{
				add { ConsignorDocumentaryAddress.E2_OA_AddressInfo.ValueChanged += value; }
				remove { ConsignorDocumentaryAddress.E2_OA_AddressInfo.ValueChanged -= value; }
			}

			ZString IBuyerSupplierRelationshipConsumer.Origin
			{
				get { return JS_RL_NKOrigin; }
				set { JS_RL_NKOrigin = value; }
			}

			ZString IBuyerSupplierRelationshipConsumer.Destination
			{
				get { return JS_RL_NKDestination; }
				set { JS_RL_NKDestination = value; }
			}

			ZString IBuyerSupplierRelationshipConsumer.LoadPort
			{
				get { return ""; }
				set { }
			}

			ZString IBuyerSupplierRelationshipConsumer.DischargePort
			{
				get { return ""; }
				set { }
			}

			ZString IBuyerSupplierRelationshipConsumer.ContainerMode
			{
				get { return JS_PackingMode; }
				set { JS_PackingMode = value; }
			}

			ZString IBuyerSupplierRelationshipConsumer.TransportMode
			{
				get { return JS_TransportMode; }
				set { JS_TransportMode = value; }
			}

			ZString IBuyerSupplierRelationshipConsumer.ServiceLevel
			{
				get { return JS_RS_NKServiceLevel; }
				set { JS_RS_NKServiceLevel = value; }
			}

			void IBuyerSupplierRelationshipConsumer.RestoreServiceLevelFallback()
			{
				ServiceLevelFallBackSet = true;
			}

			public bool ServiceLevelFallBackSet;

			ZGuid IBuyerSupplierRelationshipConsumer.PickupCartageCoPK
			{
				get { return IsDeleted ? ZGuid.Empty : DocsAndCartage.PickupCartageCoPK; }
				set
				{
					if (!IsDeleted)
					{
						DocsAndCartage.PickupCartageCoPK = value;
					}
				}
			}

			ZGuid IBuyerSupplierRelationshipConsumer.SendingAgentPK { get; set; }

			ZGuid IBuyerSupplierRelationshipConsumer.ReceivingAgentPK { get; set; }

			ZGuid IBuyerSupplierRelationshipConsumer.ShippingLinePK
			{
				get
				{
					ZGuid result = ZGuid.Empty;

					CommonConsol consol = Consols.Count == 1 ? Consols[0] : FindCorrectConsol();
					if (consol != null && consol.ShippingLinePK.IsValid)
					{
						result = consol.ShippingLinePK;
					}

					return result;
				}
				set
				{
					CommonConsol consol = Consols.Count == 1 ? Consols[0] : Consols.AddNew();
					OrgHeader line = Factory.Load<OrgHeader>(value);
					consol.JK_OA_ShippingLineAddress = line != null ? line.MainAddress.PK : ZGuid.Empty;
				}
			}

			ZGuid IBuyerSupplierRelationshipConsumer.DeliveryCartageCoPK
			{
				get { return IsDeleted ? ZGuid.Empty : DocsAndCartage.DeliveryCartageCoPK; }
				set
				{
					if (!IsDeleted)
					{
						DocsAndCartage.DeliveryCartageCoPK = value;
					}
				}
			}

			ZGuid IBuyerSupplierRelationshipConsumer.ImportBrokerPK
			{
				get { return JS_OH_ImportBroker; }
				set { JS_OH_ImportBroker = value; }
			}

			ZString IBuyerSupplierRelationshipConsumer.GoodsCurrency
			{
				get { return JS_RX_NKGoodsValueCurr; }
				set { JS_RX_NKGoodsValueCurr = value; }
			}

			void IBuyerSupplierRelationshipConsumer.RestoreGoodsCurrencyFallback()
			{
			}

			ZString IBuyerSupplierRelationshipConsumer.GoodsDescription
			{
				get { return JS_GoodsDescription; }
				set { JS_GoodsDescription = value; }
			}

			ZBool IBuyerSupplierRelationshipConsumer.ShouldPromptToSaveBuyerSupplierRelationship
			{
				get { return !IsCoLoadMaster && !IsBlindCoLoadMaster; }
			}

			ZByte IBuyerSupplierRelationshipConsumer.NoCopyBills
			{
				get { return JS_NoCopyBills; }
				set { JS_NoCopyBills = value; }
			}

			ZByte IBuyerSupplierRelationshipConsumer.NoOriginalBills
			{
				get { return JS_NoOriginalBills; }
				set { JS_NoOriginalBills = value; }
			}

			void IBuyerSupplierRelationshipConsumer.RestoreNumberOfBillsWithFallback()
			{
				((IBuyerSupplierRelationshipConsumer)this).NoOriginalBills = this.NoOriginalBillsForFallback;
				((IBuyerSupplierRelationshipConsumer)this).NoCopyBills = this.NoCopyBillsForFallback;
			}

			void IBuyerSupplierRelationshipConsumer.RestoreEFreightStatusFallback(ZString defaultStatus)
			{
			}

			ZBool IBuyerSupplierRelationshipConsumer.PreventBuyerSupplierRelationships
			{
				get
				{
					return false;
				}
			}

			public void RestoreImportBrokerFallback()
			{
			}

			ZString IBuyerSupplierRelationshipConsumer.PaymentTerms
			{
				get { return JS_INCO; }
				set { JS_INCO = value; }
			}

			ZBool IBuyerSupplierRelationshipConsumer.ShouldRestorePaymentTerm
			{
				get { return !IsDomesticFreight; }
			}

			public ZBool ShouldRestoreHandlingInformation
			{
				get;
				set;
			}

			ZString IBuyerSupplierRelationshipConsumer.ReleaseType
			{
				get { return JS_ReleaseType; }
			}

			OrgHeader IBuyerSupplierRelationshipConsumer.Consignee
			{
				get { return Consignee; }
				set
				{
					if (!ConsigneeDocumentaryAddress.E2_AddressOverride)
					{
						ConsigneeDocumentaryAddress.OrganisationPK = value.PK;
					}
				}
			}

			OrgHeader IBuyerSupplierRelationshipConsumer.Consignor
			{
				get { return Consignor; }
				set
				{
					if (!ConsignorDocumentaryAddress.E2_AddressOverride)
					{
						ConsignorDocumentaryAddress.OrganisationPK = value.PK;
					}
				}
			}

			ZBool IBuyerSupplierRelationshipConsumer.ShouldDefaultContainerModeAndIsContainerised(ZString containerMode) => containerMode == "LCL";

			#endregion

			public ZByte NoOriginalBillsForFallback { get; set; }

			public ZByte NoCopyBillsForFallback { get; set; }

			#region IsInDatabase

			public bool IsInDatabaseSetter;
			public override bool IsInDatabase
			{
				get
				{
					return IsInDatabaseSetter;
				}
			}

			#endregion

			ZBool IBuyerSupplierRelationshipConsumer.ShouldRestorePickupDeliveryAndNotifyPartyAddress
			{
				get { return true; }
			}
		}

		BuyerSupplierLinksHelper<ShipmentForTest> LinksHelper;

		BuyerSupplierLinksHelper<ShipmentForTest> SetupShipmentWithLink(out OrgHeader buyer, out OrgHeader supplier, out ShipmentForTest bO, string linkTransportMode, string linkContainerMode = "FCL")
		{
			// set up the buyer/suppler and their link
			buyer = CreateNewOrg(Factory, "buy");
			supplier = CreateNewOrg(Factory, "supp");
			buyer.OH_RL_NKClosestPort = "AUSYD";
			supplier.OH_RL_NKClosestPort = "AUSYD";

			bO = SetupBuyerLinkForSupplier(buyer, supplier, linkTransportMode, false, linkContainerMode);
			// make sure the defaults come from the link appropriately
			((ISupportDataImporting)bO).IsImportingData = true;

			LinksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(bO);
			LinksHelper.Register();
			return LinksHelper;
		}

		ShipmentForTest SetupBuyerLinkForSupplier(OrgHeader buyer, OrgHeader supplier, string linkTransportMode, bool addNewTrnMode, string linkContainerMode = "FCL")
		{
			OrgSupplierBuyerLink link = supplier.BuyerLinks.AddNew();
			ShipmentForTest bO;

			link.OL_OH_Buyer = buyer.PK;

			bO = Factory.New<ShipmentForTest>();

			// set up some values for the defaults
			RefCurrency currency1 = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			RefCurrency currency2 = Factory.New<RefCurrency>();
			RefServiceLevel serviceLevel = Factory.New<RefServiceLevel>();//, Env.Registry.ServiceLevel);
			serviceLevel.RS_Code = "GGG";

			OrgHeader aIRCarrier = CreateNewOrg(Factory, "carr");
			OrgHeader fCLCarrier = CreateNewOrg(Factory, "carr");
			ZString transportMode = linkTransportMode;
			ZString containerMode = linkContainerMode;
			ZString inco = "FOB";

			supplier.MiscServ.OM_RX_NKEXDefCurrency = currency2.RX_Code;
			buyer.MiscServ.OM_RS_NKIMDefaultServiceLevel = serviceLevel.RS_Code;

			// set the defaults on the link and supplier/misc serv
			link.OL_RX_NKDefaultCurrency = currency1.RX_Code;
			if (!addNewTrnMode)
			{
				link.OrgSupBuyLinkTrnModes[0].PF_OH_CarrierLine = aIRCarrier.PK;
				link.OrgSupBuyLinkTrnModes[0].PF_TransportMode = transportMode;
				link.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = containerMode;
				link.OrgSupBuyLinkTrnModes[0].PF_IncoTerm = inco;
				link.OrgSupBuyLinkTrnModes[0].PF_RS_NKDefaultServiceLevel = serviceLevel.RS_Code;
				link.OrgSupBuyLinkTrnModes[0].PF_RL_NKDischargePort = "AUSYD";
				link.OL_RN_NKImporterCountry = "AU";
			}
			else
			{
				OrgSupBuyLinkTrnMode newMode = link.OrgSupBuyLinkTrnModes.AddNew();
				newMode.PF_OH_CarrierLine = aIRCarrier.PK;
				newMode.PF_TransportMode = transportMode;
				newMode.PF_ContainerMode = containerMode;
				newMode.PF_IncoTerm = inco;
				newMode.PF_RS_NKDefaultServiceLevel = serviceLevel.RS_Code;
				newMode.PF_RL_NKDischargePort = "AUSYD";
				link.OL_RN_NKImporterCountry = "AU";
			}
			return bO;
		}

		OrgHeader CreateNewOrg(BusinessObjectFactory factory, ZString code)
		{
			OrgHeader result = factory.New<OrgHeader>();
			result.OH_FullName = "some full name";
			result.OH_Code = code;
			result.MainAddress.OA_Address1 = "some place";
			return result;
		}

		class SetOfOrgsForTest
		{
			public SetOfOrgsForTest(BusinessObjectFactory factory)
			{
				SendingAgent = factory.NewWithValidTestData<OrgHeader>();
				ReceivingAgent = factory.NewWithValidTestData<OrgHeader>();
				Carrier = factory.NewWithValidTestData<OrgHeader>();
				ImportBroker = factory.NewWithValidTestData<OrgHeader>();
				PickupCartageContractor = factory.NewWithValidTestData<OrgHeader>();
				DeliveryCartageContractor = factory.NewWithValidTestData<OrgHeader>();
			}

			public readonly OrgHeader SendingAgent;
			public readonly OrgHeader ReceivingAgent;
			public readonly OrgHeader Carrier;
			public readonly OrgHeader ImportBroker;
			public readonly OrgHeader PickupCartageContractor;
			public readonly OrgHeader DeliveryCartageContractor;
		}

		#endregion
	}
}
