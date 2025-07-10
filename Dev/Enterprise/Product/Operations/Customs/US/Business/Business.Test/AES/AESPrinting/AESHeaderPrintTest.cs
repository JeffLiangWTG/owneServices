using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AES.Testing
{
	[TestedType(typeof(AESHeaderPrint))]
	sealed class AESHeaderPrintTest : AESPrintTest
	{
		public void TestProperties()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3901", "CHICAGO, IL", startDate, endDate);
			newFactory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.US_TransportReference = "Transportation Reference";
			declaration.US_DateOfExport = ZDateTime.Today;
			declaration.US_RN_NKCountryOfDestination = "GB";
			declaration.US_SchDExport = "3901";

			var supplier = CreateOrganisation("SUPPLIER", "USLAX", "Test Address 1", "Test Address 2", "CITY", "HY", "203001");
			var pickup = CreateOrganisation("SUPPLIER", "USLAX", "pickup Address 1", "pickup Address 2", "San Francisco", "CA", "111111");
			var importer = CreateOrganisation("IMPORTER", "AUSYD", "Importer Address 1", "Importer Address 2", "london", "TE", "13189");
			var forwarder = CreateOrganisation("FORWARDER", "PRGUY", "Forwarder Address 1", "FWD Address 2", "FWDER", "TX", "00001");
			var intermediateConsignee = CreateOrganisation("INTERMEDIATE", "AUSYD", "Intermediate Address 1", "INT Address 2", "INTER", "INT", "00002");

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Forwarder = forwarder.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			invoice.US_ImportEntryNo = "12456";
			invoice.US_StateOfOrigin = "IL";
			invoice.US_RoutedTransaction = YesNoDefaultList.Codes.Yes;
			invoice.US_HazardousCargo = YesNoDefaultList.Codes.Yes;
			invoice.JZ_OH_Consignee = intermediateConsignee.PK;
			invoice.US_IntermediateConsignee.ZO_Contact = "CONTACT LASTNAME";
			intermediateConsignee.MainAddress.OA_Phone = "963766";
			invoice.US_UltimateConsigneeType = UltimateConsigneeTypeList.Codes.DirectConsumer;
			invoice.SupplierPickupAddress.E2_OA_Address = pickup.MainAddress.PK;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1010101010";
			invoiceLine1.JI_Description = "DESCRIPTION 1";
			invoiceLine1.US_ExportCode = ExportInformationCodeList.Codes.OS;
			invoiceLine1.US_LicenseType = USAESLicenseCode.Codes.C33;
			invoiceLine1.US_LicenseNo = "LCN1234";
			invoiceLine1.JI_Weight = 145m;
			invoiceLine1.JI_CustomsQuantity = 24m;
			invoiceLine1.JI_CustomsUnitQty = AESUnitOfMeasureList.Codes.Barrels;
			invoiceLine1.JI_CustomsSecondQuantity = 46m;
			invoiceLine1.JI_CustomsSecondUnitQty = AESUnitOfMeasureList.Codes.Pairs;
			invoiceLine1.US_ECCN = "EC12";
			invoiceLine1.JI_LinePrice = 1500m;
			invoiceLine1.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Domestic;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);

			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "Reference1";

			var headerForPrinting = new AESHeaderPrint(entryHeader);
			AssertEquals("Related Companies", "Yes", headerForPrinting.RelatedCompaniesIndicator);
			AssertEquals("Transportation Reference", "Transportation Reference", headerForPrinting.TransportationReferenceNo);
			AssertEquals("MOT", "Rail, Non-container (20)", headerForPrinting.ModeOfTransportation);
			AssertEquals("Date of Export", ZDateTime.Today, headerForPrinting.DepartureDate);
			AssertEquals("Shipment Reference Number", "Reference1", headerForPrinting.ShipmentReferenceNumber);
			AssertEquals("State Of Origin", "CALIFORNIA (CA)", headerForPrinting.StateOfOrigin);
			AssertEquals("Country of Destination", "AUSTRALIA (AU)", headerForPrinting.CountryOfDestination);
			AssertEquals("Port Of Export", "CHICAGO, IL (3901)", headerForPrinting.PortOfExport);
			AssertEquals("Routed Transaction", "Yes", headerForPrinting.RoutedTransactionIndicator);
			AssertEquals("Hazardous Indicator", "Yes", headerForPrinting.HazardousIndicator);
			AssertEquals("UltimateConsigneeType", "Direct Consumer", headerForPrinting.UltimateConsigneeTypeFormat);

			AssertEquals("USPPICargoOriginLine1", "PICKUP ADDRESS 1", headerForPrinting.USPPICargoOriginLine1);
			AssertEquals("USPPICargoOriginLine2", "PICKUP ADDRESS 2", headerForPrinting.USPPICargoOriginLine2);
			AssertEquals("USPPICargoOriginLine3", "SAN FRANCISCO CA US 111111", headerForPrinting.USPPICargoOriginLine3);

			AssertEquals("UltimateConsigneeAddress1", "IMPORTER ADDRESS 1", headerForPrinting.UltimateConsigneeAddress1);
			AssertEquals("UltimateConsigneeAddress2", "IMPORTER ADDRESS 2", headerForPrinting.UltimateConsigneeAddress2);
			AssertEquals("UltimateConsigneeAddress3", "LONDON AU 13189", headerForPrinting.UltimateConsigneeAddress3);

			AssertEquals("FreightForwarderAddress1", "FORWARDER ADDRESS 1", headerForPrinting.FreightForwarderAddress1);
			AssertEquals("FreightForwarderAddress2", "FWD ADDRESS 2", headerForPrinting.FreightForwarderAddress2);
			AssertEquals("FreightForwarderAddress3", "FWDER PR US 00001", headerForPrinting.FreightForwarderAddress3);

			AssertEquals("INTERMEDIATE ADDRESS 1", headerForPrinting.IntermediateConsigneeAddress1);
			AssertEquals("INT ADDRESS 2", headerForPrinting.IntermediateConsigneeAddress2);
			AssertEquals("INTER AU 00002", headerForPrinting.IntermediateConsigneeAddress3);
			AssertEquals("CONTACT LASTNAME", headerForPrinting.IntermediateConsigneeContact);
			AssertEquals("INTERMEDIATE", headerForPrinting.IntermediateConsigneeName);
			AssertEquals("963766", headerForPrinting.IntermediateConsigneePhone);

			importer.MainAddress.OA_Address2 = ZString.Empty;
			headerForPrinting = new AESHeaderPrint(entryHeader);
			AssertEquals("UltimateConsigneeAddress1", "IMPORTER ADDRESS 1", headerForPrinting.UltimateConsigneeAddress1);
			AssertEquals("UltimateConsigneeAddress2", "LONDON AU 13189", headerForPrinting.UltimateConsigneeAddress2);
			AssertEquals("UltimateConsigneeAddress3", ZString.Empty, headerForPrinting.UltimateConsigneeAddress3);
		}

		public void TestIParentDocManagerSupportMembers()
		{
			var headerForPrinting = new AESHeaderPrint(Entry);

			IParentDocManagerSupport supporter = headerForPrinting;
			AssertEquals(Core.Constants.DocManagerCodes.CustomsEntry, supporter.DocManagerInfo.DocManagerCode);
			AssertEquals("ParentGuid", Entry.PK, supporter.ParentGuid);
			AssertEquals("ParentTableName", Entry.TableName, supporter.ParentTableName);
		}

		public void TestIDocumentDeliveredLogSupporterMembers()
		{
			var headerForPrinting = new AESHeaderPrint(Entry);

			IDocumentDeliveredLogSupporter supporter = headerForPrinting;
			AssertEquals("BusinessObjectTypeToLogAgainst", typeof(CusEntryHeader), supporter.BusinessObjectTypeToLogAgainst);
			AssertEquals("Identifier", Entry.PK, supporter.Identifier);
		}

		OrgHeader CreateOrganisation(ZString fullName, ZString closestPort, ZString address1, ZString address2, ZString city, ZString state, ZString postCode)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = fullName;
			org.OH_RL_NKClosestPort = closestPort;
			org.MainAddress.OA_Address1 = address1;
			org.MainAddress.OA_Address2 = address2;
			org.MainAddress.OA_City = city;
			org.MainAddress.OA_State = state;
			org.MainAddress.OA_PostCode = postCode;
			return org;
		}

		protected override AESPrint GetObjectForTest() => new AESHeaderPrint(Entry);

		protected override BusinessObject GetNewBusinessObject() => new AESHeaderPrint(Entry);
	}
}
