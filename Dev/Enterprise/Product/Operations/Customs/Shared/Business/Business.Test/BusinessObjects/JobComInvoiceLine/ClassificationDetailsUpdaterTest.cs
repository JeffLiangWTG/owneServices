using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ClassificationDetailsUpdaterTest : TestCaseWithFactory
	{
		public void TestConvertedUnitDelegation()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "~~~";
			var partApple = Factory.NewWithValidTestData<OrgSupplierPart>();
			partApple.FillWithValidTestData();
			partApple.OP_PartNum = "APPLE";
			partApple.OP_StockKeepingUnit = Core.Constants.PkgUnit.Package;
			partApple.OP_Desc = "Granny smith";
			partApple.RelatedOrganisations.RemoveAndDeleteAll();
			partApple.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			var classificationGranny = Factory.NewWithValidTestData<BaseCusClassification>();
			var appleGrannyPivot = Factory.New<BaseCusClassPartPivot>();
			appleGrannyPivot.CI_CC = classificationGranny.PK;
			appleGrannyPivot.CI_OP = partApple.PK;
			classificationGranny.CC_TariffNum = "0000.00.00 1";
			classificationGranny.CC_LookupCode = "GRANNY";
			classificationGranny.CC_ClassificationType = "BTH";
			Factory.Save();

			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_OH_Importer = importer.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var updater = new ClassificationDetailsUpdater(invoiceLine);
			invoiceLine.JI_PartNo = "APPLE";
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			updater.UpdateWhenJI_PartNoIsSet();
			AssertEquals(Core.Constants.PkgUnit.Package, invoiceLine.JI_InvoiceUQ);

			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			invoiceLine.GetConvertedStockUnit = x => Core.Constants.PkgUnit.Box;
			Factory.InvalidateCachedProperties();
			updater.UpdateWhenJI_PartNoIsSet();
			AssertEquals(Core.Constants.PkgUnit.Box, invoiceLine.JI_InvoiceUQ);
		}

		public void TestRefreshDetailsIfPartHasBeenCreatedSinceCodeWasEntered()
		{
			BusinessObjectFactory pFactory = new BusinessObjectFactory();
			OrgSupplierPart partApple = Factory.New<OrgSupplierPart>();
			OrgHeader importer = pFactory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "~~~";
			partApple.FillWithValidTestData();
			partApple.OP_PartNum = "APPLE";
			partApple.OP_Desc = "Granny smith";
			partApple.RelatedOrganisations.RemoveAndDeleteAll();
			partApple.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			BaseCusClassification classificationGranny = Factory.New<BaseCusClassification>();
			classificationGranny.FillWithValidTestData();
			BaseCusClassPartPivot appleGrannyPivot = Factory.New<BaseCusClassPartPivot>();
			appleGrannyPivot.CI_CC = classificationGranny.PK;
			appleGrannyPivot.CI_OP = partApple.PK;
			classificationGranny.CC_TariffNum = "0000.00.00 1";
			classificationGranny.CC_LookupCode = "GRANNY";
			classificationGranny.CC_ClassificationType = "BTH";
			pFactory.Save();

			BaseJobDeclaration dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			dec.JE_OH_Importer = importer.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;

			BaseJobComInvoiceLine line1 = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			line1.JI_PartNo = "NOODLE";
			line1.JI_InvoiceQuantity = 44;
			AssertNull("No noodle part", line1.Part);

			BaseJobComInvoiceLine line2 = line1.InvoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_PartNo = "";
			line2.JI_InvoiceQuantity = 32;
			line2.JI_Description = "Empty";

			BaseJobComInvoiceLine line3 = line1.InvoiceHeader.JobComInvoiceLines.AddNew();
			line3.JI_PartNo = "APPLE";
			line3.JI_InvoiceQuantity = 10;
			AssertEquals(partApple.PK, line3.JI_OP);
			AssertEquals("0000.00.00 1", line3.JI_Tariff);

			BaseJobComInvoiceLine line4 = line1.InvoiceHeader.JobComInvoiceLines.AddNew();
			line4.JI_CC = classificationGranny.PK;
			AssertEquals("0000.00.00 1", line4.JI_Tariff);
			line4.JI_InvoiceQuantity = 34;

			BaseJobComInvoiceLine line5 = line1.InvoiceHeader.JobComInvoiceLines.AddNew();
			line5.JI_Tariff = "0000.00.00 2";
			AssertEquals("0000.00.00 2", line5.JI_Tariff);
			line5.JI_InvoiceQuantity = 5;

			BaseJobComInvoiceLine line6 = line1.InvoiceHeader.JobComInvoiceLines.AddNew();
			line6.JI_PartNo = "NOODLE";
			line6.JI_CC = classificationGranny.PK;
			AssertEquals("0000.00.00 1", line6.JI_Tariff);
			line6.JI_InvoiceQuantity = 96;

			BaseJobComInvoiceLine line7 = line1.InvoiceHeader.JobComInvoiceLines.AddNew();
			line7.JI_PartNo = "NOODLE";
			line7.JI_Tariff = "0000.00.00 9";
			AssertEquals("0000.00.00 9", line7.JI_Tariff);
			line7.JI_InvoiceQuantity = 97;

			BaseJobComInvoiceLine line8 = line1.InvoiceHeader.JobComInvoiceLines.AddNew();
			line8.JI_PartNo = "BLAH";
			line8.JI_CustomsQuantity = 5;

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			OrgSupplierPart part = factory2.New<OrgSupplierPart>();
			part.OP_PartNum = "NOODLE";
			part.OP_Desc = "Oodles of noodles";
			part.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			BaseCusClassification classification = factory2.New<BaseCusClassification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = "BTH";
			BaseCusClassPartPivot pivot = factory2.New<BaseCusClassPartPivot>();
			pivot.CI_CC = classification.PK;
			pivot.CI_OP = part.PK;
			classification.CC_TariffNum = "0000.00.00 3";
			factory2.Save();

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			BaseJobDeclaration reloadedDec = factory3.Load<BaseJobDeclaration>(dec.PK);
			reloadedDec.FilteredInvoiceLines.Sort(JobComInvoiceLineSchema.Constants.JI_LineNo, System.ComponentModel.ListSortDirection.Ascending);
			BaseJobComInvoiceLine reloadedLine1 = reloadedDec.FilteredInvoiceLines[0];
			BaseJobComInvoiceLine reloadedLine2 = reloadedDec.FilteredInvoiceLines[1];
			BaseJobComInvoiceLine reloadedLine3 = reloadedDec.FilteredInvoiceLines[2];
			BaseJobComInvoiceLine reloadedLine4 = reloadedDec.FilteredInvoiceLines[3];
			BaseJobComInvoiceLine reloadedLine5 = reloadedDec.FilteredInvoiceLines[4];
			BaseJobComInvoiceLine reloadedLine6 = reloadedDec.FilteredInvoiceLines[5];
			BaseJobComInvoiceLine reloadedLine7 = reloadedDec.FilteredInvoiceLines[6];
			BaseJobComInvoiceLine reloadedLine8 = reloadedDec.FilteredInvoiceLines[7];

			AssertNotNull("Noodle part has been automatically loaded", reloadedLine1.Part);
			AssertEquals("OODLES OF NOODLES", reloadedLine1.JI_Description);
			AssertEquals("NOODLE", reloadedLine1.JI_PartNo);
			AssertEquals(classification.PK, reloadedLine1.JI_CC);
			AssertEquals("0000.00.00 3", reloadedLine1.JI_Tariff);
			AssertEquals(44m, reloadedLine1.JI_InvoiceQuantity);

			AssertEquals("Empty", reloadedLine2.JI_Description);
			AssertEquals("", reloadedLine2.JI_PartNo);
			AssertEquals(32m, reloadedLine2.JI_InvoiceQuantity);
			AssertNull(reloadedLine2.Part);

			AssertEquals("APPLE", reloadedLine3.JI_PartNo);
			AssertEquals(10m, reloadedLine3.JI_InvoiceQuantity);
			AssertEquals(partApple.PK, reloadedLine3.JI_OP);
			AssertEquals("0000.00.00 1", reloadedLine3.JI_Tariff);
			AssertEquals(classificationGranny.PK, reloadedLine3.JI_CC);

			AssertEquals("", reloadedLine4.JI_PartNo);
			AssertEquals(34m, reloadedLine4.JI_InvoiceQuantity);
			AssertEquals(Guid.Empty, reloadedLine4.JI_OP);
			AssertEquals("0000.00.00 1", reloadedLine4.JI_Tariff);
			AssertEquals(classificationGranny.PK, reloadedLine4.JI_CC);

			AssertEquals("", reloadedLine5.JI_PartNo);
			AssertEquals(5m, reloadedLine5.JI_InvoiceQuantity);
			AssertEquals(Guid.Empty, reloadedLine5.JI_OP);
			AssertEquals("0000.00.00 2", reloadedLine5.JI_Tariff);
			AssertEquals(Guid.Empty, reloadedLine5.JI_CC);

			AssertEquals("NOODLE", reloadedLine6.JI_PartNo);
			AssertEquals(96m, reloadedLine6.JI_InvoiceQuantity);
			AssertEquals(part.PK, reloadedLine6.JI_OP);
			AssertEquals("0000.00.00 1", reloadedLine6.JI_Tariff);
			AssertEquals(classificationGranny.PK, reloadedLine6.JI_CC);

			AssertEquals("NOODLE", reloadedLine7.JI_PartNo);
			AssertEquals(97m, reloadedLine7.JI_InvoiceQuantity);
			AssertEquals(part.PK, reloadedLine7.JI_OP);
			AssertEquals("0000.00.00 9", reloadedLine7.JI_Tariff);
			AssertEquals(Guid.Empty, reloadedLine7.JI_CC);

			AssertEquals("BLAH", reloadedLine8.JI_PartNo);
			AssertEquals(5m, reloadedLine8.JI_CustomsQuantity);
			AssertEquals(Guid.Empty, reloadedLine8.JI_OP);
			AssertEquals(Guid.Empty, reloadedLine8.JI_CC);
		}

		public void TestSelectPartCodeWithClassificationEXPAndBTH_IfImportDeclaration()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC";

			var supplierPart1 = Factory.New<OrgSupplierPart>();
			supplierPart1.FillWithValidTestData();
			supplierPart1.OP_PartNum = "PILLOW CASE";
			supplierPart1.OP_Desc = "PILLOW CASE";
			supplierPart1.RelatedOrganisations.RemoveAndDeleteAll();
			supplierPart1.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			var classificationPillowCase = Factory.New<BaseCusClassification>();
			classificationPillowCase.FillWithValidTestData();
			classificationPillowCase.CC_LookupCode = "PILLOW CASES";
			classificationPillowCase.CC_ClassificationType = ClassificationType.EXP;

			var pillowCasePivot = Factory.New<BaseCusClassPartPivot>();
			pillowCasePivot.CI_CC = classificationPillowCase.PK;
			pillowCasePivot.CI_OP = supplierPart1.PK;
			pillowCasePivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pillowCasePivot.CI_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			pillowCasePivot.CI_TariffNum = "1111.11.11.11F";

			var classificationPillowCase2 = Factory.New<BaseCusClassification>();
			classificationPillowCase2.FillWithValidTestData();
			classificationPillowCase2.CC_LookupCode = "PILLOW CASES";
			classificationPillowCase2.CC_ClassificationType = ClassificationType.Both;

			var pillowCasePivot2 = Factory.New<BaseCusClassPartPivot>();
			pillowCasePivot2.CI_CC = classificationPillowCase2.PK;
			pillowCasePivot2.CI_OP = supplierPart1.PK;
			pillowCasePivot2.CI_ChildType = ClassificationTypeList.Codes.HTB;
			pillowCasePivot2.CI_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			pillowCasePivot2.CI_TariffNum = "2222.22.22.22F";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_PartNo = "PILLOW CASE";

			AssertEquals("2222.22.22.22F", line1.JI_Tariff);
		}

		public void TestSelectPartCodeWithClassificationIMPAndEXP_IfImportDeclaration()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC";

			var supplierPart1 = Factory.New<OrgSupplierPart>();
			supplierPart1.FillWithValidTestData();
			supplierPart1.OP_PartNum = "PILLOW CASE";
			supplierPart1.OP_Desc = "PILLOW CASE";
			supplierPart1.RelatedOrganisations.RemoveAndDeleteAll();
			supplierPart1.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			var classificationPillowCase = Factory.New<BaseCusClassification>();
			classificationPillowCase.FillWithValidTestData();
			classificationPillowCase.CC_LookupCode = "PILLOW CASES";
			classificationPillowCase.CC_ClassificationType = ClassificationType.IMP;

			var pillowCasePivot = Factory.New<BaseCusClassPartPivot>();
			pillowCasePivot.CI_CC = classificationPillowCase.PK;
			pillowCasePivot.CI_OP = supplierPart1.PK;
			pillowCasePivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pillowCasePivot.CI_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			pillowCasePivot.CI_TariffNum = "1111.11.11.11F";

			var classificationPillowCase2 = Factory.New<BaseCusClassification>();
			classificationPillowCase2.FillWithValidTestData();
			classificationPillowCase2.CC_LookupCode = "PILLOW CASES";
			classificationPillowCase2.CC_ClassificationType = ClassificationType.EXP;

			var pillowCasePivot2 = Factory.New<BaseCusClassPartPivot>();
			pillowCasePivot2.CI_CC = classificationPillowCase2.PK;
			pillowCasePivot2.CI_OP = supplierPart1.PK;
			pillowCasePivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pillowCasePivot2.CI_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			pillowCasePivot2.CI_TariffNum = "2222.22.22.22F";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_PartNo = "PILLOW CASE";

			AssertEquals("1111.11.11.11F", line1.JI_Tariff);
		}

		public void TestSelectPartCodeWithClassificationIMPAndBTH_IfExportDeclaration()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC";

			var supplierPart1 = Factory.New<OrgSupplierPart>();
			supplierPart1.FillWithValidTestData();
			supplierPart1.OP_PartNum = "Game";
			supplierPart1.OP_Desc = "TOY GAMES";
			supplierPart1.RelatedOrganisations.RemoveAndDeleteAll();
			supplierPart1.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			var classificationGame = Factory.New<BaseCusClassification>();
			classificationGame.FillWithValidTestData();
			classificationGame.CC_LookupCode = "Games";
			classificationGame.CC_ClassificationType = ClassificationType.IMP;

			var gamePivot = Factory.New<BaseCusClassPartPivot>();
			gamePivot.CI_CC = classificationGame.PK;
			gamePivot.CI_OP = supplierPart1.PK;
			gamePivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			gamePivot.CI_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gamePivot.CI_TariffNum = "3926.90.69.70I";

			var classificationGame2 = Factory.New<BaseCusClassification>();
			classificationGame2.FillWithValidTestData();
			classificationGame2.CC_LookupCode = "Games";
			classificationGame2.CC_ClassificationType = ClassificationType.Both;

			var gamePivot2 = Factory.New<BaseCusClassPartPivot>();
			gamePivot2.CI_CC = classificationGame2.PK;
			gamePivot2.CI_OP = supplierPart1.PK;
			gamePivot2.CI_ChildType = ClassificationTypeList.Codes.HTB;
			gamePivot2.CI_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gamePivot2.CI_TariffNum = "3926.90.69.70B";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_PartNo = "Game";

			AssertEquals("3926.90.69.70B", line1.JI_Tariff);
		}

		public void TestSelectPartCodeWithClassificationIMPAndEXP_IfExportDeclaration()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC";

			var supplierPart1 = Factory.New<OrgSupplierPart>();
			supplierPart1.FillWithValidTestData();
			supplierPart1.OP_PartNum = "Game";
			supplierPart1.OP_Desc = "TOY GAMES";
			supplierPart1.RelatedOrganisations.RemoveAndDeleteAll();
			supplierPart1.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			var classificationGame = Factory.New<BaseCusClassification>();
			classificationGame.FillWithValidTestData();
			classificationGame.CC_LookupCode = "Games";
			classificationGame.CC_ClassificationType = ClassificationType.IMP;

			var gamePivot = Factory.New<BaseCusClassPartPivot>();
			gamePivot.CI_CC = classificationGame.PK;
			gamePivot.CI_OP = supplierPart1.PK;
			gamePivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			gamePivot.CI_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gamePivot.CI_TariffNum = "3926.90.69.70I";

			var classificationGame2 = Factory.New<BaseCusClassification>();
			classificationGame2.FillWithValidTestData();
			classificationGame2.CC_LookupCode = "Games";
			classificationGame2.CC_ClassificationType = ClassificationType.EXP;

			var gamePivot2 = Factory.New<BaseCusClassPartPivot>();
			gamePivot2.CI_CC = classificationGame2.PK;
			gamePivot2.CI_OP = supplierPart1.PK;
			gamePivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			gamePivot2.CI_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gamePivot2.CI_TariffNum = "3926.90.69.70E";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_PartNo = "Game";

			AssertEquals("3926.90.69.70E", line1.JI_Tariff);
		}

		public void TestSetTariffIfNoClassification()
		{
			var part = Factory.New<OrgSupplierPart>();
			Importer.OH_Code = "~~~";
			part.OP_PartNum = "APPLE";
			part.RelatedOrganisations.RemoveAndDeleteAll();
			part.RelatedOrganisations.AddOrganisationIfNotExist(Importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_OP = part.PK;
			pivot.CI_TariffNum = "0000.00.22";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			pivot.CI_ChildType = invoiceLine.GetClassificationTypeProvider().HTICode;
			invoiceLine.JI_CC = ZGuid.Invalid;
			invoiceLine.JI_PartNo = "APPLE";
			AssertEquals("0000.00.22", invoiceLine.JI_Tariff);
			AssertEquals(ZGuid.Empty, invoiceLine.JI_CC);
		}

		OrgHeader Importer
		{
			get { return fImporter ?? (fImporter = Factory.NewWithValidTestData<OrgHeader>()); }
		}
		OrgHeader fImporter;
	}
}
