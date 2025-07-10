using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using AIMMessaging = Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.AIM.ServiceTasks.Testing
{
	[TestedType(typeof(AIMMessageServiceTask))]
	public class AIMMessageServiceTaskTest : ServiceTaskTestCase<AIMMessageServiceTask>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("30Seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestHostedServiceAttributeParameters()
		{
			AssertSingleHostedServiceAttribute("AMM", "US Air Manifest Message", "USC");
		}

		[TestDate(2020, 12, 7)]
		public void TestAIMMessage_FreightErrorReport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				_ = ManifestHeader;
				var bill = HouseBill;
				_ = ArrivalHeader;
				_ = ArrivalLine;

				var responseMessageText = @"FER
KL325/12DEC
081-11223344-HAWB123/123456
ERR/001ERROR DESCRIPTION
ERR/002ANOTHER ERROR
";

				CreateSentMessage(bill);
				var inboundMessage = CreateQueuedInboundAIMMessage(responseMessageText, AIMMessaging.Constants.AIMMessageSubTypes.FER);
				Factory.Save();

				var serviceTask = new AIMMessageServiceTask();
				InitialiseTaskSchedule(serviceTask);
				RunTaskSchedule(serviceTask);

				inboundMessage.Reload();
				AssertEquals("EM_Status", EDIMessage.Status.Received, inboundMessage.EM_Status);
				AssertEquals("AsycudaBill", inboundMessage.EM_LinkTable);
				AssertEquals(bill.PK, inboundMessage.EM_LinkUniqueID);

				bill.Reload();
				AssertEquals("ABL_MessageStatus", ASYCUDA.Business.MessageStatusCodeList.Codes.Error, bill.ABL_MessageStatus);
			}
		}

		[TestDate(2020, 12, 7)]
		public void TestAIMMessage_FreightStatusInformation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				_ = ManifestHeader;
				var bill = HouseBill;
				_ = ArrivalHeader;
				var arrLine = ArrivalLine;

				var responseMessageText = @"FSI
MIAKLM
081-11223344-HAWB123/123456
ARR/KLM325/12DEC
CSN/1C-20/12DEC1430/0158232873876
";

				CreateSentMessage(bill);
				var inboundMessage = CreateQueuedInboundAIMMessage(responseMessageText, AIMMessaging.Constants.AIMMessageSubTypes.FSI);
				Factory.Save();

				var serviceTask = new AIMMessageServiceTask();
				InitialiseTaskSchedule(serviceTask);
				RunTaskSchedule(serviceTask);

				inboundMessage.Reload();
				AssertEquals("EM_Status", EDIMessage.Status.Received, inboundMessage.EM_Status);
				AssertEquals("AsycudaBill", inboundMessage.EM_LinkTable);
				AssertEquals(bill.PK, inboundMessage.EM_LinkUniqueID);

				bill.Reload();
				AssertEquals("ABL_MessageStatus", ZString.Empty, bill.ABL_MessageStatus);
				AssertEquals("ABL_BillStatus", "", bill.ABL_BillStatus);

				arrLine.Reload();
				AssertEquals("ABL_MessageStatus", "", arrLine.ATL_CargoStatus);
			}
		}

		[TestDate(2020, 12, 7)]
		public void TestAIMMessage_FreightStatusNotification()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				_ = ManifestHeader;
				var bill = HouseBill;
				_ = ArrivalHeader;
				var arrLine = ArrivalLine;

				var responseMessageText = @"FSN
MIAKLM
081-11223344-HAWB123/123456
ARR/KL325/12DEC
CSN/1C-20/12DEC1430/0158232873876
";

				CreateSentMessage(bill);
				var inboundMessage = CreateQueuedInboundAIMMessage(responseMessageText, AIMMessaging.Constants.AIMMessageSubTypes.FSN);

				Factory.Save();

				var serviceTask = new AIMMessageServiceTask();
				InitialiseTaskSchedule(serviceTask);
				RunTaskSchedule(serviceTask);

				inboundMessage.Reload();
				AssertEquals("EM_Status", EDIMessage.Status.Received, inboundMessage.EM_Status);
				AssertEquals("AsycudaBill", inboundMessage.EM_LinkTable);
				AssertEquals(bill.PK, inboundMessage.EM_LinkUniqueID);

				bill.Reload();
				AssertEquals("ABL_MessageStatus", ZString.Empty, bill.ABL_MessageStatus);
				AssertEquals("ABL_BillStatus", "1C", bill.ABL_BillStatus);

				arrLine.Reload();
				AssertEquals("ABL_MessageStatus", "1C", arrLine.ATL_CargoStatus);
			}
		}

		[TestDate(2020, 12, 7)]
		public void TestAIMMessage_MawbHawbNotFound()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var manifestHeader = ManifestHeader;
				manifestHeader.AMA_MasterBill = "08112345678";
				var bill = HouseBill;
				bill.ABL_BillNumber = "HAWB999";
				var arrHeader = ArrivalHeader;
				var arrLine = ArrivalLine;

				var responseMessageText = @"FSI
