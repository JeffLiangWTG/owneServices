using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing;

[TestedType(typeof(StmTemplate))]
public class StmTemplateAuditParentTest : AuditParentTest<StmTemplate>
{
	protected override StmTemplate NewTestAuditParent()
	{
		return Factory.New<StmTemplate>();
	}
}