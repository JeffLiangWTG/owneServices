using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeRevRecOverrideCollection))]
	sealed class AccChargeRevRecOverrideCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestNoAuditLogsWithAttibute()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			var revRecOverride = chargeCode.RevenueRecOverrides.AddNew();
			revRecOverride.AE_JobType = "SHP";
			revRecOverride.AE_Direction = Core.Constants.FreightShipmentDirection.Code.All;
			revRecOverride.AE_Mode = "AIR";
			revRecOverride.AE_BrokerType = RevenueRecognitionLookups.BrokerCodes.All;
			revRecOverride.AE_RecognitionType = "IMM";
			Factory.Save();

			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, chargeCode.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				revRecOverride.AE_RecognitionType = "ARV";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				chargeCode.RevenueRecOverrides.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			return new AccChargeRevRecOverrideCollection(chargeCode);
		}
	}
}
