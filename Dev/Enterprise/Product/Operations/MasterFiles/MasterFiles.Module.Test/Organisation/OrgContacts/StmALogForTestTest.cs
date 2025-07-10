using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgContactsStmALogModule.StmALogForTest))]
	sealed class StmALogForTestTest : StmALogTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<OrgContactsStmALogModule.StmALogForTest>();
		}
	}
}
