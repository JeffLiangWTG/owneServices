using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	class UniversalMilestoneWriterTest : TestCaseWithFactory
	{
		public void TestPopulateMilestones()
		{
			var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var shipment1 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var shipment2 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			AssertPopulateMilestones(shipmentBO, shipment1, shipment2);
		}

		public void TestContainerPopulateMilestones()
		{
			var containerBO = Factory.New(ObjectFactory.GetType<ICommonContainer>());
			var container1 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			var container2 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			AssertPopulateMilestones(containerBO, container1, container2);
		}

		public void TestTransactionPopulateMilestones()
		{
			var transaction = Factory.New(ObjectFactory.GetType<Accounting.Integration.IARInvoice>());
			var transactionInfo1 = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionInfo2 = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			AssertPopulateMilestones(transaction, transactionInfo1, transactionInfo2);
		}

		void AssertPopulateMilestones(BusinessObject source, IMilestoneCollectionParent milestoneCollectionParent1, IMilestoneCollectionParent milestoneCollectionParent2)
		{
			var writer = new UniversalMilestoneWriter();
			var milestone1 = ((IWorkflowProvider)source).WorkflowItems.AddNew();
			SetupNotPublishedMilestone(milestone1);
			var milestone2 = ((IWorkflowProvider)source).WorkflowItems.AddNew();
			SetupPublishedMilestone(milestone2);
			var trigger = ((IWorkflowProvider)source).WorkflowItems.AddNew();
			trigger.P9_Type = "TRG";

			writer.PopulateMilestones(source, milestoneCollectionParent1 as IDataObject, true);
			AssertEquals("include internal milestone", 2, milestoneCollectionParent1.MilestoneCollection.Count);

			var milestoneCollection1 = milestoneCollectionParent1.MilestoneCollection[0];
			AssertEquals("Sequence", milestone1.P9_Sequence, milestoneCollection1.Sequence);
			AssertEquals("Description", milestone1.P9_Description, milestoneCollection1.Description);
			AssertEquals("EventCode", milestone1.P9_SE_NKMilestoneEvent, milestoneCollection1.EventCode);
			AssertEquals("Condition Type", milestone1.P9_TriggerCondition, milestoneCollection1.ConditionType);
			AssertEquals("Condition Reference", "Notes", milestoneCollection1.ConditionReference);
			AssertEquals("Actual Date", milestone1.P9_ActualDateOffset, milestoneCollection1.ActualDate);
			AssertEquals("Scheduled Date", milestone1.P9_ScheduledDateOffset, milestoneCollection1.EstimatedDate);

			var milestoneCollection2 = milestoneCollectionParent1.MilestoneCollection[1];
			AssertEquals("Sequence", milestone2.P9_Sequence, milestoneCollection2.Sequence);
			AssertEquals("Description", milestone2.P9_Description, milestoneCollection2.Description);
			AssertEquals("Milestone Event", milestone2.P9_SE_NKMilestoneEvent, milestoneCollection2.EventCode);
			AssertEquals("Trigger Condition", milestone2.P9_TriggerCondition, milestoneCollection2.ConditionType);
			AssertEquals("Condition Value", milestoneCollection2.ConditionReference);
			AssertEquals("Actual Date", milestone2.P9_ActualDateOffset, milestoneCollection2.ActualDate);
			AssertEquals("Scheduled Date", milestone2.P9_ScheduledDateOffset, milestoneCollection2.EstimatedDate);

			writer.PopulateMilestones(source, milestoneCollectionParent2 as IDataObject, false);
			AssertEquals("exclude internal milestone", 1, milestoneCollectionParent2.MilestoneCollection.Count);

			var milestoneCollection3 = milestoneCollectionParent2.MilestoneCollection[0];
			AssertEquals("Sequence", milestone2.P9_Sequence, milestoneCollection3.Sequence);
			AssertEquals("Description", milestone2.P9_Description, milestoneCollection3.Description);
			AssertEquals("Milestone Event", milestone2.P9_SE_NKMilestoneEvent, milestoneCollection3.EventCode);
			AssertEquals("Trigger Condition", milestone2.P9_TriggerCondition, milestoneCollection3.ConditionType);
			AssertEquals("Condition Value", milestoneCollection3.ConditionReference);
			AssertEquals("Actual Date", milestone2.P9_ActualDateOffset, milestoneCollection3.ActualDate);
			AssertEquals("Scheduled Date", milestone2.P9_ScheduledDateOffset, milestoneCollection3.EstimatedDate);
		}

		#region Implementation

		void SetupNotPublishedMilestone(ProcessTask milestone1)
		{
			milestone1.P9_Type = "MIL";
			milestone1.P9_Sequence = 1;
			milestone1.P9_Description = "DESC";
			milestone1.TriggerConditions.TriggerEventCode = "ATH";
			milestone1.TriggerConditions.TriggerCondition = "MCR";
			milestone1.P9_Notes = Encoding.ASCII.GetBytes("Notes");
			milestone1.SetMilestoneActualDateForTest(ZDateTime.Now.AddDays(-1));
			milestone1.P9_ScheduledDateForBinding = ZDateTimeOffset.Now.AddDays(-2);
			milestone1.P9_IsPublished = false;
		}

		void SetupPublishedMilestone(ProcessTask milestone2)
		{
			milestone2.P9_Type = "MIL";
			milestone2.P9_Sequence = 1;
			milestone2.P9_Description = "DESCRIPTION";
			milestone2.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			milestone2.TriggerConditions.TriggerCondition = "REF";
			milestone2.P9_Notes = Encoding.ASCII.GetBytes("Condition Value");
			milestone2.SetMilestoneActualDateForTest(ZDateTime.Now.AddDays(-3));
			milestone2.P9_ScheduledDateForBinding = ZDateTimeOffset.Now.AddDays(-4);
			milestone2.P9_IsPublished = true;
		}

		#endregion
	}
}
