using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.BIRD.Testing
{
	sealed class BRDZCTest : TestCaseWithFactory
	{
		public void TestCreateContainerIfNecessary()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CusContainers.AddNew().CO_ContainerNumber = "1";

			BRDZC zc = new BRDZC();
			zc.ContainerNumber = "1";
			zc.ContainerType = "A";
			zc.ContainerNumber2 = "2";
			zc.ContainerType = "B";

			((IBIRDHeaderRecord)zc).Update(declaration, new NotificationCollection());

			AssertEquals("Two containers expected", 2, declaration.CusContainers.Count);

			AssertNotNull(declaration.CusContainers.Find("1"));
			AssertNotNull(declaration.CusContainers.Find("2"));
		}
	}
}
