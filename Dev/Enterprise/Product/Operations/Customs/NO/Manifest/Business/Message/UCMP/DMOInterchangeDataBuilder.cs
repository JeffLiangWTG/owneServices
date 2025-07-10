using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Shared;
using static Enterprise.Customs.NO.Manifest.Business.DMOMessageConstants;

namespace Enterprise.Customs.NO.Manifest.Business;

static class DMOInterchangeDataBuilder
{
	public static DMOInterchangeData BuildData(EDIMessage message)
	{
		Argument.NotNull(message, nameof(message));

		var data = new DMOInterchangeData();
		return GetInterchangeDataBuilders().Aggregate(data, (interchangeData, dataBuilder) => dataBuilder.Build(message, interchangeData));
	}

	static IEnumerable<IInterchangeDataBuilder> GetInterchangeDataBuilders()
	{
		yield return new MessageTextDataBuilder();
		yield return new InterchangeTypeDataBuilder();
		yield return new MessageSubTypeDataBuilder();
		yield return new MovementDeclarationIdDataBuilder();
		yield return new MovementReferenceNumberDataBuilder();
		yield return new MovementRequestIdDataBuilder();
	}

	interface IInterchangeDataBuilder
	{
		DMOInterchangeData Build(EDIMessage message, DMOInterchangeData data);
	}

	sealed class MessageTextDataBuilder : IInterchangeDataBuilder
	{
		public DMOInterchangeData Build(EDIMessage message, DMOInterchangeData data)
		{
			var messageText = message.EM_MessageText;
			data.MessageText = messageText;
			return data;
		}
	}

	sealed class InterchangeTypeDataBuilder : IInterchangeDataBuilder
	{
		public DMOInterchangeData Build(EDIMessage message, DMOInterchangeData data)
		{
			var messageType = message.EM_MessageType;
			if (messageType == NODMOEDIMessageTypeList.Codes.DUP)
			{
				data.InterchangeType = NODMOEDIMessageTypeList.Codes.DUP;
				return data;
			}
			if (message.EM_LinkedObject is not ITransportModeProvider transportModeProvider)
			{
				data.ErrorBuilder.Append($"Parent object linked to EDIMessage does not implement {nameof(ITransportModeProvider)} interface");
				return data;
			}
			var transportMode = transportModeProvider.TransportMode;
			if (!EDIInterchangeTypeMapper.TryGetInterchangeType(messageType, transportMode, out var interchangeType))
			{
				data.ErrorBuilder.Append($"Unable to find mapping for Transport Mode: {transportMode} and Message Type (Custom Level): {messageType}");
				return data;
			}
			data.InterchangeType = interchangeType;
			return data;
		}
	}

	sealed class MessageSubTypeDataBuilder : IInterchangeDataBuilder
	{
		public DMOInterchangeData Build(EDIMessage message, DMOInterchangeData data)
		{
			data.MessageAttributes.Add(xTMessaging.Shared.Constants.CustomMsgAttributes.MessageSubType, message.EM_MessageSubType);
			return data;
		}
	}

	sealed class MovementDeclarationIdDataBuilder : IInterchangeDataBuilder
	{
		public DMOInterchangeData Build(EDIMessage message, DMOInterchangeData data)
		{
			if (message.EM_MessageType != NODMOEDIMessageTypeList.Codes.DUP)
			{
				return data;
			}
			if (message.EM_LinkedObject is not IDocumentUploadSupporter documentUploadSupporter)
			{
				data.ErrorBuilder.Append($"Parent object linked to EDIMessage does not implement {nameof(IDocumentUploadSupporter)} interface");
				return data;
			}
			var declarationId = documentUploadSupporter.DeclarationId;
			if (declarationId.IsEmpty)
			{
				data.ErrorBuilder.Append($"Parent object linked to EDIMessage does implement {nameof(IDocumentUploadSupporter)} interface but DeclarationId is empty");
				return data;
			}
			data.MessageAttributes.Add(DMOMessageAttributes.DeclarationID, declarationId);
			return data;
		}
	}

	sealed class MovementReferenceNumberDataBuilder : IInterchangeDataBuilder
	{
		public DMOInterchangeData Build(EDIMessage message, DMOInterchangeData data)
		{
			var messageSubType = message.EM_MessageSubType;
			if (messageSubType != NODMOMessageFunctionList.Codes.UPD && messageSubType != NODMOMessageFunctionList.Codes.DEL)
			{
				return data;
			}

			if (message.EM_LinkedObject is not IMovementReferenceNumberSupporter movementReferenceNumberSupporter)
			{
				data.ErrorBuilder.Append($"Parent object linked to EDIMessage does not implement {nameof(IMovementReferenceNumberSupporter)} interface");
			}
			else
			{
				data.MessageAttributes.Add(Constants.CustomMsgAttributes.ReferenceNumber, movementReferenceNumberSupporter.MovementReferenceNumber);
			}

			return data;
		}
	}

	sealed class MovementRequestIdDataBuilder : IInterchangeDataBuilder
	{
		public DMOInterchangeData Build(EDIMessage message, DMOInterchangeData data)
		{
			if (!IsValidMessageTypeAndSubType(message.EM_MessageType, message.EM_MessageSubType))
			{
				return data;
			}
			if (message.EM_LinkedObject is not IMovementRequestIdProvider movementRequestIdProvider)
			{
				data.ErrorBuilder.Append($"Parent object linked to EDIMessage does not implement {nameof(IMovementRequestIdProvider)} interface");
				return data;
			}
			var requestId = movementRequestIdProvider.RequestId;
			if (requestId.IsEmpty)
			{
				data.ErrorBuilder.Append($"Parent object linked to EDIMessage does implement {nameof(IMovementRequestIdProvider)} interface but RequestId is empty");
				return data;
			}
			data.MessageAttributes.Add(DMOMessageAttributes.RequestID, requestId);
			return data;
		}

		static bool IsValidMessageTypeAndSubType(string messageType, string messageSubType) => (messageType, messageSubType) switch
		{
			(NODMOEDIMessageTypeList.Codes.TRA, NODMOMessageFunctionList.Codes.VAL) => true,
			(NODMOEDIMessageTypeList.Codes.HCS, NODMOMessageFunctionList.Codes.VAL) => true,
			(NODMOEDIMessageTypeList.Codes.MCS, NODMOMessageFunctionList.Codes.VAL) => true,
			_ => false,
		};
	}
}
