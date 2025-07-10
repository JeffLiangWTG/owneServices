using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.NO.Business;

sealed class NOCustomsPurgeSettingsConfig : PurgeSettingsConfig
{
	public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
	{
		yield return AddApplicationCodeMessageSubTypePurgeType(ApplicationCodeList.Codes.NOCustoms, [NewInterchangeConfigObj(5, TimeUnit.Year)], new MessageSubTypePurgeTypeObjCollection()
			.Add(MessageSendingMessageTypes.Codes.CompleteOrdinaryDeclaration, MessageSendingMessageTypes.Descriptions.CompleteOrdinaryDeclaration, 5, TimeUnit.Year)
			.Add(MessageSendingMessageTypes.Codes.ManualDeclaration, MessageSendingMessageTypes.Descriptions.ManualDeclaration, 5, TimeUnit.Year)
			.Add(MessageSendingMessageTypes.Codes.Correction, MessageSendingMessageTypes.Descriptions.Correction, 5, TimeUnit.Year)
			.Add(MessageSendingMessageTypes.Codes.PreliminaryDeclaration, MessageSendingMessageTypes.Descriptions.PreliminaryDeclaration, 5, TimeUnit.Year)
			.Add(MessageSendingMessageTypes.Codes.FinalDeclaration, MessageSendingMessageTypes.Descriptions.FinalDeclaration, 5, TimeUnit.Year)
			.Add(MessageSendingMessageTypes.Codes.PostDeclaration, MessageSendingMessageTypes.Descriptions.PostDeclaration, 5, TimeUnit.Year)
			.Add(MessageSendingMessageTypes.Codes.RefundDeclaration, MessageSendingMessageTypes.Descriptions.RefundDeclaration, 5, TimeUnit.Year)
			.Add(MessageSendingMessageTypes.Codes.StatisticalRecalculatedDeclaration, MessageSendingMessageTypes.Descriptions.StatisticalRecalculatedDeclaration, 5, TimeUnit.Year)
			.Add(MessageSendingMessageTypes.Codes.CollectiveCustomClearance, MessageSendingMessageTypes.Descriptions.CollectiveCustomClearance, 5, TimeUnit.Year)
			.Add(xTMessageConstants.MessageTypes.Codes.Acknowledgement, xTMessageConstants.MessageTypes.Descriptions.Acknowledgement, 5, TimeUnit.Year)
			.Add(xTMessageConstants.MessageTypes.Codes.Error, xTMessageConstants.MessageTypes.Descriptions.Error, 5, TimeUnit.Year)
		);
		yield return AddApplicationCodeMessageTypePurgeType(EDIMessageConstants.MessageTypes.CUSRES,
			[NewInterchangeConfigObj(5, TimeUnit.Year)],
			new MessageTypePurgeTypeObjCollection()
				.Add(EDIMessageConstants.MessageTypes.CUSRES, "RES", 5, TimeUnit.Year)
		);
		yield return AddApplicationCodeMessageTypePurgeType(ApplicationCodeList.Codes.NOCustomsEmma,
			[NewInterchangeConfigObj(5, TimeUnit.Year)],
			new MessageTypePurgeTypeObjCollection()
				.Add(EDIMessageConstants.MessageTypes.EMMA, "EMMA", 5, TimeUnit.Year)
				.Add(xTMessageConstants.MessageTypes.Codes.Acknowledgement, xTMessageConstants.MessageTypes.Descriptions.Acknowledgement, 5, TimeUnit.Year)
				.Add(xTMessageConstants.MessageTypes.Codes.Error, xTMessageConstants.MessageTypes.Descriptions.Error, 5, TimeUnit.Year)
		);
	}
}
