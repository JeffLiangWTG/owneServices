using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using OrgSupplierPart = Enterprise.Customs.US.Business.OrgSupplierPart;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(USProductValueObjectDataAdapter))]
	sealed class USProductValueObjectDataAdapterTest : CustomsProductValueObjectDataAdapterTest
	{
		public void TestImportCustomsProductClassification()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test Part";
			product.OP_Desc = "Test descr";
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "Lookup 1";
			classification.CC_Description = "Descr 1";
			classification.CC_TariffNum = "1601.00.00 01";
			classification.CC_ClassificationType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.IMP;
			CusClassPartPivot pivot = product.PivotsForBinding.AddNew();
			pivot.CI_CC = classification.PK;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			GetProductClassificationLevelDetails(product);
			Xsd.Product xsdProduct = MakeExport(product);
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);
			AssertEquals(1, product.PivotsForBinding.Count);
			pivot = product.PivotsForBinding[0];
			AssertEquals(1, pivot.PGAs.Count);
			AssertEquals("IT", pivot.CD_UC_NKCountryOfOrigin);
			AssertEquals(false, pivot.CD_NAFTANetCost);
			AssertEquals("125", pivot.CD_AgricultureLicenceNo);
			AssertEquals("cert", pivot.CD_SugarCertificate);
			AssertEquals("126", pivot.CD_CBTPACertificate);
			AssertEquals("127", pivot.CD_MiscLicenceNo);
			AssertEquals("131", pivot.CD_WoolLicenceNo);
			AssertEquals(YesNoDefaultList.Codes.No, pivot.CD_CottonFeeExempt);
			AssertEquals("cotton", pivot.CD_CottonCertificate);
			AssertEquals("A345GF", pivot.CD_ADDCaseNo);
			AssertEquals("C45RTG", pivot.CD_CVDCaseNo);
			AssertEquals("165", pivot.CD_RulingNumber);
			AssertEquals("R", pivot.CD_RulingType);
			AssertEquals("A", pivot.CD_ProductClaim);
			AssertEquals("Z", pivot.CD_SPI);
			AssertEquals(12.6m, pivot.CD_ActiveIngredientPercentage);
			AssertEquals("P", pivot.CD_ZoneStatus);
			AssertEquals("+", pivot.CD_TSCAIndicator);
			AssertEquals(3.5m, pivot.CD_TaxRate);
			AssertEquals(TaxApplyList.Codes.Override, pivot.CD_TaxApplicability);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, pivot.CD_TaxCode);
			OGADataTransferToolTest.AssertLaceyActDetails(pivot.PGAs[0]);
		}

		public override void TestImportProductForLookupTariff()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test Part";
			product.OP_Desc = "Test descr";
			CusClassPartPivot pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1601.00.00 01";
			Xsd.Product xsdProduct = MakeExport(product);
			pivot.Delete();
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);
			AssertEquals(0, product.ClassificationsForBinding.Count);
			AssertEquals(1, product.PivotsForBinding.Count);
			pivot = product.PivotsForBinding[0];
			AssertEquals("1601000001", pivot.CI_TariffNum);
			AssertEquals(ClassificationTypeList.Codes.HTI, pivot.CI_ChildType);
		}

		public void TestImportExportChildren()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test Part";
			product.OP_Desc = "Test descr";
			CusClassPartPivot pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1601.00.00 01";
			pivot.CI_SupplementalTariff = "9802008068";
			pivot.CD_9802USDValuePerUnit = 21m;
			pivot.CD_AMMVPerUnit = 22m;
			pivot.CD_PerUnitCost = 23m;
			pivot.CD_RX_NKPerUnitCostCurr = "NZD";
			pivot.CD_9802ValuePerUnit = 24m;
			pivot.CD_RX_NK9802ValuePerUnitCurr = "GBP";
			CusClassPartPivot childPivot = pivot.Children.AddNew();
			childPivot.CI_TariffNum = "9801001010";
			childPivot.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			childPivot.CD_9802USDValuePerUnit = 11m;
			childPivot.CD_AMMVPerUnit = 12m;
			childPivot.CD_PerUnitCost = 13m;
			childPivot.CD_RX_NKPerUnitCostCurr = "CAD";
			childPivot.CD_9802ValuePerUnit = 16m;
			childPivot.CD_RX_NK9802ValuePerUnitCurr = "EUR";
			childPivot.CD_GrossWeight = 15m;
			childPivot.CD_NetWeight = 14m;
			childPivot.CD_WeightUQ = Core.Constants.Weight.Hectograms;
			Xsd.Product xsdProduct = MakeExport(product);
			pivot.Delete();
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);
			AssertEquals(1, product.PivotsForBinding.Count);
			pivot = product.PivotsForBinding[0];
			AssertEquals("1601000001", pivot.CI_TariffNum);
			AssertEquals("9802008068", pivot.CI_SupplementalTariff);
			AssertEquals(21m, pivot.CD_9802USDValuePerUnit);
			AssertEquals(22m, pivot.CD_AMMVPerUnit);
			AssertEquals(23m, pivot.CD_PerUnitCost);
			AssertEquals("NZD", pivot.CD_RX_NKPerUnitCostCurr);
			AssertEquals(24m, pivot.CD_9802ValuePerUnit);
			AssertEquals("GBP", pivot.CD_RX_NK9802ValuePerUnitCurr);
			AssertEquals(1, pivot.Children.Count);
			childPivot = pivot.Children[0];
			AssertEquals(ClassificationChildTypeList.Codes.Related, childPivot.CI_ChildType);
			AssertEquals("9801001010", childPivot.CI_TariffNum);
			AssertEquals(11m, childPivot.CD_9802USDValuePerUnit);
			AssertEquals(12m, childPivot.CD_AMMVPerUnit);
			AssertEquals(13m, childPivot.CD_PerUnitCost);
			AssertEquals("CAD", childPivot.CD_RX_NKPerUnitCostCurr);
			AssertEquals(16m, childPivot.CD_9802ValuePerUnit);
			AssertEquals("EUR", childPivot.CD_RX_NK9802ValuePerUnitCurr);
			AssertEquals(15m, childPivot.CD_GrossWeight);
			AssertEquals(14m, childPivot.CD_NetWeight);
			AssertEquals(Core.Constants.Weight.Hectograms, childPivot.CD_WeightUQ);
		}

		public void TestImportExportAttributes()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test Part";
			product.OP_Desc = "Test descr";
			OrgHeader relatedOrg = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgPartRelation partRelation = product.RelatedOrganisations.AddNew();
			partRelation.OU_OH = relatedOrg.PK;
			partRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			CusClassPartPivot pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1601.00.00 01";
			pivot.CI_OH = partRelation.OU_OH;
			Customs.Business.CusAttributeFilter attribute = pivot.Attributes1.AddNew();
			attribute.BG_AttributeValue1 = "XXX";
			attribute = pivot.Attributes2.AddNew();
			attribute.BG_AttributeValue1 = "YYY";
			attribute = pivot.Attributes3.AddNew();
			attribute.BG_AttributeValue1 = "ZZZ";
			Xsd.Product xsdProduct = MakeExport(product);
			pivot.Delete();
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);
			AssertEquals(1, product.PivotsForBinding.Count);
			pivot = product.PivotsForBinding[0];
			AssertEquals("1601000001", pivot.CI_TariffNum);
			AssertEquals(relatedOrg.PK, pivot.CI_OH);
			AssertNotNull(pivot.RelatedOrganisation);
			AssertEquals(1, pivot.Attributes1.Count);
			attribute = pivot.Attributes1[0];
			AssertEquals("XXX", attribute.BG_AttributeValue1);
			AssertEquals(1, pivot.Attributes2.Count);
			attribute = pivot.Attributes2[0];
			AssertEquals("YYY", attribute.BG_AttributeValue1);
			AssertEquals(1, pivot.Attributes3.Count);
			attribute = pivot.Attributes3[0];
			AssertEquals("ZZZ", attribute.BG_AttributeValue1);
		}

		public void TestWhenMaxLengthExceeds()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "90001A";
			var relation = product.RelatedOrganisations.AddSupplier(Supplier);
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1601.00.00 01";
			GetProductClassificationLevelDetails(product);
			pivot.CI_OH = relation.OU_OH;
			var xsdProduct = MakeExport(product);
			var usClassification = xsdProduct.Customs.Classifications[0].CountryClassifications.USClassification;
			usClassification.ProductClaim = "AA";
			usClassification.SPI = "XXXX";
			usClassification.AntiDumping.CaseNo = "A357818002+2";
			usClassification.AntiDumping.IsSpecified = true;
			usClassification.Countervailing.CaseNo = "C357818002+2";
			usClassification.Countervailing.IsSpecified = true;
			AssertNoExceptionThrown(delegate
			{
				ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);
			});
			AssertEquals(1, product.PivotsForBinding.Count);
			AssertEquals("A", pivot.CD_ProductClaim);
			AssertEquals("XXX", pivot.CD_SPI);
			AssertEquals("A357818002", pivot.CD_ADDCaseNo);
			AssertEquals("C357818002", pivot.CD_CVDCaseNo);
		}

		public void TestUpdateDetails()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "90001A";
			var relation = product.RelatedOrganisations.AddSupplier(Supplier);
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "Lookup 1";
			classification.CC_Description = "Descr 1";
			classification.CC_TariffNum = "1601.00.00 01";
			classification.CC_ClassificationType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.IMP;
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_CC = classification.PK;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			GetProductClassificationLevelDetails(product);
			pivot.CI_OH = relation.OU_OH;
			pivot.CD_SugarCertificate = ZString.Empty;
			pivot.CD_CBTPACertificate = ZString.Empty;
			var xsdProduct = MakeExport(product);
			pivot.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Andorra;
			pivot.CD_AgricultureLicenceNo = ZString.Empty;
			pivot.CD_SugarCertificate = "CA500603";
			pivot.CD_CBTPACertificate = "90564";
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);
			AssertEquals(1, product.PivotsForBinding.Count);
			pivot = product.PivotsForBinding[0];
			//pivot details should remain as they were or updated, and new details should be added
			AssertEquals(1, pivot.PGAs.Count);
			AssertEquals(Core.Constants.CountryCodes.Italy, pivot.CD_UC_NKCountryOfOrigin);
			AssertEquals(false, pivot.CD_NAFTANetCost);
			AssertEquals("125", pivot.CD_AgricultureLicenceNo);
			AssertEquals("CA500603", pivot.CD_SugarCertificate);
			AssertEquals("90564", pivot.CD_CBTPACertificate);
			AssertEquals("127", pivot.CD_MiscLicenceNo);
			AssertEquals("131", pivot.CD_WoolLicenceNo);
			AssertEquals(YesNoDefaultList.Codes.No, pivot.CD_CottonFeeExempt);
			AssertEquals("cotton", pivot.CD_CottonCertificate);
			AssertEquals("A345GF", pivot.CD_ADDCaseNo);
			AssertEquals("C45RTG", pivot.CD_CVDCaseNo);
			AssertEquals("165", pivot.CD_RulingNumber);
			AssertEquals("R", pivot.CD_RulingType);
			AssertEquals("A", pivot.CD_ProductClaim);
			AssertEquals("Z", pivot.CD_SPI);
			AssertEquals(12.6m, pivot.CD_ActiveIngredientPercentage);
			AssertEquals("P", pivot.CD_ZoneStatus);
			AssertEquals("+", pivot.CD_TSCAIndicator);
			AssertEquals(3.5m, pivot.CD_TaxRate);
			AssertEquals(TaxApplyList.Codes.Override, pivot.CD_TaxApplicability);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, pivot.CD_TaxCode);
		}

		public void TestMatchProductByAttributes()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "1001";
			product.OP_Desc = "Test descr";
			var relation = product.RelatedOrganisations.AddOwner(Factory.LoadTop1<OrgHeader>(new ZQuery()));
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1601.00.00 01";
			pivot.CI_SupplementalTariff = "9802008068";
			pivot.CD_9802USDValuePerUnit = 21m;
			pivot.CI_OH = relation.OU_OH;
			pivot.Attributes1.AddNew().BG_AttributeValue1 = "11";
			pivot.Attributes1.AddNew().BG_AttributeValue1 = "12";
			pivot.Attributes2.AddNew().BG_AttributeValue1 = "160";
			pivot.Attributes2.AddNew().BG_AttributeValue1 = "TEST";
			pivot.Attributes3.AddNew().BG_AttributeValue1 = "188";
			pivot.Attributes3.AddNew().BG_AttributeValue1 = "189";
			pivot.Attributes3.AddNew().BG_AttributeValue1 = "209";
			AssertEquals(2, pivot.Attributes1.Count);
			AssertEquals(2, pivot.Attributes2.Count);
			AssertEquals(3, pivot.Attributes3.Count);
			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_TariffNum = "340102001";
			pivot2.CI_OH = relation.OU_OH;
			pivot2.Attributes1.AddNew().BG_AttributeValue1 = "15";
			pivot2.Attributes3.AddNew().BG_AttributeValue1 = "20";
			AssertEquals(1, pivot2.Attributes1.Count);
			AssertEquals(0, pivot2.Attributes2.Count);
			AssertEquals(1, pivot2.Attributes3.Count);
			Factory.Save();
			var xsdProduct = MakeExport(product);
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);
			AssertEquals("Pivots should be found and updated", 2, product.PivotsForBinding.Count);
			AssertNotNull(product.PivotsForBinding.FindByPK(pivot.PK));
			AssertNotNull(product.PivotsForBinding.FindByPK(pivot2.PK));
			AssertEquals(2, pivot.Attributes1.Count);
			AssertEquals(2, pivot.Attributes2.Count);
			AssertEquals(3, pivot.Attributes3.Count);
			AssertEquals(1, pivot2.Attributes1.Count);
			AssertEquals(0, pivot2.Attributes2.Count);
			AssertEquals(1, pivot2.Attributes3.Count);
			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "10022";
			relation = product2.RelatedOrganisations.AddSupplier(Factory.LoadTop1<OrgHeader>(new ZQuery()));
			var product2_pivot1 = product2.PivotsForBinding.AddNew();
			product2_pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			product2_pivot1.CI_TariffNum = "6405.00.00 01";
			product2_pivot1.CI_OH = relation.OU_OH;
			product2_pivot1.Attributes1.AddNew().BG_AttributeValue1 = "11";
			product2_pivot1.Attributes1.AddNew().BG_AttributeValue1 = "12";
			product2_pivot1.Attributes2.AddNew().BG_AttributeValue1 = "160";
			product2_pivot1.Attributes2.AddNew().BG_AttributeValue1 = "TEST";
			product2_pivot1.Attributes3.AddNew().BG_AttributeValue1 = "188";
			product2_pivot1.Attributes3.AddNew().BG_AttributeValue1 = "189";
			product2_pivot1.Attributes3.AddNew().BG_AttributeValue1 = "209";
			AssertEquals(2, product2_pivot1.Attributes1.Count);
			AssertEquals(2, product2_pivot1.Attributes2.Count);
			AssertEquals(3, product2_pivot1.Attributes3.Count);
			var product2_pivot2 = product2.PivotsForBinding.AddNew();
			product2_pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			product2_pivot2.CI_TariffNum = "340102001";
			product2_pivot2.CI_OH = relation.OU_OH;
			product2_pivot2.Attributes2.AddNew().BG_AttributeValue1 = "30";
			AssertEquals(0, product2_pivot2.Attributes1.Count);
			AssertEquals(1, product2_pivot2.Attributes2.Count);
			AssertEquals(0, product2_pivot2.Attributes3.Count);
			xsdProduct = MakeExport(product2);
			product2_pivot2.Attributes3.AddNew().BG_AttributeValue1 = "40";
			product2_pivot1.Attributes3.DeleteAll();
			ProductDataAdapter.ImportFromValueObject(product2, xsdProduct, Context);
			AssertEquals("Pivots cannot be matched, because attributes are different. New Pivots created.", 4, product2.PivotsForBinding.Count);
		}

		public void TestExportLevelClassificationDetails()
		{
			OrgSupplierPart product = (OrgSupplierPart)GetNewPopulatedProductWithLookup();
			product.PivotsForBinding[0].CI_ChildType = ClassificationTypeList.Codes.HTI;
			product.PivotsForBinding[1].CI_ChildType = ClassificationTypeList.Codes.SHB;
			GetProductClassificationLevelDetails(product);
			Xsd.Product xsdProduct = MakeExport(product);
			Xsd.ClassificationCollection xmlLookups = xsdProduct.Customs.Classifications;
			Xsd.Classification xmlLookup = xmlLookups[0];
			Xsd.ClassificationCountryClassifications levelDetailsCollection = xmlLookup.CountryClassifications;
			Xsd.USProductClassification levelDetails = levelDetailsCollection.USClassification;
			AssertEquals("IT", levelDetails.CountryOfOrigin);
			AssertEquals("Test Org", ((Xsd.Organisation)levelDetails.Manufacturer.Item).OrganisationDetails.Name);
			AssertEquals(Xsd.TrueFalse.@false, levelDetails.NAFTANetCost);
			AssertEquals(true, levelDetails.NAFTANetCostSpecified);
			AssertEquals("125", levelDetails.PermitsLicenses.AgricultureLicNo);
			AssertEquals("cert", levelDetails.PermitsLicenses.CAExportCertificate);
			AssertEquals("126", levelDetails.PermitsLicenses.CBTPACertificate);
			AssertEquals("127", levelDetails.PermitsLicenses.MiscPermitNo);
			AssertEquals("131", levelDetails.PermitsLicenses.WoolLicenceNo);
			AssertEquals("cotton", levelDetails.PermitsLicenses.CottonCertificateNo);
			AssertEquals(YesNoDefaultList.Codes.No, levelDetails.PermitsLicenses.CottonFeeExemptIndicator);
			AssertEquals("A345GF", levelDetails.AntiDumping.CaseNo);
			AssertEquals("C45RTG", levelDetails.Countervailing.CaseNo);
			AssertEquals("165", levelDetails.PIRPRuling.Number);
			AssertEquals("R", levelDetails.PIRPRuling.Type);
			AssertEquals("A", levelDetails.ProductClaim);
			AssertEquals("Z", levelDetails.SPI);
			AssertEquals(12.6m, levelDetails.PercentageOfActiveIngredient);
			AssertEquals("P", ZoneStatusToXmlCodeMappings.Instance.GetEnterpriseCode(levelDetails.ZoneStatus.ToString(), "", Context));
			AssertEquals("+", levelDetails.TSCAIndicator);
			AssertEquals(true, levelDetails.OverriddenTaxRateSpecified);
			AssertEquals(3.5m, levelDetails.OverriddenTaxRate);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, USProductClassificationTaxCodeMappings.Instance.GetEnterpriseCode(levelDetails.TaxCode, "", null));
			AssertEquals(OGAIndicatorList.Codes.Declared, OGAIndicatorToXmlCodeMappings.Instance.GetEnterpriseCode(levelDetails.OGAIndicators.DOTIndicator.ToString(), "", Context));
			AssertEquals(OGAIndicatorList.Codes.Declared, OGAIndicatorToXmlCodeMappings.Instance.GetEnterpriseCode(levelDetails.OGAIndicators.FCCIndicator.ToString(), "", Context));
			AssertEquals(OGAIndicatorList.Codes.Declared, OGAIndicatorToXmlCodeMappings.Instance.GetEnterpriseCode(levelDetails.OGAIndicators.FDAIndicator.ToString(), "", Context));
			OGADataTransferToolTest.AssertXmlLaceyActDetails(levelDetails.LaceyActDetails[0]);
			AssertEquals(0, levelDetails.LaceyActDetails[0].Value);
		}

		protected override string TestingCountry => Core.Constants.CountryCodes.UnitedStates;

		void GetProductClassificationLevelDetails(OrgSupplierPart product)
		{
			var organisationAccessed = Organization;
			var supplierAccessed = Supplier;
			Factory.Save();
			var pivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CD_OA_Manufacturer = Organization.Addresses[0].PK;
			pivot.CD_UC_NKCountryOfOrigin = "IT";
			pivot.CD_NAFTANetCost = false;
			pivot.CD_AgricultureLicenceNo = "125";
			pivot.CD_CottonCertificate = "cotton";
			pivot.CD_CottonFeeExempt = YesNoDefaultList.Codes.No;
			pivot.CD_SugarCertificate = "cert";
			pivot.CD_CBTPACertificate = "126";
			pivot.CD_MiscLicenceNo = "127";
			pivot.CD_WoolLicenceNo = "131";
			pivot.CD_ADDCaseNo = "A345GF";
			pivot.CD_CVDCaseNo = "C45RTG";
			pivot.CD_RulingNumber = "165";
			pivot.CD_RulingType = "R";
			pivot.CD_ProductClaim = "A";
			pivot.CD_SPI = "Z";
			pivot.CD_ActiveIngredientPercentage = 12.6m;
			pivot.CD_ZoneStatus = "P";
			pivot.CD_TSCAIndicator = "+";
			pivot.CD_TaxApplicability = TaxApplyList.Codes.Override;
			pivot.CD_TaxCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			pivot.CD_TaxRate = 3.5m;
			var pga = pivot.PGAs.AddNew();
			OGADataTransferToolTest.GetLaceyActDetals(pga);
		}
	}
}
