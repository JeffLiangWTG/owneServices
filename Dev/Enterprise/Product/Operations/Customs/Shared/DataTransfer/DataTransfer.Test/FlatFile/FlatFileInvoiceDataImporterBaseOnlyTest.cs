using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Customs.DataTransfer.FlatFileInvoiceDataImporter.Constants;

namespace Enterprise.Customs.DataTransfer.Testing
{
	sealed class FlatFileInvoiceDataImporterBaseOnlyTest : FlatFileInvoiceDataImporterAbstractTest
	{
		public void TestInvoiceLineWithUnKnownPartCodeButKnownClassification()
		{
			string[] invoiceLineRecord = GetInvoiceLineRecord("1", InvoiceNumber, "10", "PK", Description, UnKnownProductCode, Classification.CC_LookupCode, "", "100.00");
			DataImporter.PopulateInvoiceLine(InvoiceLine, invoiceLineRecord);
			AssertEquals("InvoiceLine.JI_PartNo", UnKnownProductCode, InvoiceLine.JI_PartNo);
			AssertEquals("InvoiceLine.JI_CC", Classification.PK, InvoiceLine.JI_CC);
			AssertEquals("InvoiceLine.JI_Tariff", KnownClassificationTariff, InvoiceLine.JI_Tariff);
			AssertEquals("InvoiceLine.JI_Description", Description, InvoiceLine.JI_Description);
		}

		public void TestInvoiceLineWithUnKnownClassificationAndPartCode()
		{
			string[] invoiceLineRecord = GetInvoiceLineRecord("1", InvoiceNumber, "10", "PK", Description, UnKnownProductCode, UnKnownClassificationCode, TariffCode, "100.00");
			DataImporter.PopulateInvoiceLine(InvoiceLine, invoiceLineRecord);
			AssertEquals("InvoiceLine.JI_PartNo", UnKnownProductCode, InvoiceLine.JI_PartNo);
			AssertEquals("InvoiceLine.JI_CC", ZGuid.Empty, InvoiceLine.JI_CC);
			AssertEquals("InvoiceLine.JI_Tariff", TariffCode, InvoiceLine.JI_Tariff);
			AssertEquals("InvoiceLine.JI_Description", Description, InvoiceLine.JI_Description);
		}

		public void TestInvoiceLineWithUnKnownPartCodeButKnownClassificationAndTariffCode()
		{
			string[] invoiceLineRecord = GetInvoiceLineRecord("1", InvoiceNumber, "10", "PK", Description, UnKnownProductCode, Classification.CC_LookupCode, TariffCode, "100.00");
			DataImporter.PopulateInvoiceLine(InvoiceLine, invoiceLineRecord);
			AssertEquals("InvoiceLine.JI_PartNo", UnKnownProductCode, InvoiceLine.JI_PartNo);
			AssertEquals("InvoiceLine.JI_CC", ZGuid.Empty, InvoiceLine.JI_CC);
			AssertEquals("InvoiceLine.JI_Tariff", TariffCode, InvoiceLine.JI_Tariff);
			AssertEquals("InvoiceLine.JI_Description", Description, InvoiceLine.JI_Description);
		}

		public void TestInvoiceLineWithKnownPartCodeAndUnKnownClassificationAndNoTariffCode()
		{
			string[] invoiceLineRecord = GetInvoiceLineRecord("1", InvoiceNumber, "10", "PK", Description, Product.OP_PartNum, UnKnownClassificationCode, "", "100.00");
			DataImporter.PopulateInvoiceLine(InvoiceLine, invoiceLineRecord);
			AssertEquals("InvoiceLine.JI_PartNo", KnownProductCode, InvoiceLine.JI_PartNo);
			AssertEquals("InvoiceLine.JI_CC", Classification.PK, InvoiceLine.JI_CC);
			AssertEquals("InvoiceLine.JI_Tariff", KnownClassificationTariff, InvoiceLine.JI_Tariff);
			AssertEquals("InvoiceLine.JI_Description", Description, InvoiceLine.JI_Description);
		}

