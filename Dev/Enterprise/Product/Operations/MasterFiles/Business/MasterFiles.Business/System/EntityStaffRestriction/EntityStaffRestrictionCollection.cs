using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class EntityStaffRestrictionCollection : ActiveBusinessObjectCollection<EntityStaffRestriction>
	{
		public EntityStaffRestrictionCollection(BusinessObject parent)
			: base(parent.Factory)
		{
			this.parent = parent;
		}

		readonly BusinessObject parent;

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(EntityStaffRestrictionSchema.ESR_ParentID, parent.PK);
			return result;
		}

		protected override void SetDefaultsForNewElementCore(EntityStaffRestriction newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.ESR_ParentID = parent.PK;
			newElement.ESR_ParentTableCode = parent.TablePrefix;
		}
	}
}
