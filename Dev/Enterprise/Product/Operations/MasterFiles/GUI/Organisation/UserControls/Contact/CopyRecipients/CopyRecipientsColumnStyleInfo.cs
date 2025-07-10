using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	/// <summary>
	/// Defines the base class for copy recipients column style info.
	/// </summary>
	public abstract class CopyRecipientsColumnStyleInfo : ZCodeFindBoxColumnStyleInfo
	{
		protected CopyRecipientsColumnStyleInfo()
		{
			CharacterCasing = CharacterCasing.Lower;
		}

		public string EmailAddressPropertyName { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public class CopyRecipientsColumnStyleInfo<TCopyRecipient, TCopyRecipientCollection, TCopyRecipientOwner> : CopyRecipientsColumnStyleInfo where TCopyRecipient : BusinessObject where TCopyRecipientCollection : CopyRecipientCollection<TCopyRecipient, TCopyRecipientOwner> where TCopyRecipientOwner : BusinessObject, ILinkable
	{
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(CopyRecipientsColumnStyle<TCopyRecipient, TCopyRecipientCollection, TCopyRecipientOwner>); }
		}

		public Func<TCopyRecipientOwner, TCopyRecipientCollection> GetCopyRecipients { get; set; }
	}

	public class NonPersistentCopyRecipientsColumnStyleInfo<TCopyRecipientOwner> : CopyRecipientsColumnStyleInfo where TCopyRecipientOwner : NonPersistentBusinessObject, ILinkable
	{
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(NonPersistentCopyRecipientsColumnStyle<TCopyRecipientOwner>); }
		}

		public Func<TCopyRecipientOwner, NonPersistentCopyRecipientCollection> GetCopyRecipients { get; set; }
	}
}
