using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefUNLOCOZoneCollection : ManyToManyBusinessObjectCollection<RefZoneHeader, RefUNLOCO>
	{
		public RefUNLOCOZoneCollection(RefUNLOCO loco)
			: base(loco)
		{
		}

		protected override Type TypeOfRelationshipBusinessObject
		{
			get { return typeof(RefZonePivot); }
		}

		protected override SchemaGuidColumn PivotTableFKToAssociatedBusinessObject
		{
			get { return RefZonePivotSchema.F2_ParentID; }
		}

		protected override SchemaGuidColumn PivotTableFKToCollectionBusinessObjects
		{
			get { return RefZonePivotSchema.F2_FZ; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			((RefZonePivot)GetRelationshipBusinessObject(bizOAdded)).F2_ParentTableCode = RefUNLOCOSchema.Constants.Prefix;
		}

		protected override ZQuery RelationshipBusinessObjectsFilter
		{
			get
			{
				ZQuery query = base.RelationshipBusinessObjectsFilter;
				query.AddToFilter(RefZonePivotSchema.F2_ParentTableCode, RefUNLOCOSchema.Constants.Prefix);
				return query;
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