		protected override BaseJobDeclaration GetNewJobDeclaration() => BaseJobDeclaration.New(Factory);

		BaseJobComInvoiceLine invoiceLine;
		BaseJobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var groupHeader = Declaration.JobComInvoiceGroupHeaders[0];
					var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
					invoiceHeader.JZ_InvoiceNumber = InvoiceNumber;
					invoiceHeader.JZ_OH_Supplier = Supplier.PK;
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				}
				return invoiceLine;
			}
		}

		BaseJobDeclaration jobDeclaration;
		BaseJobDeclaration Declaration
		{
			get
			{
				if (jobDeclaration == null)
				{
					jobDeclaration = GetNewJobDeclaration();
					jobDeclaration.JE_OH_Supplier = Supplier.PK;
				}
				return jobDeclaration;
			}
		}

		BaseCusClassification classification;
		BaseCusClassification Classification
		{
			get
			{
				if (classification == null)
				{
					classification = Factory.New<BaseCusClassification>();
					classification.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
					classification.CC_LookupCode = KnownClassificationCode;
					classification.CC_Description = KnownClassificationDescription;
					classification.CC_TariffNum = KnownClassificationTariff;
					classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				}
				return classification;
			}
		}

		Business.OrgSupplierPart product;
		Business.OrgSupplierPart Product
		{
			get
			{
				if (product == null)
				{
					product = Factory.New<Business.OrgSupplierPart>();
					product.ClassificationsForBinding.Add(Classification);
					product.RelatedOrganisations.AddOrganisationIfNotExist(Supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
					product.OP_PartNum = KnownProductCode;
					product.OP_Desc = KnownProductDescription;
				}
				return product;
			}
		}

		OrgHeader supplier;
		OrgHeader Supplier
		{
			get
			{
				if (supplier == null)
				{
					supplier = OrgHeader.New(Factory);
					supplier.FillWithValidTestData();
					supplier.OH_IsConsignor = true;
				}
				return supplier;
			}
		}

		FlatFileInvoiceDataImporter dataImporter;
		FlatFileInvoiceDataImporter DataImporter
		{
			get
			{
				if (dataImporter == null)
				{
					dataImporter = new FlatFileInvoiceDataImporter("", Declaration);
				}
				return dataImporter;
			}
		}

		string[] GetInvoiceLineRecord(string lineNo, string invoiceNo, string quantity, string unitOfQty, string description, string productCode, string classificationCode, string tariffCode, string linePrice)
		{
			string[] result = new string[Constants.InvoiceLineFields.RecordLength];

			result[Constants.InvoiceLineFields.Type] = Constants.InvoiceRecordType.Line;
			result[Constants.InvoiceLineFields.LineNo] = lineNo;
			result[Constants.InvoiceLineFields.InvoiceNo] = invoiceNo;
			result[Constants.InvoiceLineFields.ProductCode] = productCode;
			result[Constants.InvoiceLineFields.ProductDescription] = description;
			result[Constants.InvoiceLineFields.Quantity] = quantity;
			result[Constants.InvoiceLineFields.UnitofQty] = unitOfQty;
			result[Constants.InvoiceLineFields.LinePrice] = linePrice;
			result[Constants.InvoiceLineFields.TariffCode] = tariffCode;
			result[Constants.InvoiceLineFields.TariffLookup] = classificationCode;

			return result;
		}

		const string InvoiceNumber = "INVOICE1";
		const string KnownProductCode = "KNOWNPRODUCT";
		const string KnownProductDescription = "PRODUCT DESCRIPTION";
		const string UnKnownProductCode = "UNKNOWNPRODUCT";
		const string KnownClassificationCode = "KNOWNLOOKUP";
		const string KnownClassificationDescription = "CLASSIFICATION DESCRIPTION";
		const string KnownClassificationTariff = "5678";
		const string UnKnownClassificationCode = "UNKNOWNLOOKUP";
		const string TariffCode = "1234";
		const string Description = "STANDALONE DESCRIPTION";
	}
}
