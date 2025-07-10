using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISInvoiceLineRangeCollection))]
	sealed class DISInvoiceLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DISInvoiceLineRangeCollection>
	{
		protected override DISInvoiceLineRangeCollection GetCollectionToTest() => new DISInvoiceLineRangeCollection(Invoice);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DISInvoiceLineRange(Invoice);

		DISInvoice invoice;
		DISInvoice Invoice
		{
			get
			{
				var jobDeclaration = new TestHelper(Factory).GetJobDeclaration();
				var hostWrapper = new DISHostWrapper((MasterFiles.Business.DIS.IUSDISHost)jobDeclaration);
				var disDocument = new DISDocument(hostWrapper);
				return invoice ?? (invoice = new DISInvoice(disDocument));
			}
		}
	}
}
