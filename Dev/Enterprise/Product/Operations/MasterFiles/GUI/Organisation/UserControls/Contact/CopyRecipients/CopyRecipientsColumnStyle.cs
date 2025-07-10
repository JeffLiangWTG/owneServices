using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public class CopyRecipientsColumnStyle<TCopyRecipient, TCopyRecipientCollection, TCopyRecipientOwner> : ZCodeFindBoxColumnStyle where TCopyRecipient : BusinessObject where TCopyRecipientCollection : CopyRecipientCollection<TCopyRecipient, TCopyRecipientOwner> where TCopyRecipientOwner : BusinessObject, ILinkable
	{
		public CopyRecipientsColumnStyle(CopyRecipientsColumnStyleInfo<TCopyRecipient, TCopyRecipientCollection, TCopyRecipientOwner> columnInfo) : this(() => new CopyRecipientsFindBox<TCopyRecipient, TCopyRecipientOwner>(), columnInfo)
		{
		}

		public CopyRecipientsColumnStyle(Func<CopyRecipientsFindBox<TCopyRecipient, TCopyRecipientOwner>> gridFindBox, CopyRecipientsColumnStyleInfo<TCopyRecipient, TCopyRecipientCollection, TCopyRecipientOwner> columnInfo) : base(gridFindBox, columnInfo)
		{
			InsertCurrentUsersEmailAddressHotKey.Register(this, GridControl);
		}

		protected new CopyRecipientsColumnStyleInfo<TCopyRecipient, TCopyRecipientCollection, TCopyRecipientOwner> ColumnInfo => (CopyRecipientsColumnStyleInfo<TCopyRecipient, TCopyRecipientCollection, TCopyRecipientOwner>)base.ColumnInfo;

		protected new CopyRecipientsFindBox<TCopyRecipient, TCopyRecipientOwner> FindBox => (CopyRecipientsFindBox<TCopyRecipient, TCopyRecipientOwner>)base.FindBox;

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string displayText, bool cellIsVisible)
		{
			if (!IsEditing)
			{
				var parentBusinessObject = source.List != null && source.List.Count > rowNum && rowNum >= 0 ? source.List[rowNum] as BusinessObject : null;
				if (parentBusinessObject != null)
				{
					FindBox.CopyRecipients = ColumnInfo.GetCopyRecipients((TCopyRecipientOwner)parentBusinessObject);
					FindBox.EmailAddressPropertyName = ColumnInfo.EmailAddressPropertyName;
					MaybeBindTextTemplatesFactory(FindBox.CodeBox, source, MappingName);
				}
			}

			base.Edit(source, rowNum, bounds, readOnly, displayText, cellIsVisible);
		}
	}

	public class NonPersistentCopyRecipientsColumnStyle<TCopyRecipientOwner> : ZCodeFindBoxColumnStyle where TCopyRecipientOwner : NonPersistentBusinessObject, ILinkable
	{
		public NonPersistentCopyRecipientsColumnStyle(NonPersistentCopyRecipientsColumnStyleInfo<TCopyRecipientOwner> columnInfo) : this(() => new NonPersistentCopyRecipientsFindBox(), columnInfo)
		{
		}

		public NonPersistentCopyRecipientsColumnStyle(Func<NonPersistentCopyRecipientsFindBox> gridFindBox, NonPersistentCopyRecipientsColumnStyleInfo<TCopyRecipientOwner> columnInfo) : base(gridFindBox, columnInfo)
		{
			InsertCurrentUsersEmailAddressHotKey.Register(this, GridControl);
		}

		protected new NonPersistentCopyRecipientsColumnStyleInfo<TCopyRecipientOwner> ColumnInfo => (NonPersistentCopyRecipientsColumnStyleInfo<TCopyRecipientOwner>)base.ColumnInfo;

		protected new NonPersistentCopyRecipientsFindBox FindBox => (NonPersistentCopyRecipientsFindBox)base.FindBox;

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string displayText, bool cellIsVisible)
		{
			if (!IsEditing)
			{
				var parentBusinessObject = source.List != null && source.List.Count > rowNum && rowNum >= 0 ? source.List[rowNum] as BusinessObject : null;
				if (parentBusinessObject != null)
				{
					FindBox.CopyRecipients = ColumnInfo.GetCopyRecipients((TCopyRecipientOwner)parentBusinessObject);
					FindBox.EmailAddressPropertyName = ColumnInfo.EmailAddressPropertyName;
					MaybeBindTextTemplatesFactory(FindBox.CodeBox, source, MappingName);
				}
			}
			base.Edit(source, rowNum, bounds, readOnly, displayText, cellIsVisible);
		}
	}
}
