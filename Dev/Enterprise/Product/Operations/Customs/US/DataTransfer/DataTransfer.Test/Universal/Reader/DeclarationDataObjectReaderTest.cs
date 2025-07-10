using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	partial class DeclarationDataObjectReaderTest : Customs.DataTransfer.Universal.Testing.DataObjectReaderTest
	{
		public void TestInvoicerAddressAndFDAShipperAdddress()
		{
			var org1 = CreateOrganisation("BOB", "ABC!@#1");

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
			declarationDataObject.PaymentMethod = null;
			declarationDataObject.AddOrgAddress(writeManager, org1, Constants.AddressType.Invoicer);
			var invoiceHeaderDataObject = SetupCommercialInvoiceHeaderData(null, null);
			invoiceHeaderDataObject.AddOrgAddress(writeManager, org1, Constants.AddressType.Invoicer);
			invoiceHeaderDataObject.AddOrgAddress(writeManager, org1, Constants.AddressType.FDAShipper);
			declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group", null, new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceHeaderDataObject }), null);

			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				var invoice = declarationBO.Invoices.OfType<JobComInvoiceHeader>().First();
				AssertEquals(org1.MainAddress.PK, declarationBO.JE_OA_InvoicerAddress);
				AssertEquals(org1.MainAddress.PK, declarationBO.InvoicerDocumentaryAddress.E2_OA_Address);
				AssertEquals(org1.MainAddress.PK, invoice.JZ_OA_InvoicerDocAddress);
				AssertEquals(ZGuid.Empty, invoice.InvoicerDocumentaryAddress.E2_OA_Address);
				AssertEquals(org1.MainAddress.PK, invoice.JZ_OA_FDAShipperAddress);
				AssertEquals(ZGuid.Empty, invoice.FDAShipperDocumentaryAddress.E2_OA_Address);
			}

			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				var invoice = declarationBO.Invoices.OfType<JobComInvoiceHeader>().First();
				AssertEquals(org1.MainAddress.PK, declarationBO.JE_OA_InvoicerAddress);
				AssertEquals(org1.MainAddress.PK, declarationBO.InvoicerDocumentaryAddress.E2_OA_Address);
				AssertEquals(org1.MainAddress.PK, invoice.JZ_OA_InvoicerDocAddress);
				AssertEquals(ZGuid.Empty, invoice.InvoicerDocumentaryAddress.E2_OA_Address);
				AssertEquals(org1.MainAddress.PK, invoice.JZ_OA_FDAShipperAddress);
				AssertEquals(ZGuid.Empty, invoice.FDAShipperDocumentaryAddress.E2_OA_Address);
			}
		}

		public void TestInvoicerAddressAndFDAShipperAdddress_HasInvoicerAddress()
		{
			var org1 = CreateOrganisation("BOB", "ABC!@#1");

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
			declarationDataObject.PaymentMethod = null;
			declarationDataObject.AddOrgAddress(writeManager, org1, DocAddressType.InvoicerAddress);
			declarationDataObject.AddOrgAddress(writeManager, org1, Constants.AddressType.Invoicer);
			var invoiceHeaderDataObject = SetupCommercialInvoiceHeaderData(null, null);
			invoiceHeaderDataObject.AddOrgAddress(writeManager, org1, Constants.AddressType.Invoicer);
			invoiceHeaderDataObject.AddOrgAddress(writeManager, org1, Constants.AddressType.FDAShipper);
			declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group", null, new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceHeaderDataObject }), null);

			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				var invoice = declarationBO.Invoices.OfType<JobComInvoiceHeader>().First();
				AssertEquals(org1.MainAddress.PK, declarationBO.JE_OA_InvoicerAddress);
				AssertEquals(org1.MainAddress.PK, declarationBO.InvoicerDocumentaryAddress.E2_OA_Address);
				AssertEquals(org1.MainAddress.PK, invoice.JZ_OA_InvoicerDocAddress);
				AssertEquals(ZGuid.Empty, invoice.InvoicerDocumentaryAddress.E2_OA_Address);
				AssertEquals(org1.MainAddress.PK, invoice.JZ_OA_FDAShipperAddress);
				AssertEquals(ZGuid.Empty, invoice.FDAShipperDocumentaryAddress.E2_OA_Address);
			}

			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				var invoice = declarationBO.Invoices.OfType<JobComInvoiceHeader>().First();
				AssertEquals(org1.MainAddress.PK, declarationBO.JE_OA_InvoicerAddress);
				AssertEquals(org1.MainAddress.PK, declarationBO.InvoicerDocumentaryAddress.E2_OA_Address);
				AssertEquals(org1.MainAddress.PK, invoice.JZ_OA_InvoicerDocAddress);
				AssertEquals(ZGuid.Empty, invoice.InvoicerDocumentaryAddress.E2_OA_Address);
				AssertEquals(org1.MainAddress.PK, invoice.JZ_OA_FDAShipperAddress);
				AssertEquals(ZGuid.Empty, invoice.FDAShipperDocumentaryAddress.E2_OA_Address);
			}
		}

		public void TestImportTariffAfterCOOAndCOE()
		{
			var factory = new BusinessObjectFactory();
			var startDate = ZDateTime.Today.AddMonths(-5);
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var uscTariff = Factory.New<USCTariff>();
			uscTariff.UE_Tariff = "3333333333";
			uscTariff.UE_DateFrom = startDate;
			uscTariff.UE_DateTo = endDate;
			var helper = new Customs.Universal.Testing.UniversalReferenceTestDataHelper(factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff7217 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "3333333333", startDate, endDate);
			var tariff99With301 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "5555555555", startDate, endDate);
			var tariff99With232 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7777777777", startDate, endDate);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.GAE, tariff7217);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99With301);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._301, tariff99With301);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99With232);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99With232);
			helper.CreateTariffRelationship(tariff99With301.PK, tariffType.PK, tariff7217.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariff99With232.PK, tariffType.PK, tariff7217.ZZ1_TariffCode);

			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Enterprise.Customs.Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(factory, Enterprise.Customs.Universal.Constants.RateTypes.Duty, dutyRateType.PK);
			var rate3 = helper.CreateRate(tariff99With301, rateCode.PK, startDate, endDate, "0");
			var rate4 = helper.CreateRate(tariff99With232, rateCode.PK, startDate, endDate, "0");

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.HongKong, Core.Constants.CountryCodes.HongKong, startDate, endDate);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.HongKong, startDate.Date, endDate.Date);
			helper.CreateCusApplicability(rate3, tradeGroup, startDate, endDate);
			helper.CreateCusApplicability(rate4, tradeGroup, startDate, endDate);
			factory.Save();

			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
				declarationDataObject.MessageType = new CodeDescriptionPair() { Code = USJobMessageTypeList.Codes.Import };
				declarationDataObject.MessagingApplicationCode = new CodeDescriptionPair() { Code = "ACE" };

				var invoiceLineDataObject = SetupCommercialInvoiceLine(1, null, null);
				invoiceLineDataObject.AddInfoCollection = AddInfoCollectionCreator.CreateCollection("UC_NKCountryOfOrigin=CN");
				invoiceLineDataObject.HarmonisedCode = "3333333333";
				var invoiceHeaderData = SetupCommercialInvoiceHeaderData(null, null, new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject }));
				invoiceHeaderData.AddInfoCollection = AddInfoCollectionCreator.CreateCollection("UC_NKCountryOfOrigin=HK");
				declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group", null, new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceHeaderData }));

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				var invoiceLine = declarationBO.InvoiceLines[0];
				AssertEquals("", invoiceLine.US_SupTariff);
			}
		}

		public void TestFTZNoShouldBeUpdated_WhenMasterBillIsNotEmptyAndFTZNoIsEmpty()
		{
			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.WayBillNumber = "1234567";
			declarationDataObject.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = USJobMessageTypeList.Codes.Import };
			declarationDataObject.SetAddInfoCollection(() => new List<AddInfo>());
			declarationDataObject.AddInfoCollection.Add(new AddInfo() { Key = "EntryType", Value = "06" });

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("1234567", declarationBO.US_FTZNo);
		}

		public void TestContainerAddInfoIsNotSupported()
		{
			var declaration = Factory.New<JobDeclaration>();
			var containerDataObject = SetupContainer();
			containerDataObject.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(string.Format("{0}={1}*{2}={3}*{4}={5}*DUMMY=3",
				"InBondArrivalDate", AutoUSAddInfo.GetStringRepresentation(new ZDateTime(2012, 3, 2)),
				"CargoStorageCode", "D",
				"SchDINBArrival", "2689")));

			var declarationDataObject = SetupDeclaration(null, "MYHOUSE1", new WayBillType() { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House });
			declarationDataObject.SetContainerCollection(() => new DataObjectList<Container>(new[] { containerDataObject }));

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			declarationBO.CusContainers.Load();
			AssertEquals(1, declarationBO.CusContainers.Count);
			var containerBO = declarationBO.CusContainers[0];
			var addInfos = AddInfoParser.CreateDictionaryWithAddInfoString(containerBO.CO_AddInfo);
			AssertEquals(0, addInfos.Count);
		}

		public void TestGetAddInfosThatShouldNotBeImported()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.JE_MasterBill = "MB2343";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			Factory.SaveForTesting();

			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.GoodsDescription = "TEST ADDINFO";
			declarationDataObject.MessagingApplicationCode = new CodeDescriptionPair() { Code = "ACE" };
			var invoiceLineDataObject = SetupCommercialInvoiceLine(1, null, null);
			invoiceLineDataObject.LinePrice = 1500m;
			invoiceLineDataObject.Weight = 9m;
			invoiceLineDataObject.AddInfoCollection = AddInfoCollectionCreator.CreateCollection(string.Format("{0}={1}", USAddInfoSchema.US_LicenseNo.Name.Substring(3), "T0001"));
			declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group", null, new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { SetupCommercialInvoiceHeaderData(null, null, new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject })) }), null);
			declarationDataObject.SetAddInfoCollection(() => new List<AddInfo>());
			declarationDataObject.AddInfoCollection.Add(new AddInfo() { Key = "IsInvoiceByRequest", Value = "Y" });
			declarationDataObject.AddInfoCollection.Add(new AddInfo() { Key = "EnableAII", Value = "Y" });
			declarationDataObject.AddInfoCollection.Add(new AddInfo() { Key = "CargoReleaseType", Value = "ACE" });

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("The Job updated", declarationBO.JE_GoodsDescription, "TEST ADDINFO");
		}

		public void TestImportInvalidEntryNumberShouldBeRejected()
		{
			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = USJobMessageTypeList.Codes.Import };
			declarationDataObject.GoodsDescription = "Rejected";
			var entryNumber = new EntryNumber
			{
				CountryOfIssue = new Country
				{
					Code = "US"
				},
				Type = new EntryType
				{
					Code = CusEntryHeaderMessageTypeList.Codes.EntrySummary
				},
				Number = "11122233300012"
			};
			declarationDataObject.SetEntryNumberCollection(() => new List<EntryNumber>
			{
				entryNumber
			});

			Factory.SaveForTesting();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertContains("The data import will not proceed because the entry number in this file is not valid.", logger.Logs);
		}

		public void TestImportReconDeclarationShouldCreateNewEntryHeader()
		{
			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = USJobMessageTypeList.Codes.Recon };

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBo = reader.ReadIntoBusinessObject();

			Assert("It should create at least one entry header if the daclaration is ReconDeclaration.", declarationBo.ActiveEntryHeaders.Cast<CusEntryHeader>().Any(x => x.CH_MessageType == CusEntryHeaderMessageTypeList.Codes.ReconEntry));
		}

		public void TestUpdateMessageType()
		{
			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
			var declarationBOToLoad = Factory.New<JobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Export;
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			var entry = declarationBOToLoad.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entry.EntryNumber = "50689378";
			var ediMessage = entry.Messages.AddNew(typeof(MQEDIMessage));
			ediMessage.EM_MessageNum = "123400001";
			ediMessage.EM_MessageType = Messaging.Business.MessageBuildingBlocks.ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse;
			ediMessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;

			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationDataObject.GoodsDescription = "ABC";
			Factory.SaveForTesting();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("messageType should not updated, because the message type does not match the current message type", JobMessageTypeList.Codes.Export, declarationBO.JE_MessageType);
			AssertEquals(declarationBO.JE_GoodsDescription, ZString.Empty);
			AssertContains("Cannot change declaration message type once messaging has started", logger.Logs);

			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
			declarationDataObject.GoodsDescription = "ABC";
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationBOToLoad.Messages.RemoveAndDeleteAll();
			Factory.SaveForTesting();
			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("The data should be imported", declarationBO.JE_GoodsDescription, "ABC");
		}

		public void TestImportingShipmentDeclarationDoesNotTickOverrideIfNotNeeded()
		{
			var oldValue = GlbDepartment.CurrentDepartment.GE_Import;
			try
			{
				GlbDepartment.CurrentDepartment.GE_Import = ZBool.True;
				var refContainer = Factory.New<RefContainer>();
				refContainer.RC_Code = "40!@";

				eAdaptorRegistry.Instance.UseBrokerageDataFirstWhenExportUniversalXML.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_MasterBillNum = "OB1510161200";

				var org = Factory.New<OrgHeader>();
				org.OH_Code = "aa1";
				org.OH_FullName = "bb1";
				var cusCode = org.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
				cusCode.OK_CustomsRegNo = "XXXD";
				cusCode.OK_RN_NKCodeCountry = "US";
				consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
				consol.JK_OA_ShippingLineAddress = org.MainAddress.PK;

				var container1 = consol.Containers.AddNew();
				container1.JC_ContainerNum = "CONT1";
				container1.JC_RC = refContainer.PK;

				var container2 = consol.Containers.AddNew();
				container2.JC_ContainerNum = "CONT2";
				container2.JC_RC = refContainer.PK;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_HouseBill = "HB1510161200";
				shipment.JS_RL_NKOrigin = "AUMEL";
				shipment.JS_RL_NKDestination = "USCHI";

				var org2 = Factory.New<OrgHeader>();
				org2.OH_Code = "aa2";
				org2.OH_FullName = "bb2";
				var cusCode2 = org2.CustomsCodes.AddNew();
				cusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
				cusCode2.OK_CustomsRegNo = "APLU";
				cusCode2.OK_RN_NKCodeCountry = "US";
				shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org2.PK;

				var packingLine1 = shipment.OuterPackLines.AddNew();
				packingLine1.SetContainer(consol, container1);

				var packingLine2 = shipment.OuterPackLines.AddNew();
				packingLine2.SetContainer(consol, container2);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.ShipmentSynchroniser.Synchronise(true);
				if (declaration.Forwarder != null)
				{
					declaration.Forwarder.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
				}
				AssertEquals("declaration.Bills.Count", 2, declaration.Bills.Count);
				AssertEquals("declaration.JE_MasterBill", "OB1510161200", declaration.JE_MasterBill);
				AssertEquals("declaration.JE_MasterBillIssuerSCAC", "XXXD", declaration.JE_MasterBillIssuerSCAC);
				AssertEquals("declaration.JE_HouseBill", "HB1510161200", declaration.JE_HouseBill);
				AssertEquals("declaration.JE_MasterBillIssuerSCAC", "APLU", declaration.JE_HouseBillIssuerSCAC);
				AssertEquals("declaration.JE_OH_ShippingLine", org.PK, declaration.JE_OH_ShippingLine);
				AssertEquals("declaration.CusContainers.Count", 2, declaration.CusContainers.Count);
				AssertEquals("declaration.PackingGroups.Count", 2, declaration.PackingGroups.Count);
				AssertEquals("declaration.Packages.Count", 2, declaration.Packages.Count);
				declaration.US_EnableENS = true;
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "INV1510161200";
				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "1010101010";
				var containersForInvoiceLines = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
				containersForInvoiceLines[0].IsForInvoiceLine = false;
				var containerForInvoiceLine = containersForInvoiceLines[1];
				containerForInvoiceLine.IsForInvoiceLine = true;
				var fda = invoiceLine.FDAs.AddNew();
				fda.US_FDAProductCode = "A";
				var containersForInvoiceLine = fda.ContainersForInvoiceLine;
				containersForInvoiceLine[0].IsForFDALine = true;
				Factory.SaveForTesting();
				var sourceBOManager = shipment.GetUniversalDataContextManager() as IShipmentDataContextManager;
				var writer = sourceBOManager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)));
				var dataObject = (Shipment)writer.GetDataObject(shipment);
				var message = GetQueuedUniversalShipmentMessage(dataObject);
				message.EM_MessageText = message.EM_MessageText.Replace(consol.JK_UniqueConsignRef, "").
					Replace(shipment.JS_UniqueConsignRef, "").
					Replace(declaration.JE_DeclarationReference, "").
					Replace("DataSource", "DataTarget").
					Replace("1510161200", "1510161300");
				ProcessMessage(message, new ServiceTaskLogForTesting());

				var newDeclaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MasterBill, "OB1510161300"));
				AssertEquals("newDeclaration.JE_OverrideFreightDefaults", ZBool.False, newDeclaration.JE_OverrideFreightDefaults);
				AssertEquals("newDeclaration.Bills.Count", 2, newDeclaration.Bills.Count);
				AssertEquals("newDeclaration.JE_MasterBill", "OB1510161300", newDeclaration.JE_MasterBill);
				AssertEquals("newDeclaration.JE_MasterBillIssuerSCAC", "XXXD", newDeclaration.JE_MasterBillIssuerSCAC);
				AssertEquals("newDeclaration.JE_HouseBill", "HB1510161300", newDeclaration.JE_HouseBill);
				AssertEquals("newDeclaration.JE_MasterBillIssuerSCAC", "APLU", newDeclaration.JE_HouseBillIssuerSCAC);
				AssertEquals("newDeclaration.CusContainers.Count", 2, newDeclaration.CusContainers.Count);
				AssertEquals("newDeclaration.Invoices.Count", 1, newDeclaration.Invoices.Count);
				var newInvoice = newDeclaration.Invoices[0];
				AssertEquals("newInvoice.JZ_InvoiceNumber", "INV1510161300", newInvoice.JZ_InvoiceNumber);
				AssertEquals("newInvoice.JobComInvoiceLines.Count", 1, newInvoice.JobComInvoiceLines.Count);
				var newInvoiceLine = newInvoice.JobComInvoiceLines[0];
				AssertEquals("newInvoiceLine.JI_Tariff", "1010101010", newInvoiceLine.JI_Tariff);
				AssertEquals("newInvoiceLine.ContainersPivot.Count", 1, newInvoiceLine.ContainersPivot.Count);
				var containerPivot1 = newInvoiceLine.ContainersPivot[0];
				AssertEquals("containerPivot1.ContainerNumber", containerForInvoiceLine.ContainerNumber, containerPivot1.ContainerNumber);
				AssertEquals("newInvoiceLine.FDAs.Count", 1, newInvoiceLine.FDAs.Count);
				var newFda = newInvoiceLine.FDAs[0];
				AssertEquals("newFda.US_FDAProductCode", "A", newFda.US_FDAProductCode);
				AssertEquals("newFda.ContainersForFDALine.Count", 1, newFda.ContainersForFDALine.Count);
				var containerForFDALine = newFda.ContainersForFDALine[0];
				AssertEquals("containerForFDALine.Container.CO_ContainerNumber", containerForInvoiceLine.ContainerNumber, containerForFDALine.Container.CO_ContainerNumber);
			}
			finally
			{
				GlbDepartment.CurrentDepartment.GE_Import = oldValue;
			}
		}

		public void TestGetJobDeclarationAddInfoFieldHandleSeparately()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MB2343";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.US_CheckNo = "111123614";
			declaration.US_SchDEntry = "2410";
			declaration.US_IsAIIRequested = true;
			declaration.US_PaymentDueDate = new ZDateTime(2015, 08, 28);
			declaration.US_PaymentDate = new ZDateTime(2015, 08, 28);
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_Weight = 8m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entry.EntryNumber = "50689378";
			var ediMessage = entry.Messages.AddNew(typeof(MQEDIMessage));
			ediMessage.EM_MessageNum = "123400001";
			ediMessage.EM_MessageType = Messaging.Business.MessageBuildingBlocks.ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse;
			ediMessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;

			Factory.SaveForTesting();

			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.GoodsDescription = "TEST ADDINFO";
			var invoiceLineDataObject = SetupCommercialInvoiceLine(1, null, null);
			invoiceLineDataObject.LinePrice = 1500m;
			invoiceLineDataObject.Weight = 9m;
			invoiceLineDataObject.AddInfoCollection = AddInfoCollectionCreator.CreateCollection(string.Format("{0}={1}", USAddInfoSchema.US_LicenseNo.Name.Substring(3), "T0001"));
			declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group", null, new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { SetupCommercialInvoiceHeaderData(null, null, new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject })) }), null);
			declarationDataObject.SetAddInfoCollection(() => new List<AddInfo>());
			declarationDataObject.AddInfoCollection.Add(new AddInfo() { Key = "CheckNo", Value = "000999" });
			declarationDataObject.AddInfoCollection.Add(new AddInfo() { Key = "PaymentDueDate", Value = "2014-06-26 00:00:00.000" });
			declarationDataObject.AddInfoCollection.Add(new AddInfo() { Key = "IsAIIRequested", Value = "N" });
			declarationDataObject.AddInfoCollection.Add(new AddInfo() { Key = "PaymentDate", Value = "2014-06-27 00:00:00.000" });
			declarationDataObject.AddInfoCollection.Add(new AddInfo() { Key = "SchDEntry", Value = "1101" });

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("The Job updated", declarationBO.JE_GoodsDescription, "TEST ADDINFO");

			AssertEquals("US_CheckNo should not updated", declarationBO.US_CheckNo, "111123614");
			AssertEquals("US_IsAIIRequested should not updated", declarationBO.US_IsAIIRequested, true);
			AssertEquals("US_PaymentDueDate should not updated", declarationBO.US_PaymentDueDate, new ZDateTime(2015, 08, 28));
			AssertEquals("US_PaymentDate should not updated", declarationBO.US_PaymentDate, new ZDateTime(2015, 08, 28));

			AssertEquals("US_SchDEntry updated", declarationBO.US_SchDEntry, "1101");
		}

		public void TestCalculationReplacementMessageNeed()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.MessageType = new CodeDescriptionPair()
			{
				Code = JobMessageTypeList.Codes.Export,
				Description = USJobMessageTypeList.Descriptions.Export
			};
			declarationDataObject.GoodsDescription = "TEST EXP";
			declarationDataObject.TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.BorderWaterBorne };

			var declarationBOToLoad = Factory.New<JobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Export;
			declarationBOToLoad.JE_MasterBill = "MB2343";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_GoodsDescription = ZString.Empty;
			declarationBOToLoad.US_EnableAII = true;
			declarationBOToLoad.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var invoiceHeader = declarationBOToLoad.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
			declarationBOToLoad.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.SaveForTesting();

			var entryHeader = declarationBOToLoad.ActiveEntryHeaders[0];
			entryHeader.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;
			Factory.SaveForTesting();
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			entryHeader = declarationBO.ActiveEntryHeaders[0];
			AssertEquals("The transport mode changed, so Replacement Message need to send", entryHeader.CH_Status, AESDirectCustomsEntryStatus.Codes.ReplacementSEDRequired);
		}

		public void TestDeclarationMergeAfterImport()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MB2343";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_Weight = 8m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.SaveForTesting();

			var cusEntryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertNotNull(cusEntryHeader);
			var entryLine = cusEntryHeader.EntryLines.FirstOrDefault();
			AssertNotNull(entryLine);
			AssertEquals(1000m, entryLine.CL_CustomsValue);

			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.GoodsDescription = "TEST MERGE";
			var invoiceLineDataObject = SetupCommercialInvoiceLine(1, null, null);
			invoiceLineDataObject.LinePrice = 1500m;
			invoiceLineDataObject.Weight = 9m;
			invoiceLineDataObject.AddInfoCollection = AddInfoCollectionCreator.CreateCollection(string.Format("{0}={1}", USAddInfoSchema.US_LicenseNo.Name.Substring(3), "T0001"));
			declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group", null, new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { SetupCommercialInvoiceHeaderData(null, null, new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject })) }), null);

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("Job has been updated", "TEST MERGE", declarationBO.JE_GoodsDescription);
			var cusEntryHeader2 = declarationBO.ActiveEntryHeaders.EntrySummaryEntry;
			AssertNotNull(cusEntryHeader2);
			var entryLine2 = cusEntryHeader2.EntryLines.FirstOrDefault();
			AssertNotNull(entryLine2);
			AssertEquals("Declaration Merge After Import", 1500m, entryLine2.CL_CustomsValue);
		}

		public void TestDeclarationMergeFailAfterImport()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MB2343";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_Weight = 8m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.SaveForTesting();
			declaration.US_EnableENS = false;
			Factory.SaveForTesting();
			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });

			declarationDataObject.GoodsDescription = "TEST MERGE FAIL";

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				"Cannot be updated, Merge failed. Error : Merge attempted.   Either Enable Entry Summary or Enable Cargo Release must be selected to merge." + System.Environment.NewLine,
				() =>
				{
					try
					{
						declaration = reader.ReadIntoBusinessObject();
					}
					catch
					{
						declaration.UnlockDoMergeMutex();
						throw;
					}
				});
		}

		public void TestShouldNotImportWhenHasBeenLodgedAtCustoms()
		{
			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.GoodsDescription = "TEST MSG";

			var invoiceLineDataObject = SetupCommercialInvoiceLine(1, null, null);
			invoiceLineDataObject.AddInfoCollection = AddInfoCollectionCreator.CreateCollection(string.Format("{0}={1}", USAddInfoSchema.US_LicenseNo.Name.Substring(3), "T0001"));
			declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group", null, new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { SetupCommercialInvoiceHeaderData(null, null, new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject })) }), null);

			var declarationBOToLoad = Factory.New<JobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationBOToLoad.JE_MasterBill = "MB2343";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GoodsDescription = ZString.Empty;
			declarationBOToLoad.US_EnableAII = true;
			declarationBOToLoad.JE_GoodsDescription = "ABC";
			var invoice = declarationBOToLoad.Invoices.AddNew();
			var invoicePK = invoice.PK;
			var edimessage = invoice.Messages.AddNew(typeof(EDIMessageForTest));
			edimessage.EM_MessageNum = "123400001";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var ensEntry = Factory.NewMoq<CusEntryHeader>();
			ensEntry.Object.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.Object.CH_JE = declarationBOToLoad.PK;
			ensEntry.Object.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			ensEntry.Object.Messages.AddNew(typeof(EDIMessageForTest));
			declarationBOToLoad.CustomsEntryHeaders.Add(ensEntry.Object);
			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(ensEntry.Object.MergedLines.AddNew());
			AssertEquals("HasBeenLodgedAtCustoms", true, ensEntry.Object.HasBeenLodgedAtCustoms);
			Factory.SaveForTesting();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("Should Not Import When the job HasBeenLodgedAtCustoms", "ABC", declarationBO.JE_GoodsDescription);
			ensEntry.VerifyAll();
		}

		public void TestImportedExportJob()
		{
			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.MessageType = new CodeDescriptionPair()
			{
				Code = JobMessageTypeList.Codes.Export,
				Description = USJobMessageTypeList.Descriptions.Export
			};
			declarationDataObject.GoodsDescription = "TEST EXP";

			var entryHeaderDataObject = SetupEntryHeader("INT", "SNT", ZString.Empty, "0000094141", 0m, null, null);
			entryHeaderDataObject.EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[] { SetupEntryNumber() });
			entryHeaderDataObject.EntryLineCollection = new List<UniversalCustoms.EntryLine>(new[] { SetupEntryLine() });
			entryHeaderDataObject.EntryHeaderChargeCollection = new List<UniversalCustoms.EntryHeaderCharge>(new[] { SetupEntryHeaderCharge() });
			entryHeaderDataObject.RelatedEntryHeaderCollection = new List<UniversalCustoms.EntryHeader>(new[]
					{
						SetupEntryHeader("IM$", "MS1", "ES1", "BG89756", 869.54m, new ZDateTime(2012, 4, 4), new ZDateTime(2012, 4, 5))
					});
			declarationDataObject.SetEntryHeaderCollection(() => new List<UniversalCustoms.EntryHeader>() { entryHeaderDataObject });

			var declarationBOToLoad = Factory.New<JobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Export;
			declarationBOToLoad.JE_MasterBill = "MB2343";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_GoodsDescription = ZString.Empty;

			var invoiceHeader = declarationBOToLoad.Invoices.AddNew();
			invoiceHeader.Messages.AddNew(typeof(EDIMessageForTest));

			Factory.SaveForTesting();
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			Assert(!declarationBO.DeclarationMessagesHaveBeenSent());
			AssertEquals("The job is an export (EXP) declaration (regardless of the number of messages on the job) will be updated", "TEST EXP", declarationBO.JE_GoodsDescription);
			AssertEquals("Never allow any update of CusEntryHeader details for all jobs ", 0, declarationBO.ActiveEntryHeaders.Count);
		}

		public void TestImportedFTZAdmissionJob()
		{
			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.MessageType = new CodeDescriptionPair()
			{
				Code = JobMessageTypeList.Codes.FTZ,
				Description = USJobMessageTypeList.Descriptions.FTZ
			};
			declarationDataObject.GoodsDescription = "TEST FTZ";
			var declarationBOToLoad = Factory.New<JobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declarationBOToLoad.JE_MasterBill = "MB2343";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_GoodsDescription = ZString.Empty;
			Factory.SaveForTesting();
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			var entry = declarationBOToLoad.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entry.EntryNumber = "50689378";
			var ediMessage = entry.Messages.AddNew(typeof(MQEDIMessage));
			ediMessage.EM_MessageNum = "123400001";
			ediMessage.EM_MessageType = Messaging.Business.MessageBuildingBlocks.ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse;
			ediMessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;

			Assert(declarationBO.DeclarationMessagesHaveBeenSent());
			AssertEquals("The job is an export (FTZ) declaration (regardless of the number of messages on the job) will be updated", "TEST FTZ", declarationBO.JE_GoodsDescription);
		}

		public void TestImportedFTZAdmissionJob_FTZAdmissionNumber_BillMatch_EmptyFTZNumber()
		{
			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.MessageType = new CodeDescriptionPair()
			{
				Code = JobMessageTypeList.Codes.FTZ,
				Description = USJobMessageTypeList.Descriptions.FTZ
			};
			declarationDataObject.GoodsDescription = "TEST FTZ";
			declarationDataObject.SetEntryNumberCollection(() => new List<EntryNumber>
			{
				new EntryNumber
				{
					CountryOfIssue = new Country
					{
						Code = "US"
					},
					Type = new EntryType
					{
						Code = "FTZ"
					},
					Number = "0123|18|TEST"
				}
			});
			var declarationBOToLoad = Factory.New<JobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declarationBOToLoad.JE_MasterBill = "MB2343";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_GoodsDescription = ZString.Empty;
			declarationBOToLoad.US_EnableAII = true;
			Factory.SaveForTesting();
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("The original declaration should be updated", declarationBO.PK, declarationBOToLoad.PK);
			AssertEquals("The job should be matched through the bills", "TEST FTZ", declarationBO.JE_GoodsDescription);
			AssertEquals("The FTZ Admission Number should be updated", "0123|18|TEST", declarationBO.FTZAdmissionNumber);
		}

		public void TestImportedFTZAdmissionJob_FTZAdmissionNumber_BillMatch_NonEmptyFTZNumber()
		{
			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.MessageType = new CodeDescriptionPair()
			{
				Code = JobMessageTypeList.Codes.FTZ,
				Description = USJobMessageTypeList.Descriptions.FTZ
			};
			declarationDataObject.GoodsDescription = "TEST FTZ";
			declarationDataObject.SetEntryNumberCollection(() => new List<EntryNumber>
			{
				new EntryNumber
				{
					CountryOfIssue = new Country
					{
						Code = "US"
					},
					Type = new EntryType
					{
						Code = "FTZ"
					},
					Number = "0123|18|TEST"
				}
			});
			var declarationBOToLoad = Factory.New<JobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declarationBOToLoad.JE_MasterBill = "MB2343";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_GoodsDescription = ZString.Empty;
			declarationBOToLoad.US_EnableAII = true;
			declarationBOToLoad.FTZAdmissionNumber = "3210|18|TEST";

			Factory.SaveForTesting();
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertNotEquals("A new declaration should be created", declarationBO.PK, declarationBOToLoad.PK);
			AssertEquals("New goods description", "TEST FTZ", declarationBO.JE_GoodsDescription);
			AssertEquals("New FTZ Admission Number", "0123|18|TEST", declarationBO.FTZAdmissionNumber);
		}

		public void TestImportedFTZAdmissionJob_FTZAdmissionNumber_FTZAdmissionNumberMatch()
		{
			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.MessageType = new CodeDescriptionPair()
			{
				Code = JobMessageTypeList.Codes.FTZ,
				Description = USJobMessageTypeList.Descriptions.FTZ
			};
			declarationDataObject.GoodsDescription = "TEST FTZ";
			declarationDataObject.SetEntryNumberCollection(() => new List<EntryNumber>
			{
				new EntryNumber
				{
					CountryOfIssue = new Country
					{
						Code = "US"
					},
					Type = new EntryType
					{
						Code = "FTZ"
					},
					Number = "0123|18|TEST"
				}
			});
			var declarationBOToLoad = Factory.New<JobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declarationBOToLoad.JE_MasterBill = "MB2344";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_GoodsDescription = ZString.Empty;
			declarationBOToLoad.FTZAdmissionNumber = "0123|18|TEST";
			declarationBOToLoad.US_EnableAII = true;
			Factory.SaveForTesting();
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("The original declaration should be updated", declarationBO.PK, declarationBOToLoad.PK);
			AssertEquals("The job should be matched by the FTZ Admission Number", "TEST FTZ", declarationBO.JE_GoodsDescription);
			AssertEquals("The FTZ Admission Number should be the same", "0123|18|TEST", declarationBO.FTZAdmissionNumber);
		}

		public void TestImportedFTZAdmissionJob_FTZAdmissionNumber_NoMatch()
		{
			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.MessageType = new CodeDescriptionPair()
			{
				Code = JobMessageTypeList.Codes.FTZ,
				Description = USJobMessageTypeList.Descriptions.FTZ
			};
			declarationDataObject.GoodsDescription = "TEST FTZ";
			declarationDataObject.SetEntryNumberCollection(() => new List<EntryNumber>
			{
				new EntryNumber
				{
					CountryOfIssue = new Country
					{
						Code = "US"
					},
					Type = new EntryType
					{
						Code = "FTZ"
					},
					Number = "0123|18|TEST"
				}
			});
			var declarationBOToLoad = Factory.New<JobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declarationBOToLoad.JE_MasterBill = "MB2344";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_GoodsDescription = ZString.Empty;
			declarationBOToLoad.FTZAdmissionNumber = "0124|18|TEST";
			declarationBOToLoad.US_EnableAII = true;
			Factory.SaveForTesting();
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertNotEquals("A new declaration should have been created", declarationBO.PK, declarationBOToLoad.PK);
			AssertEquals("New job goods description", "TEST FTZ", declarationBO.JE_GoodsDescription);
			AssertEquals("New job FTZ Admission Number", "0123|18|TEST", declarationBO.FTZAdmissionNumber);
		}

		public void TestImportedFTZAdmissionJob_FTZAdmissionNumber_MultiMatch_Error()
		{
			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.MessageType = new CodeDescriptionPair()
			{
				Code = JobMessageTypeList.Codes.FTZ,
				Description = USJobMessageTypeList.Descriptions.FTZ
			};
			declarationDataObject.GoodsDescription = "TEST FTZ";
			declarationDataObject.SetEntryNumberCollection(() => new List<EntryNumber>
			{
				new EntryNumber
				{
					CountryOfIssue = new Country
					{
						Code = "US"
					},
					Type = new EntryType
					{
						Code = "FTZ"
					},
					Number = "0124|18|TEST"
				}
			});
			var declarationBO1 = Factory.New<JobDeclaration>();
			declarationBO1.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declarationBO1.JE_MasterBill = "MB2344";
			declarationBO1.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBO1.JE_GS_NKCusAgent = ZString.Empty;
			declarationBO1.JE_GoodsDescription = ZString.Empty;
			declarationBO1.FTZAdmissionNumber = "0124|18|TEST";
			declarationBO1.US_EnableAII = true;
			declarationBO1.JE_DeclarationReference = "B00172423";

			var declarationBO2 = Factory.New<JobDeclaration>();
			declarationBO2.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declarationBO2.JE_MasterBill = "MB2344";
			declarationBO2.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBO2.JE_GS_NKCusAgent = ZString.Empty;
			declarationBO2.JE_GoodsDescription = ZString.Empty;
			declarationBO2.FTZAdmissionNumber = "0124|18|TEST";
			declarationBO2.US_EnableAII = true;
			declarationBO2.JE_DeclarationReference = "B00111111";
			Factory.SaveForTesting();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertContains("Importation is rejected.", "A search by FTZ Admission Number has located multiple declarations. The data import will not proceed because the system is unable to identify a unique target for update based on the FTZ Admission Number Specified.", logger.Logs);
		}

		public void TestImportedFTZAdmissionJob_FTZAdmissionNumber_DataTargetMatch_DifferentFTZAdmissionNumber_NoError()
		{
			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.MessageType = new CodeDescriptionPair()
			{
				Code = JobMessageTypeList.Codes.FTZ,
				Description = USJobMessageTypeList.Descriptions.FTZ
			};
			declarationDataObject.GoodsDescription = "TEST FTZ";
			declarationDataObject.SetEntryNumberCollection(() => new List<EntryNumber>
			{
				new EntryNumber
				{
					CountryOfIssue = new Country
					{
						Code = "US"
					},
					Type = new EntryType
					{
						Code = "FTZ"
					},
					Number = "0123|18|TEST"
				}
			});
			declarationDataObject.DataContext = new DataContext()
			{
				DataTargetCollection = new List<DataTarget>
				{
					new DataTarget
					{
						Key = "B00172423",
						Type = "CustomsDeclaration"
					}
				}
			};
			var declarationBOToLoad = Factory.New<JobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declarationBOToLoad.JE_MasterBill = "MB2344";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_GoodsDescription = ZString.Empty;
			declarationBOToLoad.FTZAdmissionNumber = "0124|18|TEST";
			declarationBOToLoad.US_EnableAII = true;
			declarationBOToLoad.JE_DeclarationReference = "B00172423";
			Factory.SaveForTesting();
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertNotContains("Importation is rejected.", "A different FTZ Admission Number already exists for the specified Job Number. It is for this reason that the data import will not proceed.", logger.Logs);
			AssertEquals("The original declaration should be updated", declarationBO.PK, declarationBOToLoad.PK);
			AssertEquals("The goods description should be updated", "TEST FTZ", declarationBO.JE_GoodsDescription);
			AssertEquals("The FTZ Admission Number should be ignored", "0124|18|TEST", declarationBO.FTZAdmissionNumber);
		}

		public void TestImportedFTZAdmissionJob_FTZAdmissionNumber_DataTargetMatch_SameFTZAdmissionNumber_NoError()
		{
			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.MessageType = new CodeDescriptionPair()
			{
				Code = JobMessageTypeList.Codes.FTZ,
				Description = USJobMessageTypeList.Descriptions.FTZ
			};
			declarationDataObject.GoodsDescription = "TEST FTZ";
			declarationDataObject.SetEntryNumberCollection(() => new List<EntryNumber>
			{
				new EntryNumber
				{
					CountryOfIssue = new Country
					{
						Code = "US"
					},
					Type = new EntryType
					{
						Code = "FTZ"
					},
					Number = "0123|18|TEST"
				}
			});
			declarationDataObject.DataContext = new DataContext()
			{
				DataTargetCollection = new List<DataTarget>
				{
					new DataTarget
					{
						Key = "B00172423",
						Type = "CustomsDeclaration"
					}
				}
			};
			var declarationBOToLoad = Factory.New<JobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declarationBOToLoad.JE_MasterBill = "MB2344";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_GoodsDescription = ZString.Empty;
			declarationBOToLoad.FTZAdmissionNumber = "0123|18|TEST";
			declarationBOToLoad.US_EnableAII = true;
			declarationBOToLoad.JE_DeclarationReference = "B00172423";
			Factory.SaveForTesting();
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertNotContains("Importation should not be rejected.", "A different FTZ Admission Number already exists for the specified Job Number. It is for this reason that the data import will not proceed.", logger.Logs);
			AssertEquals("The original declaration should be updated", declarationBO.PK, declarationBOToLoad.PK);
			AssertEquals("The goods description should be updated", "TEST FTZ", declarationBO.JE_GoodsDescription);
			AssertEquals("The FTZ Admission Number should be the same", "0123|18|TEST", declarationBO.FTZAdmissionNumber);
		}

		public void TestImportedFTZAdmissionJob_FTZAdmissionNumber_DataTargetMatch_NoFTZAdmissionNumber_NoError()
		{
			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.MessageType = new CodeDescriptionPair()
			{
				Code = JobMessageTypeList.Codes.FTZ,
				Description = USJobMessageTypeList.Descriptions.FTZ
			};
			declarationDataObject.GoodsDescription = "TEST FTZ";
			declarationDataObject.SetEntryNumberCollection(() => new List<EntryNumber>
			{
				new EntryNumber
				{
					CountryOfIssue = new Country
					{
						Code = "US"
					},
					Type = new EntryType
					{
						Code = "FTZ"
					},
					Number = "0123|18|TEST"
				}
			});
			declarationDataObject.DataContext = new DataContext()
			{
				DataTargetCollection = new List<DataTarget>
				{
					new DataTarget
					{
						Key = "B00172423",
						Type = "CustomsDeclaration"
					}
				}
			};
			var declarationBOToLoad = Factory.New<JobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declarationBOToLoad.JE_MasterBill = "MB2344";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_GoodsDescription = ZString.Empty;
			declarationBOToLoad.US_EnableAII = true;
			declarationBOToLoad.JE_DeclarationReference = "B00172423";
			Factory.SaveForTesting();
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertNotContains("Importation should not be rejected.", "A different FTZ Admission Number already exists for the specified Job Number. It is for this reason that the data import will not proceed.", logger.Logs);
			AssertEquals("The original declaration should be updated", declarationBO.PK, declarationBOToLoad.PK);
			AssertEquals("The goods description should be updated", "TEST FTZ", declarationBO.JE_GoodsDescription);
			AssertEquals("The FTZ Admission Number should be the same", "0123|18|TEST", declarationBO.FTZAdmissionNumber);
		}

		public void TestImportedImportJobWithClearEntrySummaryOriginal()
		{
			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.GoodsDescription = "TEST IMP CEO";

			var declarationBOToLoad = Factory.New<JobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationBOToLoad.JE_MasterBill = "MB2343";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GoodsDescription = ZString.Empty;
			var invoice = declarationBOToLoad.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var ensEntry = Factory.NewMoq<CusEntryHeader>();
			ensEntry.Object.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.Object.CH_JE = declarationBOToLoad.PK;
			ensEntry.Object.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			ensEntry.Object.Messages.AddNew(typeof(EDIMessageForTest));
			declarationBOToLoad.CustomsEntryHeaders.Add(ensEntry.Object);
			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(ensEntry.Object.MergedLines.AddNew());
			AssertEquals("HasBeenLodgedAtCustoms", true, ensEntry.Object.HasBeenLodgedAtCustoms);
			Factory.SaveForTesting();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("The Job is an Import (IMP) job Unless the entry summary has already been submitted (either ACS or ACE)", "", declarationBO.JE_GoodsDescription);
			ensEntry.VerifyAll();
		}

		public void TestPrimaryMasterBillIsSetCorrectly_UseDefaulting_True() => AssertPrimaryMasterBillIsSetCorrectly(true);
		public void TestPrimaryMasterBillIsSetCorrectly_UseDefaulting_False() => AssertPrimaryMasterBillIsSetCorrectly(false);
		void AssertPrimaryMasterBillIsSetCorrectly(bool useDefaulting)
		{
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, useDefaulting))
			{
				var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "HB1",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House }
				};
				declarationDataObject.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(string.Format("{0}=MB1*{1}=AMB1*{2}=AHB1", Constants.AddInfoKeys.Declaration.MasterWayBillNumber, Constants.AddInfoKeys.Declaration.MasterWayBillIssuerSCAC, Constants.AddInfoKeys.Declaration.WayBillIssuerSCAC)));

				var bill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { NoOfPacks = 10m, BillNumber = "MB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master } };
				bill1.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(string.Format("{0}=BMB1", Bill.Schema.US_UI_NKBillIssuerSCAC.Substring(3))));
				var bill2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { NoOfPacks = 10m, BillNumber = "HB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House }, ParentBillNumber = "MB1" };
				bill2.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(string.Format("{0}=BMB1*{1}=BHB1", Constants.AddInfoKeys.AdditionalBill.ParentBillIssuerSCAC, Bill.Schema.US_UI_NKBillIssuerSCAC.Substring(3))));
				var bill3 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { NoOfPacks = 10m, BillNumber = "HB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.SubHouse }, ParentBillNumber = "HB1" };
				bill3.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(string.Format("{0}=BMB1*{1}=MB1*{2}=BHB1*{3}=AHB1", Constants.AddInfoKeys.AdditionalBill.ParentMasterBillIssuerSCAC, Constants.AddInfoKeys.AdditionalBill.ParentMasterBillNumber, Constants.AddInfoKeys.AdditionalBill.ParentBillIssuerSCAC, Bill.Schema.US_UI_NKBillIssuerSCAC.Substring(3))));
				var bill4 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { NoOfPacks = 15m, BillNumber = "MB2", BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master } };
				bill4.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(string.Format("{0}=AMB1", Bill.Schema.US_UI_NKBillIssuerSCAC.Substring(3))));
				var bill5 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { NoOfPacks = 15m, BillNumber = "HB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House }, ParentBillNumber = "MB2" };
				bill5.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(string.Format("{0}=AMB1*{1}=AHB1", Constants.AddInfoKeys.AdditionalBill.ParentBillIssuerSCAC, Bill.Schema.US_UI_NKBillIssuerSCAC.Substring(3))));
				var bill6 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { NoOfPacks = 15m, BillNumber = "HB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.SubHouse }, ParentBillNumber = "HB1" };
				bill6.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(string.Format("{0}=AMB1*{1}=MB2*{2}=AHB1*{3}=AHB1", Constants.AddInfoKeys.AdditionalBill.ParentMasterBillIssuerSCAC, Constants.AddInfoKeys.AdditionalBill.ParentMasterBillNumber, Constants.AddInfoKeys.AdditionalBill.ParentBillIssuerSCAC, Bill.Schema.US_UI_NKBillIssuerSCAC.Substring(3))));
				var bill7 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { NoOfPacks = 5m, BillNumber = "MB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master } };
				bill7.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(string.Format("{0}=AMB1", Bill.Schema.US_UI_NKBillIssuerSCAC.Substring(3))));
				var bill8 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { NoOfPacks = 5m, BillNumber = "HB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House }, ParentBillNumber = "MB1" };
				bill8.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(string.Format("{0}=AMB1*{1}=AHB1", Constants.AddInfoKeys.AdditionalBill.ParentBillIssuerSCAC, Bill.Schema.US_UI_NKBillIssuerSCAC.Substring(3))));
				var bill9 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { NoOfPacks = 5m, BillNumber = "HB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.SubHouse }, ParentBillNumber = "HB1" };
				bill9.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(string.Format("{0}=AMB1*{1}=MB1*{2}=AHB1*{3}=AHB1", Constants.AddInfoKeys.AdditionalBill.ParentMasterBillIssuerSCAC, Constants.AddInfoKeys.AdditionalBill.ParentMasterBillNumber, Constants.AddInfoKeys.AdditionalBill.ParentBillIssuerSCAC, Bill.Schema.US_UI_NKBillIssuerSCAC.Substring(3))));

				declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[]
				{
					bill1, bill2, bill3, bill4, bill5, bill6, bill7, bill8, bill9
				}));
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				declarationBO.Bills.Load();
				AssertEquals(9, declarationBO.Bills.Count);
				AssertEquals("JE_MasterBill", "MB1", declarationBO.JE_MasterBill);
				AssertEquals("JE_MasterBillIssuerSCAC", "AMB1", declarationBO.JE_MasterBillIssuerSCAC);
				AssertEquals("JE_HouseBill", "HB1", declarationBO.JE_HouseBill);
				AssertEquals("JE_HouseBillIssuerSCAC", "AHB1", declarationBO.JE_HouseBillIssuerSCAC);
				var bills = declarationBO.Bills.OfType<Bill>().ToArray();
				AssertEquals("bills.Length", 9, bills.Length);
				var mb1 = bills.First(x => x.CU_BillNum == "MB1" && x.CU_BillType == Enterprise.Customs.Business.BillTypeList.Codes.MasterBill && x.US_UI_NKBillIssuerSCAC == "BMB1" && x.CU_NoOfPacks == 10m && x.CU_CU_ParentBill.IsEmpty);
				var mb2 = bills.First(x => x.CU_BillNum == "MB2" && x.CU_BillType == Enterprise.Customs.Business.BillTypeList.Codes.MasterBill && x.US_UI_NKBillIssuerSCAC == "AMB1" && x.CU_NoOfPacks == 15m && x.CU_CU_ParentBill.IsEmpty);
				var mb3 = bills.First(x => x.CU_BillNum == "MB1" && x.CU_BillType == Enterprise.Customs.Business.BillTypeList.Codes.MasterBill && x.US_UI_NKBillIssuerSCAC == "AMB1" && x.CU_NoOfPacks == 5m && x.CU_CU_ParentBill.IsEmpty);
				var mb1hb1 = bills.First(x => x.CU_BillNum == "HB1" && x.CU_BillType == Enterprise.Customs.Business.BillTypeList.Codes.HouseBill && x.US_UI_NKBillIssuerSCAC == "BHB1" && x.CU_NoOfPacks == 10m && x.CU_CU_ParentBill == mb1.PK);
				var mb2hb1 = bills.First(x => x.CU_BillNum == "HB1" && x.CU_BillType == Enterprise.Customs.Business.BillTypeList.Codes.HouseBill && x.US_UI_NKBillIssuerSCAC == "AHB1" && x.CU_NoOfPacks == 15m && x.CU_CU_ParentBill == mb2.PK);
				var mb3hb1 = bills.First(x => x.CU_BillNum == "HB1" && x.CU_BillType == Enterprise.Customs.Business.BillTypeList.Codes.HouseBill && x.US_UI_NKBillIssuerSCAC == "AHB1" && x.CU_NoOfPacks == 5m && x.CU_CU_ParentBill == mb3.PK);
				var mb3hb1hb1 = bills.First(x => x.CU_BillNum == "HB1" && x.CU_BillType == Enterprise.Customs.Business.BillTypeList.Codes.SubHouseBill && x.US_UI_NKBillIssuerSCAC == "AHB1" && x.CU_NoOfPacks == 5m && x.CU_CU_ParentBill == mb3hb1.PK);
				var mb1hb1hb1 = bills.First(x => x.CU_BillNum == "HB1" && x.CU_BillType == Enterprise.Customs.Business.BillTypeList.Codes.SubHouseBill && x.US_UI_NKBillIssuerSCAC == "AHB1" && x.CU_NoOfPacks == 10m && x.CU_CU_ParentBill == mb1hb1.PK);
				var mb2hb1hb1 = bills.First(x => x.CU_BillNum == "HB1" && x.CU_BillType == Enterprise.Customs.Business.BillTypeList.Codes.SubHouseBill && x.US_UI_NKBillIssuerSCAC == "AHB1" && x.CU_NoOfPacks == 15m && x.CU_CU_ParentBill == mb2hb1.PK);
				AssertEquals("PrimaryMasterBill", mb3, declarationBO.PrimaryMasterBill);
				AssertEquals("PrimaryHouseBill", mb3hb1, declarationBO.PrimaryHouseBill);
			}
		}

		public void TestDeclarationDataAreSetInASpecificOrder()
		{
			#region Setup
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, true);
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_Code = "B@#";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			Dictionary<ZString, ZString> addInfos = new Dictionary<ZString, ZString>();
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, "JOB123ABC");

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			declarationDataObject.DataContext = dataContext;
			declarationDataObject.Branch = new Branch() { Code = "B@#" };
			var supplier = CreateOrganisation("SUPPLIER 1", "SUP123!");
			declarationDataObject.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);
			var importer = CreateOrganisation("SUPPLIER 1", "IMP123!");
			declarationDataObject.AddOrgAddress(writeManager, importer, AddressTypes.Importer);
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
			declarationDataObject.MessageSubType = new CodeDescriptionPair() { Code = "ST1" };
			declarationDataObject.TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Sea };
			declarationDataObject.CustomsContainerMode = new ContainerMode() { Code = Core.Constants.ContainerModes.Containerised };
			UpdateAddInfo(addInfos, USAddInfoSchema.US_EntryType, (ZString)EntryTypeList.Codes.Baggage);
			UpdateAddInfo(addInfos, USAddInfoSchema.US_EntryMode, (ZString)EntryModeList.Codes.Paired);
			UpdateAddInfo(addInfos, USAddInfoSchema.US_LiveEntryIndicator, (ZString)YesNoDefaultList.Codes.Yes);
			UpdateAddInfo(addInfos, USAddInfoSchema.US_EnableENS, ZBool.True);
			UpdateAddInfo(addInfos, USAddInfoSchema.US_IsInvoiceByRequest, ZBool.True);
			UpdateAddInfo(addInfos, USAddInfoSchema.US_EnableCRL, ZBool.True);
			UpdateAddInfo(addInfos, USAddInfoSchema.US_CargoReleaseType, (ZString)CargoReleaseTypeList.Codes.CR);
			UpdateAddInfo(addInfos, USAddInfoSchema.US_EnableAII, ZBool.True);
			UpdateAddInfo(addInfos, USAddInfoSchema.US_CertifyCargoRelease, ZBool.False);
			UpdateAddInfo(addInfos, USAddInfoSchema.US_EnableSPN, ZBool.True);
			declarationDataObject.ServiceLevel = new ServiceLevel() { Code = "SL1" };

			var masterBillDataObject = SetupAdditionalBill("MB1", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 3, 1), null, null, 10m, Core.Constants.PkgUnit.Box, "BOX");
			declarationDataObject.VesselName = "VESSEL1";
			declarationDataObject.VoyageFlightNo = "V234";
			UpdateAddInfo(addInfos, USAddInfoSchema.US_UI_NKCarrierSCAC, (ZString)"ABC1");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_SchDLoading, (ZString)"FP001");
			declarationDataObject.SetDateCollection(() => new List<Date>());
			declarationDataObject.DateCollection.Add(Date.New(DateType.LoadingDate, ZBool.False, new ZDateTime(2012, 10, 1, 1, 1, 1)));
			declarationDataObject.PortOfLoading = new UNLOCO() { Code = "UNP01" };
			UpdateAddInfo(addInfos, USAddInfoSchema.US_SchDArrival, (ZString)"LP01");
			declarationDataObject.DateCollection.Add(Date.New(DateType.DischargeDate, ZBool.False, new ZDateTime(2012, 10, 2, 2, 2, 2)));
			declarationDataObject.PortOfDischarge = new UNLOCO() { Code = "UNP02" };
			UpdateAddInfo(addInfos, USAddInfoSchema.US_SchDEntry, (ZString)"LP02");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_EntryDate, new ZDateTime(2012, 10, 3, 3, 3, 3));
			UpdateAddInfo(addInfos, USAddInfoSchema.US_IsHMFApplicable, (ZString)"N");
			declarationDataObject.DateCollection.Add(Date.New(DateType.FirstArrivalInCountry, ZBool.False, new ZDateTime(2012, 10, 4, 4, 4, 4)));
			UpdateAddInfo(addInfos, USAddInfoSchema.US_ITDate, new ZDateTime(2012, 10, 5, 5, 5, 5));

			var masterBillHouseBillDataObject = SetupAdditionalBill("MB1HB1", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 3, 2), "MB1", null, 6m, Core.Constants.PkgUnit.Piece, "Piece");
			declarationDataObject.WayBillNumber = "MB1HB1";
			declarationDataObject.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { masterBillDataObject, masterBillHouseBillDataObject }));
			declarationDataObject.PortOfOrigin = new UNLOCO() { Code = "UNP03" };
			declarationDataObject.DateCollection.Add(Date.New(DateType.Departure, ZBool.False, new ZDateTime(2012, 9, 1, 1, 1, 1)));
			declarationDataObject.PortOfDestination = new UNLOCO() { Code = "UNP04" };
			declarationDataObject.DateCollection.Add(Date.New(DateType.Arrival, ZBool.False, new ZDateTime(2012, 10, 6, 6, 6, 6)));
			UpdateAddInfo(addInfos, USAddInfoSchema.US_DestinationState, (ZString)"DS");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_UC_NKCountryOfExport, (ZString)"CE");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_DateOfExport, new ZDateTime(2012, 9, 2, 2, 2, 2));
			declarationDataObject.GoodsDescription = "GOODS DESC";
			declarationDataObject.OwnerRef = "OWNREF1";
			declarationDataObject.TotalWeight = 1010.10m;
			declarationDataObject.TotalWeightUnit = new UnitOfWeight() { Code = "TU" };
			declarationDataObject.TotalVolume = 10.10m;
			declarationDataObject.TotalVolumeUnit = new UnitOfVolume() { Code = "VU" };
			declarationDataObject.TotalNoOfPieces = 110;
			declarationDataObject.TotalNoOfPacks = 222;
			declarationDataObject.TotalNoOfPacksPackageType = new PackageType() { Code = "PT" };

			UpdateAddInfo(addInfos, USAddInfoSchema.US_US_NKLocationOfGoods, (ZString)"LG23");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_US_NKCentralizedExamSite, (ZString)"CES1");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_EntryFilerCode, (ZString)"EX2");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_7501Purchased, (ZString)YesNoDefaultList.Codes.Yes);
			UpdateAddInfo(addInfos, USAddInfoSchema.US_ManEntry, ZBool.True);
			UpdateAddInfo(addInfos, USAddInfoSchema.US_7501Agent, ZBool.False);

			var shippingLine = CreateOrganisation("SHIPPINGLINE 1", "SHP123!");
			declarationDataObject.AddOrgAddress(writeManager, shippingLine, AddressTypes.ShippingLine);
			var forwarder = CreateOrganisation("FORWARDER 1", "FWD123!");
			declarationDataObject.AddOrgAddress(writeManager, forwarder, AddressTypes.Forwarder);
			var customsContainerTerminalOperator = CreateOrganisation("CONTAINERTERMINALOPERATOR 1", "CNT123!");
			var customsContainerTerminalOperatorAddress = customsContainerTerminalOperator.MainAddress;
			declarationDataObject.AddOrgAddress(writeManager, customsContainerTerminalOperatorAddress, DocAddressType.CustomsContainerTerminalOperatorAddress);
			var customsDepot = CreateOrganisation("CUSTOMSDEPOT 1", "DEP123!");
			var customsDepotAddress = customsDepot.MainAddress;
			declarationDataObject.AddOrgAddress(writeManager, customsDepotAddress, DocAddressType.CustomsDepotAddress);
			var customsContainerYard = CreateOrganisation("CUSTOMSCONTAINERYARD 1", "CCY123!");
			var customsContainerYardAddress = customsContainerYard.MainAddress;
			declarationDataObject.AddOrgAddress(writeManager, customsContainerYardAddress, DocAddressType.CustomsContainerYardAddress);
			var controllingAgent = CreateOrganisation("ControllingAgent 1", "CA123!");
			declarationDataObject.AddOrgAddress(writeManager, controllingAgent, DocAddressType.ControllingAgent);
			var controllingCustomer = CreateOrganisation("ControllingCustomer 1", "CC123!");
			declarationDataObject.AddOrgAddress(writeManager, controllingCustomer, DocAddressType.ControllingCustomer);
			var externalBroker = CreateOrganisation("EXTERNALBROKER 1", "EBK123!");
			declarationDataObject.AddOrgAddress(writeManager, externalBroker, DocAddressType.ExternalBroker);
			var ultimateConsignee = CreateOrganisation("ULTIMATECONSIGNEE 1", "ULC123!");
			declarationDataObject.AddOrgAddress(writeManager, ultimateConsignee, DocAddressType.UltimateConsignee);
			var importerOfRecord = CreateOrganisation("IMPORTEROFRECORD 1", "IOR123!");
			declarationDataObject.AddOrgAddress(writeManager, importerOfRecord, Constants.AddressType.ImporterOfRecord);
			var notifyParty = CreateOrganisation("NOTIFYPARTY 1", "NTF123!");
			declarationDataObject.AddOrgAddress(writeManager, notifyParty, DocAddressType.NotifyParty);

			var importerPickupDelivery = CreateOrganisation("IMPORTERPICKUPDELIVERY 1", "IPD123!");
			var importerPickupDeliveryAddress = importerPickupDelivery.MainAddress;
			declarationDataObject.AddOrgAddress(writeManager, importerPickupDeliveryAddress, DocAddressType.ImporterPickupDeliveryAddress);
			var deliveryCartageCo = CreateOrganisation("DELIVERYCARTAGECO 1", "DCC123!");
			var deliveryCartageCoAddress = deliveryCartageCo.MainAddress;
			declarationDataObject.AddOrgAddress(writeManager, deliveryCartageCoAddress, AddressTypes.DeliveryLocalCartage);
			var localProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			declarationDataObject.LocalProcessing = localProcessing;
			localProcessing.FCLAvailable = new ZDateTime(2012, 11, 1, 1, 1, 1);
			localProcessing.FCLStorageCommences = new ZDateTime(2012, 11, 2, 2, 2, 2);
			localProcessing.FCLDeliveryEquipmentNeeded = new CodeDescriptionPair() { Code = "DEN" };
			localProcessing.LCLDatesOverrideConsol = ZBool.True;
			localProcessing.LCLAvailable = new ZDateTime(2012, 11, 3, 3, 3, 3);
			localProcessing.LCLStorageCommences = new ZDateTime(2012, 11, 4, 4, 4, 4);
			localProcessing.EstimatedDelivery = new ZDateTime(2012, 11, 5, 5, 5, 5);
			localProcessing.DeliveryRequiredBy = new ZDateTime(2012, 11, 6, 6, 6, 6);
			localProcessing.DeliveryCartageAdvised = new ZDateTime(2012, 11, 7, 7, 7, 7);
			localProcessing.DeliveryCartageCompleted = new ZDateTime(2012, 11, 8, 8, 8, 8);
			localProcessing.DeliveryLabourCharge = 20.20m;
			localProcessing.DeliveryLabourTime = new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 11, 9, 9, 9, 9);
			localProcessing.DemurrageOnDeliveryCharge = 234.12m;
			localProcessing.DemurrageOnDeliveryTime = new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 11, 10, 10, 10, 10);

			var cbpBroker = CreateOrganisation("CBPBROKER 1", "BRK123!");
			declarationDataObject.AddOrgAddress(writeManager, cbpBroker, Constants.AddressType.CBPBroker);
			var exporter = CreateOrganisation("EXPORTER 1", "EXP123!");
			declarationDataObject.AddOrgAddress(writeManager, exporter, Constants.AddressType.Exporter);
			var manufacturer = CreateOrganisation("MANUFACTURER 1", "MAN123!");
			var manufacturerAddress = manufacturer.MainAddress;
			declarationDataObject.AddOrgAddress(writeManager, manufacturerAddress, DocAddressType.Manufacturer);
			var seller = CreateOrganisation("SELLER 1", "SEL123!");
			declarationDataObject.AddOrgAddress(writeManager, seller, Constants.AddressType.Seller);
			var sellingAgent = CreateOrganisation("SELLINGAGENT 1", "SAG123!");
			declarationDataObject.AddOrgAddress(writeManager, sellingAgent, Customs.DataTransfer.Universal.Constants.AddressTypes.SellingAgent);
			var invoicer = CreateOrganisation("INVOICER 1", "INV123!");
			var invoicerAddress = invoicer.MainAddress;
			declarationDataObject.AddOrgAddress(writeManager, invoicerAddress, Constants.AddressType.Invoicer);
			var buyer = CreateOrganisation("BUYER 1", "BUY123!");
			declarationDataObject.AddOrgAddress(writeManager, buyer, DocAddressType.BuyerDocumentaryAddress);
			var buyingAgent = CreateOrganisation("BUYINGAGENT 1", "BAG123!");
			declarationDataObject.AddOrgAddress(writeManager, buyingAgent, Customs.DataTransfer.Universal.Constants.AddressTypes.BuyingAgent);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "Z!";
			staff.GS_FullName = "DUMMY BOB";

			declarationDataObject.CustomsBroker = new Staff() { Code = "Z!" };
			declarationDataObject.MergeBy = new CodeDescriptionPair() { Code = "TRF" };
			UpdateAddInfo(addInfos, USAddInfoSchema.US_BRDRefNo, (ZString)"BRDREFNO1");
			// Entry Data
			UpdateAddInfo(addInfos, USAddInfoSchema.US_MissingDocument1, (ZString)"M1");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_MissingDocument2, (ZString)"M2");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_TeamNo, (ZString)"T1");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_OGALineReleaseIndicator, (ZString)YesNoDefaultList.Codes.Yes);
			UpdateAddInfo(addInfos, USAddInfoSchema.US_EntryDateElectionCode, (ZString)"E");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_PresentationDate, new ZDateTime(2012, 11, 11, 11, 11, 11));
			UpdateAddInfo(addInfos, USAddInfoSchema.US_ConsolidatedInformalIndicator, (ZString)YesNoDefaultList.Codes.Yes);
			UpdateAddInfo(addInfos, USAddInfoSchema.US_GeneralOrderNo, (ZString)"GO12");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_TaxDeferIndicator, (ZString)YesNoDefaultList.Codes.Yes);
			// Remote Filing Data
			UpdateAddInfo(addInfos, USAddInfoSchema.US_PreparerDistrictPort, (ZString)"PDP1");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_SchDExam, (ZString)"SDE1");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_DES, (ZString)"DES1");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_PreparerOfficeCode, (ZString)"P2");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_OtherReconIndicator, (ZString)YesNoDefaultList.Codes.Yes);
			// Reconciliation Data
			UpdateAddInfo(addInfos, USAddInfoSchema.US_NAFTAReconIndicator, ZBool.True);
			UpdateAddInfo(addInfos, USAddInfoSchema.US_FixRecon, ZBool.True);
			// Bond Data
			UpdateAddInfo(addInfos, USAddInfoSchema.US_BondType, (ZString)"B");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_BondAmount, (ZDecimal)53.4m);
			UpdateAddInfo(addInfos, USAddInfoSchema.US_BondCalcCode, (ZString)"BC");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_BondProducerAccNo, (ZString)"BPA1");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_SuretyCode, (ZString)"SC1");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_ADDCVDSuretyCode, (ZString)"AS1");
			// Payment Data
			UpdateAddInfo(addInfos, USAddInfoSchema.US_PaymentType, (ZString)"P");
			declarationDataObject.PaymentMethod = new CodeDescriptionPair() { Code = "PM" };
			UpdateAddInfo(addInfos, USAddInfoSchema.US_PeriodicStatementMM, (ZString)"01");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_PreliminaryStatementPrintDate, new ZDateTime(2012, 11, 12, 12, 12, 12));
			UpdateAddInfo(addInfos, USAddInfoSchema.US_FixPSD, ZBool.True);
			UpdateAddInfo(addInfos, USAddInfoSchema.US_ClientBranchDesignation, (ZString)"CB");

			var fdaSubmitter = CreateOrganisation("FDASUBMITTER 1", "FDA123!");
			declarationDataObject.AddOrgAddress(writeManager, fdaSubmitter, Constants.AddressType.FDASubmitter);
			UpdateAddInfo(addInfos, USAddInfoSchema.US_FDAContactName, (ZString)"BOB");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_FDAContactPhoneNo, (ZString)"B123");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_FDAContactEmail, (ZString)"B@WHERE.COM");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_FDAADTA, new ZDateTime(2012, 11, 13, 13, 13, 13));
			UpdateAddInfo(addInfos, USAddInfoSchema.US_FDAAPC, (ZString)"APC");
			// Carrier/Privately Owned Vehicle
			UpdateAddInfo(addInfos, USAddInfoSchema.US_FDACANType, (ZString)"T");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_FDACCN, (ZString)"CC");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_FDACAN, (ZString)"CAN");
			var builder = new ZStringBuilder(addInfos.Select((KeyValuePair<ZString, ZString> pair) => AddInfoParser.Serialise(pair.Key, pair.Value)));
			declarationDataObject.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(builder.ToString()));

			Factory.SaveForTesting();
			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_DeclarationReference = "JOB123ABC";
			var expectedOrders = new List<InfoValueChangeData>(new[]
			{
				new InfoValueChangeData(declaration.JE_GBInfo, ZGuid.Empty, branch2.PK),
				new InfoValueChangeData(declaration.JE_OH_SupplierInfo, ZGuid.Empty, supplier.PK),
				new InfoValueChangeData(declaration.JE_OH_ImporterInfo, ZGuid.Empty, importer.PK),
				new InfoValueChangeData(declaration.JE_MessageTypeInfo, ZString.Empty, (ZString)JobMessageTypeList.Codes.Import),
				new InfoValueChangeData(declaration.JE_MessageSubTypeInfo, ZString.Empty, (ZString)"ST1"),
				new InfoValueChangeData(declaration.JE_TransportModeInfo, ZString.Empty, declaration.TransportModeSeaCodeForTesting),
				new InfoValueChangeData(declaration.JE_ContainerModeInfo, ZString.Empty, (ZString)Core.Constants.ContainerModes.Containerised),

				new InfoValueChangeData(declaration.US_EntryTypeInfo, ZString.Empty, (ZString)EntryTypeList.Codes.Baggage),
				new InfoValueChangeData(declaration.US_EntryModeInfo, ZString.Empty, (ZString)EntryModeList.Codes.Paired),
				new InfoValueChangeData(declaration.US_LiveEntryIndicatorInfo, ZString.Empty, (ZString)YesNoDefaultList.Codes.Yes),
				new InfoValueChangeData(declaration.US_EnableENSInfo, ZBool.False, ZBool.True),
				new InfoValueChangeData(declaration.US_CertifyCargoReleaseInfo, ZBool.False, ZBool.True),
				new InfoValueChangeData(declaration.US_IsInvoiceByRequestInfo, ZBool.False, ZBool.True),
				new InfoValueChangeData(declaration.US_EnableCRLInfo, ZBool.False, ZBool.True),
				new InfoValueChangeData(declaration.US_CertifyCargoReleaseInfo, ZBool.True, ZBool.False),
				new InfoValueChangeData(declaration.US_CargoReleaseTypeInfo, (ZString)CargoReleaseTypeList.Codes.SE, (ZString)CargoReleaseTypeList.Codes.CR),
				new InfoValueChangeData(declaration.US_EnableAIIInfo, ZBool.False, ZBool.True),
				new InfoValueChangeData(declaration.US_EnableSPNInfo, ZBool.False, ZBool.True),
				new InfoValueChangeData(declaration.JE_RS_NKServiceLevelInfo, ZString.Empty, (ZString)"SL1"),

				new InfoValueChangeData(declaration.JE_MasterBillInfo, ZString.Empty, (ZString)"MB1"),
				new InfoValueChangeData(declaration.JE_VesselNameInfo, ZString.Empty, (ZString)"VESSEL1"),
				new InfoValueChangeData(declaration.JE_VoyageFlightNoInfo, ZString.Empty, (ZString)"V234"),
				new InfoValueChangeData(declaration.US_UI_NKCarrierSCACInfo, ZString.Empty, (ZString)"ABC1"),
				new InfoValueChangeData(declaration.US_SchDLoadingInfo, ZString.Empty, (ZString)"FP001"),
				new InfoValueChangeData(declaration.JE_ExportDateInfo, ZDateTime.Empty, new ZDateTime(2012, 10, 1)),
				new InfoValueChangeData(declaration.JE_RL_NKPortOfLoadingInfo, ZString.Empty, (ZString)"UNP01"),
				new InfoValueChangeData(declaration.US_SchDArrivalInfo, ZString.Empty, (ZString)"LP01"),
				new InfoValueChangeData(declaration.JE_DateOfArrivalInfo, ZDateTime.Empty, new ZDateTime(2012, 10, 2)),
				new InfoValueChangeData(declaration.JE_RL_NKPortOfArrivalInfo, ZString.Empty, (ZString)"UNP02"),
				new InfoValueChangeData(declaration.US_SchDEntryInfo, ZString.Empty, (ZString)"LP02"),
				new InfoValueChangeData(declaration.US_EntryDateInfo, ZDateTime.Empty, new ZDateTime(2012, 10, 3, 3, 3, 3)),
				new InfoValueChangeData(declaration.US_IsHMFApplicableInfo, (ZString)"Y", (ZString)"N"),
				new InfoValueChangeData(declaration.JE_DateOfFirstArrivalInfo, ZDateTime.Empty, new ZDateTime(2012, 10, 4)),
				new InfoValueChangeData(declaration.US_ITDateInfo, ZDateTime.Empty, new ZDateTime(2012, 10, 5, 5, 5, 5)),

				new InfoValueChangeData(declaration.JE_HouseBillInfo, ZString.Empty, (ZString)"MB1HB1"),
				new InfoValueChangeData(declaration.JE_RL_NKOriginInfo, (ZString)"UNP01", (ZString)"UNP03"),
				new InfoValueChangeData(declaration.JE_DateAtOriginInfo, new ZDateTime(2012, 10, 1), new ZDateTime(2012, 9, 1, 1, 1, 1)),
				new InfoValueChangeData(declaration.JE_RL_NKFinalDestinationInfo, ZString.Empty, (ZString)"UNP04"),
				new InfoValueChangeData(declaration.JE_DateAtFinalDestinationInfo, new ZDateTime(2012, 10, 3, 3, 3, 3), new ZDateTime(2012, 10, 6, 6, 6, 6)),
				new InfoValueChangeData(declaration.US_DestinationStateInfo, ZString.Empty, (ZString)"DS"),
				new InfoValueChangeData(declaration.US_UC_NKCountryOfExportInfo, (ZString)"UN", (ZString)"CE"),
				new InfoValueChangeData(declaration.US_DateOfExportInfo, ZDateTime.Empty, new ZDateTime(2012, 9, 2, 2, 2, 2)),
				new InfoValueChangeData(declaration.JE_GoodsDescriptionInfo, ZString.Empty, (ZString)"GOODS DESC"),
				new InfoValueChangeData(declaration.JE_OwnerRefInfo, ZString.Empty, (ZString)"OWNREF1"),
				new InfoValueChangeData(declaration.JE_TotalWeightInfo, ZDecimal.Zero, (ZDecimal)1010.10m),
				new InfoValueChangeData(declaration.JE_TotalWeightUnitInfo, ZString.Empty, (ZString)"TU"),
				new InfoValueChangeData(declaration.JE_TotalVolumeInfo, ZDecimal.Zero, (ZDecimal)10.10m),
				new InfoValueChangeData(declaration.JE_TotalVolumeUnitInfo, ZString.Empty, (ZString)"VU"),
				new InfoValueChangeData(declaration.JE_TotalNoOfPiecesInfo, ZInt.Zero, (ZInt)110),
				new InfoValueChangeData(declaration.JE_TotalNoOfPacksInfo, ZInt.Zero, (ZInt)222),
				new InfoValueChangeData(declaration.JE_TotalNoOfPacksPackTypeInfo, ZString.Empty, (ZString)"PT"),

				new InfoValueChangeData(declaration.US_US_NKLocationOfGoodsInfo, ZString.Empty, (ZString)"LG23"),
				new InfoValueChangeData(declaration.US_US_NKCentralizedExamSiteInfo, ZString.Empty, (ZString)"CES1"),
				new InfoValueChangeData(declaration.US_EntryFilerCodeInfo, ZString.Empty, (ZString)"EX2"),
				new InfoValueChangeData(declaration.US_7501PurchasedInfo, ZString.Empty, (ZString)YesNoDefaultList.Codes.Yes),
				new InfoValueChangeData(declaration.US_ManEntryInfo, ZBool.False, ZBool.True),
				new InfoValueChangeData(declaration.US_7501AgentInfo, ZBool.True, ZBool.False),

				new InfoValueChangeData(declaration.JE_OH_ShippingLineInfo, ZGuid.Empty, shippingLine.PK),
				new InfoValueChangeData(declaration.JE_OH_ForwarderInfo, ZGuid.Empty, forwarder.PK),
				new InfoValueChangeData(declaration.ContainerTerminalOperatorDocAddress.E2_OA_AddressInfo, ZGuid.Empty, customsContainerTerminalOperatorAddress.PK),
				new InfoValueChangeData(declaration.DepotDocAddress.E2_OA_AddressInfo, ZGuid.Empty, customsDepotAddress.PK),
				new InfoValueChangeData(declaration.ContainerYardDocAddress.E2_OA_AddressInfo, ZGuid.Empty, customsContainerYardAddress.PK),
				new InfoValueChangeData(declaration.JE_OH_ExternalBrokerInfo, ZGuid.Empty, externalBroker.PK),
				new InfoValueChangeData(declaration.JE_OA_ConsigneeAddressInfo, importer.MainAddress.PK, ultimateConsignee.MainAddress.PK),
				new InfoValueChangeData(declaration.JE_OA_DeclarantAddressInfo, ZGuid.Empty, importerOfRecord.MainAddress.PK),
				new InfoValueChangeData(declaration.JE_OH_NotifyPartyInfo, ZGuid.Empty, notifyParty.PK),

				new InfoValueChangeData(declaration.ImporterDeliveryAddress.E2_OA_AddressInfo, importer.MainAddress.PK, importerPickupDeliveryAddress.PK),
				new InfoValueChangeData(declaration.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo, ZGuid.Empty, deliveryCartageCoAddress.PK),
				new InfoValueChangeData(declaration.DocsAndCartage.JP_FCLAvailableInfo, ZDateTime.Empty, new ZDateTime(2012, 11, 1, 1, 1, 1)),
				new InfoValueChangeData(declaration.DocsAndCartage.JP_FCLStorageCommencesInfo, ZDateTime.Empty, new ZDateTime(2012, 11, 2, 2, 2, 2)),
				new InfoValueChangeData(declaration.DocsAndCartage.JP_FCLDeliveryEquipmentNeededInfo, ZString.Empty, (ZString)"DEN"),
				new InfoValueChangeData(declaration.DocsAndCartage.JP_LCLDatesOverrideConsolInfo, ZBool.False, ZBool.True),
				new InfoValueChangeData(declaration.DocsAndCartage.JP_LCLAvailableInfo, ZDateTime.Empty, new ZDateTime(2012, 11, 3, 3, 3, 3)),
				new InfoValueChangeData(declaration.DocsAndCartage.JP_LCLStorageCommencesInfo, ZDateTime.Empty, new ZDateTime(2012, 11, 4, 4, 4, 4)),
				new InfoValueChangeData(declaration.DocsAndCartage.JP_EstimatedDeliveryInfo, ZDateTime.Empty, new ZDateTime(2012, 11, 5, 5, 5, 5)),
				new InfoValueChangeData(declaration.DocsAndCartage.JP_DeliveryRequiredByInfo, ZDateTime.Empty, new ZDateTime(2012, 11, 6, 6, 6, 6)),
				new InfoValueChangeData(declaration.DocsAndCartage.JP_DeliveryCartageAdvisedInfo, ZDateTime.Empty, new ZDateTime(2012, 11, 7, 7, 7, 7)),
				new InfoValueChangeData(declaration.DocsAndCartage.JP_DeliveryCartageCompletedInfo, ZDateTime.Empty, new ZDateTime(2012, 11, 8, 8, 8, 8)),
				new InfoValueChangeData(declaration.DocsAndCartage.JP_DeliveryLabourChargeInfo, ZDecimal.Zero, (ZDecimal)20.20m),
				new InfoValueChangeData(declaration.DocsAndCartage.JP_DeliveryLabourTimeInfo, ZDateTime.Empty, new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 11, 9, 9, 9, 9)),
				new InfoValueChangeData(declaration.DocsAndCartage.JP_DeliveryTruckWaitChargeInfo, ZDecimal.Zero, (ZDecimal)234.12m),
				new InfoValueChangeData(declaration.DocsAndCartage.JP_DeliveryTruckWaitTimeInfo, ZDateTime.Empty, new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 11, 10, 10, 10, 10)),

				new InfoValueChangeData(declaration.JE_OH_ExporterInfo, ZGuid.Empty, exporter.PK),
				new InfoValueChangeData(declaration.JE_OA_ManufacturerAddressInfo, ZGuid.Empty, manufacturerAddress.PK),
				new InfoValueChangeData(declaration.JE_OA_SellerAddressInfo, ZGuid.Empty, seller.MainAddress.PK),
				new InfoValueChangeData(declaration.JE_OH_SellingAgentInfo, ZGuid.Empty, sellingAgent.PK),
				new InfoValueChangeData(declaration.JE_OA_InvoicerAddressInfo, ZGuid.Empty, invoicerAddress.PK),
				new InfoValueChangeData(declaration.JE_OH_BuyerInfo, ZGuid.Empty, buyer.PK),
				new InfoValueChangeData(declaration.JE_OH_BuyingAgentInfo, ZGuid.Empty, buyingAgent.PK),

				new InfoValueChangeData(declaration.JE_GS_NKCusAgentInfo, ZString.Empty, (ZString)"Z!"),
				new InfoValueChangeData(declaration.JE_MergeByInfo, (ZString)"NON", (ZString)"TRF"),
				new InfoValueChangeData(declaration.US_BRDRefNoInfo, ZString.Empty, (ZString)"BRDREFNO1"),
				// Entry Data
				new InfoValueChangeData(declaration.US_MissingDocument1Info, ZString.Empty, (ZString)"M1"),
				new InfoValueChangeData(declaration.US_MissingDocument2Info, ZString.Empty, (ZString)"M2"),
				new InfoValueChangeData(declaration.US_TeamNoInfo, ZString.Empty, (ZString)"T1"),
				new InfoValueChangeData(declaration.US_OGALineReleaseIndicatorInfo, ZString.Empty, (ZString)YesNoDefaultList.Codes.Yes),
				new InfoValueChangeData(declaration.US_EntryDateElectionCodeInfo, ZString.Empty, (ZString)"E"),
				new InfoValueChangeData(declaration.US_PresentationDateInfo, ZDateTime.Empty, new ZDateTime(2012, 11, 11, 11, 11, 11)),
				new InfoValueChangeData(declaration.US_ConsolidatedInformalIndicatorInfo, ZString.Empty, (ZString)YesNoDefaultList.Codes.Yes),
				new InfoValueChangeData(declaration.US_GeneralOrderNoInfo, ZString.Empty, (ZString)"GO12"),
				new InfoValueChangeData(declaration.US_TaxDeferIndicatorInfo, (ZString)"0", (ZString)YesNoDefaultList.Codes.Yes),
				// Remote Filing Data
				new InfoValueChangeData(declaration.US_PreparerDistrictPortInfo, ZString.Empty, (ZString)"PDP1"),
				new InfoValueChangeData(declaration.US_SchDExamInfo, ZString.Empty, (ZString)"SDE1"),
				new InfoValueChangeData(declaration.US_DESInfo, ZString.Empty, (ZString)"DES1"),
				new InfoValueChangeData(declaration.US_PreparerOfficeCodeInfo, ZString.Empty, (ZString)"P2"),
				new InfoValueChangeData(declaration.US_OtherReconIndicatorInfo, ZString.Empty, (ZString)YesNoDefaultList.Codes.Yes),
				// Reconciliation Data
				new InfoValueChangeData(declaration.US_NAFTAReconIndicatorInfo, ZBool.False, ZBool.True),
				new InfoValueChangeData(declaration.US_FixReconInfo, ZBool.False, ZBool.True),
				// Bond Data
				new InfoValueChangeData(declaration.US_BondTypeInfo, ZString.Empty, (ZString)"B"),
				new InfoValueChangeData(declaration.US_BondAmountInfo, ZDecimal.Zero, (ZDecimal)54m),
				new InfoValueChangeData(declaration.US_BondCalcCodeInfo, ZString.Empty, (ZString)"BC"),
				new InfoValueChangeData(declaration.US_BondProducerAccNoInfo, ZString.Empty, (ZString)"BPA1"),
				new InfoValueChangeData(declaration.US_SuretyCodeInfo, ZString.Empty, (ZString)"SC1"),
				new InfoValueChangeData(declaration.US_ADDCVDSuretyCodeInfo, ZString.Empty, (ZString)"AS1"),
				// Payment Data
				new InfoValueChangeData(declaration.US_PaymentTypeInfo, ZString.Empty, (ZString)"P"),
				new InfoValueChangeData(declaration.JE_PaymentMethodInfo, ZString.Empty, (ZString)"PM"),
				new InfoValueChangeData(declaration.US_PeriodicStatementMMInfo, ZString.Empty, (ZString)"01"),
				new InfoValueChangeData(declaration.US_PreliminaryStatementPrintDateInfo, ZDateTime.Empty, new ZDateTime(2012, 11, 12, 12, 12, 12)),
				new InfoValueChangeData(declaration.US_FixPSDInfo, ZBool.False, ZBool.True),
				new InfoValueChangeData(declaration.US_ClientBranchDesignationInfo, ZString.Empty, (ZString)"CB"),

				new InfoValueChangeData(declaration.US_FDAADTAInfo, ZDateTime.Empty, new ZDateTime(2012, 11, 13, 13, 13, 13)),
				new InfoValueChangeData(declaration.US_FDAAPCInfo, ZString.Empty, (ZString)"APC"),
				// Carrier/Privately Owned Vehicle
				new InfoValueChangeData(declaration.US_FDACANTypeInfo, ZString.Empty, (ZString)"T"),
				new InfoValueChangeData(declaration.US_FDACCNInfo, ZString.Empty, (ZString)"CC"),
				new InfoValueChangeData(declaration.US_FDACANInfo, ZString.Empty, (ZString)"CAN")
			});
			BusinessObject targetBO = declaration;
			AssertDataWasSetInSpecificOrder(expectedOrders, () => ((ITopLevelDataObjectReader)new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory, null)).ReadIntoBusinessObject(ref targetBO));
			Factory.FireCleanupAfterSaving();

			declaration = Factory.New<JobDeclaration>();
			expectedOrders = new List<InfoValueChangeData>(new[]
			{
				new InfoValueChangeData(declaration.JE_OverrideFreightDefaultsInfo, ZBool.False, ZBool.True),
				new InfoValueChangeData(declaration.JE_GBInfo, ZGuid.Empty, branch2.PK),
			});
			targetBO = declaration;
			AssertDataWasSetInSpecificOrder(expectedOrders, () => ((ITopLevelDataObjectReader)new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory, Factory.New<ForwardingShipment>())).ReadIntoBusinessObject(ref targetBO));
			Factory.FireCleanupAfterSaving();
		}

		public void TestDeclarationDefaulting()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var countryData = Factory.New<OrgCountryData>();
			countryData.OV_OH_OrgHeader = importer.PK;
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedStates;

			var addInfo = new OrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
			addInfo.ZO_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			addInfo.ZO_BrokerToPay = YesNoDefaultList.Codes.Yes;
			Factory.SaveForTesting();

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
			declarationDataObject.PaymentMethod = null;
			declarationDataObject.AddOrgAddress(writeManager, importer, Constants.AddressType.ImporterOfRecord);
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("declarationBO.IOROrgPK", importer.PK, declarationBO.IOROrgPK);
			AssertEquals("Payment type defaulted", PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter, declarationBO.US_PaymentType);
			AssertEquals("Broker To Pay Indicator defaulted", YesNoDefaultList.Codes.Yes, declarationBO.BrokerToPayIndicator);
			AssertEquals("Payment Method should be set", Customs.Business.PaymentPartyCodeDescriptionList.Codes.Broker, declarationBO.JE_PaymentMethod);

			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			declarationBO.Delete();
			declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("declarationBO.IOROrgPK", importer.PK, declarationBO.IOROrgPK);
			AssertEquals("Payment type not defaulted", "", declarationBO.US_PaymentType);
			AssertEquals("Broker To Pay Indicator not defaulted", "", declarationBO.BrokerToPayIndicator);
			AssertEquals("Payment Method 'DEF' by default for import", Customs.Business.PaymentPartyCodeDescriptionList.Codes.Default, declarationBO.JE_PaymentMethod);
		}

		public void TestPackNoAndPackType()
		{
			var shipmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataTarget(DataContextType.ForwardingShipment, null);
			shipmentDataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			shipmentDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import, Description = "" };
			shipmentDataObject.OuterPacks = 54;
			shipmentDataObject.OuterPacksPackageType = new PackageType() { Code = "PKG", Description = "" };

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipment = reader.ReadIntoBusinessObject();
			var declaration = (JobDeclaration)shipment.Declarations.FirstOrDefault();

			AssertEquals(54, declaration.JE_TotalNoOfPacks);
			AssertEquals("PK", declaration.JE_TotalNoOfPacksPackType);

			shipmentDataObject.TotalNoOfPacks = 99;
			shipmentDataObject.TotalNoOfPacksPackageType = new PackageType() { Code = "AG", Description = "Pallet, shrinkwrapped" };

			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipment = reader.ReadIntoBusinessObject();
			declaration = (JobDeclaration)shipment.Declarations.FirstOrDefault();

			AssertEquals(99, declaration.JE_TotalNoOfPacks);
			AssertEquals("AG", declaration.JE_TotalNoOfPacksPackType);

			Factory.FireCleanupAfterSaving();
		}

		public void TestOrganizationsOnSubShipmentLevel()
		{
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var org1 = CreateOrganisation("BOB", "ABC!@#1");

			var shipmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S00001");
			shipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C00001");

			var subShipmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipmentDataObject.DataContext = DataContextFactory.New();
			subShipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S00001");
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>(new[] { subShipmentDataObject }));
			subShipmentDataObject.AddOrgAddress(writeManager, org1, Enterprise.Customs.DataTransfer.Universal.Constants.AddressTypes.IntermediateConsignee);

			var reader = new JobDeclarationDataObjectReader(shipmentDataObject, logger, Factory);

			AssertNoExceptionThrown(delegate
			{
				reader.ReadIntoBusinessObject();
			});
		}

		public void TestFillOrganizationForDeclaration()
		{
			var org1 = CreateOrganisation("BOB", "ABC!@#1");
			var org2 = CreateOrganisation("JACK", "ABC!@#2");
			var org3 = CreateOrganisation("JANE", "ABC!@#3");
			var org4 = CreateOrganisation("PETE", "ABC!@#4");
			var org5 = CreateOrganisation("MARY", "ABC!@#5");
			var org6 = CreateOrganisation("MAT", "ABC!@#6");
			var org7 = CreateOrganisation("JOE", "ABC!@#7");
			var org8 = CreateOrganisation("MARK", "ABC!@#8");
			var org9 = CreateOrganisation("SUE", "ABC!@#9");
			var org10 = CreateOrganisation("KATE", "ABC!@#10");
			var org11 = CreateOrganisation("CATE", "ABC!@#11");
			var org12 = CreateOrganisation("TIM", "ABC!@#12");
			var org13 = CreateOrganisation("GARY", "ABC!@#13");
			var org14 = CreateOrganisation("ILYA", "ABC!@#14");
			var org15 = CreateOrganisation("DAVID", "ABC!@#15");
			var org16 = CreateOrganisation("HUYE", "ABC!@#16");
			var org17 = CreateOrganisation("BENQ", "ABC!@#17");
			var org18 = CreateOrganisation("RICH", "ABC!@#18");
			var org19 = CreateOrganisation("BILLY", "ABC!@#19");
			var org20 = CreateOrganisation("SUPL", "ABC!@#20");
			var org21 = CreateOrganisation("IMPO", "ABC!@#21");
			var org22 = CreateOrganisation("HXU", "ABC!@#22");

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var licenseNo = new ZString("LIC1");
			var declarationDataObject = SetupDeclaration(null, "MYMASTER1", new WayBillType() { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master });
			declarationDataObject.SetAddInfoCollection(() => Enterprise.Customs.DataTransfer.Universal.AddInfoCollectionCreator.CreateCollection(string.Format("{0}={1}", USAddInfoSchema.US_LicenseNo.Name.Substring(3), licenseNo)));
			declarationDataObject.AddOrgAddress(writeManager, org1, Enterprise.Customs.DataTransfer.Universal.Constants.AddressTypes.IntermediateConsignee);
			declarationDataObject.AddOrgAddress(writeManager, org2, DocAddressType.UltimateConsignee);
			declarationDataObject.AddOrgAddress(writeManager, org3, Constants.AddressType.ImporterOfRecord);
			declarationDataObject.AddOrgAddress(writeManager, org4, Constants.AddressType.FDASubmitter);
			declarationDataObject.AddOrgAddress(writeManager, org5, Constants.AddressType.CBPBroker);
			declarationDataObject.AddOrgAddress(writeManager, org6, Constants.AddressType.Exporter);
			declarationDataObject.AddOrgAddress(writeManager, org7.MainAddress, DocAddressType.Manufacturer);
			declarationDataObject.AddOrgAddress(writeManager, org8, Constants.AddressType.Seller);
			declarationDataObject.AddOrgAddress(writeManager, org9, Customs.DataTransfer.Universal.Constants.AddressTypes.SellingAgent);
			declarationDataObject.AddOrgAddress(writeManager, org10.MainAddress, Constants.AddressType.Invoicer);
			declarationDataObject.AddOrgAddress(writeManager, org11, DocAddressType.BuyerDocumentaryAddress);
			declarationDataObject.AddOrgAddress(writeManager, org12, Customs.DataTransfer.Universal.Constants.AddressTypes.BuyingAgent);
			declarationDataObject.AddOrgAddress(writeManager, org13, DocAddressType.NotifyParty);
			declarationDataObject.AddOrgAddress(writeManager, org14, Constants.AddressType.SoldToParty);
			declarationDataObject.AddOrgAddress(writeManager, org18, Constants.AddressType.Transferee);
			declarationDataObject.AddOrgAddress(writeManager, org19, "UNKNOWN123ABC");
			declarationDataObject.AddOrgAddress(writeManager, org20, DocAddressType.ConsignorDocumentaryAddress);
			declarationDataObject.AddOrgAddress(writeManager, org21, DocAddressType.ConsigneeDocumentaryAddress);
			declarationDataObject.AddOrgAddress(writeManager, org22, Constants.AddressType.ForeignPrincipalPartyInInterest);

			declarationDataObject.MessageType.Code = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Export;
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(true, declarationBO.IsExport);
			var addInfos = AddInfoParser.CreateDictionaryWithAddInfoString(declarationBO.JE_AddInfo);
			AssertEquals(1, addInfos.Count);
			AssertAddInfosContains(addInfos, licenseNo, USAddInfoSchema.US_LicenseNo);
			AssertEquals(org22.PK, declarationBO.USD_OH_ForeignPrincipalParty);

			declarationDataObject.WayBillNumber = "MYMASTER2";
			declarationDataObject.MessageType.Code = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Import;
			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(true, declarationBO.IsImport);
			AssertEquals(org7.MainAddress.PK, declarationBO.JE_OA_ManufacturerAddress);
			addInfos = AddInfoParser.CreateDictionaryWithAddInfoString(declarationBO.JE_AddInfo);
			AssertEquals(1, addInfos.Count);
			AssertAddInfosContains(addInfos, licenseNo, USAddInfoSchema.US_LicenseNo);
			AssertEquals(org3.MainAddress.PK, declarationBO.JE_OA_DeclarantAddress);
			AssertEquals(org4.PK, declarationBO.JE_OH_FDASubmitter);
			AssertEquals(org5.PK, declarationBO.JE_OH_CBPBroker);
			AssertEquals(org6.PK, declarationBO.JE_OH_Exporter);

			AssertEquals(org8.MainAddress.PK, declarationBO.JE_OA_SellerAddress);
			AssertEquals(org9.PK, declarationBO.JE_OH_SellingAgent);
			AssertEquals(org10.MainAddress.PK, declarationBO.JE_OA_InvoicerAddress);
			AssertEquals(org11.PK, declarationBO.JE_OH_Buyer);
			AssertEquals(org12.PK, declarationBO.JE_OH_BuyingAgent);
			AssertEquals(org13.PK, declarationBO.JE_OH_NotifyParty);
			AssertEquals(org14.MainAddress.PK, declarationBO.JE_OA_SoldToPartyAddress);

			declarationDataObject.WayBillNumber = "MYMASTER3";
			declarationDataObject.AddInfoCollection.Add(UniversalDataBuss.DataObjects.Universal.AddInfo.New("EnableAII", "Y"));
			logger.ClearLogs();
			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(true, declarationBO.IsImport);
			AssertEquals(org7.MainAddress.PK, declarationBO.JE_OA_ManufacturerAddress);
			addInfos = AddInfoParser.CreateDictionaryWithAddInfoString(declarationBO.JE_AddInfo);
			AssertEquals(2, addInfos.Count);
			AssertAddInfosContains(addInfos, licenseNo, USAddInfoSchema.US_LicenseNo);
			AssertEquals(org3.MainAddress.PK, declarationBO.JE_OA_DeclarantAddress);
			AssertEquals(org4.PK, declarationBO.JE_OH_FDASubmitter);
			AssertEquals(org5.PK, declarationBO.JE_OH_CBPBroker);
			AssertEquals(org6.PK, declarationBO.JE_OH_Exporter);

			AssertEquals(org8.MainAddress.PK, declarationBO.JE_OA_SellerAddress);
			AssertEquals(org9.PK, declarationBO.JE_OH_SellingAgent);
			AssertEquals(org10.MainAddress.PK, declarationBO.JE_OA_InvoicerAddress);
			AssertEquals(org11.PK, declarationBO.JE_OH_Buyer);
			AssertEquals(org12.PK, declarationBO.JE_OH_BuyingAgent);
			AssertEquals(org13.PK, declarationBO.JE_OH_NotifyParty);
			AssertEquals(org14.MainAddress.PK, declarationBO.JE_OA_SoldToPartyAddress);
			AssertAddInfosContains(addInfos, ZBool.True, USAddInfoSchema.US_EnableAII);
			AssertEquals("declarationBO.NotifyPartyDocumentaryAddress.E2_OA_Address", org13.MainAddress.PK, declarationBO.NotifyPartyDocumentaryAddress.E2_OA_Address);
			var warningMessage = "Warning - Unknown Address Type [{0}] found. Job Document Address not imported.";
			var logs = logger.Logs;
			var message = string.Format(warningMessage, "UNKNOWN123ABC");
			AssertNotContains(message, logs);
			message = string.Format(warningMessage, Enterprise.Customs.DataTransfer.Universal.Constants.AddressTypes.IntermediateConsignee);
			AssertNotContains(message, logs);
			message = string.Format(warningMessage, nameof(DocAddressType.UltimateConsignee));
			AssertNotContains(message, logs);
			message = string.Format(warningMessage, Constants.AddressType.ImporterOfRecord);
			AssertNotContains(message, logs);
			AssertNotContains(message, logs);
			message = string.Format(warningMessage, Constants.AddressType.FDASubmitter);
			AssertNotContains(message, logs);
			message = string.Format(warningMessage, Constants.AddressType.CBPBroker);
			AssertNotContains(message, logs);
			message = string.Format(warningMessage, Constants.AddressType.Exporter);
			AssertNotContains(message, logs);
			message = string.Format(warningMessage, nameof(DocAddressType.Manufacturer));
			AssertNotContains(message, logs);
			message = string.Format(warningMessage, Constants.AddressType.Seller);
			AssertNotContains(message, logs);
			message = string.Format(warningMessage, Customs.DataTransfer.Universal.Constants.AddressTypes.SellingAgent);
			AssertNotContains(message, logs);
			message = string.Format(warningMessage, Constants.AddressType.Invoicer);
			AssertNotContains(message, logs);
			message = string.Format(warningMessage, nameof(DocAddressType.BuyerDocumentaryAddress));
			AssertNotContains(message, logs);
			message = string.Format(warningMessage, Customs.DataTransfer.Universal.Constants.AddressTypes.BuyingAgent);
			AssertNotContains(message, logs);
			message = string.Format(warningMessage, Constants.AddressType.SoldToParty);
			AssertNotContains(message, logs);
			message = string.Format(warningMessage, Constants.AddressType.Transferee);
			AssertNotContains(message, logs);
			AssertEquals(org20, declarationBO.Supplier);
			AssertEquals(org21, declarationBO.Importer);

			var declarationBOToLoad = Factory.New<JobDeclaration>();
			declarationBOToLoad.JE_MasterBill = "MYMASTER4";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Import;
			declarationBOToLoad.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declarationDataObject.WayBillNumber = "MYMASTER4";
			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(declarationBOToLoad, declarationBO);
			AssertEquals(true, declarationBO.IsImport);
			AssertEquals(org7.MainAddress.PK, declarationBO.JE_OA_ManufacturerAddress);
			AssertEquals(true, declarationBOToLoad.IsACE);
			addInfos = AddInfoParser.CreateDictionaryWithAddInfoString(declarationBO.JE_AddInfo);
			AssertEquals(2, addInfos.Count);
			AssertAddInfosContains(addInfos, licenseNo, USAddInfoSchema.US_LicenseNo);
			AssertEquals(org3.MainAddress.PK, declarationBO.JE_OA_DeclarantAddress);
			AssertEquals(org4.PK, declarationBO.JE_OH_FDASubmitter);
			AssertEquals(org5.PK, declarationBO.JE_OH_CBPBroker);
			AssertEquals(org6.PK, declarationBO.JE_OH_Exporter);

			AssertEquals(org8.MainAddress.PK, declarationBO.JE_OA_SellerAddress);
			AssertEquals(org9.PK, declarationBO.JE_OH_SellingAgent);
			AssertEquals(org10.MainAddress.PK, declarationBO.JE_OA_InvoicerAddress);
			AssertEquals(org11.PK, declarationBO.JE_OH_Buyer);
			AssertEquals(org12.PK, declarationBO.JE_OH_BuyingAgent);
			AssertAddInfosContains(addInfos, ZBool.True, USAddInfoSchema.US_EnableAII);
			AssertEquals(org13.PK, declarationBO.JE_OH_NotifyParty);
			AssertEquals(org14.MainAddress.PK, declarationBO.JE_OA_SoldToPartyAddress);

			declarationDataObject.WayBillNumber = "MYMASTER5";
			declarationDataObject.MessageType.Code = Enterprise.Customs.Business.JobMessageTypeList.Codes.Drawback;
			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(true, declarationBO.IsDrawback);
			addInfos = AddInfoParser.CreateDictionaryWithAddInfoString(declarationBO.JE_AddInfo);
			AssertEquals(3, addInfos.Count);
			AssertAddInfosContains(addInfos, licenseNo, USAddInfoSchema.US_LicenseNo);
			AssertAddInfosContains(addInfos, org18.PK, USAddInfoSchema.US_DRWTransferee);
			AssertAddInfosContains(addInfos, ZBool.True, USAddInfoSchema.US_EnableAII);
			AssertEquals(org13.PK, declarationBO.JE_OH_NotifyParty);
		}

		public void TestImportInBond()
		{
			var inBondData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New()
			};
			inBondData.DataContext.AddDataTarget(DataContextType.InBond, null);
			inBondData.VesselName = "BOB'S BEST VESSEL";
			var declarationDataObject = SetupDeclaration(null, "MYMASTER1", new WayBillType() { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master });
			declarationDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>(new[] { inBondData }));
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			var inBondHeader = declarationBO.InBondHeader;
			AssertNotNull("inBondHeader", inBondHeader);
			AssertEquals(ZBool.True, inBondHeader.BH_OverrideFreightDefaults);
			AssertEquals("BOB'S BEST VESSEL", inBondHeader.BH_ImportConveyanceName);
			Factory.SaveForTesting();
			Factory.FireCleanupAfterSaving();
		}

		public void TestCountrySpecificDetails()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataTarget(DataContextType.ForwardingShipment, null);
			shipmentDataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			shipmentDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Export };
			shipmentDataObject.TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Sea };
			shipmentDataObject.CFSReference = "50603101";
			shipmentDataObject.WayBillNumber = "HB32342";
			shipmentDataObject.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House };

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipment = reader.ReadIntoBusinessObject();
			var declaration = (JobDeclaration)shipment.Declarations.FirstOrDefault();
			AssertEquals("Should be empty when synchronisation is active", "", declaration.US_TransportReference);
			Factory.SaveForTesting();

			declaration.JE_OverrideFreightDefaults = ZBool.True;
			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipment = reader.ReadIntoBusinessObject();
			declaration = (JobDeclaration)shipment.Declarations.FirstOrDefault();
			AssertEquals("50603101", declaration.US_TransportReference);

			declaration.US_TransportReference = ZString.Empty;
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipment = reader.ReadIntoBusinessObject();
			declaration = (JobDeclaration)shipment.Declarations.FirstOrDefault();
			AssertEquals("50603101", declaration.US_TransportReference);

			declaration.US_TransportReference = ZString.Empty;
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			shipmentDataObject.TransportMode.Code = null;
			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipment = reader.ReadIntoBusinessObject();
			declaration = (JobDeclaration)shipment.Declarations.FirstOrDefault();
			AssertEquals("50603101", declaration.US_TransportReference);

			declaration.US_TransportReference = ZString.Empty;
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipment = reader.ReadIntoBusinessObject();
			declaration = (JobDeclaration)shipment.Declarations.FirstOrDefault();
			AssertEquals("50603101", declaration.US_TransportReference);

			declaration.US_TransportReference = ZString.Empty;
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			shipmentDataObject.TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Sea };
			shipmentDataObject.SetAddInfoCollection(() => new List<AddInfo>(new[]
			{
				new AddInfo()
				{
					Key = JobDeclaration.Schema.US_TransportReference.Substring(3),
					Value = "HELLO WORLD"
				}
			}));
			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipment = reader.ReadIntoBusinessObject();
			declaration = (JobDeclaration)shipment.Declarations.FirstOrDefault();
			AssertEquals("HELLO WORLD", declaration.US_TransportReference);

			declaration.US_TransportReference = ZString.Empty;
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipment = reader.ReadIntoBusinessObject();
			declaration = (JobDeclaration)shipment.Declarations.FirstOrDefault();
			AssertEquals("HELLO WORLD", declaration.US_TransportReference);

			declaration.US_TransportReference = ZString.Empty;
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			shipmentDataObject.SetAddInfoCollection(() => null);
			shipmentDataObject.TransportMode.Code = Core.Constants.TransportModes.Air;
			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipment = reader.ReadIntoBusinessObject();
			declaration = (JobDeclaration)shipment.Declarations.FirstOrDefault();
			AssertEquals("HB32342", declaration.US_TransportReference);

			declaration.US_TransportReference = ZString.Empty;
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipment = reader.ReadIntoBusinessObject();
			declaration = (JobDeclaration)shipment.Declarations.FirstOrDefault();
			AssertEquals("HB32342", declaration.US_TransportReference);

			declaration.US_TransportReference = ZString.Empty;
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			shipmentDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipment = reader.ReadIntoBusinessObject();
			declaration = (JobDeclaration)shipment.Declarations.FirstOrDefault();
			AssertEquals("Should be empty as it's not used by Import", "", declaration.US_TransportReference);

			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			shipmentDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipment = reader.ReadIntoBusinessObject();
			declaration = (JobDeclaration)shipment.Declarations.FirstOrDefault();
			AssertEquals("Should be empty as it's not used by Import", "", declaration.US_TransportReference);
			Factory.FireCleanupAfterSaving();
		}

		public void TestImportDeclarationWithBillNumbersAndITNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MasterBill = "12345678";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "CJ6";
			declaration.ImportEntryNumber = "00000172";

			var bill = declaration.Bills.OfType<Bill>().FirstOrDefault();
			bill.ITNumber = "V12345678";
			Factory.SaveForTesting();

			var declarationDataObject = SetupDeclaration(null, "12345678", new WayBillType() { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master });
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
			declarationDataObject.MessagingApplicationCode = new CodeDescriptionPair() { Code = JobApplicationCodeList.Codes.ACE };
			declarationDataObject.SetAdditionalBillCollection(() =>
			{
				var bill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
				{
					BillNumber = "12345678",
					BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master }
				};
				bill1.SetAddInfoCollection(() => new List<AddInfo>()
				{
					new AddInfo() { Key = "", Value = "APLU" }
				});
				bill1.SetAddInfoGroupCollection(() => new List<UniversalCustoms.AddInfoGroup>()
				{
					new UniversalCustoms.AddInfoGroup()
					{
						Type = new CodeDescriptionPair() { Code = "ITN" },
						AddInfoCollection = new List<AddInfo>() { new AddInfo() { Key = "ITNumber", Value = "V23456789" } }
					}
				});

				return new List<AdditionalBill> { bill1 };
			});
			declarationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>()
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					BillNumber = "12345678",
					BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
					PackQty = 100
				}
			});
			declarationDataObject.PackingLineCollection.Content = CollectionContent.Complete;

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory, null);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(declaration.PK, declarationBO.PK);
			declarationBO.Bills.Load();
			var billBO = declarationBO.Bills.OfType<Bill>().FirstOrDefault();
			billBO.ITAndSplitDetails.Load();
			AssertEquals(1, billBO.ITAndSplitDetails.Count);
			AssertEquals("V23456789", billBO.ITAndSplitDetails[0].ITNumber);
			declarationBO.Packages.Load();
			AssertEquals(1, declarationBO.Packages.Count);
			AssertEquals("MB:12345678", declarationBO.Packages[0].CW_HouseBill);
			AssertEquals(100, declarationBO.Packages[0].CW_PackQty);

			declarationDataObject.SetEntryNumberCollection(() => new List<EntryNumber>());
			declarationDataObject.EntryNumberCollection.Add(new EntryNumber()
			{
				Number = "00000172",
				Type = new EntryType() { Code = CusEntryHeaderMessageTypeList.Codes.EntrySummary },
				EntryIsSystemGenerated = true,
			});
			declarationDataObject.GoodsDescription = "TEST MATCH JOB BY ENTRY NUMBER";
			var addInfos = new Dictionary<ZString, ZString>();
			UpdateAddInfo(addInfos, USAddInfoSchema.US_EntryType, (ZString)"");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_EntryFilerCode, (ZString)"CJ6");
			var builder = new ZStringBuilder(addInfos.Select((KeyValuePair<ZString, ZString> pair) => AddInfoParser.Serialise(pair.Key, pair.Value)));
			declarationDataObject.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(builder.ToString()));

			reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory, null);
			reader.ReadIntoBusinessObject();
			AssertEquals("Entry Number should not be updated", "00000172", declaration.ImportEntryNumber);
		}

		public void TestImportDeclarationWithITNumbersButWithoutBillNumbers()
		{
			var declarationDataObject = SetupDeclaration(null, ZString.Empty, new WayBillType() { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master });
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
			declarationDataObject.MessagingApplicationCode = new CodeDescriptionPair() { Code = JobApplicationCodeList.Codes.ACE };
			declarationDataObject.SetAdditionalBillCollection(() =>
			{
				var bill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
				{
					BillNumber = "12345678",
					BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master }
				};
				bill1.SetAddInfoCollection(() => new List<AddInfo>()
				{
					new AddInfo() { Key = "", Value = "APLU" }
				});
				bill1.SetAddInfoGroupCollection(() => new List<UniversalCustoms.AddInfoGroup>()
				{
					new UniversalCustoms.AddInfoGroup()
					{
						Type = new CodeDescriptionPair() { Code = "ITN" },
						AddInfoCollection = new List<AddInfo>() { new AddInfo() { Key = "ITNumber", Value = "V23456789" } }
					}
				});

				return new List<AdditionalBill> { bill1 };
			});

			declarationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>()
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
					PackQty = 100
				}
			});
			declarationDataObject.PackingLineCollection.Content = CollectionContent.Complete;

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory, null);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertNotNull(declarationBO);
			declarationBO.Bills.Load();
			var billBO = declarationBO.Bills.OfType<Bill>().FirstOrDefault();
			AssertEquals(1, billBO.ITAndSplitDetails.Count);
			AssertEquals("V23456789", billBO.ITAndSplitDetails[0].ITNumber);
			declarationBO.Packages.Load();
			AssertEquals(1, declarationBO.Packages.Count);
			AssertEquals(100, declarationBO.Packages[0].CW_PackQty);
		}

		public void TestImportFTZJobsMatchByEntryNumber()
		{
			var declarationDataObject = SetupDeclaration(null, ZString.Empty, null);
			declarationDataObject.GoodsDescription = "TEST MATCH FTZ JOB BY ENTRY NUMBER";

			var addInfos = new Dictionary<ZString, ZString>();
			UpdateAddInfo(addInfos, USAddInfoSchema.US_EntryType, (ZString)"06");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_EntryFilerCode, (ZString)"CJ6");
			var builder = new ZStringBuilder(addInfos.Select((KeyValuePair<ZString, ZString> pair) => AddInfoParser.Serialise(pair.Key, pair.Value)));
			declarationDataObject.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(builder.ToString()));

			declarationDataObject.SetEntryNumberCollection(() => new List<EntryNumber>());
			declarationDataObject.EntryNumberCollection.Add(new EntryNumber()
			{
				Number = "00000172",
				Type = new EntryType() { Code = CusEntryHeaderMessageTypeList.Codes.EntrySummary },
				EntryIsSystemGenerated = true,
			});

			var declarationBOToLoad = Factory.New<JobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GoodsDescription = ZString.Empty;
			declarationBOToLoad.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declarationBOToLoad.US_EntryFilerCode = "SV9";
			declarationBOToLoad.ImportEntryNumber = "00000172";

			var invoice = declarationBOToLoad.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.SaveForTesting();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var newDeclaration = reader.ReadIntoBusinessObject();
			AssertEquals("This job is not matched because Entry Filer Code is different", "", declarationBOToLoad.JE_GoodsDescription);
			AssertEquals("This job is matched by Entry Filer Code and Entry Number", "TEST MATCH FTZ JOB BY ENTRY NUMBER", newDeclaration.JE_GoodsDescription);

			newDeclaration.JE_IsCancelled = true;

			declarationBOToLoad.US_EntryFilerCode = "CJ6";
			Factory.SaveForTesting();
			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertEquals("This job is matched by Entry Filer Code and Entry Number", "TEST MATCH FTZ JOB BY ENTRY NUMBER", declarationBOToLoad.JE_GoodsDescription);
			AssertEquals("Entry Number", "00000172", declarationBOToLoad.ImportEntryNumber);

			var declarationDataObject2 = SetupDeclaration(null, ZString.Empty, null);
			declarationDataObject2.GoodsDescription = "TEST MATCH FTZ JOB BY ENTRY NUMBER";

			var addInfos2 = new Dictionary<ZString, ZString>();
			UpdateAddInfo(addInfos2, USAddInfoSchema.US_EntryType, (ZString)"01");
			UpdateAddInfo(addInfos2, USAddInfoSchema.US_EntryFilerCode, (ZString)"CJ6");
			builder = new ZStringBuilder(addInfos2.Select((KeyValuePair<ZString, ZString> pair) => AddInfoParser.Serialise(pair.Key, pair.Value)));
			declarationDataObject2.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(builder.ToString()));
			declarationDataObject2.SetEntryNumberCollection(() => new List<EntryNumber>());
			declarationDataObject2.EntryNumberCollection.Add(new EntryNumber()
			{
				Number = "00000172",
				Type = new EntryType() { Code = CusEntryHeaderMessageTypeList.Codes.EntrySummary },
				EntryIsSystemGenerated = true,
			});

			declarationBOToLoad.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Factory.SaveForTesting();
			reader = new JobDeclarationDataObjectReader(declarationDataObject2, logger, Factory);
			var reLoadJob = reader.ReadIntoBusinessObject();
			AssertNotEquals("This job is not matched because entry type is not 06, should not using an entry number as a matching criteria", reLoadJob.PK, declarationBOToLoad.PK);
		}

		public void TestCreateDeclarationIfNoMatchesByEntryNumber()
		{
			var declarationDataObject = SetupDeclaration(null, ZString.Empty, null);
			declarationDataObject.GoodsDescription = "TEST FTZ JOB";

			var addInfos = new Dictionary<ZString, ZString>();
			UpdateAddInfo(addInfos, USAddInfoSchema.US_EntryFilerCode, (ZString)"XJ5");
			var builder = new ZStringBuilder(addInfos.Select((KeyValuePair<ZString, ZString> pair) => AddInfoParser.Serialise(pair.Key, pair.Value)));
			declarationDataObject.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(builder.ToString()));

			declarationDataObject.SetEntryNumberCollection(() => new List<EntryNumber>());
			declarationDataObject.EntryNumberCollection.Add(new EntryNumber()
			{
				Number = "00000172",
				Type = new EntryType() { Code = CusEntryHeaderMessageTypeList.Codes.EntrySummary },
				EntryIsSystemGenerated = true,
			});

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			reader.ReadIntoBusinessObject();

			UpdateAddInfo(addInfos, USAddInfoSchema.US_EntryType, (ZString)"06");
			builder = new ZStringBuilder(addInfos.Select((KeyValuePair<ZString, ZString> pair) => AddInfoParser.Serialise(pair.Key, pair.Value)));
			declarationDataObject.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(builder.ToString()));
			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertNotNull(declarationBO);
			AssertEquals("Job created", "TEST FTZ JOB", declarationBO.JE_GoodsDescription);
			AssertEquals("Entry Number", "00000172", declarationBO.ImportEntryNumber);
			AssertEquals("Entry Filer Code", "XJ5", declarationBO.US_EntryFilerCode);

			declarationDataObject.SetEntryNumberCollection(() => new List<EntryNumber>());
			declarationDataObject.EntryNumberCollection.Add(new EntryNumber()
			{
				Number = "00000173",
				Type = new EntryType() { Code = CusEntryHeaderMessageTypeList.Codes.EntrySummary },
				EntryIsSystemGenerated = true,
			});

			UpdateAddInfo(addInfos, USAddInfoSchema.US_EntryType, (ZString)"07");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_EntryFilerCode, (ZString)"XJ5");
			builder = new ZStringBuilder(addInfos.Select((KeyValuePair<ZString, ZString> pair) => AddInfoParser.Serialise(pair.Key, pair.Value)));
			declarationDataObject.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(builder.ToString()));
			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("Entry Number", "00000173", declarationBO.ImportEntryNumber);
		}

		public void TestMatchByEntryNumberForShipmentDeclaration()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_MasterBillNum = "XXXDOB1510161200";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "APLUHB1510161200";
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDestination = "USCHI";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EntryFilerCode = "SV9";
			declaration.ImportEntryNumber = "00000172";

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			declarationDataObject.DataContext = dataContext;
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
			declarationDataObject.GoodsDescription = "TEST MATCH BY ENTRY NUMBER";

			var addInfos = new Dictionary<ZString, ZString>();
			UpdateAddInfo(addInfos, USAddInfoSchema.US_EntryType, (ZString)"06");
			UpdateAddInfo(addInfos, USAddInfoSchema.US_EntryFilerCode, (ZString)"SV9");
			var builder = new ZStringBuilder(addInfos.Select((KeyValuePair<ZString, ZString> pair) => AddInfoParser.Serialise(pair.Key, pair.Value)));
			declarationDataObject.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(builder.ToString()));

			declarationDataObject.SetEntryNumberCollection(() => new List<EntryNumber>());
			declarationDataObject.EntryNumberCollection.Add(new EntryNumber()
			{
				Number = "00000172",
				Type = new EntryType() { Code = CusEntryHeaderMessageTypeList.Codes.EntrySummary },
				EntryIsSystemGenerated = true,
			});
			Factory.SaveForTesting();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory, shipment);

			reader.ReadIntoBusinessObject();
			AssertEquals("This job should be updated", "TEST MATCH BY ENTRY NUMBER", declaration.JE_GoodsDescription);

			var shipment2 = Factory.New<ForwardingShipment>();
			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory, shipment2);
			var dec = reader.ReadIntoBusinessObject();
			AssertContains("Importation is rejected.", "At least one Import Declaration with a matching Entry Filer Code and Entry Number exists. It is for this reason that the data import will not proceed.", logger.Logs);
		}

		public void TestImportBasedOnBillNumber_ShouldConsiderIssuerCode()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterBill = "12345678";

			var bill = declaration1.Bills.OfType<Bill>().FirstOrDefault();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.US_UI_NKBillIssuerSCAC = "123";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MasterBill = "12345678";

			var bill2 = declaration2.Bills.OfType<Bill>().FirstOrDefault();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill2.US_UI_NKBillIssuerSCAC = "321";

			Factory.SaveForTesting();

			var declarationDataObject = SetupDeclaration(null, "12345678", new WayBillType() { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master });
			declarationDataObject.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo()
					{
						Key = Constants.AddInfoKeys.Declaration.WayBillIssuerSCAC,
						Value = "123",
					}
				});

			var additionalBill = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = "12345678",
				BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master }
			};
			additionalBill.SetAddInfoGroupCollection(() => new List<UniversalCustoms.AddInfoGroup> { new UniversalCustoms.AddInfoGroup() });
			additionalBill.SetAddInfoCollection(() => new List<AddInfo>() { new AddInfo() { Key = "UI_NKBillIssuerSCAC", Value = "123" } });

			declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill> { additionalBill });

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory, null);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(declaration1.PK, declarationBO.PK);

			#region Check Contents of declaration Business Object

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("logger.Logs", $@"
Information - Successfully loaded matching JobDeclaration.
Information - Populating JobDeclaration...
Information - Successfully loaded matching Bill.
Information - Populating Bill...
Information - Updated Declaration {declaration1.BrokerReferenceNumber} from UniversalShipment.
			".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestImportWithSameBillNumberAndDifferentIssuerCode_ShouldCreateNewJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterBill = "12345678";

			var bill = declaration.Bills.OfType<Bill>().FirstOrDefault();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.US_UI_NKBillIssuerSCAC = "123";

			Factory.SaveForTesting();

			var declarationDataObject = SetupDeclaration(null, "12345678", new WayBillType() { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master });

			declarationDataObject.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo()
				{
					Key = Constants.AddInfoKeys.Declaration.WayBillIssuerSCAC,
					Value = "321"
				}
			});
			declarationDataObject.SetAdditionalBillCollection(() =>
			{
				var additionalBill = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
				{
					BillNumber = "12345678",
					BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master }
				};
				additionalBill.SetAddInfoCollection(() => new List<AddInfo>()
				{
					new AddInfo() { Key = "UI_NKBillIssuerSCAC", Value = "321" }
				});
				return new List<AdditionalBill> { additionalBill };
			});

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory, null);

			var declarationBO = reader.ReadIntoBusinessObject();

			AssertNotEquals(declaration.PK, declarationBO.PK);

			#region Check Contents of declaration Business Object

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching JobDeclaration found, creating new JobDeclaration.
Information - Populating JobDeclaration...
Information - No matching Bill found, creating new Bill.
Information - Populating Bill...
Information - Added Declaration (Master Bill='12345678') from UniversalShipment.
			".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestSynchroniseFIRMSCodeWhenCreateImportDeclarationUsingCTOAddress()
		{
			var org = Factory.New<OrgHeader>();

			org.OH_Code = "ANDLOG";
			org.OH_FullName = "ANDREWS LOGISTICS SOLUTIONS";
			org.OH_RL_NKClosestPort = "AUSYD";

			var address = Factory.NewWithValidTestData<OrgAddress>();

			address.OA_Address1 = "1 ANDREW ROAD";
			address.OA_City = "ANDREW";
			address.OA_PostCode = "2036";
			address.OA_OH = org.PK;
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			var orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_CodeType = OrgCusCode.USACodeTypes.FIRMSCode;
			orgCusCode.OK_CustomsRegNo = "DCBA";
			orgCusCode.OK_OH = org.PK;
			orgCusCode.OK_OA_PremisesAddress = address.PK;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.SaveForTesting();
			var message = GetQueuedUniversalShipmentMessage(string.Format(DeclarationWithBothCTOXML, JobMessageTypeList.Codes.Import));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should be able to load a Declaration with a Decl Ref of B00001000.", declaration);

			AssertEquals("declaration.JE_MessageType", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertNotNull("declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address", declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address);
			address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address));
			AssertEquals("Address1 Inserted", "1 ANDREW ROAD", address.OA_Address1);
			AssertEquals("Check FIRMS Code.", "DCBA", declaration.US_US_NKLocationOfGoods);
		}

		public void TestSynchroniseFIRMSCodeWhenCreateExportDeclarationUsingCTOAddress()
		{
			var org = Factory.New<OrgHeader>();

			org.OH_Code = "LACLOG";
			org.OH_FullName = "LACHLAN LOGISTICS SOLUTIONS";
			org.OH_RL_NKClosestPort = "AUSYD";

			var address = Factory.NewWithValidTestData<OrgAddress>();

			address.OA_Address1 = "1 LACHLAN ROAD";
			address.OA_City = "LACHLAN";
			address.OA_PostCode = "2036";
			address.OA_OH = org.PK;
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			var orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_CodeType = OrgCusCode.USACodeTypes.FIRMSCode;
			orgCusCode.OK_CustomsRegNo = "DCBA";
			orgCusCode.OK_OH = org.PK;
			orgCusCode.OK_OA_PremisesAddress = address.PK;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(string.Format(DeclarationWithBothCTOXML, JobMessageTypeList.Codes.Export));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should be able to load a Declaration with a Decl Ref of B00001000.", declaration);

			AssertEquals("declaration.JE_MessageType", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			AssertNotNull("declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address", declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address);
			address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address));
			AssertEquals("Address1 Inserted", "1 LACHLAN ROAD", address.OA_Address1);
			AssertEquals("Check FIRMS Code.", "", declaration.US_US_NKLocationOfGoods);
		}

		public void TestSynchroniseFIRMSCodeWhenCreateImportDeclarationUsingDepotAddress()
		{
			var org = Factory.New<OrgHeader>();

			org.OH_Code = "ANDLOG";
			org.OH_FullName = "ANDREWS LOGISTICS SOLUTIONS";
			org.OH_RL_NKClosestPort = "AUSYD";

			var address = Factory.NewWithValidTestData<OrgAddress>();

			address.OA_Address1 = "1 ANDREW ROAD";
			address.OA_City = "ANDREW";
			address.OA_PostCode = "2036";
			address.OA_OH = org.PK;
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			var orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_CodeType = OrgCusCode.USACodeTypes.FIRMSCode;
			orgCusCode.OK_CustomsRegNo = "DCBA";
			orgCusCode.OK_OH = org.PK;
			orgCusCode.OK_OA_PremisesAddress = address.PK;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.SaveForTesting();
			var message = GetQueuedUniversalShipmentMessage(string.Format(DeclarationWithBothCFSXML, JobMessageTypeList.Codes.Import));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should be able to load a Declaration with a Decl Ref of B00001000.", declaration);

			AssertEquals("declaration.JE_MessageType", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertNotNull("declaration.DepotDocAddress.E2_OA_Address", declaration.DepotDocAddress.E2_OA_Address);
			address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, declaration.DepotDocAddress.E2_OA_Address));
			AssertEquals("Address1 Inserted", "1 ANDREW ROAD", address.OA_Address1);
			AssertEquals("Check FIRMS Code.", "DCBA", declaration.US_US_NKLocationOfGoods);
		}

		public void TestSynchroniseFIRMSCodeWhenCreateExportDeclarationUsingDepotAddress()
		{
			var org = Factory.New<OrgHeader>();

			org.OH_Code = "LACLOG";
			org.OH_FullName = "LACHLAN LOGISTICS SOLUTIONS";
			org.OH_RL_NKClosestPort = "AUSYD";

			var address = Factory.NewWithValidTestData<OrgAddress>();

			address.OA_Address1 = "1 LACHLAN ROAD";
			address.OA_City = "LACHLAN";
			address.OA_PostCode = "2036";
			address.OA_OH = org.PK;
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			var orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_CodeType = OrgCusCode.USACodeTypes.FIRMSCode;
			orgCusCode.OK_CustomsRegNo = "DCBA";
			orgCusCode.OK_OH = org.PK;
			orgCusCode.OK_OA_PremisesAddress = address.PK;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(string.Format(DeclarationWithBothCFSXML, JobMessageTypeList.Codes.Export));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should be able to load a Declaration with a Decl Ref of B00001000.", declaration);

			AssertEquals("declaration.JE_MessageType", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			AssertNotNull("declaration.DepotDocAddress.E2_OA_Address", declaration.DepotDocAddress.E2_OA_Address);
			address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, declaration.DepotDocAddress.E2_OA_Address));
			AssertEquals("Address1 Inserted", "1 LACHLAN ROAD", address.OA_Address1);
			AssertEquals("Check FIRMS Code.", "", declaration.US_US_NKLocationOfGoods);
		}

		public void TestSynchroniseFIRMSCodeWhenCreateImportDeclarationUsingCTYAddress()
		{
			var org = Factory.New<OrgHeader>();

			org.OH_Code = "ANDLOG";
			org.OH_FullName = "ANDREWS LOGISTICS SOLUTIONS";
			org.OH_RL_NKClosestPort = "AUSYD";

			var address = Factory.NewWithValidTestData<OrgAddress>();

			address.OA_Address1 = "1 ANDREW ROAD";
			address.OA_City = "ANDREW";
			address.OA_PostCode = "2036";
			address.OA_OH = org.PK;
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			var orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_CodeType = OrgCusCode.USACodeTypes.FIRMSCode;
			orgCusCode.OK_CustomsRegNo = "DCBA";
			orgCusCode.OK_OH = org.PK;
			orgCusCode.OK_OA_PremisesAddress = address.PK;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.SaveForTesting();
			var message = GetQueuedUniversalShipmentMessage(string.Format(DeclarationWithBothCTYXML, JobMessageTypeList.Codes.Import));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should be able to load a Declaration with a Decl Ref of B00001000.", declaration);

			AssertEquals("declaration.JE_MessageType", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertNotNull("declaration.ContainerYardAddress.E2_OA_Address", declaration.ContainerYardDocAddress.E2_OA_Address);
			address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, declaration.ContainerYardDocAddress.E2_OA_Address));
			AssertEquals("Address1 Inserted", "1 ANDREW ROAD", address.OA_Address1);
			AssertEquals("Check FIRMS Code.", "DCBA", declaration.US_US_NKLocationOfGoods);
		}

		public void TestSynchroniseFIRMSCodeWhenCreateExportDeclarationUsingCTYAddress()
		{
			var org = Factory.New<OrgHeader>();

			org.OH_Code = "ANDLOG";
			org.OH_FullName = "ANDREWS LOGISTICS SOLUTIONS";
			org.OH_RL_NKClosestPort = "AUSYD";

			var address = Factory.NewWithValidTestData<OrgAddress>();

			address.OA_Address1 = "1 ANDREW ROAD";
			address.OA_City = "ANDREW";
			address.OA_PostCode = "2036";
			address.OA_OH = org.PK;
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			var orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_CodeType = OrgCusCode.USACodeTypes.FIRMSCode;
			orgCusCode.OK_CustomsRegNo = "DCBA";
			orgCusCode.OK_OH = org.PK;
			orgCusCode.OK_OA_PremisesAddress = address.PK;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.SaveForTesting();
			var message = GetQueuedUniversalShipmentMessage(string.Format(DeclarationWithBothCTYXML, JobMessageTypeList.Codes.Import));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should be able to load a Declaration with a Decl Ref of B00001000.", declaration);

			AssertEquals("declaration.JE_MessageType", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertNotNull("declaration.ContainerYardAddress.E2_OA_Address", declaration.ContainerYardDocAddress.E2_OA_Address);
			address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, declaration.ContainerYardDocAddress.E2_OA_Address));
			AssertEquals("Address1 Inserted", "1 ANDREW ROAD", address.OA_Address1);
			AssertEquals("Check FIRMS Code.", "DCBA", declaration.US_US_NKLocationOfGoods);
		}

		public void TestAddAddInfoFromHVLV()
		{
			var declarationDataObject = SetupDeclaration(null, ZString.Empty, null);
			declarationDataObject.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.HighVolumeLowValue };
			declarationDataObject.DataContext.AddDataSource(DataContextType.HVLVConsignment, "TEST00001");
			declarationDataObject.SetAddInfoCollection(() => new List<AddInfo>());

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory, null);
			var declaration = reader.ReadIntoBusinessObject();
			AssertEquals(EntryTypeList.Codes.LowValue, declaration.US_EntryType);
			AssertEquals(true, declaration.US_7501IOR);

			var declarationDataObject2 = SetupDeclaration(null, ZString.Empty, null);
			declarationDataObject2.SetAddInfoCollection(() => new List<AddInfo>() { AddInfo.New(JobDeclaration.Schema.US_EntryType.Substring(3), EntryTypeList.Codes.ImmediateExportation) });
			declarationDataObject2.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.HighVolumeLowValue };
			declarationDataObject2.DataContext.AddDataSource(DataContextType.HVLVConsignment, "TEST00002");

			var reader2 = new JobDeclarationDataObjectReader(declarationDataObject2, logger, Factory, null);
			var declaration2 = reader2.ReadIntoBusinessObject();
			AssertEquals(EntryTypeList.Codes.ImmediateExportation, declaration2.US_EntryType);

			var declarationDataObject3 = SetupDeclaration(null, ZString.Empty, null);
			declarationDataObject3.SetAddInfoCollection(() => new List<AddInfo>());

			var reader3 = new JobDeclarationDataObjectReader(declarationDataObject3, logger, Factory, null);
			var declaration3 = reader3.ReadIntoBusinessObject();
			AssertEquals(ZString.Empty, declaration3.US_EntryType);
		}

		public void TestPopulateApplicationCodeFromUXML()
		{
			var declarationDataObject = SetupDeclaration(null, null, null, AddInfoCollectionCreator.CreateCollection("EntryType=01*EnableENS=Y"));
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
			declarationDataObject.MessagingApplicationCode = new CodeDescriptionPair() { Code = JobApplicationCodeList.Codes.ACE };

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("declarationBO.JE_MessageType", JobMessageTypeList.Codes.Import, declarationBO.JE_MessageType);
			AssertEquals("declarationBO.JE_ApplicationCode", JobApplicationCodeList.Codes.ACE, declarationBO.JE_ApplicationCode);
			AssertEquals("declarationBO.US_EntryType", EntryTypeList.Codes.ConsumptionFreeDutiable, declarationBO.US_EntryType);
			AssertEquals("declarationBO.US_EnableENS", true, declarationBO.US_EnableENS);

			declarationDataObject = SetupDeclaration(null, null, null, AddInfoCollectionCreator.CreateCollection("EntryType=58*DRWPurpose=DRW"));
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Drawback };
			declarationDataObject.MessagingApplicationCode = new CodeDescriptionPair() { Code = JobApplicationCodeList.Codes.ACE };

			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("declarationBO.JE_MessageType", JobMessageTypeList.Codes.Drawback, declarationBO.JE_MessageType);
			AssertEquals("declarationBO.JE_ApplicationCode", JobApplicationCodeList.Codes.ACE, declarationBO.JE_ApplicationCode);
			AssertEquals("declarationBO.US_EntryType", ACEDrawbackProvisionsList.Codes._58, declarationBO.US_EntryType);
			AssertEquals("declarationBO.US_DRWPurpose", DrawbackDeclarationPurposeList.Codes.DRW, declarationBO.US_DRWPurpose);
		}

		public void TestNoRegularTariffPopulatedInAdditionalImportTariffCollection()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			importDeclaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			importDeclaration.US_EnableENS = true;
			importDeclaration.US_EntryFilerCode = "XJ5";
			importDeclaration.ImportEntryNumber = "12345678";
			importDeclaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			importDeclaration.JE_EntryAuthorisationDate = new ZDateTime(2020, 12, 22);
			var invoice = importDeclaration.Invoices.AddNew();
			var invoiceLineOne = invoice.JobComInvoiceLines.AddNew();
			invoiceLineOne.JI_Tariff = tariff4202924500.UE_Tariff;
			invoiceLineOne.US_UC_NKCountryOfExport = "CN";
			invoiceLineOne.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineOne.US_SupTariff = tariff99038803.UE_Tariff;
			var invoiceLineTwo = invoice.JobComInvoiceLines.AddNew();
			invoiceLineTwo.JI_Tariff = tariff4202924500.UE_Tariff;
			invoiceLineTwo.US_UC_NKCountryOfExport = "AU";
			invoiceLineTwo.US_UC_NKCountryOfOrigin = "AU";
			importDeclaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.SaveForTesting();

			var formalEntry = importDeclaration.FormalEntry;
			AssertEquals(3, formalEntry.AllEntryLines.Count);

			var invoiceLineDataObject1 = SetupCommercialInvoiceLine(1, null, null);
			invoiceLineDataObject1.HarmonisedCode = tariff4202924500.UE_Tariff;
			invoiceLineDataObject1.AddInfoCollection = AddInfoCollectionCreator.CreateCollection("DRWIsForImportSection=Y*DRWImpActInd=E*ImportEntryNo=XJ512345678*DRWImportEntryLine=1");
			invoiceLineDataObject1.AddInfoGroupCollection = new List<UniversalCustoms.AddInfoGroup>()
			{
				new UniversalCustoms.AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USDrawbackAdditionalImportTariffNumber },
					AddInfoCollection = AddInfoCollectionCreator.CreateCollection("LineNo=1*Tariff=99038803")
				}
			};
			var invoiceLineDataObject2 = SetupCommercialInvoiceLine(2, null, null);
			invoiceLineDataObject2.HarmonisedCode = tariff4202924500.UE_Tariff;
			invoiceLineDataObject2.AddInfoCollection = AddInfoCollectionCreator.CreateCollection("DRWIsForImportSection=Y*DRWImpActInd=E*ImportEntryNo=XJ512345678*DRWImportEntryLine=2");

			var declarationDataObject = SetupDeclaration(null, null, null, AddInfoCollectionCreator.CreateCollection("DRWPurpose=DRW*EntryType=58"));
			declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group", null, new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { SetupCommercialInvoiceHeaderData(null, null, new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject1, invoiceLineDataObject2 })) }), null);
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Drawback };
			declarationDataObject.MessagingApplicationCode = new CodeDescriptionPair() { Code = JobApplicationCodeList.Codes.ACE };

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var drawbackBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var newFactory = new BusinessObjectFactory();
			var drawbackLoaded = newFactory.Load<JobDeclaration>(drawbackBO.PK);
			AssertEquals("drawbackBO.InvoiceLines.Count", 2, drawbackLoaded.InvoiceLines.Count);
			var invoiceLineBO1 = drawbackLoaded.InvoiceLines[0];
			AssertEquals("invoiceLineBO1.JI_Tariff", tariff4202924500.UE_Tariff, invoiceLineBO1.JI_Tariff);
			AssertEquals("invoiceLineBO1.DrawbackAdditionalImportTariffNumbers.Count", 1, invoiceLineBO1.DrawbackAdditionalImportTariffNumbers.Count);
			AssertEquals("invoiceLineBO1.DrawbackAdditionalImportTariffNumbers[0].US_Tariff", tariff99038803.UE_Tariff, invoiceLineBO1.DrawbackAdditionalImportTariffNumbers[0].US_Tariff);
			var invoiceLineBO2 = drawbackLoaded.InvoiceLines[1];
			AssertEquals("invoiceLineBO2.JI_Tariff", tariff4202924500.UE_Tariff, invoiceLineBO2.JI_Tariff);
			AssertEquals("invoiceLineBO2.DrawbackAdditionalImportTariffNumbers.Count", 0, invoiceLineBO2.DrawbackAdditionalImportTariffNumbers.Count);
		}

		public void TestCalculateDutyRateFromBothRegularTariffAndAdditionalTariff()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			importDeclaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			importDeclaration.US_EnableENS = true;
			importDeclaration.US_EntryFilerCode = "XJ5";
			importDeclaration.ImportEntryNumber = "12345678";
			importDeclaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			importDeclaration.JE_EntryAuthorisationDate = new ZDateTime(2020, 12, 22);
			var invoice = importDeclaration.Invoices.AddNew();
			var invoiceLineOne = invoice.JobComInvoiceLines.AddNew();
			invoiceLineOne.JI_Tariff = tariff4202924500.UE_Tariff;
			invoiceLineOne.US_UC_NKCountryOfExport = "CN";
			invoiceLineOne.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineOne.US_SupTariff = tariff99038803.UE_Tariff;
			invoiceLineOne.JI_InvoiceUQ = "NO";
			invoiceLineOne.JI_InvoiceQuantity = 1000m;
			invoiceLineOne.JI_LinePrice = 800m;
			var invoiceLineTwo = invoice.JobComInvoiceLines.AddNew();
			invoiceLineTwo.JI_Tariff = tariff4202924500.UE_Tariff;
			invoiceLineTwo.US_UC_NKCountryOfExport = "AU";
			invoiceLineTwo.US_UC_NKCountryOfOrigin = "AU";
			invoiceLineTwo.JI_InvoiceUQ = "NO";
			invoiceLineTwo.JI_InvoiceQuantity = 2000m;
			invoiceLineTwo.JI_LinePrice = 500m;
			importDeclaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.SaveForTesting();

			var formalEntry = importDeclaration.FormalEntry;
			AssertEquals(3, formalEntry.AllEntryLines.Count);

			var invoiceLineDataObject1 = SetupCommercialInvoiceLine(1, null, null);
			invoiceLineDataObject1.HarmonisedCode = tariff4202924500.UE_Tariff;
			invoiceLineDataObject1.AddInfoCollection = AddInfoCollectionCreator.CreateCollection("DRWClaimAmountOverriden_New=Y*DRWCalcDutyWithAdValoremRate=N*DRWCMCDIndicator=E*DRWDeclaredHMF=1.63*DRWDeclaredMPF=26.79*DRWDeclaredVFD=800*DRWLineDuty=360*DRWIsForImportSection=Y*DRWImpActInd=E*DRWExportAction=E*DRWEntryDate=27-10-2020*DRWExportQuantity=100*DRWExportUQ=NO*ImportEntryNo=XJ512345678*DRWImportEntryLine=1*DRWImportQuantity=1000*DRWImportUQ=NO*DRWIsForExportSection=Y*TariffType=HTS*ExportTariff=4202924500");
			invoiceLineDataObject1.AddInfoGroupCollection = new List<UniversalCustoms.AddInfoGroup>()
			{
				new UniversalCustoms.AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USDrawbackAdditionalImportTariffNumber },
					AddInfoCollection = AddInfoCollectionCreator.CreateCollection("LineNo=1*Tariff=99038803")
				}
			};
			var invoiceLineDataObject2 = SetupCommercialInvoiceLine(2, null, null);
			invoiceLineDataObject2.HarmonisedCode = tariff4202924500.UE_Tariff;
			invoiceLineDataObject2.AddInfoCollection = AddInfoCollectionCreator.CreateCollection("DRWClaimAmountOverriden_New=Y*DRWCalcDutyWithAdValoremRate=N*DRWCMCDIndicator=E*DRWDeclaredHMF=1.63*DRWDeclaredMPF=26.79*DRWDeclaredVFD=800*DRWLineDuty=100*DRWIsForImportSection=Y*DRWImpActInd=E*DRWExportAction=E*DRWEntryDate=27-10-2020*DRWExportQuantity=200*DRWExportUQ=NO*ImportEntryNo=XJ512345678*DRWImportEntryLine=2*DRWImportQuantity=2000*DRWImportUQ=NO*DRWIsForExportSection=Y*TariffType=HTS*ExportTariff=4202924500");

			var declarationDataObject = SetupDeclaration(null, null, null, AddInfoCollectionCreator.CreateCollection("DRWPurpose=DRW*EntryType=58"));
			declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group", null, new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { SetupCommercialInvoiceHeaderData(null, null, new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject1, invoiceLineDataObject2 })) }), null);
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Drawback };
			declarationDataObject.MessagingApplicationCode = new CodeDescriptionPair() { Code = JobApplicationCodeList.Codes.ACE };

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var drawbackBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var newFactory = new BusinessObjectFactory();
			var drawbackLoaded = newFactory.Load<JobDeclaration>(drawbackBO.PK);
			AssertEquals("drawbackBO.InvoiceLines.Count", 2, drawbackLoaded.InvoiceLines.Count);
			var invoiceLineBO1 = drawbackLoaded.InvoiceLines[0];
			AssertEquals("invoiceLineBO1.US_DRWClaimAmountOverriden_New", true, invoiceLineBO1.US_DRWClaimAmountOverriden_New);
			AssertEquals("invoiceLineBO1.LineDuty", 360m, invoiceLineBO1.LineDuty);
			AssertEquals("invoiceLineBO1.LineDutyRateDesc", "20%+25%", invoiceLineBO1.LineDutyRateDesc);
			var invoiceLineBO2 = drawbackLoaded.InvoiceLines[1];
			AssertEquals("invoiceLineBO2.US_DRWClaimAmountOverriden_New", true, invoiceLineBO2.US_DRWClaimAmountOverriden_New);
			AssertEquals("invoiceLineBO2.LineDuty", 100m, invoiceLineBO2.LineDuty);
			AssertEquals("invoiceLineBO2.LineDutyRateDesc", "20%", invoiceLineBO2.LineDutyRateDesc);
		}

		[StressTest]
		public void TestFetchHintIsUsedWhenImportFromProductClassification()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var importer = CreateOrganisation("IMPORTER", "#@923@#4");
			for (var i = 0; i < 10; i++)
			{
				var part = Factory.New<Business.OrgSupplierPart>();
				part.OP_PartNum = "PD3234" + i;
				part.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Both);
				var pivot = part.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot.CI_TariffNum = "12345678";
				pivot.CD_ACEFDAIndicator = OGAIndicatorList.Codes.Disclaimed;
				pivot.CD_ACEFDADisclaimReason = PGADisclaimReasonList.Codes.A;
			}

			Factory.SaveForTesting();

			var commercialInvoiceLineDataObjectList = new DataObjectList<UniversalCustoms.CommercialInvoiceLine>();
			for (var i = 0; i < 50; i++)
			{
				var invocieLineDataObject = SetupCommercialInvoiceLine(i, null, null);
				invocieLineDataObject.PartNo = "PD3234" + (i % 10);
				commercialInvoiceLineDataObjectList.Add(invocieLineDataObject);
			}

			var declarationDataObject = SetupDeclaration(null, null, null, AddInfoCollectionCreator.CreateCollection("EnableENS=Y*EntryType=01"));
			declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group", null, new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { SetupCommercialInvoiceHeaderData(null, null, commercialInvoiceLineDataObjectList) }), null);
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			declarationDataObject.AddOrgAddress(writeManager, importer, AddressTypes.Importer);

			var newUniversalObjectFactory = new UniversalObjectFactory();
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, newUniversalObjectFactory);
			var declarationBO = reader.ReadIntoBusinessObject();
			var expectedDbHits = new Dictionary<string, int>()
			{
					{ CusUSClassification.Schema.TableName, 1 }
			};
			AssertDbHits(expectedDbHits, newUniversalObjectFactory.BOFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true);
			AssertEquals(50, declarationBO.InvoiceLines.Count);
		}

		public void TestFDADatePopulatedAfterDeclarationImport()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1234567890";
			tariff.UE_PGACodes = "FD3";
			tariff.UE_DateFrom = new ZDateTime(2021, 01, 01);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			Factory.SaveForTesting();

			var invoiceLineDataObject = SetupCommercialInvoiceLine(1, null, null);
			invoiceLineDataObject.HarmonisedCode = tariff.UE_Tariff;
			var declarationDataObject = SetupDeclaration(null, null, null, AddInfoCollectionCreator.CreateCollection("EnableENS=Y*EntryType=01*EntryDate=2021-02-24"));
			declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group", null, new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { SetupCommercialInvoiceHeaderData(null, null, new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject })) }), null);
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
			declarationDataObject.MessagingApplicationCode = new CodeDescriptionPair() { Code = JobApplicationCodeList.Codes.ACE };

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(ZDateTime.Empty, declarationBO.US_FDAADTA);

			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(new ZDateTime(2021, 02, 24), declarationBO.US_FDAADTA);

			declarationDataObject = SetupDeclaration(null, null, null, AddInfoCollectionCreator.CreateCollection("EnableENS=Y*EntryType=01*EntryDate=2021-02-24*FDAADTA=2021-02-01"));
			declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group", null, new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { SetupCommercialInvoiceHeaderData(null, null, new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject })) }), null);
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
			declarationDataObject.MessagingApplicationCode = new CodeDescriptionPair() { Code = JobApplicationCodeList.Codes.ACE };

			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(new ZDateTime(2021, 02, 01), declarationBO.US_FDAADTA);
		}

		protected override void SetUp()
		{
			base.SetUp();

			tariff4202924500 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "4202924500")).LastOrDefault();
			if (tariff4202924500 == null)
			{
				tariff4202924500 = Factory.New<USCTariff>();
				tariff4202924500.UE_Tariff = "4202924500";
				tariff4202924500.UE_DutyComputationCode = "7";
				tariff4202924500.UE_Column1RateAdValorem = 0.2m;
				tariff4202924500.UE_Unit1 = "NO";
			}
			tariff4202924500.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff4202924500.UE_DateTo = ZDateTime.MaxSmallDateTime;

			tariff99038803 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99038803")).LastOrDefault();
			if (tariff99038803 == null)
			{
				tariff99038803 = Factory.New<USCTariff>();
				tariff99038803.UE_Tariff = "99038803";
				tariff99038803.UE_DutyComputationCode = "7";
				tariff99038803.UE_Column1RateAdValorem = 0.25m;
			}
			tariff99038803.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff99038803.UE_DateTo = ZDateTime.MaxSmallDateTime;
		}

		USCTariff tariff4202924500;
		USCTariff tariff99038803;

		void UpdateAddInfo(Dictionary<ZString, ZString> addInfos, SchemaColumn column, IZType value)
		{
			var key = column.Name.Substring(3);
			addInfos.Update(key, value);
		}

		void AssertAddInfosContains(Dictionary<ZString, ZString> addInfos, IZType value, SchemaColumn addInfoColumn)
		{
			var key = addInfoColumn.Name.Substring(3);
			if (addInfos.TryGetValue(key, out var addInfoValue))
			{
				AssertEquals(addInfoColumn.Name, AutoUSAddInfo.GetStringRepresentation(value), addInfoValue);
			}
			else
			{
				Fail("AddInfos does not contain key " + key);
			}
		}

		sealed class EDIMessageForTest : MQEDIMessage
		{
			public EDIMessageForTest(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			protected override string GetMessageReferenceNumber() => EM_MessageNum + "888";
		}

		const string DeclarationWithBothCTOXML = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <Company>
        <Code>XXX</Code>
        <Country>
          <Code>NZ</Code>
          <Name>New Zealand</Name>
        </Country>
        <Name>XXX VAN BUGGER FORWARDING</Name>
      </Company>

			<RecipientRoleCollection>
        <RecipientRole>
          <Code>BRO</Code>
          <Description>Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <MessageType>
      <Code>{0}</Code>
    </MessageType>

    <OrganizationAddressCollection>
      <OrganizationAddress>
		<AddressType>ArrivalCTOAddress</AddressType>
        <AddressShortCode>ANDREWS LOGISTICS</AddressShortCode>
        <OrganizationCode>ANDLOG</OrganizationCode>
        <Address1>1 ANDREW ROAD</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>ANDREW</City>
        <CompanyName>ANDREWS LOGISTICS SOLUTIONS</CompanyName>
        <Contact>ANDREW</Contact>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2036</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>DepartureCTOAddress</AddressType>
        <AddressShortCode>LACHLANS LOGISTICS</AddressShortCode>
        <OrganizationCode>LACLOG</OrganizationCode>
        <Address1>1 LACHLAN STREET</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>SYDNEY</City>
        <CompanyName>LACHLANS LOGISTICS COMPANY</CompanyName>
        <Contact>LACHLAN</Contact>
        <Country>
          <Code>AU</Code>
          <Name>AUSTRALIA</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <GovRegNumType>
          <Code>EIN</Code>
          <Description>Employer Identification Number</Description>
        </GovRegNumType>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>SYDNEY</Name>
        </Port>
        <Postcode>2036</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		const string DeclarationWithBothCFSXML = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <Company>
        <Code>XXX</Code>
        <Country>
          <Code>NZ</Code>
          <Name>New Zealand</Name>
        </Country>
        <Name>XXX VAN BUGGER FORWARDING</Name>
      </Company>

			<RecipientRoleCollection>
        <RecipientRole>
          <Code>BRO</Code>
          <Description>Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <MessageType>
      <Code>{0}</Code>
    </MessageType>

    <OrganizationAddressCollection>
      <OrganizationAddress>
		<AddressType>ArrivalCFSAddress</AddressType>
        <AddressShortCode>ANDREWS LOGISTICS</AddressShortCode>
        <OrganizationCode>ANDLOG</OrganizationCode>
        <Address1>1 ANDREW ROAD</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>ANDREW</City>
        <CompanyName>ANDREWS LOGISTICS SOLUTIONS</CompanyName>
        <Contact>ANDREW</Contact>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2036</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>DepartureCFSAddress</AddressType>
        <AddressShortCode>LACHLANS LOGISTICS</AddressShortCode>
        <OrganizationCode>LACLOG</OrganizationCode>
        <Address1>1 LACHLAN STREET</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>SYDNEY</City>
        <CompanyName>LACHLANS LOGISTICS COMPANY</CompanyName>
        <Contact>LACHLAN</Contact>
        <Country>
          <Code>AU</Code>
          <Name>AUSTRALIA</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <GovRegNumType>
          <Code>EIN</Code>
          <Description>Employer Identification Number</Description>
        </GovRegNumType>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>SYDNEY</Name>
        </Port>
        <Postcode>2036</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		const string DeclarationWithBothCTYXML = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <Company>
        <Code>XXX</Code>
        <Country>
          <Code>NZ</Code>
          <Name>New Zealand</Name>
        </Country>
        <Name>XXX VAN BUGGER FORWARDING</Name>
      </Company>

			<RecipientRoleCollection>
        <RecipientRole>
          <Code>BRO</Code>
          <Description>Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <MessageType>
      <Code>{0}</Code>
    </MessageType>

    <OrganizationAddressCollection>
      <OrganizationAddress>
		<AddressType>ContainerYardAddress</AddressType>
        <AddressShortCode>ANDREWS LOGISTICS</AddressShortCode>
        <OrganizationCode>ANDLOG</OrganizationCode>
        <Address1>1 ANDREW ROAD</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>ANDREW</City>
        <CompanyName>ANDREWS LOGISTICS SOLUTIONS</CompanyName>
        <Contact>ANDREW</Contact>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2036</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ContainerYardAddress</AddressType>
        <AddressShortCode>LACHLANS LOGISTICS</AddressShortCode>
        <OrganizationCode>LACLOG</OrganizationCode>
        <Address1>1 LACHLAN STREET</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>SYDNEY</City>
        <CompanyName>LACHLANS LOGISTICS COMPANY</CompanyName>
        <Contact>LACHLAN</Contact>
        <Country>
          <Code>AU</Code>
          <Name>AUSTRALIA</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <GovRegNumType>
          <Code>EIN</Code>
          <Description>Employer Identification Number</Description>
        </GovRegNumType>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>SYDNEY</Name>
        </Port>
        <Postcode>2036</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";
	}
}
