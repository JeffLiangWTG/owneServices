using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC060MessageProcessor))]
sealed class CC060MessageProcessorTest : BaseNctsMessageProcessorTestCase<CC060MessageProcessor, IIE060>
{
	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE060;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC060CMessageInterpreter);

	public void TestProcessMessage()
	{
		const string expectedEmailAddress = "abc@abc.com";
		const string staffCode = "ABC";
		var user = Factory.New<GlbStaff>();
		user.GS_Code = staffCode;
		user.GS_EmailAddress = expectedEmailAddress;

		const string messageCode = MessageNameList.Codes.IE060;
		const string transmitMessageNum = $"24{messageCode}TST_MSG_NUM";
		var fullCorrelationIdentifier = transmitMessageNum;

		const string testMrn = "TST_MRN";
		const string testLrn = "TST_LRN";
		const string testNotificationDateTime = "20/01/2002 15:30:00";
		dataProviderMock.Setup(x => x.MRN).Returns(testMrn);
		dataProviderMock.Setup(x => x.LRN).Returns(testLrn);
		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(fullCorrelationIdentifier);
		mockTransitOperation.Setup(x => x.ControlNotificationDateAndTime).Returns(testNotificationDateTime);

		const string testOfficeCode = $"{CountryCodes.Poland}2233";
		const string testOfficeDescription = "Test Office Of Departure";
		const string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, $"Test code type {codeType}", CountryCodes.Poland);
		helper.CreateCusCodeList(CountryCodes.Poland, codeType, testOfficeCode, testOfficeDescription, new DateTime(1900, 1, 1), new DateTime(2079, 6, 6));
		Factory.Save();
		dataProviderMock.Setup(x => x.CustomsOfficeOfDeparture).Returns(testOfficeCode);

		CombineAssertions(() =>
		{
			AssertProcessMessage(NCTS5NotificationTypes.Codes.DecisionToControlAndRequestedDocumentsIfNeeded,
				NCTS5DepartureCustomsStatusList.Codes.DecisionToControl,
				NCTS5NotificationTypes.Descriptions.DecisionToControlAndRequestedDocumentsIfNeeded);
			AssertProcessMessage(NCTS5NotificationTypes.Codes.AdditionalDocumentsRequest,
				NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest,
				NCTS5NotificationTypes.Descriptions.AdditionalDocumentsRequest);
			AssertProcessMessage(NCTS5NotificationTypes.Codes.IntentionToControl,
				NCTS5DepartureCustomsStatusList.Codes.IntentionToControl,
				NCTS5NotificationTypes.Descriptions.IntentionToControl);
		});

		void AssertProcessMessage(string notificationType, string expectedCustomsStatus, string expectedServiceDescription)
		{
			mockTransitOperation.Setup(x => x.NotificationType).Returns(notificationType);

			const string testJobReference = "TEST_JOB_REF";
			var testNctsHeader = Factory.New<NctsHeader>();
			testNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			testNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			testNctsHeader.BH_JobReference = testJobReference;

			var message = Factory.New<EDIMessage>();
			message.EM_LinkedObject = testNctsHeader.MovementHeader;
			message.EM_Status = EDIMessage.Status.PreProcessedOK;

			var transmittedMessage = Factory.New<EDIMessage>();
			transmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.PLCustomsNCTS;
			transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			transmittedMessage.EM_MessageNum = transmitMessageNum;
			transmittedMessage.EM_SystemCreateUser = staffCode;
			transmittedMessage.EM_LinkedObject = testNctsHeader.MovementHeader;
			transmittedMessage.EM_Status = EDIMessage.Status.Sent;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			MessageProcessor.ProcessMessage(message);

			var movementHeader = testNctsHeader.MovementHeader;
			AssertEquals("Linked object Bm_MessageStatus", "ACC", testNctsHeader.MovementHeader.BM_MessageStatus);
			AssertEquals("Message EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
			AssertEquals($"Movement header BM_CustomsStatus for NotificationType {notificationType}", expectedCustomsStatus, movementHeader.BM_CustomsStatus);

			var expectedServiceNote = $"{notificationType} : {expectedServiceDescription}";
			var expectedServiceBooked = new ZDateTime(testNotificationDateTime);
			const string expectedServiceSubLocation = $"{testOfficeCode} - {testOfficeDescription}";
			var jobService = movementHeader.Header.Services.Single() as JobService;
			AssertEquals("Service ES_ServiceCode", "CTL", jobService.ES_ServiceCode);
			AssertEquals("Service ES_ServiceNote", expectedServiceNote, jobService.ES_ServiceNote);
			AssertEquals("Service ES_Booked", expectedServiceBooked, jobService.ES_Booked);
			AssertEquals("Service ES_References", testMrn, jobService.ES_References);
			AssertEquals("Service ES_SubLocation", expectedServiceSubLocation, jobService.ES_SubLocation.ToString());

			var expectedSubject = $"IE060_Control_Decision_({testLrn}) Response for {testJobReference}";
			var headerUri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(testNctsHeader);
			var expectedJobHRef = $"Response for <a href=\"{headerUri}\">{testJobReference}</a>";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			AssertEquals("Email address", expectedEmailAddress, email.Recipients.Cast<RecipientDef>().Single().Email);
			AssertEquals("Email subject", expectedSubject, email.Subject);
			AssertContains("Email body", message.EM_MessageInterpretation, email.Body);
			AssertContains("Job reference", expectedJobHRef, email.Body);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		mockTransitOperation = new Mock<ICC060CTransitOperation>();
		dataProviderMock.Setup(x => x.MessageType).Returns("CC060C");
		dataProviderMock.Setup(x => x.TransitOperation).Returns(mockTransitOperation.Object);
	}

	Mock<ICC060CTransitOperation> mockTransitOperation;
}
