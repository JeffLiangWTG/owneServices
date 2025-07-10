using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPatternMatchOverrideCollection))]
	sealed class OrgPatternMatchOverrideCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			return organisation.PatternMatchOverrides_ForBinding;
		}
	}
}
