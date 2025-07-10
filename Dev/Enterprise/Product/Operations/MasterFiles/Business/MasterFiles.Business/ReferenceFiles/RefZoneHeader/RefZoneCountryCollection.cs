using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefCountry)]
	public class RefZoneCountryCollection : ManyToManyBusinessObjectCollection<RefCountry, RefZoneHeader>
	{
		public RefZoneCountryCollection(RefZoneHeader zone)
			: base(zone)
		{
		}

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
				pivot.F2_ParentTableCode = RefCountrySchema.Constants.Prefix;
			}
		}

		protected override ZQuery RelationshipBusinessObjectsFilter
		{
			get
			{
				ZQuery query = base.RelationshipBusinessObjectsFilter;
				query.AddToFilter(RefZonePivotSchema.F2_ParentTableCode, RefCountrySchema.Constants.Prefix);
				return query;
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public override bool ReadOnly => ((RefZoneHeader)fAssociatedObject).ReadOnly || ((RefZoneHeader)fAssociatedObject).FZ_ZoneType == RefZoneHeaderLookups.ZoneTypeCodes.Schedules;
	}
}
