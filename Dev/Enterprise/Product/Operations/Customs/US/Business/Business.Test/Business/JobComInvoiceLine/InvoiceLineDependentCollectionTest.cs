using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(InvoiceLineDependentCollection))]
	sealed class InvoiceLineDependentCollectionTest : Customs.Business.Testing.InvoiceLineDependentCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new InvoiceLineDependentCollection(Factory.New<JobComInvoiceHeader>());
	}
}
