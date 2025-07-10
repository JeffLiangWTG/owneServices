using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ImportersControlledGroupName))]
	sealed class ImportersControlledGroupNameTest : Customs.Business.Testing.CusCodeDataTest<ImportersControlledGroupName>
	{
		public void TestProperties()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(org);
			var collection = new ImportersControlledGroupNameCollection(wrapper.CountryData);
			var controlledGroupName = collection.AddNew();
			controlledGroupName.US_GroupName = "AbCdEfG";
			AssertEquals("US_GroupName", "ABCDEFG", controlledGroupName.US_GroupName);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var loadedGroupName = newFactory.Load<ImportersControlledGroupName>(controlledGroupName.PK);
			AssertEquals("US_GroupName", "ABCDEFG", loadedGroupName.US_GroupName);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<ImportersControlledGroupName>();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => OrgHeaderWrapper.New(factory.NewWithValidTestData<OrgHeader>()).ImportersControlledGroupNames.AddNew();
	}
}
