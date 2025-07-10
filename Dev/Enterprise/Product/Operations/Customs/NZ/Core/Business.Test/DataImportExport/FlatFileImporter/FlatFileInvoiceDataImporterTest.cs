using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using GlbCompany = Enterprise.MasterFiles.Business.GlbCompany;
using OrgHeader = Enterprise.MasterFiles.Business.OrgHeader;
using OrgPartRelation = Enterprise.MasterFiles.Business.OrgPartRelation;

namespace Enterprise.Customs.NZ.Business.Data.FlatFileImporter.Testing
{
	sealed class FlatFileInvoiceDataImporterTest : DataTransfer.Testing.FlatFileInvoiceDataImporterAbstractTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOrderOfPrecedence()
		{
			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			part.OP_PartNum = "PART";
			part.OP_Desc = "PART DESCRIPTION";

			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LOOKUP";
			classification.CC_TariffNum = "LOOKUPTAR";
			classification.CC_Description = "LOOKUP DESCRIPTION";
			classification.CC_ClassificationType = CusClassification.ClassificationType.Both;

			FlatFileInvoiceDataImporter importer = GetFlatFileInvoiceDataImporterLocal(BaseSourcePath + @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\DataImportExport\FlatFileImporter\TestFiles\InvoiceDataForPartAndTariffPrecedenceTesting.csv");
			importer.Import();
			AssertEquals("Precondition: Declaration.Invoices.Count", 1, Declaration.Invoices.Count);
			AssertEquals("Precondition: Declaration.InvoiceLines.Count", 5, Declaration.InvoiceLines.Count);

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices[0];
			AssertEquals("Invoice Number", "2040925204", invoiceHeader.JZ_InvoiceNumber);

			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines[0];
			AssertEquals("InvoiceLine1.JI_Description", "DESCRIPTION", invoiceLine1.JI_Description);
			AssertEquals("InvoiceLine1.JI_PartNo", "PART", invoiceLine1.JI_PartNo);

			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines[1];
			AssertEquals("InvoiceLine2.JI_Description", "DESCRIPTION", invoiceLine2.JI_Description);
			AssertEquals("InvoiceLine2.Class", null, invoiceLine2.Classification);
			AssertEquals("InvoiceLine2.JI_Tariff", "0000", invoiceLine2.JI_Tariff);

			JobComInvoiceLine invoiceLine3 = invoiceHeader.JobComInvoiceLines[2];
			AssertEquals("InvoiceLine3.JI_Description", "DESCRIPTION", invoiceLine3.JI_Description);
			AssertEquals("InvoiceLine3.Class", classification, invoiceLine3.Classification);

			JobComInvoiceLine invoiceLine4 = invoiceHeader.JobComInvoiceLines[3];
			AssertEquals("InvoiceLine4.JI_Description", "DESCRIPTION", invoiceLine4.JI_Description);
			AssertEquals("InvoiceLine4.JI_Tariff", "0000", invoiceLine4.JI_Tariff);

			JobComInvoiceLine invoiceLine5 = invoiceHeader.JobComInvoiceLines[4];
			AssertEquals("InvoiceLine5.JI_Description", "DESCRIPTION", invoiceLine5.JI_Description);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportInvoice()
		{
			FlatFileInvoiceDataImporter importer = GetFlatFileInvoiceDataImporterLocal(BaseSourcePath + @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\DataImportExport\FlatFileImporter\TestFiles\InvoiceData.csv");
			importer.Import();

			AssertEquals("Should have one Commercial Invoice After Import", 1, Declaration.Invoices.Count);
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices[0];
			AssertEquals("Invoice Number", "2040925204", invoiceHeader.JZ_InvoiceNumber);
			AssertEquals("Related", "Y", invoiceHeader.JZ_RelationshipIndicator);

			AssertEquals("Invoice Should have one commercial Invoice Line", 1, invoiceHeader.JobComInvoiceLines.Count);

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines[0];
			AssertEquals("InvoiceLine.JI_OrderNumber", "CRTA81253506", invoiceLine.JI_OrderNumber);
			AssertEquals("InvoiceLine.JI_PartNo", "PRODCODE", invoiceLine.JI_PartNo);
			AssertEquals("InvoiceLine.JI_PartAttrib1", "Attrib1", invoiceLine.JI_PartAttrib1);
			AssertEquals("InvoiceLine.JI_PartAttrib2", "Attrib2", invoiceLine.JI_PartAttrib2);
			AssertEquals("InvoiceLine.JI_PartAttrib3", "Attrib3", invoiceLine.JI_PartAttrib3);
			AssertEquals("InvoiceLine.JI_Description", "DESCRIPTION", invoiceLine.JI_Description);
			AssertEquals("InvoiceLine.JI_InvoiceQuantity", 1m, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("InvoiceLine.JI_InvoiceUQ", "PCE", invoiceLine.JI_InvoiceUQ);
			AssertEquals("InvoiceLine.JI_Volume", 0.064m, invoiceLine.JI_Volume);
			AssertEquals("InvoiceLine.JI_VolumeUQ", "M3", invoiceLine.JI_VolumeUQ);
			AssertEquals("InvoiceLine.JI_Weight", 4m, invoiceLine.JI_Weight);
			AssertEquals("InvoiceLine.JI_WeightUQ", "KG", invoiceLine.JI_WeightUQ);
			AssertEquals("InvoiceLine.JI_LinePrice", 1908.06m, invoiceLine.JI_LinePrice);
			AssertEquals("InvoiceLine.JI_Tariff", "0000.00.00.00K", invoiceLine.JI_Tariff);
			AssertEquals("InvoiceLine.Class", null, invoiceLine.Classification);
			AssertEquals("InvoiceLine.JI_CustomsQuantity", 1.92m, invoiceLine.JI_CustomsQuantity);
			AssertEquals("InvoiceLine.JI_CustomsUnitQty", "KG", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("InvoiceLine.JI_CountryOfOrigin", "ID", invoiceLine.JI_CountryOfOrigin);
			AssertEquals("InvoiceLine.JI_RN_NKCountryOfExport", "ZA", invoiceLine.JI_RN_NKCountryOfExport);
			AssertEquals("InvoiceLine.JI_QualifiesForPreferentialDuty", QualifiesForPreferentialDutyList.Codes.Qualifies, invoiceLine.JI_QualifiesForPreferentialDuty);
			AssertEquals("InvoiceLine.JI_ConcessionCode", "601180C", invoiceLine.JI_ConcessionCode);
			AssertEquals("InvoiceLine.JI_CustomAttrib1", "CustomText1", invoiceLine.JI_CustomAttrib1);
			AssertEquals("InvoiceLine.JI_CustomAttrib2", "CustomText2", invoiceLine.JI_CustomAttrib2);
			AssertEquals("InvoiceLine.JI_CustomAttrib3", "CustomText3", invoiceLine.JI_CustomAttrib3);
			AssertEquals("InvoiceLine.JI_CustomDate1", new ZDateTime(2004, 9, 28, 10, 6, 14), invoiceLine.JI_CustomDate1);
			AssertEquals("InvoiceLine.JI_CustomDate2", new ZDateTime(2004, 9, 28, 11, 6, 15), invoiceLine.JI_CustomDate2);
			AssertEquals("InvoiceLine.JI_CustomDate3", new ZDateTime(2004, 9, 28, 12, 6, 16), invoiceLine.JI_CustomDate3);
			AssertEquals("InvoiceLine.JI_CustomFlag1", true, invoiceLine.JI_CustomFlag1);
			AssertEquals("InvoiceLine.JI_CustomFlag2", false, invoiceLine.JI_CustomFlag2);
			AssertEquals("InvoiceLine.JI_CustomFlag3", true, invoiceLine.JI_CustomFlag3);
			AssertEquals("InvoiceLine.JI_CustomDecimal1", 1m, invoiceLine.JI_CustomDecimal1);
			AssertEquals("InvoiceLine.JI_CustomDecimal2", 2m, invoiceLine.JI_CustomDecimal2);
			AssertEquals("InvoiceLine.JI_CustomDecimal3", 3m, invoiceLine.JI_CustomDecimal3);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportInvoiceWithPartPresent()
		{
			Declaration.JE_OH_Importer = Importer.PK;
			Declaration.JE_OH_Supplier = Supplier.PK;
			AssertNotNull(Part);
			AssertNotNull(Lookup);

			Factory.Save();

			FlatFileInvoiceDataImporter dataImporter = GetFlatFileInvoiceDataImporterLocal(BaseSourcePath + @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\DataImportExport\FlatFileImporter\TestFiles\InvoiceData.csv");
			dataImporter.Import();

			AssertEquals("Should have one Commercial Invoice After Import", 1, Declaration.Invoices.Count);
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices[0];
			AssertEquals("Invoice Number", "2040925204", invoiceHeader.JZ_InvoiceNumber);

			AssertEquals("Invoice Should have one commercial Invoice Line", 1, invoiceHeader.JobComInvoiceLines.Count);

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines[0];
			AssertEquals("InvoiceLine.JI_OrderNumber", "CRTA81253506", invoiceLine.JI_OrderNumber);
			AssertEquals("InvoiceLine.JI_PartNo", "PRODCODE", invoiceLine.JI_PartNo);
			AssertEquals("InvoiceLine.JI_PartAttrib1", "Attrib1", invoiceLine.JI_PartAttrib1);
			AssertEquals("InvoiceLine.JI_PartAttrib2", "Attrib2", invoiceLine.JI_PartAttrib2);
			AssertEquals("InvoiceLine.JI_PartAttrib3", "Attrib3", invoiceLine.JI_PartAttrib3);
			AssertEquals("InvoiceLine.JI_Description", "DESCRIPTION", invoiceLine.JI_Description);
			AssertEquals("InvoiceLine.JI_InvoiceQuantity", 1m, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("InvoiceLine.JI_InvoiceUQ", "PCE", invoiceLine.JI_InvoiceUQ);
			AssertEquals("InvoiceLine.JI_Volume", 0.064m, invoiceLine.JI_Volume);
			AssertEquals("InvoiceLine.JI_VolumeUQ", "M3", invoiceLine.JI_VolumeUQ);
			AssertEquals("InvoiceLine.JI_Weight", 4m, invoiceLine.JI_Weight);
			AssertEquals("InvoiceLine.JI_WeightUQ", "KG", invoiceLine.JI_WeightUQ);
			AssertEquals("InvoiceLine.JI_LinePrice", 1908.06m, invoiceLine.JI_LinePrice);
			AssertEquals("InvoiceLine.JI_Tariff", "0000.00.00.00K", invoiceLine.JI_Tariff);
			AssertEquals("InvoiceLine.Class", Lookup, invoiceLine.Classification);
			AssertEquals("InvoiceLine.JI_CustomsQuantity", 1.92m, invoiceLine.JI_CustomsQuantity);
			AssertEquals("InvoiceLine.JI_CustomsUnitQty", "KG", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("InvoiceLine.JI_CountryOfOrigin", "ID", invoiceLine.JI_CountryOfOrigin);
			AssertEquals("InvoiceLine.JI_RN_NKCountryOfExport", "ZA", invoiceLine.JI_RN_NKCountryOfExport);
			AssertEquals("InvoiceLine.JI_QualifiesForPreferentialDuty", QualifiesForPreferentialDutyList.Codes.Qualifies, invoiceLine.JI_QualifiesForPreferentialDuty);
			AssertEquals("InvoiceLine.JI_ConcessionCode", "601180C", invoiceLine.JI_ConcessionCode);
			AssertEquals("InvoiceLine.JI_CustomAttrib1", "CustomText1", invoiceLine.JI_CustomAttrib1);
			AssertEquals("InvoiceLine.JI_CustomAttrib2", "CustomText2", invoiceLine.JI_CustomAttrib2);
			AssertEquals("InvoiceLine.JI_CustomAttrib3", "CustomText3", invoiceLine.JI_CustomAttrib3);
			AssertEquals("InvoiceLine.JI_CustomDate1", new ZDateTime(2004, 9, 28, 10, 6, 14), invoiceLine.JI_CustomDate1);
			AssertEquals("InvoiceLine.JI_CustomDate2", new ZDateTime(2004, 9, 28, 11, 6, 15), invoiceLine.JI_CustomDate2);
			AssertEquals("InvoiceLine.JI_CustomDate3", new ZDateTime(2004, 9, 28, 12, 6, 16), invoiceLine.JI_CustomDate3);
			AssertEquals("InvoiceLine.JI_CustomFlag1", true, invoiceLine.JI_CustomFlag1);
			AssertEquals("InvoiceLine.JI_CustomFlag2", false, invoiceLine.JI_CustomFlag2);
			AssertEquals("InvoiceLine.JI_CustomFlag3", true, invoiceLine.JI_CustomFlag3);
			AssertEquals("InvoiceLine.JI_CustomDecimal1", 1m, invoiceLine.JI_CustomDecimal1);
			AssertEquals("InvoiceLine.JI_CustomDecimal2", 2m, invoiceLine.JI_CustomDecimal2);
			AssertEquals("InvoiceLine.JI_CustomDecimal3", 3m, invoiceLine.JI_CustomDecimal3);
		}

		protected override ZString GetApplicationCode => JobApplicationCodeList.Codes.CUS;

		protected override Customs.Business.BaseJobDeclaration GetNewJobDeclaration() => JobDeclaration.New(Factory);

		protected override DataTransfer.FlatFileInvoiceDataImporter GetFlatFileInvoiceDataImporter(ZString sourceFile) => GetFlatFileInvoiceDataImporterLocal(sourceFile);

		CusClassification Lookup
		{
			get
			{
				if (lookup == null)
				{
					lookup = Factory.New<CusClassification>();
					lookup.CC_LookupCode = "MYLOOKUP";
					lookup.CC_Description = "My Lookup is deadset HUGE.";
					lookup.CC_ClassificationType = CusClassification.ClassificationType.Both;
					lookup.CC_TariffNum = "0000.00.00.00K";
				}
				return lookup;
			}
		}
		CusClassification lookup;

		OrgSupplierPart Part
		{
			get
			{
				if (part == null)
				{
					part = OrgSupplierPart.New(Factory);
					part.OP_PartNum = "PRODCODE";
					part.OP_Desc = "My Part Rocks.";
					var classPartPivot = Factory.New<Customs.Business.BaseCusClassPartPivot>();
					classPartPivot.CI_CC = Lookup.PK;
					classPartPivot.CI_OP = part.PK;

					var supplierRelation = part.RelatedOrganisations.AddNew();
					supplierRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
					supplierRelation.OU_OH = Supplier.PK;

					var importerRelation = part.RelatedOrganisations.AddNew();
					importerRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
					importerRelation.OU_OH = Importer.PK;
					Factory.Save();
				}
				return part;
			}
		}
		OrgSupplierPart part;

		OrgHeader Importer
		{
			get
			{
				if (importer == null)
				{
					importer = OrgHeader.New(Factory);
					importer.FillWithValidTestData();
					importer.OH_IsConsignee = true;
					importer.OH_RL_NKClosestPort = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					importer.OH_FullName = "IMPORTER";
					importer.OH_Code = "ZZIMPZZZ";
				}
				return importer;
			}
		}
		OrgHeader importer;

		OrgHeader Supplier => supplier ?? (supplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "NISIAW")));
		OrgHeader supplier;

		JobDeclaration Declaration => (JobDeclaration)declaration;

		FlatFileInvoiceDataImporter GetFlatFileInvoiceDataImporterLocal(ZString sourceFile) => new FlatFileInvoiceDataImporter(sourceFile, Declaration);
	}
}
