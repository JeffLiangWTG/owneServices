using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class DeclarationForProductCreationHelperTest : TestCaseWithFactory
	{
		#region Organizations

		public void TestImporter()
		{
			AssertEquals(Declaration.Importer.PK, importer.PK);
			AssertEquals(helper.Importer.PK, importer.PK);
			invoiceHeader.JZ_OH_Buyer = importer2.PK;
			helper = new DeclarationForProductCreationHelper(invoiceHeader);
			AssertEquals("Importer2 should be selected, as it is the most relevant importer (i.e the effective importer)", helper.Importer.PK, importer2.PK);
		}

		public void TestSupplier()
		{
			AssertEquals(Declaration.Supplier.PK, supplier.PK);
			AssertEquals(helper.Supplier.PK, supplier.PK);
			invoiceHeader.JZ_OH_Supplier = supplier2.PK;
			helper = new DeclarationForProductCreationHelper(invoiceHeader);
			AssertEquals("Supplier2 should be selected, as it is the most relevant supplier (i.e the effective supplier)", helper.Supplier.PK, supplier2.PK);

			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			var invoiceHeader2 = Declaration.Invoices.AddNew();
			var line2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceHeader2.JZ_OH_Supplier = supplier2.PK;
			helper = new DeclarationForProductCreationHelper(invoiceHeader2);
			AssertEquals(helper.Supplier.PK, supplier2.PK);
		}

		#endregion

		#region DefaultingLayers

		public void TestSetDefaultOptionForCreateProduct_Elimination()
		{
			Declaration.JE_MessageType = ZString.Empty;
			Assert("If this fails, change the message type to something that is neither import nor export", !Declaration.IsImport && !Declaration.IsExport);

			AssertEquals(ProductRelationDefaultOption.None, helper.CalculateDefaultOptionForCreateProduct());
			AssertNullOrEmpty(helper.BehaviorDefaultedFromText);
			Assert(helper.CanChangeTheDefaultOption);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_OH_Supplier = ZGuid.Empty;
			this.invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			helper = new DeclarationForProductCreationHelper(this.invoiceHeader);
			AssertNull(this.invoiceHeader.Supplier_Effective);
			AssertNotNull(this.invoiceHeader.Importer_Effective);

			AssertEquals(ProductRelationDefaultOption.OptionForImporter, helper.CalculateDefaultOptionForCreateProduct());
			AssertNullOrEmpty(helper.BehaviorDefaultedFromText);
			Assert(!helper.CanChangeTheDefaultOption);

			Declaration.JE_OH_Importer = ZGuid.Empty;
			helper = new DeclarationForProductCreationHelper(this.invoiceHeader);
			AssertNull(this.invoiceHeader.Importer_Effective);
			AssertNull(this.invoiceHeader.Supplier_Effective);

			AssertEquals(ProductRelationDefaultOption.None, helper.CalculateDefaultOptionForCreateProduct());
			AssertNullOrEmpty(helper.BehaviorDefaultedFromText);
			Assert(helper.CanChangeTheDefaultOption);

			Declaration.JE_OH_Supplier = supplier.PK;
			AssertNotNull(this.invoiceHeader.Supplier_Effective);
			helper = new DeclarationForProductCreationHelper(this.invoiceHeader);

			AssertEquals(ProductRelationDefaultOption.OptionForSupplier, helper.CalculateDefaultOptionForCreateProduct());
			AssertNullOrEmpty(helper.BehaviorDefaultedFromText);
			Assert(!helper.CanChangeTheDefaultOption);

			Declaration.JE_OH_Importer = importer.PK;
			Declaration.SetSupportsBondedWarehousingForTesting(true);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = Declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			Assert(Declaration.IsInwardBondedWarehousingEnabledForSingleOrMultipleEntry);
			helper = new DeclarationForProductCreationHelper(invoiceHeader);

			helper.CalculateDefaultOptionForCreateProduct();
			AssertEquals("If no elimination methods are available, then it should fall through to the 'IntegratedBondedWarehouse' Layer", "Behavior defaulted from: Warehousing Defaults", helper.BehaviorDefaultedFromText);
		}

		public void TestSetDefaultOptionForCreateProduct_IntegratedBondedWarehouse()
		{
			Declaration.SetSupportsBondedWarehousingForTesting(true);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = Declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			Assert(Declaration.IsInwardBondedWarehousingEnabledForSingleOrMultipleEntry);
			helper = new DeclarationForProductCreationHelper(invoiceHeader);

			AssertEquals(ProductRelationDefaultOption.OptionForImporter, helper.CalculateDefaultOptionForCreateProduct());
			AssertEquals("Behavior defaulted from: Warehousing Defaults", helper.BehaviorDefaultedFromText);
			Assert(!helper.CanChangeTheDefaultOption);

			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
			Assert(!Declaration.IsInwardBondedWarehousingEnabledForSingleOrMultipleEntry);
			helper = new DeclarationForProductCreationHelper(invoiceHeader);

			var link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = importer.PK;
			link.OL_OH_Supplier = supplier.PK;
			link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
			link.OL_ProductRelation = OrgRelationTypeList.Codes.Supplier;
			Factory.Save();
			helper = new DeclarationForProductCreationHelper(this.invoiceHeader);

			helper.CalculateDefaultOptionForCreateProduct();
			AssertEquals("If no entries going into a integrated bonded warehouse exist, then it should fall through to the SupplierImporterLink layer", "Behavior defaulted from: Importer/Supplier Relationship", helper.BehaviorDefaultedFromText);
		}

		public void TestSetDefaultOptionForCreateProduct_SupplierImporterLink()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				Declaration.JE_RL_NKFinalDestination = "USLAX";

				var link = Factory.New<OrgSupplierBuyerLink>();
				link.OL_OH_Buyer = importer.PK;
				link.OL_OH_Supplier = supplier.PK;
				link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
				link.OL_ProductRelation = OrgRelationTypeList.Codes.Supplier;
				Factory.Save();

				AssertEquals(ProductRelationDefaultOption.OptionForSupplier, helper.CalculateDefaultOptionForCreateProduct());
				AssertEquals("Behavior defaulted from: Importer/Supplier Relationship", helper.BehaviorDefaultedFromText);
				Assert(!helper.CanChangeTheDefaultOption);

				link.OL_ProductRelation = OrgRelationTypeList.Codes.Importer;
				Factory.Save();
				helper = new DeclarationForProductCreationHelper(invoiceHeader);

				AssertEquals(ProductRelationDefaultOption.OptionForImporter, helper.CalculateDefaultOptionForCreateProduct());
				AssertEquals("Behavior defaulted from: Importer/Supplier Relationship", helper.BehaviorDefaultedFromText);
				Assert(!helper.CanChangeTheDefaultOption);

				var link2 = Factory.New<OrgSupplierBuyerLink>();
				link2.OL_OH_Buyer = importer.PK;
				link2.OL_OH_Supplier = supplier.PK;
				link2.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;
				link2.OL_ProductRelation = OrgRelationTypeList.Codes.Supplier;
				Factory.Save();
				helper = new DeclarationForProductCreationHelper(invoiceHeader);

				AssertEquals(ProductRelationDefaultOption.OptionForSupplier, helper.CalculateDefaultOptionForCreateProduct());
				AssertEquals("Behavior defaulted from: Importer/Supplier Relationship", helper.BehaviorDefaultedFromText);
				Assert(!helper.CanChangeTheDefaultOption);

				link.Delete();
				link2.Delete();
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				importer.MiscServ.OM_IMOwnsProducts = true;
				helper = new DeclarationForProductCreationHelper(invoiceHeader);

				helper.CalculateDefaultOptionForCreateProduct();
				AssertEquals("If no applicable OrgSupplierBuyer Links exist, then it should fall through to the Client Organization layer", "Behavior defaulted from: Importer", helper.BehaviorDefaultedFromText);
			}
		}

		public void TestSetDefaultOptionForCreateProduct_ClientOrganization()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importer.MiscServ.OM_IMOwnsProducts = true;

			AssertEquals(ProductRelationDefaultOption.OptionForImporter, helper.CalculateDefaultOptionForCreateProduct());
			AssertEquals("Behavior defaulted from: Importer", helper.BehaviorDefaultedFromText);
			Assert(!helper.CanChangeTheDefaultOption);

			importer.MiscServ.OM_IMOwnsProducts = false;
			helper = new DeclarationForProductCreationHelper(invoiceHeader);
			helper.CalculateDefaultOptionForCreateProduct();
			AssertEquals("If OM_IMOwnsProducts = false, then the defaulting should fall through to the registry", "Behavior defaulted from: Registry", helper.BehaviorDefaultedFromText);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			supplier.MiscServ.OM_EXOwnsProducts = true;
			helper = new DeclarationForProductCreationHelper(invoiceHeader);

			AssertEquals(ProductRelationDefaultOption.OptionForSupplier, helper.CalculateDefaultOptionForCreateProduct());
			AssertEquals("Behavior defaulted from: Supplier", helper.BehaviorDefaultedFromText);
			Assert(!helper.CanChangeTheDefaultOption);

			supplier.MiscServ.OM_EXOwnsProducts = false;
			helper = new DeclarationForProductCreationHelper(invoiceHeader);
			helper.CalculateDefaultOptionForCreateProduct();
			AssertEquals("If OM_EXOwnsProducts = false, then the defaulting should fall through to the registry", "Behavior defaulted from: Registry", helper.BehaviorDefaultedFromText);
		}

		public void TestSetDefaultOptionForCreateProduct_Registry()
		{
			AssertDefaultOptionByRegistrySetting(ClientProductCreationTypeList.Codes.Never, ProductRelationDefaultOption.OptionForSupplier, ProductRelationDefaultOption.OptionForSupplier, false);

			AssertDefaultOptionByRegistrySetting(ClientProductCreationTypeList.Codes.No, ProductRelationDefaultOption.OptionForSupplier, ProductRelationDefaultOption.OptionForSupplier, true);

			AssertDefaultOptionByRegistrySetting(ClientProductCreationTypeList.Codes.Yes, ProductRelationDefaultOption.OptionForImporter, ProductRelationDefaultOption.OptionForSupplier, true);

			AssertDefaultOptionByRegistrySetting(ClientProductCreationTypeList.Codes.Always, ProductRelationDefaultOption.OptionForImporter, ProductRelationDefaultOption.OptionForSupplier, false);

			void AssertDefaultOptionByRegistrySetting(string clientProductCreationType, ProductRelationDefaultOption importOption, ProductRelationDefaultOption exportOption, bool canChangeTheDefaultOption)
			{
				CustomsDataRegistry.Instance.AssumeCreatedProductBelongsToClient.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, clientProductCreationType);
				var invoiceHeader = Declaration.Invoices.AddNew();

				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				helper = new DeclarationForProductCreationHelper(invoiceHeader);
				AssertEquals(clientProductCreationType, importOption, helper.CalculateDefaultOptionForCreateProduct());
				AssertEquals(clientProductCreationType, "Behavior defaulted from: Registry", helper.BehaviorDefaultedFromText);
				AssertEquals(clientProductCreationType, canChangeTheDefaultOption, helper.CanChangeTheDefaultOption);

				Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				helper = new DeclarationForProductCreationHelper(invoiceHeader);
				AssertEquals(clientProductCreationType, exportOption, helper.CalculateDefaultOptionForCreateProduct());
				AssertEquals(clientProductCreationType, "Behavior defaulted from: Registry", helper.BehaviorDefaultedFromText);
				AssertEquals(clientProductCreationType, canChangeTheDefaultOption, helper.CanChangeTheDefaultOption);
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			organization = Factory.New<OrgHeader>();
			organization.OH_Code = "gfdasd";

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
			helper = new DeclarationForProductCreationHelper(invoiceHeader);
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

		OrgHeader organization;
		BaseJobDeclaration declaration;

		BaseJobComInvoiceHeader invoiceHeader;
		DeclarationForProductCreationHelper helper;

		#endregion
	}
}
