using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(AllocationAdjustmentsSecurity))]
	sealed class AllocationAdjustmentsSecurityTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCtor()
		{
			AllocationAdjustmentsSecurity adjustmentsSecurity = new AllocationAdjustmentsSecurity();
			Assert(adjustmentsSecurity.IsRegisteredEditableChildObject(adjustmentsSecurity.Adjustments));
			AssertEquals(0, adjustmentsSecurity.Adjustments.Count);

			adjustmentsSecurity = new AllocationAdjustmentsSecurity(null);
			AssertEquals(0, adjustmentsSecurity.Adjustments.Count);

			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			ForwardingConsol consol3 = Factory.New<ForwardingConsol>();

			adjustmentsSecurity = new AllocationAdjustmentsSecurity(new ForwardingConsol[] { consol1, consol2, consol3 });
			AssertContainsExactElementsInAnyOrder(new ForwardingConsol[] { consol1, consol2, consol3 }, adjustmentsSecurity.Adjustments.Cast<AllocationAdjustment>().Select(x => x.Consol));
		}

		public void TestAdjustAll()
		{
			PreAllocationCheckCollection allocationChecks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			allocationChecks.Weight.Action = PreAllocationCheck.Actions.Restriction;
			allocationChecks.Weight.Percentage = 40m;
			ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allocationChecks);

			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_TotalShipmentActWeightCheck = 100m;
			consol1.Shipments.AddNew().JS_ActualWeight = 100m;

			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_TotalShipmentActWeightCheck = 200m;
			consol2.Shipments.AddNew().JS_ActualWeight = 200m;

			AllocationAdjustmentsSecurity adjustmentsSecurity = new AllocationAdjustmentsSecurity(new ForwardingConsol[] { consol1, consol2 });
			AssertContainsExactElementsInAnyOrder("Precondition", new ZDecimal[] { 100m, 200m }, adjustmentsSecurity.Adjustments.Cast<AllocationAdjustment>().Select(x => x.AllocatedWeight));

			adjustmentsSecurity.AdjustAll(null);
			AssertContainsExactElementsInAnyOrder("All consols adjusted", new ZDecimal[] { 250m, 500m }, adjustmentsSecurity.Adjustments.Cast<AllocationAdjustment>().Select(x => x.AllocatedWeight));
		}
	}
}
