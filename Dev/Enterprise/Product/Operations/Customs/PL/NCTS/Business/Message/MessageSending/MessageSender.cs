using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.PL.NCTS.Business;

public abstract class MessageSender
{
	protected MessageSender(MessageSendingObject sendingObject)
	{
		this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		nctsHeader = Argument.NotNull(sendingObject.NctsHeader, nameof(sendingObject.NctsHeader));
	}

	protected readonly MessageSendingObject sendingObject;

	protected readonly NctsHeader nctsHeader;

	protected abstract ZString MessageType { get; }

	protected abstract ZString MessageSubType { get; }

	protected abstract ZString PhaseCode { get; }

	protected abstract IXmlMessageBuilder GetMessageBuilder();

	protected virtual ZString GetPrettyView(EDIMessage message, NctsHeader header) => ZString.Empty;

	public void Send()
	{
		PreSend();

		var messageBuilder = GetMessageBuilder();
		var xmlMessageStream = messageBuilder.GenerateXmlMessage().GetSerializedStream();

		var newMessage = nctsHeader.Factory.New<EDIMessage>();
		newMessage.EM_MessageType = MessageType;
		newMessage.EM_MessageSubType = MessageSubType;
		newMessage.EM_ApplicationReference = ZString.Empty;
		newMessage.EM_Status = EDIMessage.Status.Queued;
		newMessage.EM_SystemLastEditUser = GlbStaff.CurrentUser.GS_Code;
		newMessage.EM_SystemLastEditTimeUtc = ZDateTime.UtcNow;
		newMessage.EM_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
		newMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
		newMessage.EM_GB = GlbBranch.CurrentBranch.PK;
		newMessage.EM_GE = GlbDepartment.CurrentDepartment.PK;
		newMessage.SetEM_MessageTextOrDataSource(xmlMessageStream);
		newMessage.EM_IsActive = false;
		newMessage.EM_IsTestMessage = true;
		newMessage.EM_GP = GetPLCustomsPasswordPK();
		newMessage.EM_MessageInterpretation = GetPrettyView(newMessage, nctsHeader);

		var phaseCode = PhaseCode;
		if (!phaseCode.IsEmpty)
		{
			((NctsCommonMovementHeader)nctsHeader.ArrivalMovementHeader ?? nctsHeader.MovementHeader).BM_Phase = phaseCode;
		}

		if (nctsHeader.MovementHeader is NctsDepartureMovementHeader movementHeader)
		{
			newMessage.EM_LinkedObject = movementHeader;
			movementHeader.Messages.Add(newMessage);
		}
		else
		{
			newMessage.EM_LinkedObject = nctsHeader;
			nctsHeader.Messages.Add(newMessage);
		}
	}

	ZGuid GetPLCustomsPasswordPK() => GlbStaff.CurrentUser.GetPLWrapper()?.PLBPassword.PK ?? ZGuid.Empty;

	protected virtual void PreSend()
	{
	}
}
