using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class PLInterchangeProviderLegacyTest : InterchangeProviderTestCase
{
	const string TestUser = "TestUser";
	const string TestPassword = "TestPassword";

	public void TestConstructorValidation()
	{
		var messageCollection = new NonDependentEDIMessageCollection(Factory);

		AssertExceptionThrown(
			"Value cannot be null",
			typeof(ArgumentNullException),
			"Value cannot be null.\r\nParameter name: logging",
			() => new PLInterchangeProviderLegacy(messageCollection, logging: null));
	}

	public void TestHeaderHasUserCredentials_EmptyCredentials()
	{
		var linkedObject = Factory.NewWithValidTestData<JobDeclaration>();
		var password = Factory.NewWithValidTestData<GlbExternalPassword>();
		password.GP_MailBoxID = string.Empty;
		password.CurrentDecryptedPassword = string.Empty;

		var message = Factory.CreateCoreMessage(messageType: EUJobMessageTypeList.Codes.Import, linkedObject: linkedObject, password: password);

		var messageCollection = new NonDependentEDIMessageCollection(Factory)
		{
			message
		};

		var logger = new LoggingInformation();

		var interchangeProvider = new PLInterchangeProviderLegacy(messageCollection, logger);
		interchangeProvider.PackCollatedMessagesIntoInterchanges();

		Factory.Save();

		var interchange = interchangeProvider.Interchanges[0];

		var resultDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(interchange.EI_HeaderText);
		var user = resultDictionary[InterchangeHeaderAttributes.User];
		var resultPassword = resultDictionary[InterchangeHeaderAttributes.EncryptedPassword];
		CombineAssertions(() =>
		{
			AssertEquals(InterchangeHeaderAttributes.User, string.Empty, user);
			AssertEquals(InterchangeHeaderAttributes.EncryptedPassword, string.Empty, resultPassword);
		});
	}

	public void TestHeaderHasUserCredentials()
	{
		var message = Factory.CreateCoreMessage();
		var interchange = CreateInterchange(message);

		var resultDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(interchange.EI_HeaderText);
		var user = resultDictionary[InterchangeHeaderAttributes.User];
		var resultPassword = resultDictionary[InterchangeHeaderAttributes.EncryptedPassword];

		CombineAssertions(() =>
		{
			AssertEquals(InterchangeHeaderAttributes.User, TestUser, user);
			AssertNotEquals(InterchangeHeaderAttributes.EncryptedPassword, TestPassword, resultPassword);
		});
	}

	public void TestMessageBodyWrapsMessageText()
	{
		var linkedObject = Factory.NewWithValidTestData<JobDeclaration>();
		var message = Factory.CreateCoreMessage("Test Message", linkedObject: linkedObject);
		message.EM_MessageType = "TST";

		var messageCollection = new NonDependentEDIMessageCollection(Factory)
		{
			message
		};
		var logger = new LoggingInformation();
		var interchangeProvider = new PLInterchangeProviderLegacy(messageCollection, logger);
		interchangeProvider.PackCollatedMessagesIntoInterchanges();

		var interchange = interchangeProvider.Interchanges[0];
		var envelope = XDocument.Parse(interchange.EI_BodyText);
		CombineAssertions(() =>
		{
			XNamespace messageIdNamespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing";
			var messageIdElement = envelope.Descendants(messageIdNamespace + "MessageID").First();
			var expectedMessageId = "urn:uuid:" + interchange.EI_SessionGUID;
			AssertEquals("messageIdElement value", expectedMessageId, messageIdElement.Value);
		});
	}

	public void TestMessageBodyWrapsMessageText_AcceptDocumentRequest()
	{
		var linkedObject = Factory.NewWithValidTestData<JobDeclaration>();
		var message = Factory.CreateCoreMessage("Test Message", linkedObject: linkedObject);
		message.EM_MessageType = "TST";
		message.EM_MessageNum = "TSTMessage01";

		var messageCollection = new NonDependentEDIMessageCollection(Factory)
		{
			message
		};
		var logger = new LoggingInformation();
		var interchangeProvider = new PLInterchangeProviderLegacy(messageCollection, logger);
		interchangeProvider.PackCollatedMessagesIntoInterchanges();

		var interchange = interchangeProvider.Interchanges[0];
		var envelope = XDocument.Parse(interchange.EI_BodyText);
		CombineAssertions(() =>
		{
			const string expectedMethodName = "AcceptDocument";
			XNamespace actionNamespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing";
			var actionElement = envelope.Descendants(actionNamespace + "Action").First();
			AssertEquals("Action value", expectedMethodName, actionElement.Value);

			XNamespace contentNamespace = "http://www.mf.gov.pl/schematy/SISC/WsChannel/2014/01_v2_0";
			var contentElement = envelope.Descendants(contentNamespace + "content").First();

			var expectedFileName = message.EM_MessageNum;
			AssertEquals("Content filename", expectedFileName, contentElement.Attribute("filename").Value);

			var expectedContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(message.EM_MessageText));
			AssertEquals("Content value", expectedContent, contentElement.Value);
		});
	}

	public void TestMessageBodyWrapsMessageText_GetDocumentsRequest()
	{
		const string messageText = "<TestRequest>Test Message</TestRequest>";
		var linkedObject = Factory.NewWithValidTestData<JobDeclaration>();
		var message = Factory.CreateCoreMessage(messageText, linkedObject: linkedObject);
		message.EM_MessageType = "CPT";
		message.EM_MessageSubType = "CPT";

		var messageCollection = new NonDependentEDIMessageCollection(Factory)
		{
			message
		};
		var logger = new LoggingInformation();
		var interchangeProvider = new PLInterchangeProviderLegacy(messageCollection, logger);
		interchangeProvider.PackCollatedMessagesIntoInterchanges();

		var interchange = interchangeProvider.Interchanges[0];
		var envelope = XDocument.Parse(interchange.EI_BodyText);
		CombineAssertions(() =>
		{
			const string expectedMethodName = "GetDocuments";
			XNamespace actionNamespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing";
			var actionElement = envelope.Descendants(actionNamespace + "Action").First();
			AssertEquals("Action value", expectedMethodName, actionElement.Value);

			XNamespace bodyNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
			var bodyElement = envelope.Descendants(bodyNamespace + "Body").First();
			var requestNode = bodyElement.FirstNode;
			AssertEquals("Body content", messageText, requestNode.ToString());
		});
	}

	public override void TestMessagesPopulateNewInterchange()
	{
		var importMessage = Factory.CreateCoreMessage(messageType: EUJobMessageTypeList.Codes.Import);
		var exportMessage = Factory.CreateCoreMessage(messageType: EUJobMessageTypeList.Codes.Export);

		var interchanges = CreateInterchanges(importMessage, exportMessage);
		var importInterchange = interchanges.Single(x => x.PK == importMessage.EM_EI);
		var exportInterchange = interchanges.Single(x => x.PK == exportMessage.EM_EI);

		CombineAssertions(() =>
		{
			AssertNotEquals("Confirmed different interchanges for each message no collation", importMessage.EM_EI, exportMessage.EM_EI);

			AssertInterchange("import interchange", importInterchange, importMessage);
			AssertInterchange("export interchange", exportInterchange, exportMessage);
			AssertNotEquals("Unique InterchangeNum", importInterchange.EI_InterchangeNum, exportInterchange.EI_InterchangeNum);
		});
	}

	public void TestMessagePopulateInterchange_eZalaczniki()
	{
		const string fromMailAddress = "from@test.pl";
		PLCustomsDataRegistry.Instance.CommunicationEmailChannelEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, fromMailAddress);
		var eZalacznikiMessage = Factory.CreateCoreMessage(applicationCode: ApplicationCodeList.Codes.PLCustomsPUESCEmailSystem,
			messageType: EdiMessageMessageType.Attachment);

		var interchanges = CreateInterchanges(eZalacznikiMessage);
		var eZalacznikiInterchange = interchanges.Single(x => x.PK == eZalacznikiMessage.EM_EI);
		var resultDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(eZalacznikiInterchange.EI_HeaderText);
		var fromMailBox = resultDictionary[InterchangeHeaderAttributes.FromMailBox];
		var destinationMailBox = resultDictionary[InterchangeHeaderAttributes.DestinationMailBox];

		CombineAssertions(() =>
		{
			AssertInterchange("eZalaczniki interchange", eZalacznikiInterchange, eZalacznikiMessage);
			AssertEquals("From Mail Address", fromMailBox, fromMailAddress);
			AssertEquals("Destination Mail Address", destinationMailBox, "test.puesc@mf.gov.pl");
		});
	}

	public void TestMessagesPopulate_InterchangeNumBranchIndependentStrategy()
	{
		var (_, branches) = Factory.NewCompanyWithBranches(CountryCodes.Poland,
			new[] { CountryCodes.Italy, CountryCodes.Poland, CountryCodes.Poland, CountryCodes.UnitedKingdom });
		Factory.Save();

		CombineAssertions(() =>
		{
			long interchangeNum = 1, prevInterchangeNum = 0;
			foreach (var branch in branches)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					CertificateHelper.SetUpTestCertificate();

					var interchange = CreateInterchange(Factory.CreateCoreMessage());
					AssertNoExceptionThrown($"Get branch {branch.BaseCountry.Code} InterchangeNum",
						() => interchangeNum = long.Parse(interchange.EI_InterchangeNum));
					if (prevInterchangeNum != 0)
					{
						AssertEquals(prevInterchangeNum + 1, interchangeNum);
					}
					prevInterchangeNum = interchangeNum;
				}
			}
		});
	}

	public void TestMessagesPopulateNewInterchangeWithoutLinkedObject()
	{
		var messageText = "<test>Test Message</test>";
		BusinessObject linkedObject = null;
		var errorMessage = "Message's Linked Object is null so can't continue with processing. The message's status has been set to 'Failed'.";
		AssertPopulateNewInterchangeRaisesError(messageText, linkedObject, errorMessage);
	}

	public void TestMessagesPopulateNewInterchangeWithoutMessageText()
	{
		var messageText = ZString.Empty;
		var linkedObject = Factory.NewWithValidTestData<JobDeclaration>();
		var errorMessage = "Message's Message Text is empty so can't continue with processing. The message's status has been set to 'Failed'.";
		AssertPopulateNewInterchangeRaisesError(messageText, linkedObject, errorMessage);
	}

	protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
	{
		var logger = new LoggingInformation();
		return new PLInterchangeProviderLegacy(collection, logger);
	}

	void AssertPopulateNewInterchangeRaisesError(ZString messageText, BusinessObject linkedObject, ZString errorMessage)
	{
		var message = Factory.CreateCoreMessage(messageText, messageType: EUJobMessageTypeList.Codes.Import, linkedObject: linkedObject);

		var messageCollection = new NonDependentEDIMessageCollection(Factory);
		messageCollection.Add(message);

		var logger = new LoggingInformation();

		var interchangeProvider = new PLInterchangeProviderLegacy(messageCollection, logger);
		interchangeProvider.PackCollatedMessagesIntoInterchanges();

		Factory.Save();

		message.Reload();

		var interchanges = interchangeProvider.Interchanges;

		CombineAssertions(() =>
		{
			AssertEquals("No interchanges created", 0, interchanges.Length);
			AssertEquals("message Status is Failed", EDIMessage.Status.Failed, message.EM_Status);
			AssertNull("message Interchange is not linked", message.Interchange);

			var note = message.Notes.FindByDescription("Packing Log").Single();
			AssertContains("ST_NoteText contains error message", errorMessage, note.ST_NoteText);
		});
	}

	static void AssertInterchange(string itemNo, EDIInterchange interchange, BaseEDIMessage message)
	{
		AssertEquals($"{itemNo} message is sent", EDIMessage.Status.Sent, message.EM_Status);
		AssertEquals($"{itemNo} {nameof(interchange.EI_ApplicationCode)}", message.EM_ApplicationCode, interchange.EI_ApplicationCode);
		AssertEquals($"{itemNo} {nameof(interchange.EI_InterchangeType)}", message.EM_MessageType, interchange.EI_InterchangeType);
		AssertEquals($"{itemNo} {nameof(interchange.EI_ReceiveTransmit)}", EDIMessage.Direction.Transmit, interchange.EI_ReceiveTransmit);
		AssertEquals($"{itemNo} {nameof(interchange.EI_From)}", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
		AssertEquals($"{itemNo} {nameof(interchange.EI_To)}", CustomsDestinationCodes.PlCustomsTest, interchange.EI_To);
		AssertEquals($"{itemNo} {nameof(interchange.EI_TransportType)}", EDIInterchange.TransportType.xT, interchange.EI_TransportType);
		AssertEquals($"{itemNo} {nameof(interchange.EI_Status)}", EDIInterchange.Status.Queued, interchange.EI_Status);
		AssertEquals($"{itemNo} {nameof(interchange.EI_Status)}", "HGH", interchange.EI_Priority);
		AssertEquals($"{itemNo} {nameof(interchange.EI_IsActive)}", true, interchange.EI_IsActive);
		AssertEquals($"{itemNo} {nameof(interchange.EI_GP)}", message.EM_GP, interchange.EI_GP);
		Assert($"{itemNo} {nameof(interchange.EI_InterchangeNum)}", !interchange.EI_InterchangeNum.IsEmpty);
		Assert($"{itemNo} {nameof(interchange.EI_SessionGUID)}", !interchange.EI_SessionGUID.IsEmpty);
	}

	GlbExternalPassword CreatePassword()
	{
		var password = Factory.NewWithValidTestData<GlbExternalPassword>();
		password.GP_MailBoxID = TestUser;
		password.CurrentDecryptedPassword = TestPassword;
		return password;
	}

	EDIInterchange CreateInterchange(BaseEDIMessage message) => CreateInterchanges(message).Single();

	EDIInterchange[] CreateInterchanges(params BaseEDIMessage[] messages)
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var password = CreatePassword();

		var messageCollection = new NonDependentEDIMessageCollection(Factory);
		foreach (var message in messages)
		{
			message.EM_LinkedObject = declaration;
			message.EM_GP = password.PK;
			messageCollection.Add(message);
		}

		var interchangeProvider = new PLInterchangeProviderLegacy(messageCollection, new LoggingInformation());
		interchangeProvider.PackCollatedMessagesIntoInterchanges();

		Factory.Save();

		var interchanges = interchangeProvider.Interchanges;
		AssertEquals("Number Of Interchanges", messages.Length, interchanges.Length);

		var num = 1;
		foreach (var message in messages)
		{
			var interchange = interchanges.FirstOrDefault(x => x.PK == message.EM_EI);
			AssertNotNull($"Interchange for message number {num}", interchange);
			AssertInterchange(num.ToString(), interchange, message);
			num++;
		}
		return interchangeProvider.Interchanges;
	}

	protected override void SetUp()
	{
		base.SetUp();
		CertificateHelper.SetUpTestCertificate();
	}
}
