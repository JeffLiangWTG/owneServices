using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.EntityFramework.Testing;
	using EDITariff_ReferenceFiles_NZ;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
	using Enterprise.MasterFiles.Business;
	using NUnit.Framework;

	public class CusEntryLineWrapperTest : TestCaseWithFactory
	{
		public void TestIsGSTPrePaidAndGSTNumber()
		{
			CreateImportSeaJob();
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_FullName = "Supplier Co-Op Pty Ltd";

			var invoice1 = JobDeclaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 100m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_IncoTerm = "FOB";
			invoice1.JZ_OH_Supplier = supplier1.PK;
			invoice1.JZ_SupplierGSTNumber = "TEST1233";
			invoice1.JZ_IsGSTPrePaid = "Y";

			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 100m;
			line1.JI_Tariff = "2204.21.18.11A";
			line1.JI_CustomsQuantity = 1000m;
			line1.JI_Description = "Invoice 1 / Line 1";

			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryLine1 = JobDeclaration.CustomsEntryHeaders[0].MergedLines[0];
			var wrappedEntryLine = new CusEntryLineWrapper(entryLine1);
			JobDeclaration.JE_DateOfArrival = new ZDateTime(2019, 08, 01);
			AssertEquals("Y", wrappedEntryLine.IsGSTPrePaid);
			AssertEquals("TEST1233", wrappedEntryLine.VendorIdentifier);
		}

		public void TestVendorIdStripsIllegalChars()
		{
			CreateImportSeaJob();
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_FullName = "Supplier Co-Op Pty Ltd";

			var invoice1 = JobDeclaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 100m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_IncoTerm = "FOB";
			invoice1.JZ_OH_Supplier = supplier1.PK;
			invoice1.JZ_SupplierGSTNumber = "141-135-236";
			invoice1.JZ_IsGSTPrePaid = "Y";

			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 100m;
			line1.JI_Tariff = "2204.21.18.11A";
			line1.JI_CustomsQuantity = 1000m;
			line1.JI_Description = "Invoice 1 / Line 1";

			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryLine1 = JobDeclaration.CustomsEntryHeaders[0].MergedLines[0];
			var wrappedEntryLine = new CusEntryLineWrapper(entryLine1);
			JobDeclaration.JE_DateOfArrival = new ZDateTime(2019, 08, 01);
			AssertEquals("Y", wrappedEntryLine.IsGSTPrePaid);
			AssertEquals("141135236", wrappedEntryLine.VendorIdentifier);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new CusEntryLineWrapper(null);
		}

		public void TestCusEntryLineWrapper()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_DateOfArrival = new ZDateTime(2006, 1, 1);
			JobDeclaration.JE_ATFOtherInfoValue = "9JRCZ";

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 20000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = "FOB";

			var grower = Factory.NewWithValidTestData<OrgHeader>();
			grower.OH_FullName = "Grower Co-Op Pty Ltd";
			var producer = Factory.NewWithValidTestData<OrgHeader>();
			producer.OH_FullName = "PINK BATTS P/L";
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_FullName = "Heavy Manufacturing Inc.";

			var useByDate = ZDateTime.Today.AddMonths(12);

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 15000m;
			line1.JI_Tariff = "2204.21.18.11A";
			line1.JI_CustomsQuantity = 1000m;
			line1.JI_CustomsUnitQty = "LTR";
			line1.JI_QualifiesForPreferentialDuty = "N";
			line1.JI_CountryOfOrigin = "US";
			line1.JI_RN_NKCountryOfExport = "US";
			line1.JI_LotNumber = "15750";
			line1.JI_DateMarking = useByDate;
			line1.JI_IntendedUseCode = IntendedUseCodeList.Codes.PU;
			line1.GrowerOrgPK = grower.PK;
			line1.ManufacturerOrgPK = manufacturer.PK;
			line1.ProducerOrgPK = producer.PK;
			var packaging1 = line1.ItemPackages.AddNew();
			packaging1.NZ_NumberOfPackages = 5;
			packaging1.NZ_PackageUQ = "BG";
			packaging1.NZ_PackageVolume = 3m;
			packaging1.NZ_ShippingMarks = "U839238/G6";

			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 5000m;
			line2.JI_Tariff = "2204.21.18.11A";
			line2.JI_CustomsQuantity = 10m;
			line2.JI_CustomsUnitQty = "LTR";
			line2.JI_QualifiesForPreferentialDuty = "N";
			line2.JI_CountryOfOrigin = "US";
			line2.JI_RN_NKCountryOfExport = "US";
			line2.JI_LotNumber = "15750";
			line2.JI_DateMarking = useByDate;
			line2.JI_IntendedUseCode = IntendedUseCodeList.Codes.PU;
			line2.GrowerOrgPK = grower.PK;
			line2.ManufacturerOrgPK = manufacturer.PK;
			line2.ProducerOrgPK = producer.PK;
			var packaging2 = line2.ItemPackages.AddNew();
			packaging2.NZ_NumberOfPackages = 3;
			packaging2.NZ_PackageUQ = "DR";
			packaging2.NZ_PackageVolume = 1.2m;

			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryLine = JobDeclaration.CustomsEntryHeaders[0].MergedLines[0];

			var wrappedEntryLine = new CusEntryLineWrapper(entryLine);
			AssertEquals("ValueForDutyInNZD", 20000m, wrappedEntryLine.ValueForDutyInNZD);
			AssertEquals("TransitionalFacilityCode", "9JRCZ", wrappedEntryLine.TransitionalFacilityCode);
			AssertEquals("GoodsDescription", "OTHER RED WINES IN CONTAINERS 750ML OR LESS", wrappedEntryLine.GoodsDescription);
			AssertEquals("LotNumber", "15750", wrappedEntryLine.LotNumber);
			AssertEquals("DateMarking", useByDate, wrappedEntryLine.DateMarking);
			AssertEquals("HasForeignCurrency", false, wrappedEntryLine.HasForeignCurrency);
			AssertEquals("IntendedUseCode", IntendedUseCodeList.Codes.PU, wrappedEntryLine.IntendedUseCode);
			AssertEquals("IntendedUse", ZString.Empty, wrappedEntryLine.IntendedUse);
			var classifications = ZString.Empty;
			var classificationTypes = ZString.Empty;
			foreach (IClassification classification in wrappedEntryLine.Classifications)
			{
				classifications += classification.Classification;
				classificationTypes += classification.ClassificationTypeCode;
			}
			AssertEquals("Classifications - codes", "2204211811A", classifications);
			AssertEquals("Classifications - types", "HS", classificationTypes);
			AssertEquals("PreferenceClaimed", "NML", wrappedEntryLine.PreferenceClaimed);
			AssertEquals("Grower", "Grower Co-Op Pty Ltd", wrappedEntryLine.Grower.Name);
			AssertEquals("Producer", "PINK BATTS P/L", wrappedEntryLine.Producer.Name);
			AssertEquals("Manufacturer", "Heavy Manufacturing Inc.", wrappedEntryLine.Manufacturer.Name);
			AssertEquals("ExportCountry", "US", wrappedEntryLine.ExportCountry);
			AssertEquals("OriginCountry", "US", wrappedEntryLine.OriginCountry);
			AssertEquals("StatisticalQty", 1010m, wrappedEntryLine.StatisticalQty);
			AssertEquals("StatisticalQty", "LTR", wrappedEntryLine.StatisticalQtyUnit);
			AssertNotNull("Adjustments", wrappedEntryLine.Adjustments);
			AssertEquals("BrandName", "", wrappedEntryLine.BrandName);
			AssertNotNull("Classifications", wrappedEntryLine.Classifications);
			AssertEquals("CommonName", "", wrappedEntryLine.CommonName);
			AssertNotNull("Constituents", wrappedEntryLine.Constituents);
			AssertEquals("DateMarking", ZDate.Today.AddYears(1), wrappedEntryLine.DateMarking);
			AssertEquals("ExportCountry", "US", wrappedEntryLine.ExportCountry);
			AssertEquals("ForeignCurrencyCode", "", wrappedEntryLine.ForeignCurrencyCode);
			AssertEquals("GeneticallyModified", false, wrappedEntryLine.GeneticallyModified);
			AssertEquals("GoodsDescription", "OTHER RED WINES IN CONTAINERS 750ML OR LESS", wrappedEntryLine.GoodsDescription);
			AssertNotNull("Grower", wrappedEntryLine.Grower);
			AssertEquals("HasForeignCurrency", false, wrappedEntryLine.HasForeignCurrency);
			AssertEquals("IntendedUse", "", wrappedEntryLine.IntendedUse);
			AssertEquals("ItemGrossWeightInKGM", 0m, wrappedEntryLine.ItemGrossWeightInKGM);
			AssertEquals("ItemNetWeightInKGM", 0m, wrappedEntryLine.ItemNetWeightInKGM);
			AssertNotNull("LineDutyTaxFees", wrappedEntryLine.LineDutyTaxFees);
			AssertEquals("LotNumber", "15750", wrappedEntryLine.LotNumber);
			AssertNotNull("Manufacturer", wrappedEntryLine.Manufacturer);
			AssertEquals("OriginCountry", "US", wrappedEntryLine.OriginCountry);
			AssertEquals("OriginRegion", "", wrappedEntryLine.OriginRegion);
			AssertNotNull("Packaging", wrappedEntryLine.Packaging);
			foreach (IPackaging packaging in wrappedEntryLine.Packaging)
			{
				AssertEquals("NumberOfPackages", true, packaging.NumberOfPackages > 0);
				AssertEquals("PackageType", false, packaging.PackageType.IsEmpty);
				AssertEquals("ShippingMarks", false, packaging.ShippingMarks.IsEmpty);
				AssertEquals("PackageVolumeInMTQ", true, packaging.PackageVolumeInMTQ > 0);
			}

			AssertNotNull("Permits", wrappedEntryLine.Permits);
			AssertEquals("PreferenceClaimed", "NML", wrappedEntryLine.PreferenceClaimed);
			AssertNotNull("Producer", wrappedEntryLine.Producer);
			AssertNotNull("Products", wrappedEntryLine.Products);
			AssertNotNull("ProhibitedCodes", wrappedEntryLine.ProhibitedCodes);
			AssertEquals("RegisteredName", "", wrappedEntryLine.RegisteredName);
			AssertEquals("RelationshipIndicator", "135", wrappedEntryLine.RelationshipIndicator);
			AssertNotNull("RoutingCountryCodes", wrappedEntryLine.RoutingCountryCodes);
			AssertEquals("StatisticalQty", 1010m, wrappedEntryLine.StatisticalQty);
			AssertEquals("StatisticalQtyUnit", "LTR", wrappedEntryLine.StatisticalQtyUnit);
			AssertEquals("SupplementaryQty", 0m, wrappedEntryLine.SupplementaryQty);
			AssertEquals("SupplementaryQtyUnit", "", wrappedEntryLine.SupplementaryQtyUnit);
			AssertEquals("SupplierLineIsRelatedTo", 1, wrappedEntryLine.SupplierLineIsRelatedTo);
			AssertNull("Temperatures", wrappedEntryLine.Temperatures);
			AssertEquals("TradeName", "", wrappedEntryLine.TradeName);
			AssertEquals("TransitionalFacilityCode", "9JRCZ", wrappedEntryLine.TransitionalFacilityCode);
			AssertNull("TreatmentProvider", wrappedEntryLine.TreatmentProvider);
			AssertEquals("UsedGoods", false, wrappedEntryLine.UsedGoods);
			AssertEquals("ValueForDutyInNZD", 20000m, wrappedEntryLine.ValueForDutyInNZD);
			AssertEquals("ValueInForeignCurrency", 0m, wrappedEntryLine.ValueInForeignCurrency);
		}

		public void TestSupplierLineIsRelatedTo()
		{
			CreateImportSeaJob();

			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_FullName = "Supplier Co-Op Pty Ltd";

			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			supplier2.OH_FullName = "Jones Supplier Pty Ltd";

			var invoice1 = JobDeclaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 100m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_IncoTerm = "FOB";
			invoice1.JZ_OH_Supplier = supplier1.PK;

			var invoice2 = JobDeclaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 200m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.JZ_IncoTerm = "FOB";
			invoice2.JZ_OH_Supplier = supplier2.PK;

			var invoice3 = JobDeclaration.Invoices.AddNew();
			invoice3.JZ_InvoiceAmount = 300m;
			invoice3.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice3.JZ_IncoTerm = "FOB";
			invoice3.JZ_OH_Supplier = supplier1.PK;

			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 100m;
			line1.JI_Tariff = "2204.21.18.11A";
			line1.JI_CustomsQuantity = 1000m;
			line1.JI_Description = "Invoice 1 / Line 1";

			var line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 200m;
			line2.JI_Tariff = "4201.00.00.01B";
			line2.JI_CustomsQuantity = 10m;
			line2.JI_Description = "Invoice 2 / Line 1";

			var line3 = invoice3.JobComInvoiceLines.AddNew();
			line3.JI_LinePrice = 300m;
			line3.JI_Tariff = "0203.11.00.02C";
			line3.JI_CustomsQuantity = 10m;
			line3.JI_Description = "Invoice 3 / Line 1";

			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Should be 3 entry lines created", 3, JobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);

			var entryLine1 = JobDeclaration.CustomsEntryHeaders[0].MergedLines[0];
			var wrappedEntryLine1 = new CusEntryLineWrapper(entryLine1);
			var entryLine2 = JobDeclaration.CustomsEntryHeaders[0].MergedLines[1];
			var wrappedEntryLine2 = new CusEntryLineWrapper(entryLine2);
			var entryLine3 = JobDeclaration.CustomsEntryHeaders[0].MergedLines[2];
			var wrappedEntryLine3 = new CusEntryLineWrapper(entryLine3);

			if (wrappedEntryLine1.GoodsDescription == "Invoice 1 / Line 1" || wrappedEntryLine1.GoodsDescription == "Invoice 3 / Line 1")
			{
				AssertEquals("SupplierLineIsRelatedTo - line 1 should be Supplier 1", 1, wrappedEntryLine1.SupplierLineIsRelatedTo);
				if (wrappedEntryLine1.GoodsDescription == "Invoice 2 / Line 1")
				{
					AssertEquals("SupplierLineIsRelatedTo - line 2 should be Supplier 2", 2, wrappedEntryLine2.SupplierLineIsRelatedTo);
					AssertEquals("SupplierLineIsRelatedTo - line 3 should be Supplier 1 also", 1, wrappedEntryLine3.SupplierLineIsRelatedTo);
				}
				else
				{
					AssertEquals("SupplierLineIsRelatedTo - line 2 should be Supplier 1 also", 1, wrappedEntryLine2.SupplierLineIsRelatedTo);
					AssertEquals("SupplierLineIsRelatedTo - line 3 should be Supplier 2", 2, wrappedEntryLine3.SupplierLineIsRelatedTo);
				}
			}
			else if (wrappedEntryLine1.GoodsDescription == "Invoice 2 / Line 1")
			{
				AssertEquals("SupplierLineIsRelatedTo - line 1 should be Supplier 2", 1, wrappedEntryLine1.SupplierLineIsRelatedTo);
				AssertEquals("SupplierLineIsRelatedTo - line 2 should be Supplier 1", 1, wrappedEntryLine2.SupplierLineIsRelatedTo);
				AssertEquals("SupplierLineIsRelatedTo - line 3 should be Supplier 1 also", 2, wrappedEntryLine3.SupplierLineIsRelatedTo);
			}
		}

		public void TestContainersLineIsRelatedToFromMergedLines()
		{
			CreateImportSeaJob();
			var container1 = JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "HLMU0393842";
			var container2 = JobDeclaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "MRKU0239488";

			var invoice1 = JobDeclaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 200m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_IncoTerm = "FOB";

			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 100m;
			line1.JI_Tariff = "2204.21.18.11A";
			line1.JI_CustomsQuantity = 10m;
			line1.JI_Description = "Invoice 1 / Line 1";

			var line2 = invoice1.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 100m;
			line2.JI_Tariff = "2204.21.18.11A";
			line2.JI_CustomsQuantity = 10m;
			line2.JI_Description = "Invoice 1 / Line 2";

			line1.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			line2.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;

			JobDeclaration.JE_MergeBy = "TRF";
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Should only be 1 merged entry line", 1, JobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);

			var entryLine = JobDeclaration.CustomsEntryHeaders[0].MergedLines[0];
			var wrappedEntryLine = new CusEntryLineWrapper(entryLine);
			var containerCount = 0;
			var containerNumber1 = "";
			var containerNumber2 = "";
			foreach (ZString containerNo in wrappedEntryLine.ContainerNumbers)
			{
				containerCount++;
				if (containerCount == 1)
				{
					containerNumber1 = containerNo;
				}
				else
				{
					containerNumber2 = containerNo;
				}
			}

			AssertEquals("Should be two containers related to this line", 2, containerCount);
			AssertEquals("Container related to this line", "HLMU0393842", containerNumber1);
			AssertEquals("Container related to this line", "MRKU0239488", containerNumber2);
		}

		public void TestAdjustments()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_DateOfArrival = new ZDateTime(2013, 12, 11);
			JobDeclaration.JE_ATFOtherInfoValue = "9JRCZ";

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 20000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = "FOB";
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 385m, "NZD");
			invoice.Charges.AddNew(Enterprise.Customs.NZ.Business.CustomsChargeTypeList.TSWCodes.Royalties, 250m, "NZD");

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 15000m;
			line1.JI_Tariff = "2204.21.18.11A";
			line1.JI_CustomsQuantity = 1000m;
			line1.JI_CustomsUnitQty = "LTR";
			line1.JI_QualifiesForPreferentialDuty = "N";
			line1.JI_CountryOfOrigin = "US";
			line1.JI_RN_NKCountryOfExport = "US";

			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 5000m;
			line2.JI_Tariff = "2204.21.18.11A";
			line2.JI_CustomsQuantity = 10m;
			line2.JI_CustomsUnitQty = "LTR";
			line2.JI_QualifiesForPreferentialDuty = "N";
			line2.JI_CountryOfOrigin = "US";
			line2.JI_RN_NKCountryOfExport = "US";

			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryLine = JobDeclaration.CustomsEntryHeaders[0].MergedLines[0];
			var wrappedEntryLine = new CusEntryLineWrapper(entryLine);

			AssertNotNull("Adjustments", wrappedEntryLine.Adjustments);
			var freightAdjustment = ZDecimal.Zero;
			var insuranceAdjustment = ZDecimal.Zero;
			var royaltyAdjustment = ZDecimal.Zero;
			foreach (IValuationAdjustment adjustment in wrappedEntryLine.Adjustments)
			{
				if (adjustment.AdjustmentQualifier == ValuationAdjustmentTypeList.Codes.V151)
				{
					freightAdjustment = adjustment.AdjustmentAmountInNZD;
				}

				if (adjustment.AdjustmentQualifier == ValuationAdjustmentTypeList.Codes.V150)
				{
					insuranceAdjustment = adjustment.AdjustmentAmountInNZD;
				}

				if (adjustment.AdjustmentQualifier == ValuationAdjustmentTypeList.Codes.V146)
				{
					royaltyAdjustment = adjustment.AdjustmentAmountInNZD;
				}
			}

			AssertEquals("Freight Charge Adjustment", 385m, freightAdjustment);
			AssertEquals("Insurance Charge Adjustment - mandatory to send, should be zero here as no insurance charge entered", 0m, insuranceAdjustment);
			AssertEquals("Royalties Charge Adjustment", 250m, royaltyAdjustment);
		}

		public void TestProhibitedCodes()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ExportDate = new ZDateTime(2015, 12, 11);

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 20000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = "FOB";
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 385m, "NZD");

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 15000m;
			line1.JI_Tariff = "2204.21.18.11A";
			line1.JI_CustomsQuantity = 1000m;
			line1.JI_CustomsUnitQty = "LTR";
			line1.JI_QualifiesForPreferentialDuty = "N";
			line1.JI_CountryOfOrigin = "US";
			line1.JI_RN_NKCountryOfExport = "US";
			line1.ProhibitedCodes.AddNew(ProhibitedCodeList.Codes.Samples, "");

			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryLine = JobDeclaration.CustomsEntryHeaders[0].MergedLines[0];
			var wrappedEntryLine = new CusEntryLineWrapper(entryLine);
			AssertNotNull("ProhibitedCodes", wrappedEntryLine.ProhibitedCodes);
			foreach (ZString prohibitedCode in wrappedEntryLine.ProhibitedCodes)
			{
				AssertEquals("prohibitedCode", "SAM", prohibitedCode);
			}

			AssertEquals("IsPartsRelated", false, wrappedEntryLine.IsPartsRelated);
		}

		public void TestPermitCodes()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ExportDate = new ZDateTime(2015, 12, 11);

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 20000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = "FOB";
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 385m, "NZD");

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 15000m;
			line1.JI_Tariff = "2204.21.18.11A";
			line1.JI_CustomsQuantity = 1000m;
			line1.JI_CustomsUnitQty = "LTR";
			line1.JI_QualifiesForPreferentialDuty = "N";
			line1.JI_CountryOfOrigin = "US";
			line1.JI_RN_NKCountryOfExport = "US";
			line1.PermitCodes.AddNew(PermitCodeList.Codes.NZFoodSafetyAuthorityNew, "123456");

			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryLine = JobDeclaration.CustomsEntryHeaders[0].MergedLines[0];
			var wrappedEntryLine = new CusEntryLineWrapper(entryLine);
			AssertNotNull("Permits", wrappedEntryLine.Permits);
			foreach (ZString permit in wrappedEntryLine.Permits)
			{
				AssertEquals("permit", "FSA,123456", permit);
			}

			AssertEquals("IsPartsRelated", false, wrappedEntryLine.IsPartsRelated);
		}

		public void TestRoutingCountryCodes()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfLoading = "CNSHA";

			var leg1 = JobDeclaration.Transports.AddNew();
			leg1.JW_VoyageFlight = "QF2";
			leg1.JW_RL_NKLoadPort = "CNSHA";
			leg1.JW_RL_NKDiscPort = "SGSIN";
			leg1.JW_ETD = new ZDateTime(2018, 01, 12);

			var leg2 = JobDeclaration.Transports.AddNew();
			leg2.JW_VoyageFlight = "QF117";
			leg2.JW_RL_NKLoadPort = "SGSIN";
			leg2.JW_RL_NKDiscPort = "AUSYD";
			leg2.JW_ETD = new ZDateTime(2018, 01, 13);

			var leg3 = JobDeclaration.Transports.AddNew();
			leg3.JW_VoyageFlight = "QF108";
			leg3.JW_RL_NKLoadPort = "AUSYD";
			leg3.JW_RL_NKDiscPort = "NZAKL";
			leg3.JW_ETD = new ZDateTime(2018, 01, 14);
			AssertEquals("Pre-condition: Declaration has Three transport legs", 3, JobDeclaration.Transports.Count);

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = "FOB";
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 385m, "NZD");

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 15000m;
			line1.JI_Tariff = "2204.21.18.11A";
			line1.JI_CustomsQuantity = 1000m;
			line1.JI_CustomsUnitQty = "LTR";
			line1.JI_QualifiesForPreferentialDuty = "N";
			line1.JI_CountryOfOrigin = "CN";
			line1.JI_RN_NKCountryOfExport = "CN";

			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryLine = JobDeclaration.CustomsEntryHeaders[0].MergedLines[0];
			var wrappedEntryLine = new CusEntryLineWrapper(entryLine);
			AssertNotNull("RoutingCountryCodes", wrappedEntryLine.RoutingCountryCodes);
			foreach (ZString routingCountry in wrappedEntryLine.RoutingCountryCodes)
			{
				AssertEquals("Routing Country/Region Codes are AU and SG", true, (routingCountry == "SG" || routingCountry == "AU"));
			}
		}

		public void TestItinerarySendsOnlyCurrentRoutingLegs()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfLoading = "CNSHA";

			var leg1 = JobDeclaration.Transports.AddNew();
			leg1.JW_VoyageFlight = "QF2";
			leg1.JW_RL_NKLoadPort = "CNSHA";
			leg1.JW_RL_NKDiscPort = "SGSIN";
			leg1.JW_ETD = new ZDateTime(2018, 01, 12);

			var leg2 = JobDeclaration.Transports.AddNew();
			leg2.JW_VoyageFlight = "QF117";
			leg2.JW_RL_NKLoadPort = "SGSIN";
			leg2.JW_RL_NKDiscPort = "AUSYD";
			leg2.JW_ETD = new ZDateTime(2018, 01, 13);

			var leg3 = JobDeclaration.Transports.AddNew();
			leg3.JW_VoyageFlight = "QF108";
			leg3.JW_RL_NKLoadPort = "AUSYD";
			leg3.JW_RL_NKDiscPort = "NZAKL";
			leg3.JW_ETD = new ZDateTime(2018, 01, 14);
			AssertEquals("Pre-condition: Declaration has Three transport legs", 3, JobDeclaration.Transports.Count);

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = "FOB";
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 385m, "NZD");

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 15000m;
			line1.JI_Tariff = "2204.21.18.11A";
			line1.JI_CustomsQuantity = 1000m;
			line1.JI_CustomsUnitQty = "LTR";
			line1.JI_QualifiesForPreferentialDuty = "N";
			line1.JI_CountryOfOrigin = "CN";
			line1.JI_RN_NKCountryOfExport = "CN";

			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryLine = JobDeclaration.CustomsEntryHeaders[0].MergedLines[0];
			var wrappedEntryLine = new CusEntryLineWrapper(entryLine);
			AssertNotNull("RoutingCountryCodes", wrappedEntryLine.RoutingCountryCodes);
			AssertEquals("wrappedEntryLine.RoutingCountryCodes count should be 2 itinerary codes", true, wrappedEntryLine.RoutingCountryCodes.IsCountEqualTo(2));
			bool aUCodeFound = false;
			bool sGCodeFound = false;
			foreach (ZString routingCountry in wrappedEntryLine.RoutingCountryCodes)
			{
				AssertEquals("Routing Country/Region Codes are AU and SG", true, (routingCountry == "SG" || routingCountry == "AU"));
				if (routingCountry == "AU")
				{
					aUCodeFound = true;
				}
				else if (routingCountry == "SG")
				{
					sGCodeFound = true;
				}
			}

			Assert(aUCodeFound);
			Assert(sGCodeFound);

			aUCodeFound = false;
			sGCodeFound = false;
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entryLine = JobDeclaration.CustomsEntryHeaders[0].MergedLines[0];
			var wrappedEntryLineSecondTime = new CusEntryLineWrapper(entryLine);
			AssertNotNull("RoutingCountryCodes", wrappedEntryLineSecondTime.RoutingCountryCodes);
			AssertEquals("wrappedEntryLine.RoutingCountryCodes count should still be 2 itinerary codes", true, wrappedEntryLineSecondTime.RoutingCountryCodes.IsCountEqualTo(2));
			foreach (ZString routingCountry in wrappedEntryLineSecondTime.RoutingCountryCodes)
			{
				AssertEquals("Routing Country/Region Codes are AU and SG with no duplicates", true, (routingCountry == "SG" || routingCountry == "AU"));
				if (routingCountry == "AU")
				{
					aUCodeFound = true;
				}
				else if (routingCountry == "SG")
				{
					sGCodeFound = true;
				}
			}

			Assert(aUCodeFound);
			Assert(sGCodeFound);
		}

		public void TestPreferenceClaimed()
		{
			NZCustomsDataRegistry.Instance.PreferentialCountryGroupCodeDefaulting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_ATFOtherInfoValue = "9JRCZ";

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 20000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_DefaultQualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 385m, "NZD");
			invoice.Charges.AddNew(Enterprise.Customs.NZ.Business.CustomsChargeTypeList.TSWCodes.Royalties, 250m, "NZD");

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 15000m;
			line1.JI_Tariff = "8413.81.19.00J";
			line1.JI_CustomsQuantity = 1000m;
			line1.JI_CustomsUnitQty = "LTR";
			line1.JI_QualifiesForPreferentialDuty = "Q";
			line1.JI_CountryOfOrigin = "AU";
			line1.JI_RN_NKCountryOfExport = "AU";
			AssertEquals("PreferenceClaimed for line 1 should default from best rate", "8413.81.19.00J @ AU = FREE", line1.JI_DutyRateComplete);

			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 5000m;
			line2.JI_Tariff = "2204.21.18.11A";
			line2.JI_CustomsQuantity = 10m;
			line2.JI_CustomsUnitQty = "LTR";
			line2.JI_QualifiesForPreferentialDuty = "N";
			line2.JI_CountryOfOrigin = "AU";
			line2.JI_RN_NKCountryOfExport = "AU";
			AssertEquals("No PreferenceClaimed for line 2 - should default to Normal rate", "2204.21.18.11A @ NML = ALAC:$0.0418/LTR", line2.JI_DutyRateComplete);

			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryLine1 = JobDeclaration.CustomsEntryHeaders[0].MergedLines[0];
			var wrappedEntryLine1 = new CusEntryLineWrapper(entryLine1);
			AssertEquals("PreferenceClaimed for line 1 should default from best rate", "AU", wrappedEntryLine1.PreferenceClaimed);

			var entryLine2 = JobDeclaration.CustomsEntryHeaders[0].MergedLines[1];
			var wrappedEntryLine2 = new CusEntryLineWrapper(entryLine2);
			AssertEquals("No PreferenceClaimed for line 2 -  should be Normal rate", "NML", wrappedEntryLine2.PreferenceClaimed);
		}

		[TestDate(2016, 11, 09)]
		public void TestPreferenceWhenPrefGroupIsBlank()
		{
			var nzGroup = Factory.New<NZCGroup>();
			nzGroup.Q4_Code = "JP";
			nzGroup.Q4_Name = "JAPAN";
			nzGroup.Q4_IsCountry = true;

			var classification = Factory.New<NZCClassification>();
			classification.U0_Tariff = "0101.99.99.99J";
			classification.U0_DateActiveFrom = new ZDateTime(2016, 1, 1);
			classification.U0_Description = "TestClass";
			classification.U0_ComputerDumpDescr = "TestClass";

			var countryGroup = Factory.New<NZCCountryGroup>();
			countryGroup.U4_Country = "JP";
			countryGroup.U4_DateFrom = new ZDateTime(2016, 1, 1);

			var dutyRateFuturePreferential = Factory.New<NZCClassificationDutyRate>();
			dutyRateFuturePreferential.U1_U0_Classification = classification.PK;
			dutyRateFuturePreferential.U1_PreferentialCountryGroup = "JP";
			dutyRateFuturePreferential.U1_DateActiveFrom = new ZDateTime(2016, 1, 1);
			dutyRateFuturePreferential.U1_DutyRatePercent = 0m;
			Factory.Save();

			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_ATFOtherInfoValue = "9JRCZ";

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_DefaultQualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;
			invoice.JZ_DefaultPreferentialCountryGroup = ZString.Empty;
			invoice.JZ_RN_NKDefaultOrigin = "AU";
			invoice.JZ_RN_NKDefaultExport = "AU";
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 23m, "NZD");
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 2m, "NZD");

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 3000m;
			line1.JI_Tariff = "0101.99.99.99J";
			line1.JI_CustomsQuantity = 1m;
			line1.JI_CustomsUnitQty = "KGM";
			line1.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;
			line1.JI_PreferentialCountryGroup = ZString.Empty;
			line1.JI_CountryOfOrigin = "JP";
			line1.JI_RN_NKCountryOfExport = "JP";
			AssertEquals("PreferenceClaimed for line 1 should default from the only qualifying rate", "0101.99.99.99J @ JP = FREE", line1.JI_DutyRateComplete);

			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryLine1 = JobDeclaration.CustomsEntryHeaders[0].MergedLines[0];
			var wrappedEntryLine1 = new CusEntryLineWrapper(entryLine1);
			AssertEquals("PreferenceClaimed for line 1 should return country value from only qualifying rate if preference group is left blank", "JP", wrappedEntryLine1.PreferenceClaimed);
		}

		public void TestEntryLineCreditFees()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			CreateExportDrawbackJob();

			bool alacCreditFound = false;
			bool accCreditFound = false;
			bool heraCreditFound = false;
			bool pfmlCreditFound = false;
			var entryLine1 = JobDeclaration.CustomsEntryHeaders[0].MergedLines[0];
			var wrappedEntryLine1 = new CusEntryLineWrapper(entryLine1);
			foreach (IDutyTaxFee fee in wrappedEntryLine1.LineDutyTaxFees)
			{
				if (fee.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.AL)
				{
					alacCreditFound = true;
					AssertEquals("ALAC Credit", 100m, fee.Amount);
				}
			}

			var entryLine2 = JobDeclaration.CustomsEntryHeaders[0].MergedLines[1];
			var wrappedEntryLine2 = new CusEntryLineWrapper(entryLine2);
			foreach (IDutyTaxFee fee in wrappedEntryLine2.LineDutyTaxFees)
			{
				if (fee.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.AC)
				{
					accCreditFound = true;
					AssertEquals("ACC credit", 200m, fee.Amount);
				}
			}

			var entryLine3 = JobDeclaration.CustomsEntryHeaders[0].MergedLines[2];
			var wrappedEntryLine3 = new CusEntryLineWrapper(entryLine3);
			foreach (IDutyTaxFee fee in wrappedEntryLine3.LineDutyTaxFees)
			{
				if (fee.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.SL)
				{
					heraCreditFound = true;
					AssertEquals("HERA Credit", 300m, fee.Amount);
				}
			}

			var entryLine4 = JobDeclaration.CustomsEntryHeaders[0].MergedLines[3];
			var wrappedEntryLine4 = new CusEntryLineWrapper(entryLine4);
			foreach (IDutyTaxFee fee in wrappedEntryLine4.LineDutyTaxFees)
			{
				if (fee.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.PF)
				{
					pfmlCreditFound = true;
					AssertEquals("PFML Credit", 400m, fee.Amount);
				}
			}

			AssertEquals("alacCreditFound", true, alacCreditFound);
			AssertEquals("accCreditFound", true, accCreditFound);
			AssertEquals("heraCreditFound", true, heraCreditFound);
			AssertEquals("pfmlCreditFound", true, pfmlCreditFound);
		}

		[ExpectNoExceptions]
		public void TestEffectiveWeights()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ExportDate = new ZDateTime(2016, 03, 22);

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 20000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = "FOB";
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 385m, "NZD");

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 15000m;
			line1.JI_Tariff = "2204.21.18.11A";
			line1.JI_CustomsQuantity = 1000m;
			line1.JI_CustomsUnitQty = "LTR";
			line1.JI_Weight = 100m;
			line1.JI_WeightUQ = "";

			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 15000m;
			line2.JI_Tariff = "2204.21.18.11A";
			line2.JI_CustomsQuantity = 1000m;
			line2.JI_CustomsUnitQty = "LTR";
			line2.JI_NetWeightUQ = "";

			var line3 = invoice.JobComInvoiceLines.AddNew();
			line3.JI_LinePrice = 15000m;
			line3.JI_Tariff = "2204.21.18.11A";
			line3.JI_CustomsQuantity = 1000m;
			line3.JI_CustomsUnitQty = "LTR";
			line3.JI_NetWeightUQ = "";

			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryLine = JobDeclaration.CustomsEntryHeaders[0].MergedLines[0];
			var wrappedEntryLine = new CusEntryLineWrapper(entryLine);
			AssertEquals("ItemNetWeightInKGM should not cause an exception", 0m, wrappedEntryLine.ItemNetWeightInKGM);
			AssertEquals("ItemGrossWeightInKGM should not cause an exception", 0m, wrappedEntryLine.ItemGrossWeightInKGM);
		}

		public void TestIsPartsRelated()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ExportDate = new ZDateTime(2016, 08, 18);

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 20000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = "FOB";
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 385m, "NZD");

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 15000m;
			line1.JI_Tariff = "2204.21.18.11A";
			line1.JI_CustomsQuantity = 1000m;
			line1.JI_CustomsUnitQty = "LTR";
			line1.JI_QualifiesForPreferentialDuty = "N";
			line1.JI_CountryOfOrigin = "US";
			line1.JI_RN_NKCountryOfExport = "US";
			line1.OtherInfos.AddNew(LineOtherInfoList.Codes.Parts, "");

			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryLine = JobDeclaration.CustomsEntryHeaders[0].MergedLines[0];
			var wrappedEntryLine = new CusEntryLineWrapper(entryLine);
			AssertNotNull("OtherInfoCodes", wrappedEntryLine.OtherInfoCodes);
			AssertEquals("IsPartsRelated", true, wrappedEntryLine.IsPartsRelated);
		}

		public void TestForeignCurrency()
		{
			CreateExportAirJob();

			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_FullName = "Supplier Co-Op Pty Ltd";

			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			supplier2.OH_FullName = "Jones Supplier Pty Ltd";

			var invoice1 = JobDeclaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 1000m;
			invoice1.JZ_RX_NKInvoice_Currency = "AUD";
			invoice1.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.Floating;
			invoice1.JZ_IncoTerm = "FOB";
			invoice1.JZ_OH_Supplier = supplier1.PK;

			var invoice2 = JobDeclaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 2000m;
			invoice2.JZ_RX_NKInvoice_Currency = "USD";
			invoice2.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.ForwardCover;
			invoice2.JZ_InvoiceCurrExRate = invoice2.JZ_InvoiceCurrExRate + 0.02m;
			invoice2.JZ_IncoTerm = "FOB";
			invoice2.JZ_OH_Supplier = supplier2.PK;

			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 1000m;
			line1.JI_Tariff = "2204.21.18.11A";
			line1.JI_CustomsQuantity = 1000m;
			line1.JI_Description = "Invoice 1 / Line 1";

			var line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 2000m;
			line2.JI_Tariff = "4201.00.00.01B";
			line2.JI_CustomsQuantity = 10m;
			line2.JI_Description = "Invoice 2 / Line 1";

			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Should be 2 entry lines created", 2, JobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);

			int aUDLineIndex = JobDeclaration.CustomsEntryHeaders[0].MergedLines[0].InvoiceCurrency.Code == "AUD" ? 0 : 1;
			int uSDLineIndex = JobDeclaration.CustomsEntryHeaders[0].MergedLines[0].InvoiceCurrency.Code == "USD" ? 0 : 1;
			var entryLine1 = JobDeclaration.CustomsEntryHeaders[0].MergedLines[aUDLineIndex];
			var wrappedEntryLine1 = new CusEntryLineWrapper(entryLine1);
			AssertEquals("ForeignCurrencyCode - line 1", "AUD", wrappedEntryLine1.ForeignCurrencyCode);
			AssertEquals("ValueInForeignCurrency - line 1", 1000.00m, wrappedEntryLine1.ValueInForeignCurrency);

			var entryLine2 = JobDeclaration.CustomsEntryHeaders[0].MergedLines[uSDLineIndex];
			var wrappedEntryLine2 = new CusEntryLineWrapper(entryLine2);
			AssertEquals("ForeignCurrencyCode - line 2", "USD", wrappedEntryLine2.ForeignCurrencyCode);
			AssertEquals("ValueInForeignCurrency - line 2", 2000.00m, wrappedEntryLine2.ValueInForeignCurrency);
		}

		public void TestForeignCurrencyUsingForwardCoverRateAUD()
		{
			CreateExportAirJob();

			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_FullName = "Supplier Co-Op Pty Ltd";

			var invoice1 = JobDeclaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 1000m;
			invoice1.JZ_RX_NKInvoice_Currency = "AUD";
			invoice1.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.ForwardCover;
			invoice1.JZ_InvoiceCurrExRate = invoice1.JZ_InvoiceCurrExRate + 0.02m;
			invoice1.JZ_IncoTerm = "FOB";
			invoice1.JZ_OH_Supplier = supplier1.PK;

			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 1000m;
			line1.JI_Tariff = "2204.21.18.11A";
			line1.JI_CustomsQuantity = 10m;
			line1.JI_Description = "Invoice 1 / Line 1";

			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Should be 1 entry line created", 1, JobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);

			var entryLine1 = JobDeclaration.CustomsEntryHeaders[0].MergedLines[0];
			var wrappedEntryLine1 = new CusEntryLineWrapper(entryLine1);
			AssertEquals("ForeignCurrencyCode - line 1", "AUD", wrappedEntryLine1.ForeignCurrencyCode);
			AssertEquals("ValueInForeignCurrency - line 1", 1000.00m, wrappedEntryLine1.ValueInForeignCurrency);
		}

		public void TestForeignCurrencyUsingForwardCoverRateUSD()
		{
			CreateExportAirJob();

			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_FullName = "Supplier Co-Op Pty Ltd";

			var invoice1 = JobDeclaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 2000m;
			invoice1.JZ_RX_NKInvoice_Currency = "USD";
			invoice1.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.ForwardCover;
			invoice1.JZ_InvoiceCurrExRate = invoice1.JZ_InvoiceCurrExRate + 0.02m;
			invoice1.JZ_IncoTerm = "FOB";
			invoice1.JZ_OH_Supplier = supplier1.PK;

			var line2 = invoice1.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 2000m;
			line2.JI_Tariff = "4201.00.00.01B";
			line2.JI_CustomsQuantity = 10m;
			line2.JI_Description = "Invoice 1 / Line 1";

			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Should be 1 entry line created", 1, JobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);

			var entryLine1 = JobDeclaration.CustomsEntryHeaders[0].MergedLines[0];
			var wrappedEntryLine1 = new CusEntryLineWrapper(entryLine1);
			AssertEquals("ForeignCurrencyCode - line 1", "USD", wrappedEntryLine1.ForeignCurrencyCode);
			AssertEquals("ValueInForeignCurrency - line 1", 2000.00m, wrappedEntryLine1.ValueInForeignCurrency);
		}

		#region Implementation

		JobDeclaration JobDeclaration
		{
			get
			{
				if (jobDeclaration == null)
				{
					jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				}

				return jobDeclaration;
			}
		}
		JobDeclaration jobDeclaration;

		void CreateImportSeaJob()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_TotalWeight = 15m;
			JobDeclaration.JE_TotalWeightUnit = "T";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_DeclarationReference = "BSIS00002309";
			JobDeclaration.JE_ExportDate = ZDateTime.Today.AddDays(-14);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "HB92027";
			JobDeclaration.JE_MasterBill = "OB293042-24902";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_RL_NKFinalDestination = "NZAKL";
			JobDeclaration.JE_RL_NKOrigin = "SGSIN";
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfLoading = "SGSIN";
			JobDeclaration.JE_VesselName = "HYOGO MARU";
			JobDeclaration.JE_TotalNoOfPacks = 15;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CNT";
		}

		void CreateExportAirJob()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_TransportMode = "AIR";
			JobDeclaration.JE_TotalWeight = 15m;
			JobDeclaration.JE_TotalWeightUnit = "T";
			JobDeclaration.JE_VoyageFlightNo = "NZ1";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_DeclarationReference = "BSIS00002309";
			JobDeclaration.JE_ExportDate = ZDateTime.Today;
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "HB92027";
			JobDeclaration.JE_MasterBill = "08600238210";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_RL_NKFinalDestination = "SGSIN";
			JobDeclaration.JE_RL_NKOrigin = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfArrival = "SGSIN";
			JobDeclaration.JE_RL_NKPortOfLoading = "NZAKL";
			JobDeclaration.JE_TotalNoOfPacks = 15;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CNT";
		}

		protected void CreateExportDrawbackJob()
		{
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobDeclaration.JE_ApplicationCode = "TSW";
			JobDeclaration.JE_DeclarationReference = "BSES002932";
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_PaymentMethod = PaymentMethodList.Codes.CashPaidByBroker;
			JobDeclaration.JE_TotalWeight = 15m;
			JobDeclaration.JE_TotalWeightUnit = "T";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_ExportDate = new ZDateTime(2013, 2, 28);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "HB92027";
			JobDeclaration.JE_MasterBill = "OB293042-24902";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_RL_NKFinalDestination = "SGSIN";
			JobDeclaration.JE_RL_NKOrigin = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfArrival = "SGSIN";
			JobDeclaration.JE_RL_NKPortOfLoading = "NZAKL";
			JobDeclaration.JE_VesselName = "HYOGO MARU";
			JobDeclaration.JE_TotalNoOfPacks = 15;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CNT";

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 2, 22);

			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_LevyCreditAmountCode = LevyCodesList.Codes.ALAC;
			invoiceLine.JI_LevyCreditAmount = 100M;

			var invoiceLine2 = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0203.11.00.02C";
			invoiceLine2.JI_LinePrice = 900m;
			invoiceLine2.JI_LevyCreditAmountCode = LevyCodesList.Codes.ACC;
			invoiceLine2.JI_LevyCreditAmount = 200M;

			var invoiceLine3 = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2301.10.00.01A";
			invoiceLine3.JI_LinePrice = 2000m;
			invoiceLine3.JI_LevyCreditAmountCode = LevyCodesList.Codes.HERA;
			invoiceLine3.JI_LevyCreditAmount = 300M;

			var invoiceLine4 = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "2701.11.00.00C";
			invoiceLine4.JI_LinePrice = 3000m;
			invoiceLine4.JI_LevyCreditAmountCode = LevyCodesList.Codes.PFML;
			invoiceLine4.JI_LevyCreditAmount = 400M;

			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		#endregion
	}
}
