using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InvoiceRelatedDeclarationGenPivotCollection))]
	sealed class InvoiceRelatedDeclarationGenPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			return new InvoiceRelatedDeclarationGenPivotCollection(invoice);
		}

		#endregion
	}
}
