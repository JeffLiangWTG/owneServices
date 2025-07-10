using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrgLocatedWithinModuleFilter : ModuleFilter
	{
		#region Constructor
		public OrgLocatedWithinModuleFilter(ZString description, GetTextQueryWithOperator queryDelegate)
		: base(description, queryDelegate)
		{
			DefaultFilterType = SearchTypeUNLOCO;
			DefaultUnit = UnitKilometer;
			DefaultDistance = 15m;
		}

		public OrgLocatedWithinModuleFilter(ZString description, BusinessObjectFactory factory)
		: base(description, factory)
		{
			DefaultFilterType = SearchTypeUNLOCO;
			DefaultUnit = UnitKilometer;
			DefaultDistance = 15m;
		}

		public OrgLocatedWithinModuleFilter(ZString description)
		: this(description, EmptyQuery)
		{
		}

		protected OrgLocatedWithinModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		: base(category, parentCollection) { }
		#endregion

		#region CodeDescriptionPairList

		public static string SearchTypeUNLOCO { get { return Res.GetString("D061CAFB-4F09-4024-9FBF-20F80954FAD8", "UNLOCO"); } }

		public static string SearchTypeOrgAddress { get { return Res.GetString("3E4987EF-4CCB-4764-BFD2-DC93E4F1BC58", "Org. Address"); } }

		CodeDescriptionPairList filterTypeList;

		public CodeDescriptionPairList FilterTypeList
		{
			get
			{
				if (filterTypeList == null)
				{
					filterTypeList = new CodeDescriptionPairList();
					filterTypeList.AddPair(SearchTypeUNLOCO);
					filterTypeList.AddPair(SearchTypeOrgAddress);
				}

				return filterTypeList;
			}
		}

		public const string UnitKilometer = "KM";
		public const string UnitMile = "MI";

		CodeDescriptionPairList unitTypeList;

		public CodeDescriptionPairList UnitTypeList
		{
			get
			{
				if (unitTypeList == null)
				{
					unitTypeList = new CodeDescriptionPairList();
					unitTypeList.AddPair(UnitKilometer, Res.GetString("426C726E-2712-49F8-BB29-1ADDB6A853C8", "Kilometer"));
					unitTypeList.AddPair(UnitMile, Res.GetString("5263774C-D772-4AA7-A39D-EB9E9DF6DCDA", "Mile"));
				}

				return unitTypeList;
			}
		}
		#endregion

		#region DefaultFilterType
		ZString defaultFilterType;

		ZString DefaultFilterType
		{
			get
			{
				return defaultFilterType;
			}
			set
			{
				if (defaultFilterType != value)
				{
					defaultFilterType = value;
					InvalidateCachedQuery();
					FilterType = value;
				}
			}
		}
		#endregion

		#region DefaultUnit
		ZString defaultUnit;

		ZString DefaultUnit
		{
			get
			{
				return defaultUnit;
			}
			set
			{
				if (defaultUnit != value)
				{
					defaultUnit = value;
					InvalidateCachedQuery();
					UnitForGeolocation = value;
				}
			}
		}
		#endregion

		#region DefaulDistance
		ZDecimal defaultDistance;

		ZDecimal DefaultDistance
		{
			get
			{
				return defaultDistance;
			}
			set
			{
				if (defaultDistance != value)
				{
					defaultDistance = value;
					InvalidateCachedQuery();
					Distance = value;
				}
			}
		}

		#endregion

		#region Query

		static ZQuery EmptyQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery();
		}

		protected override ZQuery GetQuery()
		{
			var query = new ZQuery();
			if (!IsEmpty)
			{
				query.AddToFilter(GetQueryCore());
			}

			return query;
		}

		ZQuery GetQueryCore()
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));

			var geoLoacationColumn = new SchemaDecimalColumn(Schema.GenericTableSchema, (NoResString)"Column", 0, SqlDbType.Decimal, 0, false, 10, 2);
			query.IgnoreBlobFieldsCheck = true;
			var paramCollection = new ZSqlParameterCollection {
					ZSqlParameter.New("@geoLocation", CentraGeolocation, OrgAddressSchema.OA_GeoLocation),
					ZSqlParameter.New((NoResString)"@distance", ActualDistance, geoLoacationColumn)
				};
			query.AddFilterAndZSQLParameterCollection(OrgHeaderSchema.Constants.PK + " IN (SELECT OH_PK FROM GetOrgPKsNearTheGeoLocation(@geoLocation, @distance))", paramCollection);

			return query;
		}
		#endregion

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		protected override bool IsEmptyCore
		{
			get
			{
				var result = true;
				if (FilterType == SearchTypeUNLOCO)
				{
					result = UNLOCO.IsEmpty || ActualDistance <= 0 || CentraGeolocation.IsEmpty;
				}
				else if (FilterType == SearchTypeOrgAddress)
				{
					result = orgPK.IsEmpty || AddressPK.IsEmpty || ActualDistance <= 0 || CentraGeolocation.IsEmpty;
				}
				return result;
			}
		}

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Locations; }
		}

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { CentraGeolocation, ActualDistance }; }
		}

		protected override void ClearCore()
		{
			FilterType = DefaultFilterType;
			UNLOCO = Guid.Empty;
			UnitForGeolocation = DefaultUnit;
			Distance = DefaultDistance;
			OrgPK = Guid.Empty;
			AddressPK = Guid.Empty;
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = (OrgLocatedWithinModuleFilter)filterToCopyFrom;
			FilterType = filter.FilterType;
			UNLOCO = filter.UNLOCO;
			AddressPK = filter.AddressPK;
			OrgPK = filter.OrgPK;
			Distance = filter.Distance;
			UnitForGeolocation = filter.UnitForGeolocation;
		}

		#region Test Data Setup
