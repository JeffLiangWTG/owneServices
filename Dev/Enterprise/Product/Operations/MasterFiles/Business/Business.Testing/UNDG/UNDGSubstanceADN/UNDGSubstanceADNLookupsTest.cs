using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UNDGSubstanceADNLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLanguages()
		{
			AssertSame(lookups.Languages, Factory.GetCachedValue("UNDGAttribute_Languages", () => new CodeDescriptionPairList()));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var substanceADN = Factory.New<UNDGSubstanceADN>();
			lookups = new UNDGSubstanceADNLookups(substanceADN);
		}
		UNDGSubstanceADNLookups lookups;
	}
}
