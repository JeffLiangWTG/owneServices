using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmFeatureTest))]
	sealed class StmFeatureTestTest : EnterpriseBusinessObjectTestCase
	{
		public void TestShortcutNameShouldIncludeFeatureCodeAndGroupDescription()
		{
			var winzorFeature = Factory.NewWithValidTestData<StmFeatureTest>();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			winzorFeature.SFT_FeatureName = "WINZOR";
			group.GG_Code = "WZT";
			group.GG_Desc = "User group for Winzor tests";
			winzorFeature.SFT_GG_Group = group.PK;

			AssertEquals("Feature Test - WINZOR - User group for Winzor tests", winzorFeature.HumanReadableShortcutName);
		}
	}
}
