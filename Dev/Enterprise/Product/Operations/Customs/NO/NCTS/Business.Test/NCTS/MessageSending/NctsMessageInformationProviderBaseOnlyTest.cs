using System;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NO.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NO.NCTS.Business.Testing;

[TestedType(typeof(NctsMessageInformationProvider))]
sealed class NctsMessageInformationProviderBaseOnlyTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NctsMessageInformationProviderForTest(null, false));
	}

	public void TestMessageType()
	{
		var messageSendingObject = CreateMessageSendingObject();
		var provider = CreateMessageInformationProvider(messageSendingObject);

		CombineAssertions(() =>
		{
			AssertEquals("Default Value when MessageType is not set explicitly", "007", provider.MessageType);

			messageSendingObject.MessageType = NctsArrivalMessageTypeCodeList.Codes.UnloadingRemarks;
			AssertEquals("When MessageType = 044", "044", provider.MessageType);
		});
	}

	public void TestMessageSubType()
	{
		var messageSendingObject = CreateMessageSendingObject();
		var provider = CreateMessageInformationProvider(messageSendingObject);
		AssertEquals("MessageSubType", ZString.Empty, provider.MessageSubType);
	}

	public void TestParent()
	{
		var messageSendingObject = CreateMessageSendingObject();
		var provider = CreateMessageInformationProvider(messageSendingObject);
		AssertSame("Parent", messageSendingObject.NctsHeader, provider.Parent);
	}

	public void TestApplicationReference()
	{
		var messageSendingObject = CreateMessageSendingObject();
		var provider = CreateMessageInformationProvider(messageSendingObject);
		AssertEquals("ApplicationReference", ZString.Empty, provider.ApplicationReference);
	}

	public void TestApplicationCode()
	{
		var messageSendingObject = CreateMessageSendingObject();
		var provider = CreateMessageInformationProvider(messageSendingObject);
		AssertEquals("ApplicationCode", Enterprise.Messaging.Integration.ApplicationCodeList.Codes.NOCustomsNcts, provider.ApplicationCode);
	}

	public void TestMessageNumberStrategy()
	{
		var messageSendingObject = CreateMessageSendingObject();
		var provider = CreateMessageInformationProvider(messageSendingObject);
		var providerMessageNumberStrategy = provider.MessageNumberStrategy;

		CombineAssertions(() =>
		{
			AssertType<FixedMessageNumberStrategy>(providerMessageNumberStrategy);
			AssertSame(providerMessageNumberStrategy, provider.MessageNumberStrategy);
		});
	}

	public void TestMessageText()
	{
		var messageSendingObject = CreateMessageSendingObject();
		var provider = CreateMessageInformationProvider(messageSendingObject);
		AssertEquals("MessageText", "DUMMY MESSAGE", provider.MessageText);
	}

	public void TestMessageText_WhenNoMessageBuilder()
	{
		var messageSendingObject = CreateMessageSendingObject();
		IMessageInformationProvider provider = new NctsMessageInformationProviderForTest(messageSendingObject, false);
		AssertEquals("MessageText", ZString.Empty, provider.MessageText);
	}

	IMessageInformationProvider CreateMessageInformationProvider(NctsHeaderMessageSendingObject messageSendingObject)
		=> new NctsMessageInformationProviderForTest(messageSendingObject, true);

	NctsHeaderMessageSendingObject CreateMessageSendingObject()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		return new NctsHeaderMessageSendingObject(nctsHeader);
	}

	class NctsMessageInformationProviderForTest : NctsMessageInformationProvider
	{
		public NctsMessageInformationProviderForTest(NctsHeaderMessageSendingObject messageSendingObject, bool returnMessageBuilder)
			: base(messageSendingObject)
		{
			this.returnMessageBuilder = returnMessageBuilder;
		}
		readonly bool returnMessageBuilder;

		protected override IXmlMessageBuilder CreateMessageBuilder()
		{
			if (!returnMessageBuilder)
			{
				return null;
			}

			var mockXmlMessage = Mock.Of<IXmlMessage>(m => m.GetSerializedString() == "DUMMY MESSAGE");
			return Mock.Of<IXmlMessageBuilder>(b => b.GenerateXmlMessage() == mockXmlMessage);
		}

		protected override BusinessObject GetParentCore() => Header;
	}
}
