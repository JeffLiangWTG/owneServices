using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CargoReportWorkflowTestCase : TestCaseWithFactory
	{
		[TestDate(2008, 8, 6)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestMileStoneScheduledDateDoesNotUpdateIfShipmentDoesNotHaveScheduledCargoReportDate()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = Enterprise.Core.Constants.CountryCodes.Australia + "SYD";
			GlbBranch.CurrentBranch.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			ProcessTask mileStone = shipment.WorkflowItems.Milestones.AddNew();
			mileStone.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment).CargoReportAcceptedEvent.Code;
			mileStone.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2008, 8, 8)));
			Factory.Save();

			cargoReportWorkflow.ProcessOneImportShipment(shipment);
			AssertEquals(new ZDateTime(2008, 8, 8), mileStone.P9_ScheduledDate.ToZDateTime());

			cargoReportWorkflow.Logger = new LoggingInformation();
			cargoReportWorkflow.ExecuteBatch();
			AssertEquals(new ZDateTime(2008, 8, 8), mileStone.P9_ScheduledDate.ToZDateTime());
		}

		public void TestProcessOneImportShipment()
		{
			GlbGroup group1 = Factory.New<GlbGroup>();
			group1.GG_Code = "G1";
			CustomsDataRegistry.Instance.GroupToSendLateAirCargoReportAndWarningsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group1.PK.ToGuid());
			GlbGroup group2 = Factory.New<GlbGroup>();
			group2.GG_Code = "G2";
			CustomsDataRegistry.Instance.GroupToSendLateSeaCargoReportAndWarningsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group2.PK.ToGuid());

			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			ZDateTime testingETA1 = new ZDateTime(2008, 1, 15, 1, 2, 0);
			shipment1.JS_E_ARV = testingETA1;
			ProcessTask mileStone = shipment1.WorkflowItems.Milestones.AddNew();
			mileStone.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment1).CargoReportAcceptedEvent.Code;
			mileStone.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2007, 12, 31, 2, 2, 0)));
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			ZDateTime testingETA2 = new ZDateTime(2008, 1, 16, 4, 5, 0);
			shipment2.JS_E_ARV = testingETA2;

			ZInt exstingMilestoneCount = shipment1.WorkflowItems.Milestones.Count;
			cargoReportWorkflow.ProcessOneImportShipment(shipment1);
			AssertEquals("No new milestone", exstingMilestoneCount, shipment1.WorkflowItems.Milestones.Count);
			AssertEquals("Date updated", testingETA1.AddHours(-24), mileStone.P9_ScheduledDate.ToZDateTime());

			exstingMilestoneCount = shipment2.WorkflowItems.Milestones.Count;
			cargoReportWorkflow.ProcessOneImportShipment(shipment2);
			AssertEquals("One new milestone", exstingMilestoneCount + 1, shipment2.WorkflowItems.Milestones.Count);
			ProcessTask[] cargoReportAcceptedMilestones = ForwardingShipmentProcessTask.CargoReportAcceptedMilestones(shipment2);
			AssertEquals("One selected", 1, cargoReportAcceptedMilestones.Length);
			AssertEquals("Description", "Cargo Report Accepted", cargoReportAcceptedMilestones[0].P9_Description);
			AssertEquals("Event", ((IParentForCargoReporter)shipment2).CargoReportAcceptedEvent.Code, cargoReportAcceptedMilestones[0].P9_SE_NKMilestoneEvent);
			AssertEquals("Date", testingETA2.AddHours(-48), cargoReportAcceptedMilestones[0].P9_ScheduledDate.ToZDateTime());
			AssertEquals("Group", group2.PK, cargoReportAcceptedMilestones[0].P9_GG_AssignedGroup);
			Assert("Published", mileStone.P9_IsPublished);
		}

		public void TestProcessOneImportShipment_InvalidRegistryItemValues()
		{
			CustomsDataRegistry.Instance.GroupToSendLateAirCargoReportAndWarningsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZGuid.NewZGuid().ToGuid());
			CustomsDataRegistry.Instance.GroupToSendLateSeaCargoReportAndWarningsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZGuid.NewZGuid().ToGuid());

			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			ZDateTime testingETA1 = new ZDateTime(2008, 1, 15, 1, 2, 0);
			shipment1.JS_E_ARV = testingETA1;
			ProcessTask mileStone = shipment1.WorkflowItems.Milestones.AddNew();
			mileStone.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment1).CargoReportAcceptedEvent.Code;
			mileStone.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2007, 12, 31, 2, 2, 0)));
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			ZDateTime testingETA2 = new ZDateTime(2008, 1, 16, 4, 5, 0);
			shipment2.JS_E_ARV = testingETA2;

			ZInt exstingMilestoneCount = shipment1.WorkflowItems.Milestones.Count;
			cargoReportWorkflow.ProcessOneImportShipment(shipment1);
			AssertEquals("No new milestone", exstingMilestoneCount, shipment1.WorkflowItems.Milestones.Count);
			AssertEquals("Date updated", testingETA1.AddHours(-24), mileStone.P9_ScheduledDate.ToZDateTime());

			exstingMilestoneCount = shipment2.WorkflowItems.Milestones.Count;
			cargoReportWorkflow.ProcessOneImportShipment(shipment2);
			AssertEquals("One new milestone", exstingMilestoneCount + 1, shipment2.WorkflowItems.Milestones.Count);
			ProcessTask[] cargoReportAcceptedMilestones = ForwardingShipmentProcessTask.CargoReportAcceptedMilestones(shipment2);
			AssertEquals("One selected", 1, cargoReportAcceptedMilestones.Length);
			AssertEquals("Description", "Cargo Report Accepted", cargoReportAcceptedMilestones[0].P9_Description);
			AssertEquals("Event", ((IParentForCargoReporter)shipment2).CargoReportAcceptedEvent.Code, cargoReportAcceptedMilestones[0].P9_SE_NKMilestoneEvent);
			AssertEquals("Date", testingETA2.AddHours(-48), cargoReportAcceptedMilestones[0].P9_ScheduledDate.ToZDateTime());
			AssertEquals("Group", ZGuid.Empty, cargoReportAcceptedMilestones[0].P9_GG_AssignedGroup);
		}

		public void TestProcessOneImportShipment_WithoutETA()
		{
			GlbGroup group1 = Factory.New<GlbGroup>();
			group1.GG_Code = "G1";
			CustomsDataRegistry.Instance.GroupToSendLateAirCargoReportAndWarningsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group1.PK.ToGuid());
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals((ZInt)0, shipment1.WorkflowItems.Milestones.Count);
			cargoReportWorkflow.ProcessOneImportShipment(shipment1);
			AssertEquals("A new milestone created", (ZInt)1, shipment1.WorkflowItems.Milestones.Count);
			var mileStone = shipment1.WorkflowItems.Milestones[0];
			AssertEquals("No date on milestone", ZDateTime.Empty, mileStone.P9_ScheduledDate.ToZDateTime());
		}

		[TestDate(2008, 01, 20, 11, 11, 11)]
		public void TestDoDailyProcessing()
		{
			DoDailyProcessingTestCore();
			AssertEquals("No retry", 0, cargoReportWorkflow.retryCount);
		}

		[TestDate(2008, 01, 20, 11, 11, 11)]
		public void TestDoDailyProcessingWithConcurrencyError()
		{
			cargoReportWorkflow.ForceConcurrencyError = true;
			DoDailyProcessingTestCore();
			AssertEquals("Was successful on retry", 1, cargoReportWorkflow.retryCount);
			Assert(cargoReportWorkflow.Logger.UserLogStrings[1].Replace("\n", "").Replace("\r", "").Contains("Late and Pending Cargo Report save error on first try: **CONCURRENCY Error Saving Record **"));
		}

		public void DoDailyProcessingTestCore()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(Culture.Default))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

				RefVessel testVessel = RefVessel.New(Factory);
				testVessel.RV_Code = "VESSEL NAME";
				testVessel.RV_LloydsNumber = "1111111";

				// shipment 1 is open and manualy actioned
				ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
				shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				shipment1.JS_RL_NKDestination = "AUSYD";
				shipment1.JS_UniqueConsignRef = "S00000001";
				var testingETA1 = new ZDateTimeOffset(new ZDateTime(2008, 1, 15, 1, 2, 0));
				shipment1.JS_E_ARV = testingETA1.ToZDateTime();
				ProcessTask mileStone1 = shipment1.WorkflowItems.Milestones.AddNew();
				mileStone1.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment1).CargoReportAcceptedEvent.Code;
				ProcessTask mileStoneException1 = mileStone1.CreateMilestoneException();
				AssertEquals(((IParentForCargoReporter)shipment1).CargoReportAcceptedEvent.Code, mileStoneException1.TriggerConditions.TriggerEventCode);
				mileStoneException1.IsExceptionActioned = true;
				ForwardingConsol consol = shipment1.Consols.AddNew();
				consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				consol.JK_RL_NKPortOfFirstArrival = "AUSYD";
				consol.JK_DatePortOfFirstArrival = testingETA1.ToZDateTime();
				consol.JK_RL_NKLoadPort = "SGSIN";
				consol.JK_RL_NKDischargePort = "AUSYD";
				Transport transport = consol.Transports[0];
				transport.JW_RL_NKDiscPort = "AUSYD";
				transport.JW_VoyageFlight = "1234";
				transport.JW_Vessel = "VESSEL NAME";

				// shipment 2 open is overdue
				ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
				shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				shipment2.JS_RL_NKDestination = "AUMEL";
				shipment2.JS_UniqueConsignRef = "S00000002";
				var testingETA2 = new ZDateTimeOffset(new ZDateTime(2008, 1, 16, 1, 2, 0));
				shipment2.JS_E_ARV = testingETA2.ToZDateTime();
				ProcessTask mileStone2 = shipment2.WorkflowItems.Milestones.AddNew();
				mileStone2.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment2).CargoReportAcceptedEvent.Code;
				ProcessTask mileStoneException2 = mileStone2.CreateMilestoneException();
				AssertEquals(((IParentForCargoReporter)shipment2).CargoReportAcceptedEvent.Code, mileStoneException2.TriggerConditions.TriggerEventCode);

				// shipment 3 is open approaching overdue
				ForwardingShipment shipment3 = Factory.New<ForwardingShipment>();
				shipment3.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				shipment3.JS_RL_NKDestination = "AUSYD";
				shipment3.JS_UniqueConsignRef = "S00000003";
				var testingETA3 = new ZDateTimeOffset(new ZDateTime(2008, 1, 21, 1, 2, 0));
				shipment3.JS_E_ARV = testingETA3.ToZDateTime();
				ProcessTask mileStone3 = shipment3.WorkflowItems.Milestones.AddNew();
				mileStone3.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment3).CargoReportAcceptedEvent.Code;
				ProcessTask mileStoneException3 = mileStone3.CreateMilestoneException();
				AssertEquals(((IParentForCargoReporter)shipment3).CargoReportAcceptedEvent.Code, mileStoneException3.TriggerConditions.TriggerEventCode);

				// shipment 4 is open and not approaching overdue and so should not be reported
				ForwardingShipment shipment4 = Factory.New<ForwardingShipment>();
				shipment4.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				shipment4.JS_RL_NKDestination = "AUSYD";
				var testingETA4 = new ZDateTimeOffset(new ZDateTime(2008, 1, 23, 1, 2, 0));
				shipment4.JS_E_ARV = testingETA4.ToZDateTime();
				shipment4.JS_UniqueConsignRef = "S00000004";
				ProcessTask mileStone4 = shipment4.WorkflowItems.Milestones.AddNew();
				mileStone4.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment4).CargoReportAcceptedEvent.Code;

				// shipment 5 is closed as not overdue and is still not overdue and so should not be reported
				ForwardingShipment shipment5 = Factory.New<ForwardingShipment>();
				shipment5.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				shipment5.JS_RL_NKDestination = "AUSYD";
				shipment5.JS_E_ARV = testingETA4.ToZDateTime();
				shipment5.JS_UniqueConsignRef = "S00000005";
				ProcessTask mileStone5 = shipment5.WorkflowItems.Milestones.AddNew();
				mileStone5.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment5).CargoReportAcceptedEvent.Code;
				mileStone5.SetMilestoneScheduledDateForTest(testingETA1);
				mileStone5.SetMilestoneActualDateForTest(testingETA4);

				// shipment 6 is closed as not overdue (no exception) but now is overdue
				ForwardingShipment shipment6 = Factory.New<ForwardingShipment>();
				shipment6.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				shipment6.JS_RL_NKDestination = "AUSYD";
				shipment6.JS_E_ARV = testingETA4.ToZDateTime();
				shipment6.JS_UniqueConsignRef = "S00000006";
				ProcessTask mileStone6 = shipment6.WorkflowItems.Milestones.AddNew();
				mileStone6.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment6).CargoReportAcceptedEvent.Code;
				mileStone6.SetMilestoneScheduledDateForTest(testingETA1);
				mileStone6.SetMilestoneActualDateForTest(testingETA4.AddHours(1));

				// shipment 7 is closed as not overdue (exception without overdue reason) but is now overdue
				ForwardingShipment shipment7 = Factory.New<ForwardingShipment>();
				shipment7.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				shipment7.JS_RL_NKDestination = "AUSYD";
				shipment7.JS_E_ARV = testingETA4.ToZDateTime();
				shipment7.JS_UniqueConsignRef = "S00000007";
				ProcessTask mileStone7 = shipment7.WorkflowItems.Milestones.AddNew();
				mileStone7.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment7).CargoReportAcceptedEvent.Code;
				mileStone7.SetMilestoneScheduledDateForTest(testingETA1);
				mileStone7.SetMilestoneActualDateForTest(testingETA4.AddHours(1));
				ProcessTask mileStoneException7 = shipment7.WorkflowItems.Exceptions.AddNew();
				mileStoneException7.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment7).CargoReportAcceptedEvent.Code;
				mileStoneException7.IsExceptionActioned = true;

				// shipment 8 is closed as overdue and is still overdue and so should not be reported
				ForwardingShipment shipment8 = Factory.New<ForwardingShipment>();
				shipment8.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				shipment8.JS_RL_NKDestination = "AUSYD";
				shipment8.JS_E_ARV = testingETA4.ToZDateTime();
				shipment8.JS_UniqueConsignRef = "S00000008";
				ProcessTask mileStone8 = shipment8.WorkflowItems.Milestones.AddNew();
				mileStone8.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment8).CargoReportAcceptedEvent.Code;
				mileStone8.SetMilestoneScheduledDateForTest(testingETA1);
				mileStone8.SetMilestoneActualDateForTest(testingETA4.AddHours(1));
				ProcessTask mileStoneException8 = mileStone8.CreateMilestoneException();
				mileStoneException8.IsExceptionActioned = true;
				mileStoneException8.P9_Notes = ZBlob.FromAscii(ORtfTextUtil.TextToRtf("Some text"));

				// shipment 9 is closed as overdue but now is not overdue
				ForwardingShipment shipment9 = Factory.New<ForwardingShipment>();
				shipment9.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				shipment9.JS_RL_NKDestination = "AUSYD";
				shipment9.JS_E_ARV = testingETA4.ToZDateTime();
				shipment9.JS_UniqueConsignRef = "S00000009";
				ProcessTask mileStone9 = shipment9.WorkflowItems.Milestones.AddNew();
				mileStone9.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment9).CargoReportAcceptedEvent.Code;
				mileStone9.SetMilestoneScheduledDateForTest(testingETA1);
				mileStone9.SetMilestoneActualDateForTest(testingETA4);
				ProcessTask mileStoneException9 = mileStone9.CreateMilestoneException();
				mileStoneException9.IsExceptionActioned = true;
				mileStoneException9.P9_Notes = ZBlob.FromAscii(ORtfTextUtil.TextToRtf("Some text"));

				// shipment 10 is closed as not overdue but now is overdue, but scheduled date has not changed and so should not be reported
				ForwardingShipment shipment10 = Factory.New<ForwardingShipment>();
				shipment10.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				shipment10.JS_RL_NKDestination = "AUSYD";
				shipment10.JS_E_ARV = testingETA4.AddHours(shipment10.CargoReportAcceptedEventSafetyMargin).ToZDateTime();
				shipment10.JS_UniqueConsignRef = "S00000010";
				ProcessTask mileStone10 = shipment10.WorkflowItems.Milestones.AddNew();
				mileStone10.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment10).CargoReportAcceptedEvent.Code;
				mileStone10.SetMilestoneScheduledDateForTest(testingETA4);
				mileStone10.SetMilestoneActualDateForTest(testingETA4.AddHours(1));

				// shipment11 is open and has no scheduled date, should be ignored
				ForwardingShipment shipment11 = Factory.New<ForwardingShipment>();
				shipment11.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				shipment11.JS_RL_NKDestination = "AUSYD";
				shipment11.JS_UniqueConsignRef = "S00000011";
				ProcessTask mileStone11 = shipment11.WorkflowItems.Milestones.AddNew();
				mileStone11.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment11).CargoReportAcceptedEvent.Code;

				// shipment12 is domestic shipment, should be ignored
				ForwardingShipment shipment12 = Factory.New<ForwardingShipment>();
				shipment12.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				shipment12.JS_RL_NKOrigin = "AUSYD";
				shipment12.JS_RL_NKDestination = "AUFRE";
				shipment12.JS_E_ARV = testingETA4.ToZDateTime();
				shipment12.JS_UniqueConsignRef = "S00000012";
				Assert(shipment12.IsDomesticFreight);
				ProcessTask mileStone12 = shipment12.WorkflowItems.Milestones.AddNew();
				mileStone12.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment12).CargoReportAcceptedEvent.Code;
				mileStone12.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment12).CargoReportAcceptedEvent.Code;
				mileStone12.SetMilestoneScheduledDateForTest(testingETA1);
				mileStone12.SetMilestoneActualDateForTest(testingETA4);
				ProcessTask mileStoneException12 = mileStone12.CreateMilestoneException();
				mileStoneException12.IsExceptionActioned = true;
				mileStoneException12.P9_Notes = ZBlob.FromAscii(ORtfTextUtil.TextToRtf("Some text"));

				// shipment13 open is overdue like shipment2, but created over 1 year ago, should be ignored.
				var shipment13 = Factory.New<ForwardingShipment>();
				shipment13.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment13.JS_RL_NKDestination = "AUMEL";
				shipment13.JS_UniqueConsignRef = "S00000013";
				shipment13.JS_SystemCreateTimeUtc = new ZDateTime(2007, 1, 19, 23, 59, 59);
				var testingETA13 = new ZDateTimeOffset(new ZDateTime(2008, 1, 16, 1, 2, 0));
				shipment13.JS_E_ARV = testingETA13.ToZDateTime();
				var mileStone13 = shipment13.WorkflowItems.Milestones.AddNew();
				mileStone13.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment13).CargoReportAcceptedEvent.Code;
				mileStone13.CreateMilestoneException();

				// shipment14 open is overdue like shipment2, but JS_RL_NKDestination not stat with AU, should be ignored.
				var shipment14 = Factory.New<ForwardingShipment>();
				shipment14.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment14.JS_RL_NKDestination = "ZAJNB";
				shipment14.JS_UniqueConsignRef = "S00000014";
				var testingETA14 = new ZDateTimeOffset(new ZDateTime(2008, 1, 16, 1, 2, 0));
				shipment14.JS_E_ARV = testingETA14.ToZDateTime();
				var mileStone14 = shipment14.WorkflowItems.Milestones.AddNew();
				mileStone14.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment14).CargoReportAcceptedEvent.Code;
				mileStone14.CreateMilestoneException();

				Factory.Save();

				cargoReportWorkflow.Logger = new LoggingInformation();
				cargoReportWorkflow.ExecuteBatch();
				mileStone1.Reload();
				mileStone2.Reload();
				mileStone3.Reload();
				mileStone4.Reload();
				AssertEquals("Shipment1 milestone date is reset", testingETA1.AddHours(-48).ToZDateTime(), mileStone1.P9_ScheduledDate.ToZDateTime());
				AssertEquals("Shipment2 milestone date is reset", testingETA2.AddHours(-48).ToZDateTime(), mileStone2.P9_ScheduledDate.ToZDateTime());
				AssertEquals("Shipment3 milestone date is reset", testingETA3.AddHours(-48).ToZDateTime(), mileStone3.P9_ScheduledDate.ToZDateTime());
				AssertEquals("Shipment4 milestone date is reset", testingETA4.AddHours(-48).ToZDateTime(), mileStone4.P9_ScheduledDate.ToZDateTime());
				AssertMultilineASCIIEquals("Email body", expectedBody.Replace("''", "\""), cargoReportWorkflow.cargoReportExceptionsEmailForTesting.Body);
				AssertEquals("Email subject", "Late and Pending Shipment Cargo Report Notifications", cargoReportWorkflow.cargoReportExceptionsEmailForTesting.Subject);
			}
		}

		[TestDate(2008, 01, 20, 11, 11, 11)]
		public void TestDoDailyProcessingInBacth()
		{
			var saveCountWithoutBatching = 0;
			var saveCountWithBatching = 0;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_UniqueConsignRef = "S00000001";
			var testingETA = new ZDateTimeOffset(new ZDateTime(2008, 1, 21, 1, 2, 0));
			shipment.JS_E_ARV = testingETA.ToZDateTime();
			cargoReportWorkflow.Logger = new LoggingInformation();

			for (int i = 0; i < 101; i++)
			{
				var mileStone = shipment.WorkflowItems.Milestones.AddNew();
				mileStone.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment).CargoReportAcceptedEvent.Code;
				var mileStoneException = shipment.WorkflowItems.Exceptions.AddNew();
				mileStoneException.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment).CargoReportAcceptedEvent.Code;
			}
			Factory.Save();

			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(Culture.Default))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				using (CustomsDataRegistry.Instance.LateAndPendingCargoReportBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 101))
				{
					var saveCount = BusinessObjectFactory.GlobalSaveCount;
					cargoReportWorkflow.ExecuteBatch();
					saveCountWithoutBatching = BusinessObjectFactory.GlobalSaveCount - saveCount;
				}

				using (CustomsDataRegistry.Instance.LateAndPendingCargoReportBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
				{
					var saveCount = BusinessObjectFactory.GlobalSaveCount;
					cargoReportWorkflow.ExecuteBatch();
					saveCountWithBatching = BusinessObjectFactory.GlobalSaveCount - saveCount;
				}
			}

			AssertEquals("Since each batching creates 1 Factory, the number of Factoies created with batching(11 batches) should be 10 more than that with no batching.", 10, saveCountWithBatching - saveCountWithoutBatching);
		}

		[TestDate(2008, 01, 20, 11, 11, 11)]
		public void TestMilestoneExceptionMessageInEmail()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(Culture.Default))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var group = Factory.New<GlbGroup>();
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
				group.GG_Code = "TES";
				var milestoneError = string.Format("This email is sent to the group defined at Milestone -> '{0}'. Since there are no valid email addresses set up in this group this email has been sent to all users.", group.GG_Code);

				var allGroup = Factory.Load<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "ALL")).FirstOrDefault();

				var staff = Factory.New<GlbStaff>();
				staff.GS_IsActive = true;
				staff.GS_EmailAddress = "grouptest@wisetechglobal.com";
				staff.GS_Code = "TES";

				var groupLink = Factory.New<GlbGroupLink>();
				groupLink.GK_GG = allGroup.PK;
				groupLink.GK_GS = staff.PK;

				RefVessel testVessel = RefVessel.New(Factory);
				testVessel.RV_Code = "VESSEL NAME";
				testVessel.RV_LloydsNumber = "1111111";

				// shipment 1 is open and manualy actioned
				ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
				shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				shipment1.JS_RL_NKDestination = "AUSYD";
				shipment1.JS_UniqueConsignRef = "S00000001";
				var testingETA1 = new ZDateTimeOffset(new ZDateTime(2008, 1, 15, 1, 2, 0));
				shipment1.JS_E_ARV = testingETA1.ToZDateTime();
				ProcessTask mileStone1 = shipment1.WorkflowItems.Milestones.AddNew();
				mileStone1.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment1).CargoReportAcceptedEvent.Code;
				ProcessTask mileStoneException1 = shipment1.WorkflowItems.Exceptions.AddNew();
				mileStoneException1.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment1).CargoReportAcceptedEvent.Code;
				mileStoneException1.IsExceptionActioned = true;
				ForwardingConsol consol = shipment1.Consols.AddNew();
				consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				consol.JK_RL_NKPortOfFirstArrival = "AUSYD";
				consol.JK_DatePortOfFirstArrival = testingETA1.ToZDateTime();
				consol.JK_RL_NKLoadPort = "SGSIN";
				consol.JK_RL_NKDischargePort = "AUSYD";
				Transport transport = consol.Transports[0];
				transport.JW_RL_NKDiscPort = "AUSYD";
				transport.JW_VoyageFlight = "1234";
				transport.JW_Vessel = "VESSEL NAME";

				mileStone1.P9_GG_AssignedGroup = group.PK;
				Factory.Save();

				cargoReportWorkflow.Logger = new LoggingInformation();
				cargoReportWorkflow.ExecuteBatch();
				mileStone1.Reload();
				AssertEquals("Email contains expected exception", milestoneError, cargoReportWorkflow.cargoReportExceptionsEmailForTesting.FooterText);
			}
		}

		readonly string expectedBody =
