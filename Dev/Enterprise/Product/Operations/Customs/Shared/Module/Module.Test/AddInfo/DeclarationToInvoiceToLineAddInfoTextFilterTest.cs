using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(DeclarationToInvoiceToLineAddInfoTextFilter))]
	sealed class DeclarationToInvoiceToLineAddInfoTextFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DeclarationToInvoiceToLineAddInfoTextFilter("TestDescription", "FooBar");
		}
	}
}
