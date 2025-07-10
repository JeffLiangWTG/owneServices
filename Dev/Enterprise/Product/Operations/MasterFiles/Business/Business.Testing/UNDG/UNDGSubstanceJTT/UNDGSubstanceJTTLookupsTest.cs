using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UNDGSubstanceJTTLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLanguages()
		{
			AssertSame(lookups.Languages, Factory.GetCachedValue("UNDGAttribute_Languages", () => new CodeDescriptionPairList()));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var substanceJTT = Factory.New<UNDGSubstanceJTT>();
			lookups = new UNDGSubstanceJTTLookups(substanceJTT);
		}
		UNDGSubstanceJTTLookups lookups;
	}
}
