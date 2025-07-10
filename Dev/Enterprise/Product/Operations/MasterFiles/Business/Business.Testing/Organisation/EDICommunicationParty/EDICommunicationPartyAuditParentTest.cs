using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EDICommunicationParty))]
	public class EDICommunicationPartyAuditParentTest : AuditParentTest<EDICommunicationParty>
	{
		protected override EDICommunicationParty NewTestAuditParent()
		{
			return Factory.New<EDICommunicationParty>();
		}
	}
}
