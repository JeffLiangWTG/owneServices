using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ContainerHandlingRateClassListTest : TestCase
	{
		public void TestParameterlessConstructor()
		{
			CodeDescriptionPairList list = new ContainerHandlingRateClassList();
			AssertEquals("List.Count", 57, list.Count);
		}
	}
}
