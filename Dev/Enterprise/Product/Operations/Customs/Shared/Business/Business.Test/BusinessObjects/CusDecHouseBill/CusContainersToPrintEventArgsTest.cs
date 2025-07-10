using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusContainersToPrintEventArgsTest : TestCaseWithFactory
	{
		public void TestCusContainersToPrintEventArgs()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			DocumentCusContainerCollectionHeader documentCusContainerCollectionHeader = new DocumentCusContainerCollectionHeader(new DocumentCusContainerCollection(Factory, declaration.CusContainers));
			CusContainersToPrintEventArgs e = new CusContainersToPrintEventArgs(documentCusContainerCollectionHeader);
			AssertSame("DocumentCusContainerHeader is same as EventArgs DocumentCusContainerHeader", documentCusContainerCollectionHeader, e.DocumentCusContainerCollectionHeader);
			AssertEquals("Continue to print is true", true, e.ContinueToPrint);
		}
	}
}
