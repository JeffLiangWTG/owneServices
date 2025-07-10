using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class GlbExternalPasswordWithPasswordTypeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPasswordTypeList()
		{
			var bizo = Factory.New<DummyGlbExternalPasswordWithPasswordType>();

			AssertNotNull("PasswordStatusList should not be null", bizo.Lookups.PasswordTypeList);
			AssertEquals("PasswordStatusList.Count", 1, bizo.Lookups.PasswordTypeList.Count);

			AssertEquals("1st Element (Code)", "TST", bizo.Lookups.PasswordTypeList[0].Code);
			AssertEquals("1st Element (Description)", "Test Description", bizo.Lookups.PasswordTypeList[0].Description);
		}

		class DummyGlbExternalPasswordWithPasswordType : GlbExternalPasswordWithPasswordType
		{
			public DummyGlbExternalPasswordWithPasswordType(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override string PasswordTypeCode => "TST";

			public override string PasswordTypeDescription => "Test Description";
		}
	}
}
