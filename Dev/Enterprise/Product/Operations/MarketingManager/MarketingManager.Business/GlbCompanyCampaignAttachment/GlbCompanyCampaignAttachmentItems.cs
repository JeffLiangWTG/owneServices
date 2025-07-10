using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignAttachmentItem : NonPersistentBusinessObject, IObsoleteValidation
	{
		public GlbCompanyCampaignAttachmentItem(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void SetLinkedDoc(IeDoc doc, bool isImage)
		{
			this.LinkedDoc = doc;
			this.IsImage = isImage;
		}

		public IeDoc LinkedDoc { get; private set; }
		public bool IsImage { get; private set; }

		#region Properties

		#region Description

		[MaxLength(128)]
		public ZString Description
		{
			get { return LinkedDoc != null ? LinkedDoc.Description : ZString.Empty; }
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}

		#endregion

		#region FileName

		[MaxLength(256)]
		public ZString FileName
		{
			get { return LinkedDoc?.FileName ?? ZString.Empty; }
		}

		public ZPropertyInfo FileNameInfo
		{
			get { return GetZPropertyInfo(nameof(FileName)); }
		}

		#endregion

		#region Selected

		public ZBool Selected
		{
			get { return selected; }
			set { SetNonPersistentPropertyValue(SelectedInfo, ref selected, value); }
		}

		ZBool selected;

		public ZPropertyInfo SelectedInfo
		{
			get { return GetZPropertyInfo(nameof(Selected)); }
		}

		#endregion

		#endregion
	}
}
