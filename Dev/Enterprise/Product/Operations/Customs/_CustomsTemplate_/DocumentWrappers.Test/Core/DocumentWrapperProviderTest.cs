using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs._CustomsTemplate_.Business;

namespace Enterprise.Customs._CustomsTemplate_.DocumentWrappers.Testing
{
	sealed class DocumentWrapperProviderTest : TestCaseWithFactory
	{
		public void TestI_CustomsTemplate_DocumentWrapperProviderMembers()
		{
			var provider = ObjectFactory.Get<Integration.Customs._CustomsTemplate_.IDocumentWrapperProvider>();
			AssertType<DocumentWrapperProvider>(provider);
			AssertType<DocDeclaration>("NewDocDeclaration", provider.NewDocDeclaration(Factory.New<JobDeclaration>(), Factory));
			AssertType<DocJobComInvoiceLine>("NewDocJobComInvoiceLine", provider.NewDocJobComInvoiceLine(Factory.New<JobComInvoiceLine>(), Factory));
		}
	}
}
