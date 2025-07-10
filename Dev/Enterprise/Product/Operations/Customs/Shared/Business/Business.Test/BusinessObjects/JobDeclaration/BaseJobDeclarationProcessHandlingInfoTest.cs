using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobDeclarationProcessHandlingInfoTest : TestCaseWithFactory
	{
		#region GetCascadingTargets - Orders

		public void TestGetCascadingTargets_DeclarationWithLinkedOrder_ReturnOrderAsTarget()
		{
			CombineAssertions(delegate
			{
				AssertCascadingTargetsFromOrder(Events.Arrival);
				AssertCascadingTargetsFromOrder(Events.Departure);
				AssertCascadingTargetsFromOrder(Events.CargoAvailable);
				AssertCascadingTargetsFromOrder(Events.DeliveryCartageAdvised);
				AssertCascadingTargetsFromOrder(Events.DeliveryCartageCompleteFinalised);
				AssertCascadingTargetsFromOrder(Events.CustomsCommenced);
				AssertCascadingTargetsFromOrder(Events.CustomsCleared);
				AssertCascadingTargetsFromOrder(Events.ExportCustomsCommenced, extraEventToBeReturned: Events.CustomsCommenced);
				AssertCascadingTargetsFromOrder(Events.ExportCustomsCleared, extraEventToBeReturned: Events.CustomsCleared);
			});
		}

		void AssertCascadingTargetsFromOrder(Event eventToLookup, Event extraEventToBeReturned = null)
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_JE = declaration.PK;

			var orderMilestones = new List<ProcessTask>();

			foreach (Event orderProcessTaskEvent in new[] { eventToLookup, extraEventToBeReturned }.Where(ev => ev != null))
			{
				var milestone = order.WorkflowItems.Milestones.Cast<ProcessTask>()
					.FirstOrDefault(m => m.P9_SE_NKMilestoneEvent == orderProcessTaskEvent.Code && m.P9_RespondToCascadedEvents);

				if (milestone == null)
				{
					milestone = order.WorkflowItems.Milestones.AddNew();
					milestone.TriggerConditions.TriggerEventCode = orderProcessTaskEvent.Code;
					milestone.P9_RespondToCascadedEvents = true;
				}

				orderMilestones.Add(milestone);
			}

			Factory.Save();

			var eventLog = order.Logs.AddNew();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_SE_NKEvent = eventToLookup.Code;
			}

			var handler = new BaseJobDeclarationProcessHandlingInfo(declaration);
			var targets = handler.GetCascadingTargets(eventLog).ToArray();

			var orderTarget = targets.Single(t => t.Parent.LogsParentPK == order.PK);

			AssertContainsExactElementsInAnyOrder(string.Format("{0}: Order process tasks", eventToLookup.Code),
				orderMilestones,
				orderTarget.Triggers);
		}

		#endregion
	}
}
