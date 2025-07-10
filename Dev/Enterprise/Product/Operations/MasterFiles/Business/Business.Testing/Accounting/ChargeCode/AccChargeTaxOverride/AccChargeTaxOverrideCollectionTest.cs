using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class AccChargeTaxOverrideCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestNoAuditLogsWithAttibute()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			var taxOverride = chargeCode.TaxOverrides.AddNew();
			taxOverride.AO_Direction = "IMP";
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_JobType = "ALL";
			taxOverride.AO_CostSellAll = "ALL";
			taxOverride.AO_Origin = "ALX";
			taxOverride.AO_Destination = "DE";
			taxOverride.AO_TaxRegCntryOrGroup = "EUX";
			AccTaxRate rate = Factory.LoadTop1<AccTaxRate>(new ZQuery());
			taxOverride.AO_AT = rate.PK;
			taxOverride.AO_CustomsStatus = "PMT";
			Factory.Save();

			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, chargeCode.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				taxOverride.AO_CostSellAll = "COS";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				chargeCode.TaxOverrides.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		public void TestDefaults()
		{
			AccChargeTaxOverrideCollection col = (AccChargeTaxOverrideCollection)GetCollectionToTest();
			AccChargeTaxOverride item = col.AddNew();
			AssertEquals("AO_TaxRegCntryOrGroup", AccChargeTaxOverride.ALL, item.AO_TaxRegCntryOrGroup);
		}

		public void TestAddRemove()
		{
			AccChargeTaxOverrideCollection collection = (AccChargeTaxOverrideCollection)GetCollectionToTest();
			AccChargeTaxOverride item = collection.AddNew();
			AssertEquals("Add: AO_ParentID", collection.Master.PK, item.AO_ParentID);
			AssertEquals("Add: AO_ParentTableCode", collection.Master.TablePrefix, item.AO_ParentTableCode);

			collection.RemoveAll();
			AssertEquals("Remove: AO_ParentID", ZGuid.Empty, item.AO_ParentID);
			AssertEquals("Remove: AO_ParentTableCode", "", item.AO_ParentTableCode);
		}
	}
}
