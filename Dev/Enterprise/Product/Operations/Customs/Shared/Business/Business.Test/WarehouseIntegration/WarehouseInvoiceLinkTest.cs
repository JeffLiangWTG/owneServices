using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Business.WarehouseInvoiceLink;

namespace Enterprise.Customs.Business.Testing
{
	sealed class WarehouseInvoiceLinkTest : TestCaseWithFactory
	{
		public void TestKey()
		{
			InvoiceLineMapper mapper = new InvoiceLineMapper(null, Factory.New<BaseJobDeclaration>());
			WarehouseInvoiceLinkTestCase.DummyBondedWarehouseTransactionLine transactionLine = new WarehouseInvoiceLinkTestCase.DummyBondedWarehouseTransactionLine();

			OrgHeader supplier = OrgHeader.New(Factory);
			supplier.FillWithValidTestData();
			OrgHeader buyer = OrgHeader.New(Factory);
			buyer.FillWithValidTestData();

			RefCountry country = Factory.New<RefCountry>();
			country.FillWithValidTestData();
			country.Code = "~~";

			MasterFiles.Business.OrgSupplierPart part = MasterFiles.Business.OrgSupplierPart.New(Factory);
			part.OP_PartNum = "~~";
			OrgPartRelation relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = supplier.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			BaseCusClassification classification = Factory.NewWithValidTestData<BaseCusClassification>();
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			classification.CC_LookupCode = "~~";
			classification.CC_TariffNum = "0001.01.01";
			classification.CC_Description = "TESTDESCRIPTION";

			BaseCusClassPartPivot partClassPivot = Factory.NewWithValidTestData<BaseCusClassPartPivot>();
			partClassPivot.CI_OP = part.PK;
			partClassPivot.CI_CC = classification.PK;

			Factory.Save();

			transactionLine.ExposedWarehouse = OrgHeader.New(Factory).MainAddress;
			transactionLine.ExposedEntryLineNumber = 5;
			transactionLine.ExposedEntryKey = "ENTRY-KEY";
			transactionLine.ExposedProduct = part;
			transactionLine.ExposedCustomsQuantity = 99.9m;
			transactionLine.ExposedCustomsQuantityUnit = "XX";
			transactionLine.ExposedQuantity = 10.3m;
			transactionLine.ExposedQuantityUnit = "ZX";
			transactionLine.ExposedValueForDuty = 232.2499m;
			transactionLine.ExposedCountryOfOrigin = country;
			transactionLine.ExposedBondedWarehouseQuantity = 88.8m;
			transactionLine.ExposedBondedWarehouseQuantityUnit = "QQ";
			transactionLine.ExposedPartAttrib1 = "Attrib1";
			transactionLine.ExposedPartAttrib2 = "Attrib2";
			transactionLine.ExposedPartAttrib3 = "Attrib3";
			transactionLine.ExposedSerialNumber = "SerialNumber";

			AssertEquals("Correct key",
				"PRODUCTPKENTRY-KEY510.3ZXWAREHOUSEPKAttrib1Attrib2Attrib3SerialNumber232.25".Replace("PRODUCTPK", part.PK.ToStringKey()).Replace("WAREHOUSEPK", transactionLine.Warehouse.PK.ToStringKey()),
				mapper.GetKey(transactionLine));

			transactionLine.ExposedProduct = null;
			transactionLine.ExposedWarehouse = null;

			AssertEquals("Correct key",
				"ENTRY-KEY510.3ZXAttrib1Attrib2Attrib3SerialNumber232.25",
				mapper.GetKey(transactionLine));
		}
	}
}
