using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ViewUNDGAttributeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypes()
		{
			AssertEquals("PSN, OBS, PRP, QDT, OTN", lookups.Types.CodesAsString);
		}

		public void TestUNDGSubstances()
		{
			AssertType<UNDGSubstanceCollection>(lookups.UNDGSubstances);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var viewAttribute = Factory.New<ViewUNDGAttribute>();
			lookups = new ViewUNDGAttributeLookups(viewAttribute);
		}
		ViewUNDGAttributeLookups lookups;
	}
}
