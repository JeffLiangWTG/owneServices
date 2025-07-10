using System;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.Customs.NO.MessageContracts.EMMA;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Argument = CargoWise.Common.Argument;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Customs.NO.Business;

class EmmaMessageGeneratorProcessor(CusEntryHeader entryHeader) : IProcessor
{
	readonly CusEntryHeader entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));

	void IProcessor.Process(INotifications notifications, CancellationToken token)
	{
		using (DisposableEnvironment.ForBranch(entryHeader.RegistryBranchPK))
		{
			var entryHeaderPK = entryHeader.PK;
			try
			{
				notifications.Add(NotificationType.Information, FormattableString.Invariant($"'EMMA' Message generation started for EntryHeader with PK [{entryHeaderPK}]."));
				var xmlMessage = GetEmmaXmlMessage(entryHeader);
				var ediMessage = AddEmmaMessageToEntryHeader(xmlMessage);
				entryHeader.Factory.Save();
				notifications.Add(NotificationType.Information, FormattableString.Invariant($"'EMMA' Message with PK [{ediMessage.PK}] generated for EntryHeader with PK [{entryHeaderPK}]."));
			}
			catch
			{
				notifications.Add(NotificationType.Error, FormattableString.Invariant($"'EMMA' Message generation failed for EntryHeader with PK [{entryHeaderPK}]."));
				throw;
			}
		}
	}

	protected virtual IXmlMessage GetEmmaXmlMessage(CusEntryHeader cusEntryHeader)
	{
		var dataProvider = EmmaMessageDataProvider.CreateProvider(new(cusEntryHeader), new EmmaMessageAdditionalDataProvider());
		IXmlMessageBuilder messageBuilder = new EmmaSystemsFortollingMessageBuilder(dataProvider);
		var message = messageBuilder.GenerateXmlMessage();
		return message;
	}

	EmmaEDIMessage AddEmmaMessageToEntryHeader(IXmlMessage xmlMessage)
	{
		var emmaMessage = entryHeader.Factory.New<EmmaEDIMessage>();
		emmaMessage.EM_MessageText = xmlMessage.GetSerializedString();
		emmaMessage.EM_ApplicationCode = Messaging.Integration.ApplicationCodeList.Codes.NOCustomsEmma;
		emmaMessage.EM_Status = EDIMessage.Status.Queued;
		emmaMessage.EM_LinkedObject = entryHeader;
		emmaMessage.EM_GB = entryHeader.Declaration?.JE_GB ?? GlbBranch.CurrentBranch.PK;
		return emmaMessage;
	}
}
