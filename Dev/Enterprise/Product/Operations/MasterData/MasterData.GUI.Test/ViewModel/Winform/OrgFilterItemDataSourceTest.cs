using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Test
{
	[TestedType(typeof(OrgFilterItemDataSource))]
	public class OrgFilterItemDataSourceTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOrgFilterType()
		{
			var dataSource = new OrgFilterItemDataSource();
			dataSource.OrgFilterType = OrgFilterTypeList.Descriptions.MainUNLOCO.GetUnresolvedString();
			AssertEquals(OrgFilterTypeList.Descriptions.MainUNLOCO, dataSource.OrgFilterTypeDescription);

			dataSource.OrgFilterType = OrgFilterTypeList.Descriptions.Name.GetUnresolvedString();
			AssertEquals(OrgFilterTypeList.Descriptions.Name, dataSource.OrgFilterTypeDescription);

			dataSource.OrgFilterType = OrgFilterTypeList.Descriptions.OrgTypes.GetUnresolvedString();
			AssertEquals(OrgFilterTypeList.Descriptions.OrgTypes, dataSource.OrgFilterTypeDescription);

			dataSource.OrgFilterType = OrgFilterTypeList.Descriptions.Email.GetUnresolvedString();
			AssertEquals(OrgFilterTypeList.Descriptions.Email, dataSource.OrgFilterTypeDescription);
		}

		public void TestOrgFilterOption()
		{
			var dataSource = new OrgFilterItemDataSource();
			dataSource.OrgFilterOption = OrgFilterOptionList.Descriptions.Contains.GetUnresolvedString();
			AssertEquals(OrgFilterOptionList.Descriptions.Contains, dataSource.OrgFilterOptionDescription);

			dataSource.OrgFilterOption = OrgFilterOptionList.Descriptions.ExactMatch.GetUnresolvedString();
			AssertEquals(OrgFilterOptionList.Descriptions.ExactMatch, dataSource.OrgFilterOptionDescription);

			dataSource.OrgFilterOption = OrgFilterOptionList.Descriptions.NotContain.GetUnresolvedString();
			AssertEquals(OrgFilterOptionList.Descriptions.NotContain, dataSource.OrgFilterOptionDescription);

			dataSource.OrgFilterOption = OrgFilterOptionList.Descriptions.NotEqual.GetUnresolvedString();
			AssertEquals(OrgFilterOptionList.Descriptions.NotEqual, dataSource.OrgFilterOptionDescription);

			dataSource.OrgFilterOption = OrgFilterOptionList.Descriptions.NotStartWith.GetUnresolvedString();
			AssertEquals(OrgFilterOptionList.Descriptions.NotStartWith, dataSource.OrgFilterOptionDescription);

			dataSource.OrgFilterOption = OrgFilterOptionList.Descriptions.StartsWith.GetUnresolvedString();
			AssertEquals(OrgFilterOptionList.Descriptions.StartsWith, dataSource.OrgFilterOptionDescription);
		}
	}
}
