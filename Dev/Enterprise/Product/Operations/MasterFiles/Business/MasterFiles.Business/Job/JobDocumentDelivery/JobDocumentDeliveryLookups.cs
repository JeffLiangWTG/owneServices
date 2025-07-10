using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocumentDeliveryLookups : AutoJobDocumentDeliveryLookups
	{
		public JobDocumentDeliveryLookups(AutoJobDocumentDelivery parent)
			: base(parent)
		{
		}

		#region Document Groups

		public CodeDescriptionPairList JDC_DocumentGroup_List => OrgCodeLists.ContactType_List;

		#endregion

		#region Delivery Methods

		public CodeDescriptionPairList JDC_DeliveryMethod_List
		{
			get
			{
				var list = new CodeDescriptionPairList(OLookUpEditType.NotifyMode);
				list.AddPair(Core.Constants.ContactNotifyModes.DoNotDeliver, Res.GetString("1c78b910-9e12-41f6-b30d-5e3da7d973cb", "Do Not Deliver"));
				return list;
			}
		}

		#endregion

		#region Attachment Types

		public CodeDescriptionPairList JDC_AttachmentType_List => OrgCodeLists.AttachmentType_List;

		#endregion
	}
}
