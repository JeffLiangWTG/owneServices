using CargoWise.EntityFramework;
using Enterprise.Integration.Recruiter;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationJobSkillPivotCollection : BusinessObjectCollection<GlbAccreditationJobSkillPivot>, IGlbAccreditationJobSkillPivotCollection
	{
		public GlbAccreditationJobSkillPivotCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		readonly GlbAccreditationJobSkillGroup group;

		public GlbAccreditationJobSkillPivotCollection(GlbAccreditationJobSkillGroup group)
			: base(group.Factory, new ZQuery(GlbAccreditationJobSkillPivotSchema.HAJ_HJG, group.PK))
		{
			this.@group = group;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			var newPivot = child as GlbAccreditationJobSkillPivot;
			if (newPivot == null || group == null)
			{
				return;
			}

			newPivot.HAJ_HJG = group.PK;
		}

		IGlbAccreditationJobSkillPivot IGlbAccreditationJobSkillPivotCollection.this[int i]
		{
			get { return base[i]; }
		}

		void IGlbAccreditationJobSkillPivotCollection.Reload()
		{
			Reload(true);
		}
	}
}
