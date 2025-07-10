using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Shared;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Customs.NO.Manifest.Business.DMOMessageConstants;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(DMOInterchangeDataBuilder))]
sealed class DMOInterchangeDataBuilderTest : TestCaseWithFactory
{
	public void TestBuild_Parameter()
	{
		AssertExceptionThrown<ArgumentNullException>(() => DMOInterchangeDataBuilder.BuildData(null));
	}

	public void TestBuild_MessageHavingMessageText()
	{
		message.EM_MessageText = "TEST_MESSAGE";
		var data = DMOInterchangeDataBuilder.BuildData(message);
		AssertNotNull("Interchange Data", data);
		AssertEquals("MessageText", "TEST_MESSAGE", data.MessageText);
	}

	public void TestBuild_ForParentNotImplementingTransportMode()
	{
		const string missingTransportModeProvider = "Parent object linked to EDIMessage does not implement ITransportModeProvider interface";

		CombineAssertions("When has Transport Mode Provider", () =>
		{
			message.EM_LinkedObject = CreateManifestHeader(TransportTypeList.Codes.Road);
			message.EM_MessageType = NODMOEDIMessageTypeList.Codes.TRA;
			var data = DMOInterchangeDataBuilder.BuildData(message);
			AssertNotNullOrEmpty("InterchangeType", data.InterchangeType);
			AssertNoInterchangeErrorContaining(data, missingTransportModeProvider);
		});

		CombineAssertions("When no Transport Mode Provider", () =>
		{
			message.EM_LinkedObject = Factory.New<DummyBusinessObject>();
			var data = DMOInterchangeDataBuilder.BuildData(message);
			AssertNullOrEmpty("InterchangeType", data.InterchangeType);
			AssertHasInterchangeErrorContaining(data, missingTransportModeProvider);
		});
	}

	public void TestBuild_ForModeAndMessageWithNoInterchangeMapping()
	{
		const string noInterchangeTypeMapping = "Unable to find mapping for Transport Mode: SEA and Message Type (Custom Level): TRA";

		CombineAssertions("When has Interchange Type Mapping", () =>
		{
			message.EM_LinkedObject = CreateManifestHeader(TransportTypeList.Codes.Road);
			message.EM_MessageType = NODMOEDIMessageTypeList.Codes.TRA;
			var data = DMOInterchangeDataBuilder.BuildData(message);
			AssertNotNullOrEmpty("InterchangeType", data.InterchangeType);
			AssertNoInterchangeErrorContaining(data, noInterchangeTypeMapping);
		});

		CombineAssertions("When no Interchange Type Mapping", () =>
		{
			message.EM_LinkedObject = CreateManifestHeader(TransportTypeList.Codes.Sea);
			message.EM_MessageType = NODMOEDIMessageTypeList.Codes.TRA;
			var data = DMOInterchangeDataBuilder.BuildData(message);
			AssertNullOrEmpty("InterchangeType", data.InterchangeType);
			AssertHasInterchangeErrorContaining(data, noInterchangeTypeMapping);
		});
	}

	public void TestBuild_ForValidModeOfTransportAndMessageType()
	{
		message.EM_LinkedObject = CreateManifestHeader(TransportTypeList.Codes.Rail);
		message.EM_MessageType = NODMOEDIMessageTypeList.Codes.HCS;
		var data = DMOInterchangeDataBuilder.BuildData(message);
		AssertNotNull("Interchange Data", data);
		AssertEquals("InterchangeType", NODMOInterchangeTypeList.Codes.RLH, data.InterchangeType);
	}

	public void TestBuild_MovementDeclarationIdDataBuilder_WhenValidMessageType() => CombineAssertions(() =>
	{
		const string declarationId = "1337";
		message.EM_LinkedObject = CreateManifestHeader(TransportTypeList.Codes.Road, declarationId: declarationId);
		message.EM_MessageType = NODMOEDIMessageTypeList.Codes.DUP;
		var data = DMOInterchangeDataBuilder.BuildData(message);
		AssertEquals("InterchangeType is DUP (Document Upload)", NODMOEDIMessageTypeList.Codes.DUP, data.InterchangeType);
		AssertHasInterchangeHeaderAttribute("When EM_MessageType <b>is</b> 'DUP'", data, DMOMessageAttributes.DeclarationID, declarationId);
		AssertNoInterchangeHeaderAttribute("When EM_MessageType <b>is</b> 'DUP'", data, Constants.CustomMsgAttributes.ReferenceNumber);
	});

