using System.IO;
using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.ExitControl.Business;

public abstract class ExitControlMessageSender(ExitControlMessageSendingObject sendingObject)
{
	public void Send()
	{
		var message = (EDIMessage)ExitReport.Messages.AddNew(typeof(EDIMessage));

		message.EM_SendWithMessageErrors = false;
		message.EM_ApplicationCode = ApplicationCode;
		message.EM_ApplicationReference = ApplicationReference;
		message.EM_MessageType = MessageType;
		message.EM_MessageSubType = MessageSubType;
		message.EM_MessageOwner = ZString.Empty;

		message.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		message.EM_Status = EDIMessage.Status.Queued;
		message.EM_SystemLastEditUser = GlbStaff.CurrentUser.GS_Code;
		var utcNow = ZDateTime.UtcNow;
		message.EM_SystemLastEditTimeUtc = utcNow;
		message.EM_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
		message.EM_SystemCreateTimeUtc = utcNow;
		message.EM_GB = GlbBranch.CurrentBranch.PK;
		message.EM_GE = GlbDepartment.CurrentDepartment.PK;
		message.EM_GP = GlbStaff.CurrentUser.GetPLWrapper()?.PLBPassword.PK ?? ZGuid.Empty;

		if (sendingObject is not null)
		{
			message.EM_LinkedObject = sendingObject.MessagingObject;
		}

		if (GetXmlMessageBuilder()?.GenerateXmlMessage().GetSerializedStream() is Stream serializedMessage)
		{
			message.SetEM_MessageTextOrDataSource(serializedMessage);
		}

		LastSentMessage = message;

		AfterSendCore();
	}

	public virtual void AfterSendCore()
	{
	}

	public EDIMessage LastSentMessage { get; private set; }

	protected abstract IXmlMessageBuilder GetXmlMessageBuilder();

	protected ExitControlMessageSendingObject SendingObject { get; } = Argument.NotNull(sendingObject, nameof(sendingObject));

	static ZString ApplicationCode => ApplicationCodeList.Codes.PLCustomsExitControl;

	protected internal virtual ZString ApplicationReference => ZString.Empty;

	static ZString MessageType => EdiMessageMessageType.ExitControl;

	public CusExitReport ExitReport => (CusExitReport)sendingObject.MessagingObject;

	public JobDeclaration Declaration => (JobDeclaration)ExitReport.Header.Declaration;

	public CusEntryHeader EntryHeader => Declaration?.CustomsEntryHeaders
		.FirstOrDefault(entryHeader => entryHeader.MovementReferenceNumber == ExitReport.Consignment.CXC_MovementReference);

	protected internal abstract ZString MessageSubType { get; }
}
