using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOrganisations()
		{
			var declaration = Factory.New<JobDeclaration>();
			var line = declaration.InvoiceLines.AddNew();
			AssertEquals(typeof(OrgHeaderCollection), line.Lookups.Organisations.GetType());
		}

		public void TestInvoiceUQListForExport()
		{
			var refPack = Factory.New<RefPackType>();
			refPack.F3_Code = "XX";
			refPack.F3_Description = "TEST";
			Factory.Save();

			var refPackCollectionList = new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits();
			Assert("PreCondition", refPackCollectionList.ContainsCode("CTN"));

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			Assert(invoiceLine.Lookups.InvoiceUQList.ContainsCode("XX"));
			Assert("PreCondition", invoiceLine.Lookups.InvoiceUQList.ContainsCode("CTN"));

			invoiceLine.JI_InvoiceUQ = "XX";
			Assert(!invoiceLine.JI_InvoiceUQInfo.HasMessageErrors());
			AssertEquals("AESUnitOfMeasureList should take priority when there are duplicate codes between RefPack and AESList", AESUnitOfMeasureList.Descriptions.ContentTons, invoiceLine.Lookups.InvoiceUQList.GetDescriptionFromCode("CTN"));
		}

		public void TestAntidumpingDutyDepositRates()
		{
			USCACCase uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "AXXAAABBB";

			USCACCaseRate rate = uscCase.CaseRates.AddNew();
			rate.U6_EffectiveDate = ZDateTime.Today;
			rate.U6_AdValoremRate = 0.1234m;
			rate.U6_SpecificRate = 0.2345m;
			rate.U6_Unit = "KG";
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_ADDCaseNo = "AXXAAABBB";

			DepositRateIndicatorList list = invoiceLine.Lookups.AntidumpingDutyDepositRates;
			AssertEquals(4, list.Count);
			AssertCodeDescription(list[0], DepositRateIndicatorList.Codes.AdValorem, "12.34%");
			AssertCodeDescription(list[1], DepositRateIndicatorList.Codes.OverrideAdValorem, DepositRateIndicatorList.Descriptions.OverrideAdValorem);
			AssertCodeDescription(list[2], DepositRateIndicatorList.Codes.Specific, "23c/KG");
			AssertCodeDescription(list[3], DepositRateIndicatorList.Codes.OverrideSpecific, DepositRateIndicatorList.Descriptions.OverrideSpecific);
		}

		public void TestInvoiceUQContainsPoundsAndCase()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			Assert(invoiceLine.Lookups.InvoiceUQList.ContainsCode(AESUnitOfMeasureList.Codes.Pounds));
			Assert(invoiceLine.Lookups.InvoiceUQList.ContainsCode(AESUnitOfMeasureList.Codes.Case));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(invoiceLine.Lookups.InvoiceUQList.ContainsCode(ABIUnitOfMeasureList.Codes.Pounds));
			Assert(invoiceLine.Lookups.InvoiceUQList.ContainsCode(ABIUnitOfMeasureList.Codes.Case));
		}

		public void TestAntidumpingDutyDepositRatesForACE()
		{
			var acCase = Factory.New<USCACCase>();
			acCase.U5_CaseNumber = "A1";

			var caseRate = acCase.CaseRates.AddNew();
			caseRate.U6_EffectiveDate = ZDateTime.BrettsBirthday;
			caseRate.U6_AdValoremRate = 0.52m;
			caseRate.U6_SpecificRate = 0.62m;
			caseRate.U6_Unit = "KG";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			declaration.Invoices.AddNew().JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_ADDCaseNo = "A1";

			DepositRateIndicatorList list = invoiceLine.Lookups.AntidumpingDutyDepositRates;
			AssertEquals(4, list.Count);
			AssertCodeDescription(list[0], DepositRateIndicatorList.Codes.AdValorem, "52.00%");
			AssertCodeDescription(list[1], DepositRateIndicatorList.Codes.OverrideAdValorem, DepositRateIndicatorList.Descriptions.OverrideAdValorem);
			AssertCodeDescription(list[2], DepositRateIndicatorList.Codes.Specific, "62c/KG");
			AssertCodeDescription(list[3], DepositRateIndicatorList.Codes.OverrideSpecific, DepositRateIndicatorList.Descriptions.OverrideSpecific);
		}

		public void TestCountervailingDutyDepositRates()
		{
			USCACCase uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "AXXAAABBB";

			USCACCaseRate rate = uscCase.CaseRates.AddNew();
			rate.U6_EffectiveDate = ZDateTime.BrettsBirthday;
			rate.U6_AdValoremRate = 0.52m;
			rate.U6_SpecificRate = 0.62m;
			rate.U6_Unit = "KG";
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_CVDCaseNo = "AXXAAABBB";

			DepositRateIndicatorList list = invoiceLine.Lookups.CountervailingDutyDepositRates;
			AssertEquals(4, list.Count);
			AssertCodeDescription(list[0], DepositRateIndicatorList.Codes.AdValorem, "52.00%");
			AssertCodeDescription(list[1], DepositRateIndicatorList.Codes.OverrideAdValorem, DepositRateIndicatorList.Descriptions.OverrideAdValorem);
			AssertCodeDescription(list[2], DepositRateIndicatorList.Codes.Specific, "62c/KG");
			AssertCodeDescription(list[3], DepositRateIndicatorList.Codes.OverrideSpecific, DepositRateIndicatorList.Descriptions.OverrideSpecific);
		}

		public void TestLimitedReportingExportCodeList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineLookups lookups = invoiceLine.Lookups;
			AssertEquals(typeof(LimitedReportingExportInformationCodeList), lookups.LimitedReportingExportCodeList.GetType());
		}

		public void TestClassificationList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineLookups lookups = invoiceLine.Lookups;
			AssertEquals(typeof(ImportClassificationCollection), lookups.ClassificationList.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(typeof(ExportClassificationCollection), lookups.ClassificationList.GetType());
			declaration.US_TariffType = TariffTypeList.Codes.HTS;
			AssertEquals(typeof(ImportClassificationCollection), lookups.ClassificationList.GetType());
		}

		public void TestInvoiceLine()
		{
			JobComInvoiceLine parent = Factory.New<JobComInvoiceLine>();
			AssertEquals(parent.Lookups.InvoiceLine, parent);
		}

		public void TestTariffs()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineLookups lookups = new JobComInvoiceLineLookups(invoiceLine);
			AssertEquals("Tariffs", typeof(USCTariffCollection), lookups.Tariffs.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Tariffs", typeof(Universal.TariffViewCollection), lookups.Tariffs.GetType());
			AssertEquals("ImportTariffs", typeof(USCTariffCollection), lookups.ImportTariffs.GetType());
			AssertEquals("ExportTariffs", typeof(Universal.TariffViewCollection), lookups.ExportTariffs.GetType());
			declaration.US_TariffType = TariffTypeList.Codes.HTS;
			AssertEquals("Tariffs", typeof(Universal.TariffViewCollection), lookups.Tariffs.GetType());
			AssertEquals("ImportTariffs", typeof(USCTariffCollection), lookups.ImportTariffs.GetType());
			AssertEquals("ExportTariffs", typeof(Universal.TariffViewCollection), lookups.ExportTariffs.GetType());
		}

		public void TestImportTariffs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_FormattedTariff = "9876.54.3210";
			var collection = invoiceLine.Lookups.ImportTariffs;
			AssertType<USCTariffCollection>("ImportTariffs", collection);

			var tariffFilterKey = USCTariff.FilterSchema.Tariff + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property";
			Assert("Tariff filter should be defaulted", collection.FilterBusinessObjectDefaults.ContainsDefaultFor(tariffFilterKey));
			AssertEquals("Default from JI_FormattedTariff", "9876.54.3210", collection.FilterBusinessObjectDefaults[tariffFilterKey].Value.ToString());
		}

		public void TestExportTariffs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var exportDate = ZDateTime.Today.AddYears(-1);
			declaration.US_DateOfExport = exportDate;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.US_ExportTariff = "9876.54.3210";
			invoiceLine.US_TariffType = TariffTypeList.Codes.ScheduleB;
			var collection = invoiceLine.Lookups.ExportTariffs;
			AssertType<Universal.TariffViewCollection>("ExportTariffs is TariffViewCollection", collection);
			var tariffFilterKey = Universal.Constants.RefCusTariffFilters.TariffType + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2";
			Assert("Code filter should be defaulted", collection.FilterBusinessObjectDefaults.ContainsDefaultFor(tariffFilterKey));
			AssertEquals("Default SHB", Universal.Constants.TariffTypes.ScheduleB, collection.FilterBusinessObjectDefaults[tariffFilterKey].Value.ToString());

			invoiceLine.US_TariffType = TariffTypeList.Codes.HTS;
			collection = invoiceLine.Lookups.ExportTariffs;
			AssertType<Universal.TariffViewCollection>("ExportTariffs is TariffViewCollection", collection);
			var tariffTypeFilterKey = Universal.Constants.RefCusTariffFilters.TariffType + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2";
			Assert("TariffType filter should be defaulted", collection.FilterBusinessObjectDefaults.ContainsDefaultFor(tariffTypeFilterKey));
			AssertEquals("Default EXP", Universal.Constants.TariffTypes.Export, collection.FilterBusinessObjectDefaults[tariffTypeFilterKey].Value.ToString());

			var effectiveDateFilterKey = Universal.Constants.RefCusTariffFilters.EffectiveDate + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1";
			Assert("EffectiveDate filter should be defaulted", collection.FilterBusinessObjectDefaults.ContainsDefaultFor(effectiveDateFilterKey));
			AssertEquals("Default Export Date", exportDate, collection.FilterBusinessObjectDefaults[effectiveDateFilterKey].Value);

			tariffFilterKey = Universal.Constants.RefCusTariffFilters.TariffCode + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property";
			Assert("Code filter should be defaulted", collection.FilterBusinessObjectDefaults.ContainsDefaultFor(tariffFilterKey));
			AssertEquals("Default from JI_FormattedTariff", "9876543210", collection.FilterBusinessObjectDefaults[tariffFilterKey].Value.ToString());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoiceLineExportDate = ZDateTime.Today.AddYears(-2);
			invoiceLine.US_DRWExportDate = invoiceLineExportDate;
			collection = invoiceLine.Lookups.ExportTariffs;
			effectiveDateFilterKey = Universal.Constants.RefCusTariffFilters.EffectiveDate + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1";
			Assert("EffectiveDate filter should be defaulted", collection.FilterBusinessObjectDefaults.ContainsDefaultFor(effectiveDateFilterKey));
			AssertEquals("Default Export Date", invoiceLineExportDate, collection.FilterBusinessObjectDefaults[effectiveDateFilterKey].Value);
		}

		public void TestInvoiceUQList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("InvoiceUQList", typeof(ABIUnitOfMeasureList), invoiceLine.Lookups.InvoiceUQList.GetType());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("InvoiceUQList", typeof(CodeDescriptionPairList), invoiceLine.Lookups.InvoiceUQList.GetType());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("InvoiceUQList", typeof(ABIUnitOfMeasureList), invoiceLine.Lookups.InvoiceUQList.GetType());

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertEquals("InvoiceUQList", typeof(ACEDrawbackUnitOfMeasureList), invoiceLine.Lookups.InvoiceUQList.GetType());
		}

		public void TestCustomsUQList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("CustomsUQList", typeof(ABIUnitOfMeasureList), invoiceLine.Lookups.CustomsUQList.GetType());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("CustomsUQList", typeof(AESUnitOfMeasureList), invoiceLine.Lookups.CustomsUQList.GetType());
		}

		public void TestPartsCollection()
		{
			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_IsConsignor = true;

			CusClassification lookUp = Factory.New<CusClassification>();
			lookUp.CC_TariffNum = "12345678";
			lookUp.CC_LookupCode = "LOOKUP";
			lookUp.CC_ClassificationType = CusClassification.ClassificationType.IMP;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_OH_Importer = consignor.PK;

			invoiceLine.JI_PartNo = "PARTNO";
			invoiceLine.JI_CC = lookUp.PK;
			invoiceLine.JI_Description = "DESC";
			invoiceLine.JI_InvoiceUQ = "EA";
			OrgSupplierPart newPart = (OrgSupplierPart)invoiceLine.Lookups.PartsList.AddNew();
			AssertEquals("New Part Defaults", "DESC", newPart.OP_Desc);
			AssertEquals("New Part Defaults", "EA", newPart.OP_StockKeepingUnit);
			AssertEquals("New Part Defaults", "12345678", newPart.PivotsForBinding[0].TariffNumber);
			AssertEquals("Collection Count is 1", 1, newPart.RelatedOrganisations.Count);
			AssertEquals("Owner", declaration.JE_OH_Importer, newPart.RelatedOrganisations[0].OU_OH);
		}

		public void TestPartsCollectionForInvoiceOrganizations()
		{
			var declarationSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			var declarationImporter = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
			declarationImporter.OH_RL_NKClosestPort = "USLAX";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = declarationSupplier.PK;
			declaration.JE_OH_Importer = declarationImporter.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceSupplier = Factory.NewWithValidTestData<OrgHeader>();
			var invoiceImporter = Factory.NewWithValidTestData<OrgHeader>();

			var classificationLookup = Factory.New<CusClassification>();
			classificationLookup.CC_TariffNum = "12345678";
			classificationLookup.CC_LookupCode = "LOOKUP";
			classificationLookup.CC_ClassificationType = CusClassification.ClassificationType.IMP;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = invoiceSupplier.PK;
			invoice.JZ_OH_Buyer = invoiceImporter.PK;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_PartNo = "PARTNO";
			invoiceLine.JI_CC = classificationLookup.PK;
			invoiceLine.JI_Description = "DESC";
			invoiceLine.JI_InvoiceUQ = "EA";
			OrgSupplierPart newPart = (OrgSupplierPart)invoiceLine.Lookups.PartsList.AddNew();
			AssertEquals(1, newPart.RelatedOrganisations.Count);
			AssertEquals("Owner", invoiceImporter.PK, newPart.RelatedOrganisations[0].OU_OH);
		}

		public void TestUSCountryOfOrigins()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Assert(invoiceLine.Lookups.USCountryOfOrigins is USCCountryCollection);
		}

		public void TestParentIDList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "Inv73849";
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("Invoice line should not offer itself as parent", 0, invoiceLine1.Lookups.ParentIDList.Count);

			JobComInvoiceLine invoiceLine2 = invoiceLine1.AddSecondaryInvoiceLine();
			JobComInvoiceLine invoiceLine3 = invoiceLine1.AddSecondaryInvoiceLine();
			JobComInvoiceLine invoiceLine4 = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("Invoice line 1 should have 1 other line (Invoice line 4) as potential parent", 1, invoiceLine1.Lookups.ParentIDList.Count);
			Assert("Invoice line 1 should now show Invoice line 4 as potential parent", invoiceLine1.Lookups.ParentIDList.ContainsCode("Inv73849 - 4"));

			AssertEquals("Invoice line 2 is an additional tariff line & should show both Parent Invoice lines and children lines as potential parent lines", 2, invoiceLine2.Lookups.ParentIDList.Count);
			Assert("Invoice line 2 should show Invoice line 1 & 4 as potential parents", invoiceLine2.Lookups.ParentIDList.ContainsCode("Inv73849 - 1"));
			Assert("Invoice line 2 should show Invoice line 1 & 4 as potential parents", invoiceLine2.Lookups.ParentIDList.ContainsCode("Inv73849 - 4"));

			AssertEquals("Invoice line 3 is an additional tariff line & should show both Parent Invoice lines and children lines as potential parent lines", 2, invoiceLine3.Lookups.ParentIDList.Count);
			Assert("Invoice line 3 should show Invoice line 1 & 4 as potential parents", invoiceLine3.Lookups.ParentIDList.ContainsCode("Inv73849 - 1"));
			Assert("Invoice line 3 should show Invoice line 1 & 4 as potential parents", invoiceLine3.Lookups.ParentIDList.ContainsCode("Inv73849 - 4"));

			AssertEquals("Invoice line 4 should show Invoice line 1 as potential parent", 1, invoiceLine4.Lookups.ParentIDList.Count);
			Assert("Invoice line 4 should show Invoice line 1 as potential parents", invoiceLine4.Lookups.ParentIDList.ContainsCode("Inv73849 - 1"));
			Assert("Invoice line 4 should not offer itself as parent", !invoiceLine4.Lookups.ParentIDList.ContainsCode("Inv73849 - 4"));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine1.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine2.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine3.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;

			AssertEquals(3, invoiceLine3.Lookups.ParentIDList.Count);
			Assert("Invoice line 3 should show Invoice line 1 & 2 & 4 as potential parents", invoiceLine3.Lookups.ParentIDList.ContainsCode("Inv73849 - 1"));
			Assert("Invoice line 3 should show Invoice line 1 & 2 & 4 as potential parents", invoiceLine3.Lookups.ParentIDList.ContainsCode("Inv73849 - 2"));
			Assert("Invoice line 3 should show Invoice line 1 & 2 & 4 as potential parents", invoiceLine3.Lookups.ParentIDList.ContainsCode("Inv73849 - 4"));
			Assert("Invoice line 3 should not show Invoice line 3 as potential parents", !invoiceLine3.Lookups.ParentIDList.ContainsCode("Inv73849 - 3"));
		}

		public void TestDRWCMCDIndicatorCodeList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "Inv73849";
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			CodeDescriptionPairList dRWCMCDIndicatorCodeList = invoiceLine1.Lookups.DRWCMCDIndicatorCodeList;
			AssertEquals("DRWCMCDIndicatorCodeList count", 3, dRWCMCDIndicatorCodeList.Count);
			Assert(dRWCMCDIndicatorCodeList.ContainsCode("E"));
			Assert(dRWCMCDIndicatorCodeList.ContainsCode("D"));
			Assert(dRWCMCDIndicatorCodeList.ContainsCode("M"));
		}

		public void TestFDADisclaimReasonList()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1111111111";
			tariff.UE_PGACodes = "FD1";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "Inv73849";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var list = invoiceLine.AddInfoLookups.FDADisclaimReasonList;
			AssertEquals("count", 4, list.Count);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.B));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.C));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.D));

			invoiceLine.JI_Tariff = "1111111111";
			list = invoiceLine.AddInfoLookups.FDADisclaimReasonList;
			AssertEquals("count", 2, list.Count);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.F));
			Assert(!list.ContainsCode(PGADisclaimReasonList.Codes.B));
			Assert(!list.ContainsCode(PGADisclaimReasonList.Codes.C));
			Assert(!list.ContainsCode(PGADisclaimReasonList.Codes.D));
		}

		public void TestDRWExportActionList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(6, invoiceLine.Lookups.DRWExportActionList.Count);
			Assert(invoiceLine.Lookups.DRWExportActionList.ContainsCode("D"));
			Assert(invoiceLine.Lookups.DRWExportActionList.ContainsCode("F"));
			Assert(!invoiceLine.Lookups.DRWExportActionList.ContainsCode("E"));

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertEquals(2, invoiceLine.Lookups.DRWExportActionList.Count);
			Assert(invoiceLine.Lookups.DRWExportActionList.ContainsCode("D"));
			Assert(invoiceLine.Lookups.DRWExportActionList.ContainsCode("E"));
			Assert(!invoiceLine.Lookups.DRWExportActionList.ContainsCode("F"));
		}

		public void TestDRWExportActionList_DeclarationIsNull()
		{
			var line = Factory.New<JobComInvoiceLine>();
			AssertNoExceptionThrown(() =>
			{
				_ = line.Lookups.DRWExportActionList;
			});
		}

		public void TestACEDrawbackActionList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var list = invoiceLine.Lookups.ACEDrawbackActionList;
			AssertEquals(9, list.Count);
		}

		public void TestDrawbackClaimBasisList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var list = invoiceLine.Lookups.DrawbackClaimBasisList;
			AssertEquals(8, list.Count);
		}

		public void TestDrawbackAccountingCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNotNull(invoiceLine.Lookups.DrawbackAccountingCodes);
			AssertEquals(typeof(DrawbackAccountingMethodCodeList), invoiceLine.Lookups.DrawbackAccountingCodes.GetType());
			AssertEquals(1, invoiceLine.Lookups.DrawbackAccountingCodes.Count);
			AssertEquals(true, invoiceLine.Lookups.DrawbackAccountingCodes.ContainsCode(DrawbackAccountingMethodCodeList.Codes._00));
			AssertEquals(false, invoiceLine.Lookups.DrawbackAccountingCodes.ContainsCode(DrawbackAccountingMethodCodeList.Codes._01));

			declaration.US_EntryType = "77";
			AssertNotNull(invoiceLine.Lookups.DrawbackAccountingCodes);
			AssertEquals(typeof(DrawbackAccountingMethodCodeList), invoiceLine.Lookups.DrawbackAccountingCodes.GetType());
			AssertEquals(1, invoiceLine.Lookups.DrawbackAccountingCodes.Count);
			AssertEquals(true, invoiceLine.Lookups.DrawbackAccountingCodes.ContainsCode(DrawbackAccountingMethodCodeList.Codes._00));
			AssertEquals(false, invoiceLine.Lookups.DrawbackAccountingCodes.ContainsCode(DrawbackAccountingMethodCodeList.Codes._01));

			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._51;
			AssertNotNull(invoiceLine.Lookups.DrawbackAccountingCodes);
			AssertEquals(typeof(DrawbackAccountingMethodCodeList), invoiceLine.Lookups.DrawbackAccountingCodes.GetType());
			AssertEquals(8, invoiceLine.Lookups.DrawbackAccountingCodes.Count);
			AssertEquals(false, invoiceLine.Lookups.DrawbackAccountingCodes.ContainsCode(DrawbackAccountingMethodCodeList.Codes._00));
			AssertEquals(true, invoiceLine.Lookups.DrawbackAccountingCodes.ContainsCode(DrawbackAccountingMethodCodeList.Codes._01));
		}

		public void TestLicencePermitTypes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "91", "Test Code 1", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "92", "Test Code 2", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(2, invoiceLine.Lookups.LicencePermitTypes.Count);
			AssertEquals("91", invoiceLine.Lookups.LicencePermitTypes[0].Code);
			AssertEquals("92", invoiceLine.Lookups.LicencePermitTypes[1].Code);
		}

		public void TestSupTariffsList()
		{
			var startDate = ZDateTime.Now.AddMonths(-1);
			var endDate = ZDateTime.Now.AddMonths(1);

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping.ZZZ_DataGrouping, Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var tradeGroup = helper.LoadOrCreateTradeGroup(dataGrouping.ZZZ_DataGrouping, Core.Constants.CountryCodes.Russia);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Russia, startDate.Date, endDate.Date);
			var rateType = helper.CreateNewOrGetExistingRateType(dataGrouping.ZZZ_DataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RateCodes.Codes.Duty, rateType.PK);

			var parent01 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "1111111111", startDate, endDate);
			var parent02 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "2222222222", startDate, endDate);
			var parent03 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "3333333333", startDate, endDate);

			var child01 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "4444444444", startDate, endDate);
			var child02 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "5555555555", startDate, endDate);
			var child03 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "6666666666", startDate, endDate);

			var attribute11 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.RussianTariffs, child01);
			var attribute12 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.BabyFomula, child01);
			var attribute21 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.RussianTariffs, child02);
			var attribute32 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.BabyFomula, child03);

			var rate01 = helper.CreateRefCusRate(child01.PK, rateCode.PK, startDate, endDate);
			var rate02 = helper.CreateRefCusRate(child02.PK, rateCode.PK, startDate, endDate);
			var rate03 = helper.CreateRefCusRate(child03.PK, rateCode.PK, startDate, endDate);

			var applicability01 = helper.CreateCusApplicability(rate01.PK, tradeGroup, startDate, endDate);
			var applicability02 = helper.CreateCusApplicability(rate02.PK, tradeGroup, startDate, endDate);
			var applicability03 = helper.CreateCusApplicability(rate03.PK, tradeGroup, startDate, endDate);

			var relation11 = helper.CreateTariffRelationship(child01.PK, tariffType.PK, parent01.ZZ1_TariffCode);
			var relation12 = helper.CreateTariffRelationship(child01.PK, tariffType.PK, parent02.ZZ1_TariffCode);
			var relation22 = helper.CreateTariffRelationship(child02.PK, tariffType.PK, parent02.ZZ1_TariffCode);
			var relation23 = helper.CreateTariffRelationship(child02.PK, tariffType.PK, parent03.ZZ1_TariffCode);
			var relation33 = helper.CreateTariffRelationship(child03.PK, tariffType.PK, parent03.ZZ1_TariffCode);
			var relation31 = helper.CreateTariffRelationship(child03.PK, tariffType.PK, parent01.ZZ1_TariffCode);

			Factory.Save();

			var uscTariff01 = Factory.New<USCTariff>();
			uscTariff01.UE_Tariff = parent01.ZZ1_TariffCode;
			uscTariff01.UE_DateFrom = parent01.ZZ1_StartDate;
			uscTariff01.UE_DateTo = parent01.ZZ1_EndDate;

			var uscTariff02 = Factory.New<USCTariff>();
			uscTariff02.UE_Tariff = parent02.ZZ1_TariffCode;
			uscTariff02.UE_DateFrom = parent02.ZZ1_StartDate;
			uscTariff02.UE_DateTo = parent02.ZZ1_EndDate;

			var uscTariff03 = Factory.New<USCTariff>();
			uscTariff03.UE_Tariff = parent03.ZZ1_TariffCode;
			uscTariff03.UE_DateFrom = parent03.ZZ1_StartDate;
			uscTariff03.UE_DateTo = parent03.ZZ1_EndDate;

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Russia;
			invoiceLine.JI_Tariff = "1111111111";
			var supTariffs = invoiceLine.ApplicableSupTariffs;
			AssertEquals(2, supTariffs.Length);
			AssertEquals("Sup tariff list includes N/A.", true, supTariffs.FirstOrDefault(x => x.Tariff == TariffViewAsCodeDescription.NotApplicableCode) != null);

			invoiceLine.JI_Tariff = "2222222222";
			supTariffs = invoiceLine.ApplicableSupTariffs;
			AssertEquals(3, supTariffs.Length);
			AssertEquals("Sup tariff list includes N/A.", true, supTariffs.FirstOrDefault(x => x.Tariff == TariffViewAsCodeDescription.NotApplicableCode) != null);

			invoiceLine.JI_Tariff = "3333333333";
			supTariffs = invoiceLine.ApplicableSupTariffs;
			AssertEquals(1, supTariffs.Length);
			AssertEquals("Sup tariff list does not include N/A.", false, supTariffs.FirstOrDefault(x => x.Tariff == TariffViewAsCodeDescription.NotApplicableCode) != null);
		}

		void AssertCodeDescription(ICodeDescription codeDescription, string expectedCode, string expectedDescription)
		{
			AssertEquals("Code", expectedCode, codeDescription.Code);
			AssertEquals("Description", expectedDescription, codeDescription.Description);
		}
	}
}
