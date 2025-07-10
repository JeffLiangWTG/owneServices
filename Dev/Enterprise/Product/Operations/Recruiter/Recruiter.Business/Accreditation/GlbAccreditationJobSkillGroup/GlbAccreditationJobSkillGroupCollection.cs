using CargoWise.EntityFramework;
using Enterprise.Integration.Recruiter;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationJobSkillGroupCollection : BusinessObjectCollection<GlbAccreditationJobSkillGroup>, IGlbAccreditationJobSkillGroupCollection
	{
		readonly BusinessObject parent;

		public GlbAccreditationJobSkillGroupCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbAccreditationJobSkillGroupCollection(BusinessObject parent)
		: base(parent.Factory,
			new ZQuery(GlbAccreditationJobSkillGroupSchema.HJG_ParentID, parent.PK).AddToFilter(
				GlbAccreditationJobSkillGroupSchema.HJG_ParentTableCode, parent.TablePrefix))
		{
			this.parent = parent;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			var newGroup = child as GlbAccreditationJobSkillGroup;
			if (newGroup == null || parent == null)
			{
				return;
			}

			newGroup.HJG_ParentID = parent.PK;
			newGroup.HJG_ParentTableCode = parent.TablePrefix;
		}

		IGlbAccreditationJobSkillGroup IGlbAccreditationJobSkillGroupCollection.AddNew()
		{
			var group = AddNew();
			group.HJG_ParentID = parent.PK;
			group.HJG_ParentTableCode = parent.TablePrefix;

			return group;
		}

		IGlbAccreditationJobSkillGroup IGlbAccreditationJobSkillGroupCollection.this[int i]
		{
			get { return base[i]; }
		}

		void IGlbAccreditationJobSkillGroupCollection.Reload()
		{
			Reload(true);
		}
	}
}
