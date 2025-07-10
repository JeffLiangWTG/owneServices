using Enterprise.Integration.Recruiter;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbAccreditationGroupWrapper))]
	sealed class GlbAccreditationGroupWrapperTestCase : GlbAccreditationTreeBizObjWrapperTestCase<GlbAccreditationGroupWrapper>
	{
		protected override GlbAccreditationTreeBizObjWrapperBase GetWrapper(GlbAccreditationTreeModel model, GlbPerson person)
		{
			var group = Factory.New<IGlbAccreditationJobSkillGroup>();
			return new GlbAccreditationGroupWrapper(model, group, person);
		}
	}
}
