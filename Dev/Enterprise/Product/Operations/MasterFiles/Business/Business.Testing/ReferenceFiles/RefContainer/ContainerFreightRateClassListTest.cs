using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ContainerFreightRateClassListTest : TestCase
	{
		public void TestParameterlessConstructor()
		{
			var list = new ContainerFreightRateClassList();
			AssertEquals("List.Count", 57, list.Count);
		}
	}
}
