using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UNDGSubstanceRIDLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLanguages()
		{
			AssertSame(lookups.Languages, Factory.GetCachedValue("UNDGAttribute_Languages", () => new CodeDescriptionPairList()));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var substanceRID = Factory.New<UNDGSubstanceRID>();
			lookups = new UNDGSubstanceRIDLookups(substanceRID);
		}
		UNDGSubstanceRIDLookups lookups;
	}
}
