using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccAlternateGLAccount))]
	public class AccAlternateGLAccountAuditParentTest : AuditParentTest<AccAlternateGLAccount>
	{
		protected override AccAlternateGLAccount NewTestAuditParent()
		{
			return Factory.New<AccAlternateGLAccount>();
		}
	}
}