#if DEBUG
		protected override void FillWithValidTestFilterValueCore()
		{
			var point1 = ZGeography.CreatePoint(1, 0, 4326);

			var refUNLOCO1 = Factory.NewWithValidTestData<RefUNLOCO>();
			refUNLOCO1.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			refUNLOCO1.Code = "AABBC";
			refUNLOCO1.RL_GeoLocation = point1;
			refUNLOCO1.Longitude = 1;
			refUNLOCO1.Latitude = 0;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Test001";
			org1.OH_RL_NKClosestPort = refUNLOCO1.Code;
			this.FilterType = SearchTypeUNLOCO;
			this.UNLOCO = refUNLOCO1.PK;
			this.Distance = 10m;
		}
#endif
		#endregion

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new OrgLocatedWithinModuleFilter(category, parentCollection);
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new OrgLocatedWithinModuleFilterValidation(this);
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			throw new NotImplementedException();
		}

		#region XML Serialization

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			writer.WriteElementString(Constants.FilterType, FilterType == SearchTypeUNLOCO ? unloco : orgaddress);
			writer.WriteElementString(Constants.Distance, Distance.ToString());
			writer.WriteElementString(Constants.UnitForGeolocation, UnitForGeolocation.ToString());
			writer.WriteElementString(Constants.OrgPK, OrgPK.ToString());
			writer.WriteElementString(Constants.AddressPK, AddressPK.ToString());
			writer.WriteElementString(Constants.UNLOCO, UNLOCO.ToString());
		}

		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			if (reader.Name == Constants.FilterType)
			{
				FilterType = reader.ReadElementString(Constants.FilterType) == unloco ? SearchTypeUNLOCO : SearchTypeOrgAddress;
			}

			if (reader.Name == Constants.Distance)
			{
				Distance = Convert.ToDecimal(reader.ReadElementString(Constants.Distance), CultureInfo.CurrentCulture);
			}

			if (reader.Name == Constants.UnitForGeolocation)
			{
				UnitForGeolocation = reader.ReadElementString(Constants.UnitForGeolocation);
			}

			if (reader.Name == Constants.OrgPK)
			{
				OrgPK = new ZGuid(reader.ReadElementString(Constants.OrgPK));
			}

			if (reader.Name == Constants.AddressPK)
			{
				AddressPK = new ZGuid(reader.ReadElementString(Constants.AddressPK));
			}

			if (reader.Name == Constants.UNLOCO)
			{
				UNLOCO = new ZGuid(reader.ReadElementString(Constants.UNLOCO));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not to be translated")]
		static class Constants
		{
			public const string Distance = "Distance";
			public const string AddressPK = "AddressPK";
			public const string OrgPK = "OrgPK";
			public const string UNLOCO = "UNLOCO";
			public const string FilterType = "FilterType";
			public const string CentraGeolocation = "CentraGeolocation";
			public const string UnitForGeolocation = "UnitForGeolocation";
		}

		const string unloco = "UNLOCO";
		const string orgaddress = "ORGADDRESS";
		#endregion

		#region Implementation
		#region Distance
		public ZDecimal Distance
		{
			get
			{
				return distance;
			}
			set
			{
				if (SetNonPersistentPropertyValue(DistanceInfo, ref distance, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateDistance();
					}

					distance = value;
					DistanceInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}
		ZDecimal distance;

		public ZPropertyInfo DistanceInfo
		{
			get { return GetZPropertyInfo(Constants.Distance); }
		}

		public ZDecimal ActualDistance
		{
			get
			{
				return UnitForGeolocation == UnitMile ? (ZDecimal)(Distance * 1.609344m * 1000m) : (ZDecimal)(Distance * 1000m);
			}
		}

		#endregion

		#region UnitForGeolocation
		[List("UnitTypeList")]
		public ZString UnitForGeolocation
		{
			get
			{
				return unitForGeolocation;
			}
			set
			{
				if (SetNonPersistentPropertyValue(UnitForGeolocationInfo, ref unitForGeolocation, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateUnitForGeolocation();
						Validation.ValidateDistance();
					}

					unitForGeolocation = value;
					UnitForGeolocationInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}
		ZString unitForGeolocation;

		public ZPropertyInfo UnitForGeolocationInfo
		{
			get { return GetZPropertyInfo(Constants.UnitForGeolocation); }
		}
		#endregion

		#region AddressPK
		public OrgAddressCollection AddressPKList
		{
			get
			{
				var query = new ZQuery(OrgAddressSchema.OA_OH, OrgPK);
				query.AddToFilter(OrgAddressSchema.OA_IsActive, true);
				var collection = new OrgAddressCollection(Factory, query);
				collection.Load();
				return collection;
			}
		}

		[List("AddressPKList")]
		public ZGuid AddressPK
		{
			get
			{
				return addressPK;
			}
			set
			{
				if (SetNonPersistentPropertyValue(AddressPKInfo, ref addressPK, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateAddressPK();
					}

					addressPK = value;
					AddressPKInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}
		ZGuid addressPK;

		public ZPropertyInfo AddressPKInfo
		{
			get { return GetZPropertyInfo(Constants.AddressPK); }
		}
		#endregion

		#region OrgPK
		public OrgHeaderCollection OrgPKList
		{
			get
			{
				return new OrgHeaderCollection(Factory);
			}
		}

		[List("OrgPKList")]
		public ZGuid OrgPK
		{
			get
			{
				return orgPK;
			}
			set
			{
				if (SetNonPersistentPropertyValue(OrgPKInfo, ref orgPK, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateOrgPK();
					}

					orgPK = value;
					AddressPK = Guid.Empty;
					OrgPKInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}
		ZGuid orgPK;

		public ZPropertyInfo OrgPKInfo
		{
			get { return GetZPropertyInfo(Constants.OrgPK); }
		}
		#endregion

		#region UNLOCO
		public RefUNLOCOCollection UNLOCOList
		{
			get
			{
				return new RefUNLOCOCollection(Factory);
			}
		}

		[List("UNLOCOList")]
		public ZGuid UNLOCO
		{
			get
			{
				return uNLOCO;
			}
			set
			{
				if (SetNonPersistentPropertyValue(UNLOCOInfo, ref uNLOCO, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateUNLOCO();
					}

					uNLOCO = value;
					UNLOCOInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}
		ZGuid uNLOCO;

		public ZPropertyInfo UNLOCOInfo
		{
			get { return GetZPropertyInfo(Constants.UNLOCO); }
		}
		#endregion

		#region FilterType
		[List("FilterTypeList")]
		public ZString FilterType
		{
			get
			{
				return filterType;
			}
			set
			{
				if (SetNonPersistentPropertyValue(FilterTypeInfo, ref filterType, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateFilterType();
					}

					filterType = value;
					FilterTypeInfo.RefreshBinding();
					InvalidateCachedQuery();
					ResetRelatedPropertiesWhenFilterTypeChanged();
				}
			}
		}
		ZString filterType;

		public ZPropertyInfo FilterTypeInfo
		{
			get { return GetZPropertyInfo(Constants.FilterType); }
		}
		#endregion

		#region CentraGeolocation
		public ZGeography CentraGeolocation
		{
			get
			{
				var geolocation = ZGeography.Empty;

				if (FilterType == SearchTypeUNLOCO && !UNLOCO.Equals(ZGuid.Empty))
				{
					ZQuery query = new ZQuery(RefUNLOCOSchema.PK, UNLOCO);
					var unloco = Factory.Load<RefUNLOCO>(query)?.FirstOrDefault();
					geolocation = unloco == null ? ZGeography.Empty : unloco.RL_GeoLocation;
				}
				else if (FilterType == SearchTypeOrgAddress && !OrgPK.Equals(ZGuid.Empty))
				{
					ZQuery query = new ZQuery(OrgAddressSchema.PK, AddressPK);
					var address = Factory.Load<OrgAddress>(query)?.FirstOrDefault();
					geolocation = address == null ? ZGeography.Empty : address.GeoLocation;
				}

				return geolocation;
			}
		}

		public ZPropertyInfo CentraGeolocationInfo
		{
			get { return GetZPropertyInfo(Constants.CentraGeolocation); }
		}
		#endregion

		void ResetRelatedPropertiesWhenFilterTypeChanged()
		{
			UNLOCO = Guid.Empty;
			OrgPK = Guid.Empty;
			AddressPK = Guid.Empty;
		}

		public new OrgLocatedWithinModuleFilterValidation Validation
		{
			get { return (OrgLocatedWithinModuleFilterValidation)base.Validation; }
		}
		#endregion
	}
}
