using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPickableDocketLineProcessHandlingInfoTest : WhsTestCaseWithFactory
	{
		#region TestPopulateCascadingTargets

		public void TestPopulateCascadingTargets()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Factory.Save();

			var handlingInfo = new WhsDocketLineProcessHandlingInfo(order.Lines[0]);
			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_SE_NKEvent = Events.ArrivalCode;
			}

			AssertEquals(Enumerable.Empty<CascadingLink>(), handlingInfo.GetCascadingTargets(eventLog));
		}

		#endregion

		#region TestPropagationTargets

		public void TestPropagationTargets()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Factory.Save();

			var orderWorkflowItemPropagated = order.WorkflowItems.AddNew();
			orderWorkflowItemPropagated.P9_Type = Constants.Workflow.WorkflowTriggerType;
			orderWorkflowItemPropagated.TriggerConditions.TriggerEventCode = Events.ChangeOfIdentifierCode;

			var logAdded = order.Lines[0].Logs.AddNew(Events.ChangeOfIdentifier,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.HoldCode));
			AssertEquals("Should not propogate.", ZDateTime.Empty, orderWorkflowItemPropagated.P9_ActualDate.ToZDateTime());
		}

		#endregion
	}
}
