using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(CusContainerCollection))]
	public class CusContainerCollectionTest : Customs.Business.Testing.CusContainerCollectionTest
	{
		public void TestAllowNew()
		{
			AssertEquals("Max Count 100", 100, CusContainerCollection.MaxCount);
		}

		public override void TestCountChanged()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.CusContainers.AddNew();
			AssertEquals((short)1, declaration.JE_ContainerCount);
			declaration.CusContainers.AddNew();
			AssertEquals((short)2, declaration.JE_ContainerCount);
			declaration.CusContainers.AddNew();
			AssertEquals((short)3, declaration.JE_ContainerCount);
			declaration.CusContainers.RemoveAll();
			AssertEquals((short)0, declaration.JE_ContainerCount);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return CusContainerCollection;
		}

		CusContainerCollection CusContainerCollection
		{
			get
			{
				return cusContainerCollection ?? (cusContainerCollection = new CusContainerCollection(Factory.New<JobDeclaration>(), Factory));
			}
		}

		CusContainerCollection cusContainerCollection;
	}
}
