using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterData.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class RelatedOrgPartyScreeningStatusCollection : ActiveBusinessObjectCollection<RelatedOrgPartyScreeningStatus>, IRelatedOrgPartyScreeningStatusCollection
	{
		public RelatedOrgPartyScreeningStatusCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
			ApplySort(StmEntityScreeningLogSchema.PJ_SystemCreateTimeUtc.Name, ListSortDirection.Descending);
		}

		IRelatedOrgPartyScreeningStatus IRelatedOrgPartyScreeningStatusCollection.this[int index] => base[index];

		protected override bool AllowNew => false;

		protected override void OnAdded(RelatedOrgPartyScreeningStatus businessObject)
		{
			base.OnAdded(businessObject);
			ApplySort(StmEntityScreeningLogSchema.PJ_SystemCreateTimeUtc.Name, ListSortDirection.Descending);
		}
	}
}

