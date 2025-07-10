using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusPackageCusPackableItemRelationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackTypes()
		{
			AssertSame(packageCusPackableItemRelation.PackableItem.Lookups.PackTypes, Lookups.PackTypes);
		}

		public void TestWeightUQs()
		{
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), Lookups.WeightUQs);
		}

		CusPackageCusPackableItemRelationLookups Lookups => packageCusPackableItemRelation.Lookups;
		BaseJobComInvoiceLine invoiceLine;
		CusPackageCusPackableItemRelation packageCusPackableItemRelation;

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			Factory.Save();
			var cusPackage = cusPackingList.PackageJob.Packages.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			var cusPackableItem = Factory.New<CusPackableItem>();
			cusPackableItem.CUI_JI = invoiceLine.PK;
			packageCusPackableItemRelation = new CusPackageCusPackableItemRelation(cusPackage, cusPackableItem);
		}
	}
}
