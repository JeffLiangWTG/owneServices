using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business;

public class MessageManager : MultiMessageManager
{
	public MessageManager(MessageSendingObjectParent declarationWrapper, IMessageNotificationCollector notification)
	{
		this.declarationWrapper = Argument.NotNull(declarationWrapper, nameof(declarationWrapper));
		this.notification = Argument.NotNull(notification, nameof(notification));
	}

	readonly MessageSendingObjectParent declarationWrapper;

	public override IMessageManageableBizObj TopLevelBizObjToManage => declarationWrapper.ParentDeclaration;

	protected override bool SendWheneverPossibleOnceMessagingActive => true;

	readonly IMessageNotificationCollector notification;

	protected override SingleMessageManager[] GetAllMessageManagers()
	{
		return declarationWrapper.SendingObjectsCollection
			.Cast<MessageSendingObject>()
			.Where(sendingObject => sendingObject.ShouldSend)
			.Select(sendingObject => new CUSDECMessageManager(sendingObject, notification))
			.ToArray<SingleMessageManager>();
	}

	public void SendMessages()
	{
		notification.Notifications?.Clear();
		foreach (var messageManager in GetAllMessageManagers().Cast<CUSDECMessageManager>())
		{
			var header = messageManager.Header;
			var oldBGMReference = header.CH_BGMReference;
			PopulateCH_BGMReferenceIfRequired(header);
			Factory.Save();

			var result = messageManager.SendMessage();
			if (!result)
			{
				header.CH_BGMReference = oldBGMReference;
				Factory.Save();
			}
		}
	}

	void PopulateCH_BGMReferenceIfRequired(CusEntryHeader header)
	{
		var declarationDate = UpdateAndGetCEI_DateForDuty(header);
		if (header.CH_BGMReference.IsEmpty)
		{
			GenerateBGMReference(header, declarationDate);
		}
		else
		{
			UpdateBGMReference(header, declarationDate);
		}
	}

	void GenerateBGMReference(CusEntryHeader header, ZDateTime declarationDate)
	{
		var referenceNumberWrapper = new ReferenceNumberWrapper(header.CH_BGMReference);
		referenceNumberWrapper.GenerateFrom(Factory, header.Declaration?.DeclarantAddress?.Header, declarationDate, header.CH_BGMReference);
		header.CH_BGMReference = referenceNumberWrapper.ReferenceNumber;
	}

	void UpdateBGMReference(CusEntryHeader header, ZDateTime declarationDate)
	{
		var oldValue = header.CH_BGMReference;
		var referenceNumberWrapper = new ReferenceNumberWrapper(header.CH_BGMReference);
		referenceNumberWrapper.TryIncrementVersion();
		if (!referenceNumberWrapper.IsValid)
		{
			referenceNumberWrapper.GenerateFrom(Factory, header.Declaration?.DeclarantAddress?.Header, declarationDate, oldValue);
		}
		header.CH_BGMReference = referenceNumberWrapper.ReferenceNumber;
	}

	ZDateTime UpdateAndGetCEI_DateForDuty(CusEntryHeader header)
	{
		var entryInstruction = header.EntryInstruction;
		if (entryInstruction == null)
		{
			return ZDateTime.UtcNow;
		}

		if (entryInstruction.CEI_DateForDuty.IsEmpty)
		{
			entryInstruction.CEI_DateForDuty = ZDateTime.UtcNow;
		}

		return entryInstruction.CEI_DateForDuty;
	}
}
