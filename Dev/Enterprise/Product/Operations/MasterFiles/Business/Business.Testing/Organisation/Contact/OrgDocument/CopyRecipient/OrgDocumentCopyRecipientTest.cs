using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgDocumentCopyRecipient))]
	public class OrgDocumentCopyRecipientTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}
	}
}
