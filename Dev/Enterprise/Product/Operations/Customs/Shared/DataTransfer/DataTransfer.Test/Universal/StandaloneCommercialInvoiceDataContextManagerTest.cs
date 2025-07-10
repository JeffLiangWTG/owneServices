using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	[TestedType(typeof(StandaloneCommercialInvoiceDataContextManager))]
	sealed class StandaloneCommercialInvoiceDataContextManagerTest : ShipmentDataContextManagerTestCase<StandaloneCommercialInvoiceDataContextManager, BaseJobComInvoiceHeader>
	{
		public void TestAvoidNullReferenceException_When_CommercialInfo_CommercialInvoiceCollection_Is_Null()
		{
			var consignee = OrganizationAddressTestHelper.GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			consignee.OH_IsConsignee = ZBool.True;
			var consignor = OrganizationAddressTestHelper.GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			consignor.OH_IsConsignor = ZBool.True;
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var shipmentData = new UniversalShipment { DataContext = dataContext };
			var invoiceData1 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData1.Buyer = invoiceData1.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData1.Supplier = invoiceData1.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData1.InvoiceNumber = "INV1";
			invoiceData1.BillNumber = "HB1";
			invoiceData1.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			var invoiceData2 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData2.Buyer = invoiceData2.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData2.Supplier = invoiceData2.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData2.InvoiceNumber = "INV2";
			invoiceData2.BillNumber = "HB1";
			invoiceData2.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			var invoiceData3 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData3.Buyer = invoiceData3.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData3.Supplier = invoiceData3.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData3.InvoiceNumber = "INV3";
			invoiceData3.BillNumber = "HB2";
			invoiceData3.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			var invoiceData4 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData4.Buyer = invoiceData4.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData4.Supplier = invoiceData4.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData4.InvoiceNumber = "INV4";
			invoiceData4.BillNumber = "HB2";
			invoiceData4.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			var invoiceData5 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData5.Buyer = invoiceData5.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData5.Supplier = invoiceData5.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData5.InvoiceNumber = "INV5";
			invoiceData5.BillNumber = "HB2";
			invoiceData5.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			var invoiceData6 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData6.Buyer = invoiceData6.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData6.Supplier = invoiceData6.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData6.InvoiceNumber = "INV6";
			invoiceData6.BillNumber = "HB3";
			invoiceData6.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			var invoiceData7 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData7.Buyer = invoiceData7.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData7.Supplier = invoiceData7.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData7.InvoiceNumber = "INV7";
			invoiceData7.BillNumber = "HB3";
			invoiceData7.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };

			shipmentData.CommercialInfo = new UniversalCustoms.CommercialInfo()
			{
				Name = "GROUP1",
				//CommercialInvoiceCollection = new List<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData1, invoiceData4 }),
				SubGroupCollection = new List<UniversalCustoms.CommercialInfo>(new[]
				{
					new UniversalCustoms.CommercialInfo()
					{
						Name = "SUBGROUP1",
						//CommercialInvoiceCollection = new List<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData2, invoiceData3 }),
						SubGroupCollection = new List<UniversalCustoms.CommercialInfo>(new[]
						{
							new UniversalCustoms.CommercialInfo()
							{
								Name = "SUBGROUP2",
								//CommercialInvoiceCollection = new List<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData5 })
							}
						})
					}
				})
			};

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalShipmentMessage(shipmentData);

			AssertNoExceptionThrown(() =>
			{
				manager.Process(message);
			});
		}

		public void TestHandlingMultiInvoices()
		{
			var consignee = OrganizationAddressTestHelper.GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			consignee.OH_IsConsignee = ZBool.True;
			var consignor = OrganizationAddressTestHelper.GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			consignor.OH_IsConsignor = ZBool.True;
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var shipmentData = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = dataContext };
			var invoiceData1 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData1.Buyer = invoiceData1.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData1.Supplier = invoiceData1.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData1.InvoiceNumber = "INV1";
			invoiceData1.BillNumber = "HB1";
			invoiceData1.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			var invoiceData2 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData2.Buyer = invoiceData2.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData2.Supplier = invoiceData2.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData2.InvoiceNumber = "INV2";
			invoiceData2.BillNumber = "HB1";
			invoiceData2.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			var invoiceData3 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData3.Buyer = invoiceData3.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData3.Supplier = invoiceData3.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData3.InvoiceNumber = "INV3";
			invoiceData3.BillNumber = "HB2";
			invoiceData3.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			var invoiceData4 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData4.Buyer = invoiceData4.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData4.Supplier = invoiceData4.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData4.InvoiceNumber = "INV4";
			invoiceData4.BillNumber = "HB2";
			invoiceData4.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			var invoiceData5 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData5.Buyer = invoiceData5.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData5.Supplier = invoiceData5.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData5.InvoiceNumber = "INV5";
			invoiceData5.BillNumber = "HB2";
			invoiceData5.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			var invoiceData6 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData6.Buyer = invoiceData6.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData6.Supplier = invoiceData6.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData6.InvoiceNumber = "INV6";
			invoiceData6.BillNumber = "HB3";
			invoiceData6.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			var invoiceData7 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData7.Buyer = invoiceData7.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData7.Supplier = invoiceData7.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData7.InvoiceNumber = "INV7";
			invoiceData7.BillNumber = "HB3";
			invoiceData7.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };

			shipmentData.CommercialInfo = new UniversalCustoms.CommercialInfo()
			{
				Name = "GROUP1",
				CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData1, invoiceData4 }),
				SubGroupCollection = new List<UniversalCustoms.CommercialInfo>(new[]
				{
					new UniversalCustoms.CommercialInfo()
					{
						Name = "SUBGROUP1",
						CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData2, invoiceData3 }),
						SubGroupCollection = new List<UniversalCustoms.CommercialInfo>(new[]
						{
							new UniversalCustoms.CommercialInfo()
							{
								Name = "SUBGROUP2",
								CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData5 })
							}
						})
					}
				})
			};
			var innerShipment1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					Name = "GROUP2",
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData7 })
				},
			};
			innerShipment1.SetContainerCollection(() => new DataObjectList<Container>(new[] { new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT2" } }));
			var innerShipment2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					Name = "GROUP3",
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData6 })
				},
			};
			innerShipment2.SetContainerCollection(() => new DataObjectList<Container>(new[] { new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT3" } }));
			shipmentData.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>(new[] { innerShipment1, innerShipment2 }));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalShipmentMessage(shipmentData);
			manager.Process(message);

			var newFactory = new BusinessObjectFactory();
			var query = new ZQuery(JobComInvoiceHeaderSchema.JZ_OH_Buyer, consignee.PK);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_OH_Supplier, consignor.PK);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, SQLComparisonOperator.StartsWith, "INV");
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, ZBool.False);
			var invoices = newFactory.Load<BaseJobComInvoiceHeader>(query);
			AssertEquals("invoices.Length", 7, invoices.Length);
			AssertHasBill(invoices, "INV1", "HB1");
			AssertHasBill(invoices, "INV2", "HB1");
			AssertHasBill(invoices, "INV3", "HB2");
			AssertHasBill(invoices, "INV4", "HB2");
			AssertHasBill(invoices, "INV5", "HB2");
			AssertHasBill(invoices, "INV6", "HB3");
			AssertHasBill(invoices, "INV7", "HB3");

			AssertMultilineASCIIEquals("Service Task Log", @"
Added Invoice INV1 (SUP:WUFSHIJNB IMP:CRAHOLSYD) from UniversalShipment.
Added Invoice INV4 (SUP:WUFSHIJNB IMP:CRAHOLSYD) from UniversalShipment.
Added Invoice INV2 (SUP:WUFSHIJNB IMP:CRAHOLSYD) from UniversalShipment.
Added Invoice INV3 (SUP:WUFSHIJNB IMP:CRAHOLSYD) from UniversalShipment.
Added Invoice INV5 (SUP:WUFSHIJNB IMP:CRAHOLSYD) from UniversalShipment.
Added Invoice INV7 (SUP:WUFSHIJNB IMP:CRAHOLSYD) from UniversalShipment.
Added Invoice INV6 (SUP:WUFSHIJNB IMP:CRAHOLSYD) from UniversalShipment.
Successfully saved Invoice INV6 (SUP:WUFSHIJNB IMP:CRAHOLSYD) with 6 x BaseJobComInvoiceHeader.
".Trim(), serviceTaskLog.ToString());

			var logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matching 'BuyerDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '1804 Fudrucker Way' (only address).
Matching 'SupplierDocumentaryAddress':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
No matching BaseJobComInvoiceHeader found, creating new BaseJobComInvoiceHeader.
Populating BaseJobComInvoiceHeader...
Added Invoice INV1 (SUP:WUFSHIJNB IMP:CRAHOLSYD) from UniversalShipment.
Matching 'BuyerDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '1804 Fudrucker Way' (only address).
Matching 'SupplierDocumentaryAddress':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
No matching BaseJobComInvoiceHeader found, creating new BaseJobComInvoiceHeader.
Populating BaseJobComInvoiceHeader...
Added Invoice INV4 (SUP:WUFSHIJNB IMP:CRAHOLSYD) from UniversalShipment.
Matching 'BuyerDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '1804 Fudrucker Way' (only address).
Matching 'SupplierDocumentaryAddress':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
No matching BaseJobComInvoiceHeader found, creating new BaseJobComInvoiceHeader.
Populating BaseJobComInvoiceHeader...
Added Invoice INV2 (SUP:WUFSHIJNB IMP:CRAHOLSYD) from UniversalShipment.
Matching 'BuyerDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '1804 Fudrucker Way' (only address).
Matching 'SupplierDocumentaryAddress':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
No matching BaseJobComInvoiceHeader found, creating new BaseJobComInvoiceHeader.
Populating BaseJobComInvoiceHeader...
Added Invoice INV3 (SUP:WUFSHIJNB IMP:CRAHOLSYD) from UniversalShipment.
Matching 'BuyerDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '1804 Fudrucker Way' (only address).
Matching 'SupplierDocumentaryAddress':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
No matching BaseJobComInvoiceHeader found, creating new BaseJobComInvoiceHeader.
Populating BaseJobComInvoiceHeader...
Added Invoice INV5 (SUP:WUFSHIJNB IMP:CRAHOLSYD) from UniversalShipment.
Matching 'BuyerDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '1804 Fudrucker Way' (only address).
Matching 'SupplierDocumentaryAddress':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
No matching BaseJobComInvoiceHeader found, creating new BaseJobComInvoiceHeader.
Populating BaseJobComInvoiceHeader...
Added Invoice INV7 (SUP:WUFSHIJNB IMP:CRAHOLSYD) from UniversalShipment.
Matching 'BuyerDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '1804 Fudrucker Way' (only address).
Matching 'SupplierDocumentaryAddress':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
No matching BaseJobComInvoiceHeader found, creating new BaseJobComInvoiceHeader.
Populating BaseJobComInvoiceHeader...
Added Invoice INV6 (SUP:WUFSHIJNB IMP:CRAHOLSYD) from UniversalShipment.
Successfully saved Invoice INV6 (SUP:WUFSHIJNB IMP:CRAHOLSYD) with 6 x BaseJobComInvoiceHeader.
".Trim(), logNoteText);
		}

		public void TestDefaultOutputDirectory()
		{
			var directory = "\\WHERE\\IS\\THIS\\DIRECTORY";
			SystemDataRegistry.Instance.CustomDeclarationExportDirectory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, directory);
			var manager = new StandaloneCommercialInvoiceDataContextManager();
			AssertEquals("DefaultOutputDirectory", directory, manager.DefaultOutputDirectory);
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("BaseJobComInvoiceHeader doesn't have any unique jobnumber", true);
		}

		public void TestImportStandaloneCommercialInvoice()
		{
			var consignor = OrganizationAddressTestHelper.GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var consignee = OrganizationAddressTestHelper.GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_Code = "B@#";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				Branch = new Branch() { Code = "B@#" },
				MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import }
			};
			var masterBill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master } };
			var masterBill2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB2", BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master } };
			var masterBill1HouseBill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB1HB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House }, ParentBillNumber = "MB1" };
			var masterBill1HouseBill2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB1HB2", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House }, ParentBillNumber = "MB1" };
			var masterBill2HouseBill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB2HB2", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House }, ParentBillNumber = "MB2" };
			var masterBill1HouseBill2SubHouse1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB1HB2SB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.SubHouse }, ParentBillNumber = "MB1HB2" };
			var masterBill2HouseBill1SubHouse1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB2HB2SB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.SubHouse }, ParentBillNumber = "MB2HB2" };

			var containerDataObject1 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT123ABC" };
			var containerDataObject2 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT456DEF" };

			var transportDataObject1 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance) { TransportMode = TransportMode.Sea, LegOrder = 1, PortOfLoading = new UNLOCO() { Code = "AUSYD" }, PortOfDischarge = new UNLOCO() { Code = "USLAX" }, EstimatedDeparture = new ZDateTime(2012, 3, 1), EstimatedArrival = new ZDateTime(2012, 3, 11) };
			var transportDataObject2 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance) { TransportMode = TransportMode.Sea, LegOrder = 2, PortOfLoading = new UNLOCO() { Code = "USLAX" }, PortOfDischarge = new UNLOCO() { Code = "USCHI" }, EstimatedDeparture = new ZDateTime(2012, 3, 12), EstimatedArrival = new ZDateTime(2012, 3, 13) };

			declarationDataObject.SetContainerCollection(() => new DataObjectList<Container>(new[] { containerDataObject2, containerDataObject1 }));
			declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { masterBill1, masterBill2, masterBill1HouseBill1, masterBill1HouseBill2, masterBill2HouseBill1, masterBill1HouseBill2SubHouse1, masterBill2HouseBill1SubHouse1 }));
			declarationDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>(new[] { transportDataObject1, transportDataObject2 }));

			var foreignCurrency = new Currency() { Code = Core.Constants.CurrencyCodes.CaymanIslands };
			var commission = new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.Commission, Description = CustomsChargeTypeList.Descriptions.Commission };
			var discount = new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.Discount, Description = CustomsChargeTypeList.Descriptions.Discount };
			var overseasFreight = new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.OverseasFreight, Description = CustomsChargeTypeList.Descriptions.OverseasFreight };
			var overseasInsurance = new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.OverseasInsurance, Description = CustomsChargeTypeList.Descriptions.OverseasInsurance };
			var fixedRate = new CodeDescriptionPair() { Code = ChargeExchangeRateTypeList.Codes.FixedRate, Description = ChargeExchangeRateTypeList.Descriptions.FixedRate };

			var invoiceLineDataObject1Charge = new UniversalCustoms.CommercialCharge() { ChargeType = commission, Currency = foreignCurrency, Amount = 100m, AgreedExchangeRate = 1.5m, ExchangeRateType = fixedRate };
			var invoiceLineDataObject2Charge = new UniversalCustoms.CommercialCharge() { ChargeType = discount, Currency = foreignCurrency, Amount = 150m, AgreedExchangeRate = 1.5m, ExchangeRateType = fixedRate };
			var invoiceLineDataObject1 = new UniversalCustoms.CommercialInvoiceLine() { LineNo = 1, HarmonisedCode = "1010101010", CommercialChargeCollection = new List<UniversalCustoms.CommercialCharge>(new[] { invoiceLineDataObject1Charge }) };
			var invoiceLineDataObject2 = new UniversalCustoms.CommercialInvoiceLine() { LineNo = 2, HarmonisedCode = "2020202020", CommercialChargeCollection = new List<UniversalCustoms.CommercialCharge>(new[] { invoiceLineDataObject2Charge }) };

			var invoiceDataObjectCharge1 = new UniversalCustoms.CommercialCharge() { ChargeType = overseasInsurance, Currency = foreignCurrency, Amount = 200m, AgreedExchangeRate = 1.5m, ExchangeRateType = fixedRate };
			var invoiceDataObjectCharge2 = new UniversalCustoms.CommercialCharge() { ChargeType = overseasFreight, Currency = foreignCurrency, Amount = 300m, AgreedExchangeRate = 1.5m, ExchangeRateType = fixedRate };

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var invoiceDataObject = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV1856",
				InvoiceAmount = 1500.32m,
				InvoiceCurrency = foreignCurrency,
				InvoiceDate = new ZDateTime(2012, 3, 25, 13, 3, 2),
				IncoTerm = new CodeDescriptionPair() { Code = Core.Constants.IncoTerms.CostInsuranceAndFreight },
				ExchangeRateType = fixedRate,
				AgreedExchangeRate = 1.5m
			};
			invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, consignor, AddressTypes.Supplier);
			invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, consignee, AddressTypes.Importer);
			invoiceDataObject.OrganizationAddressCollection = null;
			invoiceDataObject.CommercialChargeCollection = new List<UniversalCustoms.CommercialCharge>(new[] { invoiceDataObjectCharge1, invoiceDataObjectCharge2 });
			invoiceDataObject.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject1, invoiceLineDataObject2 }));

			declarationDataObject.CommercialInfo = new UniversalCustoms.CommercialInfo()
			{
				Name = "Top Group",
				CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject })
			};

			var message = GetQueuedUniversalShipmentMessage(declarationDataObject);

			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			manager.Process(message);
			var query = new ZQuery(JobComInvoiceHeaderSchema.JZ_GB, branch2.PK);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_OH_Supplier, consignor.PK);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_OH_Buyer, consignee.PK);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_StandAloneInvoiceDirection, JobMessageTypeList.Codes.Import);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_JE, null);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, "INV1856");
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_InvoiceAmount, 1500.32m);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_RX_NKInvoice_Currency, Core.Constants.CurrencyCodes.CaymanIslands);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_InvoiceDate, new ZDateTime(2012, 3, 25, 13, 3, 2));
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_IncoTerm, Core.Constants.IncoTerms.CostInsuranceAndFreight);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_InvoiceCurrExRateType, ChargeExchangeRateTypeList.Codes.FixedRate);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_InvoiceCurrExRate, 1.5m);

			var invoice = Factory.LoadTop1<BaseJobComInvoiceHeader>(query);
			AssertNotNull(invoice);
			CombineAssertions(delegate
			{
				AssertEquals("invoice.Charges.Count", 2, invoice.Charges.Count);
				AssertNotNull(invoice.Charges.OfType<BaseInvoiceCharge>().FirstOrDefault(x => x.J7_ChargeType == CustomsChargeTypeList.Codes.OverseasInsurance && x.J7_Amount == 200m && x.J7_RX_NKCurrency == Core.Constants.CurrencyCodes.CaymanIslands && x.J7_ExchangeRateType == ChargeExchangeRateTypeList.Codes.FixedRate && x.J7_ExchangeRate == 1.5m));
				AssertNotNull(invoice.Charges.OfType<BaseInvoiceCharge>().FirstOrDefault(x => x.J7_ChargeType == CustomsChargeTypeList.Codes.OverseasFreight && x.J7_Amount == 300m && x.J7_RX_NKCurrency == Core.Constants.CurrencyCodes.CaymanIslands && x.J7_ExchangeRateType == ChargeExchangeRateTypeList.Codes.FixedRate && x.J7_ExchangeRate == 1.5m));

				AssertEquals("invoice.Transports.Count", 2, invoice.Transports.Count);
				AssertNotNull(invoice.Transports.OfType<Transport>().FirstOrDefault(x => x.JW_LegOrder == 1 && x.JW_RL_NKLoadPort == "AUSYD" && x.JW_RL_NKDiscPort == "USLAX" && x.JW_ETD == new ZDateTime(2012, 3, 1) && x.JW_ETA == new ZDateTime(2012, 3, 11)));
				AssertNotNull(invoice.Transports.OfType<Transport>().FirstOrDefault(x => x.JW_LegOrder == 2 && x.JW_RL_NKLoadPort == "USLAX" && x.JW_RL_NKDiscPort == "USCHI" && x.JW_ETD == new ZDateTime(2012, 3, 12) && x.JW_ETA == new ZDateTime(2012, 3, 13)));

				AssertEquals("invoice.JobComInvoiceLines.Count", 2, invoice.JobComInvoiceLines.Count);
				var invoiceLine1 = invoice.JobComInvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_LineNo == 1 && x.JI_Tariff == "1010101010");
				AssertEquals("invoiceLine1.Charges.Count", 1, invoiceLine1.Charges.Count);
				AssertNotNull(invoiceLine1.Charges.OfType<BaseInvoiceLineCharge>().FirstOrDefault(x => x.J7_ChargeType == CustomsChargeTypeList.Codes.Commission && x.J7_Amount == 100m && x.J7_RX_NKCurrency == Core.Constants.CurrencyCodes.CaymanIslands && x.J7_ExchangeRateType == ChargeExchangeRateTypeList.Codes.FixedRate && x.J7_ExchangeRate == 1.5m));

				var invoiceLine2 = invoice.JobComInvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_LineNo == 2 && x.JI_Tariff == "2020202020");
				AssertEquals("invoiceLine2.Charges.Count", 1, invoiceLine2.Charges.Count);
				AssertNotNull(invoiceLine2.Charges.OfType<BaseInvoiceLineCharge>().FirstOrDefault(x => x.J7_ChargeType == CustomsChargeTypeList.Codes.Discount && x.J7_Amount == 150m && x.J7_RX_NKCurrency == Core.Constants.CurrencyCodes.CaymanIslands && x.J7_ExchangeRateType == ChargeExchangeRateTypeList.Codes.FixedRate && x.J7_ExchangeRate == 1.5m));

				AssertEquals("invoice.InvoiceHeaderRefs.Count", 9, invoice.InvoiceHeaderRefs.Count);
				AssertNotNull("Container CONT123ABC should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.CN && x.J2_ReferenceNumber == "CONT123ABC"));
				AssertNotNull("Container CONT456DEF should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.CN && x.J2_ReferenceNumber == "CONT456DEF"));
				AssertNotNull("MasterBill MB1 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.MB && x.J2_ReferenceNumber == "MB1"));
				AssertNotNull("MasterBill MB2 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.MB && x.J2_ReferenceNumber == "MB2"));
				AssertNotNull("HouseBill MB1HB1 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.HB && x.J2_ReferenceNumber == "MB1HB1"));
				AssertNotNull("HouseBill MB1HB2 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.HB && x.J2_ReferenceNumber == "MB1HB2"));
				AssertNotNull("HouseBill MB2HB2 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.HB && x.J2_ReferenceNumber == "MB2HB2"));
				AssertNotNull("SubHouseBill MB1HB2SB1 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.SH && x.J2_ReferenceNumber == "MB1HB2SB1"));
				AssertNotNull("SubHouseBill MB2HB2SB1 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.SH && x.J2_ReferenceNumber == "MB2HB2SB1"));
			});
		}

		public void TestManagesEventsAndShipments()
		{
			var manager = new StandaloneCommercialInvoiceDataContextManager();
			Assert(manager.ManagesEvents);
			Assert(manager.ManagesShipments);
		}

		protected override void SetupDataForDataContextManagerTestCase()
		{
			var factory = new BusinessObjectFactory();
			var importer = factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			importer.OH_FullName = "USIMP COMPÎANY NAMÎE";
			importer.OH_RL_NKClosestPort = "USLAX";
			importer.MainAddress.OA_Address1 = "USIMPÎ ADDRESS 1";
			importer.MainAddress.OA_Address2 = "USIMP ADDRESS 2";
			importer.MainAddress.OA_City = "USIMÎPCITY";
			importer.MainAddress.OA_PostCode = "90Î008";
			importer.MainAddress.OA_State = "CA";
			importer.OH_Code = "USIMP";

			var supplier = factory.New<OrgHeader>();
			supplier.OH_IsConsignor = true;
			supplier.OH_FullName = "AUEXP COMPANY NAME";
			supplier.OH_RL_NKClosestPort = "AUSYD";
			supplier.MainAddress.OA_Address1 = "AUEXP ADDRESS 1";
			supplier.MainAddress.OA_Address2 = "AUEXP ADDRESS 2";
			supplier.MainAddress.OA_City = "AUEXPCITY";
			supplier.MainAddress.OA_PostCode = "4345";
			supplier.MainAddress.OA_State = "NSW";
			supplier.OH_Code = "AUEXP";
			factory.Save();
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML => @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>CustomsCommercialInvoice</Type>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>

	<Branch>
	  <Code>SUV</Code>
	  <Name>Suva</Name>
	</Branch>
	<CommercialInfo>
	  <Name>All Invoices</Name>

	  <CommercialInvoiceCollection>
		<CommercialInvoice>
		  <InvoiceNumber>INV1856</InvoiceNumber>
		  <Buyer>
			<AddressType>Importer</AddressType>
			<AddressShortCode>USIMPÎ ADDRESS 1</AddressShortCode>
			<OrganizationCode>USIMP</OrganizationCode>
			<Address1>USIMPÎ ADDRESS 1</Address1>
			<Address2>USIMP ADDRESS 2</Address2>
			<AddressOverride>false</AddressOverride>
			<City>USIMÎPCITY</City>
			<CompanyName>USIMP COMPÎANY NAMÎE</CompanyName>
			<Contact>Bob Smith</Contact>
			<Country>
			  <Code>US</Code>
			  <Name>United States</Name>
			</Country>
			<Email>bob@usimp.com</Email>
			<Fax>+1 (801) 232-2232</Fax>
			<GovRegNum>91-013199000</GovRegNum>
			<GovRegNumType>
			  <Code>SSN</Code>
			  <Description>Social Security Number</Description>
			</GovRegNumType>
			<Mobile></Mobile>
			<Phone>+1 (801) 324-2333</Phone>
			<Port>
			  <Code>USLAX</Code>
			  <Name>Los Angeles</Name>
			</Port>
			<Postcode>90Î008</Postcode>
			<ScreeningStatus>
			  <Code>UNK</Code>
			  <Description>Unknown</Description>
			</ScreeningStatus>
			<State>CA</State>

			<RegistrationNumberCollection>
			  <RegistrationNumber>
				<CountryOfIssue>
				  <Code>US</Code>
				  <Name>United States</Name>
				</CountryOfIssue>
				<Type>
				  <Code>FEI</Code>
				  <Description>FDA Establishment Identifier</Description>
				</Type>
				<Value>45868554</Value>
			  </RegistrationNumber>
			</RegistrationNumberCollection>
		  </Buyer>
		  <ExchangeRateType>
			<Code></Code>
		  </ExchangeRateType>
		  <IncoTerm>
			<Code>CIF</Code>
			<Description>Cost, Insurance And Freight</Description>
		  </IncoTerm>
		  <InvoiceAmount>15000.0000</InvoiceAmount>
		  <InvoiceCurrency>
			<Code>AUD</Code>
			<Description>Australia, Dollars</Description>
		  </InvoiceCurrency>
		  <InvoiceDate>2012-11-13T00:00:00</InvoiceDate>
		  <LandedCostExchangeRate>1.100000000</LandedCostExchangeRate>
		  <MessageStatus>
			<Code></Code>
		  </MessageStatus>
		  <NoOfPacks>100.000</NoOfPacks>
		  <PaymentAmount>15001.0000</PaymentAmount>
		  <PaymentDate>2012-11-13T07:57:00</PaymentDate>
		  <PaymentExchangeRate>1.105000000</PaymentExchangeRate>
		  <PaymentNumber>PO342</PaymentNumber>
		  <Supplier>
			<AddressType>Supplier</AddressType>
			<AddressShortCode>AUEXP ADDRESS 1</AddressShortCode>
			<OrganizationCode>AUEXP</OrganizationCode>
			<Address1>AUEXP ADDRESS 1</Address1>
			<Address2>AUEXP ADDRESS 2</Address2>
			<AddressOverride>false</AddressOverride>
			<City>AUEXPCITY</City>
			<CompanyName>AUEXP COMPANY NAME</CompanyName>
			<Contact>MR Supplier</Contact>
			<Country>
			  <Code>AU</Code>
			  <Name>Australia</Name>
			</Country>
			<Email>supplier@auexp.com</Email>
			<Fax>+61 (2) 9845-6578</Fax>
			<Mobile></Mobile>
			<Phone>+61 (2) 9845-6579</Phone>
			<Port>
			  <Code>AUSYD</Code>
			  <Name>Sydney</Name>
			</Port>
			<Postcode>4345</Postcode>
			<ScreeningStatus>
			  <Code>UNK</Code>
			  <Description>Unknown</Description>
			</ScreeningStatus>
			<State>NSW</State>

			<RegistrationNumberCollection>
			  <RegistrationNumber>
				<CountryOfIssue>
				  <Code>US</Code>
				  <Name>United States</Name>
				</CountryOfIssue>
				<Type>
				  <Code>MID</Code>
				  <Description>Supplier/Manufacturer ID Number</Description>
				</Type>
				<Value>AUAUECOM2AUE</Value>
			  </RegistrationNumber>
			  <RegistrationNumber>
				<CountryOfIssue>
				  <Code>US</Code>
				  <Name>United States</Name>
				</CountryOfIssue>
				<Type>
				  <Code>FEI</Code>
				  <Description>FDA Establishment Identifier</Description>
				</Type>
				<Value>157</Value>
			  </RegistrationNumber>
			  <RegistrationNumber>
				<CountryOfIssue>
				  <Code>US</Code>
				  <Name>United States</Name>
				</CountryOfIssue>
				<Type>
				  <Code>PFR</Code>
				  <Description>Food Facility Registration Number</Description>
				</Type>
				<Value>23569874518</Value>
			  </RegistrationNumber>
			</RegistrationNumberCollection>
		  </Supplier>
		  <Volume>10.000</Volume>
		  <VolumeUnit>
			<Code>M3</Code>
			<Description>Cubic Meters</Description>
		  </VolumeUnit>
		  <Weight>1500.000</Weight>
		  <WeightUnit>
			<Code>KG</Code>
			<Description>Kilograms</Description>
		  </WeightUnit>

		  <CommercialChargeCollection>
			<CommercialCharge>
			  <ChargeType>
				<Code>OFT</Code>
				<Description>Overseas Freight</Description>
			  </ChargeType>
			  <AdjustedCharge>false</AdjustedCharge>
			  <Amount>150.0000</Amount>
			  <ApportionmentType>
				<Code>PAA</Code>
				<Description>Partial Apportionment: Apportion this amount to the invoices that don't have thi</Description>
			  </ApportionmentType>
			  <Currency>
				<Code>AUD</Code>
				<Description>Australia, Dollars</Description>
			  </Currency>
			  <DistributeBy>
				<Code>VAL</Code>
				<Description>Value</Description>
			  </DistributeBy>
			  <ExchangeRateType>
				<Code></Code>
			  </ExchangeRateType>
			  <IsApportionedCharge>false</IsApportionedCharge>
			  <IsDutiable>false</IsDutiable>
			  <IsGSTApplicable>true</IsGSTApplicable>
			  <IsIncludedInITOT>false</IsIncludedInITOT>
			  <IsNotIncludedInInvoice>false</IsNotIncludedInInvoice>
			  <PercentageOfLinePrice>0.000</PercentageOfLinePrice>
			  <PrepaidCollect>
				<Code>PPD</Code>
				<Description>Prepaid</Description>
			  </PrepaidCollect>
			</CommercialCharge>
			<CommercialCharge>
			  <ChargeType>
				<Code>ONS</Code>
				<Description>Overseas Insurance</Description>
			  </ChargeType>
			  <AdjustedCharge>false</AdjustedCharge>
			  <Amount>100.0000</Amount>
			  <ApportionmentType>
				<Code>PAA</Code>
				<Description>Partial Apportionment: Apportion this amount to the invoices that don't have thi</Description>
			  </ApportionmentType>
			  <Currency>
				<Code>AUD</Code>
				<Description>Australia, Dollars</Description>
			  </Currency>
			  <DistributeBy>
				<Code>VAL</Code>
				<Description>Value</Description>
			  </DistributeBy>
			  <ExchangeRateType>
				<Code></Code>
			  </ExchangeRateType>
			  <IsApportionedCharge>false</IsApportionedCharge>
			  <IsDutiable>false</IsDutiable>
			  <IsGSTApplicable>true</IsGSTApplicable>
			  <IsIncludedInITOT>false</IsIncludedInITOT>
			  <IsNotIncludedInInvoice>false</IsNotIncludedInInvoice>
			  <PercentageOfLinePrice>0.000</PercentageOfLinePrice>
			  <PrepaidCollect>
				<Code>PPD</Code>
				<Description>Prepaid</Description>
			  </PrepaidCollect>
			</CommercialCharge>
		  </CommercialChargeCollection>

		  <CommercialInvoiceLineCollection>
			<CommercialInvoiceLine>
			  <LineNo>1</LineNo>
			  <BondedWarehouseQuantity>0.00000</BondedWarehouseQuantity>
			  <BondedWarehouseQuantityUnit>
				<Code></Code>
			  </BondedWarehouseQuantityUnit>
			  <ClassificationCode></ClassificationCode>
			  <Commodity>
				<Code></Code>
			  </Commodity>
			  <ContainerMode>
				<Code></Code>
			  </ContainerMode>
			  <CustomsQuantity>0.00000</CustomsQuantity>
			  <CustomsQuantityUnit>
				<Code></Code>
			  </CustomsQuantityUnit>
			  <Description>GOODS</Description>
			  <HarmonisedCode>1010101010</HarmonisedCode>
			  <HazardousMaterial>
				<Code></Code>
				<CodeType>
				  <Code></Code>
				</CodeType>
			  </HazardousMaterial>
			  <InvoiceQuantity>1000.00000</InvoiceQuantity>
			  <InvoiceQuantityUnit>
				<Code>PAI</Code>
				<Description>Pail</Description>
			  </InvoiceQuantityUnit>
			  <LinePrice>15000.0000</LinePrice>
			  <NetWeight>0.000</NetWeight>
			  <NetWeightUnit>
				<Code>KG</Code>
				<Description>Kilograms</Description>
			  </NetWeightUnit>
			  <OrderLineLink>0</OrderLineLink>
			  <OrderNumber></OrderNumber>
			  <PartNo></PartNo>
			  <Volume>0.000</Volume>
			  <VolumeUnit>
				<Code>M3</Code>
				<Description>Cubic Meters</Description>
			  </VolumeUnit>
			  <Weight>0.000</Weight>
			  <WeightUnit>
				<Code>KG</Code>
				<Description>Kilograms</Description>
			  </WeightUnit>
			</CommercialInvoiceLine>
		  </CommercialInvoiceLineCollection>
		</CommercialInvoice>
	  </CommercialInvoiceCollection>
	</CommercialInfo>
	<MessageType>
	  <Code>IMP</Code>
	  <Description>Import</Description>
	</MessageType>

	<AdditionalBillCollection>
	  <AdditionalBill>
		<BillNumber>MB1HB1SH1</BillNumber>
		<BillType>
		  <Code>SWB</Code>
		  <Description>Sub-House Waybill</Description>
		</BillType>
		<IssueDate></IssueDate>
		<MessageStatus>
		  <Code></Code>
		</MessageStatus>
		<NoOfPacks>0.0000</NoOfPacks>
		<PackType>
		  <Code></Code>
		</PackType>
		<ParentBillNumber>MB1HB1</ParentBillNumber>
	  </AdditionalBill>
	  <AdditionalBill>
		<BillNumber>MB1</BillNumber>
		<BillType>
		  <Code>MWB</Code>
		  <Description>Master Waybill</Description>
		</BillType>
		<IssueDate></IssueDate>
		<MessageStatus>
		  <Code></Code>
		</MessageStatus>
		<NoOfPacks>0.0000</NoOfPacks>
		<PackType>
		  <Code></Code>
		</PackType>
		<ParentBillNumber></ParentBillNumber>
	  </AdditionalBill>
	  <AdditionalBill>
		<BillNumber>MB2</BillNumber>
		<BillType>
		  <Code>MWB</Code>
		  <Description>Master Waybill</Description>
		</BillType>
		<IssueDate></IssueDate>
		<MessageStatus>
		  <Code></Code>
		</MessageStatus>
		<NoOfPacks>0.0000</NoOfPacks>
		<PackType>
		  <Code></Code>
		</PackType>
		<ParentBillNumber></ParentBillNumber>
	  </AdditionalBill>
	  <AdditionalBill>
		<BillNumber>MB2HB2</BillNumber>
		<BillType>
		  <Code>HWB</Code>
		  <Description>House Waybill</Description>
		</BillType>
		<IssueDate></IssueDate>
		<MessageStatus>
		  <Code></Code>
		</MessageStatus>
		<NoOfPacks>0.0000</NoOfPacks>
		<PackType>
		  <Code></Code>
		</PackType>
		<ParentBillNumber>MB2</ParentBillNumber>
	  </AdditionalBill>
	  <AdditionalBill>
		<BillNumber>MB1HB1</BillNumber>
		<BillType>
		  <Code>HWB</Code>
		  <Description>House Waybill</Description>
		</BillType>
		<IssueDate></IssueDate>
		<MessageStatus>
		  <Code></Code>
		</MessageStatus>
		<NoOfPacks>0.0000</NoOfPacks>
		<PackType>
		  <Code></Code>
		</PackType>
		<ParentBillNumber>MB1</ParentBillNumber>
	  </AdditionalBill>
	  <AdditionalBill>
		<BillNumber>MB2HB2SH2</BillNumber>
		<BillType>
		  <Code>SWB</Code>
		  <Description>Sub-House Waybill</Description>
		</BillType>
		<IssueDate></IssueDate>
		<MessageStatus>
		  <Code></Code>
		</MessageStatus>
		<NoOfPacks>0.0000</NoOfPacks>
		<PackType>
		  <Code></Code>
		</PackType>
		<ParentBillNumber>MB2HB2</ParentBillNumber>
	  </AdditionalBill>
	</AdditionalBillCollection>

	<ContainerCollection>
	  <Container>
		<AirVentFlow>0.0</AirVentFlow>
		<AirVentFlowRateUnit>
		  <Code></Code>
		</AirVentFlowRateUnit>
		<ArrivalCartageAdvised></ArrivalCartageAdvised>
		<ArrivalCartageComplete></ArrivalCartageComplete>
		<ArrivalCartageDemurrageCharge>0.0000</ArrivalCartageDemurrageCharge>
		<ArrivalCartageDemurrageTime></ArrivalCartageDemurrageTime>
		<ArrivalCartageRef></ArrivalCartageRef>
		<ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
		<ArrivalEstimatedDelivery></ArrivalEstimatedDelivery>
		<ArrivalPickupByRail>false</ArrivalPickupByRail>
		<ArrivalSlotDateTime></ArrivalSlotDateTime>
		<ArrivalSlotReference></ArrivalSlotReference>
		<Commodity>
		  <Code></Code>
		</Commodity>
		<ContainerDetentionCharge>0.0000</ContainerDetentionCharge>
		<ContainerDetentionDays>0</ContainerDetentionDays>
		<ContainerImportDORelease></ContainerImportDORelease>
		<ContainerNumber>CONT123ABC</ContainerNumber>
		<ContainerParkEmptyPickupGateOut></ContainerParkEmptyPickupGateOut>
		<ContainerParkEmptyReturnGateIn></ContainerParkEmptyReturnGateIn>
		<ContainerQuality>
		  <Code></Code>
		</ContainerQuality>
		<ContainerStatus>
		  <Code></Code>
		</ContainerStatus>
		<ContainerType>
		  <Code>40FR</Code>
		  <Description>Forty foot flatrack</Description>
		  <ISOCode>42P1</ISOCode>
		</ContainerType>
		<CustomsContainerSize>
		  <Code></Code>
		</CustomsContainerSize>
		<DeliveryMode></DeliveryMode>
		<DeliverySequence>0</DeliverySequence>
		<DepartureCartageAdvised></DepartureCartageAdvised>
		<DepartureCartageComplete></DepartureCartageComplete>
		<DepartureCartageDemurrageCharge>0.0000</DepartureCartageDemurrageCharge>
		<DepartureCartageDemurrageTime></DepartureCartageDemurrageTime>
		<DepartureCartageRef></DepartureCartageRef>
		<DepartureDeliveryByRail>false</DepartureDeliveryByRail>
		<DepartureDockReceipt></DepartureDockReceipt>
		<DepartureEstimatedPickup></DepartureEstimatedPickup>
		<DepartureSlotDateTime></DepartureSlotDateTime>
		<DepartureSlotReference></DepartureSlotReference>
		<DunnageWeight>0.000</DunnageWeight>
		<EmptyReadyForReturn></EmptyReadyForReturn>
		<EmptyRequired></EmptyRequired>
		<EmptyReturnedBy></EmptyReturnedBy>
		<EmptyReturnRef></EmptyReturnRef>
		<ExportDepotCustomsReference></ExportDepotCustomsReference>
		<FCL_LCL_AIR>
		  <Code>FCL</Code>
		  <Description>Full Container Load</Description>
		</FCL_LCL_AIR>
		<FCLAvailable></FCLAvailable>
		<FCLHeldInTransitStaging>false</FCLHeldInTransitStaging>
		<FCLOnBoardVessel></FCLOnBoardVessel>
		<FCLStorageArrivedUnderbond>false</FCLStorageArrivedUnderbond>
		<FCLStorageCharge>0.0000</FCLStorageCharge>
		<FCLStorageCommences></FCLStorageCommences>
		<FCLStorageDays>0</FCLStorageDays>
		<FCLStorageModuleOnlyMaster></FCLStorageModuleOnlyMaster>
		<FCLStorageUnderbondCleared></FCLStorageUnderbondCleared>
		<FCLUnloadFromVessel></FCLUnloadFromVessel>
		<FCLWharfGateIn></FCLWharfGateIn>
		<FCLWharfGateOut></FCLWharfGateOut>
		<GoodsValue>0.0000</GoodsValue>
		<GoodsValueCurrency>
		  <Code></Code>
		</GoodsValueCurrency>
		<GoodsWeight>2000.000</GoodsWeight>
		<GrossWeight>7530.000</GrossWeight>
		<HumidityPercent>0</HumidityPercent>
		<IsCFSRegistered>false</IsCFSRegistered>
		<IsControlledAtmosphere>false</IsControlledAtmosphere>
		<IsDamaged>false</IsDamaged>
		<IsEmptyContainer>false</IsEmptyContainer>
		<IsSealOk>true</IsSealOk>
		<IsShipperOwned>false</IsShipperOwned>
		<LCLAvailable></LCLAvailable>
		<LCLStorageCommences></LCLStorageCommences>
		<LCLUnpack></LCLUnpack>
		<LengthUnit>
		  <Code>FT</Code>
		  <Description>Feet</Description>
		</LengthUnit>
		<MessageStatus>
		  <Code></Code>
		</MessageStatus>
		<OverhangBack>0.000</OverhangBack>
		<OverhangFront>0</OverhangFront>
		<OverhangHeight>0</OverhangHeight>
		<OverhangLeft>0</OverhangLeft>
		<OverhangRight>0.000</OverhangRight>
		<OverrideFCLAvailableStorage>false</OverrideFCLAvailableStorage>
		<OverrideLCLAvailableStorage>false</OverrideLCLAvailableStorage>
		<PackDate></PackDate>
		<RefrigGeneratorID></RefrigGeneratorID>
		<ReleaseNum></ReleaseNum>
		<Seal>SN1</Seal>
		<SecondSeal></SecondSeal>
		<SetPointTemp>0.000</SetPointTemp>
		<SetPointTempUnit>C</SetPointTempUnit>
		<StowagePosition></StowagePosition>
		<TareWeight>5530.000</TareWeight>
		<TempRecorderSerialNo></TempRecorderSerialNo>
		<ThirdSeal></ThirdSeal>
		<TotalHeight>8.500</TotalHeight>
		<TotalLength>40.000</TotalLength>
		<TotalWidth>8.000</TotalWidth>
		<TrainWagonNumber></TrainWagonNumber>
		<UnpackGang></UnpackGang>
		<UnpackShed></UnpackShed>
		<VolumeCapacity>0.000</VolumeCapacity>
		<VolumeUnit>
		  <Code>M3</Code>
		  <Description>Cubic Meters</Description>
		</VolumeUnit>
		<WeightCapacity>0.000</WeightCapacity>
		<WeightUnit>
		  <Code>KG</Code>
		  <Description>Kilograms</Description>
		</WeightUnit>
	  </Container>
	  <Container>
		<AirVentFlow>0.0</AirVentFlow>
		<AirVentFlowRateUnit>
		  <Code></Code>
		</AirVentFlowRateUnit>
		<ArrivalCartageAdvised></ArrivalCartageAdvised>
		<ArrivalCartageComplete></ArrivalCartageComplete>
		<ArrivalCartageDemurrageCharge>0.0000</ArrivalCartageDemurrageCharge>
		<ArrivalCartageDemurrageTime></ArrivalCartageDemurrageTime>
		<ArrivalCartageRef></ArrivalCartageRef>
		<ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
		<ArrivalEstimatedDelivery></ArrivalEstimatedDelivery>
		<ArrivalPickupByRail>false</ArrivalPickupByRail>
		<ArrivalSlotDateTime></ArrivalSlotDateTime>
		<ArrivalSlotReference></ArrivalSlotReference>
		<Commodity>
		  <Code></Code>
		</Commodity>
		<ContainerDetentionCharge>0.0000</ContainerDetentionCharge>
		<ContainerDetentionDays>0</ContainerDetentionDays>
		<ContainerImportDORelease></ContainerImportDORelease>
		<ContainerNumber>CONT456DEF</ContainerNumber>
		<ContainerParkEmptyPickupGateOut></ContainerParkEmptyPickupGateOut>
		<ContainerParkEmptyReturnGateIn></ContainerParkEmptyReturnGateIn>
		<ContainerQuality>
		  <Code></Code>
		</ContainerQuality>
		<ContainerStatus>
		  <Code></Code>
		</ContainerStatus>
		<ContainerType>
		  <Code>2008</Code>
		  <Description>20 FT LONG X 8 FT WIDE X 8 FT HIGH</Description>
		  <ISOCode>22G1</ISOCode>
		</ContainerType>
		<CustomsContainerSize>
		  <Code></Code>
		</CustomsContainerSize>
		<DeliveryMode></DeliveryMode>
		<DeliverySequence>0</DeliverySequence>
		<DepartureCartageAdvised></DepartureCartageAdvised>
		<DepartureCartageComplete></DepartureCartageComplete>
		<DepartureCartageDemurrageCharge>0.0000</DepartureCartageDemurrageCharge>
		<DepartureCartageDemurrageTime></DepartureCartageDemurrageTime>
		<DepartureCartageRef></DepartureCartageRef>
		<DepartureDeliveryByRail>false</DepartureDeliveryByRail>
		<DepartureDockReceipt></DepartureDockReceipt>
		<DepartureEstimatedPickup></DepartureEstimatedPickup>
		<DepartureSlotDateTime></DepartureSlotDateTime>
		<DepartureSlotReference></DepartureSlotReference>
		<DunnageWeight>0.000</DunnageWeight>
		<EmptyReadyForReturn></EmptyReadyForReturn>
		<EmptyRequired></EmptyRequired>
		<EmptyReturnedBy></EmptyReturnedBy>
		<ExportDepotCustomsReference></ExportDepotCustomsReference>
		<FCL_LCL_AIR>
		  <Code>LCL</Code>
		  <Description>Less Container Load</Description>
		</FCL_LCL_AIR>
		<FCLAvailable></FCLAvailable>
		<FCLHeldInTransitStaging>false</FCLHeldInTransitStaging>
		<FCLOnBoardVessel></FCLOnBoardVessel>
		<FCLStorageArrivedUnderbond>false</FCLStorageArrivedUnderbond>
		<FCLStorageCharge>0.0000</FCLStorageCharge>
		<FCLStorageCommences></FCLStorageCommences>
		<FCLStorageDays>0</FCLStorageDays>
		<FCLStorageModuleOnlyMaster></FCLStorageModuleOnlyMaster>
		<FCLStorageUnderbondCleared></FCLStorageUnderbondCleared>
		<FCLUnloadFromVessel></FCLUnloadFromVessel>
		<FCLWharfGateIn></FCLWharfGateIn>
		<FCLWharfGateOut></FCLWharfGateOut>
		<GoodsValue>0.0000</GoodsValue>
		<GoodsValueCurrency>
		  <Code></Code>
		</GoodsValueCurrency>
		<GoodsWeight>1000.000</GoodsWeight>
		<GrossWeight>1000.000</GrossWeight>
		<HumidityPercent>0</HumidityPercent>
		<IsCFSRegistered>false</IsCFSRegistered>
		<IsControlledAtmosphere>false</IsControlledAtmosphere>
		<IsDamaged>false</IsDamaged>
		<IsEmptyContainer>false</IsEmptyContainer>
		<IsSealOk>true</IsSealOk>
		<IsShipperOwned>false</IsShipperOwned>
		<LCLAvailable></LCLAvailable>
		<LCLStorageCommences></LCLStorageCommences>
		<LCLUnpack></LCLUnpack>
		<LengthUnit>
		  <Code>FT</Code>
		  <Description>Feet</Description>
		</LengthUnit>
		<MessageStatus>
		  <Code></Code>
		</MessageStatus>
		<OverhangBack>0.000</OverhangBack>
		<OverhangFront>0</OverhangFront>
		<OverhangHeight>0</OverhangHeight>
		<OverhangLeft>0</OverhangLeft>
		<OverhangRight>0.000</OverhangRight>
		<OverrideFCLAvailableStorage>false</OverrideFCLAvailableStorage>
		<OverrideLCLAvailableStorage>false</OverrideLCLAvailableStorage>
		<PackDate></PackDate>
		<RefrigGeneratorID></RefrigGeneratorID>
		<ReleaseNum></ReleaseNum>
		<Seal>SN2</Seal>
		<SecondSeal></SecondSeal>
		<SetPointTemp>0.000</SetPointTemp>
		<SetPointTempUnit>C</SetPointTempUnit>
		<StowagePosition></StowagePosition>
		<TareWeight>0.000</TareWeight>
		<TempRecorderSerialNo></TempRecorderSerialNo>
		<ThirdSeal></ThirdSeal>
		<TotalHeight>0.000</TotalHeight>
		<TotalLength>0.000</TotalLength>
		<TotalWidth>0.000</TotalWidth>
		<TrainWagonNumber></TrainWagonNumber>
		<UnpackGang></UnpackGang>
		<UnpackShed></UnpackShed>
		<VolumeCapacity>0.000</VolumeCapacity>
		<VolumeUnit>
		  <Code>M3</Code>
		  <Description>Cubic Meters</Description>
		</VolumeUnit>
		<WeightCapacity>0.000</WeightCapacity>
		<WeightUnit>
		  <Code>KG</Code>
		  <Description>Kilograms</Description>
		</WeightUnit>
	  </Container>
	</ContainerCollection>
	<TransportLegCollection>
	  <TransportLeg>
		<PortOfDischarge>
		  <Code>USLAX</Code>
		  <Name>Los Angeles</Name>
		</PortOfDischarge>
		<PortOfLoading>
		  <Code>AUSYD</Code>
		  <Name>Sydney</Name>
		</PortOfLoading>
		<LegOrder>0</LegOrder>
		<TransportMode>Sea</TransportMode>
		<ActualArrival></ActualArrival>
		<ActualDeparture></ActualDeparture>
		<CarrierBookingReference></CarrierBookingReference>
		<CarrierServiceLevel>
		  <Code></Code>
		</CarrierServiceLevel>
		<EstimatedArrival>2012-11-24T04:16:00</EstimatedArrival>
		<EstimatedDeparture>2012-11-14T04:16:00</EstimatedDeparture>
		<LegType>Main</LegType>
		<VesselLloydsIMO>7819369</VesselLloydsIMO>
		<VesselName>APL EMERALD</VesselName>
		<VoyageFlightNo>315</VoyageFlightNo>
	  </TransportLeg>
	  <TransportLeg>
		<PortOfDischarge>
		  <Code>USCHI</Code>
		  <Name>Chicago</Name>
		</PortOfDischarge>
		<PortOfLoading>
		  <Code>USLAX</Code>
		  <Name>Los Angeles</Name>
		</PortOfLoading>
		<LegOrder>0</LegOrder>
		<TransportMode>Road</TransportMode>
		<ActualArrival></ActualArrival>
		<ActualDeparture></ActualDeparture>
		<CarrierBookingReference></CarrierBookingReference>
		<CarrierServiceLevel>
		  <Code></Code>
		</CarrierServiceLevel>
		<EstimatedArrival>2012-11-26T04:16:00</EstimatedArrival>
		<EstimatedDeparture>2012-11-25T04:16:00</EstimatedDeparture>
		<LegType>Other</LegType>
		<VesselLloydsIMO></VesselLloydsIMO>
		<VesselName></VesselName>
		<VoyageFlightNo>TR34</VoyageFlightNo>
	  </TransportLeg>
	</TransportLegCollection>
  </Shipment>
</UniversalShipment>
";

		void AssertHasBill(BaseJobComInvoiceHeader[] invoices, ZString invoiceNumber, ZString containerNumber)
		{
			var invoice = invoices.FirstOrDefault(x => x.JZ_InvoiceNumber == invoiceNumber);
			AssertNotNull("There should be one invoice with number " + invoiceNumber, invoice);
			AssertNotNull(string.Format("Invoice ({0}) should have bill reference ({1})", invoiceNumber, containerNumber), invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.HB && x.J2_ReferenceNumber == containerNumber));
		}
	}
}
