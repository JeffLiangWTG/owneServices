using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer.Testing
{
	[TestedType(typeof(CustomsProductValueObjectDataAdapter))]
	public class CustomsProductValueObjectDataAdapterTest : ProductValueObjectDataAdapterTest
	{
		public void TestImportProductForExistingLookup()
		{
			var product1 = GetNewPopulatedProductWithLookup();
			var xsdProduct = MakeExport(product1);

			var product = (OrgSupplierPart)GetNewPopulatedProduct();
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);

			AssertEquals("Lookup 1", product.ClassificationsForBinding[0].CC_LookupCode);
			AssertEquals("Descr 1", product.ClassificationsForBinding[0].CC_Description);
			AssertEquals("1601000001", product.ClassificationsForBinding[0].CC_TariffNum.ExcludeChars(" ."));
			AssertEquals(BaseCusClassification.ClassificationType.IMP, product.ClassificationsForBinding[0].CC_ClassificationType);
			AssertEquals(Business.ClassificationTypeList.Codes.HTI, product.PivotsForBinding[0].CI_ChildType);

			AssertEquals("Lookup 2", product.ClassificationsForBinding[1].CC_LookupCode);
			AssertEquals("Descr 2", product.ClassificationsForBinding[1].CC_Description);
			AssertEquals("1601000005", product.ClassificationsForBinding[1].CC_TariffNum.ExcludeChars(" ."));
			AssertEquals(BaseCusClassification.ClassificationType.EXP, product.ClassificationsForBinding[1].CC_ClassificationType);
			AssertEquals(BaseCusClassification.ClassificationType.EXP, product.PivotsForBinding[1].CI_ChildType);
		}

		public virtual void TestImportProductForLookupTariff()
		{
			var product = GetNewPopulatedProductWithLookup();
			var xsdProduct = MakeExport(product);

			classification.Delete();

			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);
			AssertEquals(2, product.ClassificationsForBinding.Count);

			AssertEquals("Lookup 1", product.ClassificationsForBinding[0].CC_LookupCode);
			AssertEquals("Descr 1", product.ClassificationsForBinding[0].CC_Description);
			AssertEquals("1601000001", product.ClassificationsForBinding[0].CC_TariffNum.ExcludeChars(" ."));
			AssertNotEquals(ZString.Empty, product.ClassificationsForBinding[0].CC_RN_NKCountryCode);
			AssertEquals(BaseCusClassification.ClassificationType.IMP, product.ClassificationsForBinding[0].CC_ClassificationType);

			AssertEquals("Lookup 2", product.ClassificationsForBinding[1].CC_LookupCode);
			AssertEquals("Descr 2", product.ClassificationsForBinding[1].CC_Description);
			AssertEquals("1601000005", product.ClassificationsForBinding[1].CC_TariffNum.ExcludeChars(" ."));
			AssertNotEquals(ZString.Empty, product.ClassificationsForBinding[1].CC_RN_NKCountryCode);
			AssertEquals(BaseCusClassification.ClassificationType.EXP, product.ClassificationsForBinding[1].CC_ClassificationType);

			product.ClassificationsForBinding.DeleteAll();
			xsdProduct.Customs.Classifications[0].LookupCode.Value = "";

			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);
			AssertEquals(2, product.ClassificationsForBinding.Count);
			AssertEquals(32, product.ClassificationsForBinding[0].CC_LookupCode.Length);
		}

		public void TestRemovePivotsWhenRegistryIsNo()
		{
			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var product = GetNewPopulatedProductWithLookup();
			var originalPivotPK = product.PivotsForBinding[0];

			var xsdProduct = MakeExport(product);
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);
			AssertNotEquals("Another Pivot has been created", originalPivotPK, product.PivotsForBinding[0].PK);
		}

		public void TestMatchProduct()
		{
			var product = (OrgSupplierPart)GetNewPopulatedProduct();
			product.OP_PartNum = "Test Part";
			product.OP_Desc = "Test descr";

			classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "Lookup 1";
			classification.CC_Description = "Descr 1";
			classification.CC_TariffNum = "1601.00.00 01";
			classification.CC_ClassificationType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.IMP;
			product.ClassificationsForBinding.Add(classification);

			product.BillOfMaterials.First().Component.Delete();
			product.BillOfMaterials.DeleteAll();
			var pivot = product.PivotsForBinding[0];
			pivot.CI_OH = product.RelatedOrganisations[0].OU_OH;
			pivot.CI_ChildType = "IMP";
			var originalPivotPK = pivot.PK;
			Factory.Save();
			var xsdProduct = MakeExport(product);
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);
			AssertEquals("Pivot should be matched", 1, product.PivotsForBinding.Count);
			AssertEquals("Pivot should be matched", originalPivotPK, product.PivotsForBinding[0].PK);

			var product2 = (OrgSupplierPart)GetNewPopulatedProduct();
			product2.OP_PartNum = "900560";
			product2.OP_Desc = "TEST";
			product2.ClassificationsForBinding.Add(classification);

			pivot = product2.PivotsForBinding[0];
			pivot.CI_ChildType = "IMP";
			pivot.CI_OH = product.RelatedOrganisations[0].OU_OH;
			xsdProduct = MakeExport(product2);
			pivot.CI_OH = ZGuid.Empty;
			pivot.CI_CC = ZGuid.Empty;
			pivot.CI_TariffNum = "1601.00.00 01";
			Factory.Save();

			ProductDataAdapter.ImportFromValueObject(product2, xsdProduct, Context);
			AssertEquals("Pivot cannot be matched and added", 2, product2.PivotsForBinding.Count);
		}

		public void TestIsClassificationTypeMatch()
		{
			//product 1
			var product = (OrgSupplierPart)GetNewPopulatedProduct();
			product.OP_PartNum = "Test Part";
			product.OP_Desc = "Test descr";
			product.ClassificationsForBinding.Add(CreateClassification("Lookup 1", "Descr 1", "1601.00.00 01", BaseCusClassification.ClassificationType.IMP));

			var pivot = product.PivotsForBinding[0];
			pivot.CI_ChildType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.IMP;
			pivot.CI_OH = product.RelatedOrganisations[0].OU_OH;
			Factory.Save();

			var xsdProduct = MakeExport(product);
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);
			AssertEquals("Pivot matched", pivot, product.PivotsForBinding[0]);

			//product 2
			var product2 = (OrgSupplierPart)GetNewPopulatedProduct();
			product2.OP_PartNum = "PRODUCT2";
			product2.OP_Desc = "Test 2";
			product2.ClassificationsForBinding.Add(CreateClassification("Lookup 2", "Descr 2", "", BaseCusClassification.ClassificationType.IMP));
			product2.BillOfMaterials.First().Component.OP_PartNum = "BOM PRODUCT2";

			var pivot2 = product2.PivotsForBinding[0];
			pivot2.CI_CC = product2.ClassificationsForBinding[0].PK;
			pivot2.CI_OH = product2.RelatedOrganisations[0].OU_OH;
			pivot2.CI_ChildType = ZString.Empty;
			Factory.Save();
			AssertEquals("Precondition: C_ChildType", ZString.Empty, pivot2.CI_ChildType);

			xsdProduct = MakeExport(product2);
			product2.ClassificationsForBinding[0].CC_TariffNum = "1601.00.00 05";
			Factory.Save();
			ProductDataAdapter.ImportFromValueObject(product2, xsdProduct, Context);
			AssertEquals("Pivot matched", pivot2, product2.PivotsForBinding[0]);

			//product 3
			var product3 = (OrgSupplierPart)GetNewPopulatedProduct();
			product3.OP_PartNum = "PRODUCT3";
			product3.OP_Desc = "Test 3";
			product3.ClassificationsForBinding.Add(CreateClassification("Lookup 3", "Descr 3", "4602.00.00 01", BaseCusClassification.ClassificationType.EXP));
			product3.BillOfMaterials.First().Component.OP_PartNum = "BOM PRODUCT3";

			var pivot3 = product3.PivotsForBinding[0];
			pivot3.CI_CC = product3.ClassificationsForBinding[0].PK;
			pivot3.CI_OH = product3.RelatedOrganisations[0].OU_OH;
			Factory.Save();

			xsdProduct = MakeExport(product3);
			ProductDataAdapter.ImportFromValueObject(product3, xsdProduct, Context);
			AssertEquals("Pivot matched", pivot3, product3.PivotsForBinding[0]);

			//product 4
			var product4 = (OrgSupplierPart)GetNewPopulatedProduct();
			product4.OP_PartNum = "PRODUCT4";
			product4.OP_Desc = "Test 4";
			product4.ClassificationsForBinding.Add(CreateClassification("Lookup 4", "Descr 4", "", BaseCusClassification.ClassificationType.Both));
			product4.BillOfMaterials.First().Component.OP_PartNum = "BOM PRODUCT4";

			var pivot4 = product4.PivotsForBinding[0];
			pivot4.CI_CC = product4.ClassificationsForBinding[0].PK;
			pivot4.CI_OH = product4.RelatedOrganisations[0].OU_OH;
			Factory.Save();

			xsdProduct = MakeExport(product4);
			ProductDataAdapter.ImportFromValueObject(product4, xsdProduct, Context);
			AssertEquals("Pivot matched", pivot4, product4.PivotsForBinding[0]);
		}

		public void TestExportCustomsProduct()
		{
			OrgSupplierPart product = GetNewPopulatedProductWithLookup();
			Xsd.Product xsdProduct = MakeExport(product);

			AssertEquals("TEST PART", xsdProduct.ProductCode);
			AssertEquals("Test descr", xsdProduct.ProductDescription);

			Xsd.ClassificationCollection xmlLookups = xsdProduct.Customs.Classifications;

			AssertEquals("Lookup 1", xmlLookups[0].LookupCode.Value);
			AssertEquals("IMP", xmlLookups[0].LookupCode.LookupType);
			AssertEquals("Descr 1", xmlLookups[0].Description);
			AssertEquals("1601000001", xmlLookups[0].Tariff.ExcludeChars(" ."));
			AssertEquals(MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode, xmlLookups[0].CountryCode);
			AssertEquals(Business.ClassificationTypeList.Codes.HTI, xmlLookups[0].ClassificationType);

			AssertEquals("Lookup 2", xmlLookups[1].LookupCode.Value);
			AssertEquals("Descr 2", xmlLookups[1].Description);
			AssertEquals("1601000005", xmlLookups[1].Tariff.ExcludeChars(" ."));
			AssertEquals(MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode, xmlLookups[1].CountryCode);
			AssertEquals(Enterprise.Customs.Business.BaseCusClassification.ClassificationType.EXP, xmlLookups[1].ClassificationType);
		}

		public OrgSupplierPart GetNewPopulatedProductWithLookup()
		{
			OrgSupplierPart product = (OrgSupplierPart)GetNewPopulatedProduct();
			product.OP_PartNum = "Test Part";
			product.OP_Desc = "Test descr";

			classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "Lookup 1";
			classification.CC_Description = "Descr 1";
			classification.CC_TariffNum = "1601.00.00 01";
			classification.CC_ClassificationType = Common.ClassificationType.IMP;
			product.ClassificationsForBinding.Add(classification);
			product.PivotsForBinding[0].CI_ChildType = Business.ClassificationTypeList.Codes.HTI;

			classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "Lookup 2";
			classification.CC_Description = "Descr 2";
			classification.CC_TariffNum = "1601.00.00 05";
			classification.CC_ClassificationType = Common.ClassificationType.EXP;
			product.ClassificationsForBinding.Add(classification);
			product.PivotsForBinding[1].CI_ChildType = ZString.Empty;

			return product;
		}
		BaseCusClassification classification;

		BaseCusClassification CreateClassification(ZString code, ZString descr, ZString tariff, ZString type)
		{
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = code;
			classification.CC_Description = descr;
			classification.CC_TariffNum = tariff;
			classification.CC_ClassificationType = type;
			return classification;
		}
	}
}
