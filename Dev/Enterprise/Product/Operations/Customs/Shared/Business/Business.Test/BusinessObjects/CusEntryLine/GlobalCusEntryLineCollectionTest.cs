using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(GlobalCusEntryLineCollection))]
	sealed class GlobalCusEntryLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlobalCusEntryLineCollection(Factory);
		}

		public void TestImporter()
		{
			AssertEquals(importer, filterCollection.Importer);
		}

		public void TestDefaultModuleFilterFields()
		{
			Assert(filterCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer:Property"));
		}

		public void TestPartAndTariff()
		{
			filterCollection = new GlobalCusEntryLineCollection(Factory, invoiceLine);
			AssertEquals(importer, filterCollection.Importer);
			AssertEquals(product, filterCollection.LinePart);
			AssertEquals("12345678", filterCollection.TariffCode);
		}

		public void TestDefaultModuleFilterFieldsWithPart()
		{
			filterCollection = new GlobalCusEntryLineCollection(Factory, invoiceLine);
			Assert(filterCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer:Property"));
			Assert(filterCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Product:Property"));
			Assert(!filterCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Tariff Number:Property"));
			invoiceLine.JI_PartNo = ZString.Empty;
			filterCollection = new GlobalCusEntryLineCollection(Factory, invoiceLine);
			Assert(filterCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer:Property"));
			Assert(!filterCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Product:Property"));
			Assert(filterCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Tariff Number:Property"));
		}

		#region Implementation
		GlobalCusEntryLineCollection filterCollection;
		OrgHeader importer;
		BaseJobDeclaration declaration;
		BaseJobComInvoiceLine invoiceLine;
		OrgSupplierPart product;

		protected override void SetUp()
		{
			base.SetUp();
			importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer";
			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TestPartNum";
			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_Relationship = "OWN";
			relation.OU_OH = importer.PK;
			invoiceLine.JI_PartNo = product.OP_PartNum;
			invoiceLine.JI_Tariff = "12345678";
			filterCollection = new GlobalCusEntryLineCollection(Factory, declaration);
		}
		#endregion
	}
}
