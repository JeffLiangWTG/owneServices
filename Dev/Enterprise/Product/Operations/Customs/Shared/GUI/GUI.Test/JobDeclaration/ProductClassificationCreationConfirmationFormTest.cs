using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using OrgSupplierPart = Enterprise.MasterFiles.Business.OrgSupplierPart;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(ProductClassificationCreationConfirmationForm))]
	sealed class ProductClassificationCreationConfirmationFormTest : ZFormBasherTest
	{
		public void TestOnLoad()
		{
			using (var form = new ProductClassificationCreationConfirmationForm(Declaration))
			{
				form.Show();
				AssertEquals("No Existing Product Classification To Match", form.Text);
				AssertEquals("Products for which there is no classification that matches the line on the invoice have been identified on this customs declaration.\r\n\r\nAll necessary classifications will be added to the products", (form.Controls.Find("InformationLabel", true)[0] as ZArchitecture.ZLabel).Text);
			}
		}

		public void TestConfirm()
		{
			using (var form = new ProductClassificationCreationConfirmationForm(Declaration))
			{
				form.Show();
				var part = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].InvoiceLines[0].Part;
				var classifications = Factory.Load<BaseCusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, part.PK));
				Assert(!classifications.Any());
				(form.Controls.Find("ConfirmButton", true)[0] as ZButton).PerformClick();
				classifications = Factory.Load<BaseCusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, part.PK));
				AssertEquals(1, classifications.Length);
				var classification = classifications.Single();
				AssertEquals("123", classification.TariffNumber);
			}
		}

		public void TestCancel()
		{
			using (var form = new ProductClassificationCreationConfirmationForm(Declaration))
			{
				form.Show();
				var part = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].InvoiceLines[0].Part;
				var classifications = Factory.Load<BaseCusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, part.PK));
				Assert(!classifications.Any());
				(form.Controls.Find("QuitButton", true)[0] as ZButton).PerformClick();
				classifications = Factory.Load<BaseCusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, part.PK));
				Assert(!classifications.Any());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declarationSupplier = OrgHeader.New(Factory);
			declarationSupplier.OH_Code = "SUPPLIER";
			declarationSupplier.OH_IsConsignor = true;
			declarationSupplier.MainAddress.OA_Address1 = "Add1";
			var declarationImporter = OrgHeader.New(Factory);
			declarationImporter.OH_Code = "IMPORTER";
			declarationImporter.OH_IsConsignee = true;
			declarationImporter.MainAddress.OA_Address1 = "Add1";
			Declaration.JE_OH_Supplier = declarationSupplier.PK;
			Declaration.JE_OH_Importer = declarationImporter.PK;
			var cusClassification = ZGuid.NewZGuid();
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "SomePart";
			part.OP_Desc = "Description of product";
			part.OP_StockKeepingUnit = Core.Constants.PkgUnit.Bag;
			var relOrg = part.RelatedOrganisations.AddNew();
			relOrg.OU_OH = Declaration.Supplier.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			var invoiceHeader = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invLine1 = invoiceHeader.InvoiceLines.AddNew();
			invLine1.JI_PartNo = part.OP_PartNum;
			invLine1.JI_Tariff = "123";
			var existingCusClassPartPivots = Factory.Load<BaseCusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, part.PK));
			Assert(!existingCusClassPartPivots.Any());
		}

		protected override Form GetFormToBashCore() => new ProductClassificationCreationConfirmationForm(Declaration);

		BaseJobDeclaration declaration;
		BaseJobDeclaration Declaration => declaration ?? (declaration = Factory.New<BaseJobDeclaration>());
	}
}
