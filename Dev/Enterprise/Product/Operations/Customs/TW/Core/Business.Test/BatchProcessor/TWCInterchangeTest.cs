using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWCInterchange))]
	sealed class TWCInterchangeTest : EDIInterchangeTest
	{
		[ExpectNoExceptions]
		public void TestProperties()
		{
			var interchange = Factory.New<TWCInterchange>();
			NUnit.Framework.Assert.That(interchange.EI_ApplicationCode, NUnit.Framework.Is.EqualTo(EDIInterchange.ApplicationCodes.TaiwanCustoms).Using(CustomComparers.TypeComparison), "EI_ApplicationCode");
		}

		[ExpectNoExceptions]
		public void TestNewAndLoadType()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = "TWCustoms.TEST";
			interchange.EI_To = "TEST";
			interchange.EI_ApplicationCode = "TWC";
			interchange.EI_InterchangeType = "TWC";
			interchange.EI_InterchangeNum = "XXXX.ABCD";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = "AAA";
			NUnit.Framework.Assert.That(interchange.GetType(), NUnit.Framework.Is.EqualTo(typeof(EDIInterchange)), "EDIInterchange");
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			interchange = newFactory.Load<EDIInterchange>(interchange.PK);
			NUnit.Framework.Assert.That(interchange.GetType(), NUnit.Framework.Is.EqualTo(typeof(TWCInterchange)), "TWCInterchange");
		}

		[ExpectNoExceptions]
		public void TestCreateMessagesFromInterchageXml()
		{
			CombineAssertions(() =>
			{
				AssertCreateMessagesFromInterchageXml(true);
				AssertCreateMessagesFromInterchageXml(false);
				AssertCreateMessagesFromInterchageXml("");
				AssertCreateMessagesFromInterchageXml("XXXX");
				AssertCreateMessagesFromInterchageXmlWhenCustomsDeliveryNotification(false);
				AssertCreateMessagesFromInterchageXmlWhenCustomsDeliveryNotification(true);
				AssertCreateMessagesFromInterchageXmlWhenControllingAgencyDeliveryNotification();
			});
		}

		[TestDate(2019, 7, 23)]
		[ExpectNoExceptions]
		public void TestGetInterchangeNumber()
		{
			var message = GetAMessage();
			var interchange = GetAnInterchange(message);
			Factory.Save();
			NUnit.Framework.Assert.That(interchange.EI_InterchangeNum, NUnit.Framework.Is.EqualTo("52889316000807230001").Using(CustomComparers.TypeComparison));
			var message1 = GetAMessage();
			message1.EM_MessageOwner = ZString.Empty;
			var interchange1 = GetAnInterchange(message1);
			Factory.Save();
			NUnit.Framework.Assert.That(interchange1.EI_InterchangeNum, NUnit.Framework.Is.EqualTo("00000000000807230001").Using(CustomComparers.TypeComparison));
		}

		TWCInterchange GetAnInterchange(TWMessage message)
		{
			var interchange = Factory.New<TWCInterchange>();
			interchange.ContainedMessages.Add(message);
			interchange.EI_From = "TWCustoms.TEST";
			interchange.EI_To = "TEST";
			return interchange;
		}

		TWMessage GetAMessage()
		{
			var message = Factory.New<TWMessage>();
			message.EM_MessageType = "EXP";
			message.EM_MessageOwner = "52889316";
			message.EM_MessageText = "test";
			return message;
		}

		[ExpectNoExceptions]
		void AssertCreateMessagesFromInterchageXmlWhenCustomsDeliveryNotification(bool hasBOM)
		{
			var factory = new BusinessObjectFactory();
			var logger = new LoggingInformationForTesting();
			foreach (var interchangeType in new string[]
			{
				MessageTypeList.Codes.ECD,
				MessageTypeList.Codes.ICD,
				MessageTypeList.Codes.ADM,
				MessageTypeList.Codes.IEA,
				MessageTypeList.Codes.FCF,
				MessageTypeList.Codes.FHM,
				MessageTypeList.Codes.TRA
			})
			{
				var messageText = TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.TWCustomsDeliveryNotification.xml");
				var interchange = factory.NewWithValidTestData<TWCInterchange>();
				interchange.EI_From = "TWCustoms1";
				interchange.EI_To = "TEST1";
				interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.TaiwanCustoms;
				interchange.EI_InterchangeType = interchangeType;
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
				interchange.EI_Status = EDIInterchange.Status.Queued;
				interchange.EI_IsActive = true;
				interchange.ForceDeprecatedNTextUsageForTesting = true;
				interchange.EI_BodyNText = messageText;
				var interchangeNum = interchangeType + "AA";
				if (hasBOM)
				{
					interchange.EI_BodyNText = (char)65279 + messageText;
					interchangeNum += "1";
				}

				interchange.EI_InterchangeNum = interchangeNum;
				interchange.CreateMessagesFromInterchageXml(logger);
				factory.Save();
				var newEDIMessage = interchange.ContainedMessages[0];
				NUnit.Framework.Assert.That(newEDIMessage.EM_MessageText, NUnit.Framework.Is.EqualTo(messageText), $"EM_MessageText when {interchangeType}");
				NUnit.Framework.Assert.That(newEDIMessage.EM_MessageType, NUnit.Framework.Is.EqualTo(interchangeType).Using(CustomComparers.TypeComparison), $"EM_MessageType when {interchangeType}");
				NUnit.Framework.Assert.That(newEDIMessage.EM_MessageNum, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Types.ZString)), "$EM_MessageNum when {interchangeType} - should not be [null]");
			}
		}

		[ExpectNoExceptions]
		void AssertCreateMessagesFromInterchageXmlWhenControllingAgencyDeliveryNotification()
		{
			var factory = new BusinessObjectFactory();
			var logger = new LoggingInformationForTesting();
			foreach (var interchangeType in new string[]
			{
				MessageTypeList.Codes._101,
				MessageTypeList.Codes._201,
				MessageTypeList.Codes._207,
				MessageTypeList.Codes._301,
				MessageTypeList.Codes._31A,
				MessageTypeList.Codes._31D,
				MessageTypeList.Codes._401,
				MessageTypeList.Codes._601,
				MessageTypeList.Codes._603
			})
			{
				var messageText = TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.ControllingAgencyDeliveryNotification.xml");
				var interchange = factory.NewWithValidTestData<TWCInterchange>();
				interchange.EI_From = "TWCustoms1";
				interchange.EI_To = "TEST1";
				interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.TaiwanCustoms;
				interchange.EI_InterchangeType = interchangeType;
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
				interchange.EI_Status = EDIInterchange.Status.Queued;
				interchange.EI_IsActive = true;
				interchange.ForceDeprecatedNTextUsageForTesting = true;
				interchange.EI_BodyNText = messageText;
				var interchangeNum = interchangeType + "AA";
				interchange.EI_InterchangeNum = interchangeNum;
				interchange.CreateMessagesFromInterchageXml(logger);
				factory.Save();
				var newEDIMessage = interchange.ContainedMessages[0];
				NUnit.Framework.Assert.That(newEDIMessage.EM_MessageText, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Types.ZString)), "$EM_MessageText when {interchangeType} - should not be [null]");
				NUnit.Framework.Assert.That(interchangeType, NUnit.Framework.Is.EqualTo(newEDIMessage.EM_MessageType).Using(CustomComparers.TypeComparison), $"EM_MessageType when {interchangeType}");
				NUnit.Framework.Assert.That(newEDIMessage.EM_MessageNum, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Types.ZString)), "$EM_MessageNum when {interchangeType} - should not be [null]");
			}
		}

		void AssertCreateMessagesFromInterchageXml(bool fileNameFlag)
		{
			var expectedMessageTypes = new Dictionary<string, string>();
			expectedMessageTypes["NX5106"] = "ARM";
			expectedMessageTypes["N5204"] = "ERM";
			expectedMessageTypes["N5109"] = "IEM";
			expectedMessageTypes["N5116"] = "IRM";
			expectedMessageTypes["N5107"] = "RFM";
			expectedMessageTypes["N5168"] = "UHC";
			expectedMessageTypes["N5110"] = "TPC";
			expectedMessageTypes["N5111"] = "TAD";
			var logger = new LoggingInformationForTesting();
			foreach (var expectedMessageType in expectedMessageTypes)
			{
				var messageText = TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile." + expectedMessageType.Key + ".xml");
				var interchange = Factory.NewWithValidTestData<TWCInterchange>();
				interchange.EI_From = "TWCustoms";
				interchange.EI_To = "TEST";
				interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.TaiwanCustoms;
				interchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.TaiwanCustoms;
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
				interchange.EI_Status = EDIInterchange.Status.Queued;
				interchange.EI_IsActive = true;
				interchange.ForceDeprecatedNTextUsageForTesting = true;
				interchange.EI_BodyNText = messageText;
				if (fileNameFlag)
				{
					interchange.EI_InterchangeNum = expectedMessageType.Key + ".ABC123456";
				}

				interchange.CreateMessagesFromInterchageXml(logger);
				Factory.Save();
				var newEDIMessage = interchange.ContainedMessages[0];
				var assertMessage = " when " + expectedMessageType.Key + " and EI_InterchangeNum is " + (fileNameFlag ? interchange.EI_InterchangeNum.ToString() : "");
				NUnit.Framework.Assert.That(newEDIMessage.EM_MessageText, NUnit.Framework.Is.EqualTo(messageText), "EM_MessageText" + assertMessage);
				NUnit.Framework.Assert.That(newEDIMessage.EM_MessageType, NUnit.Framework.Is.EqualTo(expectedMessageType.Value).Using(CustomComparers.TypeComparison), "EM_MessageType" + assertMessage);
				AssertNotNullOrEmpty("EM_MessageNum is not null", newEDIMessage.EM_MessageNum);
			}
		}

		[ExpectNoExceptions]
		void AssertCreateMessagesFromInterchageXml(string xml)
		{
			var expectedMessageTypes = new Dictionary<string, string>();
			expectedMessageTypes["NX5106"] = "ARM";
			expectedMessageTypes["N5204"] = "ERM";
			expectedMessageTypes["N5109"] = "IEM";
			expectedMessageTypes["N5116"] = "IRM";
			expectedMessageTypes["N5107"] = "RFM";
			expectedMessageTypes["N5168"] = "UHC";
			expectedMessageTypes["N5110"] = "TPC";
			expectedMessageTypes["N5111"] = "TAD";
			var logger = new LoggingInformationForTesting();
			foreach (var expectedMessageType in expectedMessageTypes)
			{
				var interchange = Factory.NewWithValidTestData<TWCInterchange>();
				interchange.EI_From = "TWCustoms";
				interchange.EI_To = "TEST";
				interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.TaiwanCustoms;
				interchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.TaiwanCustoms;
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
				interchange.EI_Status = EDIInterchange.Status.Queued;
				interchange.EI_IsActive = true;
				interchange.ForceDeprecatedNTextUsageForTesting = true;
				interchange.EI_BodyNText = xml;
				interchange.EI_InterchangeNum = expectedMessageType.Key + ".ABC123456";
				interchange.CreateMessagesFromInterchageXml(logger);
				NUnit.Framework.Assert.That(interchange.ContainedMessages.Count, NUnit.Framework.Is.EqualTo(0), "the Message create failed");
			}
		}

		public new void TestCreateNewIfcsumInterchange()
		{
			Assert("CreateNewIfcsumInterchange", true);
		}

		public new void TestCreateNewIftstaInterchange()
		{
			Assert("CreateNewIftstaInterchange", true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<TWCInterchange>();
		}
	}
}
