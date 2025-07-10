using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(DocCusPackageCusPackableItemRelationCollection))]
	sealed class DocCusPackageCusPackableItemRelationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocCusPackageCusPackableItemRelationCollection>
	{
		protected override DocCusPackageCusPackableItemRelationCollection GetCollectionToTest()
		{
			return new DocCusPackageCusPackableItemRelationCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			var cusPackage = cusPackingList.PackageJob.Packages.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cusPackableItem = cusPackingList.PackableItems.AddNew();
			cusPackableItem.CUI_JI = invoiceLine.PK;
			var relation = new CusPackageCusPackableItemRelation(cusPackage, cusPackableItem);
			return DocCusPackageCusPackableItemRelation.New(relation, Factory);
		}
	}
}
