using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(Milestone))]
	class MilestoneTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPopulate()
		{
			var shipment = Factory.New(ObjectFactory.GetType<IForwardingShipment>()) as IWorkflowProvider;

			var milestone1 = shipment.WorkflowItems.AddNew();
			milestone1.P9_Type = "MIL";

			var milestone2 = shipment.WorkflowItems.AddNew();
			milestone2.P9_Type = "MIL";

			AssertEquals("Precondition: 2 milestones", 2, shipment.WorkflowItems.Count);

			Factory.Save();

			var milestones = Milestone.Create(shipment);
			AssertEquals("Count should be 2", 2, milestones.Count);
		}

		public void TestPopulate_Empty()
		{
			var shipment = Factory.New(ObjectFactory.GetType<IForwardingShipment>()) as IWorkflowProvider;

			AssertEquals("Precondition: No milestones", 0, shipment.WorkflowItems.Count);

			Factory.Save();

			var milestones = Milestone.Create(shipment);
			AssertEquals("Should be empty", 0, milestones.Count);
		}

		public void TestPopulate_NullWorkflowProvider()
		{
			var milestones = Milestone.Create(null);
			AssertEquals("Should be empty", 0, milestones.Count);
		}

		[TestDate(2018, 6, 6)]
		public void TestPopulate_Contents()
		{
			var today = ZDateTime.Today;

			var shipment = Factory.New(ObjectFactory.GetType<IForwardingShipment>()) as IWorkflowProvider;

			var milestone = shipment.WorkflowItems.AddNew();
			milestone.P9_Type = "MIL";
			milestone.P9_Sequence = 1;
			milestone.P9_Description = "Prismognathus klapperichi";
			milestone.TriggerConditions.TriggerEventCode = "ATH";
			milestone.TriggerConditions.TriggerCondition = "MCR";
			milestone.P9_ScheduledDate = today;
			milestone.SetMilestoneActualDateForTest(today.AddDays(-1));

			Factory.Save();

			var milestoneDocDataObject = Milestone.Create(shipment).FirstOrDefault();
			AssertEquals("Milestone has been created", true, milestoneDocDataObject != null);

			CombineAssertions(() =>
			{
				AssertEquals("Sequence", 1, milestoneDocDataObject.Sequence);
				AssertEquals("Description", "Prismognathus klapperichi", milestoneDocDataObject.Description);
				AssertEquals("EventCode", "ATH", milestoneDocDataObject.EventCode);
				AssertEquals("ConditionType", "MCR", milestoneDocDataObject.ConditionType);
				AssertEquals("EstimatedDate", today, milestoneDocDataObject.EstimatedDate);
				AssertEquals("ActualDate", today.AddDays(-1), milestoneDocDataObject.ActualDate);
			});
		}
	}
}
