using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgDocumentCopyRecipientCollection : CopyRecipientCollection<OrgDocumentCopyRecipient, OrgDocument>
	{
		public OrgDocumentCopyRecipientCollection(OrgDocument orgDocument, string orgDocumentCopyRecipientType) : base(orgDocument, orgDocumentCopyRecipientType, OrgDocumentCopyRecipientSchema.ODR_EmailAddress, OrgDocumentCopyRecipientSchema.ODR_RecipientType)
		{
		}

		protected override void SetDefaultsForNewElementCore(OrgDocumentCopyRecipient newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			if (newElement != null && Owner != null)
			{
				newElement.ODR_OD = Owner.PK;
			}
		}
	}
}