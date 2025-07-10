using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	sealed class UYManifestMessageProcessorTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessSuccessOriginalMessage()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "8AZ8493";
			bill.ABL_BillStatus = "SNT";
			bill.ABL_MessageStatus = "AWA";

			var requestInterchange = CreateInterchange(EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received);
			var requestMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyTextWithOutSign), requestInterchange.PK, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var responseInterchange = CreateInterchange(EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var responseMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyTextWithOutSign), responseInterchange.PK, header.PK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			processor.ExecuteBatch();
			responseMessage.Reload();
			requestMessage.Reload();
			header.Reload();
			bill.Reload();
			bill.CustomsEntryNumbers.Reload(true);

			var cusEntryNumCreated = Factory.Load<CusEntryNumber>(new ZQuery());
			var cusEntryNum = cusEntryNumCreated.Where(x => x.CE_ParentID == bill.PK).FirstOrDefault();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			var expectedresult = @"Result      : Operacion Exitosa
Description : El alta del conocimiento 8AZ8493 fue exitosa.

References

DNANumber   : 7
Sequence    : 1
HBLNumber   : 8AZ8493";

			CombineAssertions(() =>
			{
				AssertEquals(header.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals(AsycudaManifestHeaderSchema.Constants.TableName, responseMessage.EM_LinkTable);

				AssertEquals("7", bill.CustomsEntryNumber);
				AssertEquals("DNA", bill.CustomsEntryNumberType);
				AssertEquals(MessageStatusCodeList.Codes.Accepted, bill.ABL_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ACP, bill.ABL_BillStatus);

				AssertEquals(MessageStatusCodeList.Codes.Accepted, bill.Header.AMA_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ACP, bill.Header.RegistrationStatus);

				AssertEquals(bill.PK, cusEntryNum.CE_ParentID);
				AssertEquals("AsycudaBill", cusEntryNum.CE_ParentTable);
				AssertEquals(bill.CustomsEntryNumber, cusEntryNum.CE_EntryNum);
				AssertEquals(bill.CustomsEntryNumberType, cusEntryNum.CE_EntryType);

				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job MAN0000001 has been cleared. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertContains(expectedresult, responseMessage.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessMessageHeaderNoFound()
		{
			var header = CreateHeader();

			var requestInterchange = CreateInterchange(EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received);
			var requestMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyTextWithOutSign), requestInterchange.PK, ZGuid.Empty, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var responseInterchange = CreateInterchange(EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var responseMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyTextWithOutSign), responseInterchange.PK, ZGuid.Empty, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			processor.ExecuteBatch();
			responseMessage.Reload();

			AssertEquals(EDIMessage.Status.Failed, responseMessage.EM_Status);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessMessageBillNoFound()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "NOBILL";
			bill.ABL_BillStatus = "SNT";
			bill.ABL_MessageStatus = "AWA";

			var bodyTextCredentialsError = Path.Combine(BaseSourcePath, DAETestingConstants.BodyTextCredentialsError);
			var requestInterchange = CreateInterchange(EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received);
			var requestMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyTextCredentialsError), requestInterchange.PK, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var responseInterchange = CreateInterchange(EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var responseMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyTextCredentialsError), responseInterchange.PK, header.PK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			processor.ExecuteBatch();
			responseMessage.Reload();
			header.Reload();
			bill.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(header.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals(AsycudaManifestHeaderSchema.Constants.TableName, responseMessage.EM_LinkTable);

				AssertEquals(EDIMessage.Status.ProcessedOK, responseMessage.EM_Status);

				AssertEquals("The Bill Status should be updated. Then, the user can Send/Resend the Manifest", "ERR", bill.ABL_BillStatus);
				AssertEquals("The Bill Customs Status should be updated. Then, the user can Send/Resend the Manifest", "ERR", bill.ABL_MessageStatus);

				AssertEquals("The Manifest Header Status should be updated. Then, the user can Send/Resend the Manifest", "ERR", header.AMA_MessageStatus);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessUnsuccessOriginalMessage()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "7LNY798";
			bill.ABL_BillStatus = "SNT";
			bill.ABL_MessageStatus = "AWA";

			var requestInterchange = CreateInterchange(EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received);
			var requestMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyTextError), requestInterchange.PK, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var responseInterchange = CreateInterchange(EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var responseMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyTextError), responseInterchange.PK, header.PK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			processor.ExecuteBatch();
			responseMessage.Reload();
			bill.Reload();
			header.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response (Failure) for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			var expectedresult = @"Result      : Ya existe el Conocimiento Original para el manifiesto o lo está enviando más de una vez en el mensaje.
Description : Conocimiento Orig. : 7LNY798 Nro. de Manifiesto DNA: LH8264

References

Sequence    : 1
HBLNumber   : 7LNY798";

			CombineAssertions(() =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, responseMessage.EM_Status);

				AssertEquals(CustomsStatusList.Codes.ERR, bill.ABL_BillStatus);
				AssertEquals(MessageStatusCodeList.Codes.Error, bill.ABL_MessageStatus);

				AssertEquals(MessageStatusCodeList.Codes.Error, header.AMA_MessageStatus);

				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job MAN0000001 has been rejected. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertContains(expectedresult, responseMessage.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCancellationSuccessful()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "8AZ8493";
			bill.ABL_BillStatus = "ACP";
			bill.ABL_MessageStatus = "AWA";
			bill.CustomsEntryNumber = "13";

			var requestInterchange = CreateInterchange(EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received);
			var requestMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyCancellation), requestInterchange.PK, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var responseInterchange = CreateInterchange(EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var responseMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyCancellation), responseInterchange.PK, header.PK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			processor.ExecuteBatch();
			responseMessage.Reload();
			header.Reload();
			bill.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			var expectedresult = @"Result      : Operacion Exitosa
Description : Baja de Conocimiento exitoso.

References

Sequence    : 1";

			CombineAssertions(() =>
			{
				AssertEquals(responseMessage.EM_LinkUniqueID, header.PK);
				AssertEquals(responseMessage.EM_LinkTable, AsycudaManifestHeaderSchema.Constants.TableName);

				AssertEquals(MessageStatusCodeList.Codes.Cancel, bill.ABL_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.CAN, bill.ABL_BillStatus);

				AssertEquals(MessageStatusCodeList.Codes.Cancel, bill.Header.AMA_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.CAN, bill.Header.RegistrationStatus);

				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job MAN0000001 has been cleared. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertContains(expectedresult, responseMessage.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCancellationSuccessfulButOneBillIsStillAccepted()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "8AZ8493";
			bill.ABL_BillStatus = "ACP";
			bill.ABL_MessageStatus = "AWA";
			bill.CustomsEntryNumber = "13";

			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "NEWBILL";
			bill2.ABL_BillStatus = "ACP";
			bill2.ABL_MessageStatus = "ACP";

			var requestInterchange = CreateInterchange(EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received);
			var requestMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyCancellation), requestInterchange.PK, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var responseInterchange = CreateInterchange(EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var responseMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyCancellation), responseInterchange.PK, header.PK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			processor.ExecuteBatch();
			responseMessage.Reload();
			header.Reload();
			bill.Reload();
			bill2.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			var expectedresult = @"Result      : Operacion Exitosa
Description : Baja de Conocimiento exitoso.

References

Sequence    : 1";

			CombineAssertions(() =>
			{
				AssertEquals(responseMessage.EM_LinkUniqueID, header.PK);
				AssertEquals(responseMessage.EM_LinkTable, AsycudaManifestHeaderSchema.Constants.TableName);

				AssertEquals(MessageStatusCodeList.Codes.Cancel, bill.ABL_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.CAN, bill.ABL_BillStatus);

				AssertEquals(MessageStatusCodeList.Codes.Accepted, bill2.ABL_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ACP, bill2.ABL_BillStatus);

				AssertEquals(MessageStatusCodeList.Codes.Accepted, bill.Header.AMA_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ACP, bill.Header.RegistrationStatus);

				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job MAN0000001 has been cleared. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertContains(expectedresult, responseMessage.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessAmendmentSuccessful()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "8AZ8493";
			bill.ABL_BillStatus = "ACP";
			bill.ABL_MessageStatus = "AWA";
			bill.CustomsEntryNumber = "15";

			var requestInterchange = CreateInterchange(EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received);
			var requestMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyAmendment), requestInterchange.PK, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var responseInterchange = CreateInterchange(EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var responseMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyAmendment), responseInterchange.PK, header.PK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			processor.ExecuteBatch();
			responseMessage.Reload();
			header.Reload();
			bill.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			var expectedresult = @"Result      : Operacion Exitosa
Description : Modificacion de Conocimiento exitosa.

References

Sequence    : 1";

			CombineAssertions(() =>
			{
				AssertEquals(responseMessage.EM_LinkUniqueID, header.PK);
				AssertEquals(responseMessage.EM_LinkTable, AsycudaManifestHeaderSchema.Constants.TableName);

				AssertEquals(MessageStatusCodeList.Codes.Accepted, bill.ABL_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ACP, bill.ABL_BillStatus);

				AssertEquals(MessageStatusCodeList.Codes.Accepted, bill.Header.AMA_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ACP, bill.Header.RegistrationStatus);

				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job MAN0000001 has been cleared. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertContains(expectedresult, responseMessage.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessUnsuccessResendMessage()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "7LNY798";
			bill.ABL_BillStatus = "ACP";
			bill.ABL_MessageStatus = "AWA";

			var requestInterchange = CreateInterchange(EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received);
			var requestMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyTextError), requestInterchange.PK, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var responseInterchange = CreateInterchange(EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var responseMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyTextError), responseInterchange.PK, header.PK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			processor.ExecuteBatch();
			responseMessage.Reload();
			bill.Reload();
			header.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response (Failure) for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			var expectedresult = @"Result      : Ya existe el Conocimiento Original para el manifiesto o lo está enviando más de una vez en el mensaje.
Description : Conocimiento Orig. : 7LNY798 Nro. de Manifiesto DNA: LH8264

References

Sequence    : 1
HBLNumber   : 7LNY798";

			CombineAssertions(() =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, responseMessage.EM_Status);

				AssertEquals(CustomsStatusList.Codes.ACP, bill.ABL_BillStatus);
				AssertEquals(MessageStatusCodeList.Codes.Error, bill.ABL_MessageStatus);

				AssertEquals(MessageStatusCodeList.Codes.Accepted, header.AMA_MessageStatus);

				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job MAN0000001 has been rejected. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertContains(expectedresult, responseMessage.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessOneBillAcceptedOneBillRejectedOriginalMessage()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			bill.ABL_BillNumber = "8AZ8493";
			bill.ABL_BillStatus = "SNT";
			bill.ABL_MessageStatus = "AWA";

			bill2.ABL_BillNumber = "7LNY798";
			bill2.ABL_BillStatus = "SNT";
			bill2.ABL_MessageStatus = "AWA";

			var bodyOneBillAcceptedOneBillRejected = Path.Combine(BaseSourcePath, DAETestingConstants.OneBillAcceptedOneBillRejected);
			var requestInterchange = CreateInterchange(EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received);
			var requestMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyOneBillAcceptedOneBillRejected), requestInterchange.PK, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var responseInterchange = CreateInterchange(EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var responseMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyOneBillAcceptedOneBillRejected), responseInterchange.PK, header.PK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			processor.ExecuteBatch();
			responseMessage.Reload();
			requestMessage.Reload();
			header.Reload();
			bill.Reload();
			bill.CustomsEntryNumbers.Reload(true);
			bill2.Reload();

			var cusEntryNumCreated = Factory.Load<CusEntryNumber>(new ZQuery());
			var cusEntryNum = cusEntryNumCreated.Where(x => x.CE_ParentID == bill.PK).FirstOrDefault();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response for " + header.AMA_JobReference);
			AssertNotNull(email);

			CombineAssertions(() =>
			{
				AssertEquals(header.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals(AsycudaManifestHeaderSchema.Constants.TableName, responseMessage.EM_LinkTable);

				AssertEquals("7", bill.CustomsEntryNumber);
				AssertEquals("DNA", bill.CustomsEntryNumberType);

				AssertEquals(MessageStatusCodeList.Codes.Accepted, bill.ABL_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ACP, bill.ABL_BillStatus);

				AssertEquals(MessageStatusCodeList.Codes.Error, bill2.ABL_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ERR, bill2.ABL_BillStatus);

				AssertEquals(MessageStatusCodeList.Codes.Accepted, bill.Header.AMA_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ACP, bill.Header.RegistrationStatus);

				AssertEquals(bill.PK, cusEntryNum.CE_ParentID);
				AssertEquals("AsycudaBill", cusEntryNum.CE_ParentTable);
				AssertEquals(bill.CustomsEntryNumber, cusEntryNum.CE_EntryNum);
				AssertEquals(bill.CustomsEntryNumberType, cusEntryNum.CE_EntryType);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessOneBillAcceptedOneBillRejectedResendMessage()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			bill.ABL_BillNumber = "8AZ8493";
			bill.ABL_BillStatus = "ACP";
			bill.ABL_MessageStatus = "AWA";
			bill.CustomsEntryNumber = "18";

			bill2.ABL_BillNumber = "7LNY798";
			bill2.ABL_BillStatus = "ACP";
			bill2.ABL_MessageStatus = "AWA";

			var bodyOneBillAcceptedOneBillRejectedResend = Path.Combine(BaseSourcePath, DAETestingConstants.OneBillAcceptedOneBillRejectedResend);
			var requestInterchange = CreateInterchange(EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received);
			var requestMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyOneBillAcceptedOneBillRejectedResend), requestInterchange.PK, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var responseInterchange = CreateInterchange(EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var responseMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyOneBillAcceptedOneBillRejectedResend), responseInterchange.PK, header.PK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			processor.ExecuteBatch();
			responseMessage.Reload();
			requestMessage.Reload();
			header.Reload();
			bill.Reload();
			bill2.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(header.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals(AsycudaManifestHeaderSchema.Constants.TableName, responseMessage.EM_LinkTable);

				AssertEquals(MessageStatusCodeList.Codes.Accepted, bill.ABL_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ACP, bill.ABL_BillStatus);

				AssertEquals(MessageStatusCodeList.Codes.Error, bill2.ABL_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ACP, bill2.ABL_BillStatus);

				AssertEquals(MessageStatusCodeList.Codes.Accepted, header.AMA_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ACP, bill.Header.RegistrationStatus);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessAmendPacks()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();

			bill.ABL_BillNumber = "TYO19633001";
			bill.ABL_BillStatus = "SNT";
			bill.ABL_MessageStatus = "AWA";

			var requestInterchange = CreateInterchange(EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received);
			var requestMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyAmendment), requestInterchange.PK, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var bodyAmendPacks = Path.Combine(BaseSourcePath, DAETestingConstants.BodyAmendPacks);
			var responseInterchange = CreateInterchange(EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var responseMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyAmendPacks), responseInterchange.PK, header.PK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			processor.ExecuteBatch();
			responseMessage.Reload();
			requestMessage.Reload();
			header.Reload();
			bill.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response for " + header.AMA_JobReference);
			AssertNotNull(email);

			var expectedresult = @"Result      : Operacion Exitosa
Description : Modificacion de la linea exitosa.

References

Sequence    : 11
HBLNumber   : TYO19633001";

			CombineAssertions(() =>
			{
				AssertEquals(header.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals(AsycudaManifestHeaderSchema.Constants.TableName, responseMessage.EM_LinkTable);

				AssertEquals(MessageStatusCodeList.Codes.Accepted, bill.ABL_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ACP, bill.ABL_BillStatus);

				AssertEquals(MessageStatusCodeList.Codes.Accepted, bill.Header.AMA_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ACP, bill.Header.RegistrationStatus);

				AssertContains(expectedresult, responseMessage.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLinkRequestMessageByTrackingId()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();

			bill.ABL_BillNumber = "TYO19633001";
			bill.ABL_BillStatus = "SNT";
			bill.ABL_MessageStatus = "AWA";

			var requestInterchange = CreateInterchange(EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received);
			var requestMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyTextWithOutSign), requestInterchange.PK, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var responseInterchange = CreateInterchange(EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var responseMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyTextWithOutSign), responseInterchange.PK, header.PK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			processor.ExecuteBatch();
			responseMessage.Reload();
			requestMessage.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(requestMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
				AssertEquals(requestMessage.EM_LinkTable, responseMessage.EM_LinkTable);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvalidCredentialsResult()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "8AZ8493";
			bill.ABL_BillStatus = "SNT";
			bill.ABL_MessageStatus = "AWA";

			var requestInterchange = CreateInterchange(EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received);
			var requestMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyTextWithOutSign), requestInterchange.PK, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var bodyWithWrongCredentials = Path.Combine(BaseSourcePath, DAETestingConstants.HeaderTextErrorNotification);
			var responseInterchange = CreateInterchange(EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var responseMessage = CreateMessage(DAETestingHelper.GetExpectedMessageTXT(bodyWithWrongCredentials), responseInterchange.PK, header.PK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			responseInterchange.EI_InterchangeType = MessageTypes.Codes.XER;
			responseMessage.EM_MessageType = MessageTypes.Codes.XER;
			Factory.Save();

			processor.ExecuteBatch();
			responseMessage.Reload();
			requestMessage.Reload();
			header.Reload();
			bill.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response (Failure) for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			var expectedresult = @"Result      : ERR
Description : Error Message
Contract: xt-contract:/Customs/UY/UY Message Contract
Reference Object: xt-httpclientaddress:{185867f5-862d-474a-a47d-60c1394d198d}
Reference Object: xt-node:{3cb3a4ef-933f-44e7-8ab1-dcd06da24652}

Error description: Message transmission to https://testeo.aduanas.gub.uy/luciaws/aws_MensManifiesto.aspx?wsdl rejected by peer: result code 401 not accepted
Reference Object: xt-httpclientaddress:{185867f5-862d-474a-a47d-60c1394d198d}
Reference Object: xt-node:{3cb3a4ef-933f-44e7-8ab1-dcd06da24652}

Error description: Message transmission to https://testeo.aduanas.gub.uy/luciaws/aws_MensManifiesto.aspx?wsdl rejected by peer: result code 401 not accepted

Notification Time : 2022-11-17 14:57:51.404
Notification Type : Failure";

			CombineAssertions(() =>
			{
				AssertEquals(header.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals(AsycudaManifestHeaderSchema.Constants.TableName, responseMessage.EM_LinkTable);

				AssertEquals(MessageStatusCodeList.Codes.Error, bill.ABL_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ERR, bill.ABL_BillStatus);

				AssertEquals(MessageStatusCodeList.Codes.Error, bill.Header.AMA_MessageStatus);

				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("The Message for job MAN0000001 has been rejected. Please update credential information of the message sender."));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertContains(expectedresult, responseMessage.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestXTErrorResult()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "8AZ8493";
			bill.ABL_BillStatus = "SNT";
			bill.ABL_MessageStatus = "AWA";

			var requestInterchange = CreateInterchange(EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received);
			var requestMessage = CreateMessage(DAETestingHelper.GetExpectedMessageXML(bodyTextWithOutSign), requestInterchange.PK, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var bodyWithWrongCredentials = Path.Combine(BaseSourcePath, DAETestingConstants.HeaderTextErrorNotificationTwo);
			var responseInterchange = CreateInterchange(EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var responseMessage = CreateMessage(DAETestingHelper.GetExpectedMessageTXT(bodyWithWrongCredentials), responseInterchange.PK, header.PK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			responseInterchange.EI_InterchangeType = MessageTypes.Codes.XER;
			responseMessage.EM_MessageType = MessageTypes.Codes.XER;
			Factory.Save();

			processor.ExecuteBatch();
			responseMessage.Reload();
			requestMessage.Reload();
			header.Reload();
			bill.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response (Failure) for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			var expectedresult = @"Result      : ERR
Description : Error Message
Contract: xt-contract:/Customs/UY/UY Message Contract
Reference Object: xt-httpclientaddress:{185867f5-862d-474a-a47d-60c1394d198d}
Reference Object: xt-node:{3cb3a4ef-933f-44e7-8ab1-dcd06da24652}

Error description: Message transmission to https://testeo.aduanas.gub.uy/luciaws/aws_MensManifiesto.aspx?wsdl rejected by peer: result code XXX
Reference Object: xt-httpclientaddress:{185867f5-862d-474a-a47d-60c1394d198d}
Reference Object: xt-node:{3cb3a4ef-933f-44e7-8ab1-dcd06da24652}

Error description: Message transmission to https://testeo.aduanas.gub.uy/luciaws/aws_MensManifiesto.aspx?wsdl rejected by peer: result code XXX

Notification Time : 2022-11-17 14:57:51.404
Notification Type : Failure";

			CombineAssertions(() =>
			{
				AssertEquals(header.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals(AsycudaManifestHeaderSchema.Constants.TableName, responseMessage.EM_LinkTable);

				AssertEquals(MessageStatusCodeList.Codes.Error, bill.ABL_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ERR, bill.ABL_BillStatus);

				AssertEquals(MessageStatusCodeList.Codes.Error, bill.Header.AMA_MessageStatus);

				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job MAN0000001 has been rejected. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertContains(expectedresult, responseMessage.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		AsycudaManifestHeader CreateHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "MAN0000001";
			header.AMA_Voyage = "LH8264";
			return header;
		}

		UYCInterchange CreateInterchange(ZString direction, ZString status)
		{
			var interchange = Factory.New<UYCInterchange>();
			interchange.EI_Status = status;
			interchange.EI_IsActive = true;
			interchange.EI_ReceiveTransmit = direction;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UYCustoms;
			interchange.EI_InterchangeType = MessageTypes.Codes.UYC;
			interchange.EI_From = UYMessageConstants.InterchangeToTest;
			interchange.EI_To = "eHub";
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_SessionGUID = new ZGuid("b0f46858-7f61-42e9-90b2-9ef35efe4b5c");

			Factory.Save();
			return interchange;
		}

		UYMessage CreateMessage(ZString file, ZGuid interchangePK, ZGuid headerPK, ZString direction, ZString status)
		{
			var message = Factory.New<UYMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UYCustoms;
			message.EM_ApplicationReference = "MAN0000001";
			message.EM_MessageNum = "0070";
			message.EM_MessageType = MessageTypes.Codes.UYC;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageText = file;
			message.EM_EI = interchangePK;

			message.EM_SystemCreateUser = Staff.GS_Code;

			if (direction == EDIMessage.Direction.Transmit)
			{
				message.EM_LinkTable = headerPK == ZGuid.Empty ? string.Empty : AsycudaManifestHeaderSchema.Constants.TableName;
				message.EM_LinkUniqueID = headerPK;
			}

			Factory.Save();
			return message;
		}

		protected override void SetUp()
		{
			base.SetUp();

			processor = new UYBranchMessageProcessor { Logger = new LoggingInformation() };

			Factory.Save();
		}
		UYBranchMessageProcessor processor;

		GlbStaff Staff => staff ?? (staff = CreateStaff("S09", "S09", "Staff09", "Dummy9@dummy.com"));
		GlbStaff staff;

		GlbStaff CreateStaff(ZString code, ZString loginName, ZString fullName, ZString email)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = code;
			staff.GS_LoginName = loginName;
			staff.GS_FullName = fullName;
			staff.GS_EmailAddress = email;

			Factory.Save();
			return staff;
		}

		readonly ZString bodyAmendment = Path.Combine(BaseSourcePath, DAETestingConstants.SuccessfulAmendment);

		readonly ZString bodyCancellation = Path.Combine(BaseSourcePath, DAETestingConstants.SuccessfulCancellation);

		readonly ZString bodyTextError = Path.Combine(BaseSourcePath, DAETestingConstants.BodyTextError);

		readonly ZString bodyTextWithOutSign = Path.Combine(BaseSourcePath, DAETestingConstants.BodyTextWithOutSign);
	}
}
