using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseInvoiceLineCompleteCollectionTest : TestCaseWithFactory
	{
		public void TestRefreshJZ_Calc_InvoiceLinesEnteredCacheOnAdded()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = declaration.FilteredInvoiceLines.AddNew();
			declaration.FilteredInvoiceLines.RemoveFromRelationship(invoiceLine1);

			invoiceLine1.JI_LinePrice = 100;
			AssertEquals(0m, invoice.JZ_Calc_LinesEntered);
			declaration.FilteredInvoiceLines.Add(invoiceLine1);
			AssertEquals("should refresh on added", 100m, invoice.JZ_Calc_LinesEntered);
		}

		public void TestDoNotSetCeiIfTheEntryInstrucitonIsNonPersistent()
		{
			var je = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var cei = je.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var jz = je.Invoices.AddNew();
			var ji = jz.InvoiceLines.AddNew();
			AssertEquals(cei.PK, ji.JI_CEI);

			cei.MakeNonPersistent();
			jz = je.Invoices.AddNew();
			ji = jz.InvoiceLines.AddNew();
			AssertEquals(ZGuid.Empty, ji.JI_CEI);
			var newJi = jz.InvoiceLines.AddNew();
			AssertEquals(ZGuid.Empty, newJi.JI_CEI);
		}

		public void TestAddNewPartClassification()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var declarationSupplier = OrgHeader.New(Factory);
			declarationSupplier.OH_Code = "SUPPLIER";
			declarationSupplier.OH_IsConsignor = true;
			declarationSupplier.MainAddress.OA_Address1 = "Add1";

			var declarationImporter = OrgHeader.New(Factory);
			declarationImporter.OH_Code = "IMPORTER";
			declarationImporter.OH_IsConsignee = true;
			declarationImporter.MainAddress.OA_Address1 = "Add1";

			declaration.JE_OH_Supplier = declarationSupplier.PK;
			declaration.JE_OH_Importer = declarationImporter.PK;

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "SomePart";
			part.OP_Desc = "Description of product";
			part.OP_StockKeepingUnit = Core.Constants.PkgUnit.Bag;
			var relOrg = part.RelatedOrganisations.AddNew();
			relOrg.OU_OH = declaration.Supplier.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invLine1 = invoiceHeader.InvoiceLines.AddNew();
			invLine1.JI_PartNo = part.OP_PartNum;
			invLine1.JI_Tariff = "123";

			var classifications = Factory.Load<BaseCusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, part.PK));
			Assert(!classifications.Any());

			declaration.InvoiceLines.AddNewPartClassifications();

			classifications = Factory.Load<BaseCusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, part.PK));
			AssertEquals(1, classifications.Length);

			var classification = classifications.Single();
			AssertEquals("123", classification.TariffNumber);
		}

		public void TestAddNewPartClassifications_UnmatchedProductClassificationAddedByPreviousLine()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var declarationSupplier = OrgHeader.New(Factory);
			declarationSupplier.OH_Code = "SUPPLIER";
			declarationSupplier.OH_IsConsignor = true;
			declarationSupplier.MainAddress.OA_Address1 = "Add1";

			var declarationImporter = OrgHeader.New(Factory);
			declarationImporter.OH_Code = "IMPORTER";
			declarationImporter.OH_IsConsignee = true;
			declarationImporter.MainAddress.OA_Address1 = "Add1";

			declaration.JE_OH_Supplier = declarationSupplier.PK;
			declaration.JE_OH_Importer = declarationImporter.PK;

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "SomePart";
			part.OP_Desc = "Description of product";
			part.OP_StockKeepingUnit = Core.Constants.PkgUnit.Bag;
			var relOrg = part.RelatedOrganisations.AddNew();
			relOrg.OU_OH = declaration.Supplier.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invLine1 = invoiceHeader.InvoiceLines.AddNew();
			invLine1.JI_PartNo = part.OP_PartNum;
			invLine1.JI_Tariff = "123";

			var invLine2 = invoiceHeader.InvoiceLines.AddNew();
			invLine2.JI_PartNo = part.OP_PartNum;
			invLine2.JI_Tariff = "987";

			var classifications = Factory.Load<BaseCusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, part.PK));
			Assert(!classifications.Any());

			declaration.InvoiceLines.AddNewPartClassifications();

			classifications = Factory.Load<BaseCusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, part.PK));
			AssertEquals(1, classifications.Length);

			var classification = classifications.Single();
			AssertEquals("123", classification.TariffNumber);
		}

		public void TestAddNewPartClassification_ConcurrencyDuplicateClassification()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var declarationSupplier = OrgHeader.New(Factory);
			declarationSupplier.OH_Code = "SUPPLIER";
			declarationSupplier.OH_IsConsignor = true;
			declarationSupplier.MainAddress.OA_Address1 = "Add1";

			var declarationImporter = OrgHeader.New(Factory);
			declarationImporter.OH_Code = "IMPORTER";
			declarationImporter.OH_IsConsignee = true;
			declarationImporter.MainAddress.OA_Address1 = "Add1";

			declaration.JE_OH_Supplier = declarationSupplier.PK;
			declaration.JE_OH_Importer = declarationImporter.PK;

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "SomePart";
			part.OP_Desc = "Description of product";
			part.OP_StockKeepingUnit = Core.Constants.PkgUnit.Bag;
			var relOrg = part.RelatedOrganisations.AddNew();
			relOrg.OU_OH = declaration.Supplier.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invLine1 = invoiceHeader.InvoiceLines.AddNew();
			invLine1.JI_PartNo = part.OP_PartNum;
			invLine1.JI_Tariff = "123";

			AssertEquals("(pre-condition) line should be linked to part", part.OP_PartNum, invLine1.Part?.OP_PartNum);

			var classifications = Factory.Load<BaseCusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, part.PK));
			AssertEquals("pre-condition: product should not have a linked classification", 0, classifications.Length);

			Factory.Save();

			var childType = invLine1.GetClassificationTypeProvider().HTECode;

			var newClassificationPK = ZGuid.NewZGuid();
			TestConnection.ExecuteNonQuery($@"
				insert into dbo.CusClassPartPivot (CI_PK, CI_OP, CI_TariffNum, CI_ChildType, CI_RN_NKCountry, CI_SystemCreateTimeUtc, CI_SystemCreateUser, CI_SystemLastEditTimeUtc, CI_SystemLastEditUser)
				values ('{newClassificationPK}', '{part.PK}', '123', '{childType}', '{invLine1.CustomsCountryCode}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');");

			var productQuery = new ZQuery();
			productQuery.AddToFilter(JoinCondition.Or, CusClassPartPivotSchema.CI_OP, part.PK);

			AssertEquals("pre-condition: existing classification should not be visible to declaration factory (because of query cache)",
				ZString.Empty,
				string.Join(", ", declaration.Factory.Load<BaseCusClassPartPivot>(productQuery).Select(
					p => $"{p.CI_OP} ({p.TariffNumber})"
				).OrderBy(r => r))
			);

			declaration.InvoiceLines.AddNewPartClassifications();

			AssertEquals("Only one classification should have been created",
				$"{newClassificationPK} (123, {childType}, {invLine1.CustomsCountryCode})",
				string.Join("\r\n", declaration.Factory.Load<BaseCusClassPartPivot>(productQuery).Select(
					p => $"{p.PK} ({p.TariffNumber}, {p.CI_ChildType}, {p.CI_RN_NKCountry})"
				).OrderBy(r => r))
			);
		}

		public void TestCreateNewProductBySupplier()
		{
			CreateTestDataForProductCreation();
			Factory.Save();
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_Description = "CUCKOO SQUEAKERS";
			classification.CC_LookupCode = "CKSQKS";
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			Factory.Save();
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_RL_NKClosestPort = "AUSYD";
			supplier1.OH_IsConsignee = true;
			supplier1.OH_IsConsignor = true;
			var importerLink = Factory.New<OrgSupplierBuyerLink>();
			importerLink.OL_OH_Supplier = supplier1.PK;
			importerLink.OL_OH_Buyer = importer.PK;
			importerLink.OL_ProductRelation = OrgRelationTypeList.Codes.Importer;
			Factory.Save();
			var part5 = Factory.New<OrgSupplierPart>();
			part5.OP_PartNum = "Test5";
			part5.OP_Desc = "part5";
			var supplierRelation1 = part1.RelatedOrganisations.AddNew();
			supplierRelation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			supplierRelation1.OU_OH = supplier1.PK;

			Factory.Save();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier1.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_CC = classification.PK;
			line1.JI_PartNo = "Test5";
			line1.JI_Description = "KEVIN TEST3";
			line1.JI_InvoiceUQ = "BAG";
			var invoiceLineCompleteCollection = declaration.InvoiceLines;
			var duplicates = invoiceLineCompleteCollection.AddNewPartsOrActivateInactiveOnes(ProductRelationDefaultOption.OptionForImporter, new DeclarationForProductCreationHelper(invoiceHeader));
			AssertEquals(0, duplicates.Count);

			var parts = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "Test5"));

			AssertEquals("New Product Create", 2, parts.Length);

			var newPart = parts.FirstOrDefault(x => x.PK != part5.PK);
			declaration.ResumeApportionment();
			AssertNotNull(newPart);
			AssertEquals(1, newPart.RelatedOrganisations.Count);
			Assert(newPart.RelatedOrganisations.Cast<OrgPartRelation>().Any(x => x.OU_OH == importer.PK && x.OU_Relationship == OrgPartRelation.RelationshipTypes.Owner));
		}

		public void TestActivateInactiveOnesByImporter()
		{
			CreateTestDataForProductCreation();

			var classification = Factory.New<BaseCusClassification>();
			classification.CC_Description = "CUCKOO SQUEAKERS";
			classification.CC_LookupCode = "CKSQKS";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_CC = classification.PK;
			line1.JI_PartNo = "Test1";
			line1.JI_Description = "KEVIN TEST3";
			line1.JI_InvoiceUQ = "BAG";

			var partOwn = Factory.Load<OrgSupplierPart>(part1.PK);
			AssertEquals(false, partOwn.OP_IsActive);
			var invoiceLineCompleteCollection = declaration.InvoiceLines;
			var duplicates = invoiceLineCompleteCollection.AddNewPartsOrActivateInactiveOnes(ProductRelationDefaultOption.OptionForImporter, new DeclarationForProductCreationHelper(invoiceHeader));
			AssertEquals(true, partOwn.OP_IsActive);
			AssertEquals(0, duplicates.Count);
		}

		public void TestActivateInactiveOnesBySupplier()
		{
			CreateTestDataForProductCreation();

			var classification = Factory.New<BaseCusClassification>();
			classification.CC_Description = "CUCKOO SQUEAKERS";
			classification.CC_LookupCode = "CKSQKS";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_CC = classification.PK;
			line1.JI_PartNo = "Test2";
			line1.JI_Description = "KEVIN TEST3";
			line1.JI_InvoiceUQ = "BAG";

			var partOwn = Factory.Load<OrgSupplierPart>(part2.PK);
			AssertEquals(false, partOwn.OP_IsActive);
			var invoiceLineCompleteCollection = declaration.InvoiceLines;
			var duplicates = invoiceLineCompleteCollection.AddNewPartsOrActivateInactiveOnes(ProductRelationDefaultOption.OptionForSupplier, new DeclarationForProductCreationHelper(invoiceHeader));
			AssertEquals(true, partOwn.OP_IsActive);
			AssertEquals(0, duplicates.Count);
		}

		public void TestActivateInactiveShouldMatchExactInvoiceLineDetail()
		{
			CreateTestDataForProductCreation();

			var classification1 = Factory.New<BaseCusClassification>();
			classification1.CC_Description = "CUCKOO SQUEAKERS";
			classification1.CC_LookupCode = "CKSQKS";

			var classification2 = Factory.New<BaseCusClassification>();
			classification2.CC_Description = "CUCKOO SQUEAKERS 2";
			classification2.CC_LookupCode = "CKSQKS2";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = supplier2.PK;
			declaration.JE_OH_Importer = importer2.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_CC = classification1.PK;
			line1.JI_PartNo = "Test3";
			line1.JI_Description = "KEVIN TEST3";
			line1.JI_InvoiceUQ = "BAG";

			AssertEquals(false, part3.OP_IsActive);
			var invoiceLineCompleteCollection = declaration.InvoiceLines;
			var duplicates = invoiceLineCompleteCollection.AddNewPartsOrActivateInactiveOnes(ProductRelationDefaultOption.None, new DeclarationForProductCreationHelper(invoiceHeader));
			AssertEquals(true, part3.OP_IsActive);
			AssertEquals(true, ((IBusinessObjectState)part3).HasChangesNotIncludingChildren);
			AssertEquals("part3.OP_Desc", "KEVIN TEST3", part3.OP_Desc);
			AssertEquals("part3.OP_StockKeepingUnit", "BAG", part3.OP_StockKeepingUnit);
			AssertCollectionContains(classification1, part3.ClassificationsForBinding.ToArray());
			AssertEquals("part3.RelatedOrganisations.Count", 1, part3.RelatedOrganisations.Count);
			AssertNotNull("Importer2", part3.RelatedOrganisations.FindByOrganisationAndRelationship(importer2, OrgPartRelation.RelationshipTypes.Owner));
			AssertEquals(0, duplicates.Count);

			var partRelation = part3.RelatedOrganisations.AddOwner(importer);
			part3.OP_IsActive = ZBool.False;
			line1.JI_OP = ZGuid.Empty;
			line1.JI_CC = classification1.PK;
			line1.JI_PartNo = "Test3";
			line1.JI_Description = "KEVIN TEST3";
			line1.JI_InvoiceUQ = "BAG";
			line1.PartSyncManager.Refresh();
			invoiceLineCompleteCollection = declaration.InvoiceLines;
			duplicates = invoiceLineCompleteCollection.AddNewPartsOrActivateInactiveOnes(ProductRelationDefaultOption.OptionForImporter, new DeclarationForProductCreationHelper(invoiceHeader));
			AssertEquals(false, part3.OP_IsActive);
			var part = line1.Part;
			AssertNotEquals(part3, part);
			AssertEquals(0, duplicates.Count);

			part.Delete();
			partRelation.Delete();
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_OP = part3.PK;
			pivot.CI_CC = classification2.PK;
			part3.OP_IsActive = ZBool.False;
			line1.JI_OP = ZGuid.Empty;
			line1.JI_CC = classification1.PK;
			line1.JI_PartNo = "Test3";
			line1.JI_Description = "KEVIN TEST3";
			line1.JI_InvoiceUQ = "BAG";
			line1.PartSyncManager.Refresh();

			duplicates = invoiceLineCompleteCollection.AddNewPartsOrActivateInactiveOnes(ProductRelationDefaultOption.OptionForImporter, new DeclarationForProductCreationHelper(invoiceHeader));
			AssertEquals(false, part3.OP_IsActive);
			part = line1.Part;
			AssertNotEquals(part3, part);
			AssertEquals(0, duplicates.Count);
		}

		public void TestCreateNewProductOnlyBySupplier()
		{
			CreateTestDataForProductCreation();
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_Description = "CUCKOO SQUEAKERS";
			classification.CC_LookupCode = "CKSQKS";
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_OH_Supplier = supplier2.PK;

			var link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Supplier = supplier.PK;
			link.OL_OH_Buyer = importer.PK;
			link.OL_ProductRelation = OrgRelationTypeList.Codes.Supplier;
			link.OL_RN_NKImporterCountry = "AU";
			Factory.Save();

			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_CC = classification.PK;
			line1.JI_PartNo = "TEST9";
			line1.JI_Description = "KEVIN TEST3";
			line1.JI_InvoiceUQ = "BAG";

			var invoiceLineCompleteCollection = declaration.InvoiceLines;
			var duplicates = invoiceLineCompleteCollection.AddNewPartsOrActivateInactiveOnes(ProductRelationDefaultOption.OptionForImporter, new DeclarationForProductCreationHelper(invoiceHeader));
			var part = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "TEST9")).FirstOrDefault();
			AssertNotNull("Create new one", part);
			AssertEquals("TEST9", part.OP_PartNum);
			AssertEquals(1, part.RelatedOrganisations.Count);
			AssertEquals(OrgPartRelation.RelationshipTypes.Owner, part.RelatedOrganisations[0].OU_Relationship);
			AssertEquals(0, duplicates.Count);
		}

		public void TestCreateNewProductOnlyByImporter()
		{
			CreateTestDataForProductCreation();
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_Description = "CUCKOO SQUEAKERS";
			classification.CC_LookupCode = "CKSQKS";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_OH_Buyer = importer2.PK;
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_CC = classification.PK;
			line1.JI_PartNo = "TEST10";
			line1.JI_Description = "KEVIN TEST3";
			line1.JI_InvoiceUQ = "BAG";

			var invoiceLineCompleteCollection = declaration.InvoiceLines;
			var duplicates = invoiceLineCompleteCollection.AddNewPartsOrActivateInactiveOnes(ProductRelationDefaultOption.OptionForImporter, new DeclarationForProductCreationHelper(invoiceHeader));
			AssertEquals(0, duplicates.Count);
			var part = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "TEST10")).FirstOrDefault();
			AssertNotNull("Create new one", part);
			AssertEquals("TEST10", part.OP_PartNum);
			AssertEquals(part.RelatedOrganisations.Count, 1);
			AssertEquals(part.RelatedOrganisations[0].OU_Relationship, "OWN");
		}

		public void TestReloadPivotsAndChildren()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				//Create the declaration, organisations and part.
				var declaration = Factory.New<BaseJobDeclaration>();
				var declarationSupplier = OrgHeader.New(Factory);
				declarationSupplier.OH_Code = "SUPPLIER";
				declarationSupplier.OH_IsConsignor = true;
				declarationSupplier.MainAddress.OA_Address1 = "Add1";

				var declarationImporter = OrgHeader.New(Factory);
				declarationImporter.OH_Code = "IMPORTER";
				declarationImporter.OH_IsConsignee = true;
				declarationImporter.MainAddress.OA_Address1 = "Add1";

				declaration.JE_OH_Supplier = declarationSupplier.PK;
				declaration.JE_OH_Importer = declarationImporter.PK;

				var part = Factory.New<OrgSupplierPart>();
				part.OP_PartNum = "SomePart";
				part.OP_Desc = "Description of product";
				part.OP_StockKeepingUnit = Core.Constants.PkgUnit.Bag;
				var relOrg = part.RelatedOrganisations.AddNew();
				relOrg.OU_OH = declaration.Importer.PK;
				relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

				var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				var invLine1 = invoiceHeader.InvoiceLines.AddNew();
				invLine1.JI_PartNo = part.OP_PartNum;
				invLine1.JI_OP = part.PK;
				AssertNotNull(invLine1.Part);
				invLine1.JI_Tariff = "123";

				var classifications = Factory.Load<BaseCusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, part.PK));
				Assert(!classifications.Any());

				Factory.Save();

				//Load the saved declaration,
				var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
				var decLoadedInOtherFactory = factory2.Load<BaseJobDeclaration>(declaration.PK);
				var invLinesInOtherFactory = decLoadedInOtherFactory.InvoiceLines;

				//Create the pivots in the factory with the loaded declaration (shouldn't be aware of the previously created pivots)
				invLinesInOtherFactory.AddNewPartClassifications();
				factory2.Save();
				var pivot = invLinesInOtherFactory[0].Pivot;

				//Create the pivots in another factory that isn't saved.
				declaration.InvoiceLines.AddNewPartClassifications();
				Factory.Save();

				CombineAssertions(() =>
				{
					AssertEquals(pivot.PK, invLine1.Pivot.PK);
					//Assert that only one was made
					classifications = Factory.Load<BaseCusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, part.PK));
					AssertEquals(1, classifications.Length);
				});
			}
		}

		public void TestReloadPivotsAndChildren_CountryDoesntSupportChildren()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "SomePart";
			part.OP_Desc = "Description of product";
			part.OP_StockKeepingUnit = Core.Constants.PkgUnit.Bag;

			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "08091998";

			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invLine1 = invoiceHeader.InvoiceLines.AddNew();
			invLine1.JI_PartNo = part.OP_PartNum;
			invLine1.JI_OP = part.PK;
			invLine1.JI_Tariff = "123";

			AssertNoExceptionThrown(() => declaration.InvoiceLines.AddNewPartClassifications());
		}

		public void TestDoesntCreateNewProductWhenActiveOneWasAddedByAnotherInvoiceHeader()
		{
			CreateTestDataForProductCreation();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invHeader1 = declaration.Invoices.AddNew();
			invHeader1.JZ_InvoiceNumber = "1";
			invHeader1.JZ_OH_Supplier = supplier.PK;

			var invLine1 = invHeader1.InvoiceLines.AddNew();
			invLine1.JI_PartNo = "PART";
			invLine1.JI_InvoiceQuantity = 10m;
			invLine1.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;
			invLine1.JI_Tariff = "999";
			invLine1.JI_Description = "123";

			var invHeader2 = declaration.Invoices.AddNew();
			invHeader2.JZ_InvoiceNumber = "2";
			invHeader2.JZ_OH_Supplier = supplier2.PK;

			var invLine2 = invHeader2.InvoiceLines.AddNew();
			invLine2.JI_PartNo = "PART";
			invLine2.JI_InvoiceQuantity = 10m;
			invLine2.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;
			invLine2.JI_Tariff = "999";
			invLine2.JI_Description = "234";

			var invoiceLineCompleteCollection = declaration.InvoiceLines;
			var result1 = invoiceLineCompleteCollection.AddNewPartsOrActivateInactiveOnes(ProductRelationDefaultOption.OptionForImporter, new DeclarationForProductCreationHelper(invHeader1));
			var result2 = invoiceLineCompleteCollection.AddNewPartsOrActivateInactiveOnes(ProductRelationDefaultOption.OptionForImporter, new DeclarationForProductCreationHelper(invHeader2));
			AssertEquals(0, result1.Count);
			AssertEquals(0, result2.Count);

			var part = invLine1.Part;
			AssertNotNull(part);
			AssertEquals("PART", part.OP_PartNum);
			AssertEquals(part, invLine2.Part);

			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestCreateDuplicateProduct_Importer()
		{
			TestCreateDuplicateProduct(ProductRelationDefaultOption.OptionForImporter);
		}

		public void TestCreateDuplicateProduct_Supplier()
		{
			TestCreateDuplicateProduct(ProductRelationDefaultOption.OptionForSupplier);
		}

		void TestCreateDuplicateProduct(ProductRelationDefaultOption relationOption)
		{
			CreateTestDataForProductCreation();
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_Description = "EXAMPLE CLASSIFICATION";
			classification.CC_LookupCode = "EXAMPLE";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;

			var invoiceHeader = declaration.Invoices.AddNew();
			//invoiceHeader.JZ_OH_Buyer = Importer2.PK;

			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_CC = classification.PK;
			line1.JI_PartNo = "DUPTEST-EXISTING1";
			line1.JI_Description = "Duplicate Product 1";
			line1.JI_InvoiceUQ = "UNT";

			var line2 = invoiceHeader.InvoiceLines.AddNew();
			line2.JI_CC = classification.PK;
			line2.JI_PartNo = "DUPTEST-NEW";
			line2.JI_Description = "New Product";
			line2.JI_InvoiceUQ = "UNT";

			var line3 = invoiceHeader.InvoiceLines.AddNew();
			line3.JI_CC = classification.PK;
			line3.JI_PartNo = "DUPTEST-EXISTING2";
			line3.JI_Description = "Duplicate Product 2";
			line3.JI_InvoiceUQ = "UNT";

			AssertEquals("pre-condition: line should not have linked product", ZGuid.Empty, line1.JI_OP);
			AssertEquals("pre-condition: line should not have linked product", ZGuid.Empty, line2.JI_OP);
			AssertEquals("pre-condition: line should not have linked product", ZGuid.Empty, line3.JI_OP);

			// in this scenario another user creates products with same code and owner/supplier
			var existingProduct1PK = ZGuid.NewZGuid();
			var existingProduct2PK = ZGuid.NewZGuid();
			var existingRelation1PK = ZGuid.NewZGuid();
			var existingRelation2PK = ZGuid.NewZGuid();
			var existingRelationOrgPK = relationOption == ProductRelationDefaultOption.OptionForSupplier ? supplier.PK : importer.PK;
			var existingRelationType = relationOption == ProductRelationDefaultOption.OptionForSupplier ? "SUP" : "OWN";
			TestConnection.ExecuteNonQuery($@"
				insert into dbo.OrgSupplierPart (OP_PK, OP_PartNum, OP_Desc)
				values ('{existingProduct1PK}', 'DUPTEST-EXISTING1', 'Existing Product 1');
				insert into dbo.OrgPartRelation (OU_PK, OU_OP, OU_OH, OU_Relationship, OU_SystemCreateTimeUtc, OU_SystemCreateUser, OU_SystemLastEditTimeUtc, OU_SystemLastEditUser)
				values ('{existingRelation1PK}', '{existingProduct1PK}', '{existingRelationOrgPK}', '{existingRelationType}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				insert into dbo.OrgSupplierPart (OP_PK, OP_PartNum, OP_Desc)
				values ('{existingProduct2PK}', 'DUPTEST-EXISTING2', 'Existing Product 2');
				insert into dbo.OrgPartRelation (OU_PK, OU_OP, OU_OH, OU_Relationship, OU_SystemCreateTimeUtc, OU_SystemCreateUser, OU_SystemLastEditTimeUtc, OU_SystemLastEditUser)
				values ('{existingRelation2PK}', '{existingProduct2PK}', '{existingRelationOrgPK}', '{existingRelationType}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
			");

			AssertEquals("pre-condition: line should not have linked product", ZGuid.Empty, line1.JI_OP);
			AssertEquals("pre-condition: line should not have linked product", ZGuid.Empty, line2.JI_OP);
			AssertEquals("pre-condition: line should not have linked product", ZGuid.Empty, line3.JI_OP);

			ZQuery productQuery = new ZQuery();
			productQuery.AddToFilter(JoinCondition.Or, OrgSupplierPartSchema.OP_PartNum, "DUPTEST-EXISTING1");
			productQuery.AddToFilter(JoinCondition.Or, OrgSupplierPartSchema.OP_PartNum, "DUPTEST-EXISTING2");
			productQuery.AddToFilter(JoinCondition.Or, OrgSupplierPartSchema.OP_PartNum, "DUPTEST-NEW");

			AssertEquals(
				"pre-condition: existing products sholud not be visible to declaration factory (because of query cache), new product is not created yet",
				"",
				string.Join(", ", declaration.Factory.Load<OrgSupplierPart>(productQuery).Select(
					p => $"{p.OP_PartNum} ({p.OP_Desc})"
				).OrderBy(r => r))
			);

			var duplicates = declaration.InvoiceLines.AddNewPartsOrActivateInactiveOnes(relationOption, new DeclarationForProductCreationHelper(invoiceHeader));
			if (relationOption == ProductRelationDefaultOption.OptionForSupplier)
			{
				AssertEquals("DUPTEST-EXISTING1 (Owner = , Supplier = Supplier); DUPTEST-EXISTING2 (Owner = , Supplier = Supplier)", string.Join("; ", duplicates.OrderBy(r => r)));
			}
			else
			{
				AssertEquals("DUPTEST-EXISTING1 (Owner = Importer, Supplier = ); DUPTEST-EXISTING2 (Owner = Importer, Supplier = )", string.Join("; ", duplicates.OrderBy(r => r)));
			}

			AssertEquals(
				"existing products sholud now be visible to declaration factory, new product should be created, duplicate product should not be created",
				"DUPTEST-EXISTING1 (Existing Product 1); DUPTEST-EXISTING2 (Existing Product 2); DUPTEST-NEW (New Product)",
				string.Join("; ", declaration.Factory.Load<OrgSupplierPart>(productQuery).Select(
					p => $"{p.OP_PartNum} ({p.OP_Desc})"
				).OrderBy(r => r))
			);
			AssertEquals("line should not have linked product", ZGuid.Empty, line1.JI_OP);
			AssertEquals("line should be linked to new product", "DUPTEST-NEW", line2.Part?.OP_PartNum);
			AssertEquals("line should not have linked product", ZGuid.Empty, line3.JI_OP);
		}

		void CreateTestDataForProductCreation()
		{
			importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "Importer";
			importer.OH_RL_NKClosestPort = "AUSYD";
			importer.OH_IsConsignee = true;
			importer.OH_IsConsignor = true;

			supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "Supplier";
			supplier.OH_RL_NKClosestPort = "AUSYD";
			supplier.OH_IsConsignor = true;
			supplier.OH_IsConsignee = true;

			importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.OH_Code = "Importer2";
			importer2.OH_IsConsignee = true;
			importer2.OH_IsConsignor = true;

			supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			supplier2.OH_Code = "Supplier2";
			supplier2.OH_IsConsignor = true;
			supplier2.OH_IsConsignee = true;

			part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "Test1";
			part1.OP_Desc = "part1";
			part1.RelatedOrganisations.AddOwner(importer);
			part1.OP_IsActive = false;

			part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "Test2";
			part2.OP_Desc = "part2";
			part2.RelatedOrganisations.AddSupplier(supplier);
			part2.OP_IsActive = false;

			part3 = Factory.New<OrgSupplierPart>();
			part3.OP_PartNum = "Test3";
			part3.OP_Desc = "part3";
			part3.RelatedOrganisations.AddOwner(importer2);
			part3.RelatedOrganisations.AddSupplier(supplier2);
			part3.OP_IsActive = false;
			Factory.Save();
		}
		OrgHeader importer;
		OrgHeader supplier;
		OrgHeader importer2;
		OrgHeader supplier2;
		OrgSupplierPart part1;
		OrgSupplierPart part2;
		OrgSupplierPart part3;

		public void TestWeightIsReApportionWhenNewInvoiceLineIsCommitted()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_AutoWeightApportion = true;
			declaration.JE_TotalWeight = 10000m;
			BaseJobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice1.JZ_InvoiceAmount = 1000m;
			AssertEquals(10000m, invoice1.JZ_Weight);
			InvoiceLineCompleteCollection invoiceLines = declaration.InvoiceLines;
			BaseJobComInvoiceLine invoiceLine1 = invoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 800m;
			BaseJobComInvoiceLine invoiceLine2 = (BaseJobComInvoiceLine)((IBindingList)invoiceLines).AddNew();
			AssertEquals(true, invoiceLines.IsNonCommittedCollectionElement(invoiceLine2));
			invoiceLine2.JI_LinePrice = 200m;
			AssertEquals(8000m, invoiceLine1.JI_Weight);
			AssertEquals(2000m, invoiceLine2.JI_Weight);
			((ICancelAddNew)invoiceLines).EndNew(1);
			AssertEquals(false, invoiceLines.IsNonCommittedCollectionElement(invoiceLine2));
			AssertEquals(8000m, invoiceLine1.JI_Weight);
			AssertEquals(2000m, invoiceLine2.JI_Weight);
			BaseJobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 1000m;

			AssertEquals(5000m, invoice1.JZ_Weight);
			AssertEquals(5000m, invoice2.JZ_Weight);
			AssertEquals(4000m, invoiceLine1.JI_Weight);
			AssertEquals(1000m, invoiceLine2.JI_Weight);

			declaration.JE_TotalWeight = 1000m;
			AssertEquals(500m, invoice1.JZ_Weight);
			AssertEquals(500m, invoice2.JZ_Weight);
			AssertEquals(400m, invoiceLine1.JI_Weight);
			AssertEquals(100m, invoiceLine2.JI_Weight);

			invoice2.Delete();
			AssertEquals(1000m, invoice1.JZ_Weight);
			AssertEquals(800m, invoiceLine1.JI_Weight);
			AssertEquals(200m, invoiceLine2.JI_Weight);
		}

		public void TestLoadStmNoteFetchHintIfNeeded()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLines = declaration.InvoiceLines;
			var invoiceLine1 = invoiceLines.AddNew();
			var invoiceLine2 = invoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			var count = Factory.ActiveTableFetchHints;
			var stmNoteCount = Factory.ActiveFetchHintsForTable(StmNoteSchema.Constants.TableName);
			invoiceLines.LoadStmNoteFetchHintIfNeeded();
			AssertEquals(count + 1, Factory.ActiveTableFetchHints);
			AssertEquals(stmNoteCount + 3, Factory.ActiveFetchHintsForTable(StmNoteSchema.Constants.TableName));
			invoiceLines.LoadStmNoteFetchHintIfNeeded();
			AssertEquals("Should not have changed", count + 1, Factory.ActiveTableFetchHints);
			AssertEquals("Should not have changed", stmNoteCount + 3, Factory.ActiveFetchHintsForTable(StmNoteSchema.Constants.TableName));
		}

		public void TestLoadingDoesNotCauseReApportionOfLineWeight()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_AutoWeightApportion = true;
			declaration.JE_TotalWeight = 10000m;
			BaseJobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice1.JZ_InvoiceAmount = 1000m;
			AssertEquals(10000m, invoice1.JZ_Weight);
			InvoiceLineCompleteCollection invoiceLines = declaration.InvoiceLines;
			BaseJobComInvoiceLine invoiceLine1 = invoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 800m;
			BaseJobComInvoiceLine invoiceLine2 = (BaseJobComInvoiceLine)((IBindingList)invoiceLines).AddNew();
			AssertEquals(true, invoiceLines.IsNonCommittedCollectionElement(invoiceLine2));
			invoiceLine2.JI_LinePrice = 200m;
			AssertEquals(8000m, invoiceLine1.JI_Weight);
			AssertEquals(2000m, invoiceLine2.JI_Weight);
			((ICancelAddNew)invoiceLines).EndNew(1);
			AssertEquals(false, invoiceLines.IsNonCommittedCollectionElement(invoiceLine2));
			AssertEquals(8000m, invoiceLine1.JI_Weight);
			AssertEquals(2000m, invoiceLine2.JI_Weight);
			BaseJobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 1000m;

			AssertEquals(5000m, invoice1.JZ_Weight);
			AssertEquals(5000m, invoice2.JZ_Weight);
			AssertEquals(4000m, invoiceLine1.JI_Weight);
			AssertEquals(1000m, invoiceLine2.JI_Weight);

			declaration.JE_TotalWeight = 1000m;
			AssertEquals(500m, invoice1.JZ_Weight);
			AssertEquals(500m, invoice2.JZ_Weight);
			AssertEquals(400m, invoiceLine1.JI_Weight);
			AssertEquals(100m, invoiceLine2.JI_Weight);

			invoice1.JZ_Weight = 600m;
			invoice2.JZ_Weight = 400m;
			invoiceLine1.JI_Weight = 480m;
			invoiceLine2.JI_Weight = 120m;

			InvoiceLineCompleteCollection collection = new InvoiceLineCompleteCollection(declaration);
			collection.Load();
			AssertEquals(600m, invoice1.JZ_Weight);
			AssertEquals(400m, invoice2.JZ_Weight);
			AssertEquals(480m, invoiceLine1.JI_Weight);
			AssertEquals(120m, invoiceLine2.JI_Weight);
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseJobDeclaration loadedDeclaration = newFactory.Load<BaseJobDeclaration>(declaration.PK);
			AssertEquals(false, loadedDeclaration.HasChanges);
			AssertEquals(2, loadedDeclaration.Invoices.Count);
			AssertEquals(2, loadedDeclaration.InvoiceLines.Count);
			BaseJobComInvoiceHeader loadedInvoice1 = (BaseJobComInvoiceHeader)loadedDeclaration.Invoices.FindByPK(invoice1.PK);
			BaseJobComInvoiceHeader loadedInvoice2 = (BaseJobComInvoiceHeader)loadedDeclaration.Invoices.FindByPK(invoice2.PK);
			BaseJobComInvoiceLine loadedInvoiceLine1 = (BaseJobComInvoiceLine)loadedDeclaration.InvoiceLines.FindByPK(invoiceLine1.PK);
			BaseJobComInvoiceLine loadedInvoiceLine2 = (BaseJobComInvoiceLine)loadedDeclaration.InvoiceLines.FindByPK(invoiceLine2.PK);

			AssertEquals(false, loadedDeclaration.HasChanges);
			AssertEquals(600m, loadedInvoice1.JZ_Weight);
			AssertEquals(400m, loadedInvoice2.JZ_Weight);
			AssertEquals(480m, loadedInvoiceLine1.JI_Weight);
			AssertEquals(120m, loadedInvoiceLine2.JI_Weight);
		}

		public void TestAddNewPartsOrActivateInactiveOnes_MakeNew()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ZZZ11Z@";
			org.OH_FullName = "BOB THE BUILDER";
			org.MainAddress.OA_Address1 = "ADDRESS 1";
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "NEWCLASS";
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			classification.CC_IsActive = true;
			Factory.Save();

			var mockDec = Factory.NewMoq<BaseJobDeclaration>();
			var testDec = mockDec.Object;
			testDec.JE_OH_Importer = org.PK;
			var header = Factory.New<BaseJobComInvoiceHeader>();
			header.JZ_InvoiceNumber = "6";
			header.JZ_JE = testDec.PK;

			var line1 = Factory.New<BaseJobComInvoiceLine>();
			line1.JI_JZ = header.PK;
			line1.JI_PartNo = "NEWPART1";
			line1.JI_CC = classification.PK;
			line1.JI_Description = "666";
			line1.JI_InvoiceUQ = "KG";
			testDec.InvoiceLines.Add(line1);
			var line2 = Factory.New<BaseJobComInvoiceLine>();
			line2.JI_JZ = header.PK;
			line2.JI_PartNo = "NEWPART2";
			line2.JI_CC = classification.PK;
			line2.JI_Description = "666666";
			line2.JI_InvoiceUQ = "KG";
			testDec.InvoiceLines.Add(line2);
			var line3 = Factory.New<BaseJobComInvoiceLine>();
			line3.JI_JZ = header.PK;
			line3.JI_PartNo = "NEWPART2";
			line3.JI_CC = classification.PK;
			line3.JI_Description = "666666666";
			line3.JI_InvoiceUQ = "KG";
			testDec.InvoiceLines.Add(line3);
			var line4 = Factory.New<BaseJobComInvoiceLine>();
			line4.JI_JZ = header.PK;
			line4.JI_PartNo = "NEWPART3";
			line4.JI_Description = "666666666";
			line4.JI_Tariff = "10101010";
			line4.JI_InvoiceUQ = "KG";
			testDec.InvoiceLines.Add(line4);
			var line5 = Factory.New<BaseJobComInvoiceLine>();
			line5.JI_JZ = header.PK;
			line5.JI_PartNo = "NEWPART4";
			line5.JI_Description = "666666666";
			line5.JI_InvoiceUQ = "KG";
			testDec.InvoiceLines.Add(line5);

			var duplicates = testDec.InvoiceLines.AddNewPartsOrActivateInactiveOnes();
			AssertEquals(0, duplicates.Count);

			Factory.Save();
			var parts = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.StartsWith, "NEWPART"));
			AssertEquals("Three products must be created.", 3, parts.Length);
			parts = parts.Cast<OrgSupplierPart>().OrderBy(x => x.OP_PartNum).ToArray();
			var part1 = parts[0];
			var part2 = parts[1];
			var part3 = parts[2];

			AssertEquals("NEWPART1", part1.OP_PartNum);
			AssertEquals("NEWPART2", part2.OP_PartNum);
			AssertEquals("NEWPART3", part3.OP_PartNum);
			AssertEquals(part1, line1.Part);
			AssertEquals(part2, line2.Part);
			AssertEquals(part2, line3.Part);
			AssertEquals(part3, line4.Part);
			AssertNull(line5.Part);

			duplicates = testDec.InvoiceLines.AddNewPartsOrActivateInactiveOnes();
			AssertEquals(0, duplicates.Count);
			Factory.Save();
			parts = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.StartsWith, "NEWPART"));
			AssertEquals("Three products must be created.", 3, parts.Length);
			parts = parts.Cast<OrgSupplierPart>().OrderBy(x => x.OP_PartNum).ToArray();
			part1 = parts[0];
			part2 = parts[1];
			part3 = parts[2];

			AssertEquals("NEWPART1", part1.OP_PartNum);
			AssertEquals("NEWPART2", part2.OP_PartNum);
			AssertEquals("NEWPART3", part3.OP_PartNum);
			AssertEquals(part1, line1.Part);
			AssertEquals(part2, line2.Part);
			AssertEquals(part2, line3.Part);
			AssertEquals(part3, line4.Part);
			AssertNull(line5.Part);

			mockDec.Protected().Setup<bool>("IsNotPersistentActiveProductInternal", ItExpr.IsAny<BaseJobComInvoiceLine>()).Returns((BaseJobComInvoiceLine line) =>
			{
				return line == line5;
			});

			duplicates = testDec.InvoiceLines.AddNewPartsOrActivateInactiveOnes();
			AssertEquals(0, duplicates.Count);
			Factory.Save();
			parts = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.StartsWith, "NEWPART"));
			AssertEquals("Four products must be created.", 4, parts.Length);
			part1 = parts.First(x => x.OP_PartNum == "NEWPART1");
			part2 = parts.First(x => x.OP_PartNum == "NEWPART2");
			part3 = parts.First(x => x.OP_PartNum == "NEWPART3");
			var part4 = parts.First(x => x.OP_PartNum == "NEWPART4");
			AssertEquals("NEWPART1", part1.OP_PartNum);
			AssertEquals("NEWPART2", part2.OP_PartNum);
			AssertEquals("NEWPART3", part3.OP_PartNum);
			AssertEquals("NEWPART4", part4.OP_PartNum);
			AssertEquals(part1, line1.Part);
			AssertEquals(part2, line2.Part);
			AssertEquals(part2, line3.Part);
			AssertEquals(part3, line4.Part);
			AssertEquals(part4, line5.Part);
			mockDec.Verify();
		}

		public void TestAddNewPartsOrActivateInactiveOnes_ActivatesTheInactive()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ZZZ11Z@";
			org.OH_FullName = "BOB THE BUILDER";
			org.MainAddress.OA_Address1 = "ADDRESS 1";
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "NEWCLASS";
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			classification.CC_IsActive = true;

			var part = Factory.New<OrgSupplierPart>();
			var relationship = part.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = "BTH";
			relationship.OU_OH = org.PK;
			part.OP_IsActive = false;
			part.OP_PartNum = "INACTIVE";

			Factory.Save();

			var testDec = Factory.New<BaseJobDeclaration>();
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_OH_Importer = org.PK;
			var header = testDec.Invoices.AddNew();
			header.JZ_InvoiceNumber = "6";

			var line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_PartNo = "INACTIVE";
			line1.JI_CC = classification.PK;
			line1.JI_Description = "666";
			line1.JI_InvoiceUQ = "KG";
			testDec.InvoiceLines.Add(line1);

			var duplicates = testDec.InvoiceLines.AddNewPartsOrActivateInactiveOnes();
			AssertEquals(0, duplicates.Count);

			Factory.Save();
			var parts = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.StartsWith, "INACTIVE"));
			AssertEquals("No new products must be created.", 1, parts.Length);
			AssertEquals("Product is now active", true, parts[0].OP_IsActive);
		}

		public void TestLoadingInvoiceLinesAndInvoicesAreIndependent()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine1 = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine1.JI_JZ = invoice1.PK;

			BaseJobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine2 = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine2.JI_JZ = invoice2.PK;

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			var mock = Factory.NewMoq<BaseJobDeclaration>();
			BaseJobDeclaration decLoaded = (BaseJobDeclaration)factory2.Load(declaration.GetType(), declaration.PK);
			TestInvoiceHeaderCollection coll = new TestInvoiceHeaderCollection(decLoaded);
			mock.Protected().Setup<InvoiceHeaderActiveCollection>("CreateNewInvoiceHeaderCollection").Returns(coll);

			AssertEquals("two invoices should have been loaded", 2, decLoaded.Invoices.Count);
			AssertEquals("two invoice lines should have been loaded", 2, decLoaded.FilteredInvoiceLines.Count);
			AssertEquals("two invoice lines should have been loaded", 2, decLoaded.InvoiceLines.Count);
		}

		class TestInvoiceHeaderCollection : InvoiceHeaderActiveCollection
		{
			public TestInvoiceHeaderCollection(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			protected override void SetDefaultsForNewElementCore(BaseJobComInvoiceHeader newElement)
			{
				base.SetDefaultsForNewElementCore(newElement);
				int invoiceLinesCollectionIsAccessed = declaration.InvoiceLines.Count;
				int invoiceLinesCollectionIsAccessed2 = declaration.FilteredInvoiceLines.Count;
			}
		}
	}
}
