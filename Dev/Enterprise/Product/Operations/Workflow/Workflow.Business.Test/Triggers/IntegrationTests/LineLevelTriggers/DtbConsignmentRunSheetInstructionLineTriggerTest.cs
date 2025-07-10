using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test.Triggers.LineTriggers.IntegrationTests
{
	class DtbConsignmentRunSheetInstructionLineTriggerTest : LineTriggerTestCase
	{
		#region TestInstructionLineTriggerFire_NTF

		[TestDate(2017, 04, 03)]
		public void TestInstructionLineTriggerFire_NTF()
		{
			AssertRunSheetInstructionLineTriggerFire((i) => SetupEmailNotification(i),
				(p) =>
				{
					AssertEquals("There should be one email sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				});
		}

		#endregion

		#region TestInstructionLineTriggerFire_FLD

		[TestDate(2017, 04, 03)]
		public void TestInstructionLineTriggerFire_FLD()
		{
			AssertRunSheetInstructionLineTriggerFire((i) => SetupFieldNotification(i),
			instruction =>
			{
				instruction.Reload();
				AssertEquals("TES", instruction.K1_FailureReason);
			});
		}

		protected override string FieldNameToUpdate => DtbConsignmentRunSheetInstructionSchema.Constants.K1_FailureReason;

		#endregion

		void AssertRunSheetInstructionLineTriggerFire(Action<ProcessTaskNotification> setupNotifications, Action<DtbConsignmentRunSheetInstruction> assertResults)
		{
			var runSheet = Helper.CreateRunSheet();
			var instruction = runSheet.RunSheetInstructions.AddNew();

			// We have to make sure the runsheet has the trigger first.
			var templateTask = CreateTriggerInNewFactory(WorkflowDescriptors.DtbConsignmentRunSheetWorkflowDescriptorCode, TriggerLineTypes.Codes.RunSheetInstruction, AutoEvents.ArrivalCode);
			setupNotifications(templateTask.ProcessTaskNotifications.AddNew());
			templateTask.Factory.Save();
			runSheet.Logs.AddNew(new EventValue(AutoEvents.Departure, eventTime: ZDateTimeOffset.Now, deferFiringWorkflow: true));
			Factory.Save();
			RunLogWalker();

			// add event to line level 
			instruction.Logs.AddNew(new EventValue(AutoEvents.Arrival, eventTime: ZDateTimeOffset.Now, deferFiringWorkflow: true));
			Factory.Save();

			// run log walker to create the process tasks
			var logs = RunLogWalker();
			runSheet.Reload();
			var wteEventQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, ProcessTask.WorkflowEventTriggerJobQueueName);
			wteEventQuery.AddToFilter(StmJobQueueSchema.SJ_ParentID, runSheet.WorkflowItems.Triggers[0].PK);
			AssertEquals("WTE event must be processed:\r\n" + logs, JobQueueStatus.StatusProcessed, Factory.Load<IQueuedLog>(wteEventQuery).FirstOrDefault()?.SJ_Status);
			assertResults(instruction);
		}

		#region Helper

		protected TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}
		TransportBookingConsignmentTestHelper helper;

		protected override void SetUp()
		{
			Db.Connection.ExecuteNonQuery("delete from dbo.StmALogQueue;delete from dbo.StmALogQueueWTE");
			base.SetUp();
		}

		#endregion
	}
}
