using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Customs.Business.Testing;
	using DocumentEngine.DocumentDelivery;
	using DocumentEngine.Scheduler.Business;
	using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.Testing;
	using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.Customs.NZ.TradeSingleWindow;
	using Enterprise.ZArchitecture;
	using NUnit.Framework;
	using ZArchitecture.Schema;

	[TestedType(typeof(CusEntryLine))]
	class CusEntryLineTest : CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		public void TestReadOnly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			Assert(entryLine.DutyRateDescriptionInfo.ReadOnly);
			Assert(entryLine.IncoTermInfo.ReadOnly);
			Assert(entryLine.TotalLinePriceInLocalCurrencyInfo.ReadOnly);
			Assert(entryLine.FreightInfo.ReadOnly);
			Assert(entryLine.InsuranceInfo.ReadOnly);
			Assert(entryLine.TotalLeviesInfo.ReadOnly);
			Assert(entryLine.TotalDutyLevyGSTInfo.ReadOnly);
			Assert(entryLine.SupplementaryQtyInfo.ReadOnly);
			Assert(entryLine.CountryOfOriginDescriptionInfo.ReadOnly);
			Assert(entryLine.ConcessionCodeInfo.ReadOnly);
			Assert(entryLine.OSCustomsValueInfo.ReadOnly);
		}

		public void TestCommissionInNZD()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "CIF";
			invoice.JZ_InvoiceAmount = 13000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 13000m;

			var freight = invoice.Charges.AddNew();
			freight.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			freight.J7_Amount = 100m;
			freight.J7_IsDutiable = true;

			var insurance = invoice.Charges.AddNew();
			insurance.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			insurance.J7_Amount = 10m;
			insurance.J7_IsDutiable = true;

			var commission = invoice.Charges.AddNew();
			commission.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			commission.J7_Amount = 100m;

			var entryLine = declaration.CusEntryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			declaration.ResumeApportionment();
			AssertEquals("CommissionInNZD", 100m, entryLine.CommissionInNZD);
		}

		public void TestRoyaltiesInNZD()
		{
			var decCreator = new TestFormalEntryCreator(Declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = Declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "CIF";
			invoice.JZ_InvoiceAmount = 13000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var freight = invoice.Charges.AddNew();
			freight.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			freight.J7_Amount = 100m;
			freight.J7_IsDutiable = true;

			var insurance = invoice.Charges.AddNew();
			insurance.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			insurance.J7_Amount = 10m;
			insurance.J7_IsDutiable = true;

			var royalties = invoice.Charges.AddNew();
			royalties.J7_ChargeType = CustomsChargeTypeList.TSWCodes.Royalties;
			royalties.J7_Amount = 355m;

			var invoiceLine1 = Declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 13000m;
			invoiceLine1.JI_Tariff = "2204.21.18.11A";
			invoiceLine1.JI_CustomsQuantity = 1000m;
			invoiceLine1.JI_CustomsUnitQty = "LTR";
			invoiceLine1.JI_QualifiesForPreferentialDuty = "N";
			invoiceLine1.JI_CountryOfOrigin = "US";
			invoiceLine1.JI_RN_NKCountryOfExport = "US";

			var invoiceLine2 = Declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 5000m;
			invoiceLine2.JI_Tariff = "2204.21.18.11A";
			invoiceLine2.JI_CustomsQuantity = 10m;
			invoiceLine2.JI_CustomsUnitQty = "LTR";
			invoiceLine2.JI_QualifiesForPreferentialDuty = "N";
			invoiceLine2.JI_CountryOfOrigin = "US";
			invoiceLine2.JI_RN_NKCountryOfExport = "US";

			decCreator.MergeDeclaration();
			AssertEquals("Precondition: MergedLines.Count", 1, Declaration.CusEntryHeader.MergedLines.Count);
			var entryLine = Declaration.CusEntryHeader.MergedLines[0];
			AssertEquals("RoyaltiesInNZD should be the total of merged line apportioned Royalties charges", 355m, entryLine.RoyaltiesInNZD);
		}

		public void TestSupplierNameDropsBack()
		{
			var decCreator = new TestFormalEntryCreator(Declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			ZGuid miscOrgPK = Declaration.CachedMiscOrgPK;
			Declaration.JE_OH_Importer = miscOrgPK;
			Declaration.MiscImporterName = "Miscellaneous Importers Guzzle Beer with Alacrity.";
			Declaration.JE_OH_Supplier = miscOrgPK;
			Declaration.MiscSupplierName = "Miscellaneous Suppliers Aren't Really Much Better.";

			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");

			decCreator.MergeDeclaration();
			AssertEquals("Precondition: MergedLines.Count", 1, Declaration.CusEntryHeader.MergedLines.Count);

			CusEntryLine entryLine = Declaration.CusEntryHeader.MergedLines[0];
			AssertEquals("entryLine.SupplierName", "MISCELLANEOUS SUPPLIERS AREN'T REALLY MUCH BETTER.", entryLine.SupplierName);

			OrgHeader realOrg = Factory.New<OrgHeader>();
			realOrg.OH_FullName = "BUT THIS IS TOPS";
			JobComInvoiceHeader invoiceHeader = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0];
			invoiceHeader.JZ_OH_Supplier = realOrg.PK;
			AssertEquals("entryLine.SupplierName", "BUT THIS IS TOPS", entryLine.SupplierName);
		}

		[TestDate(2006, 1, 1)]
		public void TestCalculateDutyAndGSTFillsInDutyRateFields()
		{
			RefCurrency currencyAUD = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			ZDecimal baseValue = 10000m;
			EntryLine.CL_CustomsValue = baseValue;
			InvoiceLine.InvoiceHeader.JobComInvoiceLines.Add(InvoiceLine);
			InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = currencyAUD.RX_Code;
			InvoiceLine.InvoiceHeader.JZ_InvoiceAmount = 17999.00m;
			InvoiceLine.InvoiceHeader.JZ_IncoTerm = "CIF";
			InvoiceLine.JI_LinePrice = 16899.00m;
			EntryLine.CL_AdValoremTariff = "7007.21.02.01K";
			InvoiceLine.InvoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000m, currencyAUD.RX_Code);
			InvoiceLine.InvoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, currencyAUD.RX_Code);
			AssertEquals("CL_DutyPercent Before Calculation", 0.00m, EntryLine.CL_DutyPercent);
			AssertEquals("CL_FlatAmount Before Calculation", 0.00m, EntryLine.CL_FlatAmount);
			AssertEquals("CL_FlatAmountUQ Before Calculation", "", EntryLine.CL_FlatAmountUQ);

			EntryLine.CalculateDutyAndGST();
			AssertEquals("CL_DutyPercent After Calculation", 17.50m, EntryLine.CL_DutyPercent);
			AssertEquals("CL_FlatAmount After Calculation", 0.00m, EntryLine.CL_FlatAmount);
			AssertEquals("CL_FlatAmountUQ After Calculation", "", EntryLine.CL_FlatAmountUQ);

			EntryLine.CL_AdValoremTariff = "2208.60.19.09J";
			InvoiceLine.JI_CustomsQuantity = 3m;
			InvoiceLine.JI_CustomsUnitQty = "LPA";
			EntryLine.CalculateDutyAndGST();
			AssertEquals("CL_DutyPercent After Calculation", 6.50m, EntryLine.CL_DutyPercent);
			AssertEquals("CL_FlatAmount After Calculation", 41.1460m, EntryLine.CL_FlatAmount);
			AssertEquals("CL_FlatAmountUQ After Calculation", "LPA", EntryLine.CL_FlatAmountUQ);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			EntryLine.CalculateDutyAndGST();
			AssertEquals("CL_DutyPercent After Calculation", 0.00m, EntryLine.CL_DutyPercent);
			AssertEquals("CL_FlatAmount After Calculation", 0.00m, EntryLine.CL_FlatAmount);
			AssertEquals("CL_FlatAmountUQ After Calculation", "", EntryLine.CL_FlatAmountUQ);
		}

		public void TestItemPackaging()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryLine1 = declaration.CusEntryHeader.MergedLines.AddNew();
			var entryLine2 = declaration.CusEntryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0203.11.00.02C";
			invoiceLine1.JI_Description = "LINE 1";
			invoiceLine1.JI_CL = entryLine1.PK;
			var packaging1 = invoiceLine1.ItemPackages.AddNew();
			packaging1.NZ_NumberOfPackages = 24;
			packaging1.NZ_PackageUQ = "PK";
			packaging1.NZ_PackageVolume = 0.5m;
			packaging1.NZ_ShippingMarks = "GOD-00397";
			var packaging2 = invoiceLine1.ItemPackages.AddNew();
			packaging2.NZ_NumberOfPackages = 12;
			packaging2.NZ_PackageUQ = "BX";
			packaging2.NZ_PackageVolume = 1m;
			packaging2.NZ_ShippingMarks = Core.Constants.ContainerMarking.NoMarks;

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0203.11.00.02C";
			invoiceLine2.JI_Description = "LINE 2";
			invoiceLine2.JI_CL = entryLine1.PK;
			var line2Packaging = invoiceLine2.ItemPackages.AddNew();
			line2Packaging.NZ_NumberOfPackages = 5;
			line2Packaging.NZ_PackageUQ = "BG";
			line2Packaging.NZ_PackageVolume = 1m;
			line2Packaging.NZ_ShippingMarks = "933428-Y";

			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2701.11.00.00C";
			invoiceLine3.JI_Description = "LINE 2";
			invoiceLine3.JI_CL = entryLine2.PK;
			var line3Packaging = invoiceLine3.ItemPackages.AddNew();
			line3Packaging.NZ_NumberOfPackages = 1;
			line3Packaging.NZ_PackageUQ = "PC";
			line3Packaging.NZ_PackageVolume = 0.2m;
			line3Packaging.NZ_ShippingMarks = "P003947/KF6";

			AssertEquals("ItemPackaging for entryLine1 should contain 3 package rows from the invoice lines merged into entryLine1, (2 rows from invoice line 1 & 1 row from invoice line 2)", 3, entryLine1.ItemPackaging.Count());
			foreach (ItemPackaging packingData in entryLine1.ItemPackaging)
			{
				if (packingData.NZ_PackageUQ == "PK")
				{
					AssertEquals("NZ_NumberOfPackages", 24, packingData.NZ_NumberOfPackages);
					AssertEquals("NZ_PackageVolume", 0.5m, packingData.NZ_PackageVolume);
					AssertEquals("NZ_ShippingMarks", "GOD-00397", packingData.NZ_ShippingMarks);
				}
				else if (packingData.NZ_PackageUQ == "BX")
				{
					AssertEquals("NZ_NumberOfPackages", 12, packingData.NZ_NumberOfPackages);
					AssertEquals("NZ_PackageVolume", 1m, packingData.NZ_PackageVolume);
					AssertEquals("NZ_ShippingMarks", Core.Constants.ContainerMarking.NoMarks, packingData.NZ_ShippingMarks);
				}
				else if (packingData.NZ_PackageUQ == "BG")
				{
					AssertEquals("NZ_NumberOfPackages", 5, packingData.NZ_NumberOfPackages);
					AssertEquals("NZ_PackageVolume", 1m, packingData.NZ_PackageVolume);
					AssertEquals("NZ_ShippingMarks", "933428-Y", packingData.NZ_ShippingMarks);
				}
			}

			AssertEquals("ItemPackaging for entryLine2 should contain 1 package rows", 1, entryLine2.ItemPackaging.Count());
			foreach (ItemPackaging packingData in entryLine2.ItemPackaging)
			{
				AssertEquals("NZ_NumberOfPackages", 1, packingData.NZ_NumberOfPackages);
				AssertEquals("NZ_PackageVolume", 0.2m, packingData.NZ_PackageVolume);
				AssertEquals("NZ_ShippingMarks", "P003947/KF6", packingData.NZ_ShippingMarks);
			}
		}

		public void TestGetDescriptionForMergedLinesWhenDescriptionOnLinesIsDifferent()
		{
			NZCClassification classification = Factory.New<NZCClassification>();
			classification.U0_Tariff = "0000.00.00.00X";
			classification.U0_Description = "Lower Case Will Be Dropped By The Message Builder. Must All Be Upper.";

			JobDeclaration declaration = Factory.New<JobDeclaration>();

			CusEntryLine entryLine = declaration.CusEntryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = classification.U0_Tariff;
			invoiceLine1.JI_Description = "LINE 1";
			invoiceLine1.JI_CL = entryLine.PK;

			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = classification.U0_Tariff;
			invoiceLine2.JI_Description = "LINE 2";
			invoiceLine2.JI_CL = entryLine.PK;

			AssertEquals("entryLine.CL_Description", classification.U0_Description.ToUpper(), entryLine.Description);
		}

		public void TestQualForPrefDutyDefaultsThroughFromInvoiceHeader()
		{
			InvoiceLine.InvoiceHeader.JZ_DefaultQualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;
			AssertEquals("EntryLine.PreferentialDutyIndicator", QualifiesForPreferentialDutyList.Codes.Qualifies, EntryLine.PreferentialDutyIndicator);
		}

		public void TestCountryOfOriginDefaultsThroughFromInvoiceHeader()
		{
			InvoiceLine.InvoiceHeader.JZ_RN_NKDefaultOrigin = "ZA";
			AssertEquals("EntryLine.CountryOfOrigin", "ZA", EntryLine.CountryOfOrigin);
		}

		public void TestOriginRegionDefaultsThroughFromInvoiceHeader()
		{
			InvoiceLine.InvoiceHeader.JZ_DefaultOriginRegion = "XXXXXXXXX";
			AssertEquals("InvoiceHeader.JZ_DefaultOriginRegion", "XXXXXXXXX", InvoiceLine.InvoiceHeader.JZ_DefaultOriginRegion);
			AssertEquals("InvoiceLine.JI_OriginRegion", "XXXXXXXXX", InvoiceLine.JI_OriginRegion);
			AssertEquals("EntryLine.OriginRegion", "XXXXXXXXX", EntryLine.OriginRegion);

			InvoiceLine.JI_OriginRegion = "YYYYYYYYY";
			AssertEquals("InvoiceHeader.JZ_DefaultOriginRegion", "XXXXXXXXX", InvoiceLine.InvoiceHeader.JZ_DefaultOriginRegion);
			AssertEquals("InvoiceLine.JI_OriginRegion", "YYYYYYYYY", InvoiceLine.JI_OriginRegion);
			AssertEquals("EntryLine.OriginRegion", "YYYYYYYYY", EntryLine.OriginRegion);
		}

		public void TestCountryOfExportDefaultsThroughFromInvoiceHeader()
		{
			InvoiceLine.InvoiceHeader.JZ_RN_NKDefaultExport = "SG";
			AssertEquals("EntryLine.CountryOfExport", "SG", EntryLine.CountryOfExport);
		}

		public void TestNewCustomsQuantitiesAndUnits()
		{
			InvoiceHeader.JZ_InvoiceNumber = "A";
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1234567890";
			invoiceLine1.JI_Description = "DESCRIPTION";
			invoiceLine1.JI_CustomsQuantity = 3;
			invoiceLine1.JI_CustomsUnitQty = "UT";
			invoiceLine1.JI_SupplementaryQty = 1;
			invoiceLine1.JI_SupplementaryUQ = "PK";
			JobComInvoiceLine invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1234567890";
			invoiceLine2.JI_Description = "DESCRIPTION";
			invoiceLine2.JI_CustomsQuantity = 4;
			invoiceLine2.JI_CustomsUnitQty = "UT";
			invoiceLine2.JI_SupplementaryQty = 2;
			invoiceLine2.JI_SupplementaryUQ = "PK";

			CusEntryLine entryLine = EntryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			AssertEquals("EntryLine.StatisticalQty", 7m, entryLine.StatisticalQty);
			AssertEquals("EntryLine.StatisticalUnit", "UT", entryLine.StatisticalUnit);
			AssertEquals("EntryLine.SupplementaryQty", 3m, entryLine.SupplementaryQty);
			AssertEquals("EntryLine.SupplementaryUQ", "PK", entryLine.SupplementaryUQ);
		}

		public void TestTotalDutiesAndLevies()
		{
			EntryLine.DutyAmount = 11m;
			EntryLine.ALACLevyAmount = 22m;
			EntryLine.ACCFuelLevyAmount = 33m;
			EntryLine.PFMLFuelLevyAmount = 55m;
			EntryLine.HERALevyAmount = 44m;
			EntryLine.SyntheticGreenhouseGasesLevyAmount = 66m;
			AssertEquals(231m, EntryLine.TotalDutiesAndLevies);
		}

		[TestDate(2018, 6, 1)]
		public void TestCalculateDutyAndGST()
		{
			InvoiceLine.InvoiceHeader.JobComInvoiceLines.Add(InvoiceLine);
			InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			InvoiceLine.InvoiceHeader.JZ_InvoiceAmount = 1000.00m;
			InvoiceLine.InvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			InvoiceLine.JI_LinePrice = 1000.00m;
			InvoiceLine.JI_CustomsQuantity = 1000.00m;
			InvoiceLine.JI_CustomsUnitQty = StatisticalUQList.Codes.Litres;
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			InvoiceLine.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Australia;
			EntryLine.CL_AdValoremTariff = InvoiceLine.JI_Tariff = "2710.19.21.10C";
			EntryLine.CL_CustomsValue = 1000m;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.Normal;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			AssertEquals("Duty Value Before Calculations", 0.00m, EntryLine.DutyAmount);

			EntryLine.CalculateDutyAndGST();
			AssertEquals("Import Duty Value Calculation", 485.24m, EntryLine.DutyAmount);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			EntryLine.CalculateDutyAndGST();
			AssertEquals("Excise Duty Calculation", 485.24m, EntryLine.DutyAmount);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			EntryLine.CalculateDutyAndGST();
			AssertEquals("Export Duty Calculation", 0.00m, EntryLine.DutyAmount);
		}

		[TestDate(2018, 6, 1)]
		public void TestNoDutyOrGSTIsCalculatedForIPIEntry()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;

			InvoiceLine.InvoiceHeader.JobComInvoiceLines.Add(InvoiceLine);
			InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			InvoiceLine.InvoiceHeader.JZ_InvoiceAmount = 1000.00m;
			InvoiceLine.InvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			InvoiceLine.JI_LinePrice = 1000.00m;
			InvoiceLine.JI_CustomsQuantity = 1000.00m;
			InvoiceLine.JI_CustomsUnitQty = StatisticalUQList.Codes.Litres;
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			InvoiceLine.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Australia;
			EntryLine.CL_AdValoremTariff = InvoiceLine.JI_Tariff = "2710.19.21.10C";
			AssertEquals("Duty Value Before Calculations", 0.00m, EntryLine.DutyAmount);

			EntryLine.CalculateDutyAndGST();
			AssertEquals("Duty on IPI Declaration", 0.00m, EntryLine.DutyAmount);
			AssertEquals("GST on IPI Declaration", 0.00m, EntryLine.GSTAmount);
			AssertEquals(InvoiceLine.JI_Tariff, EntryLine.CL_AdValoremTariff);
		}

		public void TestDissectionReportDutyAmount()
		{
			var currencyNZD = RefCurrency.LoadFromCurrencyCode(Factory, "NZD");
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = currencyNZD.RX_Code;
			InvoiceHeader.JZ_InvoiceAmount = 5785.00m;
			InvoiceHeader.JZ_IncoTerm = "CIF";

			var invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 3000.00m;
			invoiceLine1.JI_Tariff = "7007.21.02.01K";
			invoiceLine1.JI_DutyCreditAmount = 228.73m;

			var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 2785.00m;
			invoiceLine2.JI_Tariff = "7007.21.02.01K";
			invoiceLine2.JI_DutyCreditAmount = 191.30m;

			AssertEquals("Should be no Merged Lines on Declaration yet", 0, Declaration.CusEntryHeader.MergedLines.Count);
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Should be one Merged Line after merge", 1, Declaration.CusEntryHeader.MergedLines.Count);
			var cusEntryLine = Declaration.CusEntryHeader.MergedLines[0];
			AssertEquals("DissectionReportDutyAmount - should be value of individual invoice line DutyCreditAmount for Drawback", 420.03m, cusEntryLine.DissectionReportDutyAmount);

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			cusEntryLine = Declaration.CusEntryHeader.MergedLines[0];
			AssertEquals("DissectionReportDutyAmount - should be calculate DutyAmount for non Drawback entry", 578.50m, cusEntryLine.DissectionReportDutyAmount);
		}

		public void TestDissectionReportDutyRate()
		{
			var currencyNZD = RefCurrency.LoadFromCurrencyCode(Factory, "NZD");
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = currencyNZD.RX_Code;
			InvoiceHeader.JZ_InvoiceAmount = 5785.00m;
			InvoiceHeader.JZ_IncoTerm = "CIF";

			var invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 3000.00m;
			invoiceLine1.JI_Tariff = "7007.21.02.01K";
			invoiceLine1.JI_DutyCreditAmount = 228.73m;

			var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 2785.00m;
			invoiceLine2.JI_Tariff = "7007.21.02.01K";
			invoiceLine2.JI_DutyCreditAmount = 191.30m;

			AssertEquals("Should be no Merged Lines on Declaration yet", 0, Declaration.CusEntryHeader.MergedLines.Count);
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Should be one Merged Line after merge", 1, Declaration.CusEntryHeader.MergedLines.Count);
			var cusEntryLine = Declaration.CusEntryHeader.MergedLines[0];
			AssertEquals("DissectionReportDutyRate - should be blank for Drawback", "", cusEntryLine.DissectionReportDutyRate);

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			cusEntryLine = Declaration.CusEntryHeader.MergedLines[0];
			AssertEquals("DissectionReportDutyRate - should be calculate DutyAmount rate for non Drawback entries", "Duty Rate: 10.00%", cusEntryLine.DissectionReportDutyRate);
		}

		public void TestRenderedReportHasDutyCreditValueWhenDrawback()
		{
			var currencyNZD = RefCurrency.LoadFromCurrencyCode(Factory, "NZD");
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = currencyNZD.RX_Code;
			InvoiceHeader.JZ_InvoiceAmount = 5785.00m;
			InvoiceHeader.JZ_IncoTerm = "CIF";

			var invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 3000.00m;
			invoiceLine1.JI_Tariff = "7007.21.02.01K";
			invoiceLine1.JI_DutyCreditAmount = 228.73m;

			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var cusEntryLine = Declaration.CusEntryHeader.MergedLines[0];
			AssertEquals("DissectionReportDutyAmount", 228.73m, cusEntryLine.DissectionReportDutyAmount);
			AssertEquals("DissectionReportDutyRate", "", cusEntryLine.DissectionReportDutyRate);

			ZString menuPath = "";
			ZString filterList = "";
			Guid branchGuid = (Declaration.JE_GB.IsEmpty ? GlbBranch.CurrentBranch.PK : Declaration.JE_GB).ToGuid();
			Guid departmentGuid = Guid.Empty;

			ZGuid printQueuePK = new ZGuid(NZCustomsDataRegistry.Instance.CustomsCertificatePrinter.GetFallBackValueAtAllLevels(Guid.Empty, branchGuid, departmentGuid));
			ZInt copies = new ZInt(NZCustomsDataRegistry.Instance.CustomsCertificateCopies.GetValueWithoutFallback(Guid.Empty, branchGuid, Guid.Empty));
			ZBool copyToEDocs = new ZBool(NZCustomsDataRegistry.Instance.CustomsCertificateCopyToEDocs.GetValueWithoutFallback(Guid.Empty, branchGuid, Guid.Empty));
			SilentDocumentPrinter documentPrinter = new SilentDocumentPrinter(Factory, Declaration, "Dissection Report", menuPath, filterList);
			documentPrinter.Print(printQueuePK, copies, copyToEDocs);

			var print = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.Contains, "Dissection Report"));
			AssertNotNull("A print was created, we did not crash on duty rate value", print);
			ExcelContentTest.AssertPrintJobContainsAndNotContainsText(new string[] { "[7007.21.02.01K]   {AK}-[1]   {AQ}-[3000]   {AU}-[228.73]" }, Array.Empty<string>(), print);
		}

		public void TestRenderedReportHasDutyRateAndValueWhenNotDrawback()
		{
			var currencyNZD = RefCurrency.LoadFromCurrencyCode(Factory, "NZD");
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = currencyNZD.RX_Code;
			InvoiceHeader.JZ_InvoiceAmount = 5785.00m;
			InvoiceHeader.JZ_IncoTerm = "CIF";

			var invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 3000.00m;
			invoiceLine1.JI_Tariff = "7007.21.02.01K";

			var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 2785.00m;
			invoiceLine2.JI_Tariff = "7007.21.02.01K";
			invoiceLine2.JI_DutyCreditAmount = 191.30m;

			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var cusEntryLine = Declaration.CusEntryHeader.MergedLines[0];
			AssertEquals("DissectionReportDutyAmount", 578.50m, cusEntryLine.DissectionReportDutyAmount);
			AssertEquals("DissectionReportDutyRate", "Duty Rate: 10.00%", cusEntryLine.DissectionReportDutyRate);

			ZString menuPath = "";
			ZString filterList = "";
			Guid branchGuid = (Declaration.JE_GB.IsEmpty ? GlbBranch.CurrentBranch.PK : Declaration.JE_GB).ToGuid();
			Guid departmentGuid = Guid.Empty;

			ZGuid printQueuePK = new ZGuid(NZCustomsDataRegistry.Instance.CustomsCertificatePrinter.GetFallBackValueAtAllLevels(Guid.Empty, branchGuid, departmentGuid));
			ZInt copies = new ZInt(NZCustomsDataRegistry.Instance.CustomsCertificateCopies.GetValueWithoutFallback(Guid.Empty, branchGuid, Guid.Empty));
			ZBool copyToEDocs = new ZBool(NZCustomsDataRegistry.Instance.CustomsCertificateCopyToEDocs.GetValueWithoutFallback(Guid.Empty, branchGuid, Guid.Empty));
			SilentDocumentPrinter documentPrinter = new SilentDocumentPrinter(Factory, Declaration, "Dissection Report", menuPath, filterList);
			documentPrinter.Print(printQueuePK, copies, copyToEDocs);

			var print = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.Contains, "Dissection Report"));
			AssertNotNull("A print was created, we did not crash on duty values", print);
			ExcelContentTest.AssertPrintJobContainsAndNotContainsText(new string[] { "[7007.21.02.01K]   {AK}-[1]   {AQ}-[5785]   {AU}-[578.5]" }, Array.Empty<string>(), print);
			ExcelContentTest.AssertPrintJobContainsAndNotContainsText(new string[] { "Duty Rate: 10.00%" }, Array.Empty<string>(), print);
		}

		[TestDate(2007, 1, 1)]
		public override void TestGSTRate()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			InvoiceLine.JI_IsZeroRatedGST = "Y";
			AssertEquals("EntryLine.GSTRate", 0.00m, EntryLine.GSTRate);
			InvoiceLine.JI_IsZeroRatedGST = "N";
			AssertEquals("EntryLine.GSTRate", GSTCalculator.GetGSTRate(Factory, EntryLine.EntryHeader.DateForDutyRate), EntryLine.GSTRate);

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			AssertEquals("Default Value for GST Rate", 0.125m, entryLine.GSTRate);
		}

		[TestDate(2006, 1, 1)]
		public override void TestDutyRateDescription()
		{
			RefCurrency currencyNZD = RefCurrency.LoadFromCurrencyCode(Factory, "NZD");
			RefCurrency currencyAUD = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");

			ZDecimal baseValue = 10000m;
			EntryLine.CL_CustomsValue = baseValue;

			InvoiceLine.InvoiceHeader.JobComInvoiceLines.Add(InvoiceLine);
			InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = currencyAUD.RX_Code;
			InvoiceLine.InvoiceHeader.JZ_InvoiceAmount = 17999.00m;
			InvoiceLine.InvoiceHeader.JZ_IncoTerm = "CIF";

			InvoiceLine.JI_LinePrice = 16899.00m;
			EntryLine.CL_AdValoremTariff = "7007.21.02.01K";

			InvoiceLine.InvoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000m, currencyAUD.RX_Code);
			InvoiceLine.InvoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, currencyAUD.RX_Code);

			AssertEquals("EntryLine.DutyAndLevyRate", "17.50%", EntryLine.DutyRateDescription);
		}

		public void TestRandomLine()
		{
			var decCreator = new TestFormalEntryCreator(Declaration);
			decCreator.SetupTestConsignmentDetails();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			Declaration.JE_DateOfArrival = new ZDateTime(2005, 1, 1);
			Declaration.JE_EDITransmitDate = new ZDateTime(2005, 1, 1);
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10010m);
			decCreator.SetupImportInvoiceLine("0000.00.00.00A", "DESCRIPTION", "AU", "AU", "N", 10000m);
			decCreator.SetupImportInvoiceLine("0000.00.00.00A", "DESCRIPTION", "AU", "AU", "N", 10000m);
			JobComInvoiceLine invoiceLine1 = Declaration.Invoices[0].JobComInvoiceLines[0];
			JobComInvoiceLine invoiceLine2 = Declaration.Invoices[0].JobComInvoiceLines[1];
			invoiceLine1.JI_LineNo = 1;
			invoiceLine1.JI_Tariff = "0000.00.01.00A";
			invoiceLine2.JI_LineNo = 2;
			invoiceLine1.JI_LinePrice = 10m;
			invoiceLine2.JI_Tariff = "0000.00.02.00A";
			invoiceLine1.JI_ParentLineNo = "2";
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL", 10, "PK");
			AssertEquals("Should be no Merged Lines on a fresh Declaration", 0, Declaration.CusEntryHeader.MergedLines.Count);
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Should be one Merged Line after merge", 1, Declaration.CusEntryHeader.MergedLines.Count);
			CusEntryLine cusEntryLine = Declaration.CusEntryHeader.MergedLines[0];
			AssertEquals("Merged line tariff should be from line 2", "0000.00.02.00A", cusEntryLine.CL_AdValoremTariff);
			JobComInvoiceLine randomLine = cusEntryLine.RandomLine;
			AssertEquals("Random Line should be line 2", (ZShort)2, randomLine.JI_LineNo);
			invoiceLine1.JI_LinePrice = 10000m;
			invoiceLine2.JI_LinePrice = 10m;
			invoiceLine1.JI_ParentLineNo = "";
			invoiceLine2.JI_ParentLineNo = "1";
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Should be one Merged Line after merge", 1, Declaration.CusEntryHeader.MergedLines.Count);
			cusEntryLine = Declaration.CusEntryHeader.MergedLines[0];
			AssertEquals("Merged line tariff should be from line 1", "0000.00.01.00A", cusEntryLine.CL_AdValoremTariff);
			randomLine = cusEntryLine.RandomLine;
			AssertEquals("Random Line should be line 1", (ZShort)1, randomLine.JI_LineNo);
		}

		public void TestPermitCodes()
		{
			AssertNotNull(InvoiceLine); // Sets up Invoice Line through Lazy Loading
			AssertNotNull(EntryLine.PermitCodes);
		}

		public void TestOtherInfos()
		{
			AssertNotNull(InvoiceLine); // Sets up Invoice Line through Lazy Loading
			AssertNotNull(EntryLine.OtherInfos);
		}

		public void TestProhibitedCodes()
		{
			AssertNotNull(InvoiceLine); // Sets up Invoice Line through Lazy Loading
			AssertNotNull(EntryLine.ProhibitedCodes);
		}

		public void TestHasFSAPermit()
		{
			InvoiceHeader.JZ_InvoiceNumber = "TSW";
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0801.11.00.00J";
			invoiceLine1.JI_Description = "Desiccated coconuts";

			CusEntryLine entryLine = EntryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;
			AssertEquals(false, entryLine.HasFSAPermit);

			var permitCode = invoiceLine1.PermitCodes.AddNew();
			permitCode.ZO_Code = PermitCodeList.Codes.NZFoodSafetyAuthorityNew;
			AssertEquals("Only has permit when data is entered", false, entryLine.HasFSAPermit);

			permitCode.ZO_Data = "J032938Y";
			AssertEquals(true, entryLine.HasFSAPermit);
		}

		public void TestUNDGNo()
		{
			InvoiceHeader.JZ_InvoiceNumber = "TSW";
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1234567890";
			invoiceLine1.JI_Description = "DESCRIPTION";

			CusEntryLine entryLine = EntryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;
			AssertEquals("UNDGNo", "", entryLine.UNDGNo);

			var undg = invoiceLine1.UNDGs.AddNew();
			undg.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			undg.DI_DGFlashPoint = -25m;

			AssertEquals("UNDGNo", "0004a", entryLine.UNDGNo);
		}

		public void TestLotNumber()
		{
			InvoiceHeader.JZ_InvoiceNumber = "TSW";
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1234567890";
			invoiceLine1.JI_Description = "DESCRIPTION";
			invoiceLine1.JI_LotNumber = "7892001";

			CusEntryLine entryLine = EntryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;

			AssertEquals("LotNumber", "7892001", entryLine.LotNumber);
		}

		public void TestBrandName()
		{
			InvoiceHeader.JZ_InvoiceNumber = "TSW";
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1234567890";
			invoiceLine1.JI_Description = "DESCRIPTION";
			invoiceLine1.JI_BrandName = "IBM";

			CusEntryLine entryLine = EntryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;

			AssertEquals("BrandName", "IBM", entryLine.BrandName);
		}

		public void TestCommonName()
		{
			InvoiceHeader.JZ_InvoiceNumber = "TSW";
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1234567890";
			invoiceLine1.JI_Description = "DESCRIPTION";
			invoiceLine1.JI_CommonName = "Esky";

			CusEntryLine entryLine = EntryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;

			AssertEquals("CommonName", "Esky", entryLine.CommonName);
		}

		public void TestRegisteredName()
		{
			InvoiceHeader.JZ_InvoiceNumber = "TSW";
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1234567890";
			invoiceLine1.JI_Description = "DESCRIPTION";
			invoiceLine1.JI_RegisteredName = "KRAFT VEGEMITE";

			CusEntryLine entryLine = EntryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;

			AssertEquals("RegisteredName", "KRAFT VEGEMITE", entryLine.RegisteredName);
		}

		public void TestTradeName()
		{
			InvoiceHeader.JZ_InvoiceNumber = "TSW";
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1234567890";
			invoiceLine1.JI_Description = "DESCRIPTION";
			invoiceLine1.JI_TradeName = "WINDOWS";

			CusEntryLine entryLine = EntryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;

			AssertEquals("TradeName", "WINDOWS", entryLine.TradeName);
		}

		public void TestUsedGoods()
		{
			InvoiceHeader.JZ_InvoiceNumber = "TSW";
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1234567890";
			invoiceLine1.JI_Description = "DESCRIPTION";
			invoiceLine1.JI_UsedGoods = true;

			CusEntryLine entryLine = EntryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;

			AssertEquals("UsedGoods", true, entryLine.UsedGoods);
		}

		public void TestGeneticallyModified()
		{
			InvoiceHeader.JZ_InvoiceNumber = "TSW";
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1234567890";
			invoiceLine1.JI_Description = "DESCRIPTION";
			invoiceLine1.JI_GeneticallyModified = false;

			CusEntryLine entryLine = EntryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;

			AssertEquals("GeneticallyModified", false, entryLine.GeneticallyModified);
		}

		public void TestDateMarking()
		{
			ZDateTime bestBeforeDate = ZDateTime.Today.AddMonths(6);
			InvoiceHeader.JZ_InvoiceNumber = "TSW";
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1234567890";
			invoiceLine1.JI_Description = "DESCRIPTION";
			invoiceLine1.JI_DateMarking = bestBeforeDate;

			CusEntryLine entryLine = EntryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;

			AssertEquals("DateMarking", bestBeforeDate, entryLine.DateMarking);
		}

		public void TestIntendedUseCode()
		{
			InvoiceHeader.JZ_InvoiceNumber = "TSW";
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1234567890";
			invoiceLine1.JI_Description = "DESCRIPTION";
			invoiceLine1.JI_IntendedUseCode = IntendedUseCodeList.Codes.FP;

			CusEntryLine entryLine = EntryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;

			AssertEquals("IntendedUseCode", "FP", entryLine.IntendedUseCode);
		}

		public void TestIntendedUseText()
		{
			var intendedUseText = "Text to be used when there is no appropriate code";
			InvoiceHeader.JZ_InvoiceNumber = "TSW";
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1234567890";
			invoiceLine1.JI_Description = "DESCRIPTION";
			invoiceLine1.JI_IntendedUse = intendedUseText;

			CusEntryLine entryLine = EntryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;

			AssertEquals("IntendedUse", intendedUseText, entryLine.IntendedUse);
		}

		public void TestOriginRegion()
		{
			InvoiceHeader.JZ_InvoiceNumber = "TSW";
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1234567890";
			invoiceLine1.JI_Description = "DESCRIPTION";
			invoiceLine1.JI_OriginRegion = "Illawarra";

			CusEntryLine entryLine = EntryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;

			AssertEquals("OriginRegion", "Illawarra", entryLine.OriginRegion);
		}

		public void TestTreatmentProvider()
		{
			var treatmentProvider = Factory.NewWithValidTestData<OrgHeader>();
			InvoiceHeader.JZ_InvoiceNumber = "TSW";
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1234567890";
			invoiceLine1.JI_Description = "DESCRIPTION";
			invoiceLine1.JI_OH_TreatmentProvider = treatmentProvider.PK;

			CusEntryLine entryLine = EntryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;

			AssertEquals("TreatmentProvider", treatmentProvider, entryLine.TreatmentProvider);
		}

		public void TestExchangeRateIndicator()
		{
			InvoiceLine.InvoiceHeader.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.NZD;
			AssertEquals(ExchangeRateIndicatorList.Codes.NZD, EntryLine.ExchangeRateIndicator);
		}

		public void TestOSCustomsValue()
		{
			RefCurrency currencyNZD = RefCurrency.LoadFromCurrencyCode(Factory, "NZD");
			RefCurrency currencyAUD = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");

			ZDecimal baseValue = 1500m;
			EntryLine.CL_CustomsValue = baseValue;

			InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			AssertEquals(JobDeclaration.LocalCurrencyConstantCode, EntryLine.OSCurrencyCode);
			AssertEquals(baseValue, EntryLine.OSCustomsValue);

			CurrencyConverter converter = InvoiceLine.InvoiceHeader.CurrencyConverter;
			InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = currencyAUD.RX_Code;
			Money oSValue = converter.ConvertExact(new Money(baseValue, JobDeclaration.GetLocalCurrency()), currencyAUD);
			AssertEquals(oSValue.Amount, EntryLine.OSCustomsValue);
		}

		public void TestOSCustomsValueDropsDecimals()
		{
			RefCurrency currencyNZD = RefCurrency.LoadFromCurrencyCode(Factory, "NZD");
			RefCurrency currencyJPY = RefCurrency.LoadFromCurrencyCode(Factory, "JPY");

			ZDecimal baseValue = 1500m;
			EntryLine.CL_CustomsValue = baseValue;

			InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			AssertEquals(JobDeclaration.LocalCurrencyConstantCode, EntryLine.OSCurrencyCode);
			AssertEquals(baseValue, EntryLine.OSCustomsValue);

			CurrencyConverter converter = InvoiceLine.InvoiceHeader.CurrencyConverter;
			InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = currencyJPY.RX_Code;
			Money oSValue = converter.ConvertExact(new Money(baseValue, JobDeclaration.GetLocalCurrency()), currencyJPY);
			AssertEquals(ZArchitecture.Core.Utilities.Round(oSValue.Amount, 0), EntryLine.OSCustomsValue);
		}

		public void TestSupplier()
		{
			OrgHeader supplier = OrgHeader.New(Factory);
			InvoiceLine.InvoiceHeader.JZ_OH_Supplier = supplier.PK;
			AssertEquals(supplier, EntryLine.Supplier);
		}

		public void TestBuyer()
		{
			OrgHeader buyer = OrgHeader.New(Factory);
			InvoiceLine.InvoiceHeader.JZ_OH_Buyer = buyer.PK;
			AssertEquals(buyer, EntryLine.Buyer);
		}

		public void TestFees()
		{
			CusEntryLineFee fee = EntryLine.Fees.AddNew();
			AssertNotNull(fee);
		}

		public void TestConcessionCode()
		{
			InvoiceLine.JI_ConcessionCode = "123456Z";
			AssertEquals("123456Z", EntryLine.ConcessionCode);
		}

		public void TestPartsOfClassification()
		{
			InvoiceLine.JI_PartsOfClassification = "1234.56.24.56Z";
			AssertEquals("1234.56.24.56Z", EntryLine.PartsOfClassification);
		}

		public void TestIsDutyOnlyGSTOnInvoiceLine()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_DateOfArrival = new ZDateTime(2019, 08, 01);
			InvoiceLine.JI_CL = EntryLine.PK;

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			InvoiceHeader.JZ_IsGSTPrePaid = YesNoList.Codes.Yes;

			Assert("EntryLine.IsDutyOnlyGST", EntryLine.IsDutyOnlyGST);

			InvoiceHeader.JZ_IsGSTPrePaid = YesNoList.Codes.No;
			Assert("EntryLine.IsDutyOnlyGST", !EntryLine.IsDutyOnlyGST);

			InvoiceHeader.JZ_IsGSTPrePaid = YesNoList.Codes.Yes;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			Assert("EntryLine.IsDutyOnlyGST", !EntryLine.IsDutyOnlyGST);
		}

		public void TestIsZeroRatedFlagsOnInvoiceLine()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_CL = EntryLine.PK;

			InvoiceLine.JI_IsZeroRatedDuty = "Y";
			AssertEquals("EntryLine.IsZeroRatedDuty", true, EntryLine.IsZeroRatedDuty);
			AssertEquals("EntryLine.IsZeroRatedExcise", false, EntryLine.IsZeroRatedExcise);
			AssertEquals("EntryLine.IsZeroRatedLevies", false, EntryLine.IsZeroRatedLevies);
			AssertEquals("EntryLine.IsZeroRatedGST", false, EntryLine.IsZeroRatedGST);
			InvoiceLine.JI_IsZeroRatedExcise = "Y";
			AssertEquals("EntryLine.IsZeroRatedDuty", true, EntryLine.IsZeroRatedDuty);
			AssertEquals("EntryLine.IsZeroRatedExcise", true, EntryLine.IsZeroRatedExcise);
			AssertEquals("EntryLine.IsZeroRatedLevies", false, EntryLine.IsZeroRatedLevies);
			AssertEquals("EntryLine.IsZeroRatedGST", false, EntryLine.IsZeroRatedGST);
			InvoiceLine.JI_IsZeroRatedLevies = "Y";
			AssertEquals("EntryLine.IsZeroRatedDuty", true, EntryLine.IsZeroRatedDuty);
			AssertEquals("EntryLine.IsZeroRatedExcise", true, EntryLine.IsZeroRatedExcise);
			AssertEquals("EntryLine.IsZeroRatedLevies", true, EntryLine.IsZeroRatedLevies);
			AssertEquals("EntryLine.IsZeroRatedGST", false, EntryLine.IsZeroRatedGST);
			InvoiceLine.JI_IsZeroRatedGST = "Y";
			AssertEquals("EntryLine.IsZeroRatedDuty", true, EntryLine.IsZeroRatedDuty);
			AssertEquals("EntryLine.IsZeroRatedExcise", true, EntryLine.IsZeroRatedExcise);
			AssertEquals("EntryLine.IsZeroRatedLevies", true, EntryLine.IsZeroRatedLevies);
			AssertEquals("EntryLine.IsZeroRatedGST", true, EntryLine.IsZeroRatedGST);
		}

		public void TestIsZeroRatedFlagsOnInvoiceHeader()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_CL = EntryLine.PK;

			InvoiceHeader.JZ_IsZeroRatedDuty = "Y";
			AssertEquals("EntryLine.IsZeroRatedDuty", true, EntryLine.IsZeroRatedDuty);
			AssertEquals("EntryLine.IsZeroRatedExcise", false, EntryLine.IsZeroRatedExcise);
			AssertEquals("EntryLine.IsZeroRatedLevies", false, EntryLine.IsZeroRatedLevies);
			AssertEquals("EntryLine.IsZeroRatedGST", false, EntryLine.IsZeroRatedGST);
			InvoiceHeader.JZ_IsZeroRatedExcise = "Y";
			AssertEquals("EntryLine.IsZeroRatedDuty", true, EntryLine.IsZeroRatedDuty);
			AssertEquals("EntryLine.IsZeroRatedExcise", true, EntryLine.IsZeroRatedExcise);
			AssertEquals("EntryLine.IsZeroRatedLevies", false, EntryLine.IsZeroRatedLevies);
			AssertEquals("EntryLine.IsZeroRatedGST", false, EntryLine.IsZeroRatedGST);
			InvoiceHeader.JZ_IsZeroRatedLevies = "Y";
			AssertEquals("EntryLine.IsZeroRatedDuty", true, EntryLine.IsZeroRatedDuty);
			AssertEquals("EntryLine.IsZeroRatedExcise", true, EntryLine.IsZeroRatedExcise);
			AssertEquals("EntryLine.IsZeroRatedLevies", true, EntryLine.IsZeroRatedLevies);
			AssertEquals("EntryLine.IsZeroRatedGST", false, EntryLine.IsZeroRatedGST);
			InvoiceHeader.JZ_IsZeroRatedGST = "Y";
			AssertEquals("EntryLine.IsZeroRatedDuty", true, EntryLine.IsZeroRatedDuty);
			AssertEquals("EntryLine.IsZeroRatedExcise", true, EntryLine.IsZeroRatedExcise);
			AssertEquals("EntryLine.IsZeroRatedLevies", true, EntryLine.IsZeroRatedLevies);
			AssertEquals("EntryLine.IsZeroRatedGST", true, EntryLine.IsZeroRatedGST);
		}

		public void TestDescription()
		{
			InvoiceLine.JI_Description = "123456Z";
			AssertEquals("123456Z", EntryLine.Description);
		}

		public void TestStatisticalQty()
		{
			InvoiceLine.JI_CustomsQuantity = 123456m;
			AssertEquals(123456m, EntryLine.StatisticalQty);
		}

		public void TestStatisticalUnit()
		{
			InvoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals("KG", EntryLine.StatisticalUnit);
		}

		public void TestStatisticalQtyRoundedTo3Decimals()
		{
			InvoiceLine.JI_CustomsQuantity = 1.1111m;
			AssertEquals(1.111m, EntryLine.StatisticalQty);
			InvoiceLine.JI_CustomsQuantity = 1.6789m;
			AssertEquals(1.679m, EntryLine.StatisticalQty);
			InvoiceLine.JI_CustomsQuantity = 1.6798m;
			AssertEquals(1.68m, EntryLine.StatisticalQty);
		}

		public void TestSupplementaryQtyRoundedTo3Decimals()
		{
			InvoiceLine.JI_SupplementaryQty = 1.23m;
			AssertEquals(1.23m, EntryLine.SupplementaryQty);
			InvoiceLine.JI_SupplementaryQty = 0.5678m;
			AssertEquals(0.568m, EntryLine.SupplementaryQty);
			InvoiceLine.JI_SupplementaryQty = 3.899999m;
			AssertEquals(3.9m, EntryLine.SupplementaryQty);
		}

		public void TestVFDWholeNZD()
		{
			EntryLine.CL_CustomsValue = 1222.22m;
			AssertEquals(EntryLine.VFDWholeNZD, 1222.00m);

			EntryLine.CL_CustomsValue = 1222.50m;
			AssertEquals(EntryLine.VFDWholeNZD, 1223.00m);
		}

		public void TestFreightWholeNZDAndInsuranceWholeNZD()
		{
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			RefCurrency currencyNZD = RefCurrency.LoadFromCurrencyCode(Factory, "NZD");
			RefCurrency currencyAUD = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");

			ZDecimal baseValue = 1500m;
			EntryLine.CL_CustomsValue = baseValue;

			InvoiceLine.InvoiceHeader.JobComInvoiceLines.Add(InvoiceLine);
			InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = currencyAUD.RX_Code;
			InvoiceLine.InvoiceHeader.JZ_InvoiceAmount = 17999.00m;
			InvoiceLine.InvoiceHeader.JZ_IncoTerm = "CIF";

			InvoiceLine.JI_LinePrice = 16899.00m;

			InvoiceLine.InvoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000m, currencyAUD.RX_Code);
			InvoiceLine.InvoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, currencyAUD.RX_Code);
			InvoiceLine.Declaration.ResumeApportionment();

			AssertEquals("Freight Value", 1111.00m, EntryLine.FreightWholeNZD);
			AssertEquals("Insurance Value", 111.00m, EntryLine.InsuranceWholeNZD);
		}

		public void TestEntryHeader()
		{
			CusEntryLine myEntryLine = EntryLine;
			CusEntryHeader myEntryHeader = EntryHeader;
			AssertEquals("EntryHeader should match", myEntryHeader.PK, myEntryLine.EntryHeader.PK);
		}

		[TestDate(2006, 10, 1)]
		public void TestDutyLevyAndGSTAmount()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			DutyCalculatorTestObjects testObjects = new DutyCalculatorTestObjects(Factory);
			testObjects.Declaration.ResumeApportionment();
			CusEntryLine entryLine = testObjects.EntryLine;
			entryLine.CL_AdValoremTariff = "2204.21.18.19G";
			entryLine.RandomLine.JI_SupplementaryQty = 1500.00m;
			entryLine.RandomLine.JI_SupplementaryUQ = "LTR";
			entryLine.CalculateDutyAndGST();
			testObjects.Declaration.ResumeApportionment();

			AssertEquals("Precondition: ", "7.00% $2.1982/NMB ALAC:$0.0432/LTR", entryLine.DutyRateDescription);
			AssertEquals("entryLine.DutyAmount", 22122.00m, entryLine.DutyAmount);
			AssertEquals("entryLine.TotalLevies", 64.80m, entryLine.TotalLevies);
			AssertEquals("entryLine.GSTVATAmount", 3149.6m, entryLine.GSTVATAmount);
			AssertEquals("entryLine.TotalDutyLevyGST", 25336.4m, entryLine.TotalDutyLevyGST);
		}

		public void TestPublicNewConstructor()
		{
			CusEntryLine entryLine = Factory.New<CusEntryLine>();
			AssertEquals(typeof(CusEntryLine), entryLine.GetType());
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert(true);
			//TODO: remove this once entry lines are not deleted for merge required flag.
		}

		public override void TestEffectiveCustomsWeight()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var invoiceLine1 = entryLine.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsUnitQty = StatisticalUQList.Codes.Kilograms;
			invoiceLine1.JI_CustomsQuantity = 100m;
			var invoiceLine2 = entryLine.InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsUnitQty = StatisticalUQList.Codes.Kilograms;
			invoiceLine2.JI_CustomsQuantity = 200m;
			AssertEquals(new ZWeight(300m, "KG"), entryLine.EffectiveCustomsWeight);

			invoiceLine2.JI_CustomsUnitQty = StatisticalUQList.Codes.Grams;
			invoiceLine2.JI_CustomsQuantity = 2000m;
			AssertEquals(new ZWeight(102m, "KG"), entryLine.EffectiveCustomsWeight);

			invoiceLine2.JI_CustomsUnitQty = StatisticalUQList.Codes.Tonnes;
			invoiceLine2.JI_CustomsQuantity = 1m;
			AssertEquals(new ZWeight(1100m, "KG"), entryLine.EffectiveCustomsWeight);
		}

		public void TestMergingCustomsValue()
		{
			TestCaseHelper.ClearTable(RefExchangeRate.Schema.TableName);
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia);
			var audRate = audCurrency.ExchangeRates.AddNew();
			audRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			audRate.RE_StartDate = ZDateTime.Now.AddDays(-7);
			audRate.RE_ExpiryDate = ZDateTime.Now.AddDays(7);
			audRate.RE_SellRate = 0.9m;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_RX_NKInvoice_Currency = "AUD";

			for (var idx = 0; idx < 1000; idx++)
			{
				var invLine = invHeader.InvoiceLines.AddNew();
				invLine.JI_LinePrice = 1.11m;
			}

			var sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge(sender);

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("1.11AUD=1.2333...NZD≈1.23NZD, if round for each InvLine will cause 0.003*1000=3NZD loss, which should be avoided by sum->round", 1233m, entryLine.CL_CustomsValue);
		}

		public void TestRoundingExp()
		{
			TestCaseHelper.ClearTable(RefExchangeRate.Schema.TableName);
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia);
			var audRate = audCurrency.ExchangeRates.AddNew();
			audRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			audRate.RE_StartDate = ZDateTime.Now.AddDays(-7);
			audRate.RE_ExpiryDate = ZDateTime.Now.AddDays(7);
			audRate.RE_SellRate = 0.9201m;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_RX_NKInvoice_Currency = "AUD";

			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_LinePrice = 1.11m;

			var sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge(sender);

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("Should round for EXP Jobs.", 1.21m, entryLine.CL_CustomsValue);
		}

		protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection);

		#region Implementation

		protected JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = GetNewJobDeclaration();
					fDeclaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
					fDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		protected CusEntryLine EntryLine
		{
			get
			{
				if (fEntryLine == null)
				{
					fEntryLine = EntryHeader.MergedLines.AddNew();
				}
				return fEntryLine;
			}
		}
		CusEntryLine fEntryLine;

		protected CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					fEntryHeader = Declaration.CustomsEntryHeaders.AddNew();
				}
				return fEntryHeader;
			}
		}
		CusEntryHeader fEntryHeader;

		protected JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (fInvoiceLine == null)
				{
					fInvoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
					fInvoiceLine.JI_CL = EntryLine.PK;
				}
				return fInvoiceLine;
			}
		}
		JobComInvoiceLine fInvoiceLine;

		protected JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				if (fInvoiceHeader == null)
				{
					fInvoiceHeader = Declaration.Invoices.AddNew();
				}
				return fInvoiceHeader;
			}
		}
		JobComInvoiceHeader fInvoiceHeader;

		protected virtual JobDeclaration GetNewJobDeclaration()
		{
			return JobDeclaration.New(Factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return EntryLine;
		}
		#endregion
	}

	public class MergeChargeGettersTest : TestCaseWithFactory
	{
		public void TestALACLevyCreditAmount()
		{
			SetupInvoiceLineUserEnteredAmounts();

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Drawback);
			AssertEquals("EntryLine.ALACLevyCreditAmount", 1m, entryLine.ALACLevyCreditAmount);

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Normal);
			AssertEquals("EntryLine.ALACLevyCreditAmount", 0m, entryLine.ALACLevyCreditAmount);

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Completion);
			AssertEquals("EntryLine.ALACLevyCreditAmount", 1m, entryLine.ALACLevyCreditAmount);

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Completion);
			AssertEquals("EntryLine.ALACLevyCreditAmount", 0m, entryLine.ALACLevyCreditAmount);
		}

		public void TestAntiDumpingDutyAmount()
		{
			SetupInvoiceLineUserEnteredAmounts();

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Normal);
			AssertEquals("EntryLine.AntiDumpingDutyAmount", 2m, entryLine.AntiDumpingDutyAmount);

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Normal);
			AssertEquals("EntryLine.AntiDumpingDutyAmount", 0m, entryLine.AntiDumpingDutyAmount);
		}

		public void TestCountervailingDutyAmount()
		{
			SetupInvoiceLineUserEnteredAmounts();

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Normal);
			AssertEquals("EntryLine.CountervailingDutyAmount", 4m, entryLine.CountervailingDutyAmount);

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Normal);
			AssertEquals("EntryLine.CountervailingDutyAmount", 0m, entryLine.CountervailingDutyAmount);
		}

		public void TestDutyCreditAmount()
		{
			SetupInvoiceLineUserEnteredAmounts();

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Drawback);
			AssertEquals("EntryLine.DutyCreditAmount", 8m, entryLine.DutyCreditAmount);

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Normal);
			AssertEquals("EntryLine.DutyCreditAmount", 0m, entryLine.DutyCreditAmount);

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Completion);
			AssertEquals("EntryLine.DutyCreditAmount", 8m, entryLine.DutyCreditAmount);

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Completion);
			AssertEquals("EntryLine.DutyCreditAmount", 0m, entryLine.DutyCreditAmount);
		}

		public void TestGSTCreditAmount()
		{
			SetupInvoiceLineUserEnteredAmounts();

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Drawback);
			AssertEquals("EntryLine.GSTCreditAmount", 16m, entryLine.GSTCreditAmount);

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Normal);
			AssertEquals("EntryLine.GSTCreditAmount", 0m, entryLine.GSTCreditAmount);

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Completion);
			AssertEquals("EntryLine.GSTCreditAmount", 16m, entryLine.GSTCreditAmount);

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Completion);
			AssertEquals("EntryLine.GSTCreditAmount", 0m, entryLine.GSTCreditAmount);
		}

		public void TestDepositRefundAmount()
		{
			SetupInvoiceLineUserEnteredAmounts();

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Completion);
			AssertEquals("EntryLine.DepositRefundAmount", 32m, entryLine.DepositRefundAmount);

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Excise, JobMessageSubTypeList.Codes.Excise);
			AssertEquals("EntryLine.DepositRefundAmount", 0m, entryLine.DepositRefundAmount);

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Completion);
			AssertEquals("EntryLine.DepositRefundAmount", 32m, entryLine.DepositRefundAmount);

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Normal);
			AssertEquals("EntryLine.DepositRefundAmount", 0m, entryLine.DepositRefundAmount);
		}

		public void TestExciseDutyCreditAmount()
		{
			SetupInvoiceLineUserEnteredAmounts();

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Excise, JobMessageSubTypeList.Codes.Excise);
			AssertEquals("EntryLine.ExciseDutyCreditAmount", 64m, entryLine.ExciseDutyCreditAmount);

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Normal);
			AssertEquals("EntryLine.ExciseDutyCreditAmount", 0m, entryLine.ExciseDutyCreditAmount);

			GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Drawback);
			AssertEquals("EntryLine.ExciseDutyCreditAmount", 0m, entryLine.ExciseDutyCreditAmount);
		}

		#region Implementation
		void SetupInvoiceLineUserEnteredAmounts()
		{
			invoiceLine.JI_LevyCreditAmount = 1m;
			invoiceLine.JI_LevyCreditAmountCode = LevyCodesList.Codes.ALAC;
			invoiceLine.JI_AntiDumpingDutyAmount = 2m;
			invoiceLine.JI_CountervailingDutyAmount = 4m;
			invoiceLine.JI_DutyCreditAmount = 8m;
			invoiceLine.JI_GSTCreditAmount = 16m;
			invoiceLine.JI_DepositRefundAmount = 32m;
			invoiceLine.JI_ExciseDutyCreditAmount = 64m;
		}

		void GetNewEntryLineAndCopyInUserEnteredCustomsChargesAndCredits(ZString declarationMessageType, ZString declarationMessageSubType)
		{
			declaration.JE_MessageType = declarationMessageType;
			declaration.JE_MessageSubType = declarationMessageSubType;
			entryLine = declaration.CusEntryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryLine.CopyInUserEnteredCustomsChargesAndCredits();
		}

		CusEntryLine entryLine;
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		}
		#endregion
	}

	public class CusEntryLineExposedCustomsChargesTest : TestCaseWithFactory
	{
		#region Customs Charges Totalled from InvoiceLines
		public void TestALACLevyCreditAmount()
		{
			entryLine.ALACLevyCreditAmount = 12.00m;
			AssertEquals(12.00m, entryLine.ALACLevyCreditAmount);
			entryLine.Fees.RemoveAndDeleteAll();
			AssertEquals(0.00m, entryLine.ALACLevyCreditAmount);
		}

		public void TestAntiDumpingDutyAmount()
		{
			entryLine.AntiDumpingDutyAmount = 12.00m;
			AssertEquals(12.00m, entryLine.AntiDumpingDutyAmount);
			entryLine.Fees.RemoveAndDeleteAll();
			AssertEquals(0.00m, entryLine.AntiDumpingDutyAmount);
		}

		public void TestCountervailingDutyAmount()
		{
			entryLine.CountervailingDutyAmount = 12.00m;
			AssertEquals(12.00m, entryLine.CountervailingDutyAmount);
			entryLine.Fees.RemoveAndDeleteAll();
			AssertEquals(0.00m, entryLine.CountervailingDutyAmount);
		}

		public void TestDutyCreditAmount()
		{
			entryLine.DutyCreditAmount = 12.00m;
			AssertEquals(12.00m, entryLine.DutyCreditAmount);
			entryLine.Fees.RemoveAndDeleteAll();
			AssertEquals(0.00m, entryLine.DutyCreditAmount);
		}

		public void TestGSTCreditAmount()
		{
			entryLine.GSTCreditAmount = 12.00m;
			AssertEquals(12.00m, entryLine.GSTCreditAmount);
			entryLine.Fees.RemoveAndDeleteAll();
			AssertEquals(0.00m, entryLine.GSTCreditAmount);
		}

		public void TestDepositRefundAmount()
		{
			entryLine.DepositRefundAmount = 12.00m;
			AssertEquals(12.00m, entryLine.DepositRefundAmount);
			entryLine.Fees.RemoveAndDeleteAll();
			AssertEquals(0.00m, entryLine.DepositRefundAmount);
		}

		public void TestExciseDutyCreditAmount()
		{
			entryLine.ExciseDutyCreditAmount = 12.00m;
			AssertEquals(12.00m, entryLine.ExciseDutyCreditAmount);
			entryLine.Fees.RemoveAndDeleteAll();
			AssertEquals(0.00m, entryLine.ExciseDutyCreditAmount);
		}
		#endregion

		#region Customs Charges Calculated from InvoiceLines
		public void TestALACLevyAmount()
		{
			entryLine.ALACLevyAmount = 12.00m;
			AssertEquals(12.00m, entryLine.ALACLevyAmount);
			entryLine.Fees.RemoveAndDeleteAll();
			AssertEquals(0.00m, entryLine.ALACLevyAmount);
		}

		public void TestACCFuelLevyAmount()
		{
			entryLine.ACCFuelLevyAmount = 12.00m;
			AssertEquals(12.00m, entryLine.ACCFuelLevyAmount);
			entryLine.Fees.RemoveAndDeleteAll();
			AssertEquals(0.00m, entryLine.ACCFuelLevyAmount);
		}

		public void TestPFMLFuelLevyAmount()
		{
			entryLine.PFMLFuelLevyAmount = 15.00m;
			AssertEquals(15.00m, entryLine.PFMLFuelLevyAmount);
			entryLine.Fees.RemoveAndDeleteAll();
			AssertEquals(0.00m, entryLine.PFMLFuelLevyAmount);
		}

		public void TestSyntheticGreenhouseGasesLevyAmount()
		{
			entryLine.SyntheticGreenhouseGasesLevyAmount = 16.00m;
			AssertEquals(16.00m, entryLine.SyntheticGreenhouseGasesLevyAmount);
			entryLine.Fees.RemoveAndDeleteAll();
			AssertEquals(0.00m, entryLine.SyntheticGreenhouseGasesLevyAmount);
		}

		public void TestHERALevyAmount()
		{
			entryLine.HERALevyAmount = 12.00m;
			AssertEquals(12.00m, entryLine.HERALevyAmount);
			entryLine.Fees.RemoveAndDeleteAll();
			AssertEquals(0.00m, entryLine.HERALevyAmount);
		}

		public void TestDutyAmount()
		{
			entryLine.DutyAmount = 12.00m;
			AssertEquals(12.00m, entryLine.DutyAmount);
			entryLine.Fees.RemoveAndDeleteAll();
			AssertEquals(0.00m, entryLine.DutyAmount);
		}

		public void TestGSTAmount()
		{
			entryLine.GSTAmount = 12.00m;
			AssertEquals(12.00m, entryLine.GSTAmount);
			entryLine.Fees.RemoveAndDeleteAll();
			AssertEquals(0.00m, entryLine.GSTAmount);
		}
		#endregion

		#region Totals of Customs Charges
		public void TestTotalLevies()
		{
			SetupForTotalsTesting();
			AssertEquals("TotalLevies", 13185m, entryLine.TotalLevies);
		}

		public void TestTotalMisc()
		{
			SetupForTotalsTesting();
			AssertEquals("TotalMisc", 13191m, entryLine.TotalMisc);
		}

		public void TestTotalDutiesAndLevies()
		{
			SetupForTotalsTesting();
			AssertEquals("TotalDutiesAndLevies", 14287m, entryLine.TotalDutiesAndLevies);
		}

		public void TestTotalGST()
		{
			SetupForTotalsTesting();
			AssertEquals("TotalGST", 2064m, entryLine.TotalGST);
		}

		public void TestTotalAmountPayable()
		{
			SetupForTotalsTesting();
			AssertEquals("TotalAmountPayable", 16383m, entryLine.TotalAmountPayable);
		}

		void SetupForTotalsTesting()
		{
			entryLine.ALACLevyCreditAmount = 1m;
			entryLine.AntiDumpingDutyAmount = 2m;
			entryLine.CountervailingDutyAmount = 4m;
			entryLine.DutyCreditAmount = 8m;
			entryLine.GSTCreditAmount = 16m;
			entryLine.DepositRefundAmount = 32m;
			entryLine.ExciseDutyCreditAmount = 64m;

			entryLine.ALACLevyAmount = 128m;
			entryLine.ACCFuelLevyAmount = 256m;
			entryLine.PFMLFuelLevyAmount = 4096m;
			entryLine.SyntheticGreenhouseGasesLevyAmount = 8192m;
			entryLine.HERALevyAmount = 512m;
			entryLine.DutyAmount = 1024m;
			entryLine.GSTAmount = 2048m;
		}

		#endregion

		public void TestEntryFeeAmount()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_CustomsValue = 1000m;
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_CustomsValue = 4000m;

			entryHeader.EntryFeeAmount = 100m;
			entryHeader.EntryFeeGST = 10m;

			AssertEquals("EntryLine1's EntryFeeAmount", 20m, entryLine1.EntryFeeAmount);
			AssertEquals("EntryLine2's EntryFeeAmount", 80m, entryLine2.EntryFeeAmount);
		}

		#region Implementation
		CusEntryLine entryLine;
		protected override void SetUp()
		{
			base.SetUp();
			entryLine = Factory.New<CusEntryLine>();
		}
		#endregion
	}
}
