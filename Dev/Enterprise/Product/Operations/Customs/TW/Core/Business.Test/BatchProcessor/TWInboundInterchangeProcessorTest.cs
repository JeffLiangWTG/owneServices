using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TW.Business.BatchProcessor;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGenerateMessageFromInterchange()
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
			foreach (var expectedMessageType in expectedMessageTypes)
			{
				var name = expectedMessageType.Key;
				var messageText = TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile." + name + ".xml");
				var interchange = Factory.NewWithValidTestData<EDIInterchange>();
				interchange.EI_From = "TWCustoms.TEST";
				interchange.EI_To = "TEST";
				interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.TaiwanCustoms;
				interchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.TaiwanCustoms;
				interchange.EI_InterchangeNum = name + ".ABCD";
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
				interchange.EI_Status = EDIInterchange.Status.Queued;
				interchange.EI_IsActive = true;
				interchange.ForceDeprecatedNTextUsageForTesting = true;
				interchange.EI_BodyNText = messageText;
				Factory.Save();
				CombineAssertions(() =>
				{
					var log = GetNewLoggerForTesting();
					new TWCInboundInterchangeProcessor(log).ExecuteBatch();
					interchange.Reload();
					NUnit.Framework.Assert.That(interchange.EI_Status, NUnit.Framework.Is.EqualTo(EDIInterchange.Status.Received).Using(CustomComparers.TypeComparison), "Interchange status should be set to Received");
					NUnit.Framework.Assert.That(interchange.ContainedMessages.Count, NUnit.Framework.Is.EqualTo(1), "Should have been 1 message extracted from interchange");
					var createdMessage = interchange.ContainedMessages[0];
					NUnit.Framework.Assert.That(createdMessage.EM_EI, NUnit.Framework.Is.EqualTo(interchange.PK), "message linked to interchange");
					NUnit.Framework.Assert.That(createdMessage.EM_ApplicationCode, NUnit.Framework.Is.EqualTo(EDIInterchange.ApplicationCodes.TaiwanCustoms).Using(CustomComparers.TypeComparison), "EM_ApplicationCode");
					NUnit.Framework.Assert.That(createdMessage.EM_MessageType, NUnit.Framework.Is.EqualTo(expectedMessageType.Value).Using(CustomComparers.TypeComparison), "EM_MessageType");
					NUnit.Framework.Assert.That(createdMessage.EM_IsTestMessage, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "EM_IsTestMessage");
					NUnit.Framework.Assert.That(createdMessage.EM_MessageText, NUnit.Framework.Is.EqualTo(messageText), "EM_MessageText");
					NUnit.Framework.Assert.That(createdMessage.EM_ReceiveTransmit, NUnit.Framework.Is.EqualTo(EDIInterchange.Direction.Receive).Using(CustomComparers.TypeComparison), "EM_ReceiveTransmit");
					NUnit.Framework.Assert.That(createdMessage.EM_Status, NUnit.Framework.Is.EqualTo(EDIInterchange.Status.Queued).Using(CustomComparers.TypeComparison), "EM_Status");
					NUnit.Framework.Assert.That(createdMessage.EM_LinkTable, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "EM_LinkTable - this is set by message processor");
					NUnit.Framework.Assert.That(createdMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(ZGuid.Empty), "EM_LinkUniqueID - this is set by message processor");
				}

				);
			}
		}

		#region Implementation
		internal static LoggingInformation GetNewLoggerForTesting()
		{
			return new LoggingInformationForTesting();
		}
		#endregion
	}
}
