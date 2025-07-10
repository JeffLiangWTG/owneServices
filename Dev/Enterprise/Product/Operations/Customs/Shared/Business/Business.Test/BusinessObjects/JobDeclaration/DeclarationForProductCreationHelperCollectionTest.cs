using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class DeclarationForProductCreationHelperCollectionTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Chad))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var link = Factory.New<OrgSupplierBuyerLink>();
				link.OL_OH_Buyer = importer.PK;
				link.OL_OH_Supplier = supplier.PK;
				link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Chad;
				link.OL_ProductRelation = OrgRelationTypeList.Codes.Importer;
				Factory.Save();

				CustomsDataRegistry.Instance.AssumeCreatedProductBelongsToClient.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ClientProductCreationTypeList.Codes.No);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export; // a dec with an importer and no supplier
				declaration.JE_OH_Supplier = ZGuid.Empty;
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_RL_NKFinalDestination = "TDADA";

				var invoiceHeader = declaration.Invoices.AddNew(); // an invoice that has an overridden supplier, which matches it with a supplier buyer link
				invoiceHeader.JZ_InvoiceNumber = "1";
				invoiceHeader.JZ_OH_Supplier = supplier.PK;
				var line1 = CreateLineThatRequiresAProduct(invoiceHeader);

				var invoiceHeader2 = declaration.Invoices.AddNew(); // an invoice that has an overriden supplier falls right through to the registry, (i.e. 'Yes' and uses supplier)
				invoiceHeader2.JZ_InvoiceNumber = "2";
				invoiceHeader2.JZ_OH_Supplier = supplier2.PK;
				var line2 = CreateLineThatRequiresAProduct(invoiceHeader2);

				var invoiceHeader3 = declaration.Invoices.AddNew(); // an invoice with no overridden supplier, which uses elimination to use the importer,
				invoiceHeader3.JZ_InvoiceNumber = "3";
				invoiceHeader3.JZ_OH_Supplier = ZGuid.Empty;
				var line3 = CreateLineThatRequiresAProduct(invoiceHeader3);

				var helperCollection = new DeclarationForProductCreationHelperCollection(declaration);
				helperCollection.Create(ProductRelationDefaultOption.OptionForSupplier);

				AssertEquals(OrgPartRelation.RelationshipTypes.Owner, line1.Part.RelatedOrganisations[0].OU_Relationship);
				AssertEquals(importer.PK, line1.Part.RelatedOrganisations[0].OU_OH);

				AssertEquals(OrgPartRelation.RelationshipTypes.Supplier, line2.Part.RelatedOrganisations[0].OU_Relationship);
				AssertEquals(supplier2.PK, line2.Part.RelatedOrganisations[0].OU_OH);

				AssertEquals(OrgPartRelation.RelationshipTypes.Owner, line3.Part.RelatedOrganisations[0].OU_Relationship);
				AssertEquals(importer.PK, line3.Part.RelatedOrganisations[0].OU_OH);
			}
		}

		BaseJobComInvoiceLine CreateLineThatRequiresAProduct(BaseJobComInvoiceHeader header)
		{
			counter++;
			var line = header.InvoiceLines.AddNew();
			line.JI_InvoiceUQ = "AA";
			line.JI_PartNo = string.Concat("Apples", counter.ToString());
			line.JI_Description = "an apple...";
			line.JI_Tariff = "101010101";
			return line;
		}
		int counter;

		public void TestShouldShowWarning()
		{
			var line = CreateLineThatRequiresAProduct(invoiceHeader);
			line.JI_InvoiceUQ = ZString.Empty;
			var line2 = CreateLineThatRequiresAProduct(invoiceHeader);
			var invoiceHeader2 = Declaration.Invoices.AddNew();

			Assert(new DeclarationForProductCreationHelperCollection(Declaration).ShouldShowWarning);
		}

		public void TestShouldShowProductCreationConfirmation()
		{
			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;

			Declaration.JE_OH_Supplier = supplier.PK;
			Declaration.JE_OH_Importer = importer.PK;
			Assert(DeclarationForProductCreationHelperCollection.ShouldShowProductCreationConfirmation(Declaration));

			Declaration.JE_OH_Supplier = ZGuid.Empty;
			Declaration.JE_OH_Importer = importer.PK;
			Assert(DeclarationForProductCreationHelperCollection.ShouldShowProductCreationConfirmation(Declaration));

			Declaration.JE_OH_Importer = ZGuid.Empty;
			AssertNull(invoiceHeader.Importer_Effective);
			AssertNull(invoiceHeader.Supplier_Effective);
			Assert(!DeclarationForProductCreationHelperCollection.ShouldShowProductCreationConfirmation(Declaration));

			invoiceHeader.JZ_OH_Supplier = supplier2.PK;
			Assert(DeclarationForProductCreationHelperCollection.ShouldShowProductCreationConfirmation(Declaration));
		}

		public void TestShouldShowAutomaticProductCreationConfirmation()
		{
			using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Declaration.JE_OH_Importer = ZGuid.Empty;
				Assert(!DeclarationForProductCreationHelperCollection.ShouldShowAutomaticProductCreationConfirmation(Declaration));
				Declaration.JE_OH_Importer = importer.PK;
				importer.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.DEF;
				Assert(!DeclarationForProductCreationHelperCollection.ShouldShowAutomaticProductCreationConfirmation(Declaration));
				importer.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.YES;
				Assert(DeclarationForProductCreationHelperCollection.ShouldShowAutomaticProductCreationConfirmation(Declaration));
				importer.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.NO;
				Assert(!DeclarationForProductCreationHelperCollection.ShouldShowAutomaticProductCreationConfirmation(Declaration));
			}
			using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Declaration.JE_OH_Importer = ZGuid.Empty;
				Assert(DeclarationForProductCreationHelperCollection.ShouldShowAutomaticProductCreationConfirmation(Declaration));
				Declaration.JE_OH_Importer = importer.PK;
				importer.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.DEF;
				Assert(DeclarationForProductCreationHelperCollection.ShouldShowAutomaticProductCreationConfirmation(Declaration));
				importer.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.YES;
				Assert(DeclarationForProductCreationHelperCollection.ShouldShowAutomaticProductCreationConfirmation(Declaration));
				importer.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.NO;
				Assert(!DeclarationForProductCreationHelperCollection.ShouldShowAutomaticProductCreationConfirmation(Declaration));
			}
			using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				Declaration.JE_OH_Importer = ZGuid.Empty;
				Assert(DeclarationForProductCreationHelperCollection.ShouldShowAutomaticProductCreationConfirmation(Declaration));
				Declaration.JE_OH_Importer = importer.PK;
				importer.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.DEF;
				Assert(DeclarationForProductCreationHelperCollection.ShouldShowAutomaticProductCreationConfirmation(Declaration));
				importer.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.YES;
				Assert(DeclarationForProductCreationHelperCollection.ShouldShowAutomaticProductCreationConfirmation(Declaration));
				importer.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.NO;
				Assert(!DeclarationForProductCreationHelperCollection.ShouldShowAutomaticProductCreationConfirmation(Declaration));
			}
			using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				Declaration.JE_OH_Importer = ZGuid.Empty;
				Assert(!DeclarationForProductCreationHelperCollection.ShouldShowAutomaticProductCreationConfirmation(Declaration));
				Declaration.JE_OH_Importer = importer.PK;
				importer.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.DEF;
				Assert(!DeclarationForProductCreationHelperCollection.ShouldShowAutomaticProductCreationConfirmation(Declaration));
				importer.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.YES;
				Assert(DeclarationForProductCreationHelperCollection.ShouldShowAutomaticProductCreationConfirmation(Declaration));
				importer.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.NO;
				Assert(!DeclarationForProductCreationHelperCollection.ShouldShowAutomaticProductCreationConfirmation(Declaration));
			}
		}

		public void TestAlwaysAssumeLocalOrganisationIsBothImporterAndExporter()
		{
			CustomsDataRegistry.Instance.AlwaysAssumeLocalOrganisationIsBothImporterAndExporter.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var invoiceHeader = Declaration.Invoices.AddNew();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			helperCollection = new DeclarationForProductCreationHelperCollection(Declaration);
			AssertEquals(OrgPartRelation.RelationshipTypes.Owner, helperCollection.RelationshipTypeForImporters);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			helperCollection = new DeclarationForProductCreationHelperCollection(Declaration);
			AssertEquals(OrgPartRelation.RelationshipTypes.Supplier, helperCollection.RelationshipTypeForSuppliers);

			CustomsDataRegistry.Instance.AlwaysAssumeLocalOrganisationIsBothImporterAndExporter.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			helperCollection = new DeclarationForProductCreationHelperCollection(Declaration);
			AssertEquals(OrgPartRelation.RelationshipTypes.Both, helperCollection.RelationshipTypeForImporters);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			helperCollection = new DeclarationForProductCreationHelperCollection(Declaration);
			AssertEquals(OrgPartRelation.RelationshipTypes.Both, helperCollection.RelationshipTypeForSuppliers);
		}

		public void TestProductRelationOptionForImporterText()
		{
			AssertEquals("Relationship: OWN, Code: Importer", helperCollection.ProductRelationOptionForImporterText);

			invoiceHeader.JZ_OH_Buyer = importer2.PK;
			helperCollection = new DeclarationForProductCreationHelperCollection(Declaration);
			AssertEquals("Relationship: OWN, Multiple", helperCollection.ProductRelationOptionForImporterText);
		}

		public void TestProductRelationOptionForSupplierText()
		{
			AssertEquals("Relationship: SUP, Code: Supplier", helperCollection.ProductRelationOptionForSupplierText);

			invoiceHeader.JZ_OH_Supplier = supplier2.PK;
			helperCollection = new DeclarationForProductCreationHelperCollection(Declaration);
			AssertEquals("Relationship: SUP, Multiple", helperCollection.ProductRelationOptionForSupplierText);

			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			var invoiceHeader2 = Declaration.Invoices.AddNew();
			var line2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceHeader2.JZ_OH_Supplier = supplier2.PK;
			helperCollection = new DeclarationForProductCreationHelperCollection(Declaration);
			AssertEquals("Relationship: SUP, Multiple", helperCollection.ProductRelationOptionForSupplierText);
		}

		protected override void SetUp()
		{
			base.SetUp();

			organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "gfdasd";

			importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer";
			importer.OH_RL_NKClosestPort = "AUSYD";
			importer.OH_IsConsignee = true;
			importer.OH_IsConsignor = true;

			supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "Supplier";
			supplier.OH_RL_NKClosestPort = "AUSYD";
			supplier.OH_IsConsignor = true;
			supplier.OH_IsConsignee = true;

			importer2 = Factory.New<OrgHeader>();
			importer2.OH_Code = "Importer2";
			importer2.OH_IsConsignee = true;
			importer2.OH_IsConsignor = true;

			supplier2 = Factory.New<OrgHeader>();
			supplier2.OH_Code = "Supplier2";
			supplier2.OH_IsConsignor = true;
			supplier2.OH_IsConsignee = true;

			invoiceHeader = Declaration.Invoices.AddNew();
			_ = invoiceHeader.InvoiceLines.AddNew();
			helperCollection = new DeclarationForProductCreationHelperCollection(Declaration);
		}

		BaseJobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
					declaration.JE_OH_Importer = importer.PK;
					declaration.JE_OH_Supplier = supplier.PK;
				}
				return declaration;
			}
		}

		OrgHeader importer;
		OrgHeader supplier;

		OrgHeader importer2;
		OrgHeader supplier2;

		OrgHeader organisation;
		BaseJobDeclaration declaration;

		BaseJobComInvoiceHeader invoiceHeader;
		DeclarationForProductCreationHelperCollection helperCollection;
	}
}
