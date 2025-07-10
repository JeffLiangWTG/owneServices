using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Extensions.Testing
{
	sealed class LogsExtensionsTest : TestCaseWithFactory
	{
		#region TestAddATCEvent

		public void TestAddATCEvent_ParentIsInDatabase()
		{
			var shipment = Factory.New<CommonShipment>();
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C00001235";
			Factory.Save();

			var parameters = new Dictionary<string, string>() { ["TYP"] = "Consol" };
			shipment.Logs.AddATCEvent(consol.IsInDatabase, consol.LogReference(true), parameters);

			var log = shipment.Logs.MostRecentLogByEventTime(Events.Attached);
			AssertEquals("SL_FireWorkflow should be false as parent is in database", false, log.SL_FireWorkflow);
			AssertEquals("SL_Reference", "C00001235|TYP=Consol", log.SL_Reference);
			AssertEquals("SL_SE_NKEvent", "ATC", log.SL_SE_NKEvent);
			AssertEquals("SL_IsEstimate", false, log.SL_IsEstimate);
		}

		public void TestAddATCEvent_ParentIsNotInDatabase()
		{
			var shipment = Factory.New<CommonShipment>();
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C00001235";

			var parameters = new Dictionary<string, string>() { ["TYP"] = "Consol" };
			shipment.Logs.AddATCEvent(consol.IsInDatabase, consol.LogReference(true), parameters);

			var log = shipment.Logs.MostRecentLogByEventTime(Events.Attached);
			var pk = consol.PK;
			AssertEquals("SL_FireWorkflow should be true as parent is not in database", true, log.SL_FireWorkflow);
			AssertEquals("SL_Reference", string.Format("{0}|TYP=Consol", pk), log.SL_Reference);
			AssertEquals("SL_SE_NKEvent", "ATC", log.SL_SE_NKEvent);
			AssertEquals("SL_IsEstimate", false, log.SL_IsEstimate);
		}

		#endregion

		#region Test UpdateEventReferenceNumbers

		[ExpectNoExceptions]
		[TestDate(2016, 2, 8)]
		public void TestUpdateEventReferenceNumbersAfterEventThreshold_CollectionNotModified()
		{
			var shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var logs = shipment.Logs;

			var workflowProvider = (IWorkflowProvider)shipment;
			var milestone = workflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.ServiceCompleted.Code;
			milestone.P9_ScheduledDate = TestDateAttribute.Date.AddDays(1);

			var newLog = logs.AddNew(Events.ServiceCompleted);
			using (newLog.LockForUpdatingKeyFieldsForTesting())
			{
				newLog.SL_IsEstimate = true;
			}

			Factory.Save();

			logs.UpdateEventReferenceNumbers(Events.ServiceCompleted, "To:", "result");
		}

		public void TestUpdateEventReferenceNumbers_SetsDeferFiringWorkflowToFalseByDefault()
		{
			var shipment = Factory.New<CommonShipment>();
			var consol = Factory.New<CommonConsol>();
			ConsolShipmentRelationshipHelper.ShipmentAddedToConsol(consol, shipment);
			var log = shipment.Logs.MostRecentLogByEventTime(Events.Attached);
			AssertEquals("Precondition: SL_FireWorkflow", true, log.SL_FireWorkflow);
			AssertEquals("Precondition: SL_Reference contains PK", string.Format("{0}|TYP=Consol", consol.PK), log.SL_Reference);

			consol.JK_UniqueConsignRef = "C00001235";
			shipment.Logs.UpdateEventReferenceNumbers(Events.Attached, consol.PK.ToString(), consol.JK_UniqueConsignRef);

			AssertEquals("SL_FireWorkflow set to false", false, log.SL_FireWorkflow);
			AssertEquals("SL_Reference got updated", string.Format("{0}|TYP=Consol", consol.JK_UniqueConsignRef), log.SL_Reference);
		}

		public void TestUpdateEventReferenceNumbers_SetsDeferFiringWorkflowToTrue()
		{
			var shipment = Factory.New<CommonShipment>();
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C00001235";
			Factory.Save();

			ConsolShipmentRelationshipHelper.ShipmentAddedToConsol(consol, shipment);
			var log = shipment.Logs.MostRecentLogByEventTime(Events.Attached);
			AssertEquals("Precondition: SL_FireWorkflow", false, log.SL_FireWorkflow);
			AssertEquals("Precondition: SL_Reference contains consol num", string.Format("{0}|TYP=Consol", consol.JK_UniqueConsignRef), log.SL_Reference);

			shipment.Logs.UpdateEventReferenceNumbers(Events.Attached, consol.JK_UniqueConsignRef, consol.PK.ToString(), deferFiringWorkflow: true);

			AssertEquals("SL_FireWorkflow was set to true", true, log.SL_FireWorkflow);
			AssertEquals("SL_Reference got updated", string.Format("{0}|TYP=Consol", consol.PK), log.SL_Reference);
		}

		public void TestUpdateEventReferenceNumbers_DoesNotSetFiringWorkflow_WhenSL_ReferenceDoesNotMatch()
		{
			var shipment = Factory.New<CommonShipment>();
			var consol = Factory.New<CommonConsol>();
			ConsolShipmentRelationshipHelper.ShipmentAddedToConsol(consol, shipment);
			var log = shipment.Logs.MostRecentLogByEventTime(Events.Attached);
			AssertEquals("Precondition: SL_FireWorkflow", true, log.SL_FireWorkflow);
			AssertEquals("Precondition: SL_Reference contains PK", string.Format("{0}|TYP=Consol", consol.PK), log.SL_Reference);

			consol.JK_UniqueConsignRef = "C00001235";
			shipment.Logs.UpdateEventReferenceNumbers(Events.Attached, ZGuid.BrettsGuid.ToString(), consol.JK_UniqueConsignRef, deferFiringWorkflow: false);

			AssertEquals("SL_FireWorkflow remains unchanged", true, log.SL_FireWorkflow);
			AssertEquals("SL_Reference did not get updated", string.Format("{0}|TYP=Consol", consol.PK), log.SL_Reference);
		}

		#endregion
	}
}