string.Format(CultureInfo.CurrentCulture, @"<html xmlns=''http://www.w3.org/1999/xhtml''>
<head>
  <title>Late and Pending Shipment Cargo Report Notifications</title>
  <style type=''text/css''>
  <!--
  {0}
  -->
</style>
</head>
<body>
  <table border=''0'' cellpadding=''0'' cellspacing=''0'' bgcolor=''#FFFFFF''>
    <tr>
      <td><img src=''cid:Banner.jpg'' alt=''Banner Image'' /></td>
    </tr>
    <tr>
      <td>
        <br />
        <strong>Late and Pending Shipment Cargo Report Notifications as at 20-Jan-08 11:11</strong>
        <br />
        <br />
        <hr />
        <br />
        The following is a list of shipments that appear to NOT have had a Cargo Report lodged and the due date for reporting has expired.<br />
        <br />
        <br />
        <table class=''table'' cellpadding=''1'' cellspacing=''0'' border=''1'' bgcolor=''#FFFFFF''>
          <tr class=''tableheadings''>
            <th>
              <b>&nbsp;Shipment&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Vessel/Voyage or Flight&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Shipment First Port&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Shipment First Port ETA&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;CR Required By&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Comments&nbsp;</b>
            </th>
          </tr>
          <tr>
            <td>&nbsp;<a href=''(*ShipmentUrl*)''>S00000002</a>&nbsp;</td>
            <td>&nbsp;&nbsp;</td>
            <td>&nbsp;AUMEL&nbsp;</td>
            <td>&nbsp;16-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;16-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;!!! LATE.&nbsp;</td>
          </tr>
        </table>
        <br />
        <hr />
        <br />
        The following is a list of shipments that appear to NOT have had a Cargo Report lodged and the due date for reporting is approaching.<br />
        <br />
        <br />
        <table class=''table'' cellpadding=''1'' cellspacing=''0'' border=''1'' bgcolor=''#FFFFFF''>
          <tr class=''tableheadings''>
            <th>
              <b>&nbsp;Shipment&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Vessel/Voyage or Flight&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Shipment First Port&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Shipment First Port ETA&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;CR Required By&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Comments&nbsp;</b>
            </th>
          </tr>
          <tr>
            <td>&nbsp;<a href=''(*ShipmentUrl*)''>S00000003</a>&nbsp;</td>
            <td>&nbsp;&nbsp;</td>
            <td>&nbsp;AUSYD&nbsp;</td>
            <td>&nbsp;21-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;21-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;DUE DATE APPROACHING.&nbsp;</td>
          </tr>
        </table>
        <br />
        <hr />
        <br />
        The Cargo Report Accepted exception, on the following shipments, has been actioned
        but the milestone has not been closed (the Cargo Report Accepted event has not occurred).
        This has probably been caused by manually setting the 'Actioned' tick box on the exception.
        Please check these shipments and, if required, close the milestone by setting the actual
        date on the milestone.<br />
        <br />
        <br />
        <table class=''table'' cellpadding=''1'' cellspacing=''0'' border=''1'' bgcolor=''#FFFFFF''>
          <tr class=''tableheadings''>
            <th>
              <b>&nbsp;Shipment&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Vessel/Voyage or Flight&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Shipment First Port&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Shipment First Port ETA&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;CR Required By&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Comments&nbsp;</b>
            </th>
          </tr>
          <tr>
            <td>&nbsp;<a href=''(*ShipmentUrl*)''>S00000001</a>&nbsp;</td>
            <td>&nbsp;VESSEL NAME/1234&nbsp;</td>
            <td>&nbsp;AUSYD&nbsp;</td>
            <td>&nbsp;15-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;15-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;Exception manually actioned.&nbsp;</td>
          </tr>
        </table>
        <br />
        <hr />
        <br />
        It appears that the late reporting date has changed for the following shipments.
        These shipments now appear to have been reported after the new late reporting date,
        but there is no late reporting reason stored on the milestone exception.<br />
        <br />
        <br />
        <table class=''table'' cellpadding=''1'' cellspacing=''0'' border=''1'' bgcolor=''#FFFFFF''>
          <tr class=''tableheadings''>
            <th>
              <b>&nbsp;Shipment&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Vessel/Voyage or Flight&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Shipment First Port&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Shipment First Port ETA&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;CR Required By&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Comments&nbsp;</b>
            </th>
          </tr>
          <tr>
            <td>&nbsp;<a href=''(*ShipmentUrl*)''>S00000006</a>&nbsp;</td>
            <td>&nbsp;&nbsp;</td>
            <td>&nbsp;AUSYD&nbsp;</td>
            <td>&nbsp;23-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;17-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;Cargo Report was late.&nbsp;</td>
          </tr>
          <tr>
            <td>&nbsp;<a href=''(*ShipmentUrl*)''>S00000007</a>&nbsp;</td>
            <td>&nbsp;&nbsp;</td>
            <td>&nbsp;AUSYD&nbsp;</td>
            <td>&nbsp;23-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;17-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;Cargo Report was late.&nbsp;</td>
          </tr>
        </table>
        <br />
        <hr />
        <br />
        It appears that the late reporting date has changed for the following shipments.
        These shipments now appear to have been reported before the new late reporting date,
        but there is a late reporting reason stored on the milestone exception.<br />
        <br />
        <br />
        <table class=''table'' cellpadding=''1'' cellspacing=''0'' border=''1'' bgcolor=''#FFFFFF''>
          <tr class=''tableheadings''>
            <th>
              <b>&nbsp;Shipment&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Vessel/Voyage or Flight&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Shipment First Port&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Shipment First Port ETA&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;CR Required By&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Comments&nbsp;</b>
            </th>
          </tr>
          <tr>
            <td>&nbsp;<a href=''(*ShipmentUrl*)''>S00000009</a>&nbsp;</td>
            <td>&nbsp;&nbsp;</td>
            <td>&nbsp;AUSYD&nbsp;</td>
            <td>&nbsp;23-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;17-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;Cargo Report was not late.&nbsp;</td>
          </tr>
        </table>
        <br />
        <hr />
        <br />
        Regards,<br />
        <br/>
        CargoWise One Shipment Cargo Report Notifications<br/>
       <br/>
      </td>
    </tr>
    <tr>
      <td><img src=''cid:Footer.jpg'' alt=''Footer Image'' /></td>
    </tr>
  </table>
</body>
</html>
", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);

		protected override void SetUp()
		{
			base.SetUp();
			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "G0";
			CustomsDataRegistry.Instance.GroupToSendLateSeaCargoReportAndWarningsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			cargoReportWorkflow = new CargoReportWorkflowHelper(Factory);
			StmEvent cRAEvent = Factory.LoadFromNaturalKey<StmEvent>(StmEventSchema.SE_Code, Events.CargoReportAccepted.Code);
			cRAEvent.SE_AirExceptionSafetyMargin = 24;
			cRAEvent.SE_SeaExceptionSafetyMargin = 48;
			Factory.Save();
		}

		protected override void TearDown()
		{
			cargoReportWorkflow.Dispose();
			base.TearDown();
		}

		CargoReportWorkflowHelper cargoReportWorkflow;
	}
}
