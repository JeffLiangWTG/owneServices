using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeSupplyTypeOverrideCollection))]
	sealed class AccChargeSupplyTypeOverrideCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestNoAuditLogsWithAttibute()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			var supplyTypeOverride = chargeCode.SupplyTypeOverrides.AddNew();
			supplyTypeOverride.ACS_JobType = "SHP";
			supplyTypeOverride.ACS_Direction = Core.Constants.FreightShipmentDirection.Code.Domestic;
			supplyTypeOverride.ACS_TransportMode = "AIR";
			supplyTypeOverride.ACS_IncoTerm = "DAT";
			supplyTypeOverride.ACS_GE = GlbDepartment.CurrentDepartment.PK;
			supplyTypeOverride.ACS_SupplyType = "LOA";
			Factory.Save();
			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, chargeCode.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				supplyTypeOverride.ACS_JobType = "ALL";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				chargeCode.SupplyTypeOverrides.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		public void TestDefaults()
		{
			var collection = (AccChargeSupplyTypeOverrideCollection)GetCollectionToTest();
			var item = collection.AddNew();
			AssertEquals("AO_TaxRegCntryOrGroup", AccChargeTaxOverride.ALL, item.ACS_JobType);
		}

		public void TestAddRemove()
		{
			var collection = (AccChargeSupplyTypeOverrideCollection)GetCollectionToTest();
			var item = collection.AddNew();
			AssertEquals("Add: AO_ParentID", collection.Master.PK, item.ACS_ParentID);
			AssertEquals("Add: AO_ParentTableCode", collection.Master.TablePrefix, item.ACS_ParentTableCode);

			collection.RemoveAll();
			AssertEquals("Remove: AO_ParentID", ZGuid.Empty, item.ACS_ParentID);
			AssertEquals("Remove: AO_ParentTableCode", "", item.ACS_ParentTableCode);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccChargeSupplyTypeOverrideCollection(Factory.NewWithValidTestData<AccChargeCode>());
		}
	}
}
