using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Universal.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.BatchProcessor.Testing
{
	sealed class ExceptionTest : SenderReceiverIDExceptionTest
	{
		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection messages) => new ZAInterchangeProvider(new LoggingInformation(), messages);
	}

	sealed class ZAInterchangeProviderTest : InterchangeProviderTestCase
	{
		[TestDate(2016, 04, 22, 01, 23, 01)]
		public void TestCommunicationsAgreementId_NotNeededFor16A()
		{
			var messages = new NonDependentEDIMessageCollection(Factory);
			var mock = Factory.NewMoq<CusEntryHeader>();

			ZString agentCode = "12345";

			mock.Setup(m => m.AgentCode).Returns(agentCode);

			var message = messages.AddNew();
			message.EM_LinkedObject = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageType = "EXP";
			message.EM_MessageText = "UNH+315+COSTCO:D:16A:UN:RCG001'BGM+788:::DOR+84BD4B6FE624449F995EB3E3BCC3FAD0+9'DTM+137:201803080000:203'DTM+132:201803080000:203'DTM+132:201803080000:203'FTX+ADI++929'TDT+20+123+1++:172:20+++:103'RFF+ACL:123'LOC+11+ADALV:139:6+::ZZZ'NAD+MS+01::ZZZ'NAD+RL+01::ZZZ'EQD+CN+123+20FR:102:5++2+4'DTM+164:201803220000:203'SEL+123+AB+1'EQD+CN+456+:102:5++2'SEL+NO SEAL NO++2'CNI+1+123'RFF+AHY:00053592'RFF+AAZ'RFF+MB:1::ST'RFF+ABT:::12'GID+1+1:X:::CN+0'FTX+DAR++X'FTX+AEC++X'FTX+AAA'MEA+AAE+AAB+KGM:0'MEA+AAL+ACE+KGM:0'MEA+AAE+ACE+LTR:0'MEA+AAL+AAB+LTR:0'PCI+24'SGP+123+0'CNI+2+3456'RFF+AHY'RFF+AAZ'RFF+MB:1::ST'RFF+ABT'GID+1+1::::CN+0'FTX+DAR'FTX+AEC'FTX+AAA'MEA+AAE+AAB+KGM:0'MEA+AAL+ACE+KGM:0'MEA+AAE+ACE+LTR:0.000'MEA+AAL+AAB+LTR:0'PCI+24'SGP+123+0'GID+2+1::::CN+0'FTX+DAR'FTX+AEC'FTX+AAA'MEA+AAE+AAB+KGM:0'MEA+AAL+ACE+KGM:0'MEA+AAE+ACE+LTR:0.000'MEA+AAL+AAB+LTR:0'PCI+24'SGP+123+0'GID+3+0::::CN+0'FTX+DAR'FTX+AEC'FTX+AAA'MEA+AAE+AAB+KGM:0'MEA+AAL+ACE+KGM:0'MEA+AAE+ACE+LTR:0.000'MEA+AAL+AAB+LTR:0'PCI+24'SGP+456+0'GID+4+0::::CN+0'FTX+DAR'FTX+AEC'FTX+AAA'MEA+AAE+AAB+KGM:0'MEA+AAL+ACE+KGM:0'MEA+AAE+ACE+LTR:0.000'MEA+AAL+AAB+LTR:0'PCI+24'SGP++0'CNT+8:3'";
			message.EM_MessageOwner = "";
			message.EM_IsTestMessage = true;

			var provider = new ZAInterchangeProvider(new LoggingInformation(), messages);
			var interchanges = new EDIInterchangeCollection(Factory);
			interchanges.AddRange(provider.Interchanges);

			CombineAssertions(() =>
			{
				AssertEquals("NumberOfInterchanges", 1, interchanges.Count);
				var interchange1 = interchanges.FirstOrDefault(x => x.PK == message.EM_EI) as EDIInterchange;
				AssertEquals("1 - Header", "UNB+UNOB:4+12345::SENDERID:SENDERSUBID+SARSEXPT+20160422:0123+<<INTERCHANGENUMBERPLACEHOLDER>>++EXPORT++1++1'\n", interchange1.EI_HeaderText);
			});
		}

		public void TestTranslateMessageTypeToApplicationReference()
		{
			var pairs = new Dictionary<string, string>();
			pairs.Add(SARSEDIMessage.MessageTypes.COSTCO, SARSEDIMessage.MessageTypeNames.COSTCO);
			pairs.Add(SARSEDIMessage.MessageTypes.CALINF, SARSEDIMessage.MessageTypeNames.CALINF);
			pairs.Add(SARSEDIMessage.MessageTypes.CUSDEC, SARSEDIMessage.MessageTypeNames.CUSDEC);
			pairs.Add(SARSEDIMessage.MessageTypes.EXPORT, SARSEDIMessage.MessageTypeNames.EXPORT);
			pairs.Add(SARSEDIMessage.MessageTypes.REQDOC, SARSEDIMessage.MessageTypeNames.REQDOC);
			pairs.Add(SARSEDIMessage.MessageTypes.CUSCAR, SARSEDIMessage.MessageTypeNames.CUSCAR);
			pairs.Add(SARSEDIMessage.MessageTypes.GOVGIO, SARSEDIMessage.MessageTypeNames.GOVGIO);

			foreach (var pair in pairs)
			{
				AssertEquals("Expected code name for mesage code" + pair.Key, pair.Value, ZAInterchangeProvider.TranslateMessageTypeToApplicationReference(pair.Key));
			}
		}

		public void TestTranslateMessageTypeToRecipientIdentification()
		{
			List<Tuple<ZString, bool, ZString>> testCases = new List<Tuple<ZString, bool, ZString>>();
			testCases.Add(SARSEDIMessage.MessageTypes.CUSDEC, false, "SARSDEC");
			testCases.Add(SARSEDIMessage.MessageTypes.EXPORT, false, "SARSEXP");
			testCases.Add(SARSEDIMessage.MessageTypes.REQDOC, false, "SARSREQ");
			testCases.Add(SARSEDIMessage.MessageTypes.CUSCAR, false, "SARSCAR");
			testCases.Add(SARSEDIMessage.MessageTypes.COSTCO, false, "SARSCAR");
			testCases.Add(SARSEDIMessage.MessageTypes.CALINF, false, "SARSCAR");
			testCases.Add(SARSEDIMessage.MessageTypes.GOVGIO, false, "SARSCAR");

			testCases.Add(SARSEDIMessage.MessageTypes.CUSDEC, true, "SARSDECT");
			testCases.Add(SARSEDIMessage.MessageTypes.EXPORT, true, "SARSEXPT");
			testCases.Add(SARSEDIMessage.MessageTypes.REQDOC, true, "SARSREQT");
			testCases.Add(SARSEDIMessage.MessageTypes.CUSCAR, true, "SARSCART");
			testCases.Add(SARSEDIMessage.MessageTypes.COSTCO, true, "SARSCART");
			testCases.Add(SARSEDIMessage.MessageTypes.CALINF, true, "SARSCART");
			testCases.Add(SARSEDIMessage.MessageTypes.GOVGIO, true, "SARSCART");

			foreach (var testCase in testCases)
			{
				ZString messageType = testCase.Item1;
				bool isTest = testCase.Item2;
				ZString expectedRecipientIdentification = testCase.Item3;
				ZString actualRecipientIdentification = ZAInterchangeProvider.TranslateMessageTypeToRecipientIdentification(messageType, isTest);
				AssertEquals($"Expected recipient identification for mesage type '{messageType}' with testMode={isTest}", expectedRecipientIdentification, actualRecipientIdentification);
			}
		}

		[TestDate(2016, 04, 22, 01, 23, 01)]
		public override void TestMessagesPopulateNewInterchange()
		{
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			var mock = Factory.NewMoq<CusEntryHeader>();

			ZString agentCode = "12345";
			mock.Setup(m => m.AgentCode).Returns(agentCode);

			EDIMessage message1 = messages.AddNew();
			message1.EM_LinkedObject = mock.Object;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message1.EM_GB = GlbBranch.CurrentBranch.PK;
			message1.EM_MessageType = "EXP";
			message1.EM_MessageText = "UNH+189+CUSDEC:D:96B:UN:ZZZ01'BGM+830+?'+9'";
			message1.EM_MessageOwner = "";
			message1.EM_IsTestMessage = true;

			EDIMessage message2 = messages.AddNew();
			message2.EM_LinkedObject = mock.Object;
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message2.EM_GB = GlbBranch.CurrentBranch.PK;
			message2.EM_MessageType = "EXP";
			message2.EM_MessageText = "this is not for you";
			message2.EM_MessageOwner = "";
			message2.EM_IsTestMessage = true;

			EDIMessage message3 = messages.AddNew();
			message3.EM_LinkedObject = mock.Object;
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message3.EM_GB = GlbBranch.CurrentBranch.PK;
			message3.EM_MessageType = "DEC";
			message3.EM_MessageText = "this is another test";
			message3.EM_MessageOwner = "";
			message3.EM_IsTestMessage = false;

			ZAInterchangeProvider provider = new ZAInterchangeProvider(new LoggingInformation(), messages);
			EDIInterchangeCollection interchanges = new EDIInterchangeCollection(Factory);
			interchanges.AddRange(provider.Interchanges);

			CombineAssertions(() =>
			{
				AssertEquals("NumberOfInterchanges", 3, interchanges.Count);
				var interchange1 = interchanges.FirstOrDefault(x => x.PK == message1.EM_EI) as EDIInterchange;
				var interchange2 = interchanges.FirstOrDefault(x => x.PK == message2.EM_EI) as EDIInterchange;
				var interchange3 = interchanges.FirstOrDefault(x => x.PK == message3.EM_EI) as EDIInterchange;
				AssertNotEquals(message1.EM_EI, message2.EM_EI);
				AssertNotEquals(message1.EM_EI, message3.EM_EI);
				AssertNotEquals(message2.EM_EI, message3.EM_EI);
				AssertEquals("1 - Header", "UNB+UNOB:4+12345::SENDERID:SENDERSUBID+SARSEXPT+20160422:0123+<<INTERCHANGENUMBERPLACEHOLDER>>++EXPORT++1+TRADINGPARTNER+1'\n", interchange1.EI_HeaderText);
				AssertEquals("1 - Footer", "UNZ+1+<<INTERCHANGENUMBERPLACEHOLDER>>'", interchange1.EI_FooterText);
				AssertEquals("1 - Body", "UNH+189+CUSDEC:D:96B:UN:ZZZ01'\nBGM+830+?'+9'\n", interchange1.EI_BodyText);
				AssertEquals("1 - To", "ZACustoms", interchange1.EI_To);
				AssertEquals("2 - Header", "UNB+UNOB:4+12345::SENDERID:SENDERSUBID+SARSEXPT+20160422:0123+<<INTERCHANGENUMBERPLACEHOLDER>>++EXPORT++1+TRADINGPARTNER+1'\n", interchange2.EI_HeaderText);
				AssertEquals("2 - Footer", "UNZ+1+<<INTERCHANGENUMBERPLACEHOLDER>>'", interchange2.EI_FooterText);
				AssertEquals("2 - Body", "this is not for you", interchange2.EI_BodyText);
				AssertEquals("2 - To", "ZACustoms", interchange2.EI_To);
				AssertEquals("3 - Header", "UNB+UNOB:4+12345::SENDERID:SENDERSUBID+SARSDEC+20160422:0123+<<INTERCHANGENUMBERPLACEHOLDER>>++CUSDEC++1+TRADINGPARTNER'\n", interchange3.EI_HeaderText);
				AssertEquals("3 - Footer", "UNZ+1+<<INTERCHANGENUMBERPLACEHOLDER>>'", interchange3.EI_FooterText);
				AssertEquals("3 - Body", "this is another test", interchange3.EI_BodyText);
				AssertEquals("3 - To", "ZACustoms", interchange3.EI_To);
			});
		}

		[TestDate(2016, 04, 22, 01, 23, 01)]
		public void TestMessageWithoutLinkedObjectPopulatesInterchange()
		{
			var messages = new NonDependentEDIMessageCollection(Factory);

			var message1 = messages.AddNew(typeof(IInterchangeSenderIdProviderMessage));
			message1.EM_LinkedObject = null;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message1.EM_GB = GlbBranch.CurrentBranch.PK;
			message1.EM_MessageType = "EXP";
			message1.EM_MessageText = "UNH+189+CUSDEC:D:96B:UN:ZZZ01'BGM+830+?'+9'";
			message1.EM_MessageOwner = "";
			message1.EM_IsTestMessage = true;

			var message2 = messages.AddNew();
			message2.EM_LinkedObject = null;
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message2.EM_GB = GlbBranch.CurrentBranch.PK;
			message2.EM_MessageType = "EXP";
			message2.EM_MessageText = "this is not for you";
			message2.EM_MessageOwner = "";
			message2.EM_IsTestMessage = true;

			AssertNotNull(message1 as IInterchangeSenderIdProvider);
			AssertNull(message2 as IInterchangeSenderIdProvider);

			var provider = new ZAInterchangeProvider(new LoggingInformation(), messages);
			var interchanges = new EDIInterchangeCollection(Factory);
			interchanges.AddRange(provider.Interchanges);

			CombineAssertions(() =>
			{
				AssertEquals("NumberOfInterchanges", 2, interchanges.Count);
				AssertNotEquals(message1.EM_EI, message2.EM_EI);

				var interchange1 = interchanges.FirstOrDefault(x => x.PK == message1.EM_EI) as EDIInterchange;
				AssertEquals("1 - Header", "UNB+UNOB:4+9876::SENDERID:SENDERSUBID+SARSEXPT+20160422:0123+<<INTERCHANGENUMBERPLACEHOLDER>>++EXPORT++1+TRADINGPARTNER+1'\n", interchange1.EI_HeaderText);
				AssertEquals("1 - Footer", "UNZ+1+<<INTERCHANGENUMBERPLACEHOLDER>>'", interchange1.EI_FooterText);
				AssertEquals("1 - Body", "UNH+189+CUSDEC:D:96B:UN:ZZZ01'\nBGM+830+?'+9'\n", interchange1.EI_BodyText);
				AssertEquals("1 - To", "ZACustoms", interchange1.EI_To);

				var interchange2 = interchanges.FirstOrDefault(x => x.PK == message2.EM_EI) as EDIInterchange;
				AssertEquals("2 - Header", "", interchange2.EI_HeaderText);
				AssertEquals("2 - Footer", "UNZ+1+<<INTERCHANGENUMBERPLACEHOLDER>>'", interchange2.EI_FooterText);
				AssertEquals("2 - Body", "this is not for you", interchange2.EI_BodyText);
				AssertEquals("2 - To", "ZACustoms", interchange2.EI_To);
			});
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection) => new ZAInterchangeProvider(new LoggingInformation(), collection);

		protected override BusinessObject LinkedObject => Factory.NewMoq<CusEntryHeader>().Object;

		protected override void SetUp()
		{
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, true);
			base.SetUp();
		}

		sealed class IInterchangeSenderIdProviderMessage : EDIMessage, IInterchangeSenderIdProvider
		{
			public IInterchangeSenderIdProviderMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZString SenderID => "9876";
		}
	}
}
