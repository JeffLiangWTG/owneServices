using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class CopyRecipientsFindBoxTest : TestCaseWithFactory
	{
		public void TestGetList()
		{
			using (var findBox = new CopyRecipientsFindBox<OrgDocumentCopyRecipient, OrgDocument>())
			{
				AssertNull(findBox.List);
			}
			using (var findBox = new NonPersistentCopyRecipientsFindBox())
			{
				AssertNull(findBox.List);
			}
		}
	}
}
