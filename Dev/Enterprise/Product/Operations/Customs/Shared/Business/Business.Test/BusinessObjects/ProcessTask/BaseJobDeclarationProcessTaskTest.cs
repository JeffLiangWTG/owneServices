using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseJobDeclarationProcessTask<BaseJobDeclaration>))]
	class BaseJobDeclarationProcessTaskTest : ProcessTaskTest
	{
		public void TestParentType()
		{
			var task = Factory.New<BaseJobDeclarationProcessTask<BaseJobDeclarationWithAddInfo>>();
			AssertEquals("ParentType", typeof(BaseJobDeclarationWithAddInfo), task.GetPropertyType("Parent"));
		}

		public void TestPickupCartageAdvised_UpdatesDocsAndCartage()
		{
			var milestone = workflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.PickupCartageAdvised.Code;
			milestone.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 1));
			AssertEquals(new ZDateTime(2000, 1, 1), declaration.DocsAndCartage.JP_PickupCartageAdvised);
		}

		public void TestDeliveryCartageAdvised_UpdatesDocsAndCartage()
		{
			var milestone = workflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.DeliveryCartageAdvised.Code;
			milestone.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 1));
			AssertEquals(new ZDateTime(2000, 1, 1), declaration.DocsAndCartage.JP_DeliveryCartageAdvised);
		}

		public void TestClosingDeclarationAIDExceptionClosesPreadviceAIDException()
		{
			var declarationException = declaration.WorkflowItems.Exceptions.AddNew();
			declarationException.TriggerConditions.TriggerEventCode = Events.AllImportDocumentsReceived.Code;

			var preadvice = Factory.New<JobShipmentPreplanning>();
			preadvice.EF_JE = declaration.PK;
			var preadviceException = ((IWorkflowProvider)preadvice).WorkflowItems.Exceptions.AddNew();
			preadviceException.TriggerConditions.TriggerEventCode = Events.AllImportDocumentsReceived.Code;

			AssertEquals("Preadvice milestone not actioned initially", false, preadviceException.IsClosed);
			declarationException.IsExceptionActioned = true;
			AssertEquals("Preadvice milestone actioned", true, preadviceException.IsExceptionActioned);
		}

		public void TestETA_EmptyInBase()
		{
			var task = Factory.NewWithValidTestData<BaseJobDeclarationProcessTask<BaseJobDeclaration>>();
			task.P9_ParentID = declaration.PK;
			task.P9_ParentTableCode = "JE";

			Factory.Save();

			AssertEquals("ETA field should return BaseJobDeclaration.JE_DateAtFinalDestination for current Exception if its ParentTableCode is JE", declaration.JE_DateAtFinalDestination, task.ETA);
		}

		public void TestETD_EmptyInBase()
		{
			var task = Factory.NewWithValidTestData<BaseJobDeclarationProcessTask<BaseJobDeclaration>>();
			task.P9_ParentID = declaration.PK;
			task.P9_ParentTableCode = "JE";

			Factory.Save();

			AssertEquals("ETD field should return BaseJobDeclaration.JE_DateAtOrigin for current Exception if its ParentTableCode is JE", declaration.JE_DateAtOrigin, task.ETD);
		}

		public void TestLoadOrOriginPort_EmptyInBase()
		{
			var task = Factory.NewWithValidTestData<BaseJobDeclarationProcessTask<BaseJobDeclaration>>();
			task.P9_ParentID = declaration.PK;
			task.P9_ParentTableCode = "JE";

			Factory.Save();

			AssertEquals("LoadOrOriginPort field should return BaseJobDeclaration.JE_RL_NKPortOfLoading for current Exception if its ParentTableCode is JE", declaration.JE_RL_NKPortOfLoading, task.LoadOrOriginPort);
		}

		public void TestDischargeOrDestinationPort_EmptyInBase()
		{
			var task = Factory.NewWithValidTestData<BaseJobDeclarationProcessTask<BaseJobDeclaration>>();
			task.P9_ParentID = declaration.PK;
			task.P9_ParentTableCode = "JE";

			Factory.Save();

			AssertEquals("DischargeOrDestinationPort field should return BaseJobDeclaration.JE_RL_NKPortOfArrival for current Exception if its ParentTableCode is JE", declaration.JE_RL_NKPortOfArrival, task.DischargeOrDestinationPort);
		}

		public void TestEstimateDefaultedFromDate_ETA()
		{
			var milestone = declaration.WorkflowItems.Milestones.AddNew();
			milestone.P9_EstimatedDefaultedFrom = DeclarationEstimateDefaultedFromList.Codes.ETA;
			declaration.JE_DateOfArrival = ZDateTime.Now;
			declaration.JE_DateAtFinalDestination = ZDateTime.SmallDateTimeNow.AddDays(2);
			Factory.Save();
			AssertEquals("Estimate date defaulted from declaration ETA", declaration.JE_DateAtFinalDestination, milestone.P9_ScheduledDate.ToZDateTime());
		}

		public void TestActualDateDefaultedFromRouting()
		{
			var now = ZDateTimeOffset.Now;

			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "USCHI";

			var transport1 = declaration.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUMEL";
			transport1.JW_ATD = now.ToZDateTime();
			transport1.JW_ATA = now.AddDays(1).ToZDateTime();

			var transport2 = declaration.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "AUMEL";
			transport2.JW_RL_NKDiscPort = "USPHL";
			transport2.JW_ATD = now.AddDays(2).ToZDateTime();
			transport2.JW_ATA = now.AddDays(3).ToZDateTime();

			var transport3 = declaration.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "USPHL";
			transport3.JW_RL_NKDiscPort = "USCHI";
			transport3.JW_ATD = now.AddDays(4).ToZDateTime();
			transport3.JW_ATA = now.AddDays(5).ToZDateTime();

			var milestone1 = declaration.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = Events.DepartureCode;

			var milestone2 = declaration.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = Events.ArrivalCode;

			Factory.Save();

			AssertEquals("Actual departure date defaulted from the first transport", now.ToZDateTime(), milestone1.P9_ActualDate.ToZDateTime());
			AssertEquals("Actual arrival date defaulted from the last transport", now.AddDays(5).ToZDateTime(), milestone2.P9_ActualDate.ToZDateTime());
		}

		public void TestActualDateDefaultedFromRouting_DoNotRaiseWTEEventOnEveryLoad()
		{
			var now = ZDateTimeOffset.Now;

			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "USCHI";

			var transport1 = declaration.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUMEL";
			transport1.JW_ATD = now.ToZDateTime();
			transport1.JW_ATA = now.AddDays(1).ToZDateTime();

			var transport2 = declaration.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "AUMEL";
			transport2.JW_RL_NKDiscPort = "USPHL";
			transport2.JW_ATD = now.AddDays(2).ToZDateTime();
			transport2.JW_ATA = now.AddDays(3).ToZDateTime();

			var transport3 = declaration.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "USPHL";
			transport3.JW_RL_NKDiscPort = "USCHI";
			transport3.JW_ATD = now.AddDays(4).ToZDateTime();
			transport3.JW_ATA = now.AddDays(5).ToZDateTime();

			var trigger1 = declaration.WorkflowItems.Triggers.AddNew();
			trigger1.TriggerConditions.TriggerEventCode = Events.DepartureCode;
			var triggerNotification = trigger1.ProcessTaskNotifications.AddNew();
			triggerNotification.PQ_TriggerType = "NTF";
			triggerNotification.PQ_Calc_TriggerParty = "EML";
			triggerNotification.PQ_EmailAddr = "abc@email.com";
			triggerNotification.PQ_EmailText = "Test LWK";

			var trigger2 = declaration.WorkflowItems.Triggers.AddNew();
			trigger2.TriggerConditions.TriggerEventCode = Events.ArrivalCode;

			AssertEquals("Actual departure date defaulted from the first transport", now.ToZDateTime(), trigger1.P9_ActualDate.ToZDateTime());
			AssertEquals("Actual arrival date defaulted from the last transport", now.AddDays(5).ToZDateTime(), trigger2.P9_ActualDate.ToZDateTime());

			var wteEvents = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode) { FetchOnlyFromLocalCache = true });
			wteEvents.Where(l => !l.IsInDatabase).DeleteAll();

			AssertEquals("Actual departure date defaulted from the first transport", now.ToZDateTime(), trigger1.P9_ActualDate.ToZDateTime());
			wteEvents = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode) { FetchOnlyFromLocalCache = true });
			AssertEquals(0, wteEvents.Length);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<BaseJobDeclaration>();
			workflowItems = ((IWorkflowProvider)declaration).WorkflowItems;
		}
		BaseJobDeclaration declaration;
		ProcessTaskCollection workflowItems;

		protected override BusinessObject GetNewBusinessObject() => workflowItems.AddNew();
	}
}
