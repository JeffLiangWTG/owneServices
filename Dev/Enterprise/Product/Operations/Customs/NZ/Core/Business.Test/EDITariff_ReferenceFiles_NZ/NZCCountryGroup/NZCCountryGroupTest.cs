namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing
{
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(NZCCountryGroup))]
	class NZCCountryGroupTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetGroupNameProperty()
		{
			NZCGroup group = Factory.New<NZCGroup>();
			group.Q4_Code = "XXX";
			group.Q4_Name = "XXX Group Name";

			NZCCountryGroup countryGroup = Factory.New<NZCCountryGroup>();
			countryGroup.U4_Group = "XXX";
			AssertEquals("countryGroup.U4_GroupName", "XXX Group Name", countryGroup.U4_GroupName);
		}
	}
}
