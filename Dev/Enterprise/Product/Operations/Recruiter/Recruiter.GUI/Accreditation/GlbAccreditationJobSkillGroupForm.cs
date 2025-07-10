using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.GUI
{
	public partial class GlbAccreditationJobSkillGroupForm : ZChildForm
	{
		public GlbAccreditationJobSkillGroupForm(GlbAccreditationJobSkillGroup group)
			: base(group)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
		}

		public override string FormCaption
		{
			get { return Res.GetString("B5C480F6-5347-41CC-B3D9-D5B2BAB736B3", "Skill Requirement Group"); }
		}
	}
}
