using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirlineMessaging;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.AirlineMessaging;

internal sealed class AirlineMessagingLoggerTest : TestCaseWithFactory
{
	public void TestLogResponse_LoggedMessageContentIsFromMessageResponse()
	{
		var consol = Factory.NewWithValidTestData<ForwardingConsol>();
		var logger = new AirlineMessagingLogger();
		var testEvents = GetTestDataById();
		var expectedMessages = GetMessagesById(testEvents);
		logger.LogResponse(consol, expectedMessages, testEvents);
		consol.Factory.Save();

		var messagesQuery = new ZDBOnlyQuery(typeof(EDIMessage));
		var messages = Factory.Load<EDIMessage>(messagesQuery);

		foreach (var kvp in testEvents)
		{
			var testEvent = kvp.Value;
			var expectedMessage = expectedMessages[kvp.Key];
			var messageNum = testEvent.ContextCollection.First(cc => cc.Type == "MAWBNumber").Value;
			Assert("Logger should have created an EDI message with original API request in message text",
				messages.Any(m => m.EM_MessageText == expectedMessage && m.EM_MessageNum == messageNum.Value));
		}
	}

	public void TestLogResponse_MessagesAreAddedToInterchange()
	{
		var consol = Factory.NewWithValidTestData<ForwardingConsol>();
		var startingMessageCount = consol.CIMEDIMessages.Count;
		var logger = new AirlineMessagingLogger();
		var testEvents = GetTestDataById();
		var expectedMessages = GetMessagesById(testEvents);
		logger.LogResponse(consol, expectedMessages, testEvents);
		consol.Factory.Save();

		var messagesQuery = new ZDBOnlyQuery(typeof(EDIMessage));
		var messages = Factory.Load<EDIMessage>(messagesQuery);

		AssertEquals(startingMessageCount, consol.CIMEDIMessages.Count);
		foreach (var kvp in testEvents)
		{
			var testEvent = kvp.Value;
			var messageNum = testEvent.ContextCollection.First(cc => cc.Type == "MAWBNumber").Value;
			var expectedMessage = expectedMessages[kvp.Key];
			var actualMessage = messages.FirstOrDefault(m => m.EM_MessageText == expectedMessage && m.EM_MessageNum == messageNum.Value);
			AssertNotNull(actualMessage);
			AssertEquals(messageNum, actualMessage.EM_MessageNum);
			AssertEquals(expectedMessage, actualMessage.EM_MessageText);
			AssertNotNull(actualMessage.EM_EI);
		}
	}

	#region TestData

	IDictionary<int, string> GetMessagesById(IDictionary<int, UniversalEvent> eventsById)
	{
		var messages = new Dictionary<int, string>();
		foreach (var kvp in eventsById)
		{
			var message = kvp.Value.ContextCollection.First(cc => cc.Type == "OriginalFWBFHLMessage").Value;
			var messageId = kvp.Key;
			messages.Add(messageId, message);
		}

		return messages;
	}

	IDictionary<int, UniversalEvent> GetTestDataById()
	{
		return TestDataValidEvents.ToDictionary(x => x.GetHashCode());
	}

	UniversalEvent[] TestDataValidEvents => new[]
	{
		new UniversalEvent()
		{
			EventType = AutoEvents.InterchangeSentCode,
			EventParameters = new EventParameters() { Reason = "Message queued for processing" },
			ContextCollection =
			[
				new Context() { Type = "MAWBNumber", Value = "618-98234850" },
				new Context()
				{
					Type = "OriginalFWBFHLMessage",
					Value = "FWB/17\n618-98234850ZRHHKG/T4K1502\nFLT/LX040/18/XX044/19\nRTG/LAXLX\nSHP\nNAM/ANTWERP CFS\nADR/200 GEORGE STREET\nLOC/SYDNEY/NSW\n/AU/2000/TE/3212345678\nCNE\nNAM/ANTWERP CFS\nADR/200 GEORGE STREET\nLOC/SYDNEY/NSW\n/AU/2000/TE/3212345678\nAGT/1931484/9147203/0005\n/TEST COMPANY\n/HEATHROW\nNFY\nNAM/ANTWERP CFS\nADR/200 GEORGE STREET\nLOC/SYDNEY/NSW\n/AU/2000/TE/3212345678\nCVD/AUD/PP/CC/NVD/NCV/XXX\nRTD/1/P2/K751/CQ/W751/R2.5/T1877.5\n/NC/CONSOLIDATION AS PER\n/2/NC/ATTACHED LIST\n/3/P2/K751/CQ/W751/R2.5/T1877.5\n/NG/OTHER ASSORTED PARTS\n/4/ND//CMT50-50-50/1\n/5/NV/MC0.2\n/6/NS/615\n/7/CX\n/NU/PMC12541ET\nPPD/VC55.5/TX66\n/CT121.5\nCOL/WT3755/VC33/TX77\n/CT3865\nISU/14MAR23/BRISBANE CITY\nREF//C00705167/FFW/CWIDWUTDCHF5F/BSL\nSPH/GEN/HLC\nOCI/AU/SHP/CT/3212345678\n/AU/CNE/CT/3212345678\n/AU/NFY/CT/3212345678"
				}
			]
		},
		new UniversalEvent()
		{
			EventType = AutoEvents.InterchangeSentCode,
			EventParameters = new EventParameters() { Reason = "Message queued for processing" },
			ContextCollection =
			[
				new Context() { Type = "MAWBNumber", Value = "020-12345001" },
				new Context()
				{
					Type = "OriginalFWBFHLMessage",
					Value = "FHL/5\nMBI/618-98234850ZRHHKG/T4K1502\nHBS/P00003337/ZRHHKG/4/K1502/2/MOBILE PHONES\n/GEN/HLC\nTXT/OTHER ASSORTED PARTS\nHTS/920510\n/920511\nOCI/AU/SHP/CT/3212345678\n/AU/CNE/CT/3212345678\n/AU/NFY/CT/3212345678\nSHP\nNAM/ANTWERP CFS\nADR/200 GEORGE STREET\nLOC/SYDNEY/NSW\n/AU/2000/TE/3212345678\nCNE\nNAM/ANTWERP CFS\nADR/200 GEORGE STREET\nLOC/SYDNEY/NSW\n/AU/2000/TE/3212345678\nCVD/AUD/CC/NVD/NCV/XXX"
				}
			]
		},
		new UniversalEvent()
		{
			EventType = AutoEvents.InterchangeSentCode,
			EventParameters = new EventParameters() { Reason = "Message queued for processing" },
			ContextCollection =
			[
				new Context() { Type = "MAWBNumber", Value = "020-12345001" },
				new Context()
				{
					Type = "OriginalFWBFHLMessage",
					Value = "FHL/5\nMBI/618-98234850ZRHHKG/T4K1502\nHBS/P00003337/ZRHHKG/4/K1502/2/MOBILE PHONES\n/GEN/HLC\nTXT/OTHER ASSORTED PARTS\nOCI/AU/NFY/CT/3212345678"
				}
			]
		},
	};

	#endregion
}
