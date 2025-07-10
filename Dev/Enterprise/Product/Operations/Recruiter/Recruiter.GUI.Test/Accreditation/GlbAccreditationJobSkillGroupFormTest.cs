using System.Windows.Forms;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(GlbAccreditationJobSkillGroupForm))]
	public class GlbAccreditationJobSkillGroupFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var group = Factory.NewWithValidTestData<GlbAccreditationJobSkillGroup>();
			Factory.Save();
			return new GlbAccreditationJobSkillGroupForm(group);
		}
	}
}