	public void TestBuild_MovementDeclarationIdDataBuilder_WhenInvalidMessageType() => CombineAssertions(() =>
	{
		const string declarationId = "1337";
		message.EM_LinkedObject = CreateManifestHeader(TransportTypeList.Codes.Road, declarationId: declarationId);
		message.EM_MessageType = NODMOEDIMessageTypeList.Codes.TRA;
		var data = DMOInterchangeDataBuilder.BuildData(message);
		AssertNotEquals("InterchangeType is not DUP (Document Upload)", NODMOEDIMessageTypeList.Codes.DUP, data.InterchangeType);
		AssertNoInterchangeHeaderAttribute("When EM_MessageType <b>is not</b> 'DUP'", data, DMOMessageAttributes.DeclarationID);
		AssertNoInterchangeHeaderAttribute("When EM_MessageType <b>is not</b> 'DUP'", data, Constants.CustomMsgAttributes.ReferenceNumber);
	});

	public void TestBuild_MovementDeclarationIdDataBuilder_WhenMissingDocumentIdProvider()
	{
		const string missingDocumentDeclarationIdProvider = "Parent object linked to EDIMessage does not implement IDocumentUploadSupporter interface";
		const string declarationId = "1337";

		message.EM_LinkedObject = CreateManifestHeader(TransportTypeList.Codes.Road, declarationId: declarationId);
		message.EM_MessageType = NODMOEDIMessageTypeList.Codes.DUP;
		AssertNoInterchangeErrorContaining("When has IMovementRequestIdProvider", DMOInterchangeDataBuilder.BuildData(message), missingDocumentDeclarationIdProvider);

		message.EM_LinkedObject = Factory.New<DummyBusinessObject>();
		AssertHasInterchangeErrorContaining("When missing IMovementRequestIdProvider", DMOInterchangeDataBuilder.BuildData(message), missingDocumentDeclarationIdProvider);
	}

	public void TestBuild_MovementDeclarationIdDataBuilder_WhenMissingDocumentIdValue() => CombineAssertions(() =>
	{
		const string missingDocumentDeclarationId = "Parent object linked to EDIMessage does implement IDocumentUploadSupporter interface but DeclarationId is empty";
		const string declarationId = "1337";

		message.EM_LinkedObject = CreateManifestHeader(TransportTypeList.Codes.Road, declarationId: declarationId);
		message.EM_MessageType = NODMOEDIMessageTypeList.Codes.DUP;
		AssertNoInterchangeErrorContaining("When DeclarationId is not empty", DMOInterchangeDataBuilder.BuildData(message), missingDocumentDeclarationId);

		message.EM_LinkedObject = CreateManifestHeader(TransportTypeList.Codes.Road, declarationId: string.Empty);
		AssertHasInterchangeErrorContaining("When DeclarationId is empty", DMOInterchangeDataBuilder.BuildData(message), missingDocumentDeclarationId);
	});

	public void TestBuild_MovementRequestIdDataBuilder_WhenValidMessageTypeAndSubType() => CombineAssertions(() =>
	{
		const string requestId = "1337";
		message.EM_LinkedObject = CreateManifestHeader(TransportTypeList.Codes.Road, requestId: requestId);

		message.EM_MessageType = NODMOEDIMessageTypeList.Codes.TRA;
		message.EM_MessageSubType = NODMOMessageFunctionList.Codes.VAL;
		var data1 = DMOInterchangeDataBuilder.BuildData(message);
		AssertHasInterchangeHeaderAttribute("When EM_MessageType <b>is</b> 'TRA' and EM_MessageSubType <b>is</b> 'VAL'", data1, DMOMessageAttributes.RequestID, requestId);

		message.EM_MessageType = NODMOEDIMessageTypeList.Codes.MCS;
		var data2 = DMOInterchangeDataBuilder.BuildData(message);
		AssertHasInterchangeHeaderAttribute("When EM_MessageType <b>is</b> 'MCS' and EM_MessageSubType <b>is</b> 'VAL'", data2, DMOMessageAttributes.RequestID, requestId);

		message.EM_MessageType = NODMOEDIMessageTypeList.Codes.HCS;
		var data3 = DMOInterchangeDataBuilder.BuildData(message);
		AssertHasInterchangeHeaderAttribute("When EM_MessageType <b>is</b> 'HCS' and EM_MessageSubType <b>is</b> 'VAL'", data3, DMOMessageAttributes.RequestID, requestId);
	});

