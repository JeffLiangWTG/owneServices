using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class CopyRecipientsColumnStyleInfoTest : TestCaseWithFactory
	{
		public void TestColumnStyleType()
		{
			AssertEquals("The column style info should point to the correct column style.", typeof(CopyRecipientsColumnStyle<OrgDocumentCopyRecipient, OrgDocumentCopyRecipientCollection, OrgDocument>), new CopyRecipientsColumnStyleInfo<OrgDocumentCopyRecipient, OrgDocumentCopyRecipientCollection, OrgDocument>().ColumnStyleType);
			AssertEquals("The non-persistent column style info should point to the correct column style.", typeof(NonPersistentCopyRecipientsColumnStyle<DocDeliveryContact>), new NonPersistentCopyRecipientsColumnStyleInfo<DocDeliveryContact>().ColumnStyleType);
		}
	}
}
