using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmMenuEDocs))]
	sealed class StmMenuEDocsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSX_IndexWithDefaultValueZero()
		{
			var eDocsMenu = Factory.NewWithValidTestData<StmMenuEDocs>();
			AssertEquals(new ZByte(0), eDocsMenu.SX_Index);
		}
	}
}
