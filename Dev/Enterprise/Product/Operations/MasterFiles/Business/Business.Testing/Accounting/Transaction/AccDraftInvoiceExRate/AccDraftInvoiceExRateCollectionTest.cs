using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccDraftInvoiceExRateCollection))]
	public class AccDraftInvoiceExRateCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new AccDraftInvoiceExRateCollection(Factory);
	}
}
