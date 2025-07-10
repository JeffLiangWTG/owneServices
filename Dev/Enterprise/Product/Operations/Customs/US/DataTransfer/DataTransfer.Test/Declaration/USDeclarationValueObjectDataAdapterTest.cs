using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(USDeclarationValueObjectDataAdapter))]
	sealed class USDeclarationValueObjectDataAdapterTest : DeclarationValueObjectDataAdapterAbstractTest
	{
		public void TestSomePropertiesMaxLengthExceeded()
		{
			Xsd.ConsolAndShipment value = new Xsd.ConsolAndShipment();
			value.Shipment.Declaration.CountryPayload.USDeclaration.Ports.DesignatedExamPort = "SOMETESTPORT";
			value.Shipment.Declaration.CountryPayload.USDeclaration.Ports.Discharge = "ANOTHERTESTPORT";
			value.Shipment.Declaration.CountryPayload.USDeclaration.Ports.Loading = "SECONDTESTPORT";
			value.Shipment.Declaration.CountryPayload.USDeclaration.Ports.PortOfEntry = "PORTOFENTRY";
			value.Shipment.Declaration.CountryPayload.USDeclaration.Ports.PreparerDistrictPort = "DISTRICTPORT";
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			USAdapter.ImportFromValueObject(declaration, value, USContext);
			AssertEquals("SECON", declaration.US_SchDLoading);
			AssertEquals("ANOTH", declaration.US_SchDArrival);
			AssertEquals("PORTO", declaration.US_SchDEntry);
			AssertEquals("DIST", declaration.US_PreparerDistrictPort);
			AssertEquals("SOME", declaration.US_SchDExam);
		}

		public override void TestGetNewInvoicesGenerator()
		{
			JobDeclaration jobDec = Factory.New<JobDeclaration>();
			USDeclarationValueObjectDataAdapterForTest adapter = new USDeclarationValueObjectDataAdapterForTest();
			AssertEquals("Invoice Generator", InvoicesGeneratorType, adapter.GetNewInvoicesGenerator(jobDec).GetType());
		}

		public new void TestGetNewInvoiceAdapter()
		{
			USDeclarationValueObjectDataAdapterForTest adapter = new USDeclarationValueObjectDataAdapterForTest();
			AssertEquals(typeof(USInvoiceDataAdapter), adapter.GetNewInvoiceAdapter(Factory.New<JobDeclaration>()).GetType());
		}

		public void TestExportUSDeclarationPayloadData()
		{
			var decl = (JobDeclaration)GetNewPopulatedJobDeclaration();
			decl.PrimaryHouseBill.ITAndSplitDetails.AddNew().US_ITNumber = "V12345678";
			decl.PrimaryHouseBill.ITAndSplitDetails.AddNew().US_ITNumber = "987654321";
			decl.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var value = MakeExport(decl);
			AssertEquals(ZString.Empty, value.Shipment.ShipmentDetails.DeclarationStyle);
			var xmlDeclaration = value.Shipment.Declaration.CountryPayload.USDeclaration;
			AssertEquals("124", xmlDeclaration.Bond.AccountNumber);
			AssertEquals("897", xmlDeclaration.Bond.ADDCVDSuretyCode);
			AssertEquals(153m, xmlDeclaration.Bond.Amount);
			AssertEquals("891", xmlDeclaration.Bond.SuretyCode);
			AssertEquals("8", BondTypeToXmlCodeMappings.Instance.GetEnterpriseCode(xmlDeclaration.Bond.Type.ToString(), "", USContext));
			AssertEquals("AAAY", xmlDeclaration.CarrierSCAC);
			AssertEquals("A001", xmlDeclaration.CentralizedExamSite);
			AssertEquals("P", ConsolidatedInformalToXmlCodeMappings.Instance.GetEnterpriseCode(xmlDeclaration.ConsolidatedInformal.ToString(), "", USContext));
			AssertEquals("Comes from US_EntryDate", new ZDateTime(2011, 02, 21), xmlDeclaration.DateOfArrival);
			AssertEquals("A", EntryDateElectionCodeToXmlCodeMapping.Instance.GetEnterpriseCode(xmlDeclaration.EntryDateElectionCode.ToString(), "", USContext));
			AssertEquals("01", xmlDeclaration.EntryType.EntryType);
			AssertEquals(Xsd.USDeclarationEntryTypeMode.PAI, xmlDeclaration.EntryType.Mode);
			AssertEquals(Xsd.TrueFalse.@true, xmlDeclaration.EntryType.CertifyCargoRelease);
			AssertEquals(Xsd.TrueFalse.@true, xmlDeclaration.EntryType.EnableCargoRelease);
			AssertEquals(Xsd.TrueFalse.@true, xmlDeclaration.EntryType.EnableEntrySummary);
			AssertEquals(Xsd.TrueFalse.@true, xmlDeclaration.EntryType.EnableElectronicInvoice);
			AssertEquals(Xsd.TrueFalse.@true, xmlDeclaration.EntryType.EnableInBond);
			AssertEquals("Comes from US_EstimatedEntryDate", new ZDateTime(2011, 02, 25), xmlDeclaration.EstimatedEntryDate);
			AssertEquals("XJ5", xmlDeclaration.EntryFilerCode);
			AssertEquals("555", xmlDeclaration.GeneralOrderNumber);
			AssertEquals(Xsd.TrueFalse.@true, xmlDeclaration.HMFApplicable);
			AssertEquals(Xsd.TrueFalse.@true, xmlDeclaration.LiveEntry);
			AssertEquals("10ZZ", xmlDeclaration.LocationOfGoods);
			AssertEquals("XJ6", xmlDeclaration.Warehouse.EntryFilerCode);
			AssertEquals("0001", xmlDeclaration.Warehouse.EntryNumber);
			AssertEquals(true, xmlDeclaration.Warehouse.FinalWHS);
			AssertEquals("9999", xmlDeclaration.Warehouse.Port);
			AssertEquals("10", xmlDeclaration.MissingDocument2);
			AssertEquals("N", OGALineReleaseIndicatorToXmlCodeMappings.Instance.GetEnterpriseCode(xmlDeclaration.OGALineReleaseIndicator.ToString(), "", USContext));
			AssertEquals("12", xmlDeclaration.Payment.ClientBranchDesignation);
			AssertEquals("CHECK", xmlDeclaration.Payment.CheckNo);
			AssertEquals("01", PreliminaryStatementMonthToXmlCodeMappings.Instance.GetEnterpriseCode(xmlDeclaration.Payment.PreliminaryStatementMonth.ToString(), "", USContext));
			AssertEquals(new ZDateTime(2004, 1, 2), xmlDeclaration.Payment.PreliminaryStatementPrintDate);
			AssertEquals(Xsd.USImporterOfRecordDetailsPaymentType.Item2, xmlDeclaration.ImporterOfRecordDetails.PaymentType);
			AssertEquals("1111", xmlDeclaration.Ports.DesignatedExamPort);
			AssertEquals("3901", xmlDeclaration.Ports.Discharge);
			AssertEquals("60237", xmlDeclaration.Ports.Loading);
			AssertEquals("2704", xmlDeclaration.Ports.PortOfEntry);
			AssertEquals("8888", xmlDeclaration.Ports.PreparerDistrictPort);
			AssertEquals("94", xmlDeclaration.PreparerOfficeCode);
			AssertEquals(new ZDateTime(2004, 1, 2), xmlDeclaration.PresentationDate);
			AssertEquals(Xsd.USReconciliationIssue.Item98, xmlDeclaration.Recon.Issue);
			AssertEquals(Xsd.TrueFalse.@false, xmlDeclaration.Recon.NAFTA);
			AssertEquals(Xsd.USImporterOfRecordDetailsTaxDeferredInd.Item0, xmlDeclaration.ImporterOfRecordDetails.TaxDeferredInd);
			AssertEquals("123", xmlDeclaration.TeamNo);
			AssertEquals("Test Org", xmlDeclaration.Organisations.Buyer.OrganisationDetails.Name);
			AssertEquals("Test Org", xmlDeclaration.Organisations.BuyingAgent.OrganisationDetails.Name);
			AssertEquals("Test Org", xmlDeclaration.Organisations.CBPBroker.OrganisationDetails.Name);
			AssertEquals("Test Org", xmlDeclaration.Organisations.Exporter.OrganisationDetails.Name);
			AssertEquals("Test Org", xmlDeclaration.Organisations.Seller.OrganisationDetails.Name);
			AssertEquals("Test Org", xmlDeclaration.Organisations.SellingAgent.OrganisationDetails.Name);
			AssertEquals("Test Org", xmlDeclaration.Organisations.Invoicer.OrganisationDetails.Name);
			AssertEquals("Test Org", ((Xsd.Organisation)xmlDeclaration.Organisations.UltimateConsignee.Item).OrganisationDetails.Name);
			AssertEquals("Test Org", ((Xsd.Organisation)xmlDeclaration.Organisations.NotifyParty.Item).OrganisationDetails.Name);
			AssertEquals(4, xmlDeclaration.BillsOfLading.Count);
			AssertEquals("Y", xmlDeclaration.BillsOfLading[0].AMSCarrier);
			AssertEquals("", xmlDeclaration.BillsOfLading[0].ITNumber);
			AssertEquals("AAAY", xmlDeclaration.BillsOfLading[0].IssuerSCAC);
			AssertEquals("BL", xmlDeclaration.BillsOfLading[0].ManifestQty.DimensionType);
			AssertEquals(100m, xmlDeclaration.BillsOfLading[0].ManifestQty.Value);
			AssertEquals("ITNUM", xmlDeclaration.BillsOfLading[1].ITNumber);
			AssertEquals("V12345678", xmlDeclaration.BillsOfLading[2].ITNumber);
			AssertEquals("987654321", xmlDeclaration.BillsOfLading[3].ITNumber);
			AssertEquals("N", xmlDeclaration.BillsOfLading[1].AMSCarrier);
			AssertEquals("ER", xmlDeclaration.BillsOfLading[1].IssuerSCAC);
			// because DefaultNumberOfPacksToManifestQtyIfRequired() from Declaration run
			AssertEquals("PKG", xmlDeclaration.BillsOfLading[1].ManifestQty.DimensionType);
			AssertEquals(101m, xmlDeclaration.BillsOfLading[1].ManifestQty.Value);
			AssertEquals(new ZDateTime(2008, 1, 2), xmlDeclaration.PriorNotice.ActualTimeOfArrival);
			AssertEquals("4578", xmlDeclaration.PriorNotice.PortOfCrossing);
			AssertEquals("Test Org", xmlDeclaration.PriorNotice.Submitter.OrganisationDetails.Name);
			AssertEquals("MURIEL", xmlDeclaration.PriorNotice.ContactName);
			AssertEquals("7289500362", xmlDeclaration.PriorNotice.ContactPhoneNo);
			AssertEquals("HT", ((Xsd.USPriorNoticePrivatelyOwnFNVehicleType)xmlDeclaration.PriorNotice.Carrier.Item).Country);
			AssertEquals("test Province", ((Xsd.USPriorNoticePrivatelyOwnFNVehicleType)xmlDeclaration.PriorNotice.Carrier.Item).Province);
		}

		public void TestExportUSExportDeclarationSpecificData()
		{
			var decl = Factory.New<JobDeclaration>();
			decl.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			decl.US_SchDExport = "3002";
			decl.US_DateOfExport = ZDateTime.Today.AddDays(-1);
			decl.US_TransportReference = "TRansport";
			decl.US_RN_NKCountryOfDestination = "IT";
			decl.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._4Postdeparture;
			decl.US_SoldEnRouteIndicator = "Y";
			decl.US_RN_NKFirstPortOfCallCountry = "IT";
			decl.US_FirstPortOfCallCity = "MILANO";
			decl.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.UnitedStates;
			var value = MakeExport(decl);
			var xmlDeclaration = value.Shipment.Declaration.CountryPayload.USDeclaration;
			AssertEquals("3002", xmlDeclaration.Ports.Export);
			AssertEquals(ZDateTime.Today.AddDays(-1).Date, xmlDeclaration.ExportDate);
			AssertEquals("TRansport", xmlDeclaration.TransportReference);
			AssertEquals("IT", xmlDeclaration.CountryOfUltimateDestination);
			AssertEquals(Xsd.USDeclarationFilingOption.Item4, xmlDeclaration.FilingOption);
			AssertEquals("Y", xmlDeclaration.SoldEnRoute);
			AssertEquals("IT", xmlDeclaration.FirstCountry);
			AssertEquals("MILANO", xmlDeclaration.FirstCity);
			AssertEquals(decl.US_UC_NKCountryOfExport, xmlDeclaration.CountryOfExport);
		}

		public void TestExportStatusDetails_()
		{
			JobDeclaration decl = GetNewPopulatedJobDeclarationWithStatus();
			Xsd.ConsolAndShipment value = MakeExport(decl);
			Xsd.USDeclaration xmlDeclaration = value.Shipment.Declaration.CountryPayload.USDeclaration;
			AssertEquals(new ZDateTime(2008, 09, 11), xmlDeclaration.Status.EntryDate);
			AssertEquals(new ZDateTime(2010, 07, 11), xmlDeclaration.Status.LiquidationDate);
			AssertEquals(CargoReleaseProcessingResultList.Codes.CondReleaseGenExam, xmlDeclaration.Status.Dispositions[0].ActionCode);
			AssertEquals(new ZDateTime(2009, 07, 14), xmlDeclaration.Status.Dispositions[0].DateTime);
			AssertEquals(new ZDateTime(2009, 07, 14), xmlDeclaration.Status.Dispositions[0].ReleaseDateTime);
			AssertEquals(ReleaseOriginCodeList.Codes.OtherAgencyReviewCompleted, xmlDeclaration.Status.Dispositions[0].ReleaseOrigin);
			AssertEquals("33", xmlDeclaration.Status.OGADispositions[0].BeginCBPLine);
			AssertEquals("44", xmlDeclaration.Status.OGADispositions[0].BeginOGALine);
			AssertEquals("55", xmlDeclaration.Status.OGADispositions[0].EndCBPLine);
			AssertEquals("66", xmlDeclaration.Status.OGADispositions[0].EndOGALine);
			AssertEquals(new ZDateTime(2009, 07, 14), xmlDeclaration.Status.OGADispositions[0].DateTime);
			AssertEquals("06", xmlDeclaration.Status.OGADispositions[0].EntryLevelCode);
			AssertEquals(FDALineLevelDispositionCodeList.Codes._02, xmlDeclaration.Status.OGADispositions[0].LineLevelCode);
			AssertEquals("Status Message", xmlDeclaration.Status.OGADispositions[0].Message);
			AssertEquals("FDA", xmlDeclaration.Status.OGADispositions[0].Qualifier);
			AssertEquals("1", xmlDeclaration.Status.OGADispositions[0].RangeIndicator);
			AssertEquals(new ZDateTime(2010, 07, 11), xmlDeclaration.Status.LiquidationDate);
			AssertEquals("5", xmlDeclaration.Status.LiquidationType);
		}

		public void TestExportEntryMode()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = "SEA";
			var value = MakeExport(declaration);
			var xmlDeclaration = value.Shipment.Declaration.CountryPayload.USDeclaration;
			AssertEquals(Xsd.TrueFalse.@true, xmlDeclaration.EntryType.EnableCargoRelease);
			AssertEquals(Xsd.TrueFalse.@true, xmlDeclaration.EntryType.EnableEntrySummary);
			AssertEquals(Xsd.TrueFalse.@true, xmlDeclaration.EntryType.CertifyCargoRelease);
			AssertEquals(false, xmlDeclaration.EntryType.ModeSpecified);
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			value = MakeExport(declaration);
			xmlDeclaration = value.Shipment.Declaration.CountryPayload.USDeclaration;
			AssertEquals(Xsd.TrueFalse.@false, xmlDeclaration.EntryType.EnableCargoRelease);
			AssertEquals(Xsd.TrueFalse.@true, xmlDeclaration.EntryType.EnableEntrySummary);
			AssertEquals(Xsd.TrueFalse.@true, xmlDeclaration.EntryType.CertifyCargoRelease);
			AssertEquals(Xsd.USDeclarationEntryTypeMode.RLF, xmlDeclaration.EntryType.Mode);
			AssertEquals(true, xmlDeclaration.EntryType.ModeSpecified);
		}

		public void TestImportPaymentType1AndCheckNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.US.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			//no entry type is specified
			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			declaration.US_CheckNo = "879-894-3789";
			var value = MakeExport(declaration);
			var testDeclaration = Factory.New<JobDeclaration>();
			USAdapter.ImportFromValueObject(testDeclaration, value, USContext);
			AssertEquals("Payment Type is updated", PaymentTypeList.Codes.IndividualBasis, testDeclaration.US_PaymentType);
			AssertEquals("CheckNo is updated", "879-894-3789", testDeclaration.US_CheckNo);
		}

		public void TestImportUSDeclarationPayloadData()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			JobDeclaration decl = (JobDeclaration)GetNewPopulatedJobDeclaration();
			decl.PrimaryHouseBill.ITAndSplitDetails.AddNew().US_ITNumber = "V12345678";
			decl.PrimaryHouseBill.ITAndSplitDetails.AddNew().US_ITNumber = "987654321";
			Xsd.ConsolAndShipment value = MakeExport(decl);
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			USAdapter.ImportFromValueObject(declaration, value, USContext);
			AssertDeclarationPayloadData(declaration);
			declaration.JE_DeclarationReference = "B32340334";
			var newValue = new Xsd.ConsolAndShipment();
			newValue.IsSpecified = true;
			newValue.Consol.IsSpecified = true;
			newValue.Consol.ConsolDetail.IsSpecified = true;
			newValue.Consol.ConsolDetail.AgentReference = "B32340334";
			Xsd.ShipmentIdentifier shipmentIdentifier = newValue.Consol.Shipments.AddNew().ShipmentIdentifier.AddNew();
			shipmentIdentifier.IsSpecified = true;
			shipmentIdentifier.Value = declaration.JE_HouseBill;
			shipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			shipmentIdentifier.Masterbill = declaration.JE_MasterBill;
			USAdapter.ImportFromValueObject(declaration, newValue, USContext);
			AssertDeclarationPayloadData(declaration); // declaration data should no be blank out if it's not specified in XML.
		}

		public void TestImportUSExportDeclarationSpecificData()
		{
			var decl = Factory.New<JobDeclaration>();
			decl.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			decl.JE_DeclarationReference = "B32340334";
			decl.JE_MasterBill = "MB9008977";
			var newValue = new Xsd.ConsolAndShipment();
			newValue.IsSpecified = true;
			newValue.Consol.IsSpecified = true;
			newValue.Consol.ConsolDetail.IsSpecified = true;
			newValue.Consol.ConsolDetail.AgentReference = "B32340334";
			var shipmentIdentifier = newValue.Consol.Shipments.AddNew().ShipmentIdentifier.AddNew();
			shipmentIdentifier.IsSpecified = true;
			shipmentIdentifier.Value = decl.JE_HouseBill;
			shipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			shipmentIdentifier.Masterbill = decl.JE_MasterBill;
			newValue.Shipment.Declaration.CountryPayload.USDeclaration.Ports.Export = "3901";
			newValue.Shipment.Declaration.CountryPayload.USDeclaration.ExportDate = ZDateTime.Today.AddDays(-1).Date;
			newValue.Shipment.Declaration.CountryPayload.USDeclaration.TransportReference = "TRansport";
			newValue.Shipment.Declaration.CountryPayload.USDeclaration.CountryOfUltimateDestination = "IT";
			newValue.Shipment.Declaration.CountryPayload.USDeclaration.FilingOptionSpecified = false;
			newValue.Shipment.Declaration.CountryPayload.USDeclaration.SoldEnRoute = "N";
			newValue.Shipment.Declaration.CountryPayload.USDeclaration.FirstCountry = "IT";
			newValue.Shipment.Declaration.CountryPayload.USDeclaration.FirstCity = "MILANO";
			newValue.Shipment.Declaration.CountryPayload.USDeclaration.CountryOfExport = Core.Constants.CountryCodes.UnitedStates;
			USAdapter.ImportFromValueObject(decl, newValue, USContext);
			AssertEquals("3901", decl.US_SchDExport);
			AssertEquals(ZDateTime.Today.AddDays(-1), decl.US_DateOfExport);
			AssertEquals("TRansport", decl.US_TransportReference);
			AssertEquals("IT", decl.US_RN_NKCountryOfDestination);
			AssertEquals("No value is specified. FilingOption is set from the default value", AESCommodityFilingOptionList.Codes._2Predeparture, decl.US_CommodityFilingOption);
			AssertEquals("N", decl.US_SoldEnRouteIndicator);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, decl.US_UC_NKCountryOfExport);
			newValue.Shipment.Declaration.CountryPayload.USDeclaration.FilingOption = Xsd.USDeclarationFilingOption.Item4;
			USAdapter.ImportFromValueObject(decl, newValue, USContext);
			AssertEquals(AESCommodityFilingOptionList.Codes._4Postdeparture, decl.US_CommodityFilingOption);
		}

		public new void TestImportMergeBy()
		{
			Assert(true);
		}

		public new void TestExportConsolValues_Carrier()
		{
			Assert(true);
		}

		public new void TestExportLandedCostingHeadings()
		{
			Assert(true);
		}

		public override void TestExportCustomsEntry()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			JobDeclaration declaration = NewPopulatedJobDeclaration();
			MergeAndSetupCustomsCharges(declaration);
			AssertEquals("Precondition: EntryHeaders.Count", 3, declaration.CustomsEntryHeaders.Count);
			Customs.Business.CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("Precondition: MergedLines.Count", 2, entryHeader.MergedLines.Count);
			entryHeader.EntryNumber = "12345678";
			declaration.JE_EntrySubmittedDate = new ZDateTime(2006, 1, 1);
			var adapter = new USDeclarationValueObjectDataAdapterForTest();
			Xsd.Consols consolXsd = adapter.ExportConsolsValueObject(declaration, new ValueObjectExportContext(new NotificationBuffer()));
			Xsd.CustomsEntryCollection entryHeaderXsds = consolXsd.Consol[0].Shipments[0].ShipmentDetails.CustomsEntries;
			AssertEquals("entryHeaderXsds.Count", 3, entryHeaderXsds.Count);
			Xsd.CustomsEntry entryHeaderXsd = entryHeaderXsds[0];
			Xsd.MonetaryAmount customsValueXsd = entryHeaderXsd.CustomsValue;
			AssertEquals("entryHeaderXsd.EntryDate", new ZDateTime(2006, 1, 1), entryHeaderXsd.EntryDate);
			AssertEquals("entryHeaderXsd.ExchangeRate", 1m, entryHeaderXsd.ExchangeRate);
			AssertEquals("entryHeaderXsd.TransportAndInsurance", 0m, entryHeaderXsd.TransportAndInsurance.Value);
			Xsd.CustomsEntryNumber entryNumberXsd = entryHeaderXsd.CustomsEntryNumber;
			AssertNotNull("entryNumberXsd (entryHeaderXsd.CustomsEntryNumber)", entryNumberXsd);
			AssertEquals("entryNumberXsd.Number", "12345678", entryNumberXsd.Number);
			AssertEquals("entryNumberXsd.Country", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, entryNumberXsd.Country);
			AssertNotEquals("entryNumberXsd.Type", ZString.Empty, entryNumberXsd.Type);
			Xsd.CustomsEntryLineCollection entryLineXsds = entryHeaderXsd.CustomsEntryLines;
			AssertEquals("entryLineXsds.Count", 2, entryLineXsds.Count);
		}

		protected override string TestingCountry => Core.Constants.CountryCodes.UnitedStates;

		protected override Type InvoicesGeneratorType => typeof(USInvoicesGeneratorFromXSD);

		protected override ValueObjectDataAdapter<BaseJobDeclaration, Xsd.ConsolAndShipment> GetNewBizObjXmlDataAdapter() => new USDeclarationValueObjectDataAdapter();

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var declaration = (JobDeclaration)GetEmptyJobDeclaration();
			declaration.US_EntryType = "";
			declaration.US_IsHMFApplicable = "Y";
			declaration.JE_TotalNoOfPacksPackType = "PK";
			return new BusinessObjectAndExpectedOutputFileName(declaration, TestFileHelper.GetPathForResourceName("USEmptyDeclaration.xml", Assembly.GetExecutingAssembly()), ValidationKind.None, "Empty Declaration");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample() => GetEmptyBizObjSample();

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample() => new BusinessObjectAndExpectedOutputFileName(GetDeclaration(), TestFileHelper.GetPathForResourceName("USPopulatedDeclaration.xml", Assembly.GetExecutingAssembly()), ValidationKind.Xsd | ValidationKind.FactorySave, "Fully Populated Declaration");

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects() => Array.Empty<BusinessObjectAndExpectedOutputFileName>();

		protected override BaseJobDeclaration NewBusinessObject()
		{
			var result = Factory.New<JobDeclaration>();
			result.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			result.US_EnableENS = false;
			result.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			result.US_TaxDeferIndicator = "";
			result.DisableDefaultPackingInformation = true;
			return result;
		}

		protected override BaseJobDeclaration GetNewPopulatedJobDeclaration() => NewPopulatedJobDeclaration();

		JobDeclaration GetNewPopulatedJobDeclarationWithStatus()
		{
			JobDeclaration declaration = (JobDeclaration)GetNewPopulatedJobDeclaration();
			declaration.US_PresentationDate = new ZDateTime(2008, 09, 11);
			CusLiquidation liquidation = Factory.New<CusLiquidation>();
			liquidation.B8_LiquidationType = "5";
			liquidation.B8_LiquidationDate = new ZDateTime(2010, 07, 11);
			liquidation.B8_SystemCreateDate = new ZDateTime(2010, 07, 11);
			declaration.Liquidations.Add(liquidation);
			DispositionData disposition = declaration.DispositionCodes.AddNew();
			disposition.US_Code = CargoReleaseProcessingResultList.Codes.CondReleaseGenExam;
			disposition.US_DispositionDate = new ZDateTime(2009, 07, 14);
			disposition.US_Order = (ZShort)1;
			disposition.US_ReleaseDate = new ZDateTime(2009, 07, 14);
			disposition.US_ReleaseOrigin = ReleaseOriginCodeList.Codes.OtherAgencyReviewCompleted;
			OGADispositionData ogaDisposition = declaration.OGADispositionCodes.AddNew();
			ogaDisposition.US_Code = FDALineLevelDispositionCodeList.Codes._02;
			ogaDisposition.US_DispositionDate = new ZDateTime(2009, 07, 14);
			ogaDisposition.US_OGADispositionBeginningCBPLine = "33";
			ogaDisposition.US_OGADispositionBeginningOGALine = "44";
			ogaDisposition.US_OGADispositionEndCBPLine = "55";
			ogaDisposition.US_OGADispositionEndOGALine = "66";
			ogaDisposition.US_OGADispositionRangeIndicator = "1";
			ogaDisposition.US_OGADispositionStatusCode = "06";
			ogaDisposition.US_OGADispositionStatusMessage = "Status Message";
			ogaDisposition.US_OGAIdentifier = "FDA";
			ogaDisposition.US_Order = (ZShort)1;
			return declaration;
		}

		JobDeclaration NewPopulatedJobDeclaration()
		{
			JobDeclaration declaration = GetDeclaration();
			OrgHeader organization = Factory.New<OrgHeader>();
			organization.FillWithValidTestData();
			organization.OH_FullName = "Test Org";
			OrgCountryData countryData = Factory.New<OrgCountryData>();
			countryData.OV_OH_OrgHeader = organization.PK;
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedStates;
			OrgImpAddInfo addInfo = new OrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
			addInfo.ZO_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			Factory.Save();
			declaration.JE_OH_Buyer = organization.PK;
			declaration.JE_OH_BuyingAgent = organization.PK;
			declaration.JE_OH_CBPBroker = organization.PK;
			declaration.JE_OH_Exporter = organization.PK;
			declaration.JE_OA_SellerAddress = organization.MainAddress.PK;
			declaration.JE_OH_SellingAgent = organization.PK;
			declaration.JE_OA_ConsigneeAddress = organization.MainAddress.PK;
			declaration.JE_OH_NotifyParty = organization.PK;
			var address = organization.Addresses.AddNew();
			address.OA_Address1 = "Test Address 1";
			address2 = organization.Addresses.AddNew();
			address2.OA_Address1 = "Test Address 2";
			declaration.JE_OA_InvoicerAddress = address2.PK;
			declaration.JE_OA_ManufacturerAddress = address2.PK;
			declaration.JE_OH_FDASubmitter = organization.PK;
			return declaration;
		}
		OrgAddress address2;

		JobDeclaration GetDeclaration()
		{
			var firmsHelper = new UniversalReferenceTestDataHelper(Factory);
			firmsHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			firmsHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "10ZZ", "FIRM NAME", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();
			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "Lloyds1";
			vessel.RV_Code = "NEWVES";
			var declaration = Factory.New<JobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_MessageSubType = "ZZ";
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = false;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_MasterBill = "MASTERBILL";
			declaration.JE_TransportMode = "SEA";
			declaration.JE_VoyageFlightNo = "VoyageFlig";
			declaration.JE_ExportDate = new ZDateTime(2011, 02, 20);
			declaration.US_EntryDate = new ZDateTime(2011, 02, 21);
			declaration.JE_DateOfArrival = new ZDateTime(2011, 02, 22);
			declaration.JE_DateAtOrigin = new ZDateTime(2011, 02, 23);
			declaration.JE_RS_NKServiceLevel = "Ser";
			declaration.JE_HouseBill = "HOUSEBILL";
			declaration.US_SchDLoading = "60237";
			declaration.US_SchDArrival = "3901";
			declaration.JE_RL_NKPortOfArrival = "USCHI";
			declaration.US_SchDEntry = "2704";
			declaration.JE_RL_NKFinalDestination = "USLAX";
			declaration.JE_OwnerRef = "OwnerRef";
			declaration.PrimaryMasterBill.US_AMSCarrierIndicator = "Y";
			declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = "AAAY";
			declaration.PrimaryMasterBill.CU_NoOfPacks = 100m;
			declaration.PrimaryMasterBill.CU_PackType = "BL";
			declaration.PrimaryHouseBill.US_AMSCarrierIndicator = "N";
			declaration.PrimaryHouseBill.US_UI_NKBillIssuerSCAC = "ER";
			declaration.PrimaryHouseBill.CU_NoOfPacks = 32m;
			declaration.PrimaryHouseBill.CU_PackType = "KG";
			declaration.PrimaryHouseBill.ITNumber = "ITNUM";
			declaration.DisableDefaultPackingInformation = false;
			declaration.JE_TotalNoOfPacks = 101;
			declaration.JE_TotalNoOfPacksPackType = "PKG";
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_TotalNoOfPieces = 58;
			declaration.JE_TotalVolume = 229;
			declaration.JE_TotalVolumeUnit = "To";
			declaration.JE_TotalWeight = 240;
			declaration.JE_TotalWeightUnit = "To";
			declaration.JE_GoodsDescription = "GoodsDescription";
			declaration.JE_ShipmentIncoTerm = "Shi";
			declaration.JE_VesselName = "ADMIRALENGRACHT";
			declaration.US_BondType = "8";
			declaration.US_BondProducerAccNo = "124";
			declaration.US_ADDCVDSuretyCode = "897";
			declaration.US_BondAmount = 153m;
			declaration.US_SuretyCode = "891";
			declaration.US_UI_NKCarrierSCAC = "AAAY";
			declaration.US_ConsolidatedInformalIndicator = "P";
			declaration.US_EntryDateElectionCode = "A";
			declaration.US_EntryType = "01";
			declaration.US_EntryMode = EntryModeList.Codes.Paired;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EnableAII = true;
			declaration.US_EnableINB = true;
			declaration.US_EstimatedEntryDate = new ZDateTime(2011, 02, 25);
			declaration.US_GeneralOrderNo = "555";
			declaration.US_LiveEntryIndicator = "Y";
			declaration.US_7501Purchased = "Y";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_FDAContactName = "MURIEL";
			declaration.US_FDAContactPhoneNo = "7289500362";
			declaration.US_US_NKLocationOfGoods = "10ZZ";
			declaration.US_WHSDistrictPortCode = "9999";
			declaration.US_WHSEntryFilerCode = "XJ6";
			declaration.US_WHSEntryNumber = "0001";
			declaration.US_IsFinalWHS = true;
			declaration.US_MissingDocument1 = "16";
			declaration.US_MissingDocument2 = "10";
			declaration.US_OGALineReleaseIndicator = "N";
			declaration.US_PresentationDate = new ZDateTime(2004, 1, 2);
			declaration.US_ITDate = new ZDateTime(2009, 1, 2);
			declaration.US_PaymentType = "2";
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2004, 1, 2);
			declaration.US_PeriodicStatementMM = "1";
			declaration.US_ClientBranchDesignation = "12";
			declaration.US_CheckNo = "CHECK";
			declaration.US_SchDExam = "1111";
			declaration.US_PreparerDistrictPort = "8888";
			declaration.US_PreparerOfficeCode = "94";
			declaration.US_OtherReconIndicator = "98";
			declaration.US_NAFTAReconIndicator = false;
			declaration.US_FixRecon = true;
			declaration.US_TaxDeferIndicator = "0";
			declaration.US_TeamNo = "123";
			declaration.US_US_NKCentralizedExamSite = "A001";
			declaration.US_FDAADTA = new ZDateTime(2008, 1, 2);
			declaration.US_FDAAPC = "4578";
			declaration.US_FDACANType = FDACarrierTypeList.Codes.PrivatelyOwnedFNVehicle;
			declaration.US_FDACCN = "HT";
			declaration.US_FDACAN = "test Province";
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "NLU12345678";
			container.CO_Seal = "SEAL";
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			container.CO_Weight = 12.33m;
			var marksAndNumbersNote = declaration.Notes.AddNew();
			marksAndNumbersNote.ST_ParentID = declaration.PK;
			marksAndNumbersNote.ST_Table = declaration.TableName;
			marksAndNumbersNote.ST_Description = "Marks & Numbers";
			marksAndNumbersNote.ST_NoteText = "MarksAndNumbersShort";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INVOICENUMBER";
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2005, 3, 14);
			invoiceHeader.JZ_InvoiceAmount = 57;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(1998, 3, 4);
			invoiceHeader.JZ_Volume = 159;
			invoiceHeader.JZ_VolumeUQ = "Vo";
			invoiceHeader.JZ_Weight = 168;
			invoiceHeader.JZ_WeightUQ = "We";
			invoiceHeader.JZ_IncoTerm = "Inc";
			invoiceHeader.US_TermsOfDeliveryLocation = "60237";
			invoiceHeader.JZ_CU_RelatedHouseBill = declaration.PrimaryHouseBill.PK;
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Description = "CATS HEADS";
			invoiceLine1.JI_ExtraInfoForClassification = "EXTENDED CATS HEADS";
			invoiceLine1.JI_Tariff = "0101";
			invoiceLine1.JI_LinePrice = 25.00m;
			invoiceLine1.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("NLU12345678").IsForInvoiceLine = true;
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Description = "RED CRAYONS";
			invoiceLine2.JI_ExtraInfoForClassification = "EXTENDED RED CRAYONS";
			invoiceLine2.JI_Tariff = "9090";
			invoiceLine2.JI_LinePrice = 32.00m;
			invoiceLine2.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("NLU12345678").IsForInvoiceLine = true;
			declaration.JobComInvoiceGroupHeaders[0].JZ_InvoiceDate = new ZDateTime(2005, 3, 14);
			declaration.Invoices[0].JZ_JZ_GroupInvoiceFK = declaration.JobComInvoiceGroupHeaders[0].PK;
			declaration.DocsAndCartage.JP_LCLAvailable = new ZDateTime(2005, 4, 25);
			declaration.DocsAndCartage.JP_FCLAvailable = new ZDateTime(2005, 4, 25);
			declaration.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2005, 2, 1);
			declaration.DocsAndCartage.JP_DeliveryRequiredBy = new ZDateTime(2005, 3, 1);
			declaration.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2005, 4, 1);
			declaration.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2005, 5, 1);
			declaration.ImporterDeliveryAddress.E2_AddressOverride = true;
			declaration.ImporterDeliveryAddress.E2_Address1 = "ADDRESS1";
			declaration.ImporterDeliveryAddress.E2_Address2 = "ADDRESS2";
			declaration.ImporterDeliveryAddress.E2_City = "CITY";
			declaration.ImporterDeliveryAddress.E2_State = "CA";
			declaration.ImporterDeliveryAddress.E2_Postcode = "PCODE";
			declaration.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2005, 2, 1);
			declaration.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2005, 3, 1);
			declaration.DocsAndCartage.JP_PickupCartageAdvised = new ZDateTime(2005, 4, 1);
			declaration.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2005, 5, 1);
			declaration.SupplierPickupAddress.E2_AddressOverride = true;
			declaration.SupplierPickupAddress.E2_Address1 = "ADDRESS1";
			declaration.SupplierPickupAddress.E2_Address2 = "ADDRESS2";
			declaration.SupplierPickupAddress.E2_City = "CITY";
			declaration.SupplierPickupAddress.E2_State = "CA";
			declaration.SupplierPickupAddress.E2_Postcode = "PCODE";
			declaration.DocsAndCartage.JP_CustomAttrib1 = "Cus1";
			declaration.DocsAndCartage.JP_CustomAttrib2 = "Cus2";
			declaration.DocsAndCartage.JP_CustomDate1 = new ZDateTime(2005, 12, 2);
			declaration.DocsAndCartage.JP_CustomDate2 = new ZDateTime(2005, 12, 1);
			declaration.DocsAndCartage.JP_CustomDecimal1 = 2m;
			declaration.DocsAndCartage.JP_CustomDecimal2 = 20m;
			declaration.DocsAndCartage.JP_CustomFlag1 = true;
			declaration.DocsAndCartage.JP_CustomFlag2 = false;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			var packGroup = declaration.PrimaryHouseBill.PackingGroups.Count == 0 ? declaration.PrimaryHouseBill.PackingGroups.AddNew() : declaration.PrimaryHouseBill.PackingGroups[0];
			packGroup.CR_CO_Container = container.PK;
			packGroup.AddTotalOuterPackageIfRequired(101);
			var salesRep = Factory.New<GlbStaff>();
			salesRep.GS_Code = "JA";
			salesRep.GS_FullName = "JESSICA ALLEN";
			salesRep.GS_IsSalesRep = true;
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_GS_NKRepSales = salesRep.GS_Code;
			jobHeader.JH_ParentID = declaration.PK;
			return declaration;
		}

		void AssertDeclarationPayloadData(JobDeclaration declaration)
		{
			AssertEquals("124", declaration.US_BondProducerAccNo);
			AssertEquals("897", declaration.US_ADDCVDSuretyCode);
			AssertEquals(153m, declaration.US_BondAmount);
			AssertEquals("891", declaration.US_SuretyCode);
			AssertEquals("8", declaration.US_BondType);
			AssertEquals("AAAY", declaration.US_UI_NKCarrierSCAC);
			AssertEquals("P", declaration.US_ConsolidatedInformalIndicator);
			AssertEquals("Comes from US_EntryDate", new ZDateTime(2011, 02, 21), declaration.US_EntryDate);
			AssertEquals("A", declaration.US_EntryDateElectionCode);
			AssertEquals("01", declaration.US_EntryType);
			AssertEquals(EntryModeList.Codes.Paired, declaration.US_EntryMode);
			AssertEquals(false, declaration.US_CertifyCargoRelease);
			AssertEquals(true, declaration.US_EnableCRL);
			AssertEquals(true, declaration.US_EnableAII);
			AssertEquals(true, declaration.US_EnableINB);
			AssertEquals(true, declaration.US_EnableENS);
			AssertEquals("Comes from US_EstimatedEntryDate", new ZDateTime(2011, 02, 25), declaration.US_EstimatedEntryDate);
			AssertEquals("XJ5", declaration.US_EntryFilerCode);
			AssertEquals("555", declaration.US_GeneralOrderNo);
			AssertEquals("Y", declaration.US_IsHMFApplicable);
			AssertEquals("Y", declaration.US_LiveEntryIndicator);
			AssertEquals("Y", declaration.US_7501Purchased);
			AssertEquals("10ZZ", declaration.US_US_NKLocationOfGoods);
			AssertEquals("9999", declaration.US_WHSDistrictPortCode);
			AssertEquals("XJ6", declaration.US_WHSEntryFilerCode);
			AssertEquals("0001", declaration.US_WHSEntryNumber);
			AssertEquals(true, declaration.US_IsFinalWHS);
			AssertEquals("16", declaration.US_MissingDocument1);
			AssertEquals("10", declaration.US_MissingDocument2);
			AssertEquals("N", declaration.US_OGALineReleaseIndicator);
			AssertEquals("01", declaration.US_PeriodicStatementMM);
			AssertEquals(new ZDateTime(2004, 1, 2), declaration.US_PreliminaryStatementPrintDate);
			AssertEquals("2", declaration.US_PaymentType);
			AssertEquals("12", declaration.US_ClientBranchDesignation);
			AssertEquals("CHECK", declaration.US_CheckNo);
			AssertEquals("1111", declaration.US_SchDExam);
			AssertEquals("3901", declaration.US_SchDArrival);
			AssertEquals("60237", declaration.US_SchDLoading);
			AssertEquals("2704", declaration.US_SchDEntry);
			AssertEquals("8888", declaration.US_PreparerDistrictPort);
			AssertEquals("94", declaration.US_PreparerOfficeCode);
			AssertEquals(new ZDateTime(2004, 1, 2), declaration.US_PresentationDate);
			AssertEquals(new ZDateTime(2009, 1, 2), declaration.US_ITDate);
			AssertEquals("98", declaration.US_OtherReconIndicator);
			AssertEquals(false, declaration.US_NAFTAReconIndicator);
			AssertEquals("0", declaration.US_TaxDeferIndicator);
			AssertEquals("123", declaration.US_TeamNo);
			AssertEquals(address2.PK, declaration.JE_OA_ManufacturerAddress);
			AssertEquals(address2.PK, declaration.JE_OA_InvoicerAddress);
			AssertEquals(ZString.Empty, declaration.JE_MessageSubType);
			AssertEquals("A001", declaration.US_US_NKCentralizedExamSite);
			AssertEquals("Y", declaration.PrimaryMasterBill.US_AMSCarrierIndicator);
			AssertEquals("ITNUM", declaration.PrimaryHouseBill.ITAndSplitDetails[0].US_ITNumber);
			AssertEquals("V12345678", declaration.PrimaryHouseBill.ITAndSplitDetails[1].US_ITNumber);
			AssertEquals("987654321", declaration.PrimaryHouseBill.ITAndSplitDetails[2].US_ITNumber);
			AssertEquals("AAAY", declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCAC);
			AssertEquals(100m, declaration.PrimaryMasterBill.CU_NoOfPacks);
			AssertEquals("BL", declaration.PrimaryMasterBill.CU_PackType);
			AssertEquals("N", declaration.PrimaryHouseBill.US_AMSCarrierIndicator);
			AssertEquals("ER", declaration.PrimaryHouseBill.US_UI_NKBillIssuerSCAC);
			// because DefaultNumberOfPacksToManifestQtyIfRequired() from Declaration run
			AssertEquals(101m, declaration.PrimaryHouseBill.CU_NoOfPacks);
			AssertEquals("PKG", declaration.PrimaryHouseBill.CU_PackType);
			AssertEquals(new ZDateTime(2008, 1, 2), declaration.US_FDAADTA);
			AssertEquals("4578", declaration.US_FDAAPC);
			AssertEquals("Test Org", declaration.FDASubmitter.OH_FullName);
			AssertEquals("MURIEL", declaration.US_FDAContactName);
			AssertEquals("7289500362", declaration.US_FDAContactPhoneNo);
			AssertEquals(FDACarrierTypeList.Codes.PrivatelyOwnedFNVehicle, declaration.US_FDACANType);
			AssertEquals("HT", declaration.US_FDACCN);
			AssertEquals("test Province", declaration.US_FDACAN);
		}

		Xsd.ConsolAndShipment MakeExport(JobDeclaration declaration)
		{
			var value = new Xsd.ConsolAndShipment();
			USAdapter.ExportToValueObject(declaration, value, new ValueObjectExportContext(USNotification));
			return value;
		}

		USDeclarationValueObjectDataAdapter usAdapter;
		USDeclarationValueObjectDataAdapter USAdapter => usAdapter ?? (usAdapter = new USDeclarationValueObjectDataAdapter());

		ValueObjectImportContext usContext;
		ValueObjectImportContext USContext => usContext ?? (usContext = new ValueObjectImportContext(Factory, USNotification));

		NotificationBuffer usNotification;
		NotificationBuffer USNotification => usNotification ?? (usNotification = new NotificationBuffer());

		sealed class USDeclarationValueObjectDataAdapterForTest : USDeclarationValueObjectDataAdapter
		{
			public new InvoiceValueObjectDataAdapter GetNewInvoiceAdapter(BaseJobDeclaration jobDec) => (USInvoiceDataAdapter)base.GetNewInvoiceAdapter(jobDec);

			public new void ImportFromValueObjectCore(BaseJobDeclaration bizObj, Xsd.ConsolAndShipment value, IValueObjectImportContext context) => base.ImportFromValueObjectCore(bizObj, value, context);

			public new InvoicesGeneratorFromXSD GetNewInvoicesGenerator(BaseJobDeclaration jobDec) => base.GetNewInvoicesGenerator(jobDec);
		}
	}
}
