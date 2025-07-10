using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using InvoiceHeaderRefsTypeList = Enterprise.Customs.Business.InvoiceHeaderRefsTypeList;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	partial class StandaloneCommercialInvoiceDataObjectReaderTest : Customs.DataTransfer.Universal.Testing.DataObjectReaderTest
	{
		public void TestPopulateJZ_OA_SupplierAddressAndJZ_OA_ManufacturerAddressAccordingToAddressShortCode()
		{
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (USCustomsDataRegistry.Instance.DefaultManufacturerFromSupplier.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var buyer = CreateOrganisation("BUYER", "ABC!@#12");
				var supplier = CreateOrganisation("SUPPLIER", "ABC!@#11");
				var supplierMainAddress = supplier.MainAddress;
				supplierMainAddress.OA_Code = "TEST1";
				var supplierAddress2 = supplier.Addresses.AddNew();
				supplierAddress2.OA_Address1 = "Address 1";
				supplierAddress2.OA_Code = "TEST2";
				Factory.SaveForTesting();

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				var invoiceDataObject = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
				{
					InvoiceNumber = "INVABC123",
					InvoiceAmount = 150m
				};
				invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, buyer, AddressTypes.Importer);
				invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);
				invoiceDataObject.Supplier.AddressShortCode = "TEST2";
				var manufacturer = invoiceDataObject.AddOrgAddress(writeManager, supplier, nameof(DocAddressType.Manufacturer));
				manufacturer.AddressShortCode = "TEST2";

				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					Branch = new Branch() { Code = "B@#" },
					MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
					CommercialInfo = SetupCommercialInfo("Top Group", invoiceCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject }))
				};

				var invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, new UniversalObjectFactory()).ReadIntoBusinessObject();
				AssertEquals(supplierAddress2.PK, invoice.JZ_OA_SupplierAddress);
				AssertEquals(supplierAddress2.PK, invoice.JZ_OA_ManufacturerAddress);
			}
		}

		public void TestMatchingExistingInvoice()
		{
			var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			consignor.OH_IsConsignor = ZBool.True;
			var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var buyer = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			consignee.OH_IsConsignee = ZBool.True;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var topGroupInvoice = Factory.New<JobComInvoiceGroupHeader>();
			topGroupInvoice.JZ_OH_Supplier = consignor.PK;
			topGroupInvoice.JZ_OH_Buyer = consignee.PK;
			topGroupInvoice.JZ_InvoiceDate = new ZDateTime(2012, 11, 10);
			topGroupInvoice.JZ_JE = declaration.PK;
			var decInvoice = Factory.New<JobComInvoiceHeader>();
			decInvoice.JZ_OH_Supplier = consignor.PK;
			decInvoice.JZ_OH_Buyer = consignee.PK;
			decInvoice.JZ_InvoiceNumber = "INVABC123";
			decInvoice.JZ_InvoiceDate = new ZDateTime(2012, 11, 11);
			decInvoice.JZ_JE = declaration.PK;

			var standaloneInvoiceOld = Factory.New<JobComInvoiceHeader>();
			standaloneInvoiceOld.JZ_StandAloneInvoiceDirection = JobMessageTypeList.Codes.Import;
			standaloneInvoiceOld.JZ_OH_Supplier = consignor.PK;
			standaloneInvoiceOld.JZ_OH_Buyer = consignee.PK;
			standaloneInvoiceOld.JZ_InvoiceNumber = "INVABC123";
			standaloneInvoiceOld.JZ_InvoiceDate = new ZDateTime(2012, 11, 7);

			var standaloneInvoiceNew = Factory.New<JobComInvoiceHeader>();
			standaloneInvoiceNew.JZ_OH_Supplier = consignor.PK;
			standaloneInvoiceNew.JZ_OH_Buyer = consignee.PK;
			standaloneInvoiceNew.JZ_InvoiceNumber = "INVABC123";
			standaloneInvoiceNew.JZ_InvoiceDate = new ZDateTime(2012, 11, 8);
			Factory.SaveForTesting();

			var writeManager = new DataWritingManager(new ActionInfo(null, declaration));
			var invoiceDataObject = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INVABC123",
				InvoiceAmount = 150m
			};
			invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, consignor, AddressTypes.Supplier);
			invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, buyer, nameof(DocAddressType.BuyerDocumentaryAddress));
			invoiceDataObject.OrganizationAddressCollection = null;
			invoiceDataObject.AddOrgAddress(writeManager, consignee, AddressTypes.Importer);

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code },
				MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
				CommercialInfo = SetupCommercialInfo("Top Group", invoiceCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject }))
			};

			var newFactory = new UniversalObjectFactory();
			var invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, newFactory).ReadIntoBusinessObject();
			AssertEquals("Should match to standaloneInvoiceNew as all fields were matched", standaloneInvoiceNew.PK, invoice.PK);
			newFactory.SaveForTesting();

			invoiceDataObject.OrganizationAddressCollection.Add(invoiceDataObject.Supplier);
			invoiceDataObject.OrganizationAddressCollection.Add(invoiceDataObject.Buyer);
			invoiceDataObject.Supplier = null;
			invoiceDataObject.Buyer = null;
			newFactory = new UniversalObjectFactory();
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, newFactory).ReadIntoBusinessObject();
			AssertEquals("Should match to standaloneInvoiceNew as all fields were matched using fallback", standaloneInvoiceNew.PK, invoice.PK);
			newFactory.SaveForTesting();

			declarationDataObject.Branch.Code = "A#@";
			newFactory = new UniversalObjectFactory();
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, newFactory).ReadIntoBusinessObject();
			AssertEquals("Should match to standaloneInvoiceNew though Branch is invalid", standaloneInvoiceNew.PK, invoice.PK);
			newFactory.SaveForTesting();

			declarationDataObject.Branch = null;
			newFactory = new UniversalObjectFactory();
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, newFactory).ReadIntoBusinessObject();
			AssertEquals("Should match to standaloneInvoiceNew though Branch is empty", standaloneInvoiceNew.PK, invoice.PK);
		}

		public void TestSupplierAndBuyerMatch()
		{
			var consigneeDelivery = CreateOrganisation("CONSIGNEEDELIVERY", "ABC!@#1");
			var consigneeDoc = CreateOrganisation("CONSIGNEEDOC", "ABC!@#2");
			var consigneeAddress = CreateOrganisation("CONSIGNEEADDRESS", "ABC!@#3");
			var importerDelivery = CreateOrganisation("IMPORTERDELIVERY", "ABC!@#4");
			var importerDoc = CreateOrganisation("IMPORTERDOC", "ABC!@#5");
			var importer = CreateOrganisation("IMPORTER", "ABC!@#6");
			var consignorPickup = CreateOrganisation("CONSIGNORPICKUP", "ABC!@#7");
			var consignorDoc = CreateOrganisation("CONSIGNORDOC", "ABC!@#8");
			var supplierPickup = CreateOrganisation("SUPPLIERPICKUP", "ABC!@#9");
			var supplierDoc = CreateOrganisation("SUPPLIERDOC", "ABC!@#10");
			var supplier = CreateOrganisation("SUPPLIER", "ABC!@#11");
			var buyer = CreateOrganisation("BUYER", "ABC!@#12");
			var invoiceSupplier = CreateOrganisation("INVOICESUPPLIER", "ABC!@#13");
			var invoiceBuyer = CreateOrganisation("INVOICEBUYER", "ABC!@#14");
			var manufacturer = CreateOrganisation("MANUFACTURER", "ABC!@#15");
			var exporter = CreateOrganisation("EXPORTER", "ABC!@#16");
			var invoicer = CreateOrganisation("INVOICER", "ABC!@#17");
			var seller = CreateOrganisation("SELLER", "ABC!@#18");
			var sellingAgent = CreateOrganisation("SELLINGAGENT", "ABC!@#19");
			var fdaShipper = CreateOrganisation("FDASHIPPER", "ABC!@#20");
			var foreignExporter = CreateOrganisation("FOREIGNEXPORTER", "ABC!@#21");
			var ultimateConsignee = CreateOrganisation("ULTIMATECONSIGNEE", "ABC!@#22");
			var buyingAgent = CreateOrganisation("BUYINGAGENT", "ABC!@#23");
			var soldToParty = CreateOrganisation("SOLDTOPARTY", "ABC!@#24");
			var intermediateConsignee = CreateOrganisation("INTERMEDIATECONSIGNEE", "ABC!@#25");
			var usPrincipalPartyInInterest = CreateOrganisation("USPRINCIPALPARTYININTEREST", "ABC!@#26");
			Factory.SaveForTesting();
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var invoiceDataObject = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INVABC123",
				InvoiceAmount = 150m
			};
			invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, invoiceSupplier, AddressTypes.Supplier);
			invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, invoiceBuyer, AddressTypes.Importer);
			invoiceDataObject.OrganizationAddressCollection.Clear();
			invoiceDataObject.AddOrgAddress(writeManager, consigneeDelivery, DocAddressType.ConsigneePickupDeliveryAddress);
			var consigneeDocDataObj = invoiceDataObject.AddOrgAddress(writeManager, consigneeDoc, DocAddressType.ConsigneeDocumentaryAddress);
			var consigneeAddressDataObj = invoiceDataObject.AddOrgAddress(writeManager, consigneeAddress, DocAddressType.ConsigneeAddress);
			var importerDeliveryDataObj = invoiceDataObject.AddOrgAddress(writeManager, importerDelivery, DocAddressType.ImporterPickupDeliveryAddress);
			var importerDocDataObj = invoiceDataObject.AddOrgAddress(writeManager, importerDoc, DocAddressType.ImporterDocumentaryAddress);
			var importerDataObj = invoiceDataObject.AddOrgAddress(writeManager, importer, AddressTypes.Importer);
			invoiceDataObject.AddOrgAddress(writeManager, consignorPickup, DocAddressType.ConsignorPickupDeliveryAddress);
			var consignorDocDataObj = invoiceDataObject.AddOrgAddress(writeManager, consignorDoc, DocAddressType.ConsignorDocumentaryAddress);
			var supplierPickupDataObj = invoiceDataObject.AddOrgAddress(writeManager, supplierPickup, DocAddressType.SupplierPickupDeliveryAddress);
			var supplierDocDataObj = invoiceDataObject.AddOrgAddress(writeManager, supplierDoc, DocAddressType.SupplierDocumentaryAddress);
			var supplierDataObj = invoiceDataObject.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);
			var buyerDataObj = invoiceDataObject.AddOrgAddress(writeManager, buyer, nameof(DocAddressType.BuyerDocumentaryAddress));
			var manufacturerDataObj = invoiceDataObject.AddOrgAddress(writeManager, manufacturer, nameof(DocAddressType.Manufacturer));
			var exporterDataObj = invoiceDataObject.AddOrgAddress(writeManager, exporter, Constants.AddressType.Exporter);
			var invoicerDataObj = invoiceDataObject.AddOrgAddress(writeManager, invoicer, Constants.AddressType.Invoicer);
			var sellerDataObj = invoiceDataObject.AddOrgAddress(writeManager, seller, Constants.AddressType.Seller);
			var sellingAgentDataObj = invoiceDataObject.AddOrgAddress(writeManager, sellingAgent, Customs.DataTransfer.Universal.Constants.AddressTypes.SellingAgent);
			var fdaShipperDataObj = invoiceDataObject.AddOrgAddress(writeManager, fdaShipper, Constants.AddressType.FDAShipper);
			var foreignExporterDataObj = invoiceDataObject.AddOrgAddress(writeManager, foreignExporter, Constants.AddressType.ForeignExporter);
			var ultimateConsigneeDataObj = invoiceDataObject.AddOrgAddress(writeManager, ultimateConsignee, DocAddressType.UltimateConsignee);
			var buyingAgentDataObj = invoiceDataObject.AddOrgAddress(writeManager, buyingAgent, Customs.DataTransfer.Universal.Constants.AddressTypes.BuyingAgent);
			var soldToPartyDataObj = invoiceDataObject.AddOrgAddress(writeManager, soldToParty, Constants.AddressType.SoldToParty);
			var intermediateConsigneeDataObj = invoiceDataObject.AddOrgAddress(writeManager, intermediateConsignee, Enterprise.Customs.DataTransfer.Universal.Constants.AddressTypes.IntermediateConsignee);
			var usPrincipalPartyInInterestDataObj = invoiceDataObject.AddOrgAddress(writeManager, usPrincipalPartyInInterest, DocAddressType.USPrincipalPartyInInterest);
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				Branch = new Branch() { Code = "B@#" },
				MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
				CommercialInfo = SetupCommercialInfo("Top Group", invoiceCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject }))
			};
			var invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, new UniversalObjectFactory()).ReadIntoBusinessObject();
			AssertEquals("invoice.Supplier.OH_FullName", invoiceSupplier.OH_FullName, invoice.Supplier.OH_FullName);
			AssertEquals("invoice.Importer.OH_FullName", importer.OH_FullName, invoice.Importer.OH_FullName);
			invoiceDataObject.Supplier = null;
			invoiceDataObject.OrganizationAddressCollection.Remove(importerDataObj);
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, new UniversalObjectFactory()).ReadIntoBusinessObject();
			AssertEquals("invoice.Supplier.OH_FullName", supplier.OH_FullName, invoice.Supplier.OH_FullName);
			AssertEquals("invoice.Importer.OH_FullName", invoiceBuyer.OH_FullName, invoice.Importer.OH_FullName);
			invoiceDataObject.OrganizationAddressCollection.Remove(supplierDataObj);
			invoiceDataObject.Buyer = null;
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, new UniversalObjectFactory()).ReadIntoBusinessObject();
			AssertEquals("invoice.Supplier.OH_FullName", manufacturer.OH_FullName, invoice.Supplier.OH_FullName);
			AssertEquals("invoice.Importer.OH_FullName", buyer.OH_FullName, invoice.Importer.OH_FullName);
			invoiceDataObject.OrganizationAddressCollection.Remove(manufacturerDataObj);
			invoiceDataObject.OrganizationAddressCollection.Remove(buyerDataObj);
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, new UniversalObjectFactory()).ReadIntoBusinessObject();
			AssertEquals("invoice.Supplier.OH_FullName", exporter.OH_FullName, invoice.Supplier.OH_FullName);
			AssertEquals("invoice.Importer.OH_FullName", ultimateConsignee.OH_FullName, invoice.Importer.OH_FullName);
			invoiceDataObject.OrganizationAddressCollection.Remove(exporterDataObj);
			invoiceDataObject.OrganizationAddressCollection.Remove(ultimateConsigneeDataObj);
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, new UniversalObjectFactory()).ReadIntoBusinessObject();
			AssertEquals("invoice.Supplier.OH_FullName", invoicer.OH_FullName, invoice.Supplier.OH_FullName);
			AssertEquals("invoice.Importer.OH_FullName", buyingAgent.OH_FullName, invoice.Importer.OH_FullName);
			invoiceDataObject.OrganizationAddressCollection.Remove(invoicerDataObj);
			invoiceDataObject.OrganizationAddressCollection.Remove(buyingAgentDataObj);
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, new UniversalObjectFactory()).ReadIntoBusinessObject();
			AssertEquals("invoice.Supplier.OH_FullName", seller.OH_FullName, invoice.Supplier.OH_FullName);
			AssertEquals("invoice.Importer.OH_FullName", soldToParty.OH_FullName, invoice.Importer.OH_FullName);
			invoiceDataObject.OrganizationAddressCollection.Remove(sellerDataObj);
			invoiceDataObject.OrganizationAddressCollection.Remove(soldToPartyDataObj);
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, new UniversalObjectFactory()).ReadIntoBusinessObject();
			AssertEquals("invoice.Supplier.OH_FullName", sellingAgent.OH_FullName, invoice.Supplier.OH_FullName);
			AssertEquals("invoice.Importer.OH_FullName", intermediateConsignee.OH_FullName, invoice.Importer.OH_FullName);
			invoiceDataObject.OrganizationAddressCollection.Remove(sellingAgentDataObj);
			invoiceDataObject.OrganizationAddressCollection.Remove(intermediateConsigneeDataObj);
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, new UniversalObjectFactory()).ReadIntoBusinessObject();
			AssertEquals("invoice.Supplier.OH_FullName", fdaShipper.OH_FullName, invoice.Supplier.OH_FullName);
			AssertEquals("invoice.Importer.OH_FullName", importerDoc.OH_FullName, invoice.Importer.OH_FullName);
			invoiceDataObject.OrganizationAddressCollection.Remove(fdaShipperDataObj);
			invoiceDataObject.OrganizationAddressCollection.Remove(importerDocDataObj);
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, new UniversalObjectFactory()).ReadIntoBusinessObject();
			AssertEquals("invoice.Supplier.OH_FullName", foreignExporter.OH_FullName, invoice.Supplier.OH_FullName);
			AssertEquals("invoice.Importer.OH_FullName", importerDelivery.OH_FullName, invoice.Importer.OH_FullName);
			invoiceDataObject.OrganizationAddressCollection.Remove(foreignExporterDataObj);
			invoiceDataObject.OrganizationAddressCollection.Remove(importerDeliveryDataObj);
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, new UniversalObjectFactory()).ReadIntoBusinessObject();
			AssertEquals("invoice.Supplier.OH_FullName", usPrincipalPartyInInterest.OH_FullName, invoice.Supplier.OH_FullName);
			AssertEquals("invoice.Importer.OH_FullName", consigneeAddress.OH_FullName, invoice.Importer.OH_FullName);
			invoiceDataObject.OrganizationAddressCollection.Remove(usPrincipalPartyInInterestDataObj);
			invoiceDataObject.OrganizationAddressCollection.Remove(consigneeAddressDataObj);
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, new UniversalObjectFactory()).ReadIntoBusinessObject();
			AssertEquals("invoice.Supplier.OH_FullName", supplierDoc.OH_FullName, invoice.Supplier.OH_FullName);
			AssertEquals("invoice.Importer.OH_FullName", consigneeDoc.OH_FullName, invoice.Importer.OH_FullName);
			invoiceDataObject.OrganizationAddressCollection.Remove(supplierDocDataObj);
			invoiceDataObject.OrganizationAddressCollection.Remove(consigneeDocDataObj);
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, new UniversalObjectFactory()).ReadIntoBusinessObject();
			AssertEquals("invoice.Supplier.OH_FullName", supplierPickup.OH_FullName, invoice.Supplier.OH_FullName);
			AssertEquals("invoice.Importer.OH_FullName", consigneeDelivery.OH_FullName, invoice.Importer.OH_FullName);
			invoiceDataObject.OrganizationAddressCollection.Remove(supplierPickupDataObj);
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, new UniversalObjectFactory()).ReadIntoBusinessObject();
			AssertEquals("invoice.Supplier.OH_FullName", consignorDoc.OH_FullName, invoice.Supplier.OH_FullName);
			invoiceDataObject.OrganizationAddressCollection.Remove(consignorDocDataObj);
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, new UniversalObjectFactory()).ReadIntoBusinessObject();
			AssertEquals("invoice.Supplier.OH_FullName", consignorPickup.OH_FullName, invoice.Supplier.OH_FullName);
		}

		public void TestImportStandaloneCommercialInvoiceData()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, true);
			var supplier = CreateOrganisation("SUPPLIER", "ABC!@#1");
			var importer = CreateOrganisation("IMPORTER", "ABC!@#2");
			var buyer = CreateOrganisation("BUYER", "ABC!@#3");
			var manufacturer = CreateOrganisation("MANUFACTURER", "ABC!@#4");
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_Code = "B@#";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.SaveForTesting();
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				Branch = new Branch()
				{ Code = "B@#" },
				MessageType = new CodeDescriptionPair()
				{ Code = JobMessageTypeList.Codes.Import }
			};
			var masterBill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = "MB1",
				BillType = new WayBillType()
				{ Code = WayBillTypeList.Codes.Master }
			};
			var masterBill2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = "MB2",
				BillType = new WayBillType()
				{ Code = WayBillTypeList.Codes.Master }
			};
			var masterBill1HouseBill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = "MB1HB1",
				BillType = new WayBillType()
				{ Code = WayBillTypeList.Codes.House },
				ParentBillNumber = "MB1"
			};
			var masterBill1HouseBill2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = "MB1HB2",
				BillType = new WayBillType()
				{ Code = WayBillTypeList.Codes.House },
				ParentBillNumber = "MB1"
			};
			var masterBill2HouseBill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = "MB2HB1",
				BillType = new WayBillType()
				{ Code = WayBillTypeList.Codes.House },
				ParentBillNumber = "MB2"
			};
			var masterBill1HouseBill2SubHouse1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = "MB1HB2SB1",
				BillType = new WayBillType()
				{ Code = WayBillTypeList.Codes.SubHouse },
				ParentBillNumber = "MB1HB2"
			};
			var masterBill2HouseBill1SubHouse1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = "MB2HB1SB1",
				BillType = new WayBillType()
				{ Code = WayBillTypeList.Codes.SubHouse },
				ParentBillNumber = "MB2HB1"
			};
			var containerDataObject1 = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerNumber = "CONT123ABC" };
			var containerDataObject2 = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerNumber = "CONT456DEF" };
			var transport1 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LegOrder = 1,
				PortOfLoading = new UNLOCO()
				{ Code = "AUSYD" },
				PortOfDischarge = new UNLOCO()
				{ Code = "USLAX" }
			};
			var transport2 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LegOrder = 2,
				PortOfLoading = new UNLOCO()
				{ Code = "USLAX" },
				PortOfDischarge = new UNLOCO()
				{ Code = "USCHI" }
			};
			declarationDataObject.SetContainerCollection(() => new DataObjectList<Container>(new[] { containerDataObject2, containerDataObject1 }));
			declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { masterBill1, masterBill2, masterBill1HouseBill1, masterBill1HouseBill2, masterBill2HouseBill1, masterBill1HouseBill2SubHouse1, masterBill2HouseBill1SubHouse1 }));
			declarationDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>(new[] { transport1, transport2 }));
			var invoiceLineDataObject1Charge = new UniversalCustoms.CommercialCharge()
			{ ChargeType = Commission, Amount = 100m };
			var invoiceLineDataObject2Charge = new UniversalCustoms.CommercialCharge()
			{ ChargeType = Discount, Amount = 150m };
			var invoiceLineDataObject1 = new UniversalCustoms.CommercialInvoiceLine()
			{ LineNo = 1, HarmonisedCode = "1010101010", CommercialChargeCollection = new List<UniversalCustoms.CommercialCharge>(new[] { invoiceLineDataObject1Charge }) };
			var invoiceLineDataObject2 = new UniversalCustoms.CommercialInvoiceLine()
			{ LineNo = 2, HarmonisedCode = "2020202020", CommercialChargeCollection = new List<UniversalCustoms.CommercialCharge>(new[] { invoiceLineDataObject2Charge }) };
			var invoiceDataObjectCharge1 = new UniversalCustoms.CommercialCharge()
			{ ChargeType = OverseasInsurance, Amount = 200m };
			var invoiceDataObjectCharge2 = new UniversalCustoms.CommercialCharge()
			{ ChargeType = OverseasFreight, Amount = 300m };
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var invoiceDataObject = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV123",
				InvoiceAmount = 1500m,
				IncoTerm = new CodeDescriptionPair()
				{ Code = Core.Constants.IncoTerms.CostInsuranceAndFreight }
			};
			invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);
			invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, buyer, nameof(DocAddressType.BuyerDocumentaryAddress));
			invoiceDataObject.OrganizationAddressCollection = null;
			invoiceDataObject.AddOrgAddress(writeManager, manufacturer, nameof(DocAddressType.Manufacturer));
			invoiceDataObject.AddOrgAddress(writeManager, importer, AddressTypes.Importer);
			invoiceDataObject.CommercialChargeCollection = new List<UniversalCustoms.CommercialCharge>(new[] { invoiceDataObjectCharge1, invoiceDataObjectCharge2 });
			invoiceDataObject.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject1, invoiceLineDataObject2 }));
			var noteDataObj1 = new Note()
			{ IsCustomDescription = ZBool.True, Description = "DESC 1", NoteText = "HELLO WORLD" };
			var noteDataObj2 = new Note()
			{ IsCustomDescription = ZBool.True, Description = "DESC 2", NoteText = "GOODBYE WORLD" };
			declarationDataObject.SetNoteCollection(() => new DataObjectList<Note>(new[] { noteDataObj1, noteDataObj2 }));
			declarationDataObject.CommercialInfo = new UniversalCustoms.CommercialInfo()
			{ Name = "Top Group", CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject }) };
			var reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
			var invoice = reader.ReadIntoBusinessObject();
			AssertNotNull(invoice);
			CombineAssertions(delegate
			{
				AssertEquals("invoice.JZ_OH_Supplier", supplier.PK, invoice.JZ_OH_Supplier);
				AssertEquals("invoice.JZ_OH_Buyer", importer.PK, invoice.JZ_OH_Buyer);
				AssertEquals("invoice.JZ_GB", branch2.PK, invoice.JZ_GB);
				AssertEquals("invoice.JZ_StandAloneInvoiceDirection", JobMessageTypeList.Codes.Import, invoice.JZ_StandAloneInvoiceDirection);
				AssertEquals("invoice.JZ_InvoiceNumber", "INV123", invoice.JZ_InvoiceNumber);
				AssertEquals("invoice.JZ_InvoiceAmount", 1500m, invoice.JZ_InvoiceAmount);
				AssertEquals("invoice.JZ_IncoTerm", Core.Constants.IncoTerms.CostInsuranceAndFreight, invoice.JZ_IncoTerm);
				AssertEquals("invoice.BuyerOrgPK", buyer.PK, invoice.BuyerOrgPK);
				AssertEquals("invoice.JZ_OA_ManufacturerAddress", manufacturer.MainAddress.PK, invoice.JZ_OA_ManufacturerAddress);
				AssertEquals("invoice.Charges.Count", 2, invoice.Charges.Count);
				AssertNotNull(invoice.Charges.OfType<InvoiceCharge>().FirstOrDefault(x => x.J7_ChargeType == USCustomsChargeTypeList.Codes.OverseasInsurance && x.J7_Amount == 200m));
				AssertNotNull(invoice.Charges.OfType<InvoiceCharge>().FirstOrDefault(x => x.J7_ChargeType == USCustomsChargeTypeList.Codes.OverseasFreight && x.J7_Amount == 300m));
				AssertEquals("invoice.JobComInvoiceLines.Count", 2, invoice.JobComInvoiceLines.Count);
				var invoiceLine1 = invoice.JobComInvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.JI_LineNo == 1 && x.JI_Tariff == "1010101010");
				AssertEquals("invoiceLine1.Charges.Count", 1, invoiceLine1.Charges.Count);
				AssertNotNull(invoiceLine1.Charges.OfType<InvoiceLineCharge>().FirstOrDefault(x => x.J7_ChargeType == USCustomsChargeTypeList.Codes.Commission && x.J7_Amount == 100m));
				var invoiceLine2 = invoice.JobComInvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.JI_LineNo == 2 && x.JI_Tariff == "2020202020");
				AssertEquals("invoiceLine2.Charges.Count", 1, invoiceLine2.Charges.Count);
				AssertNotNull(invoiceLine2.Charges.OfType<InvoiceLineCharge>().FirstOrDefault(x => x.J7_ChargeType == USCustomsChargeTypeList.Codes.Discount && x.J7_Amount == 150m));
				AssertEquals("invoice.Transports.Count", 2, invoice.Transports.Count);
				AssertNotNull(invoice.Transports.OfType<Freight.Business.Transport>().FirstOrDefault(x => x.JW_LegOrder == 1 && x.JW_RL_NKLoadPort == "AUSYD" && x.JW_RL_NKDiscPort == "USLAX"));
				AssertNotNull(invoice.Transports.OfType<Freight.Business.Transport>().FirstOrDefault(x => x.JW_LegOrder == 2 && x.JW_RL_NKLoadPort == "USLAX" && x.JW_RL_NKDiscPort == "USCHI"));
				var notes = invoice.Notes.FindByDescription("DESC 1");
				AssertEquals("notes.Length", 1, notes.Length);
				AssertNotNull(notes.FirstOrDefault(x => x.ST_IsCustomDescription && x.ST_NoteDataAsText == "HELLO WORLD"));
				notes = invoice.Notes.FindByDescription("DESC 2");
				AssertEquals("notes.Length", 1, notes.Length);
				AssertNotNull(notes.FirstOrDefault(x => x.ST_IsCustomDescription && x.ST_NoteDataAsText == "GOODBYE WORLD"));
				AssertEquals("invoice.InvoiceHeaderRefs.Count", 9, invoice.InvoiceHeaderRefs.Count);
				AssertNotNull("Container CONT123ABC should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.CN && x.J2_ReferenceNumber == "CONT123ABC"));
				AssertNotNull("Container CONT456DEF should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.CN && x.J2_ReferenceNumber == "CONT456DEF"));
				AssertNotNull("MasterBill MB1 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.MB && x.J2_ReferenceNumber == "MB1"));
				AssertNotNull("MasterBill MB2 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.MB && x.J2_ReferenceNumber == "MB2"));
				AssertNotNull("HouseBill MB1HB1 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.HB && x.J2_ReferenceNumber == "MB1HB1"));
				AssertNotNull("HouseBill MB1HB2 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.HB && x.J2_ReferenceNumber == "MB1HB2"));
				AssertNotNull("HouseBill MB2HB1 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.HB && x.J2_ReferenceNumber == "MB2HB1"));
				AssertNotNull("SubHouseBill MB1HB2SB1 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.SH && x.J2_ReferenceNumber == "MB1HB2SB1"));
				AssertNotNull("SubHouseBill MB2HB1SB1 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.SH && x.J2_ReferenceNumber == "MB2HB1SB1"));
			});
		}

		public void TestImportAdvanceShippingNoticeData()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
				consignor.OH_IsConsignor = ZBool.True;
				consignor.OH_RL_NKClosestPort = "AUSYD";
				var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
				consignee.OH_IsConsignee = ZBool.True;
				consignee.OH_RL_NKClosestPort = "USCHI";
				var branch2 = Factory.New<GlbBranch>();
				branch2.GB_GC = GlbCompany.CurrentCompany.PK;
				branch2.GB_Code = "B@#";
				branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				var classification = Factory.New<CusClassification>();
				classification.CC_LookupCode = "TestLookup";
				classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
				var part = Factory.New<Business.OrgSupplierPart>();
				part.OP_PartNum = "PART1";
				part.OP_Weight = 100m;
				part.OP_WeightUQ = "HG";
				part.RelatedOrganisations.AddSupplier(consignor);
				part.RelatedOrganisations.AddOwner(consignee);
				CusClassPartPivot pivot = part.PivotsForBinding.AddNew();
				pivot.CI_UsageComment = "U1";
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot.CI_TariffNum = "1010101010";
				pivot.CI_SupplementalTariff = "2020202020";
				pivot.CI_CC = classification.PK;
				pivot.CI_OP = part.PK;
				CusClassPartPivot pivotChild1 = pivot.Children.AddNew();
				pivotChild1.CI_UsageComment = "CU1";
				pivotChild1.CI_ChildType = ClassificationChildTypeList.Codes.Related;
				pivotChild1.CI_TariffNum = "1010101011";
				pivotChild1.CI_SupplementalTariff = "2020202021";
				pivotChild1.CI_CC = classification.PK;
				pivotChild1.CI_OP = part.PK;
				CusClassPartPivot pivotChild2 = pivot.Children.AddNew();
				pivotChild2.CI_UsageComment = "CU2";
				pivotChild2.CI_ChildType = ClassificationChildTypeList.Codes.Related;
				pivotChild2.CI_TariffNum = "1010101012";
				pivotChild2.CI_SupplementalTariff = "2020202022";
				pivotChild2.CI_CC = classification.PK;
				pivotChild2.CI_OP = part.PK;
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);
				var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					Branch = new Branch()
					{ Code = "B@#" },
				};
				var invoiceLineDataObject1 = new UniversalCustoms.CommercialInvoiceLine()
				{ LineNo = 1, PartNo = "PART1", InvoiceQuantity = 1 };
				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				var invoiceDataObject = SetupCommercialInvoiceHeaderData("INV123", null, null, 1500.32m, ForeignCurrency, new ZDateTime(2012, 3, 25, 13, 3, 2), new CodeDescriptionPair()
				{ Code = Core.Constants.IncoTerms.CostInsuranceAndFreight }, 10.4m, new UnitOfVolume()
				{ Code = Core.Constants.Volume.CubicMetres }, 1202.53m, new UnitOfWeight()
				{ Code = Core.Constants.Weight.Pounds }, 11.11m, new UnitOfWeight()
				{ Code = Core.Constants.Weight.Kilograms }, FixedRate, 1.5m, 1.25m, 100m, "PO234", 1500m, 1.31m, new ZDateTime(2012, 3, 30), null);
				invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, consignor, AddressTypes.Supplier);
				invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, consignee, AddressTypes.Importer);
				invoiceDataObject.OrganizationAddressCollection = null;
				invoiceDataObject.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject1 }));
				declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group", invoiceCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject }));
				var reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				var invoice = reader.ReadIntoBusinessObject();
				Factory.SaveAtEndOfImport(logger);
				AssertNotNull(invoice);
				CombineAssertions(delegate
				{
					AssertEquals("invoice.JZ_StandAloneInvoiceDirection", "IMP", invoice.JZ_StandAloneInvoiceDirection);
					AssertEquals("invoice.JobComInvoiceLines.Count", 3, invoice.JobComInvoiceLines.Count);
				});
				declarationDataObject.MessageType = new CodeDescriptionPair()
				{ Code = JobMessageTypeList.MoreCodes.AdvanceShippingNotice };
				reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				invoice = reader.ReadIntoBusinessObject();
				Factory.SaveAtEndOfImport(logger);
				AssertNotNull(invoice);
				CombineAssertions(delegate
				{
					AssertEquals("invoice.JZ_StandAloneInvoiceDirection", "ASN", invoice.JZ_StandAloneInvoiceDirection);
					AssertEquals("invoice.JZ_MessageType", "ASN", invoice.JZ_MessageType);
					AssertEquals("invoice.JobComInvoiceLines.Count", 1, invoice.JobComInvoiceLines.Count);
					var invoiceLine = invoice.JobComInvoiceLines[0];
					AssertEquals("invoiceLine.JI_OP has NOT been defaulted", ZGuid.Empty, invoiceLine.JI_OP);
					AssertEquals("invoiceLine.JI_Weight has NOT been defaulted", 0m, invoiceLine.JI_Weight);
					AssertEquals("invoiceLine.JI_WeightUQ has NOT been defaulted", "KG", invoiceLine.JI_WeightUQ);
				});
			}
		}

		public void TestImportAdvanceShippingNoticeDataWithButerAndSeller()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
				consignor.OH_IsConsignor = ZBool.True;
				consignor.OH_RL_NKClosestPort = "AUSYD";
				var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
				consignee.OH_IsConsignee = ZBool.True;
				consignee.OH_RL_NKClosestPort = "USCHI";
				var branch2 = Factory.New<GlbBranch>();
				branch2.GB_GC = GlbCompany.CurrentCompany.PK;
				branch2.GB_Code = "B@#";
				branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				var classification = Factory.New<CusClassification>();
				classification.CC_LookupCode = "TestLookup";
				classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
				var part = Factory.New<Business.OrgSupplierPart>();
				part.OP_PartNum = "PART1";
				part.OP_Weight = 100m;
				part.OP_WeightUQ = "HG";
				part.RelatedOrganisations.AddSupplier(consignor);
				part.RelatedOrganisations.AddOwner(consignee);
				CusClassPartPivot pivot = part.PivotsForBinding.AddNew();
				pivot.CI_UsageComment = "U1";
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot.CI_TariffNum = "1010101010";
				pivot.CI_SupplementalTariff = "2020202020";
				pivot.CI_CC = classification.PK;
				pivot.CI_OP = part.PK;
				CusClassPartPivot pivotChild1 = pivot.Children.AddNew();
				pivotChild1.CI_UsageComment = "CU1";
				pivotChild1.CI_ChildType = ClassificationChildTypeList.Codes.Related;
				pivotChild1.CI_TariffNum = "1010101011";
				pivotChild1.CI_SupplementalTariff = "2020202021";
				pivotChild1.CI_CC = classification.PK;
				pivotChild1.CI_OP = part.PK;
				CusClassPartPivot pivotChild2 = pivot.Children.AddNew();
				pivotChild2.CI_UsageComment = "CU2";
				pivotChild2.CI_ChildType = ClassificationChildTypeList.Codes.Related;
				pivotChild2.CI_TariffNum = "1010101012";
				pivotChild2.CI_SupplementalTariff = "2020202022";
				pivotChild2.CI_CC = classification.PK;
				pivotChild2.CI_OP = part.PK;
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);
				var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					Branch = new Branch()
					{ Code = "B@#" },
				};
				var invoiceLineDataObject1 = new UniversalCustoms.CommercialInvoiceLine()
				{ LineNo = 1, PartNo = "PART1", InvoiceQuantity = 1 };
				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				var invoiceDataObject = SetupCommercialInvoiceHeaderData("INV123", null, null, 1500.32m, ForeignCurrency, new ZDateTime(2012, 3, 25, 13, 3, 2), new CodeDescriptionPair()
				{ Code = Core.Constants.IncoTerms.CostInsuranceAndFreight }, 10.4m, new UnitOfVolume()
				{ Code = Core.Constants.Volume.CubicMetres }, 1202.53m, new UnitOfWeight()
				{ Code = Core.Constants.Weight.Pounds }, 11.11m, new UnitOfWeight()
				{ Code = Core.Constants.Weight.Kilograms }, FixedRate, 1.5m, 1.25m, 100m, "PO234", 1500m, 1.31m, new ZDateTime(2012, 3, 30), null);
				invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, consignor, AddressTypes.Supplier);
				invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, consignee, AddressTypes.Importer);
				var seller = invoiceDataObject.AddOrgAddress(writeManager, consignor, Constants.AddressType.Seller);
				var buyer = invoiceDataObject.AddOrgAddress(writeManager, consignee, nameof(DocAddressType.BuyerDocumentaryAddress));
				invoiceDataObject.OrganizationAddressCollection.Add(seller);
				invoiceDataObject.OrganizationAddressCollection.Add(buyer);
				invoiceDataObject.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject1 }));
				declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group", invoiceCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject }));
				declarationDataObject.MessageType = new CodeDescriptionPair()
				{ Code = JobMessageTypeList.Codes.Import };
				var reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				var invoice = reader.ReadIntoBusinessObject();
				Factory.SaveAtEndOfImport(logger);
				AssertNotNull(invoice);
				CombineAssertions(delegate
				{
					AssertEquals("invoice.JZ_StandAloneInvoiceDirection", "IMP", invoice.JZ_StandAloneInvoiceDirection);
					AssertEquals("invoice.JobComInvoiceLines.Count", 3, invoice.JobComInvoiceLines.Count);
					Assert(!invoice.JZ_OA_SellerAddress.IsEmpty);
					Assert(!invoice.BuyerOrgPK.IsEmpty);
				});
				declarationDataObject.MessageType = new CodeDescriptionPair()
				{ Code = JobMessageTypeList.MoreCodes.AdvanceShippingNotice };
				reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				invoice = reader.ReadIntoBusinessObject();
				Factory.SaveAtEndOfImport(logger);
				AssertNotNull(invoice);
				CombineAssertions(delegate
				{
					AssertEquals("invoice.JZ_StandAloneInvoiceDirection", "ASN", invoice.JZ_StandAloneInvoiceDirection);
					AssertEquals("invoice.JZ_MessageType", "ASN", invoice.JZ_MessageType);
					Assert(!invoice.JZ_OA_SellerAddress.IsEmpty);
					Assert(!invoice.BuyerOrgPK.IsEmpty);
					AssertEquals("invoice.JobComInvoiceLines.Count", 1, invoice.JobComInvoiceLines.Count);
					var invoiceLine = invoice.JobComInvoiceLines[0];
					AssertEquals("invoiceLine.JI_OP has NOT been defaulted", ZGuid.Empty, invoiceLine.JI_OP);
					AssertEquals("invoiceLine.JI_Weight has NOT been defaulted", 0m, invoiceLine.JI_Weight);
					AssertEquals("invoiceLine.JI_WeightUQ has NOT been defaulted", "KG", invoiceLine.JI_WeightUQ);
				});
			}
		}

		public void TestPopulateJZ_OH_BuyerAndJZ_OA_BuyerAddress()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var importer = CreateOrganisation("IMPORTER", "ABC!@#6");
			var supplier = CreateOrganisation("SUPPLIER", "ABC!@#11");
			var buyer = CreateOrganisation("BUYER", "ABC!@#12");
			Factory.SaveForTesting();
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var invoiceDataObject = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{ InvoiceNumber = "INVABC123", InvoiceAmount = 150m };
			invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);
			invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, buyer, AddressTypes.Importer);
			invoiceDataObject.OrganizationAddressCollection.Clear();
			invoiceDataObject.AddOrgAddress(writeManager, importer, AddressTypes.Importer);
			invoiceDataObject.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				Branch = new Branch()
				{ Code = "B@#" },
				MessageType = new CodeDescriptionPair()
				{ Code = JobMessageTypeList.Codes.Import },
				CommercialInfo = SetupCommercialInfo("Top Group", invoiceCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject }))
			};
			var invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, new UniversalObjectFactory()).ReadIntoBusinessObject();
			AssertEquals(buyer.MainAddress.PK, invoice.JZ_OA_BuyerAddress);
			AssertNotEquals(buyer.PK, invoice.JZ_OH_Buyer);
			AssertEquals(importer.PK, invoice.JZ_OH_Buyer);
		}
	}
}
