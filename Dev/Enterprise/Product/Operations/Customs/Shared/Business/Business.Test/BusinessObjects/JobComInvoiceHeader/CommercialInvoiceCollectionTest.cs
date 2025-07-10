using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CommercialInvoiceCollection))]
	sealed class CommercialInvoiceCollectionTest : ActiveBusinessObjectCollectionTestCase<CommercialInvoiceCollection>
	{
		public void TestGroupInvoiceFilter()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var inv1 = declaration.Invoices.AddNew();
			var inv2 = declaration.AllGroupHeaders[0];
			var inv3 = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();

			var collection = new CommercialInvoiceCollection(Factory);
			collection.AdditionalFilter = new ZQuery(JobComInvoiceHeaderSchema.PK, new ZGuid[]
			{
				inv1.PK,
				inv2.PK,
				inv3.PK
			});

			AssertContainsExactElementsInAnyOrder(collection, new[]
			{
				inv1,
				inv3
			});
		}
	}
}
