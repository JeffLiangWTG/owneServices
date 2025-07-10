using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	abstract class OneStopContainerInformationProcessor_BaseTest : TestCaseWithFactory
	{
		public void TestProcessContainerEventResponseEmail_ForNonExistentMailItem()
		{
			var processor = new OneStopContainerInformationProcessor();
			bool processed = false;
			AssertNoExceptionThrown(() => processed = processor.ProcessContainerEventResponseEmail(new ZGuid(new Guid("C5CAA178-8228-408F-81DA-21653244CF58")), new LoggerForTest()));
			Assert(!processed);
		}

		public void TestMessageFilter()
		{
			EmailDef email1 = new EmailDef();
			email1.FromAddress = "helpdesk@1-stop.biz";
			email1.AddRecipientForUserCommunication("clinton@edi.com.au");
			email1.Subject = "1-STOP ALERT*etc";
			Env.OutgoingMailManager.Create(Factory, email1);
			MailItem mailItem1 = LoadMailItem(email1);

			EmailDef email2 = CreateContainerNotificationEmail("Discharge off Vessel", "XXXXX");
			Env.OutgoingMailManager.Create(Factory, email2);
			MailItem mailItem2 = LoadMailItem(email2);

			SetVesselName("ARIAKE");
			SetVoyage("0613S");

			Factory.Save();

			var helper = new MessageFilterTestHelper<OneStopContainerInformationProcessor, MailItem>();

			Assert(helper.Process(mailItem1));
			AssertEquals(0, helper.Log.Count);

			Assert(helper.Process(mailItem2));
			AssertEquals(1, helper.Log.Count);
			AssertEquals("Information|Processing event 'Discharge off Vessel' for container 'GATU0859042'", helper.Log[0]);
		}

		public void TestCanRunInAnyBranch()
		{
			var eventType = OneStopConstants.ContainerEventTypes.GateOut;
			var locationCode = "PATDH";
			var organisation = CreateOneStopOrganisationCodeAndSave(locationCode, "", IsContainerTerminal, !IsContainerYard, "AUMEL");
			var email = CreateContainerNotificationEmail(eventType, locationCode);
			Env.OutgoingMailManager.Create(Factory, email);

			SetVesselName("ARIAKE");
			SetVoyage("0613S");
			Factory.Save();

			var company = Factory.New<GlbCompany>();
			company.GC_Name = "Test Name";

			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "B1";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUMEL";

			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "B2";
			branch2.GB_GC = company.PK;
			branch2.GB_RL_NKHomePort = "AUBNE";

			var extraPort = Factory.New<GlbBranchExtraPorts>();
			extraPort.GY_GB = branch.PK;
			extraPort.GY_IsValid = true;
			extraPort.GY_RL_NKAdditionalBranchRelatedPort = "AUMEL";

			var extraPort2 = Factory.New<GlbBranchExtraPorts>();
			extraPort2.GY_GB = branch2.PK;
			extraPort2.GY_IsValid = true;
			extraPort2.GY_RL_NKAdditionalBranchRelatedPort = "AUMEL";

			Factory.Save();

			var mailItem = LoadMailItem(email);
			mailItem.MI_Direction = MailDirection.Receive;

			Factory.Save();

			var logger = new LoggerForTest();

			using (Env.Instance.TemporaryServiceTaskContext("MAP", true))
			{
				Processor.ProcessContainerEventResponseEmail(mailItem.PK, logger);
			}

			Assert(logger.LogEntries.Any(x => x.Contains("Processing event '" + eventType + "' for container 'GATU0859042'")));
			AssertEquals("No Errors", false, ErrorReporter.LastMessageReported.Contains("Service Task: MAP accesses environment current branch without setting the environment first.\r\nDirect access to Env.CurrentBranch is not allowed from service tasks, please use Enterprise.Environment.DisposableEnvironment to set your service task's running environment before the task execution.\r\n"));

			mailItem.Delete();
		}

		public void TestProcess_ForDischargeOffVessel()
		{
			TestProcess("Discharge off Vessel", "XXXXX", Container.JC_FCLUnloadFromVesselInfo);
		}

		public void TestProcess_ForLoadOnVessel()
		{
			TestProcess("Load on Vessel", "XXXXX", Container.JC_FCLOnBoardVesselInfo);
		}

		public void TestProcess_ForImportPreAdvise()
		{
			TestProcess("On Board Vessel", "XXXXX", Events.StatusUpdated, "Lloyds:9294159,Vessel:ARIAKE,Voyage:0613S,Load:2006/09/10 18:52 PM|DEP = 1-stop|TYP=Import Pre Advice|LOC=NZAKL");
		}

		public void TestProcess_ForExportPreAdvise()
		{
			TestProcess("Export Pre-Advise", "XXXXX", Events.StatusUpdated, "Lloyds:9294159,Vessel:ARIAKE,Voyage:0613S,Load:2006/09/10 18:52 PM|DEP = 1-stop|TYP=Export Pre Advice|LOC=NZAKL");
		}

		public void TestProcess_ForDehire()
		{
			TestProcess("Dehire", "NOTHING", Container.JC_ContainerYardEmptyReturnGateInInfo);
		}

		public void TestProcess_ForContainerTerminalGateIn()
		{
			TestProcess(OneStopConstants.ContainerEventTypes.GateIn, "PATDH", Container.JC_FCLWharfGateInInfo);
			TestProcess(OneStopConstants.ContainerEventTypes.GateIn, "ASLPB", Container.JC_FCLWharfGateInInfo);

			OrgHeader organisation = CreateOneStopOrganisationCodeAndSave("1STRegNo", "", IsContainerTerminal, !IsContainerYard);
			TestProcess(OneStopConstants.ContainerEventTypes.GateIn, "1STRegNo", Container.JC_FCLWharfGateInInfo);

			organisation.Delete();
			CreateRefPremiseCodeAndSave("1STRegNo", "", IsContainerTerminal, !IsContainerYard);
			TestProcess(OneStopConstants.ContainerEventTypes.GateIn, "1STRegNo", Container.JC_FCLWharfGateInInfo);
		}

		public void TestProcess_ForContainerTerminalGateOut()
		{
			TestProcess(OneStopConstants.ContainerEventTypes.GateOut, "PATDH", Container.JC_FCLWharfGateOutInfo);
			TestProcess(OneStopConstants.ContainerEventTypes.GateOut, "ASLPB", Container.JC_FCLWharfGateOutInfo);

			OrgHeader organisation = CreateOneStopOrganisationCodeAndSave("1STRegNo", "", IsContainerTerminal, !IsContainerYard);
			TestProcess(OneStopConstants.ContainerEventTypes.GateOut, "1STRegNo", Container.JC_FCLWharfGateOutInfo);

			organisation.Delete();
			CreateRefPremiseCodeAndSave("1STRegNo", "", IsContainerTerminal, !IsContainerYard);
			TestProcess(OneStopConstants.ContainerEventTypes.GateIn, "1STRegNo", Container.JC_FCLWharfGateOutInfo);
		}

		public void TestProcess_ForContainerTerminalGateOut_EventPropagated()
		{
			TestProcess(OneStopConstants.ContainerEventTypes.GateOut, "PATDH", Container.JC_FCLWharfGateOutInfo);

			var eventLog1 = Container.Logs.MostRecentLogByEventTime(AutoEvents.GateOut);
			AssertNotNull("GateOut event log should be created", eventLog1);
			AssertEquals("SL_EventTime", new ZDateTime(2006, 9, 6, 10, 31, 0), eventLog1.SL_EventTime);
			AssertEquals("SL_Reference", "|FAC=CTO|LOC=AUMEL", eventLog1.SL_Reference);

			var parent = Container.ContainerParent as EnterpriseBusinessObject;
			AssertNotNull("ContainerParent should exist", parent);
			var eventLog2 = parent.Logs.MostRecentLogByEventTime(AutoEvents.GateOut);
			AssertNotNull("GateOut event log should be created", eventLog2);
			AssertEquals("SL_EventTime", new ZDateTime(2006, 9, 6, 10, 31, 0), eventLog2.SL_EventTime);
			AssertEquals("SL_Reference", PropagatedReference, eventLog2.SL_Reference);
		}

		public void TestProcess_ForContainerYardGateIn()
		{
			TestProcess(OneStopConstants.ContainerEventTypes.GateIn, "ADLMUE", Container.JC_ContainerYardEmptyReturnGateInInfo);
			TestProcess(OneStopConstants.ContainerEventTypes.GateIn, "MBCP1", Container.JC_ContainerYardEmptyReturnGateInInfo);

			OrgHeader organisation = CreateOneStopOrganisationCodeAndSave("1STRegNo", "", !IsContainerTerminal, IsContainerYard);
			TestProcess(OneStopConstants.ContainerEventTypes.GateIn, "1STRegNo", Container.JC_ContainerYardEmptyReturnGateInInfo);

			organisation.Delete();
			CreateRefPremiseCodeAndSave("1STRegNo", "", !IsContainerTerminal, IsContainerYard);
			TestProcess(OneStopConstants.ContainerEventTypes.GateIn, "1STRegNo", Container.JC_ContainerYardEmptyReturnGateInInfo);
		}

		public void TestProcess_ForContainerArrivalCTOStorageStartDate()
		{
			TestProcess(OneStopConstants.ContainerEventTypes.StorageStart, "ADLMUE", Container.JC_ArrivalCTOStorageStartDateInfo);
			Assert(Container.JC_OverrideFCLAvailableStorage);

			Container.JC_OverrideFCLAvailableStorage = false;
			TestProcess(OneStopConstants.ContainerEventTypes.StorageStart, "MBCP1", Container.JC_ArrivalCTOStorageStartDateInfo);
			Assert(Container.JC_OverrideFCLAvailableStorage);

			var organisation = CreateOneStopOrganisationCodeAndSave("1STRegNo", "", !IsContainerTerminal, IsContainerYard);
			TestProcess(OneStopConstants.ContainerEventTypes.StorageStart, "1STRegNo", Container.JC_ArrivalCTOStorageStartDateInfo);
			Assert(Container.JC_OverrideFCLAvailableStorage);

			organisation.Delete();
			Container.JC_OverrideFCLAvailableStorage = false;
			CreateRefPremiseCodeAndSave("1STRegNo", "", !IsContainerTerminal, IsContainerYard);
			TestProcess(OneStopConstants.ContainerEventTypes.StorageStart, "1STRegNo", Container.JC_ArrivalCTOStorageStartDateInfo);
			Assert(Container.JC_OverrideFCLAvailableStorage);
		}

		public void TestProcess_ForContainerFCLAvailable()
		{
			TestProcess(OneStopConstants.ContainerEventTypes.ImportAvailable, "ADLMUE", Container.JC_FCLAvailableInfo);
			Assert(Container.JC_OverrideFCLAvailableStorage);

			Container.JC_OverrideFCLAvailableStorage = false;
			TestProcess(OneStopConstants.ContainerEventTypes.ImportAvailable, "MBCP1", Container.JC_FCLAvailableInfo);
			Assert(Container.JC_OverrideFCLAvailableStorage);

			var organisation = CreateOneStopOrganisationCodeAndSave("1STRegNo", "", !IsContainerTerminal, IsContainerYard);
			TestProcess(OneStopConstants.ContainerEventTypes.ImportAvailable, "1STRegNo", Container.JC_FCLAvailableInfo);
			Assert(Container.JC_OverrideFCLAvailableStorage);

			organisation.Delete();
			Container.JC_OverrideFCLAvailableStorage = false;
			CreateRefPremiseCodeAndSave("1STRegNo", "", !IsContainerTerminal, IsContainerYard);
			TestProcess(OneStopConstants.ContainerEventTypes.ImportAvailable, "1STRegNo", Container.JC_FCLAvailableInfo);
			Assert(Container.JC_OverrideFCLAvailableStorage);
		}

		public void TestProcess_ForContainerYardGateOut()
		{
			TestProcess(OneStopConstants.ContainerEventTypes.GateOut, "ADLMUE", Container.JC_ContainerYardEmptyPickupGateOutInfo);
			TestProcess(OneStopConstants.ContainerEventTypes.GateOut, "MBCP1", Container.JC_ContainerYardEmptyPickupGateOutInfo);

			OrgHeader organisation = CreateOneStopOrganisationCodeAndSave("1STRegNo", "", !IsContainerTerminal, IsContainerYard);
			TestProcess(OneStopConstants.ContainerEventTypes.GateOut, "1STRegNo", Container.JC_ContainerYardEmptyPickupGateOutInfo);

			organisation.Delete();
			CreateRefPremiseCodeAndSave("1STRegNo", "", !IsContainerTerminal, IsContainerYard);
			TestProcess(OneStopConstants.ContainerEventTypes.GateIn, "1STRegNo", Container.JC_ContainerYardEmptyPickupGateOutInfo);
		}

		public void TestProcess_WhenQueueLargerThanBatchSize()
		{
			for (int i = 0; i < 100; i++)
			{
				EmailDef email = CreateContainerNotificationEmail(Events.StatusUpdated.Code, "");
				Env.OutgoingMailManager.Create(Factory, email);
			}
			TestProcess(OneStopConstants.ContainerEventTypes.GateIn, "PATDH", Container.JC_FCLWharfGateInInfo);
		}

		void TestProcess(ZString eventType, ZString locationCode, ZPropertyInfo containerDateProperty)
		{
			TestProcess(eventType, locationCode);
			AssertEquals(eventType + " date should be set", new ZDateTime(2006, 9, 6, 10, 31, 0), containerDateProperty.Value);
		}

		void TestProcess(ZString eventType, ZString locationCode, Event expectedEventType, ZString expectedReference)
		{
			TestProcess(eventType, locationCode);
			StmALog eventLog = Container.Logs.MostRecentLogByEventTime(expectedEventType);
			AssertNotNull("Event log for 1-Stop event " + eventType + " should be created", eventLog);
			AssertEquals("SL_EventTime", new ZDateTime(2006, 9, 6, 10, 31, 0), eventLog.SL_EventTime);
			AssertEquals("SL_Reference", expectedReference, eventLog.SL_Reference);
		}

		void TestProcess(ZString eventType, ZString locationCode)
		{
			EmailDef email = CreateContainerNotificationEmail(eventType, locationCode);
			Env.OutgoingMailManager.Create(Factory, email);

			EmailDef decoyEmail1 = new EmailDef();
			decoyEmail1.FromAddress = "helpdesk@1-stop.biz";
			decoyEmail1.AddRecipientForUserCommunication("clinton@edi.com.au");
			decoyEmail1.Subject = "1-STOP DECOY*376530*" + locationCode + "*GATU0859042*" + eventType + "*" + Container.PK.ToString().Replace("-", "") + "*2006/09/06 10:31 AM*ARIAKE*9294159*0613S*ETA 2006/09/15 07:00 AM";
			decoyEmail1.Body = email.Body;
			Env.OutgoingMailManager.Create(Factory, decoyEmail1);

			EmailDef decoyEmail2 = new EmailDef();
			decoyEmail2.FromAddress = "clinty@edi.com.au";
			decoyEmail2.AddRecipientForUserCommunication("clinton@edi.com.au");
			decoyEmail2.Subject = "1-STOP NOTIFY*376530*" + locationCode + "*GATU0859042*" + eventType + "*" + Container.PK.ToString().Replace("-", "") + "*2006/09/06 10:31 AM*ARIAKE*9294159*0613S*ETA 2006/09/15 07:00 AM";
			decoyEmail2.Body = email.Body;
			Env.OutgoingMailManager.Create(Factory, decoyEmail2);

			SetVesselName("ARIAKE");
			SetVoyage("0613S");
			Factory.Save();

			MailItem mailItem = LoadMailItem(email);
			MailItem decoyMailItem1 = LoadMailItem(decoyEmail1);
			MailItem decoyMailItem2 = LoadMailItem(decoyEmail2);
			mailItem.MI_Direction = MailDirection.Receive;
			decoyMailItem1.MI_Direction = MailDirection.Receive;
			decoyMailItem2.MI_Direction = MailDirection.Receive;
			Factory.Save();

			Notifications.Clear();

			using (Env.Instance.TemporaryServiceTaskContext("MAP", true))
			{
				Processor.Process(Notifications);
			}
			AssertEquals("Processing event '" + eventType + "' for container 'GATU0859042'\r\n", Notifications.AsString);
			AssertEquals("The mail item should be marked 'processed' after it is processed", MailStatus.Processed, mailItem.MI_Status);
			AssertEquals("Mail item with wrong subject not processed", MailStatus.Queued, decoyMailItem1.MI_Status);
			AssertEquals("Mail item with wrong from address not processed", MailStatus.Queued, decoyMailItem2.MI_Status);

			mailItem.Delete();
			decoyMailItem1.Delete();
			decoyMailItem2.Delete();
		}

		[ExpectNoExceptions]
		public void TestProcess_ZSaveConcurrencyExceptionHandled()
		{
			var testProcessor = new TestOneStopContainerInformationProcessor();
			testProcessor.Process(Notifications);
		}

		class TestOneStopContainerInformationProcessor : OneStopContainerInformationProcessor
		{
			protected override void TryProcess(INotifications notifications)
			{
				if (!concurrencyExceptionThrown)
				{
					concurrencyExceptionThrown = true;
					throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), ((INeedRow)Factory.New<DummyBusinessObject>()).Row, ((IDbConnected)Factory).Connection), Factory);
				}
			}

			bool concurrencyExceptionThrown;

			BusinessObjectFactory Factory
			{
				get { return factory ?? (factory = new BusinessObjectFactory()); }
			}
			BusinessObjectFactory factory;
		}

		public void TestProcess_OnlyIfContainerVesselVoyageMatches()
		{
			MailItem mailItem = CreateContainerNotificationEmail(Factory, OneStopConstants.ContainerEventTypes.LoadOnVessel, "XXXXX");

			Container.JC_ContainerNum = "Wrong";
			SetVesselName("ARIAKE");
			SetVoyage("0613S");
			Factory.Save();
			Processor.Process(Notifications);
			AssertEquals("Date should NOT be set", ZDateTime.Empty, Container.JC_FCLOnBoardVessel);
			mailItem.MI_Status = MailStatus.Queued;

			Container.JC_ContainerNum = "GATU0859042";
			SetVesselName("Wrong");
			SetVoyage("0613S");
			Factory.Save();
			Processor.Process(Notifications);
			AssertEquals("Date should NOT be set", ZDateTime.Empty, Container.JC_FCLOnBoardVessel);
			mailItem.MI_Status = MailStatus.Queued;

			SetVesselName("ARIAKE");
			SetVoyage("Wrong");
			Factory.Save();
			Processor.Process(Notifications);
			AssertEquals("Date should NOT be set", ZDateTime.Empty, Container.JC_FCLOnBoardVessel);
			mailItem.MI_Status = MailStatus.Queued;

			SetVesselName("ARIaKe ");
			SetVoyage(" 0613s");
			Factory.Save();
			Processor.Process(Notifications);
			AssertEquals("Date should be set", new ZDateTime(2006, 9, 6, 10, 31, 0), Container.JC_FCLOnBoardVessel);
			AssertEquals("The mail item should be marked 'processed'", MailStatus.Processed, mailItem.MI_Status);
			Container.JC_FCLOnBoardVessel = ZDateTime.Empty;
		}

		public void TestProcess_OnlyIfArrivalCTOStorageDateTimeValid()
		{
			AssertErrorNotificationForInvalidDate(new ZDateTime(1245, 02, 22).ToISO8601ShortDateString().Replace("-", "/"));
			AssertErrorNotificationForInvalidDate(new ZDateTime(2079, 6, 7).ToISO8601ShortDateString().Replace("-", "/"));
			AssertErrorNotificationForInvalidDate("2023 / 11 / 04 08:&#8202;50 AM");

			var validDate = new ZDateTime(2024, 05, 17, 10, 31, 0);
			var mailItem4 = CreateContainerNotificationEmail(Factory, OneStopConstants.ContainerEventTypes.StorageStart, "ADLMUE", validDate.ToISO8601ShortDateString().Replace("-", "/"));
			mailItem4.MI_Status = MailStatus.Queued;
			Factory.Save();

			Processor.Process(Notifications);

			Assert("Should process without error with valid DateTime", !Notifications.HasErrors);
			AssertEquals("ArrivalCTOStorageStartDate should be set", validDate, Container.JC_ArrivalCTOStorageStartDate);
		}

		void AssertErrorNotificationForInvalidDate(string invalidDate)
		{
			var mailItem = CreateContainerNotificationEmail(Factory, OneStopConstants.ContainerEventTypes.StorageStart, "ADLMUE", invalidDate);
			SetVesselName("ARIAKE");
			SetVoyage("0613S");
			mailItem.MI_Status = MailStatus.Queued;
			Factory.Save();

			Processor.Process(Notifications);

			string error = $"Error: Data out of range error (Invalid DateTime, 1-stop message: '{mailItem.MI_Body}')";
			Assert("Should have Validation Error for invalid DateTime", Notifications.Events.Contains(error));
			AssertEquals("ArrivalCTOStorageStartDate should not be set", ZDateTime.Empty, Container.JC_ArrivalCTOStorageStartDate);

			Notifications.Clear();
			mailItem.Delete();
		}

		public void TestProcess_VesselMismatchNotification_NonAusBoundConsol()
		{
			CreateContainerNotificationEmail(Factory, OneStopConstants.ContainerEventTypes.LoadOnVessel, "XXXXX");

			SetContainerDischargePort("INBOM");
			Container.JC_ContainerNum = "GATU0859042";
			Container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			SetVesselName("NYK PROVIDER");
			SetVoyage("1234");
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			Processor.Process(Notifications);
			AssertEquals("Date should NOT be set", ZDateTime.Empty, Container.JC_FCLOnBoardVessel);

			AssertNotification(@$"Incoming 1-Stop alert for Container GATU0859042 (20GP) is not matching the details found in CargoWise<i>One</i> for this Container. Due to Vessel/Lloyds/Voyage information a discrepancy has been recognised and this event has not been processed.<br /><br />

<b>edi<i>{Core.Constants.ProductName}</i> Details:</b><br />
<br />
Container Job#: " + JobNumber + @"<br />
Consignee: Multiple<br />
Consignor: Multiple<br />
Vessel Name: NYK PROVIDER<br />
Lloyds No: 7807263<br />
Voyage Number: 1234<br />
Load: <br />
Discharge: INBOM<br />

<br /><br />

<b>1-Stop Message:</b><br />
<br />
<br />
Type 			: 1-STOP NOTIFY <br />
Message ID 		: 376530<br />
Event Location 	: XXXXX<br />
Vessel/Container 	: GATU0859042<br />
Event Type 		: Load on Vessel<br />
Event Date 		: 2006/09/06<br />
Event Time 		: 10:31 AM<br />
Information 	: " + Container.PK.ToString().ToLower().Replace("-", "") + @"<br />
Vessel Name 	: ARIAKE<br />
Lloyds No 		: 9294159<br />
Voyage Number 	: 0613S<br />
ETD from Load Port: 2006/09/10 18:52 PM from NZAKL<br />
ETA at Discharge Port: 2006/09/15 07:00 AM at AUBNE<br />
<br />      </td>
    </tr>
    <tr>
      <td><img src=""cid:Footer.jpg"" alt=""Footer Image"" /></td>
    </tr>
  </table>
</body>
</html>
");
		}

		public void TestProcess_VesselMismatchNotification_AusLoadAndDischarge()
		{
			CreateContainerNotificationEmail(Factory, OneStopConstants.ContainerEventTypes.ExportPreAdvised, "XXXXX");

			SetContainerLoadPort("AUMEL");
			SetContainerDischargePort("AUSYD");
			Container.JC_ContainerNum = "GATU0859042";
			Container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			SetVesselName("NYK PROVIDER");
			SetVoyage("1234");
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			Processor.Process(Notifications);
			AssertEquals("Date should NOT be set", ZDateTime.Empty, Container.JC_FCLOnBoardVessel);

			AssertNotification(@$"Incoming 1-Stop alert for Container GATU0859042 (20GP) is not matching the details found in CargoWise<i>One</i> for this Container. Due to Vessel/Lloyds/Voyage information a discrepancy has been recognised and this event has not been processed.<br /><br />

<b>edi<i>{Core.Constants.ProductName}</i> Details:</b><br />
<br />
Container Job#: " + JobNumber + @"<br />
Consignee: Multiple<br />
Consignor: Multiple<br />
Vessel Name: NYK PROVIDER<br />
Lloyds No: 7807263<br />
Voyage Number: 1234<br />
Load: AUMEL<br />
Discharge: AUSYD<br />

<br /><br />

<b>1-Stop Message:</b><br />
<br />
<br />
Type 			: 1-STOP NOTIFY <br />
Message ID 		: 376530<br />
Event Location 	: XXXXX<br />
Vessel/Container 	: GATU0859042<br />
Event Type 		: Export Pre-Advise<br />
Event Date 		: 2006/09/06<br />
Event Time 		: 10:31 AM<br />
Information 	: " + Container.PK.ToString().ToLower().Replace("-", "") + @"<br />
Vessel Name 	: ARIAKE<br />
Lloyds No 		: 9294159<br />
Voyage Number 	: 0613S<br />
ETD from Load Port: 2006/09/10 18:52 PM from NZAKL<br />
ETA at Discharge Port: 2006/09/15 07:00 AM at AUBNE<br />
<br />      </td>
    </tr>
    <tr>
      <td><img src=""cid:Footer.jpg"" alt=""Footer Image"" /></td>
    </tr>
  </table>
</body>
</html>
");
		}

		public void TestProcess_VesselMismatchNotification_DifferentContainerNumber()
		{
			CreateContainerNotificationEmail(Factory, OneStopConstants.ContainerEventTypes.LoadOnVessel, "XXXXX");

			SetContainerDischargePort("AUMEL");
			SetContainerLoadPort("INBOM");
			Container.JC_ContainerNum = "CHANGED";
			Container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			SetVesselName("NYK PROVIDER");
			SetVoyage("1234");
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			Processor.Process(Notifications);
			AssertEquals("Date should NOT be set", ZDateTime.Empty, Container.JC_FCLOnBoardVessel);

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestProcess_VesselMismatchNotification()
		{
			CreateContainerNotificationEmail(Factory, OneStopConstants.ContainerEventTypes.LoadOnVessel, "XXXXX");

			SetContainerDischargePort("AUMEL");
			SetContainerLoadPort("INBOM");
			Container.JC_ContainerNum = "GATU0859042";
			Container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			SetVesselName("NYK PROVIDER");
			SetVoyage("1234");
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			Processor.Process(Notifications);
			AssertEquals("Date should NOT be set", ZDateTime.Empty, Container.JC_FCLOnBoardVessel);

			AssertNotification(@$"Incoming 1-Stop alert for Container GATU0859042 (20GP) is not matching the details found in CargoWise<i>One</i> for this Container. Due to Vessel/Lloyds/Voyage information a discrepancy has been recognised and this event has not been processed.<br /><br />

<b>edi<i>{Core.Constants.ProductName}</i> Details:</b><br />
<br />
Container Job#: " + JobNumber + @"<br />
Consignee: Multiple<br />
Consignor: Multiple<br />
Vessel Name: NYK PROVIDER<br />
Lloyds No: 7807263<br />
Voyage Number: 1234<br />
Load: INBOM<br />
Discharge: AUMEL<br />

<br /><br />

<b>1-Stop Message:</b><br />
<br />
<br />
Type 			: 1-STOP NOTIFY <br />
Message ID 		: 376530<br />
Event Location 	: XXXXX<br />
Vessel/Container 	: GATU0859042<br />
Event Type 		: Load on Vessel<br />
Event Date 		: 2006/09/06<br />
Event Time 		: 10:31 AM<br />
Information 	: " + Container.PK.ToString().ToLower().Replace("-", "") + @"<br />
Vessel Name 	: ARIAKE<br />
Lloyds No 		: 9294159<br />
Voyage Number 	: 0613S<br />
ETD from Load Port: 2006/09/10 18:52 PM from NZAKL<br />
ETA at Discharge Port: 2006/09/15 07:00 AM at AUBNE<br />
<br />      </td>
    </tr>
    <tr>
      <td><img src=""cid:Footer.jpg"" alt=""Footer Image"" /></td>
    </tr>
  </table>
</body>
</html>
");
		}

		public void TestProcess_VesselMismatchNotification_ConsigneeConsignor()
		{
			CreateContainerNotificationEmail(Factory, OneStopConstants.ContainerEventTypes.LoadOnVessel, "XXXXX");

			SetContainerDischargePort("AUMEL");
			SetConsigneeConsignorsIdentical(Container);
			Container.JC_ContainerNum = "GATU0859042";
			Container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			SetVesselName("NYK PROVIDER");
			SetVoyage("1234");
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			Processor.Process(Notifications);
			AssertEquals("Date should NOT be set", ZDateTime.Empty, Container.JC_FCLOnBoardVessel);

			AssertNotification(@$"Incoming 1-Stop alert for Container GATU0859042 (20GP) is not matching the details found in CargoWise<i>One</i> for this Container. Due to Vessel/Lloyds/Voyage information a discrepancy has been recognised and this event has not been processed.<br /><br />

<b>edi<i>{Core.Constants.ProductName}</i> Details:</b><br />
<br />
Container Job#: " + JobNumber + @"<br />
Consignee: ORG123 - Organization 123 - BR1<br />
Consignor: ZUB987 - Organization 987 - BR2<br />
Vessel Name: NYK PROVIDER<br />
Lloyds No: 7807263<br />
Voyage Number: 1234<br />
Load: <br />
Discharge: AUMEL<br />

<br /><br />

<b>1-Stop Message:</b><br />
<br />
<br />
Type 			: 1-STOP NOTIFY <br />
Message ID 		: 376530<br />
Event Location 	: XXXXX<br />
Vessel/Container 	: GATU0859042<br />
Event Type 		: Load on Vessel<br />
Event Date 		: 2006/09/06<br />
Event Time 		: 10:31 AM<br />
Information 	: " + Container.PK.ToString().ToLower().Replace("-", "") + @"<br />
Vessel Name 	: ARIAKE<br />
Lloyds No 		: 9294159<br />
Voyage Number 	: 0613S<br />
ETD from Load Port: 2006/09/10 18:52 PM from NZAKL<br />
ETA at Discharge Port: 2006/09/15 07:00 AM at AUBNE<br />
<br />      </td>
    </tr>
    <tr>
      <td><img src=""cid:Footer.jpg"" alt=""Footer Image"" /></td>
    </tr>
  </table>
</body>
</html>
");
		}

		void AssertNotification(string expected)
		{
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			string body = Env.OutgoingMailManager.EmailsCreated[0].Body;
			AssertEquals("Header and footer banners should be attached", 2, Env.OutgoingMailManager.EmailsCreated[0].Attachments.Count);

			int startIndex = body.IndexOf("incoming", StringComparison.InvariantCultureIgnoreCase);
			int endIndex = body.IndexOf("You have received this email");
			string bodyCut = body.Substring(startIndex, endIndex - startIndex);

			AssertMultilineASCIIEquals("",
				expected,
				bodyCut);
		}

		public void TestProcess_WhenLloydsAndVoyageSpecifiedOnly()
		{
			EmailDef email = new EmailDef();
			email.FromAddress = "helpdesk@1-stop.biz";
			email.AddRecipientForUserCommunication("clinton@edi.com.au");
			email.Subject = "1-STOP NOTIFY*376530*XXXXX*GATU0859042*Load on Vessel*" + Container.PK.ToString().Replace("-", "") + "*2006/09/06 10:31 AM*NotAnExistingVesselName*9999998*0613S*ETA 2006/09/15 07:00 AM";
			email.Body = @"
Type 			: 1-STOP NOTIFY 
Message ID 		: 376530
Event Location 	: XXXXX
Vessel/Container 	: GATU0859042
Event Type 		: Load on Vessel
Event Date 		: 2006/09/06
Event Time 		: 10:31 AM
Information 	: " + Container.PK.ToString().Replace("-", "") + @"
Vessel Name 	: NotAnExistingVesselName
Lloyds No 		: 9999998
Voyage Number 	: 0613S
ETA at Disharge Port: 2006/09/15 07:00 AM at AUBNE";

			Env.OutgoingMailManager.Create(Factory, email);
			MailItem mailItem = LoadMailItem(email);
			mailItem.MI_Direction = MailDirection.Receive;

			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "9999998";
			vessel.RV_Name = "TestVessel";

			RefVessel decoyVessel = Factory.New<RefVessel>();
			decoyVessel.RV_LloydsNumber = "9999998";
			decoyVessel.RV_Name = "DecoyVesselName";

			SetVesselName(vessel.RV_Name);
			SetVoyage("0613S");
			Factory.Save();

			Processor.Process(Notifications);
			AssertEquals("Date should be set", new ZDateTime(2006, 9, 6, 10, 31, 0), Container.JC_FCLOnBoardVessel);
			AssertEquals("The mail item should be marked 'processed'", MailStatus.Processed, mailItem.MI_Status);
		}

		public void TestProcess_DoesntThrowKeyNotFoundException()
		{
			string emailBody = @"
Type 			: 1-STOP NOTIFY 
Message ID 		: 3826628
Event Location 	: ASLPB
Vessel/Container 	: FSCU4624811
Event Type 		: Gate Out
Event Date 		: 2007/10/15
Event Time 		: 23:43 PM
Information 	: ad377398578c40b390d8a9682e7d28d3";
			MailItem mailItem = CreateOneStopNotificationEmail(emailBody);
			Processor.Process(Notifications);
			AssertEquals("The mail item should be marked 'processed'", MailStatus.Processed, mailItem.MI_Status);
		}

		public void TestParseEmailBody()
		{
			string emailBody = @"
Type =09=09=09: 1-STOP NOTIFY 
Message ID 	=09: 3646774
Event Location =09: ASLFR
Vessel/Container =09: MEDU2168640
Event Type 	=09: Export Pre-Advise
Event Date 	=09: 2007/10/19
Event Time 	=09: 10:46 AM
Information =09: b61c89a68aba4cecbbbcbd69a5a91878
Vessel Name =09: MSC LONDON
Lloyds No =09=09: 8502884
Voyage Number =09: 46R
ETD from Load Port: 2007/10/27 23:00 PM from AUFRE

";
			MailItem mailItem = CreateOneStopNotificationEmail(emailBody);
			Processor.Process(Notifications);
			AssertEquals("The mail item should be marked 'processed'", MailStatus.Processed, mailItem.MI_Status);
		}

		public void TestParseDateTime()
		{
			string emailBody = @"
Type 			: 1-STOP NOTIFY 
Message ID 		: 3583281
Event Location 	: ASES1
Vessel/Container 	: INBU3796457
Event Type 		: Export Pre-Advise
Event Date 		: 2007/10/23
Event Time 		: 13:43 PM
Information 	: 73e21d0fba4c4b8d885766f0320e9b3a
Vessel Name 	: SANTA ELENA 1
Lloyds No 		: 9113616
Voyage Number 	: 024N
ETD from Load Port: 2007/10/27 23:00 PM from AUMEL
";
			MailItem mailItem = CreateOneStopNotificationEmail(emailBody);
			Processor.Process(Notifications);
			AssertEquals("The mail item should be marked 'processed'", MailStatus.Processed, mailItem.MI_Status);
		}

		MailItem CreateOneStopNotificationEmail(string emailBody)
		{
			EmailDef email = CreateContainerNotificationEmail(OneStopConstants.ContainerEventTypes.GateIn, ZString.Empty);
			email.Body = emailBody;
			Env.OutgoingMailManager.Create(Factory, email);
			MailItem mailItem = LoadMailItem(email);
			mailItem.MI_Direction = MailDirection.Receive;
			Factory.Save();
			return mailItem;
		}

		public void TestProcess_MarkContainerEventsExpiredResponseEmailsAsProcessed()
		{
			EmailDef email = new EmailDef();
			email.FromAddress = "helpdesk@1-stop.biz";
			email.AddRecipientForUserCommunication("clinton@edi.com.au");
			email.Subject = "1-STOP ALERT*etc";

			Env.OutgoingMailManager.Create(Factory, email);
			MailItem mailItem = LoadMailItem(email);
			mailItem.MI_Direction = MailDirection.Receive;
			Factory.Save();

			Processor.Process(Notifications);
			AssertEquals("Container Events Expired alert email should always be marked as processed", MailStatus.Processed, mailItem.MI_Status);
		}

		public void TestProcess_UnExpectedFieldCount()
		{
			string emailBody = @"
Type 			: 1-STOP NOTIFY 
Message ID 		: 3583281
Event Location 	: ASES1
Vessel/Container 	: INBU3796457
Event Type 		: Export Pre-Advise
Event Date 		: 2007/10/23
";

			try
			{
				MailItem mailItem = CreateOneStopNotificationEmail(emailBody);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				Processor.Process(Notifications);

				AssertEquals("The mail item should be marked 'failed' after it is processed", MailStatus.Failed, mailItem.MI_Status);
				AssertMultilineASCIIEquals(@"Error: Column count mismatch (Unexpected field count processing 1-stop message: '
Type 			: 1-STOP NOTIFY 
Message ID 		: 3583281
Event Location 	: ASES1
Vessel/Container 	: INBU3796457
Event Type 		: Export Pre-Advise
Event Date 		: 2007/10/23
')",
					Notifications.AsString);

				AssertNotification(@"incoming ComTrac Container event messages from 1-Stop could not be processed because they were received in an unexpected format. 
Please check your email POP3 configuration, or contact 1-Stop and ask them to investigate their ""Alert Service Notification"" emails.      </td>
    </tr>
    <tr>
      <td><img src=""cid:Footer.jpg"" alt=""Footer Image"" /></td>
    </tr>
  </table>
</body>
</html>");
			}
			finally
			{
				AssertEquals("The mailbody text should be added to exception text", true, ErrorReporter.LastMessageReported.Contains(emailBody));
				ErrorReporter.Clear();
			}
		}

		protected abstract string JobNumber { get; }
		protected abstract string PropagatedReference { get; }
		protected abstract CommonContainer NewContainer();
		protected abstract void SetVesselName(ZString vesselName);
		protected abstract void SetVoyage(ZString voyage);

		#region Notifying Error Conditions

		public void TestSendEmailIfOneStopLocationUnregistered_ForGateIn()
		{
			TestSendEmailIfOneStopLocationUnregistered_ForGateInOrOut(OneStopConstants.ContainerEventTypes.GateIn);
		}

		public void TestSendEmailIfOneStopLocationUnregistered_ForGateOut()
		{
			TestSendEmailIfOneStopLocationUnregistered_ForGateInOrOut(OneStopConstants.ContainerEventTypes.GateOut);
		}

		void TestSendEmailIfOneStopLocationUnregistered_ForGateInOrOut(ZString eventType)
		{
			CreateRefPremiseCodeAndSave("12345", "1-Stop Location for Testing", true, true);
			CreateContainerNotificationEmail(Factory, eventType, "12345");

			SetVesselName("ARIAKE");
			SetVoyage("0613S");
			Factory.Save();

			Processor.Process(Notifications);
			MailItem mailItem = Factory.LoadTop1<MailItem>(new ZQuery(MailDBItemsSchema.MI_Subject, eventType + " Unregistered 1-Stop Location Notification"));
			AssertEquals("Recipient", OneStopNotificationGroup.Staff[0].GS_EmailAddress, mailItem.MailRecipients[0].EmailAddress);
			AssertEquals("ContentType", "HTM", mailItem.MI_ContentType);

			string expectedBody = GetExpectedUnregisteredOneStopLocationEmailBody(eventType);

			AssertMultilineASCIIEquals("Body", expectedBody, mailItem.MI_Body);
			AssertEquals("There should be 2 attachments", 2, mailItem.MailAttachments.Count);
			AssertEquals("Banner.jpg", mailItem.MailAttachments[0].MA_FileName);
			AssertEquals("Footer.jpg", mailItem.MailAttachments[1].MA_FileName);
		}

		[ExpectNoExceptions]
		public void TestSendEmailIfOneStopLocationUnregistered_WhenNoStaffRecipientsInNotificationGroup()
		{
			OneStopNotificationGroup.Staff.RemoveAll();
			Factory.Save();

			CreateRefPremiseCodeAndSave("12345", "1-Stop Location for Testing", true, true);
			CreateContainerNotificationEmail(Factory, OneStopConstants.ContainerEventTypes.GateIn, "12345");

			SetVesselName("ARIAKE");
			SetVoyage("0613S");
			Factory.Save();
			Processor.Process(Notifications);
		}

		public void TestSendEmailIfStopLocationUnregistered_EmailNotSentTwiceForSameLocationToPreventSpam()
		{
			SetVesselName("ARIAKE");
			SetVoyage("0613S");
			Factory.Save();

			CreateContainerNotificationEmail(Factory, OneStopConstants.ContainerEventTypes.GateIn, "12345");
			Factory.Save();
			Processor.Process(Notifications);
			MailItem mailItem = Factory.LoadTop1<MailItem>(new ZQuery(MailDBItemsSchema.MI_Subject, SQLComparisonOperator.Contains, "Unregistered 1-Stop Location Notification"));
			AssertNotNull("Unregistered 1-Stop location email sent", mailItem);
			mailItem.Delete();

			CreateContainerNotificationEmail(Factory, OneStopConstants.ContainerEventTypes.GateIn, "12345");
			Factory.Save();
			Processor.Process(Notifications);
			mailItem = Factory.LoadTop1<MailItem>(new ZQuery(MailDBItemsSchema.MI_Subject, SQLComparisonOperator.Contains, "Unregistered 1-Stop Location Notification"));
			AssertNull("Unregistered 1-Stop location email not sent a second time for the same location", mailItem);
		}

		public void TestSendEmailIfOneStopLocationReportedByDescription()
		{
			CreateContainerNotificationEmail(Factory, OneStopConstants.ContainerEventTypes.LoadOnVessel, "A location description");
			Factory.Save();

			Processor.Process(Notifications);
			MailItem mailItem = Factory.LoadTop1<MailItem>(new ZQuery(MailDBItemsSchema.MI_Subject, SQLComparisonOperator.Contains, "configuration problem"));
			AssertEquals("Recipient", OneStopNotificationGroup.Staff[0].GS_EmailAddress, mailItem.MailRecipients[0].EmailAddress);
			AssertEquals("ContentType", "HTM", mailItem.MI_ContentType);
			AssertEquals("Subject", "1-Stop Container Event Alert Service configuration problem", mailItem.MI_Subject);
		}

		#endregion

		#region Implementation

		Guid oldOneStopNotificationGroup;

		CommonContainer Container
		{
			get
			{
				if (container == null)
				{
					container = NewContainer();
					container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
					container.JC_ContainerNum = "GATU0859042";
				}
				return container;
			}
		}
		CommonContainer container;

		protected abstract void SetContainerDischargePort(string portCode);
		protected abstract void SetContainerLoadPort(string portCode);

		protected abstract void SetConsigneeConsignorsIdentical(CommonContainer container);

		NotificationBuffer Notifications
		{
			get
			{
				if (notifications == null)
				{
					notifications = new NotificationBuffer();
				}
				return notifications;
			}
		}
		NotificationBuffer notifications;

		GlbGroup OneStopNotificationGroup
		{
			get
			{
				if (oneStopNotificationGroup == null)
				{
					oneStopNotificationGroup = Factory.New<GlbGroup>();
					oneStopNotificationGroup.GG_Code = "1SN";

					GlbStaff recipient = oneStopNotificationGroup.Staff.AddNew();
					recipient.GS_EmailAddress = "1stop_notifications@edi.com.au";
					recipient.GS_Code = "ZAC";
				}
				return oneStopNotificationGroup;
			}
		}
		GlbGroup oneStopNotificationGroup;

		OneStopContainerInformationProcessor Processor
		{
			get
			{
				if (processor == null)
				{
					processor = new OneStopContainerInformationProcessor();
				}
				return processor;
			}
		}
		OneStopContainerInformationProcessor processor;

		MailItem CreateContainerNotificationEmail(BusinessObjectFactory factory, ZString eventType, ZString locationCode)
		{
			EmailDef email = CreateContainerNotificationEmail(eventType, locationCode);
			Env.OutgoingMailManager.Create(factory, email);
			MailItem mailItem = LoadMailItem(email);
			mailItem.MI_Direction = MailDirection.Receive;
			return mailItem;
		}

		MailItem CreateContainerNotificationEmail(BusinessObjectFactory factory, ZString eventType, ZString locationCode, ZString eventDate)
		{
			var email = new EmailDef();
			email.FromAddress = "helpdesk@1-stop.biz";
			email.AddRecipientForUserCommunication("clinton@edi.com.au");
			email.Subject = "1-STOP NOTIFY*376530*" + locationCode + "*GATU0859042*" + eventType + "*" + Container.PK.ToString().Replace("-", "") + "*2006/09/06 10:31 AM*ARIAKE*9294159*0613S*ETA 2006/09/15 07:00 AM";
			email.Body = $@"
Type 			: 1-STOP NOTIFY 
Message ID 		: 376530
Event Location 	: ADLMUE
Vessel/Container 	: GATU0859042 
Event Type 		: {eventType}
Event Date 		: {eventDate}
Event Time 		: 10:31 AM
Information 	: {Container.PK.ToString().Replace("-", "")}
Vessel Name 	: ARIAKE
Lloyds No 		: 9294159
Voyage Number 	: 0613S
ETD from Load Port: 2006/09/10 18:52 PM from NZAKL
ETA at Discharge Port: 2006/09/15 07:00 AM at AUBNE
";
			Env.OutgoingMailManager.Create(factory, email);
			MailItem mailItem = LoadMailItem(email);
			mailItem.MI_Direction = MailDirection.Receive;
			return mailItem;
		}

		EmailDef CreateContainerNotificationEmail(ZString eventType, ZString locationCode)
		{
			EmailDef email = new EmailDef();
			email.FromAddress = "helpdesk@1-stop.biz";
			email.AddRecipientForUserCommunication("clinton@edi.com.au");
			email.Subject = "1-STOP NOTIFY*376530*" + locationCode + "*GATU0859042*" + eventType + "*" + Container.PK.ToString().Replace("-", "") + "*2006/09/06 10:31 AM*ARIAKE*9294159*0613S*ETA 2006/09/15 07:00 AM";
			email.Body = @"
Type 			: 1-STOP NOTIFY 
Message ID 		: 376530
Event Location 	: " + locationCode + @"
Vessel/Container 	: GATU0859042
Event Type 		: " + eventType + @"
Event Date 		: 2006/09/06
Event Time 		: 10:31 AM
Information 	: " + Container.PK.ToString().Replace("-", "") + @"
Vessel Name 	: ARIAKE
Lloyds No 		: 9294159
Voyage Number 	: 0613S
ETD from Load Port: 2006/09/10 18:52 PM from NZAKL
ETA at Discharge Port: 2006/09/15 07:00 AM at AUBNE
";
			return email;
		}

		const bool IsContainerTerminal = true;
		const bool IsContainerYard = true;

		OrgHeader CreateOneStopOrganisationCodeAndSave(ZString code, ZString description, bool isContainerTerminal, bool isContainerYard, string closestPort = "AUSYD")
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			if (isContainerTerminal)
			{
				organisation.OH_IsSeaCTO = true;
			}
			if (isContainerYard)
			{
				organisation.OH_IsContainerYard = true;
			}

			organisation.OH_RL_NKClosestPort = closestPort;

			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			cusCode.OK_CustomsRegNo = code;
			Factory.Save();
			return organisation;
		}

		RefPremisesGateCode CreateRefPremiseCodeAndSave(ZString premiseGateCode, ZString premiseGateDescription, bool isContainerTerminal, bool isContainerYard)
		{
			RefPremisesGateCode premiseCode = Factory.New<RefPremisesGateCode>();
			premiseCode.R5_DataProvider = PremiseGateCodeDataProviderList.Codes.OneStop;
			premiseCode.R5_PremisesGateCode = premiseGateCode;
			premiseCode.R5_PremisesGateDescription = premiseGateDescription;
			premiseCode.R5_IsWharf = isContainerTerminal;
			premiseCode.R5_IsContainerYard = isContainerYard;

			Factory.Save();
			return premiseCode;
		}

		string GetExpectedUnregisteredOneStopLocationEmailBody(ZString eventType)
		{
			string expectedBody = "";
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				expectedBody = resourceRetriever.GetString("Enterprise.Freight.SailingScheduleDataVendor.Test.AU_NZ.ContainerEvents.TestFiles.ExpectedOneStopLocationCodeNotRegistered.htm");
			}
			expectedBody = expectedBody.Replace("(*HtmlStyleSheet*)", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
			expectedBody = expectedBody.Replace("(*ContainerEventType*)", eventType);
			expectedBody = expectedBody.Replace("(*OneStopLocationCode*)", "12345");

			return expectedBody;
		}

		MailItem LoadMailItem(EmailDef email)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(MailDBItemsSchema.MI_From, SQLComparisonOperator.Contains, email.FromAddress);
			query.AddToFilter(MailDBItemsSchema.MI_Subject, email.Subject);
			return Factory.LoadTop1<MailItem>(query);
		}

		protected override void SetUp()
		{
			base.SetUp();
			OneStopNotificationGroup.Factory.Save();
			oldOneStopNotificationGroup = FreightDataRegistry.Instance.OneStopNotificationGroup.Value;
			FreightDataRegistry.Instance.OneStopNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OneStopNotificationGroup.PK.ToGuid());
		}

		protected override void TearDown()
		{
			base.TearDown();
			FreightDataRegistry.Instance.OneStopNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldOneStopNotificationGroup);
		}

		#endregion
	}
}
