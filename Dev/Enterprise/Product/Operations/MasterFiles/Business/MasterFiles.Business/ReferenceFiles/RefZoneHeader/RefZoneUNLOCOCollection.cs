using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefUNLOCO)]
	public class RefZoneUNLOCOCollection : ManyToManyBusinessObjectCollection<RefUNLOCO, RefZoneHeader>
	{
		public RefZoneUNLOCOCollection(RefZoneHeader zone)
			: base(zone)
		{
			this.Master = zone;
		}

		readonly RefZoneHeader Master;

		protected override Type TypeOfRelationshipBusinessObject
		{
			get { return typeof(RefZonePivot); }
		}

		protected override SchemaGuidColumn PivotTableFKToCollectionBusinessObjects
		{
			get { return RefZonePivotSchema.F2_ParentID; }
		}

		protected override SchemaGuidColumn PivotTableFKToAssociatedBusinessObject
		{
			get { return RefZonePivotSchema.F2_FZ; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			RefZonePivot pivot = (RefZonePivot)GetRelationshipBusinessObject(bizOAdded);
			if (!pivot.IsInDatabase)
			{
				pivot.F2_ParentTableCode = RefUNLOCOSchema.Constants.Prefix;
			}
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

		public override void Load()
		{
			if (RelationshipFilter.IsEmpty)
			{
				IsLoaded = true;
				RemoveAllButLeaveRelationshipsIntact();
			}
			else
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(RefUNLOCO));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(RefZonePivot), RefZonePivotSchema.F2_ParentID);
				subQuery.AddToFilter(RefZonePivotSchema.F2_FZ, Master.PK);
				query.AddSubQuery(subQuery, JoinCondition.And);

				RefUNLOCO[] locos = Factory.Load<RefUNLOCO>(query);

				UseQuickLoadRelationshipBusinessObjectFor = true;

				AddRange(locos);

				UseQuickLoadRelationshipBusinessObjectFor = false;

				IsLoaded = true;
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public override bool ReadOnly => ((RefZoneHeader)fAssociatedObject).ReadOnly;
	}
}
