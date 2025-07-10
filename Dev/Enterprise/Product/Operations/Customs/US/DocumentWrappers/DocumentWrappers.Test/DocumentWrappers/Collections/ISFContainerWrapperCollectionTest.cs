using Enterprise.Customs.US.ISF.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(ISFContainerWrapperCollection))]
	sealed class ISFContainerWrapperCollectionTest : ContainerWrapperCollectionTest
	{
		public void TestLoadFromISF()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFEquip container1 = header.Equipments.AddNew();
			container1.BE_ContainerNum = "OOCL0000006";
			CusISFEquip container2 = header.Equipments.AddNew();
			container2.BE_ContainerNum = "OOCL0000011";
			CusISFEquip container3 = header.Equipments.AddNew();
			container3.BE_ContainerNum = "OOCL0000027";

			var collection = new ISFContainerWrapperCollection(header, Factory);

			AssertEquals("collection.Count", 3, collection.Count);
			AssertEquals("collection[0].WrappedObject", container1, collection[0].WrappedObject);
			AssertEquals("collection[1].WrappedObject", container2, collection[1].WrappedObject);
			AssertEquals("collection[2].WrappedObject", container3, collection[2].WrappedObject);
		}
	}
}
