using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.GUI
{
	public abstract class CopyRecipientsFindBox : ZGridFindBox
	{
		public string EmailAddressPropertyName { get; set; }
	}

	public class CopyRecipientsFindBox<TCopyRecipient, TCopyRecipientOwner> : CopyRecipientsFindBox where TCopyRecipient : BusinessObject where TCopyRecipientOwner : BusinessObject, ILinkable
	{
		public CopyRecipientCollection<TCopyRecipient, TCopyRecipientOwner> CopyRecipients { get; set; }

		public override void SelectFromPopupForm(bool autoSelect = false)
		{
			CopyRecipientsEmailCollectionForm.ShowDialog(CopyRecipients, EmailAddressPropertyName);
			Code = CopyRecipients.Value;
		}
	}

	public class NonPersistentCopyRecipientsFindBox : CopyRecipientsFindBox
	{
		public NonPersistentCopyRecipientCollection CopyRecipients { get; set; }

		public override void SelectFromPopupForm(bool autoSelect = false)
		{
			CopyRecipientsEmailCollectionForm.ShowDialog(CopyRecipients, EmailAddressPropertyName);
			Code = CopyRecipients.Value;
		}
	}
}
