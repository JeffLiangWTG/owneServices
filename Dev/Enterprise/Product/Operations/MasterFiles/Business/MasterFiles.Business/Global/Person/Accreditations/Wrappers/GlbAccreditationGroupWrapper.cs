using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class GlbAccreditationGroupWrapper : GlbAccreditationTreeBizObjWrapperBase
	{
		readonly GlbPerson person;
		readonly IGlbAccreditationAttempt attempt;

		public GlbAccreditationGroupWrapper(ZTreeModel<GlbAccreditationTreeBizObjWrapperBase> treeModel, IGlbAccreditationJobSkillGroup group, GlbPerson person, IGlbAccreditationAttempt attempt = null)
			: base(treeModel, group)
		{
			Group = group;
			this.person = person;
			this.attempt = attempt;
		}

		public IGlbAccreditationJobSkillGroup Group { get; }

		public override ZString Description => Group.HJG_Description;
		public override ZString CommenceDateUtc => ZString.Empty;
		public override ZString CompletionDateUtc => ZString.Empty;
		public override ZString Score => ZString.Empty;
		public override ZString Progress => ZString.Empty;
		public override ZString CompetencyRules => Res.GetString("9A9440E5-9D9D-47C0-801C-03FBFC771B2B", "Complete at least {0}", Group.HJG_Threshold);
		public override ZString ApplicantEmail => ZString.Empty;
		public override ZString Comment => ZString.Empty;

		public override GlbAccreditationTreeBizObjWrapperBase[] Children
		{
			get
			{
				var result = new List<GlbAccreditationTreeBizObjWrapperBase>();

				foreach (IGlbAccreditationJobSkillGroup subGroup in Group.Groups)
				{
					result.Add(new GlbAccreditationGroupWrapper(TreeModel, subGroup, person, attempt));
				}

				return result.ToArray();
			}
		}
	}
}
