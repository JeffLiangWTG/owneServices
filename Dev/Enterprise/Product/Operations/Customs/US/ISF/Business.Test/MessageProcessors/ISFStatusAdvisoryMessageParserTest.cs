using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Modules;
using Moq.Protected;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ISFStatusAdvisoryMessageParserTest : TestCaseWithFactory
	{
		public void TestParse()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingStatusAdvisory;
			message.EM_MessageNum = "~150000";
			message.EM_MessageText = "B018888FLRSA                                                                    " +
				"SA10FLR-20080000001                                                             " +
				"SA20V1 V1123456789                                                              " +
				"SA20CR USERDEFINED1234                                                          " +
				"SA20CR USERDEFINED1235                                                          " +
				"SA30HBSC999999999999                                                            " +
				"SA50S1BILL ON FILE                                                              " +
				"Y8888FLRSA00008                                                                 ";
			var mock = Factory.NewMoq<ISFDummyObject>();
			mock
				.Protected()
				.Setup<ZString>("HumanReadableNameCore")
				.Returns("ISFHELLO123");
			ISFDummyObject obj = mock.Object;
			var parser = new ISFStatusAdvisoryMessageParser(obj, message.GetMessageBlocks<MessageBlock>().ToArray());
			AssertEquals("URI", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.ImporterSecurityFiling, obj.PK.ToGuid()), parser.URI);
			string expectedBodyMessage = string.Format(@"Transaction Number FLR-20080000001<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>User-defined Reference Number</th></tr></thead><tr><td>USERDEFINED1234</td></tr><tr><td>USERDEFINED1235</td></tr></table><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Number</th><th>Disposition Code</th><th>Remarks</th></tr></thead><tr><td>HBSC999999999999</td><td>S1</td><td>{0}</td></tr></table>", DispositionCodeList.Descriptions.S1);
			AssertMultilineASCIIEquals("HtmlData", expectedBodyMessage, parser.HtmlData);
			expectedBodyMessage = string.Format(@"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>User-defined Reference Number</th></tr></thead><tr><td>USERDEFINED1234</td></tr><tr><td>USERDEFINED1235</td></tr></table><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Number</th><th>Disposition Code</th><th>Remarks</th></tr></thead><tr><td>HBSC999999999999</td><td>S1</td><td>{0}</td></tr></table>", DispositionCodeList.Descriptions.S1);
			AssertMultilineASCIIEquals("HtmlMessageDataOnly", expectedBodyMessage, parser.HtmlMessageDataOnly);
			AssertEquals("JobReference", "ISFHELLO123", parser.JobReference);
			mock.VerifyAll();
		}
	}
}
