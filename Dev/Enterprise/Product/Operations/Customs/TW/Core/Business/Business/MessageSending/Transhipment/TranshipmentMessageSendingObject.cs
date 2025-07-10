using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class TranshipmentMessageSendingObject : AutoTranshipmentMessageSendingObject
	{
		public TranshipmentMessageSendingObject(CusInBondHeader header, ZString messageType) : base(header.Factory)
		{
			MessageType = messageType;
			Header = Argument.NotNull(header, nameof(header));
		}

		public CusInBondHeader Header { get; }

		public ZString MessageType { get; }

		public override ZString MessageStatus => Header.BH_MessageStatus;

		public override ZString EntryStatus => Header.BH_ReleaseStatus;

		public override ZString EntryNumber => Header.EntryNumber;

		[List(nameof(ActionList))]
		public override ZString Action { get => base.Action; set => base.Action = value; }

		#region Lookup Lists
		public virtual CodeDescriptionPairList ActionList => Factory.GetCachedValue<ActionCodeList>();
		#endregion

		public override void ValidateShouldSend()
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateShouldSend();
			}
		}

		public virtual ZString GetMessageOwner()
		{
			return ZString.Empty;
		}
	}
}
