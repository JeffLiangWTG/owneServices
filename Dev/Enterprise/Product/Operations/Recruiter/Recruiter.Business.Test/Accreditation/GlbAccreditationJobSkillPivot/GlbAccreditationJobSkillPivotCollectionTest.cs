using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(GlbAccreditationJobSkillPivotCollection))]
	sealed class GlbAccreditationJobSkillPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var group = Factory.New<GlbAccreditationJobSkillGroup>();
			return new GlbAccreditationJobSkillPivotCollection(group);
		}
	}
}
