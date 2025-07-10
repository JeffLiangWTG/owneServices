using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public class AddressOverrideColumnStyle<TCopyRecipient, TCopyRecipientCollection, TCopyRecipientOwner> : MultiEmailColumnStyle where TCopyRecipient : BusinessObject where TCopyRecipientCollection : CopyRecipientCollection<TCopyRecipient, TCopyRecipientOwner> where TCopyRecipientOwner : BusinessObject, ILinkable
	{
		public AddressOverrideColumnStyle(AddressOverrideColumnStyleInfo<TCopyRecipient, TCopyRecipientCollection, TCopyRecipientOwner> info)
			: base(() => new AddressOverrideCombinationControl<TCopyRecipient, TCopyRecipientCollection, TCopyRecipientOwner>(info), info)
		{
		}

		protected new AddressOverrideColumnStyleInfo<TCopyRecipient, TCopyRecipientCollection, TCopyRecipientOwner> ColumnInfo => (AddressOverrideColumnStyleInfo<TCopyRecipient, TCopyRecipientCollection, TCopyRecipientOwner>)base.ColumnInfo;
	}

	#region ZCodeFindBoxColumnStyleInfo

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public class AddressOverrideColumnStyleInfo<TCopyRecipient, TCopyRecipientCollection, TCopyRecipientOwner> : MultiEmailColumnStyleInfo where TCopyRecipient : BusinessObject where TCopyRecipientCollection : CopyRecipientCollection<TCopyRecipient, TCopyRecipientOwner> where TCopyRecipientOwner : BusinessObject, ILinkable
	{
		public override Type ColumnStyleType => typeof(AddressOverrideColumnStyle<TCopyRecipient, TCopyRecipientCollection, TCopyRecipientOwner>);

		public Func<TCopyRecipientOwner, TCopyRecipientCollection> GetCopyRecipients { get; set; }
	}

	#endregion
}
