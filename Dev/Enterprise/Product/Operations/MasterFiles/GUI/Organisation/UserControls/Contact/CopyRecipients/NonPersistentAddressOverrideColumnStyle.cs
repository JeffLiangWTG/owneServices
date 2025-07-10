using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public class NonPersistentAddressOverrideColumnStyle<TCopyRecipientOwner> : MultiEmailColumnStyle where TCopyRecipientOwner : NonPersistentBusinessObject, ILinkable
	{
		public NonPersistentAddressOverrideColumnStyle(NonPersistentAddressOverrideColumnStyleInfo<TCopyRecipientOwner> columnInfo)
			: base(() => new NonPersistentAddressOverrideCombinationControl<TCopyRecipientOwner>(columnInfo), columnInfo)
		{
		}

		protected new NonPersistentAddressOverrideColumnStyleInfo<TCopyRecipientOwner> ColumnInfo => (NonPersistentAddressOverrideColumnStyleInfo<TCopyRecipientOwner>)base.ColumnInfo;
	}

	public class NonPersistentAddressOverrideColumnStyleInfo<TCopyRecipientOwner> : MultiEmailColumnStyleInfo where TCopyRecipientOwner : NonPersistentBusinessObject, ILinkable
	{
		public override Type ColumnStyleType => typeof(NonPersistentAddressOverrideColumnStyle<TCopyRecipientOwner>);

		public Func<TCopyRecipientOwner, NonPersistentCopyRecipientCollection> GetCopyRecipients { get; set; }
	}
}
