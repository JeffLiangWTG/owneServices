using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business.DocumentWrappers;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.ZA;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Customs.ZA.Business.Documents.DocDataObjects.Testing
{
	sealed class CustomsDocDataObjectProviderTest : TestCaseWithFactory
	{
		public void TestGetDocDataObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var provider = new CustomsDocDataObjectProvider();
			var parametersMock = new Mock<IStmALogProvider>();
			var dataObject = provider.GetDocDataObject(declaration, DataContext.CargoDuesBrokerage, new DocDataObjectParameters("Cargo Dues Brokerage - Load Coastwise", "b", parametersMock.Object));
			AssertNotNull(dataObject);
			AssertType(typeof(CargoDues), dataObject);
		}

		public void TestGetDocDataObject_DA66DA63Document()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ActiveEntryHeaders.AddNew();
			var provider = new CustomsDocDataObjectProvider();

			var dataObject = provider.GetDocDataObject(declaration, DataContext.DA66DA63Document, null);
			AssertNotNull(dataObject);
			AssertType(typeof(DA66DA63DocumentWrapper), dataObject);
		}

		public void TestGetDocDataObject_DA306Document()
		{
			var declaration = Factory.New<JobDeclaration>();
			var provider = new CustomsDocDataObjectProvider();
			var dataObject = provider.GetDocDataObject(declaration, DataContext.DA306Document, null);
			AssertNotNull(dataObject);
			AssertType(typeof(DA306DeclarationWrapper), dataObject);
		}
	}
}
