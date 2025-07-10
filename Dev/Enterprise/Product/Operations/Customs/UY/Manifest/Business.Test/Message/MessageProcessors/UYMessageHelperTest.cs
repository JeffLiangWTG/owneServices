using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	sealed class UYMessageHelperTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLookForAsycudaManifestHeader()
		{
			var header = CreateHeader();
			var bodyTextWithOutSign = DAETestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, DAETestingConstants.BodyTextWithOutSign));
			var sessionGUID = new ZGuid("B1AC31CE-4D5D-45BA-AD7E-1F5227C784A0");
			var requestInterchange = CreateInterchange(sessionGUID, EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received);
			var requestMessage = CreateMessage(bodyTextWithOutSign, requestInterchange.PK, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var responseInterchange = CreateInterchange(requestInterchange.EI_SessionGUID, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var responseMessage = CreateMessage(bodyTextWithOutSign, responseInterchange.PK, header.PK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			new UYManifestMessageProcessor<UYMessage>(GetNewLoggerForTesting()).ProcessMessage(responseMessage);

			CombineAssertions(() =>
			{
				AssertEquals(header.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals(AsycudaManifestHeaderSchema.Constants.TableName, responseMessage.EM_LinkTable);

				AssertContains(bodyTextWithOutSign, responseMessage.EM_MessageInterpretation);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLookForAsycudaBill()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "8AZ8493";

			var bodyTextWithOutSign = DAETestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, DAETestingConstants.BodyTextWithOutSign));
			var sessionGUID = new ZGuid("B3C35653-E185-4495-9857-244C79CCC830");
			var requestInterchange = CreateInterchange(sessionGUID, EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received);
			var requestMessage = CreateMessage(bodyTextWithOutSign, requestInterchange.PK, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var responseInterchange = CreateInterchange(requestInterchange.EI_SessionGUID, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var responseMessage = CreateMessage(bodyTextWithOutSign, responseInterchange.PK, header.PK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			new UYManifestMessageProcessor<UYMessage>(GetNewLoggerForTesting()).ProcessMessage(responseMessage);

			var cusEntryNumCreated = Factory.Load<CusEntryNumber>(new ZQuery());
			var cusEntryNum = cusEntryNumCreated.Where(x => x.CE_ParentID == bill.PK).FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertEquals("7", bill.CustomsEntryNumber);
				AssertEquals("DNA", bill.CustomsEntryNumberType);
				AssertEquals(MessageStatusCodeList.Codes.Accepted, bill.ABL_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ACP, bill.ABL_BillStatus);

				AssertEquals(bill.PK, cusEntryNum.CE_ParentID);
				AssertEquals("AsycudaBill", cusEntryNum.CE_ParentTable);
				AssertEquals(bill.CustomsEntryNumber, cusEntryNum.CE_EntryNum);
				AssertEquals(bill.CustomsEntryNumberType, cusEntryNum.CE_EntryType);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInterpretationViewForOneBillResponse()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "8AZ8493";

			var bodyTextWithOutSign = DAETestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, DAETestingConstants.BodyTextWithOutSign));
			var sessionGUID = new ZGuid("B1AC31CE-4D5D-45BA-AD7E-1F5227C784A0");
			var requestInterchange = CreateInterchange(sessionGUID, EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received);
			var requestMessage = CreateMessage(bodyTextWithOutSign, requestInterchange.PK, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var responseInterchange = CreateInterchange(requestInterchange.EI_SessionGUID, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var responseMessage = CreateMessage(bodyTextWithOutSign, responseInterchange.PK, header.PK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			new UYManifestMessageProcessor<UYMessage>(GetNewLoggerForTesting()).ProcessMessage(responseMessage);

			var expectedresult = @"Result      : Operacion Exitosa
Description : El alta del conocimiento 8AZ8493 fue exitosa.

References

DNANumber   : 7
Sequence    : 1
HBLNumber   : 8AZ8493

Result      : Operacion Exitosa
Description : Alta linea exitosa.

References

Sequence    : 1
HBLNumber   : 8AZ8493

";

			CombineAssertions(() =>
			{
				AssertEquals(expectedresult, responseMessage.EM_MessageInterpretation);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInterpretationViewForTwoBillResponse()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "8AZ8493";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "7LNY798";

			var oneBillAcceptedOneBillRejected = DAETestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, DAETestingConstants.OneBillAcceptedOneBillRejected));
			var sessionGUID = new ZGuid("B1AC31CE-4D5D-45BA-AD7E-1F5227C784A0");
			var requestInterchange = CreateInterchange(sessionGUID, EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received);
			var requestMessage = CreateMessage(oneBillAcceptedOneBillRejected, requestInterchange.PK, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var responseInterchange = CreateInterchange(requestInterchange.EI_SessionGUID, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var responseMessage = CreateMessage(oneBillAcceptedOneBillRejected, responseInterchange.PK, header.PK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			new UYManifestMessageProcessor<UYMessage>(GetNewLoggerForTesting()).ProcessMessage(responseMessage);

			var expectedresult = @"Result      : Operacion Exitosa
Description : El alta del conocimiento 8AZ8493 fue exitosa.

References

DNANumber   : 7
Sequence    : 1
HBLNumber   : 8AZ8493

Result      : Ya existe el Conocimiento Original para el manifiesto o lo está enviando más de una vez en el mensaje.
Description : Conocimiento Orig. : 7LNY798 Nro. de Manifiesto DNA: LH8264

References

Sequence    : 2
HBLNumber   : 7LNY798

";

			CombineAssertions(() =>
			{
				AssertEquals(expectedresult, responseMessage.EM_MessageInterpretation);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInterpretationViewForInvalidResponse()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "NOBILL";
			bill.ABL_BillStatus = "SNT";
			bill.ABL_MessageStatus = "AWA";

			var bodyTextCredentialsError = DAETestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, DAETestingConstants.BodyTextCredentialsError));
			var requestInterchange = CreateInterchange(new ZGuid(), EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received);
			var requestMessage = CreateMessage(bodyTextCredentialsError, requestInterchange.PK, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var responseInterchange = CreateInterchange(requestInterchange.EI_SessionGUID, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var responseMessage = CreateMessage(bodyTextCredentialsError, responseInterchange.PK, header.PK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			new UYManifestMessageProcessor<UYMessage>(GetNewLoggerForTesting()).ProcessMessage(responseMessage);

			var expectedresult = @"Result      : El Certificado no es valido
Description : El certificado no es valido, Error:NotTimeValid:A required certificate is not within its validity period when verifying against the current system clock or the timestamp in the signed file.

References


";

			CombineAssertions(() =>
			{
				AssertEquals(expectedresult, responseMessage.EM_MessageInterpretation);
			});
		}

		AsycudaManifestHeader CreateHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "MAN0000001";
			header.AMA_Voyage = "LH8264";
			return header;
		}

		UYCInterchange CreateInterchange(ZGuid sessionGUID, ZString direction, ZString status)
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
			interchange.EI_SessionGUID = sessionGUID;

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
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			message.EM_LinkUniqueID = headerPK;

			Factory.Save();
			return message;
		}

		LoggingInformation GetNewLoggerForTesting() => new LoggingInformationForTesting();

		sealed class LoggingInformationForTesting : LoggingInformation
		{
			public LoggingInformationForTesting() : base()
			{
				LogMessages = new ZStringBuilder();
				OnLogInfoAdded += new LogInfoAdded((string log, LogType logType) => LogMessages.AppendLine($"{logType.ToString()}: {log}"));
			}

			public override void ClearLogs()
			{
				base.ClearLogs();
				LogMessages = new ZStringBuilder();
			}

			internal ZStringBuilder LogMessages;
		}
	}
}
