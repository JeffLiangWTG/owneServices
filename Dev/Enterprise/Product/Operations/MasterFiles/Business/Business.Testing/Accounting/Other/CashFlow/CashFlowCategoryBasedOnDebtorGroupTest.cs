using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CashFlowCategoryBasedOnDebtorGroup))]
	sealed class CashFlowCategoryBasedOnDebtorGroupTest : CashFlowCategoryBasedOnOrgGroupTest<CashFlowCategoryBasedOnDebtorGroup, CashFlowCategoryBasedOnDebtorGroupCollection>
	{
		public override void TestGetOrgGroupList()
		{
			item.OrgGroupList.Load();
			foreach (var element in item.OrgGroupList)
			{
				AssertEquals(typeof(OrgDebtorGroup), element.GetType());
			}
		}

		public override void TestGetOrgGroupDescriptionCore()
		{
			var factory = new BusinessObjectFactory();
			OrgDebtorGroup debtorGroup = factory.LoadTop1<OrgDebtorGroup>(new ZQuery());
			AssertNotNull(item);
			AssertEquals(debtorGroup.OJ_Desc, item.OrgGroupDescription);
		}

		public override CashFlowCategoryBasedOnDebtorGroup PopulateCashFlowCategoryBasedOnOrgGroupTestData()
		{
			CashFlowCategoryBasedOnDebtorGroup configuration = new CashFlowCategoryBasedOnDebtorGroup();
			var factory = new BusinessObjectFactory();
			OrgDebtorGroup debtorGroup = factory.LoadTop1<OrgDebtorGroup>(new ZQuery());
			configuration.OrgGroupPK = debtorGroup.PK;
			configuration.CashFlowCategory = "O01";
			return configuration;
		}
	}
}
