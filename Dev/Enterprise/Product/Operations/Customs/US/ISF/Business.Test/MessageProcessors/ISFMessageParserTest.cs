using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Modules;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ISFMessageParserTest : TestCaseWithFactory
	{
		public void TestParse()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			message.EM_MessageNum = "~150000";

			message.EM_MessageText =
				"B018888XJ5SN                                               ~150000              " +
				"SF10103A  DUN4684446465             11               SCAS                    Y  " +
				"SF90  308CONSOLIDATOR NAME/ADDRESS REQUIRED                                     " +
				"SF90  303CONSIGNEE NUMBER REQUIRED                                              " +
				"SF15BMBM123456789                                                               " +
				"SF15OBOB123456789                                                               " +
				"SF20SBNSB123456789                                                              " +
				"SF20MB MB123456789                                                              " +
				"SF20V1 V1123456789                                                              " +
				"SF206B 6B123456789                                                              " +
				"SF20CR USERDEFINED1234                                                          " +
				"SF20CR USERDEFINED1235                                                          " +
				"SF2543TURE234323         14050                                                  " +
				"SF90  252INVALID EQUIPMENT DESCRIPTION CODE                                     " +
				"SF30BY IMPORTER AUSTRALIAN COMPANY        EI                                    " +
				"SF90  315INVALID ENTITY IDENTIFIER                                              " +
				"SF30MF MANACCOM PTY LTD                   EI                                    " +
				"SF90  315INVALID ENTITY IDENTIFIER                                              " +
				"SF401234568844AU                                                                " +
				"SF90  404INVALID HTS CODE                                                       " +
				"SF30MF MAINFREIGHT INTERNATIONAL             65007252333                        " +
				"SF401042334668US                                                                " +
				"SF90  404INVALID HTS CODE                                                       " +
				"SF9001   SECURITY FILING REJECTED                                               " +
				"Y  8888XJ5SN00023";

			var mock = Factory.NewMoq<ISFDummyObject>();
			mock
				.Protected()
				.Setup<ZString>("HumanReadableNameCore")
				.Returns("ISFHELLO123");
			var shipmentID1Mock = new Mock<IShipmentReferenceID> { CallBase = true };
			shipmentID1Mock.Setup(m => m.ShipmentReferenceIdentifier)
				.Returns("HB12345678");
			var shipmentID2Mock = new Mock<IShipmentReferenceID> { CallBase = true };
			shipmentID2Mock.Setup(m => m.ShipmentReferenceIdentifier)
				.Returns("HB87654321");
			mock.Setup(m => m.ShipmentIDs)
				.Returns(new IShipmentReferenceID[] { shipmentID1Mock.Object, shipmentID2Mock.Object });
			ISFDummyObject obj = mock.Object;
			var parser = new ISFMessageParser(obj, message.GetMessageBlocks<MessageBlock>().ToArray());

			AssertEquals("URI", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.ImporterSecurityFiling, obj.PK.ToGuid()), parser.URI);
			var expectedBodyMessage = @"Importer Security Filing Message Result<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Of Lading</th></tr></thead><tr><td>HB12345678</td></tr><tr><td>HB87654321</td></tr></table><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>User-defined Reference Number</th></tr></thead><tr><td>USERDEFINED1234</td></tr><tr><td>USERDEFINED1235</td></tr></table><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Narrative Message Code Identifier</th><th>Narrative Message Text</th></tr></thead><tr><td>308</td><td>CONSOLIDATOR NAME/ADDRESS REQUIRED (An Add/Replace transaction is submitted with Submission Type 1, Action Reason Code other than FT or FX, and no Consolidators)</td></tr><tr><td>303</td><td>CONSIGNEE NUMBER REQUIRED (An Add/Replace transaction is submitted with Submission Type 1, and no Consignee Number)</td></tr><tr><td>252</td><td>INVALID EQUIPMENT DESCRIPTION CODE (A transaction is submitted with an SF25 equipment record and invalid Equipment Description Code)</td></tr><tr><td>315</td><td>INVALID ENTITY IDENTIFIER (A transaction is submitted with an IOR/Consignee Entity Code and a Entity Identifier Qualifier, but the corresponding Entity Identifier is missing)</td></tr><tr><td>315</td><td>INVALID ENTITY IDENTIFIER (A transaction is submitted with an IOR/Consignee Entity Code and a Entity Identifier Qualifier, but the corresponding Entity Identifier is missing)</td></tr><tr><td>404</td><td>INVALID HTS CODE (A transaction is submitted with an HTS Code that is not on file)</td></tr><tr><td>404</td><td>INVALID HTS CODE (A transaction is submitted with an HTS Code that is not on file)</td></tr><tr><td>&nbsp;</td><td>SECURITY FILING REJECTED</td></tr></table>";
			AssertMultilineASCIIEquals("HtmlData", expectedBodyMessage, parser.HtmlData);
			expectedBodyMessage = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>User-defined Reference Number</th></tr></thead><tr><td>USERDEFINED1234</td></tr><tr><td>USERDEFINED1235</td></tr></table><br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Narrative Message Code Identifier</th><th>Narrative Message Text</th></tr></thead><tr><td>308</td><td>CONSOLIDATOR NAME/ADDRESS REQUIRED (An Add/Replace transaction is submitted with Submission Type 1, Action Reason Code other than FT or FX, and no Consolidators)</td></tr><tr><td>303</td><td>CONSIGNEE NUMBER REQUIRED (An Add/Replace transaction is submitted with Submission Type 1, and no Consignee Number)</td></tr><tr><td>252</td><td>INVALID EQUIPMENT DESCRIPTION CODE (A transaction is submitted with an SF25 equipment record and invalid Equipment Description Code)</td></tr><tr><td>315</td><td>INVALID ENTITY IDENTIFIER (A transaction is submitted with an IOR/Consignee Entity Code and a Entity Identifier Qualifier, but the corresponding Entity Identifier is missing)</td></tr><tr><td>315</td><td>INVALID ENTITY IDENTIFIER (A transaction is submitted with an IOR/Consignee Entity Code and a Entity Identifier Qualifier, but the corresponding Entity Identifier is missing)</td></tr><tr><td>404</td><td>INVALID HTS CODE (A transaction is submitted with an HTS Code that is not on file)</td></tr><tr><td>404</td><td>INVALID HTS CODE (A transaction is submitted with an HTS Code that is not on file)</td></tr><tr><td>&nbsp;</td><td>SECURITY FILING REJECTED</td></tr></table>";
			AssertMultilineASCIIEquals("HtmlMessageDataOnly", expectedBodyMessage, parser.HtmlMessageDataOnly);
			AssertEquals("JobReference", "ISFHELLO123", parser.JobReference);
			AssertEquals("HtmlData", ABIResponseStatus.Rejected, parser.Status);
			mock.VerifyAll();
			shipmentID1Mock.VerifyAll();
			shipmentID2Mock.VerifyAll();
		}
	}
}
