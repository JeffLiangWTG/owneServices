using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccGroups))]
	sealed class AccGroupsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableNameCore()
		{
			var accGroups = Factory.New<AccGroups>();
			accGroups.AR_Code = "TAX";
			accGroups.AR_Desc = "No Tax";

			AssertEquals("Sales/Expense Groups - TAX - No Tax", accGroups.HumanReadableName);
		}

		public void TestNoStmALogs()
		{
			var group = Factory.NewWithValidTestData<AccGroups>();
			Factory.Save();
			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, group.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				group.AR_Desc = "TEST LOG";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				group.Delete();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		public void TestAR_DescMultilingual()
		{
			var group = Factory.NewWithValidTestData<AccGroups>();

			using (var mockData = Res.UseMockData())
			{
				var customDataResourceStrings = group.AR_DescInfo.CustomizableDataResourceStrings;
				var key1 = customDataResourceStrings.GetMultilingualString(group, "group1").ResourceKey;
				var key2 = customDataResourceStrings.GetMultilingualString(group, "group2").ResourceKey;
				mockData.Put(key1, new ResourceStringData(key1, "组1"));
				mockData.Put(key2, new ResourceStringData(key2, "组2"));

				group.AR_Desc = "group1";
				AssertEquals("组1", group.AR_DescMultilingual);

				group.AR_Desc = "group2";
				AssertEquals("组2", group.AR_DescMultilingual);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}
	}
}
