using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgContactsStmALogModule.StmALogCollectionForTest))]
	sealed class StmALogCollectionForTestTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgContactsStmALogModule.StmALogCollectionForTest(Factory);
		}
	}
}
