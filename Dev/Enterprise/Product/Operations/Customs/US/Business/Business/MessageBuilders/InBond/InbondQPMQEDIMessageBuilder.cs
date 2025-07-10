using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	/// <summary>
	/// BillLevelDelete is when an entry is lodged with more than one bill and users want to delete one of the bills.
	/// </summary>
	public enum InBondQPMessageType { Original, Delete, BillLevelDelete }

	public static class InBondActionList
	{
		public const string AddInBond = "A";
		public const string DeleteInBondFromBill = "B";
		public const string DeleteInBondFromAllAssociatedBills = "D";
		public const string AddBill = "A";
		public const string DeleteBill = "D";
	}

	public static class InBondQPMessageBuilderConstants
	{
		public const string FTZDefaultPOLCode = "99999";
	}

	public class InBondQPMessageBuilder
	{
		public InBondQPMessageBuilder(IInBondQPHeader inBondHeader, InBondQPMessageType messageType)
		{
			this.messageType = messageType;
			this.inBondHeader = inBondHeader;
		}

		public InBondQPMessageBuilder(IInBondQPHeader inBondHeader, IMessageAttachee inBondBill, IInBondBillDetails moveDetail, InBondQPMessageType messageType)
		{
			this.messageType = messageType;
			this.inBondHeader = inBondHeader;
			this.inBondBill = inBondBill;
			this.moveDetail = moveDetail;
		}

		readonly InBondQPMessageType messageType;
		readonly IInBondQPHeader inBondHeader;
		readonly IMessageAttachee inBondBill;
		readonly IInBondBillDetails moveDetail;

		#region IMessageBuilder Members

		public MQEDIMessage PopulateMessage()
		{
			var block = new ACEInputBlockControlGenerator(inBondHeader);
			block.B.ApplicationIdentifierCode = ACEApplicationIdentifierCodeList.Codes.InbondTransaction;

			if (messageType == InBondQPMessageType.BillLevelDelete)
			{
				block.AddMessageBlocks(new ACEInbondQPMessageBlockBuilder(inBondHeader).Build(messageType, moveDetail));
			}
			else
			{
				block.AddMessageBlocks(new ACEInbondQPMessageBlockBuilder(inBondHeader).Build(messageType));
			}

			MQEDIMessage message = block.CreateMessage<MQEDIMessage>(inBondHeader.Factory);
			SetMessageSubType(message);
			SetMessageOwner(message);

			return message;
		}

		void SetMessageSubType(MQEDIMessage message)
		{
			if (messageType == InBondQPMessageType.Original)
			{
				message.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureOriginal;
				inBondHeader.MessageStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			}
			else if (messageType == InBondQPMessageType.Delete)
			{
				message.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureDelete;
				if (inBondHeader.MessageStatus != ImportMessageStatusList.Codes.AwaitingDepartureAmendment)
				{
					inBondHeader.MessageStatus = ImportMessageStatusList.Codes.AwaitingDepartureWithdraw;
				}
			}
			else if (messageType == InBondQPMessageType.BillLevelDelete && inBondBill != null)
			{
				message.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureDelete;
				inBondBill.MessageStatus = ImportMessageStatusList.Codes.AwaitingDepartureWithdraw;
			}
			else
			{
				message.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureReplacement;
				inBondHeader.MessageStatus = ImportMessageStatusList.Codes.AwaitingDepartureAmendment;
			}
		}

		void SetMessageOwner(MQEDIMessage message)
		{
			message.EM_MessageOwner = Constants.ACE;
		}

		#endregion
	}
}
