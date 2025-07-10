using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccBankAccount))]
	public class AccBankAccountAuditParentTest : AuditParentTest<AccBankAccount>
	{
		protected override AccBankAccount NewTestAuditParent()
		{
			return Factory.New<AccBankAccount>();
		}
	}
}
