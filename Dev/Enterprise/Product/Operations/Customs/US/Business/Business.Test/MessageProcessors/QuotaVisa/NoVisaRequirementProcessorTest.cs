using System;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class NoVisaRequirementProcessorTest : ABIProcessorTest<NoVisaRequirementProcessor, APLA, APLB, APLY>
	{
		protected override void EndToEndCore()
		{
			incomingMessage.EM_MessageText = "B018888XJ5UR                                               ~150000              U9                    UMCNO VISA RECORDS FOR COUNTRY                            Y  8888XJ5UR00001";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new USRIncomingMessageProcessor().ExecuteBatch();
			incomingMessage.Reload();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Query for Visa Requirement"); }));

			AssertNotNull("An email with subject 'Query for Visa Requirement' should have been sent.", email);
			AssertEquals(declaration, incomingMessage.EM_LinkedObject);
			AssertContains(new ZDateTime(2008, 1, 1).ToString(), email.Body);
		}

		public void TestProcessWithNullLinkedObject()
		{
			outgoing.EM_LinkedObject = null;
			incomingMessage.EM_MessageText = "B018888XJ5UR                                               ~150000              U9                    UMCNO VISA RECORDS FOR COUNTRY                            Y  8888XJ5UR00001";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new USRIncomingMessageProcessor().ExecuteBatch();
			incomingMessage.Reload();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject == "Query for Visa Requirement Response for ~150000"; }));

			AssertNotNull("An email with subject 'Query for Visa Requirement' should have been sent.", email);
			AssertNull(incomingMessage.EM_LinkedObject);
			AssertContains(new ZDateTime(2008, 1, 1).ToString(), email.Body);
		}

		public void TestProcessWithoutOriginalMsg()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = "QUE";
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryQuotaResponse;
			message.EM_MessageNum = "~150000";
			message.EM_MessageText = "B018888XJ5UR                                               ~150000              U9                    UMCNO VISA RECORDS FOR COUNTRY                            Y  8888XJ5UR00001";
			Factory.Save();

			new USRIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals("Should be processed successfully", "RCV", message.EM_Status);
		}

		public void TestAllDetailsIncludedInEmail()
		{
			incomingMessage.EM_MessageText = "B018888XJ5UR                                               ~150000              U9                    UMCADDITIONAL VISA DETAILS REQUIRED FOR SOME CASES        Y  8888XJ5UR00001";

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var outgoing2 = mock.Object;
			outgoing2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoing2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing2.EM_Status = "SNT";
			outgoing2.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryQuota;
			outgoing2.EM_MessageNum = "JASATLHST_71123";
			outgoing2.EM_SystemCreateTimeUtc = new ZDateTime(2008, 1, 1);

			var incomingMessage2 = Factory.New<MQEDIMessage>();
			incomingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage2.EM_Status = "QUE";
			incomingMessage2.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryQuotaResponse;
			incomingMessage2.EM_MessageNum = "JASATLHST_71123";
			incomingMessage2.EM_SystemCreateTimeUtc = new ZDateTime(2008, 1, 2);
			incomingMessage2.EM_MessageText = "B011601D99UR                                               JASATLHST_71123      U963025140006302512000CNVTARIFF (2) INVALID                                     Y  1601D99UR00001";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new USRIncomingMessageProcessor().ExecuteBatch();
			incomingMessage.Reload();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject == "Query for Visa Requirement Response for B00001000"; }));

			AssertNotNull("An email with subject 'Query for Visa Requirement' should have been sent.", email);
			AssertContains("ADDITIONAL VISA DETAILS REQUIRED FOR SOM", email.Body);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject == "Query for Visa Requirement Response for JASATLHST_71123"; }));
			AssertNotNull("An email with subject 'Query for Visa Requirement' should have been sent.", email);
			AssertContains("TARIFF (2) INVALID", email.Body);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			outgoing = mock.Object;
			outgoing.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_Status = "SNT";
			outgoing.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryQuota;
			outgoing.EM_MessageNum = "~150000";
			declaration.Messages.Add(outgoing);
			outgoing.EM_SystemCreateTimeUtc = new ZDateTime(2008, 1, 1);

			incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryQuotaResponse;
			incomingMessage.EM_MessageNum = "~150000";
			incomingMessage.EM_SystemCreateTimeUtc = new ZDateTime(2008, 1, 2);
		}

		MQEDIMessage incomingMessage;
		MQEDIMessage outgoing;
		JobDeclaration declaration;
	}
}
