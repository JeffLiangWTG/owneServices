using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	#region Enums

	public enum ActiveStatusEnum
	{
		Active,
		Inactive,
		Combined
	}

	#endregion

	public class LocationFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filter

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			return filters;
		}

		protected override void OnModuleFiltersCreated()
		{
			base.OnModuleFiltersCreated();
			UpdateLocationType();
		}

		protected override bool ShouldAddUserDefinedFiltersCore => false;

		protected override bool SafeToAddCustomSqlFilter
		{
			get
			{
				return true;
			}
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var locationTypeFilter = filters.AddTextFilter("Location Type", val => GetLocationTypeQuery(LocationType, NeedsActiveQuery()), LocationTypeList);
			locationTypeFilter.Property = Core.Constants.LocationTypes.Codes.Port;
			locationTypeFilter.Visibility = FilterVisibility.AlwaysVisible;
			locationTypeFilter.PropertyInfo.ValueChanged += new EventHandler(LocationPropertyInfo_ValueChanged);
			locationTypeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|LocationFilterFilter|LocationType", "Location Type");

			var activeStatusString = ResString.GetMultilingualString("c00a4805-7826-4c0a-bd02-b1093a29929a", "Active Status");
			var filter = filters.AddTextFilter(activeStatusString.GetUnresolvedString(), GetActiveStatusQuery, CancelledStatusList);
			filter.MultilingualDescription = activeStatusString;

			filters.AddTextFilter("Code", (oper, code) => GetLocationCodeQuery(oper, code, LocationType)).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|LocationFilterFilter|Code", "Code");
			filters.AddTextFilter("Description", (oper, desc) => GetLocationDescQuery(oper, desc, LocationType)).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|LocationFilterFilter|Description", "Description");
		}

		bool NeedsActiveQuery() => !((ModuleTextFilter)this["Active Status"]).IsActive;

		internal static ZQuery GetLocationTypeQuery(LocationTypeEnum locationType, bool needsActiveQuery)
		{
			var query = new ZQuery();

			switch (locationType)
			{
				case LocationTypeEnum.Port:
					if (needsActiveQuery)
					{
						query.AddToFilter(GetActiveStatusQueryFromColumn(ActiveStatusEnum.Active, RefUNLOCOSchema.RL_IsActive));
					}

					break;
				case LocationTypeEnum.InternationalZone:
					//don't add an active query as it is already filtered by buisness object
					query.AddToFilter(RefZoneHeaderSchema.FZ_ZoneType, SQLComparisonOperator.NotEqual, RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean);
					break;
				case LocationTypeEnum.Country:
					if (needsActiveQuery)
					{
						query.AddToFilter(GetActiveStatusQueryFromColumn(ActiveStatusEnum.Active, RefCountrySchema.RN_IsActive));
					}

					break;
			}

			return query;
		}

		ZQuery GetActiveStatusQuery(ZString active)
		{
			var query = new ZQuery();

			var activeStatus = ActiveStatusEnum.Active;

			if (StatusInactive.EqualsUnresolvedOrLocalized(active, ignoreCase: false))
			{
				activeStatus = ActiveStatusEnum.Inactive;
			}
			else if (StatusAll.EqualsUnresolvedOrLocalized(active, ignoreCase: false))
			{
				activeStatus = ActiveStatusEnum.Combined;
			}

			switch (LocationType)
			{
				case LocationTypeEnum.Port:
					query.AddToFilter(GetActiveStatusQueryFromColumn(activeStatus, RefUNLOCOSchema.RL_IsActive));
					break;
				case LocationTypeEnum.InternationalZone:
					query.AddToFilter(GetActiveStatusQueryFromColumn(activeStatus, RefZoneHeaderSchema.FZ_IsActive));
					break;
				case LocationTypeEnum.Country:
					query.AddToFilter(GetActiveStatusQueryFromColumn(activeStatus, RefCountrySchema.RN_IsActive));
					break;
			}
			query.IgnoreActiveFilter = true;
			return query;
		}

		static ZQuery GetActiveStatusQueryFromColumn(ActiveStatusEnum activeType, SchemaBoolColumn column)
		{
			if (activeType == ActiveStatusEnum.Active)
			{
				return new ZQuery(column, true);
			}
			else if (activeType == ActiveStatusEnum.Inactive)
			{
				return new ZQuery(column, false);
			}
			else
			{
				return new ZQuery();
			}
		}

		internal static ZQuery GetLocationCodeQuery(SQLComparisonOperator comparisonOperator, ZString code, LocationTypeEnum locationType)
		{
			ZQuery query = null;
			switch (locationType)
			{
				case LocationTypeEnum.Port:
					query = new ZQuery(RefUNLOCOSchema.RL_Code, comparisonOperator, code.SubstringSafe(0, RefUNLOCOSchema.RL_Code.MaxLength));
					break;

				case LocationTypeEnum.InternationalZone:
					query = new ZQuery(RefZoneHeaderSchema.FZ_Code, comparisonOperator, code.SubstringSafe(0, RefZoneHeaderSchema.FZ_Code.MaxLength));
					query.AddToFilter(RefZoneHeaderSchema.FZ_ZoneType, SQLComparisonOperator.NotEqual, RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean);
					break;

				case LocationTypeEnum.Country:
					query = new ZQuery(RefCountrySchema.RN_Code, comparisonOperator, code.SubstringSafe(0, RefCountrySchema.RN_Code.MaxLength));
					break;
			}

			return query;
		}

		internal static ZQuery GetLocationDescQuery(SQLComparisonOperator comparisonOperator, ZString desc, LocationTypeEnum locationType)
		{
			ZQuery query = null;
			switch (locationType)
			{
				case LocationTypeEnum.Port:
					query = new ZQuery(RefUNLOCOSchema.RL_PortName, comparisonOperator, desc.SubstringSafe(0, RefUNLOCOSchema.RL_PortName.MaxLength));
					break;

				case LocationTypeEnum.InternationalZone:
					query = new ZQuery(RefZoneHeaderSchema.FZ_Description, comparisonOperator, desc.SubstringSafe(0, RefZoneHeaderSchema.FZ_Description.MaxLength));
					query.AddToFilter(RefZoneHeaderSchema.FZ_ZoneType, SQLComparisonOperator.NotEqual, RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean);
					break;

				case LocationTypeEnum.Country:
					query = new ZQuery(RefCountrySchema.RN_Desc, comparisonOperator, desc.SubstringSafe(0, RefCountrySchema.RN_Desc.MaxLength));
					break;
			}

			return query;
		}

		void LocationPropertyInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateLocationType();
		}

		void UpdateLocationType()
		{
			var locationtype = ((ModuleTextFilter)this["Location Type"]).Property;
			var sqlFilter = this[CustomSqlFilterDescription] as ModuleSQLFilter;
			switch (locationtype)
			{
				case Core.Constants.LocationTypes.Codes.Zone:
					LocationType = LocationTypeEnum.InternationalZone;
					if (sqlFilter != null)
					{
						sqlFilter.QueryObjectType = typeof(RefZoneHeader);
					}

					break;

				case Core.Constants.LocationTypes.Codes.Country:
					LocationType = LocationTypeEnum.Country;
					if (sqlFilter != null)
					{
						sqlFilter.QueryObjectType = typeof(RefCountry);
					}

					break;

				default:
					LocationType = LocationTypeEnum.Port;
					if (sqlFilter != null)
					{
						sqlFilter.QueryObjectType = typeof(RefUNLOCO);
					}

					break;
			}
		}

		LocationTypeEnum locationType = LocationTypeEnum.Port;

		public LocationTypeEnum LocationType
		{
			get { return locationType; }
			private set { locationType = value; }
		}

		#endregion

		#endregion

		#region Lookups

		CodeDescriptionPairList LocationTypeList
		{
			get
			{
				if (fLocationTypeList == null)
				{
					fLocationTypeList = new CodeDescriptionPairList();

					fLocationTypeList.AddPair(Core.Constants.LocationTypes.Codes.Port, Core.Constants.LocationTypes.Descriptions.Port);

					if (LocationCollection != null && LocationCollection.AllowCountries)
					{
						fLocationTypeList.AddPair(Core.Constants.LocationTypes.Codes.Country, Core.Constants.LocationTypes.Descriptions.Country);
					}

					if (LocationCollection != null && LocationCollection.AllowZones)
					{
						fLocationTypeList.AddPair(Core.Constants.LocationTypes.Codes.Zone, Core.Constants.LocationTypes.Descriptions.Zone);
					}
				}

				return fLocationTypeList;
			}
		}

		CodeDescriptionPairList fLocationTypeList;

		[BusinessObjectTestExclude]
		public LocationCollection LocationCollection
		{
			get { return locationCollection ?? (locationCollection = new LocationCollection(Factory)); }
			set { locationCollection = value; }
		}

		LocationCollection locationCollection;

		#endregion
	}
}
