using System;
using System.CodeDom;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Products.CACustoms.Schemas;
using CargoWise.eHub.Core.Tests.PipelineComponents;
using Microsoft.BizTalk.Component.Interop;
using Rhino.Mocks;
using Microsoft.BizTalk.Message.Interop;
using System.Collections;
using System.Data.SqlClient;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Products.CACustoms.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Core.PropertySchemas;

namespace CargoWise.eHub.Products.CACustoms.Tests
{
	[TestClass]
	public class CACXmlDissasemblerExtensionTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Tester()
		{
			var tester = new CACXmlDissasemblerExtension();
			var classID = Guid.Empty;
			tester.GetClassID(out classID);
			Assert.AreEqual<Guid>(new Guid("6EBC169A-57C8-4A91-B45F-AD30CE1CD56C"), classID);
			Assert.AreEqual<string>("1.0", tester.Version);
			Assert.AreEqual<string>("", tester.Description);
			Assert.AreEqual<string>("Xml Disassembler Extension for CACustoms", tester.Name);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCACEdiDisassembleExtensionTest_CADMessage()
		{
			var messageText = "<DocumentMetaData xmlns=\"urn:wco:datamodel:WCO:Declaration:1\"><CommunicationMetaData><ApplicationReferenceID>3333333333333100001001</ApplicationReferenceID><Recipient><ID>123456170RM0001</ID></Recipient></CommunicationMetaData><Response><IssueDateTime><DateTimeString>20211004100812</DateTimeString></IssueDateTime><Status><NameCode>200</NameCode></Status></Response></DocumentMetaData>";
			var expected = @"<CanadianCustomsReply xmlns=""http://cargowise.com/ehub/products/canadiancustoms"" xmlns:ns0=""http://cargowise.com/ehub/core/2011/02"">
	<Reference>3333333333333100001001</Reference>
	<Content>PERvY3VtZW50TWV0YURhdGEgeG1sbnM9InVybjp3Y286ZGF0YW1vZGVsOldDTzpEZWNsYXJhdGlvbjoxIj48Q29tbXVuaWNhdGlvbk1ldGFEYXRhPjxBcHBsaWNhdGlvblJlZmVyZW5jZUlEPjMzMzMzMzMzMzMzMzMxMDAwMDEwMDE8L0FwcGxpY2F0aW9uUmVmZXJlbmNlSUQ+PFJlY2lwaWVudD48SUQ+MTIzNDU2MTcwUk0wMDAxPC9JRD48L1JlY2lwaWVudD48L0NvbW11bmljYXRpb25NZXRhRGF0YT48UmVzcG9uc2U+PElzc3VlRGF0ZVRpbWU+PERhdGVUaW1lU3RyaW5nPjIwMjExMDA0MTAwODEyPC9EYXRlVGltZVN0cmluZz48L0lzc3VlRGF0ZVRpbWU+PFN0YXR1cz48TmFtZUNvZGU+MjAwPC9OYW1lQ29kZT48L1N0YXR1cz48L1Jlc3BvbnNlPjwvRG9jdW1lbnRNZXRhRGF0YT4=</Content>
</CanadianCustomsReply>";
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes(messageText));
			message.Context = MessageFactory.CreateMessageContext();

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			SetupResult.For(pipelineContext.PipelineName).Return("pipelineName");
			SetupResult.For(pipelineContext.GetMessageFactory()).Return(MessageFactory);
			var subscriptionAccessorMock = MockRepository.StrictMock<ISubscriptionAccessor>();
			subscriptionAccessorMock.Expect(_ => _.SelectSubscribedClients(message)).IgnoreArguments().Repeat.Once().Return(new[] { "TSTTSTTST" });
			var component = MockRepository.PartialMock<CACXmlDissasemblerExtension>();
			component.ProcessSubscriptions = true;
			component.Expect(_ => _.BaseDisassemble(pipelineContext, message)).Repeat.Never();
			component.Expect(_ => _.GetSubscriptionAccessor()).Repeat.Once().Return(subscriptionAccessorMock);
			MockRepository.ReplayAll();
			component.Disassemble(pipelineContext, message);
			var result = component.GetNext(pipelineContext);
			var resultText = result.BodyPart.GetOriginalDataStream().ReadToEnd();
			Assert.AreEqual(expected, resultText);
			Assert.AreEqual("TSTTSTTST", result.Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual("http://cargowise.com/ehub/products/canadiancustoms#CanadianCustomsReply", result.Context.ReadPropertyString<BTS.MessageType>());
			Assert.AreEqual("CAD", result.Context.ReadPropertyString<OverrideEmailSubject>());

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCACEdiDisassembleExtensionTest_AccountHolding()
		{
			var expected = @"<CanadianCustomsReply xmlns=""http://cargowise.com/ehub/products/canadiancustoms"" xmlns:ns0=""http://cargowise.com/ehub/core/2011/02"">
	<Reference>202110191RM0001</Reference>
	<Content>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iaXNvLTg4NTktMSI/Pg0KPFpDQVJNU09BIGZpbGVfc3BsaXQ9IkUwMSIgZmlsZV90eXBlPSJQQSIgZmlsZV9uYW1lPSJTT0EtMjAyMTEwMTkxLTIwMjExMTI1MTczNjQ3IiB2ZXJzaW9uPSIyMDE5MTIiPg0KICA8SEVBREVSPg0KICAgIDxQRVJfU1RBUlQ+MjAyMS0xMC0xODwvUEVSX1NUQVJUPg0KICAgIDxQRVJfRU5EPjIwMjEtMTEtMTc8L1BFUl9FTkQ+DQogICAgPFBBUlRZPg0KICAgICAgPEJOOT4yMDIxMTAxOTE8L0JOOT4NCiAgICAgIDxOQU1FX09SRzE+RmlnIEltcG9ydGVyPC9OQU1FX09SRzE+DQogICAgICA8T1BfTkFNRT5GaWcgSW1wb3J0ZXIgQnJhbmNoIDE8L09QX05BTUU+DQogICAgICA8WlpOQU1FPkltcG9ydGF0aW9uczwvWlpOQU1FPg0KICAgICAgPEFDQ09VTlQ+MjAyMTEwMTkxUk0wMDAxPC9BQ0NPVU5UPg0KICAgIDwvUEFSVFk+DQogICAgPFNPQV9EQVRFPjIwMjEtMTEtMjU8L1NPQV9EQVRFPg0KICAgIDxQQVlfRFVFPjIwMjEtMTItMDE8L1BBWV9EVUU+DQogICAgPEdSQU5EX1RPVD40MjgxOS45MjwvR1JBTkRfVE9UPg0KICA8L0hFQURFUj4NCiAgPFNVTU1BUlk+DQogICAgPExBU1RfVE9UX0E+MjM1MDkuOTY8L0xBU1RfVE9UX0E+DQogICAgPENPUlJfTEFTVF9CPjAuMDwvQ09SUl9MQVNUX0I+DQogICAgPFBBWV9MQVNUX0M+LTYwMC4wPC9QQVlfTEFTVF9DPg0KICAgIDxESVNCX0Q+MC4wPC9ESVNCX0Q+DQogICAgPElOVEVSRVNUX0U+MjQwLjY5PC9JTlRFUkVTVF9FPg0KICAgIDxERUJJVF9GPjE5NjY5LjI3PC9ERUJJVF9GPg0KICAgIDxDUkVESVRfRz4wLjA8L0NSRURJVF9HPg0KICAgIDxUT1RQQVlfSD40MjgxOS45MjwvVE9UUEFZX0g+DQogICAgPFJFVl9ESVNUPg0KICAgICAgPERVVElFUz44Mjc4Ljc8L0RVVElFUz4NCiAgICAgIDxFWENJU0U+NjkyLjAzPC9FWENJU0U+DQogICAgICA8RVhDSVNFRFVUSUVTPjAuMDwvRVhDSVNFRFVUSUVTPg0KICAgICAgPFNJTUE+MC4wPC9TSU1BPg0KICAgICAgPEdTVD44Nzk4LjU0PC9HU1Q+DQogICAgICA8SFNUPjAuMDwvSFNUPg0KICAgICAgPFBTVD4wLjA8L1BTVD4NCiAgICAgIDxQQVlNRU5UUz4tNjAwLjA8L1BBWU1FTlRTPg0KICAgICAgPE9USEVSUz4xOTAwLjA8L09USEVSUz4NCiAgICAgIDxUT1RBTFM+MTkwNjkuMjc8L1RPVEFMUz4NCiAgICA8L1JFVl9ESVNUPg0KICA8L1NVTU1BUlk+DQogIDxERVRBSUxTPg0KICAgIDxQUk9HUkFNX0FDQ09VTlQgY291bnQ9IjMiPg0KICAgICAgPEFDQ09VTlQ+MjAyMTEwMTkxUk0wMDAxPC9BQ0NPVU5UPg0KICAgICAgPERBWV9TVU1NQVJZIGlkPSIxIj4NCiAgICAgICAgPFJFTEVBU0VfREFURT4yMDIxLTEwLTIwPC9SRUxFQVNFX0RBVEU+DQogICAgICAgIDxBQ0NPVU5USU5HX0RBVEU+MjAyMS0xMC0yMDwvQUNDT1VOVElOR19EQVRFPg0KICAgICAgICA8TElORUlURU0+DQogICAgICAgICAgPERVVElFUz42Mjc4Ljc8L0RVVElFUz4NCiAgICAgICAgICA8RVhDSVNFPjY5Mi4wMzwvRVhDSVNFPg0KICAgICAgICAgIDxFWENJU0VEVVRJRVM+MC4wPC9FWENJU0VEVVRJRVM+DQogICAgICAgICAgPFNJTUE+MC4wPC9TSU1BPg0KICAgICAgICAgIDxHU1Q+ODc5OC41NDwvR1NUPg0KICAgICAgICAgIDxIU1Q+MC4wPC9IU1Q+DQogICAgICAgICAgPFBTVD4wLjA8L1BTVD4NCiAgICAgICAgICA8UEFZTUVOVFM+MC4wPC9QQVlNRU5UUz4NCiAgICAgICAgICA8UEFZTUVOVF9EVUVfREFURT4yMDIxLTEyLTAxPC9QQVlNRU5UX0RVRV9EQVRFPg0KICAgICAgICAgIDxPVEhFUlM+NTAwLjA8L09USEVSUz4NCiAgICAgICAgICA8VE9UQUxTPjE2MjY5LjI3PC9UT1RBTFM+DQogICAgICAgIDwvTElORUlURU0+DQogICAgICA8L0RBWV9TVU1NQVJZPg0KICAgICAgPERBWV9TVU1NQVJZIGlkPSIyIj4NCiAgICAgICAgPFJFTEVBU0VfREFURT4yMDIxLTEwLTI3PC9SRUxFQVNFX0RBVEU+DQogICAgICAgIDxBQ0NPVU5USU5HX0RBVEU+MjAyMS0xMC0yNzwvQUNDT1VOVElOR19EQVRFPg0KICAgICAgICA8TElORUlURU0+DQogICAgICAgICAgPERVVElFUz4yMDAwLjA8L0RVVElFUz4NCiAgICAgICAgICA8RVhDSVNFPjAuMDwvRVhDSVNFPg0KICAgICAgICAgIDxFWENJU0VEVVRJRVM+MC4wPC9FWENJU0VEVVRJRVM+DQogICAgICAgICAgPFNJTUE+MC4wPC9TSU1BPg0KICAgICAgICAgIDxHU1Q+MC4wPC9HU1Q+DQogICAgICAgICAgPEhTVD4wLjA8L0hTVD4NCiAgICAgICAgICA8UFNUPjAuMDwvUFNUPg0KICAgICAgICAgIDxQQVlNRU5UUz4tNjAwLjA8L1BBWU1FTlRTPg0KICAgICAgICAgIDxQQVlNRU5UX0RVRV9EQVRFPjIwMjEtMTItMDE8L1BBWU1FTlRfRFVFX0RBVEU+DQogICAgICAgICAgPE9USEVSUz42MDAuMDwvT1RIRVJTPg0KICAgICAgICAgIDxUT1RBTFM+MjAwMC4wPC9UT1RBTFM+DQogICAgICAgIDwvTElORUlURU0+DQogICAgICA8L0RBWV9TVU1NQVJZPg0KICAgICAgPERBWV9TVU1NQVJZIGlkPSIzIj4NCiAgICAgICAgPFJFTEVBU0VfREFURT4yMDIxLTEwLTI4PC9SRUxFQVNFX0RBVEU+DQogICAgICAgIDxBQ0NPVU5USU5HX0RBVEU+MjAyMS0xMC0yODwvQUNDT1VOVElOR19EQVRFPg0KICAgICAgICA8TElORUlURU0+DQogICAgICAgICAgPERVVElFUz4wLjA8L0RVVElFUz4NCiAgICAgICAgICA8RVhDSVNFPjAuMDwvRVhDSVNFPg0KICAgICAgICAgIDxFWENJU0VEVVRJRVM+MC4wPC9FWENJU0VEVVRJRVM+DQogICAgICAgICAgPFNJTUE+MC4wPC9TSU1BPg0KICAgICAgICAgIDxHU1Q+MC4wPC9HU1Q+DQogICAgICAgICAgPEhTVD4wLjA8L0hTVD4NCiAgICAgICAgICA8UFNUPjAuMDwvUFNUPg0KICAgICAgICAgIDxQQVlNRU5UUz4wLjA8L1BBWU1FTlRTPg0KICAgICAgICAgIDxQQVlNRU5UX0RVRV9EQVRFPjIwMjEtMTItMDE8L1BBWU1FTlRfRFVFX0RBVEU+DQogICAgICAgICAgPE9USEVSUz44MDAuMDwvT1RIRVJTPg0KICAgICAgICAgIDxUT1RBTFM+ODAwLjA8L1RPVEFMUz4NCiAgICAgICAgPC9MSU5FSVRFTT4NCiAgICAgIDwvREFZX1NVTU1BUlk+DQogICAgICA8TElORVRPVEFMUz4NCiAgICAgICAgPERVVElFUz44Mjc4Ljc8L0RVVElFUz4NCiAgICAgICAgPEVYQ0lTRT42OTIuMDM8L0VYQ0lTRT4NCiAgICAgICAgPEVYQ0lTRURVVElFUz4wLjA8L0VYQ0lTRURVVElFUz4NCiAgICAgICAgPFNJTUE+MC4wPC9TSU1BPg0KICAgICAgICA8R1NUPjg3OTguNTQ8L0dTVD4NCiAgICAgICAgPEhTVD4wLjA8L0hTVD4NCiAgICAgICAgPFBTVD4wLjA8L1BTVD4NCiAgICAgICAgPFBBWU1FTlRTPi02MDAuMDwvUEFZTUVOVFM+DQogICAgICAgIDxPVEhFUlM+MTkwMC4wPC9PVEhFUlM+DQogICAgICAgIDxUT1RBTFM+MTkwNjkuMjc8L1RPVEFMUz4NCiAgICAgIDwvTElORVRPVEFMUz4NCiAgICA8L1BST0dSQU1fQUNDT1VOVD4NCiAgPC9ERVRBSUxTPg0KICA8Tk9URVM+DQogICAgPE1FU1NBR0UgTGFuZ3VhZ2U9IkVOIj5Tb0EgLSBUaGUgQ0FSTSBDbGllbnQgUG9ydGFsIGlzIG5vdyBsaXZlLiBDaGVjayB0aGUgQ0JTQSBXZWJzaXRlIGZvciBtb3JlIGluZm9ybWF0aW9uLjwvTUVTU0FHRT4NCiAgICA8TUVTU0FHRSBMYW5ndWFnZT0iRlIiPlNvQSAtIExlIFBvcnRhaWwgY2xpZW50IGRlIGxhIEdDUkEgZXN0IG1haW50ZW5hbnQgZGlzcG9uaWJsZS4gVmVyaWZpZXIgbGUgc2l0ZSBXZWIgZGUgbEFTRkMgcG91ciBwbHVzIGRpbmZvcm1hdGlvbi48L01FU1NBR0U+DQogIDwvTk9URVM+DQo8L1pDQVJNU09BPg==</Content>
</CanadianCustomsReply>";
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = TestHelper.GetEmbeddedResource("PipelineComponents.TestFiles.CACustoms_AccountHolding.xml");
			message.Context = MessageFactory.CreateMessageContext();

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			SetupResult.For(pipelineContext.PipelineName).Return("pipelineName");
			SetupResult.For(pipelineContext.GetMessageFactory()).Return(MessageFactory);
			var subscriptionAccessorMock = MockRepository.StrictMock<ISubscriptionAccessor>();
			subscriptionAccessorMock.Expect(_ => _.SelectSubscribedClients(message)).IgnoreArguments().Repeat.Once().Return(new[] { "TSTTSTTST" });
			var component = MockRepository.PartialMock<CACXmlDissasemblerExtension>();
			component.ProcessSubscriptions = true;
			component.Expect(_ => _.BaseDisassemble(pipelineContext, message)).Repeat.Never();
			component.Expect(_ => _.GetSubscriptionAccessor()).Repeat.Once().Return(subscriptionAccessorMock);
			MockRepository.ReplayAll();
			component.Disassemble(pipelineContext, message);
			var result = component.GetNext(pipelineContext);
			var resultText = result.BodyPart.GetOriginalDataStream().ReadToEnd();
			Assert.AreEqual(expected, resultText);
			Assert.AreEqual("TSTTSTTST", result.Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual("http://cargowise.com/ehub/products/canadiancustoms#CanadianCustomsReply", result.Context.ReadPropertyString<BTS.MessageType>());
			Assert.AreEqual("CAD", result.Context.ReadPropertyString<OverrideEmailSubject>());

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCACEdiDisassembleExtensionTest_CustomBroker()
		{
			var expected = @"<CanadianCustomsReply xmlns=""http://cargowise.com/ehub/products/canadiancustoms"" xmlns:ns0=""http://cargowise.com/ehub/core/2011/02"">
	<Reference>202110192</Reference>
	<Content>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iaXNvLTg4NTktMSI/Pg0KPFpDQVJNRE5PVElDRUNCIGZpbGVfc3BsaXQ9IkUwMSIgZmlsZV90eXBlPSJDQiIgZmlsZV9uYW1lPSJETi0yMDIxMTAxOTItMjAyMTExMjUyMTA2MzIgIiB2ZXJzaW9uPSIyMDE5MTIiPg0KCTxIRUFERVI+DQoJCTxETl9EQVRFPjIwMjEtMTAtMjc8L0ROX0RBVEU+DQoJCTxQQVJUWT4NCgkJCTxCTjk+MjAyMTEwMTkyPC9CTjk+DQoJCQk8T1BfTkFNRT5DYW5hZGlhbiBCcm9rZXI8L09QX05BTUU+DQoJCQk8SUROVU1CRVI+NDQ5ODwvSUROVU1CRVI+DQoJCTwvUEFSVFk+DQoJPC9IRUFERVI+DQoJPFNVTU1BUlk+DQoJCTxUUk5fQ09VTlQ+MzwvVFJOX0NPVU5UPg0KCQk8VE9UX0lNUD4xNDM1LjAzPC9UT1RfSU1QPg0KCTwvU1VNTUFSWT4NCgk8REVUQUlMUyBjb3VudD0iMSI+DQoJCTxJTVBPUlRFUiBpZD0iMSI+DQoJCQk8UEFSVFk+DQoJCQkJPEJOOT4yMDIxMTAxOTE8L0JOOT4NCgkJCQk8TkFNRV9PUkcxPkZpZyBJbXBvcnRlcjwvTkFNRV9PUkcxPg0KCQkJCTxPUF9OQU1FPkZpZyBJbXBvcnRlciBCcmFuY2ggMTwvT1BfTkFNRT4NCgkJCTwvUEFSVFk+DQoJCQk8TElORUlURU1TIGNvdW50PSIzIj4NCgkJCQk8TElORUlURU0gaWQ9IjEiPg0KCQkJCQk8UkVMX0RBVEU+MjAyMS0xMC0yNzwvUkVMX0RBVEU+DQoJCQkJCTxBQ0NfREFURT4yMDIxLTEwLTI3PC9BQ0NfREFURT4NCgkJCQkJPERPQ19UWVBFPkxQPC9ET0NfVFlQRT4NCgkJCQkJPFRSQU5TX0RFU0M+SW5jb21pbmcgUGF5bWVudDwvVFJBTlNfREVTQz4NCgkJCQkJPFJFTF9ET0NfTlVNQkVSPjAyMDAwMDAxNTI1NTwvUkVMX0RPQ19OVU1CRVI+DQoJCQkJCTxBVE5fTlVNLz4NCgkJCQkJPENBRF9WRVJTSU9OPjAwMDAwPC9DQURfVkVSU0lPTj4NCgkJCQkJPFNVQl9CWT4yMDIxMTAxOTI8L1NVQl9CWT4NCgkJCQkJPFBPUlQvPg0KCQkJCQk8QU1PVU5UUz4NCgkJCQkJCTxEVVRJRVM+MC4wPC9EVVRJRVM+DQoJCQkJCQk8RVhDSVNFPjAuMDwvRVhDSVNFPg0KCQkJCQkJPEVYQ0lTRURVVElFUz4wLjA8L0VYQ0lTRURVVElFUz4NCgkJCQkJCTxTSU1BPjAuMDwvU0lNQT4NCgkJCQkJCTxHU1Q+MC4wPC9HU1Q+DQoJCQkJCQk8SFNUPjAuMDwvSFNUPg0KCQkJCQkJPFBTVD4wLjA8L1BTVD4NCgkJCQkJCTxJTlRFUkVTVD4wLjA8L0lOVEVSRVNUPg0KCQkJCQkJPFBFTkFMVElFUz4wLjA8L1BFTkFMVElFUz4NCgkJCQkJCTxQQVlNRU5UUz4tNjAwLjA8L1BBWU1FTlRTPg0KCQkJCQkJPFBBWU1FTlRfRFVFX0RBVEU+MjAyMS0xMC0yNzwvUEFZTUVOVF9EVUVfREFURT4NCgkJCQkJCTxPVEhFUlM+MC4wPC9PVEhFUlM+DQoJCQkJCQk8VE9UQUxTPi02MDAuMDwvVE9UQUxTPg0KCQkJCQk8L0FNT1VOVFM+DQoJCQkJPC9MSU5FSVRFTT4NCgkJCQk8TElORUlURU0gaWQ9IjIiPg0KCQkJCQk8UkVMX0RBVEU+MjAyMS0xMC0yNzwvUkVMX0RBVEU+DQoJCQkJCTxBQ0NfREFURT4yMDIxLTEwLTI3PC9BQ0NfREFURT4NCgkJCQkJPERPQ19UWVBFPkIzPC9ET0NfVFlQRT4NCgkJCQkJPFRSQU5TX0RFU0M+Q3VzdG9tcyBEdXRpZXM8L1RSQU5TX0RFU0M+DQoJCQkJCTxSRUxfRE9DX05VTUJFUj5URVNUODwvUkVMX0RPQ19OVU1CRVI+DQoJCQkJCTxBVE5fTlVNPjAyMTEwMDFORjAyPC9BVE5fTlVNPg0KCQkJCQk8Q0FEX1ZFUlNJT04+MDAwMDE8L0NBRF9WRVJTSU9OPg0KCQkJCQk8U1VCX0JZPjIwMjExMDE5MjwvU1VCX0JZPg0KCQkJCQk8U1RBVFVTPlU8L1NUQVRVUz4NCgkJCQkJPFBPUlQ+NDk1MTwvUE9SVD4NCgkJCQkJPEFNT1VOVFM+DQoJCQkJCQk8RFVUSUVTPjIwMDAuMDwvRFVUSUVTPg0KCQkJCQkJPEVYQ0lTRT4wLjA8L0VYQ0lTRT4NCgkJCQkJCTxFWENJU0VEVVRJRVM+MC4wPC9FWENJU0VEVVRJRVM+DQoJCQkJCQk8U0lNQT4wLjA8L1NJTUE+DQoJCQkJCQk8R1NUPjAuMDwvR1NUPg0KCQkJCQkJPEhTVD4wLjA8L0hTVD4NCgkJCQkJCTxQU1Q+MC4wPC9QU1Q+DQoJCQkJCQk8SU5URVJFU1Q+MC4wPC9JTlRFUkVTVD4NCgkJCQkJCTxQRU5BTFRJRVM+MC4wPC9QRU5BTFRJRVM+DQoJCQkJCQk8UEFZTUVOVFM+MC4wPC9QQVlNRU5UUz4NCgkJCQkJCTxQQVlNRU5UX0RVRV9EQVRFPjIwMjEtMTItMDE8L1BBWU1FTlRfRFVFX0RBVEU+DQoJCQkJCQk8T1RIRVJTPjAuMDwvT1RIRVJTPg0KCQkJCQkJPFRPVEFMUz4yMDAwLjA8L1RPVEFMUz4NCgkJCQkJPC9BTU9VTlRTPg0KCQkJCTwvTElORUlURU0+DQoJCQkJPExJTkVJVEVNIGlkPSIzIj4NCgkJCQkJPFJFTF9EQVRFPjIwMjEtMTAtMjc8L1JFTF9EQVRFPg0KCQkJCQk8QUNDX0RBVEU+MjAyMS0xMC0yNzwvQUNDX0RBVEU+DQoJCQkJCTxET0NfVFlQRT5JTjwvRE9DX1RZUEU+DQoJCQkJCTxUUkFOU19ERVNDPkludGVyZXN0IFJlY2VpdmFibGU8L1RSQU5TX0RFU0M+DQoJCQkJCTxSRUxfRE9DX05VTUJFUj4wMTEwMDAwMDA3MjI8L1JFTF9ET0NfTlVNQkVSPg0KCQkJCQk8QVROX05VTT5BVVRDQUQxMDU5MTI8L0FUTl9OVU0+DQoJCQkJCTxDQURfVkVSU0lPTj4wMDAwMDwvQ0FEX1ZFUlNJT04+DQoJCQkJCTxTVUJfQlk+Q0JTQTwvU1VCX0JZPg0KCQkJCQk8U1RBVFVTPlU8L1NUQVRVUz4NCgkJCQkJPFBPUlQvPg0KCQkJCQk8QU1PVU5UUz4NCgkJCQkJCTxEVVRJRVM+MC4wPC9EVVRJRVM+DQoJCQkJCQk8RVhDSVNFPjAuMDwvRVhDSVNFPg0KCQkJCQkJPEVYQ0lTRURVVElFUz4wLjA8L0VYQ0lTRURVVElFUz4NCgkJCQkJCTxTSU1BPjAuMDwvU0lNQT4NCgkJCQkJCTxHU1Q+MC4wPC9HU1Q+DQoJCQkJCQk8SFNUPjAuMDwvSFNUPg0KCQkJCQkJPFBTVD4wLjA8L1BTVD4NCgkJCQkJCTxJTlRFUkVTVD4zNS4wMzwvSU5URVJFU1Q+DQoJCQkJCQk8UEVOQUxUSUVTPjAuMDwvUEVOQUxUSUVTPg0KCQkJCQkJPFBBWU1FTlRTPjAuMDwvUEFZTUVOVFM+DQoJCQkJCQk8UEFZTUVOVF9EVUVfREFURT4yMDIxLTEwLTI3PC9QQVlNRU5UX0RVRV9EQVRFPg0KCQkJCQkJPE9USEVSUz4wLjA8L09USEVSUz4NCgkJCQkJCTxUT1RBTFM+MzUuMDM8L1RPVEFMUz4NCgkJCQkJPC9BTU9VTlRTPg0KCQkJCTwvTElORUlURU0+DQoJCQk8L0xJTkVJVEVNUz4NCgkJCTxMSU5FVE9UQUxTPg0KCQkJCTxEVVRJRVM+MjAwMC4wPC9EVVRJRVM+DQoJCQkJPEVYQ0lTRT4wLjA8L0VYQ0lTRT4NCgkJCQk8RVhDSVNFRFVUSUVTPjAuMDwvRVhDSVNFRFVUSUVTPg0KCQkJCTxTSU1BPjAuMDwvU0lNQT4NCgkJCQk8R1NUPjAuMDwvR1NUPg0KCQkJCTxIU1Q+MC4wPC9IU1Q+DQoJCQkJPFBTVD4wLjA8L1BTVD4NCgkJCQk8SU5URVJFU1Q+MzUuMDM8L0lOVEVSRVNUPg0KCQkJCTxQRU5BTFRJRVM+MC4wPC9QRU5BTFRJRVM+DQoJCQkJPFBBWU1FTlRTPi02MDAuMDwvUEFZTUVOVFM+DQoJCQkJPFBBWU1FTlRfRFVFX0RBVEU+MjAyMS0xMC0yNzwvUEFZTUVOVF9EVUVfREFURT4NCgkJCQk8T1RIRVJTPjAuMDwvT1RIRVJTPg0KCQkJCTxUT1RBTFM+MTQzNS4wMzwvVE9UQUxTPg0KCQkJPC9MSU5FVE9UQUxTPg0KCQk8L0lNUE9SVEVSPg0KCTwvREVUQUlMUz4NCgk8Tk9URVM+DQoJCTxNRVNTQUdFIExhbmd1YWdlPSJFTiI+RE5DQiAtIFRoZSBDQVJNIENsaWVudCBQb3J0YWwgaXMgbm93IGxpdmUuIENoZWNrIHRoZSBDQlNBIFdlYnNpdGUgZm9yIG1vcmUgaW5mb3JtYXRpb24uPC9NRVNTQUdFPg0KCQk8TUVTU0FHRSBMYW5ndWFnZT0iRlIiPkROQ0IgLSBMZSBQb3J0YWlsIGNsaWVudCBkZSBsYSBHQ1JBIGVzdCBtYWludGVuYW50IGRpc3BvbmlibGUuIFZlcmlmaWVyIGxlIHNpdGUgV2ViIGRlIGxBU0ZDIHBvdXIgcGx1cyBkaW5mb3JtYXRpb24uPC9NRVNTQUdFPg0KCTwvTk9URVM+DQo8L1pDQVJNRE5PVElDRUNCPg==</Content>
</CanadianCustomsReply>";
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = TestHelper.GetEmbeddedResource("PipelineComponents.TestFiles.CACustoms_CustomsBrokerInput.xml");
			message.Context = MessageFactory.CreateMessageContext();

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			SetupResult.For(pipelineContext.PipelineName).Return("pipelineName");
			SetupResult.For(pipelineContext.GetMessageFactory()).Return(MessageFactory);
			var subscriptionAccessorMock = MockRepository.StrictMock<ISubscriptionAccessor>();
			subscriptionAccessorMock.Expect(_ => _.SelectSubscribedClients(message)).IgnoreArguments().Repeat.Once().Return(new[] { "TSTTSTTST" });
			var component = MockRepository.PartialMock<CACXmlDissasemblerExtension>();
			component.ProcessSubscriptions = true;
			component.Expect(_ => _.BaseDisassemble(pipelineContext, message)).Repeat.Never();
			component.Expect(_ => _.GetSubscriptionAccessor()).Repeat.Once().Return(subscriptionAccessorMock);
			MockRepository.ReplayAll();
			component.Disassemble(pipelineContext, message);
			var result = component.GetNext(pipelineContext);
			var resultText = result.BodyPart.GetOriginalDataStream().ReadToEnd();
			Assert.AreEqual(expected, resultText);
			Assert.AreEqual("TSTTSTTST", result.Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual("http://cargowise.com/ehub/products/canadiancustoms#CanadianCustomsReply", result.Context.ReadPropertyString<BTS.MessageType>());
			Assert.AreEqual("CAD", result.Context.ReadPropertyString<OverrideEmailSubject>());

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCACEdiDisassembleExtensionTest_XmlUnknown()
		{
			var messageText = "<DocumentMetaData xmlns=\"namespace\"><CommunicationMetaData><ApplicationReferenceID>1111111111</ApplicationReferenceID><Recipient><ID>123456170RM0001</ID></Recipient></CommunicationMetaData><Response><IssueDateTime><DateTimeString>20211004100812</DateTimeString></IssueDateTime><Status><NameCode>200</NameCode></Status></Response></DocumentMetaData>";
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes(messageText));
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.Write("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06", "00000000-0000-0000-0000-000000000001");
			message.Context.Write("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06", "00000000-0000-0000-0000-000000000001");

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			SetupResult.For(pipelineContext.PipelineName).Return("pipelineName");
			SetupResult.For(pipelineContext.GetMessageFactory()).Return(MessageFactory);
			var exceptionsAccessor = MockRepository.StrictMock<IExceptionsAccessor>();
			exceptionsAccessor.Expect(x => x.SubmitErrorAndUpdateStatus(
				Arg<Guid>.Is.Anything,
				Arg<string>.Is.Equal("BIZ"),
				Arg<string>.Is.Equal("Failure"),
				Arg<string>.Matches(description => description.Contains("Unsupported message type")),
				Arg<Guid>.Is.Anything,
				Arg<Guid>.Is.Anything,
				Arg<Guid>.Is.Anything,
				Arg<Guid>.Is.Anything,
				Arg<SqlConnection>.Is.Anything,
				Arg<bool>.Is.Equal(false))).Repeat.Once();
			var component = MockRepository.PartialMock<CACXmlDissasemblerExtension>();
			component.Expect(_ => _.BaseDisassemble(pipelineContext, message)).Repeat.Never();
			component.Stub(x => x.GetExceptionsAccessor()).Return(exceptionsAccessor);
			MockRepository.ReplayAll();
			component.Disassemble(pipelineContext, message);
			MockRepository.VerifyAll();
		}
	}
}
