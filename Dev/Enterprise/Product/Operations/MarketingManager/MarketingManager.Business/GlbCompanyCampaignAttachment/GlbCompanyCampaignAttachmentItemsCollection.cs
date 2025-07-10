using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignAttachmentItemCollection : NonPersistentBusinessObjectCollection<GlbCompanyCampaignAttachmentItem>
	{
		public GlbCompanyCampaignAttachmentItemCollection(GlbCompanyCampaign parent)
			: base(parent.Factory)
		{
			this.Parent = parent;
			parent.Factory.Saving += new BusinessObjectFactory.SavingEventHandler(Factory_Saving);
		}

		readonly GlbCompanyCampaign Parent;

		#region Saving

		void Factory_Saving(BusinessObjectFactory factory)
		{
			if (!Parent.IsDeleted)
			{
				StoreAttachments();
				Synchronise();
			}
		}

		void StoreAttachments()
		{
			List<ZString> attachmentNames = new List<ZString>();
			foreach (GlbCompanyCampaignAttachmentItem item in this)
			{
				if (item.Selected)
				{
					attachmentNames.Add(item.FileName);
				}
			}

			Parent.G0_AttachmentList = new OCsvLine(attachmentNames.Select(name => (string)name).ToArray()).ToString();
		}

		#endregion

		#region New

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GlbCompanyCampaignAttachmentItem(Factory);
		}

		#endregion

		#region Load / Sycnrhonise

		public override void Load()
		{
			Synchronise();

			string[] attachments = new OCsvLine(Parent.G0_AttachmentList).FieldValues;
			foreach (ZString attachmentName in attachments)
			{
				foreach (GlbCompanyCampaignAttachmentItem item in this)
				{
					if (attachmentName == item.Description || attachmentName == item.FileName) // using both for backwards compatability
					{
						using (item.SuspendSettingHasChanges())
						{
							item.Selected = true;
							break;
						}
					}
				}
			}
		}

		public void Synchronise()
		{
			List<GlbCompanyCampaignAttachmentItem> newAttachments = new List<GlbCompanyCampaignAttachmentItem>();
			newAttachments.AddRange(GetAttachmentsFromEDocs(Parent.DocManagerInfo.Files, false));
			newAttachments.AddRange(GetAttachmentsFromEDocs(Parent.DocManagerInfo.Documents, true));

			RemoveAll();
			AddRange(newAttachments.ToArray());
		}

		List<GlbCompanyCampaignAttachmentItem> GetAttachmentsFromEDocs(IStorageDocsBaseCollection collection, bool isImage)
		{
			List<GlbCompanyCampaignAttachmentItem> newAttachments = new List<GlbCompanyCampaignAttachmentItem>();

			foreach (IeDoc eDoc in (BusinessObjectCollection)collection)
			{
				bool eDocFound = false;

				foreach (GlbCompanyCampaignAttachmentItem item in this)
				{
					if (item.LinkedDoc != null && item.LinkedDoc.UniqueKey == eDoc.UniqueKey)
					{
						newAttachments.Add(item);
						eDocFound = true;
					}
				}

				if (!eDocFound)
				{
					GlbCompanyCampaignAttachmentItem newItem = new GlbCompanyCampaignAttachmentItem(Factory);
					newItem.SetLinkedDoc(eDoc, isImage);
					newAttachments.Add(newItem);
				}
			}

			return newAttachments;
		}

		#endregion
	}
}
