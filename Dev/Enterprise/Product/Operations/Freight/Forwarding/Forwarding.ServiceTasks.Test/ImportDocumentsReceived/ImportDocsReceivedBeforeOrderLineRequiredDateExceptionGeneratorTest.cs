using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Test
{
	internal abstract class ImportDocsReceivedBeforeOrderLineRequiredDateExceptionGeneratorTest : TestCaseWithFactory
	{
		[TestDate]
		public void TestGenerateExceptionFromOverdueOrderReceivedBy()
		{
			TestDateAttribute.Date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			ProcessTask orderDocumentsReceivedMilestone = this.OrderDocumentsReceivedMilestone;
			ProcessTask documentsReceivedMilestoneTqoComplete = this.DocumentsReceivedMilestoneToComplete;
			OrderLine1.JO_LineDropDate = new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, 12);
			OrderLine2.JO_LineDropDate = new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, 22);
			Factory.Save();
			TestDateAttribute.Date = new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, 10).ToDateTime();
			Processor.Process(Notifications);
			OrderWorkflow.WorkflowItems.Load();
			AssertEquals("No exceptions generated as 'Received Documents' milestone not yet overdue", 0, OrderWorkflow.WorkflowItems.Exceptions.Count);
			TestDateAttribute.Date = new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, 11).ToDateTime();
			Processor.Process(Notifications);
			OrderWorkflow.WorkflowItems.Load();
			AssertEquals("1 exception generated for overdue 'Received Documents' milestone", 1, OrderWorkflow.WorkflowItems.Exceptions.Count);
			AssertEquals("Exception associated with 'Documents Received' milestone", Events.AllImportDocumentsReceived.Code, OrderWorkflow.WorkflowItems.Exceptions[0].P9_SE_NKMilestoneEvent);
			TestDateAttribute.Date = new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, 21).ToDateTime();
			Processor.Process(Notifications);
			OrderWorkflow.WorkflowItems.Load();
			AssertEquals("2 exception generated for overdue 'Received Documents' milestone", 2, OrderWorkflow.WorkflowItems.Exceptions.Count);
			TestDateAttribute.Date = new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, 23).ToDateTime();
			Processor.Process(Notifications);
			OrderWorkflow.WorkflowItems.Load();
			AssertEquals("No additional exception generated", 2, OrderWorkflow.WorkflowItems.Exceptions.Count);
		}

		[TestDate]
		public void TestDontGenerateExceptionWhenMilestoneMet()
		{
			TestDateAttribute.Date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			ProcessTask orderDocumentsReceivedMilestone = this.OrderDocumentsReceivedMilestone;
			ProcessTask documentsReceivedMilestoneToComplete = this.DocumentsReceivedMilestoneToComplete;
			OrderLine1.JO_LineDropDate = new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, 12).ToDateTime();
			OrderLine2.JO_LineDropDate = new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, 22).ToDateTime();
			OrderLine3.JO_LineDropDate = ZDateTime.Empty; // expect no exception
			documentsReceivedMilestoneToComplete.SetMilestoneActualDateForTest(ZDateTime.Now);
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2050, 1, 1);
			Processor.Process(Notifications);
			OrderWorkflow.WorkflowItems.Load();
			AssertEquals("No exceptions generated as milestone is met", 0, OrderWorkflow.WorkflowItems.Exceptions.Count);
		}

		protected abstract IWorkflowProvider WorkflowProviderWithMilestoneToComplete
		{
			get;
		}

		#region Implementation

		IWorkflowProvider OrderWorkflow
		{
			get
			{
				return Order;
			}
		}

		protected Order Order
		{
			get
			{
				return order ?? (order = Factory.NewWithValidTestData<Order>());
			}
		}

		Order order;

		OrderLine OrderLine1
		{
			get
			{
				return orderLine1 ?? (orderLine1 = Order.OrderLines.AddNew());
			}
		}

		OrderLine orderLine1;

		OrderLine OrderLine2
		{
			get
			{
				return orderLine2 ?? (orderLine2 = Order.OrderLines.AddNew());
			}
		}

		OrderLine orderLine2;

		OrderLine OrderLine3
		{
			get
			{
				return orderLine3 ?? (orderLine3 = Order.OrderLines.AddNew());
			}
		}

		OrderLine orderLine3;

		ProcessTask OrderDocumentsReceivedMilestone
		{
			get
			{
				if (orderDocumentsReceivedMilestone == null)
				{
					orderDocumentsReceivedMilestone = OrderWorkflow.WorkflowItems.Milestones.AddNew();
					orderDocumentsReceivedMilestone.TriggerConditions.TriggerEventCode = Events.AllImportDocumentsReceived.Code;
					orderDocumentsReceivedMilestone.P9_EstimatedDefaultedFrom = OrderMilestoneEstimateDefaultedFromList.Codes.OrderLineRequiredDate;
					orderDocumentsReceivedMilestone.P9_EstimatedDefaultTimeDelta = new ZDateTime(ZDateTime.Now.Year, 1, 1).AddDays(-2);
				}

				return orderDocumentsReceivedMilestone;
			}
		}

		ProcessTask orderDocumentsReceivedMilestone;

		ProcessTask DocumentsReceivedMilestoneToComplete
		{
			get
			{
				if (documentsReceivedMilestoneToComplete == null)
				{
					if (WorkflowProviderWithMilestoneToComplete is Order)
					{
						documentsReceivedMilestoneToComplete = OrderDocumentsReceivedMilestone;
					}
					else
					{
						documentsReceivedMilestoneToComplete = WorkflowProviderWithMilestoneToComplete.WorkflowItems.Milestones.AddNew();
						documentsReceivedMilestoneToComplete.TriggerConditions.TriggerEventCode = Events.AllImportDocumentsReceived.Code;
						documentsReceivedMilestoneToComplete.P9_EstimatedDefaultedFrom = OrderMilestoneEstimateDefaultedFromList.Codes.OrderLineRequiredDate;
						documentsReceivedMilestoneToComplete.P9_EstimatedDefaultTimeDelta = new ZDateTime(ZDateTime.Now.Year, 1, 1).AddDays(-2);
					}
				}

				return documentsReceivedMilestoneToComplete;
			}
		}

		ProcessTask documentsReceivedMilestoneToComplete;

		NotificationBuffer Notifications
		{
			get
			{
				return notifications ?? (notifications = new NotificationBuffer());
			}
		}

		NotificationBuffer notifications;

		ImportDocsReceivedBeforeOrderLineRequiredDateExceptionGenerator Processor
		{
			get
			{
				return processor ?? (processor = new ImportDocsReceivedBeforeOrderLineRequiredDateExceptionGenerator());
			}
		}

		ImportDocsReceivedBeforeOrderLineRequiredDateExceptionGenerator processor;

		#endregion Implementation
	}
}
