using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UNDGAttributeZZLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLanguages()
		{
			AssertSame(lookups.Languages, Factory.GetCachedValue("UNDGAttribute_Languages", () => new CodeDescriptionPairList()));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var attribute = Factory.New<UNDGAttributeZZ>();
			lookups = new UNDGAttributeZZLookups(attribute);
		}
		UNDGAttributeZZLookups lookups;
	}
}
