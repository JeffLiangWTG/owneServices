using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(GlbAccreditationJobSkillGroupCollection))]
	sealed class GlbAccreditationJobSkillGroupCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlbAccreditationJobSkillGroupCollection(Factory);
		}
	}
}