	public void TestBuild_MovementRequestIdDataBuilder_WhenInvalidMessageType()
	{
		const string requestId = "1337";
		message.EM_LinkedObject = CreateManifestHeader(TransportTypeList.Codes.Road, requestId: requestId);
		message.EM_MessageType = NODMOEDIMessageTypeList.Codes.DUP;
		message.EM_MessageSubType = NODMOMessageFunctionList.Codes.VAL;
		var data = DMOInterchangeDataBuilder.BuildData(message);
		AssertNoInterchangeHeaderAttribute("When EM_MessageType <b>is not</b> 'TRA' or 'MCS' or 'HCS'", data, DMOMessageAttributes.RequestID);
	}

	public void TestBuild_MovementRequestIdDataBuilder_WhenInvalidMessageSubType()
	{
		const string requestId = "1337";
		message.EM_LinkedObject = CreateManifestHeader(TransportTypeList.Codes.Road, requestId: requestId);
		message.EM_MessageType = NODMOEDIMessageTypeList.Codes.MCS;
		message.EM_MessageSubType = NODMOMessageFunctionList.Codes.UPD;
		var data = DMOInterchangeDataBuilder.BuildData(message);
		AssertNoInterchangeHeaderAttribute("When EM_MessageSubType <b>is not</b> 'VAL'", data, DMOMessageAttributes.RequestID);
	}

	public void TestBuild_MovementRequestIdDataBuilder_WhenMissingRequestIdProvider() => CombineAssertions(() =>
	{
		const string missingMovementRequestIdProvider = "Parent object linked to EDIMessage does not implement IMovementRequestIdProvider interface";
		const string requestId = "1337";

		message.EM_LinkedObject = CreateManifestHeader(TransportTypeList.Codes.Road, requestId: requestId);
		message.EM_MessageType = NODMOEDIMessageTypeList.Codes.TRA;
		message.EM_MessageSubType = NODMOMessageFunctionList.Codes.VAL;
		AssertNoInterchangeErrorContaining("When has IMovementRequestIdProvider", DMOInterchangeDataBuilder.BuildData(message), missingMovementRequestIdProvider);

		message.EM_LinkedObject = Factory.New<DummyBusinessObject>();
		AssertHasInterchangeErrorContaining("When missing IMovementRequestIdProvider", DMOInterchangeDataBuilder.BuildData(message), missingMovementRequestIdProvider);
	});

	public void TestBuild_MovementRequestIdDataBuilder_WhenMissingRequestIdValue() => CombineAssertions(() =>
	{
		const string missingMovementRequestId = "Parent object linked to EDIMessage does implement IMovementRequestIdProvider interface but RequestId is empty";
		const string requestId = "1337";

		message.EM_LinkedObject = CreateManifestHeader(TransportTypeList.Codes.Road, requestId: requestId);
		message.EM_MessageType = NODMOEDIMessageTypeList.Codes.TRA;
		message.EM_MessageSubType = NODMOMessageFunctionList.Codes.VAL;
		AssertNoInterchangeErrorContaining("When RequestId is not empty", DMOInterchangeDataBuilder.BuildData(message), missingMovementRequestId);

		message.EM_LinkedObject = CreateManifestHeader(TransportTypeList.Codes.Road, requestId: string.Empty);
		AssertHasInterchangeErrorContaining("When RequestId is empty", DMOInterchangeDataBuilder.BuildData(message), missingMovementRequestId);
	});

	public void TestBuild_MessageSubType()
	{
		message.EM_MessageSubType = "NEW";
		var data = DMOInterchangeDataBuilder.BuildData(message);
		AssertHasInterchangeHeaderAttribute(data, xTMessaging.Shared.Constants.CustomMsgAttributes.MessageSubType, "NEW");
	}

