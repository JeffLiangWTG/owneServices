using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using OrgSupplierPart = Enterprise.MasterFiles.Business.OrgSupplierPart;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(ProductCreationConfirmationForm))]
	sealed class ProductCreationConfrimationFormTest : ZFormBasherTest
	{
		public void TestProductCreationConfirmationForm_InvoicesContainProductWithMissingInvoiceUnitOfMeasure_WarningLabel_IsVisible()
		{
			var declaration = CreateJobDeclarationWithInvoiceHeaderAndLines();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0];
			var invoiceLine = invoiceHeader.JobComInvoiceLines[0];
			invoiceLine.JI_InvoiceUQ = string.Empty;

			using (var form = new ProductCreationConfirmationForm(declaration))
			{
				form.Show();
				Assert("WarningLabel is visible when there is any product missing invoice UQ", form.WarningLabel.Visible);
			}
		}

		public void TestProductCreationConfirmationForm_InvoicesContainProductAllHaveInvoiceUnitOfMeasure_WarningLabel_IsInvisible()
		{
			var declaration = CreateJobDeclarationWithInvoiceHeaderAndLines();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0];
			using (var form = new ProductCreationConfirmationForm(declaration))
			{
				form.Show();
				Assert("WarningLabel is not visible when all products have invoice UQ", !form.WarningLabel.Visible);
			}
		}

		public void TestCreateProduct()
		{
			var declaration = CreateJobDeclarationWithInvoiceHeaderAndLines();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0];
			using (var form = new ProductCreationConfirmationForm(declaration))
			{
				form.Show();
				form.ProductRelationOptionForImporter.Checked = true;
				form.AcceptButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				AssertEquals(
					"3 new products should be created",
					"PART 1 (description 1), PART 2 (description 2), PART 3 (description 3)",
					string.Join(", ", declaration.Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.StartsWith, "PART ")).Select(
						p => $"{p.OP_PartNum} ({p.OP_Desc})"
					).OrderBy(r => r))
				);
			}
		}

		public void TestCreateProduct_WithDuplicate()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "DUPTSTORG";
			Factory.Save();

			var declaration = CreateJobDeclarationWithInvoiceHeaderAndLines();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0];
			declaration.JE_OH_Importer = importer.PK;

			using (var form = new ProductCreationConfirmationForm(declaration))
			{
				form.Show();
				form.ProductRelationOptionForImporter.Checked = true;

				// in this scenario another user creates product with same code and owner
				var existingProductPK = ZGuid.NewZGuid();
				var existingRelationPK = ZGuid.NewZGuid();
				TestConnection.ExecuteNonQuery($@"
					insert into dbo.OrgSupplierPart (OP_PK, OP_PartNum, OP_Desc)
					values ('{existingProductPK}', 'PART 2', 'Existing Product');
					insert into dbo.OrgPartRelation (OU_PK, OU_OP, OU_OH, OU_Relationship, OU_SystemCreateTimeUtc, OU_SystemCreateUser, OU_SystemLastEditTimeUtc, OU_SystemLastEditUser)
					values ('{existingRelationPK}', '{existingProductPK}', '{declaration.Importer.PK}', 'OWN', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				");

				form.AcceptButton.PerformClick();

				AssertEquals(
					"The following products were marked for creation but have not been created as they would create duplicates. This could be caused by the following\r\n" +
					"1. Product already exists for this Owner but with a different Supplier. To fix this, remove all the Suppliers, or add the new Supplier to the existing product (F4, filter on Importer only and then edit the product).\r\n" +
					"2. These products could have been subsequently created by another user. To fix this, erase and re-enter the product code to synchronize the newly created product with the invoice line, or use a different product code.\r\n\r\n" +
					"part 2 (Owner = DUPTSTORG, Supplier = )",
					UnitTestUserNotification.Instance.LastMessage.Text
				);
				AssertEquals(
					"1 existing product should be visible to declaration factory and 2 new products should be created",
					"PART 1 (description 1), PART 2 (Existing Product), PART 3 (description 3)",
					string.Join(", ", declaration.Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.StartsWith, "PART ")).Select(
						p => $"{p.OP_PartNum} ({p.OP_Desc})"
					).OrderBy(r => r))
				);
				AssertEquals("dialog result is same as usual - saving should not be prevented", DialogResult.OK, form.DialogResult);
			}
		}

		public void TestCreateProduct_WithManyDuplicates()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "DUPTSTIMP";
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "DUPTSTSUP";
			Factory.Save();

			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;

			var invoiceHeader = declaration.Invoices.AddNew();

			const int duplicateCount = 11;

			for (int i = 1; i <= duplicateCount; i++)
			{
				var line = invoiceHeader.JobComInvoiceLines.AddNew();
				line.JI_PartNo = $"DUPTEST{i:D2}";
				line.JI_Description = "Duplicate Product " + i;
				line.JI_Tariff = "T2";
				line.JI_CC = ZGuid.NewZGuid();
				line.JI_InvoiceUQ = "UNT";
			}

			using (var form = new ProductCreationConfirmationForm(declaration))
			{
				form.Show();
				form.ProductRelationOptionForImporter.Checked = true;

				// in this scenario another user creates product with same code and owner
				StringBuilder sql = new StringBuilder();
				for (int i = 1; i <= duplicateCount; i++)
				{
					var existingProductPK = ZGuid.NewZGuid();
					var existingRelationPK = ZGuid.NewZGuid();
					sql.Append($@"
						insert into dbo.OrgSupplierPart (OP_PK, OP_PartNum, OP_Desc)
						values ('{existingProductPK}', 'DUPTEST{i:D2}', 'Existing Product {i}');
						insert into dbo.OrgPartRelation (OU_PK, OU_OP, OU_OH, OU_Relationship, OU_SystemCreateTimeUtc, OU_SystemCreateUser, OU_SystemLastEditTimeUtc, OU_SystemLastEditUser)
						values ('{existingRelationPK}', '{existingProductPK}', '{declaration.Importer.PK}', 'OWN', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
					");
				}

				TestConnection.ExecuteNonQuery(sql.ToString());

				form.AcceptButton.PerformClick();

				AssertEquals(
					"The following products were marked for creation but have not been created as they would create duplicates. This could be caused by the following\r\n" +
					"1. Product already exists for this Owner but with a different Supplier. To fix this, remove all the Suppliers, or add the new Supplier to the existing product (F4, filter on Importer only and then edit the product).\r\n" +
					"2. These products could have been subsequently created by another user. To fix this, erase and re-enter the product code to synchronize the newly created product with the invoice line, or use a different product code.\r\n" +
					"\r\n" +
					"DUPTEST01 (Owner = DUPTSTIMP, Supplier = )\r\n" +
					"DUPTEST02 (Owner = DUPTSTIMP, Supplier = )\r\n" +
					"DUPTEST03 (Owner = DUPTSTIMP, Supplier = )\r\n" +
					"DUPTEST04 (Owner = DUPTSTIMP, Supplier = )\r\n" +
					"DUPTEST05 (Owner = DUPTSTIMP, Supplier = )\r\n" +
					"DUPTEST06 (Owner = DUPTSTIMP, Supplier = )\r\n" +
					"DUPTEST07 (Owner = DUPTSTIMP, Supplier = )\r\n" +
					"DUPTEST08 (Owner = DUPTSTIMP, Supplier = )\r\n" +
					"DUPTEST09 (Owner = DUPTSTIMP, Supplier = )\r\n" +
					"DUPTEST10 (Owner = DUPTSTIMP, Supplier = )\r\n" +
					"...",
					UnitTestUserNotification.Instance.LastMessage.Text
				);
				AssertEquals("dialog result is same as usual - saving should not be prevented", DialogResult.OK, form.DialogResult);
			}
		}

		public void TestCheckBoxStatusByRegistrySetting()
		{
			var invoiceHeader = Declaration.Invoices.AddNew();

			CustomsDataRegistry.Instance.AssumeCreatedProductBelongsToClient.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ClientProductCreationTypeList.Codes.Never);
			AssertCheckBoxStatus(false, true, false);

			CustomsDataRegistry.Instance.AssumeCreatedProductBelongsToClient.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ClientProductCreationTypeList.Codes.No);
			AssertCheckBoxStatus(false, true, true);

			CustomsDataRegistry.Instance.AssumeCreatedProductBelongsToClient.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ClientProductCreationTypeList.Codes.Yes);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertCheckBoxStatus(true, false, true);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertCheckBoxStatus(false, true, true);

			CustomsDataRegistry.Instance.AssumeCreatedProductBelongsToClient.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ClientProductCreationTypeList.Codes.Always);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertCheckBoxStatus(true, false, false);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertCheckBoxStatus(false, true, false);
		}

		public void TestConfigureConfirmButtonEnabled()
		{
			var declaration = CreateJobDeclarationWithInvoiceHeaderAndLines();
			var invoiceHeader = declaration.Invoices[0];
			CombineAssertions("If there form has no selection made, and there is an option to pick from, then the Confirm Button should be disabled (i.e. unable to be clicked)...", () =>
			{
				OpenFormAndCheckConfirmButtonEnabled(declaration, "No options are shown (or selected), thus we should allow the user to contine and generate with no org.", false, false, false, false, true);
				OpenFormAndCheckConfirmButtonEnabled(declaration, "Both Importer and Supplier are shown, but neither are selected, thus the button should be disabled", true, false, true, false, false);

				OpenFormAndCheckConfirmButtonEnabled(declaration, "Only Supplier is shown, and it is not selected, thus the button should be disabled.", false, false, true, false, false);
				OpenFormAndCheckConfirmButtonEnabled(declaration, "Only Supplier is shown, and it is selected, thus the button should be enabled.", false, false, true, true, true);

				OpenFormAndCheckConfirmButtonEnabled(declaration, "Only Importer is shown, and it is not selected, thus the button should be disabled.", true, false, false, false, false);
				OpenFormAndCheckConfirmButtonEnabled(declaration, "Only Importer is shown, and it is selected, thus the button should be enabled.", true, true, false, false, true);

				OpenFormAndCheckConfirmButtonEnabled(declaration, "Both Importer and Supplier are shown, and the Supplier is selected, thus the button should be enabled", true, false, true, true, true);
				OpenFormAndCheckConfirmButtonEnabled(declaration, "Both Importer and Supplier are shown, and the Importer is selected, thus the button should be enabeld", true, true, true, false, true);
			});
		}

		public void TestProductRelationOptionsVisibility()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = Declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			AssertVisibility(true, true);

			Declaration.JE_OH_Importer = ZGuid.Empty;
			AssertVisibility(false, true);

			Declaration.JE_OH_Importer = importer.PK;
			Declaration.JE_OH_Supplier = ZGuid.Empty;
			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			AssertVisibility(true, false);
		}

		protected override Form GetFormToBashCore() => new ProductCreationConfirmationForm(Declaration);

		protected override bool ShouldIgnoreMissingBindingMember(Control control) => true;

		protected override bool AllowHasChangesOnFormOpen => true;

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
		}

		void AssertCheckBoxStatus(bool optionForImporterChecked, bool optionForSupplierChecked, bool enable)
		{
			using (var form = new ProductCreationConfirmationForm(Declaration))
			{
				form.Show();
				AssertEquals(form.ProductRelationOptionForImporter.Checked, optionForImporterChecked);
				AssertEquals(form.ProductRelationOptionForSupplier.Checked, optionForSupplierChecked);

				AssertEquals(form.ProductRelationOptionForImporter.Enabled, enable);
				AssertEquals(form.ProductRelationOptionForSupplier.Enabled, enable);
			}
		}

		void OpenFormAndCheckConfirmButtonEnabled(BaseJobDeclaration declaration, string message, bool impVis, bool impChk, bool supVis, bool supChk, bool btnEnabled = false)
		{
			using (var form = new ProductCreationConfirmationFormExposed(declaration))
			{
				form.Show();

				form.ProductRelationOptionForImporter.Visible = impVis;
				form.ProductRelationOptionForImporter.Checked = impChk;

				form.ProductRelationOptionForSupplier.Visible = supVis;
				form.ProductRelationOptionForSupplier.Checked = supChk;

				form.ConfigureConfirmButtonEnabledExposed();

				AssertEquals(message, btnEnabled, form.Controls.Find("ConfirmButton", true).Single().Enabled);
			}
		}

		BaseJobDeclaration CreateJobDeclarationWithInvoiceHeaderAndLines()
		{
			var declarationSupplier = OrgHeader.New(Factory);
			declarationSupplier.OH_Code = "SUPPLIER";
			declarationSupplier.OH_IsConsignor = true;
			declarationSupplier.MainAddress.OA_Address1 = "Add1";

			var declarationImporter = OrgHeader.New(Factory);
			declarationImporter.OH_Code = "IMPORTER";
			declarationImporter.OH_IsConsignee = true;
			declarationImporter.MainAddress.OA_Address1 = "Add1";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = declarationSupplier.PK;
			declaration.JE_OH_Importer = declarationImporter.PK;

			var invoiceHeader = declaration.Invoices.AddNew();

			for (var i = 1; i <= 3; i++)
			{
				var line = invoiceHeader.JobComInvoiceLines.AddNew();
				line.JI_PartNo = "part " + i;
				line.JI_Description = "description " + i;
				line.JI_Tariff = "T2";
				line.JI_CC = ZGuid.NewZGuid();
				line.JI_InvoiceUQ = "PKG";
			}

			return declaration;
		}

		void AssertVisibility(bool optionForImporterVisible, bool optionForSupplierVisible)
		{
			using (var form = new ProductCreationConfirmationForm(Declaration))
			{
				form.Show();
				AssertEquals(form.ProductRelationOptionForImporter.Visible, optionForImporterVisible);
				AssertEquals(form.ProductRelationOptionForSupplier.Visible, optionForSupplierVisible);
			}
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
		OrgHeader organisation;
		BaseJobDeclaration declaration;

		sealed class ProductCreationConfirmationFormExposed : ProductCreationConfirmationForm
		{
			public ProductCreationConfirmationFormExposed(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			public void ConfigureConfirmButtonEnabledExposed() => ConfigureConfirmButtonEnabled();
		}
	}
}
