using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(AttachInvoiceCollection))]
	sealed class AttachInvoiceCollectionTest : ActiveBusinessObjectCollectionTestCase<AttachInvoiceCollection>
	{
		protected override AttachInvoiceCollection GetCollectionToTest() => new AttachInvoiceCollection(BaseJobDeclaration.New(Factory));
	}
}
