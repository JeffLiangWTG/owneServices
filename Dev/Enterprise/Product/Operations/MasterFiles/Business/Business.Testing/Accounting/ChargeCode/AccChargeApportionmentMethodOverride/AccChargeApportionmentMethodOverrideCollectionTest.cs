using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeApportionmentMethodOverrideCollection))]
	sealed class AccChargeApportionmentMethodOverrideCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestChargeCodeApportionmentMethodOverrides()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			var apportionmentMethodOverride = Factory.NewWithValidTestData<AccChargeApportionmentMethodOverride>();
			apportionmentMethodOverride.AAM_AC = chargeCode1.PK;
			Factory.Save();

			AssertEquals(1, chargeCode1.ApportionmentMethodOverrides.Count);
			AssertEquals(0, chargeCode2.ApportionmentMethodOverrides.Count);
		}

		public void TestNoAuditLogsWithAttibute()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();
			var logs = chargeCode.Logs.GetAllLogs();
			AssertEquals(0, logs.Count);

			var apportionmentMethodOverride = chargeCode.ApportionmentMethodOverrides.AddNew();
			apportionmentMethodOverride.AAM_ApportionmentMethod = AllocationMethod.Manual;
			Factory.Save();
			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, chargeCode.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				apportionmentMethodOverride.AAM_ApportionmentMethod = AllocationMethod.Shipment;
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				chargeCode.ApportionmentMethodOverrides.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new AccChargeApportionmentMethodOverrideCollection(Factory.NewWithValidTestData<AccChargeCode>());
	}
}