	public void TestBuild_WithParentAsMRNNumberSupporter()
	{
		var header = CreateManifestHeader(TransportTypeList.Codes.Sea);
		message.EM_LinkedObject = header;
		message.EM_MessageType = NODMOEDIMessageTypeList.Codes.TRA;
		message.EM_MessageSubType = NODMOMessageFunctionList.Codes.NEW;
		var data = DMOInterchangeDataBuilder.BuildData(message);
		AssertNoInterchangeHeaderAttribute("NEW Message", data, Constants.CustomMsgAttributes.ReferenceNumber);

		header.MovementReferenceNumber = "12345";
		message.EM_MessageSubType = NODMOMessageFunctionList.Codes.UPD;
		data = DMOInterchangeDataBuilder.BuildData(message);
		AssertHasInterchangeHeaderAttribute("UDP Message", data, Constants.CustomMsgAttributes.ReferenceNumber, "12345");

		message.EM_MessageSubType = NODMOMessageFunctionList.Codes.DEL;
		data = DMOInterchangeDataBuilder.BuildData(message);
		AssertHasInterchangeHeaderAttribute("DEL Message", data, Constants.CustomMsgAttributes.ReferenceNumber, "12345");
	}

	public void TestBuild_ForUpdateOrDeleteMessageHavingParentAsNonMrnProvider()
	{
		const string missingIMovementReferenceNumberSupporter = "Parent object linked to EDIMessage does not implement IMovementReferenceNumberSupporter interface";
		message.EM_LinkedObject = Factory.New<DummyBusinessObject>();
		message.EM_MessageSubType = NODMOMessageFunctionList.Codes.UPD;
		AssertHasInterchangeErrorContaining(DMOInterchangeDataBuilder.BuildData(message), missingIMovementReferenceNumberSupporter);

		message.EM_LinkedObject = CreateManifestHeader(TransportTypeList.Codes.Air);
		AssertNoInterchangeErrorContaining(DMOInterchangeDataBuilder.BuildData(message), missingIMovementReferenceNumberSupporter);
	}

	protected override void SetUp()
	{
		base.SetUp();
		message = Factory.New<EDIMessage>();
		message.MessageNumberStrategy = Mock.Of<IMessageNumberStrategy>(s => s.GetMessageReferenceNumber() == "111");
	}
	EDIMessage message;

	static void AssertNoInterchangeHeaderAttribute(string assertMessage, DMOInterchangeData interchangeData, string key)
	{
		var assertPrefix = string.IsNullOrEmpty(assertMessage) ? string.Empty : $"{assertMessage}: ";
		var hasValue = interchangeData.MessageAttributes.TryGetValue(key, out _);
		AssertEquals($"{assertPrefix}Interchange header attribute {key} exists", expected: false, hasValue);
	}

	static void AssertHasInterchangeHeaderAttribute(DMOInterchangeData interchangeData, string key, string value) =>
		AssertHasInterchangeHeaderAttribute(string.Empty, interchangeData, key, value);
	static void AssertHasInterchangeHeaderAttribute(string assertMessage, DMOInterchangeData interchangeData, string key, string value)
	{
		var assertPrefix = string.IsNullOrEmpty(assertMessage) ? string.Empty : $"{assertMessage}: ";
		var hasValue = interchangeData.MessageAttributes.TryGetValue(key, out var actualValue);
		AssertEquals($"{assertPrefix}Interchange header attribute {key} exists", expected: true, hasValue);
		AssertEquals($"{assertPrefix}Interchange header attribute {key} value", value, actualValue);
	}

	static void AssertNoInterchangeErrorContaining(DMOInterchangeData interchangeData, string message) =>
		AssertNoInterchangeErrorContaining(string.Empty, interchangeData, message);
	static void AssertNoInterchangeErrorContaining(string assertMessage, DMOInterchangeData interchangeData, string message)
	{
		AssertNotContains(assertMessage, message, interchangeData.ErrorBuilder.ToStringWithNewLineBetweenAppends());
	}

	static void AssertHasInterchangeErrorContaining(DMOInterchangeData interchangeData, string message) =>
		AssertHasInterchangeErrorContaining(string.Empty, interchangeData, message);
	static void AssertHasInterchangeErrorContaining(string assertMessage, DMOInterchangeData interchangeData, string message)
	{
		AssertContains(assertMessage, message, interchangeData.ErrorBuilder.ToStringWithNewLineBetweenAppends());
	}

	AsycudaManifestHeader CreateManifestHeader(string modeOfTransport, string declarationId = null, string requestId = null)
	{
		var headerMock = Factory.NewMoq<AsycudaManifestHeader>();
		var header = headerMock.Object;
		header.AMA_TransportMode = modeOfTransport;
		headerMock.Protected().Setup<ZString>("GetDeclarationId").Returns(declarationId);
		headerMock.Protected().Setup<ZString>("GetRequestId").Returns(requestId);
		return header;
	}
}
