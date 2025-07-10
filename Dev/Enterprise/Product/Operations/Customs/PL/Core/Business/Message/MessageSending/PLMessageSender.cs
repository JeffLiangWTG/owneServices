using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.PL.Business;

public abstract class PLMessageSender : PLMessageSender<object>
{
	protected PLMessageSender(BusinessObjectFactory factory, BaseMessageSendingObjectParent sendingObjectParent)
		: base(factory)
	{
		SendingObjectParent = Argument.NotNull(sendingObjectParent, nameof(sendingObjectParent));
	}

	protected BaseMessageSendingObjectParent SendingObjectParent { get; }

	public EDIMessage Send() => SendInContext(sendOperationContext: default);

	protected override ZBool SendWithMessageErrors => SendingObjectParent.AllowSendWithError;
}

public abstract class PLMessageSender<TSendOperationContext>
{
	protected PLMessageSender(BusinessObjectFactory factory)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
	}

	protected readonly BusinessObjectFactory factory;

	protected abstract ZString MessageType { get; }

	protected abstract ZString ApplicationCode { get; }

	protected abstract ZString ApplicationReference { get; }

	protected abstract ZBool SendWithMessageErrors { get; }

	public EDIMessage SendInContext(TSendOperationContext sendOperationContext)
	{
		var message = factory.New<EDIMessage>();
		SendCoreInContextCore(sendOperationContext, message);
		return message;
	}

	protected virtual void SendCoreInContextCore(TSendOperationContext context, EDIMessage message) => SendCore(message);

	protected virtual void SendCore(EDIMessage message)
	{
		message.EM_SendWithMessageErrors = SendWithMessageErrors;
		message.EM_ApplicationCode = ApplicationCode;
		message.EM_ApplicationReference = ApplicationReference;
		message.EM_MessageType = MessageType;
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

		message.EM_IsTestMessage = true;
	}
}
