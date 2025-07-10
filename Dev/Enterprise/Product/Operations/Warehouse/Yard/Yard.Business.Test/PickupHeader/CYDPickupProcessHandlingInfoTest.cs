using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDPickupProcessHandlingInfo))]
	public class CYDPickupProcessHandlingInfoTest : TestCaseWithFactory
	{
		public void TestPopulateParentTriggers_NoLogParent()
		{
			var pickup = Factory.NewWithValidTestData<CYDPickup>();
			pickup.YPL_YTU_PickupTransportationUnit = ZGuid.Empty;
			var stmALog = pickup.Logs.AddNew(Events.BookingConfirmed);
			IEnumerable<IBaseTrigger> parentTriggers = null;
			AssertNoExceptionThrown(() =>
			{
				parentTriggers = ((IProcessHandlingInfoProvider)pickup).ProcessHandlingInfo.GetParentTriggers(stmALog);
			});
			AssertEquals(false, parentTriggers.Any());
		}

		public void TestPopulateParentTriggers_CYDTransportationUnit()
		{
			var transportationUnit = Factory.New<CYDTransportationUnit>();
			var correctLineTrigger = CreateTriggers(transportationUnit);
			var pickup = transportationUnit.Pickups.AddNew();
			var stmALog = pickup.Logs.AddNew(Events.BookingConfirmed);
			var parentTriggers = (pickup as IProcessHandlingInfoProvider).ProcessHandlingInfo.GetParentTriggers(stmALog);
			AssertContainsExactElementsInAnyOrder(new[] { correctLineTrigger }, parentTriggers);
		}

		static ProcessTask CreateTriggers(IWorkflowProvider workflowProvider)
		{
			var correctLineTrigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			correctLineTrigger.P9_LineTriggerType = TriggerLineTypes.Codes.CYDPickup;
			correctLineTrigger.TriggerConditions.TriggerEventCode = Events.BookingConfirmedCode;
			return correctLineTrigger;
		}
	}
}
