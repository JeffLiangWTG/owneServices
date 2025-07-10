using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration.Recruiter;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationJobSkillPivot : AutoGlbAccreditationJobSkillPivot, IGlbAccreditationJobSkillPivot
	{
		public GlbAccreditationJobSkillPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public GlbAccreditationJobSkillGroup JobSkillGroup
		{
			get { return Factory.Load<GlbAccreditationJobSkillGroup>(HAJ_HJG); }
		}
	}
}
