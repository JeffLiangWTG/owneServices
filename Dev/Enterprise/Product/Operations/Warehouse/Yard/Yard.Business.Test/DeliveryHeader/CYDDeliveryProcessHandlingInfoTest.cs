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
	[TestedType(typeof(CYDDeliveryProcessHandlingInfo))]
	public class CYDDeliveryProcessHandlingInfoTest : TestCaseWithFactory
	{
		public void TestPopulateParentTriggers_NoLogParent()
		{
			var delivery = Factory.NewWithValidTestData<CYDDelivery>();
			delivery.YDL_YTU_DeliveryTransportationUnit = ZGuid.Empty;
			var stmALog = delivery.Logs.AddNew(Events.BookingConfirmed);
			IEnumerable<IBaseTrigger> parentTriggers = null;
			AssertNoExceptionThrown(() =>
			{
				parentTriggers = ((IProcessHandlingInfoProvider)delivery).ProcessHandlingInfo.GetParentTriggers(stmALog);
			});
			AssertEquals(false, parentTriggers.Any());
		}

		public void TestPopulateParentTriggers_CYDTransportationUnit()
		{
			var transportationUnit = Factory.New<CYDTransportationUnit>();
			var correctLineTrigger = CreateTriggers(transportationUnit);
			var delivery = transportationUnit.Deliveries.AddNew();
			var stmALog = delivery.Logs.AddNew(Events.BookingConfirmed);
			var parentTriggers = (delivery as IProcessHandlingInfoProvider).ProcessHandlingInfo.GetParentTriggers(stmALog);
			AssertContainsExactElementsInAnyOrder(new[] { correctLineTrigger }, parentTriggers);
		}

		static ProcessTask CreateTriggers(IWorkflowProvider workflowProvider)
		{
			var correctLineTrigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			correctLineTrigger.P9_LineTriggerType = TriggerLineTypes.Codes.CYDDelivery;
			correctLineTrigger.TriggerConditions.TriggerEventCode = Events.BookingConfirmedCode;
			return correctLineTrigger;
		}
	}
}
