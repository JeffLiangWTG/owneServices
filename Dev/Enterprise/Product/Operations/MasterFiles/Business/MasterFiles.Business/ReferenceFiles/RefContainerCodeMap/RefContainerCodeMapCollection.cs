using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefContainerCodeMapCollection : BusinessObjectCollection<RefContainerCodeMap>
	{
		public RefContainerCodeMapCollection(RefContainer refContainer) : base(refContainer.Factory)
		{
			Container = refContainer;
		}

		readonly RefContainer Container;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((RefContainerCodeMap)child).RCM_RC_Container = Container.PK;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(JoinCondition.And, RefContainerCodeMapSchema.RCM_RC_Container, Container.PK);
			return result;
		}
	}
}
