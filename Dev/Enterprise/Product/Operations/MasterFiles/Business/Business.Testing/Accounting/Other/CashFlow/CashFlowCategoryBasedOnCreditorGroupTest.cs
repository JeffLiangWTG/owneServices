using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CashFlowCategoryBasedOnCreditorGroup))]
	sealed class CashFlowCategoryBasedOnCreditorGroupTest : CashFlowCategoryBasedOnOrgGroupTest<CashFlowCategoryBasedOnCreditorGroup, CashFlowCategoryBasedOnCreditorGroupCollection>
	{
		public override void TestGetOrgGroupList()
		{
			item.OrgGroupList.Load();
			foreach (var element in item.OrgGroupList)
			{
				AssertEquals(typeof(OrgCreditorGroup), element.GetType());
			}
		}

		public override void TestGetOrgGroupDescriptionCore()
		{
			var factory = new BusinessObjectFactory();
			OrgCreditorGroup creditorGroup = factory.LoadTop1<OrgCreditorGroup>(new ZQuery());
			AssertNotNull(item);
			AssertEquals(creditorGroup.OG_Desc, item.OrgGroupDescription);
		}

		public override CashFlowCategoryBasedOnCreditorGroup PopulateCashFlowCategoryBasedOnOrgGroupTestData()
		{
			CashFlowCategoryBasedOnCreditorGroup configuration = new CashFlowCategoryBasedOnCreditorGroup();
			var factory = new BusinessObjectFactory();
			OrgCreditorGroup creditorGroup = factory.LoadTop1<OrgCreditorGroup>(new ZQuery());
			configuration.OrgGroupPK = creditorGroup.PK;
			configuration.CashFlowCategory = "O01";
			return configuration;
		}
	}
}
