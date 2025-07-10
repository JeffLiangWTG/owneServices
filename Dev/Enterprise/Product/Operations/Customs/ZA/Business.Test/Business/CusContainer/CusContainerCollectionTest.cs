using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusContainerCollection))]
	sealed class CusContainerCollectionTest : Customs.Business.Testing.CusContainerCollectionTest
	{
		public void TestSetDefaultsForNewChild()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var containerCollection = new CusContainerCollection(declaration, Factory);
			var container = containerCollection.AddNew();
			Assert(container != null);
			AssertEquals("Container number", "TBA1", container.CO_ContainerNumber);
			var container2 = containerCollection.AddNew();
			container2.CO_ContainerNumber = "TRIU9679791";
			AssertEquals("Container number", "TRIU9679791", container2.CO_ContainerNumber);
			var container3 = containerCollection.AddNew();
			AssertEquals("Container number", "TBA2", container3.CO_ContainerNumber);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new CusContainerCollection(declaration, Factory);
		}
	}
}
