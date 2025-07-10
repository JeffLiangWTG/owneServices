using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Test
{
	[TestedType(typeof(ImportDocsReceivedBeforeOrderLineRequiredDateExceptionGeneratorServiceTask))]
	internal class ImportDocsReceivedBeforeOrderLineRequiredDateExceptionGeneratorServiceTaskTest : ServiceTaskTestCase<ImportDocsReceivedBeforeOrderLineRequiredDateExceptionGeneratorServiceTask>
	{
		public void TestRunTask()
		{
			ProcessTask orderDocumentsReceivedMilestone = this.OrderDocumentsReceivedMilestone;
			OrderLine.JO_LineDropDate = ZDateTime.Now.AddDays(-2);
			Factory.Save();
			ImportDocsReceivedBeforeOrderLineRequiredDateExceptionGeneratorServiceTask task = new ImportDocsReceivedBeforeOrderLineRequiredDateExceptionGeneratorServiceTask();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			Order.WorkflowItems.Load();
			AssertEquals("2 exceptions generated for overdue 'Received Documents' milestone", 2, Order.WorkflowItems.Exceptions.Count);
		}

		public void TestHostedServiceAttributes()
		{
			var result = GetHostedServiceAttributes().SingleOrDefault();

			AssertNotNull(result);
			AssertEquals(true, result.IsMandatory);
			AssertEquals(false, result.AllowsMultipleInstances);
			AssertEquals(false, result.IsScheduleReadOnly);
			AssertEquals("20minutes", result.MinimumPeriod);
			AssertEquals(true, result.CanRunInAnyBranch);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();

		Order Order
		{
			get
			{
				return order ?? (order = Factory.NewWithValidTestData<Order>());
			}
		}

		OrderLine OrderLine
		{
			get
			{
				return orderLine ?? (orderLine = Order.OrderLines.AddNew());
			}
		}

		ProcessTask OrderDocumentsReceivedMilestone
		{
			get
			{
				if (orderDocumentsReceivedMilestone == null)
				{
					orderDocumentsReceivedMilestone = Order.WorkflowItems.Milestones.AddNew();
					orderDocumentsReceivedMilestone.TriggerConditions.TriggerEventCode = Events.AllImportDocumentsReceived.Code;
					orderDocumentsReceivedMilestone.P9_EstimatedDefaultedFrom = "LNR";
					orderDocumentsReceivedMilestone.P9_EstimatedDefaultTimeDelta = new ZDateTime(ZDateTime.Now.Year, 1, 1).AddDays(-2);
				}

				return orderDocumentsReceivedMilestone;
			}
		}

		Order order;
		OrderLine orderLine;
		ProcessTask orderDocumentsReceivedMilestone;
	}
}