MIAKLM
081-11223344-HAWB123/123456
ARR/KLM325/12DEC
CSN/1C-20/12DEC1430/0158232873876
";

				CreateSentMessage(bill);
				var inboundMessage = CreateQueuedInboundAIMMessage(responseMessageText, AIMMessaging.Constants.AIMMessageSubTypes.FSI);
				Factory.Save();

				var serviceTask = new AIMMessageServiceTaskForTest();
				InitialiseTaskSchedule(serviceTask);
				RunTaskSchedule(serviceTask);

				inboundMessage.Reload();
				AssertEquals("EM_Status", EDIMessage.Status.Failed, inboundMessage.EM_Status);
				AssertEquals("EM_LinkUniqueID", ZGuid.Empty, inboundMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", ZString.Empty, inboundMessage.EM_LinkTable);

				var notes = inboundMessage.Notes.GetAllNotes();
				var note = notes.Cast<StmNote>().First(n => n.ST_Description == "AIM Message Processing");
				AssertEquals("A Manifest record was not found for MasterBill = 081-11223344, HouseBill = HAWB123.", note.ST_NoteText);

				var log = serviceTask.Logger.Logs.First(l => l.Message.StartsWith("A Manifest record was not found"));
				AssertEquals("A Manifest record was not found for MasterBill = 081-11223344, HouseBill = HAWB123.", log.Message);

				bill.Reload();
				AssertEquals("ABL_MessageStatus", ZString.Empty, bill.ABL_MessageStatus);
				AssertEquals("ABL_BillStatus", "", bill.ABL_BillStatus);

				arrLine.Reload();
				AssertEquals("ABL_MessageStatus", "", arrLine.ATL_CargoStatus);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Air Manifest Message",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USAMA,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}

		#region Implementation

		const string OutgoingMessageText = @"FRI
JFKXYZ
999-12345675-M
WBL/FRA/T1/K10/TOYS
ARR/XYZ123/25OCT
SHP/TOTLERTOYS
/12 VIRGINIA COURT
/FRANKFURT
/DE
CNE/TOYSRWE
/8812 FUN STREET
/NEWYORK/NY
/US/12345/123-456-7890";

		EDIMessage CreateSentMessage(BusinessObject linkedObject)
		{
			var outgoingMessage = Factory.New<AIMMessaging.AIMEDIMessage>();
			outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.FHL;
			outgoingMessage.EM_MessageSubType = AIMMessaging.Constants.AIMMessageSubTypes.FRI;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageText = OutgoingMessageText;
			outgoingMessage.EM_MessageNum = "TEST11223344";
			outgoingMessage.EM_LinkedObject = linkedObject;
			outgoingMessage.EM_ApplicationReference = "123456";

			return outgoingMessage;
		}

		AIMMessaging.AIMEDIMessage CreateQueuedInboundAIMMessage(ZString responseMessageText, string messageSubType)
		{
			var inboundMessage = Factory.New<AIMMessaging.AIMEDIMessage>();
			inboundMessage.EM_MessageType = EDIMessageTypeList.Codes.FHL;
			inboundMessage.EM_MessageSubType = messageSubType;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_MessageText = responseMessageText;
			inboundMessage.EM_MessageNum = "TEST998877";

			return inboundMessage;
		}

		AsycudaManifestHeader ManifestHeader
		{
			get
			{
				if (fManifestHeader == null)
				{
					fManifestHeader = Factory.NewWithValidTestData<ACEManifest.Business.AsycudaManifestHeader>();
					fManifestHeader.AMA_RN_NKCountry = "US";
					fManifestHeader.AMA_TransportMode = "AIR";
					fManifestHeader.AMA_MasterBill = "08111223344";
					fManifestHeader.AMA_SystemCreateTimeUtc = ZDateTime.UtcNow;
					fManifestHeader.AMA_CarrierCode = "KLM";
					fManifestHeader.AMA_RL_NKPortOfFirstArrival = "USMIA";
					fManifestHeader.AMA_Voyage = "KL325";
					fManifestHeader.AMA_E_ARV = new ZDateTime(2020, 12, 12);
				}
				return fManifestHeader;
			}
		}
		AsycudaManifestHeader fManifestHeader;

		AsycudaBill HouseBill
		{
			get
			{
				if (fHouseBill == null)
				{
					fHouseBill = ManifestHeader.Bills.AddNew();
					fHouseBill.ABL_BillNumber = "HAWB123";
					fHouseBill.ABL_SenderReference = "123456";
					fHouseBill.ABL_MessageStatus = ZString.Empty;
				}
				return fHouseBill;
			}
		}
		AsycudaBill fHouseBill;

		AsycudaArrivalHeader ArrivalHeader
		{
			get
			{
				if (fArrivalHeader == null)
				{
					fArrivalHeader = ManifestHeader.ArrivalHeaders.AddNew();
					fArrivalHeader.ATH_VoyageFlightNo = "KL325";
					fArrivalHeader.ATH_ETAAtDischargePort = new ZDateTime(2020, 12, 12);
				}
				return fArrivalHeader;
			}
		}
		AsycudaArrivalHeader fArrivalHeader;

		AsycudaArrivalLine ArrivalLine
		{
			get
			{
				if (fArrivalLine == null)
				{
					fArrivalLine = ArrivalHeader.ArrivalDetails.AddNew();
					fArrivalLine.ATL_ABL_AsycudaBill = HouseBill.PK;
				}
				return fArrivalLine;
			}
		}
		AsycudaArrivalLine fArrivalLine;

		class AIMMessageServiceTaskForTest : AIMMessageServiceTask
		{
			public AIMMessageServiceTaskForTest() : base()
			{
			}

			new public LoggingInformation Logger => base.Logger;
		}

		#endregion
	}
}
