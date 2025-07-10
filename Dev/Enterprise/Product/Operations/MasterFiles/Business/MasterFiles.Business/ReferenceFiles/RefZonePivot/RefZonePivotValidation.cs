using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefZonePivotValidation : AutoRefZonePivotValidation
	{
		public RefZonePivotValidation(AutoRefZonePivot parent)
			: base(parent)
		{
		}

		public new RefZonePivot Parent
		{
			get { return (RefZonePivot)base.Parent; }
		}

		#region F2_ParentID

		protected override void CheckF2_ParentID()
		{
			if (Parent.ZoneHeader != null && Parent.Location != null)
			{
				ValidateOtherZoneContainsLocation(Parent.Location, Parent.ZoneHeader);
			}
		}

		readonly HashSet<ZString> ZonesAllowingDuplicateLocationsWithoutWarnings = new HashSet<ZString>
		{
			RefZoneHeaderLookups.ZoneTypeCodes.Tax,
			RefZoneHeaderLookups.ZoneTypeCodes.OriginGateway,
			RefZoneHeaderLookups.ZoneTypeCodes.DestinationGateway
		};

		/// <summary>
		/// Zones types that allow same locations in different zones but there should be warnings about them to show to users.
		/// </summary>
		readonly HashSet<ZString> ZonesAllowingDuplicateLocationsWithWarnings = new HashSet<ZString>
		{
			RefZoneHeaderLookups.ZoneTypeCodes.Rating,
			RefZoneHeaderLookups.ZoneTypeCodes.RatingImport,
			RefZoneHeaderLookups.ZoneTypeCodes.RatingExport
		};

		// Try to use the same factory cache to avoid multiple expensive registry access.
		bool AllowSameUNLOCOInRatingInternationalZones =>
			Parent?.Factory?.GetCachedValue(
				"RefZonePivotValidation|AllowSameUNLOCOInRatingInternationalZones",
				() => RatingDataRegistry.Instance.AllowSameUNLOCOInRatingInternationalZones.Value)
			?? false;

		bool AllowSameUNLOCOForContractInternationalZones =>
			Parent?.Factory?.GetCachedValue(
				"RefZonePivotValidation|AllowSameUNLOCOForContractInternationalZones",
				() => FreightDataRegistry.Instance.AllowSameUNLOCOForContractInternationalZones.Value)
			?? false;

		protected void ValidateOtherZoneContainsLocation(ILocation location, RefZoneHeader refZone)
		{
			if (!refZone.FZ_IsActive)
			{
				return;
			}

			if (ZonesAllowingDuplicateLocationsWithoutWarnings.Contains(refZone.FZ_ZoneType))
			{
				return;
			}

			var query = new ZDBOnlyQuery(typeof(RefZoneHeader));
			query.AddToFilter(RefZoneHeaderSchema.PK, SQLComparisonOperator.NotEqual, refZone.PK);
			query.AddToFilter(RefZoneHeaderSchema.FZ_ZoneType, refZone.FZ_ZoneType);
			query.AddToFilter(RefZoneHeaderSchema.FZ_ZoneMode, refZone.FZ_ZoneMode);
			query.AddToFilter(RefZoneHeaderSchema.FZ_IsActive, true);

			if (Parent.ZoneHeader.FZ_OH_RelatedParty.IsEmpty)
			{
				query.AddToFilter(RefZoneHeaderSchema.FZ_OH_RelatedParty, null);
			}
			else
			{
				query.AddToFilter(RefZoneHeaderSchema.FZ_OH_RelatedParty, refZone.FZ_OH_RelatedParty);
			}

			var subQuery = new ZDBOnlySubQuery(typeof(RefZonePivot), RefZonePivotSchema.F2_FZ);
			subQuery.AddToFilter(RefZonePivotSchema.F2_ParentID, ((BusinessObject)location).PK);
			query.AddSubQuery(subQuery, JoinCondition.And);

			if (refZone.FZ_ZoneType == RefZoneHeaderLookups.ZoneTypeCodes.Contract && AllowSameUNLOCOForContractInternationalZones)
			{
				var otherZones = FactoryForUniquenessQuery.Load<RefZoneHeader>(query);
				if (otherZones.Length < 1)
				{
					return;
				}

				var allMatchingZones = otherZones.Append(refZone);
				var zoneCodes = string.Join(", ", allMatchingZones.Select(zone => zone.FZ_Code));
				Parent.F2_ParentIDInfo.AddWarning(Res.GetString("d3857b82-49a5-44b0-4e29-43ed61f5843b", "{0} is included in multiple Contract International Zones: {1}. It may result in Allocation Routes with different but overlapping International Zones as Load / Discharge Ports being loaded into the ‘Contract & Allocation Routes Search Form’ when launched from relevant jobs.", location.Code, zoneCodes));
			}
			else
			{
				var otherZone = FactoryForUniquenessQuery.LoadTop1<RefZoneHeader>(query);
				if (otherZone == null)
				{
					return;
				}

				if (ZonesAllowingDuplicateLocationsWithWarnings.Contains(refZone.FZ_ZoneType) && AllowSameUNLOCOInRatingInternationalZones)
				{
					Parent.F2_ParentIDInfo.AddWarning(Res.GetString("c4c0d5fb-1524-4b3c-946b-ae5b896c32b4", "{0} is already included in {1} Zone.", location.Code, otherZone.FZ_Code));
				}
				else
				{
					Parent.F2_ParentIDInfo.AddError(Res.GetString("257C7610-3B2B-4FB2-A968-03945567F8B8", "{0} is already included in {1} Zone, therefore overlapping with the current Zone.", location.Code, otherZone.FZ_Code));
				}
			}
		}

		BusinessObjectFactory FactoryForUniquenessQuery
		{
			get { return fFactoryForUniquenessQuery ?? (fFactoryForUniquenessQuery = new BusinessObjectFactory()); }
		}

		BusinessObjectFactory fFactoryForUniquenessQuery;

		#endregion
	}
}
