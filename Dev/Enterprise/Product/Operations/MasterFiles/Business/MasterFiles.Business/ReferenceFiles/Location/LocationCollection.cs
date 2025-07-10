using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Provides a collection of UNLOCOs, Countries and Zones
	/// </summary>
	[ModuleID(ModuleId.Location)]
	public class LocationCollection : BusinessObjectCollection<BusinessObject>, ILocationCollection, ICompositeCollection
	{
		public LocationCollection(BusinessObjectFactory factory)
			: this(factory, true)
		{
		}

		public LocationCollection(BusinessObjectFactory factory, ZoneTypeList requiredZoneTypes, bool allowCountries = true)
			: this(factory, true, allowCountries)
		{
			this.RequiredZoneTypes = requiredZoneTypes;
		}

		public LocationCollection(BusinessObjectFactory factory, bool allowZones, bool allowCountries = true)
			: base(factory)
		{
			AllowZones = allowZones;
			AllowCountries = allowCountries;
		}

		readonly ZoneTypeList RequiredZoneTypes;

		public ZBool AllowZones { get; }

		public ZBool AllowCountries { get; }

		#region Implementation

		public new ILocation this[int index]
		{
			get { return (ILocation)Elements[index]; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("Cannot create an ILocation from within the collection");
		}

		#endregion

		#region Zone Filter

		ZQuery AdditionalObligatoryZoneFilter
		{
			get { return fAdditionalObligatoryZoneFilter ?? (fAdditionalObligatoryZoneFilter = GetFilterForRequiredZoneTypes()); }
		}

		ZQuery GetFilterForRequiredZoneTypes()
		{
			var zoneFilter = new ZQuery();
			if (AllowZones && RequiredZoneTypes != null)
			{
				zoneFilter.AddToFilter(RefZoneHeaderSchema.FZ_ZoneType, SQLComparisonOperator.Equal, RequiredZoneTypes.GetCodes());
			}
			return zoneFilter;
		}

		ZQuery fAdditionalObligatoryZoneFilter;

		#endregion

		#region Business Object Collection Overrides

		protected override ZDataTable Table
		{
			get { return new ZDataTable((NoResString)"Locations"); }
		}

		public void LoadUNLoco(ZQuery query)
		{
			((IBindingList)this).RemoveSort();
			RemoveAllButLeaveRelationshipsIntact();
			AddRange(Factory.Load(typeof(RefUNLOCO), query));
			LastLoadedAdditionalFilter = query;
		}

		public void LoadZone(ZQuery query)
		{
			((IBindingList)this).RemoveSort();
			RemoveAllButLeaveRelationshipsIntact();
			if (AllowZones)
			{
				var resultQuery = new ZQuery(AdditionalObligatoryZoneFilter, query);
				AddRange(Factory.Load(typeof(RefZoneHeader), resultQuery));
				LastLoadedAdditionalFilter = resultQuery;
			}
		}

		public void LoadCountry(ZQuery query)
		{
			((IBindingList)this).RemoveSort();
			RemoveAllButLeaveRelationshipsIntact();
			if (AllowCountries)
			{
				AddRange(Factory.Load(typeof(RefCountry), query));
				LastLoadedAdditionalFilter = query;
			}
		}

		public override void Load(ZQuery filter)
		{
		}

		public override int GetEstimatedLoadCount(ZQuery completeFilter)
		{
			throw new NotSupportedException("GetEstimatedLoadCount must be provided a BizOType. Use override on LocationCollection that takes BizOType instead.");
		}

		public virtual int GetEstimatedLoadCount(Type bizOType, ZQuery completeFilter)
		{
			var result = -1;
			if (bizOType != null)
			{
				result = Factory.GetDatabaseCount(bizOType, completeFilter);
			}

			return result;
		}

		public ZString GetDescriptionFromCode(string code)
		{
			return FindBoxListProvider.DescriptionFromCode(code);
		}

		#endregion

		#region FindBoxListProvider

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new LocationFindBoxListProvider(this); }
		}

		protected class LocationFindBoxListProvider : FindBoxListProvider
		{
			public LocationFindBoxListProvider(BusinessObjectCollection list)
				: base(list)
			{
				locationCollection = (LocationCollection)List;
			}

			readonly LocationCollection locationCollection;

			bool AllowZones
			{
				get { return locationCollection.AllowZones; }
			}

			bool AllowCountries
			{
				get { return locationCollection.AllowCountries; }
			}

			public override ZGuid PrimaryKeyFromCode(string code)
			{
				var result = ZGuid.Empty;
				if (!string.IsNullOrEmpty(code))
				{
					var bizObj = GetBusinessObjectFromCode(code);
					result = bizObj != null ? bizObj.PK : ZGuid.Invalid;
				}

				return result;
			}

			public override string CodeFromPrimaryKey(ZGuid pk)
			{
				var location = GetBusinessObjectFromPK(pk) as ILocation;

				return location != null ? location.Code : ZString.Empty;
			}

			public override string DescriptionFromCode(string code)
			{
				string result = null;
				if (!string.IsNullOrEmpty(code))
				{
					var bizObj = GetBusinessObjectFromCode(code);
					if (bizObj != null)
					{
						result = ((ILocation)bizObj).Description;
					}
				}

				return result;
			}

			public override string CodeFromDescription(string description)
			{
				string result = null;
				if (!string.IsNullOrEmpty(description))
				{
					var bizObj = ILocationFromColumn(typeof(RefUNLOCO), RefUNLOCOSchema.RL_PortName, description);
					if (bizObj == null)
					{
						if (AllowZones)
						{
							bizObj = ILocationFromColumn(typeof(RefZoneHeader), RefZoneHeaderSchema.FZ_Description, description);
						}

						if (bizObj == null && AllowCountries)
						{
							bizObj = ILocationFromColumn(typeof(RefCountry), RefCountrySchema.RN_Desc, description);
						}
					}

					if (bizObj != null)
					{
						result = ((ILocation)bizObj).Code;
					}
				}

				return result;
			}

			public override (string, bool) NearestMatchCore(string code, bool explicitAutoComplete)
			{
				IFindBoxListProvider collection = new RefUNLOCOCollection(List.Factory);

				return collection.NearestMatchCore(code, explicitAutoComplete);
			}

			internal BusinessObject GetBusinessObjectFromPK(ZGuid pk)
			{
				BusinessObject bizObj = null;
				if (pk.IsValid)
				{
					var factory = List.Factory;
					bizObj = factory.Load(typeof(RefUNLOCO), pk);
					if (bizObj == null)
					{
						if (AllowCountries)
						{
							bizObj = factory.Load(typeof(RefCountry), pk);
						}
						if (bizObj == null && AllowZones)
						{
							var query = new ZQuery(RefZoneHeaderSchema.PK, pk);
							if (locationCollection.RequiredZoneTypes != null)
							{
								query.AddToFilter(locationCollection.AdditionalObligatoryZoneFilter, JoinCondition.And);
							}
							bizObj = factory.LoadTop1(typeof(RefZoneHeader), query);
						}
					}
				}

				return bizObj;
			}

			internal BusinessObject GetBusinessObjectFromCode(ZString code)
			{
				BusinessObject bizObj = null;
				if (!code.IsEmpty)
				{
					bizObj = ILocationFromFixedLengthNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, code);
					if (bizObj == null)
					{
						if (AllowCountries)
						{
							bizObj = ILocationFromFixedLengthNaturalKey(typeof(RefCountry), RefCountrySchema.RN_Code, code);
						}
						if (bizObj == null && AllowZones)
						{
							bizObj = ILocationFromColumn(typeof(RefZoneHeader), RefZoneHeaderSchema.FZ_Code, code);
						}
					}
				}

				return bizObj;
			}

			public override bool AutoCompleteOnCommit
			{
				get { return true; }
			}

			BusinessObject ILocationFromFixedLengthNaturalKey(Type locationType, SchemaColumn column, ZString columnValue)
			{
				return columnValue.Length == column.MaxLength
					? List.Factory.LoadFromNaturalKey(locationType, column, columnValue)
					: null;
			}

			BusinessObject ILocationFromColumn(Type locationType, SchemaColumn column, ZString columnValue)
			{
				if (columnValue.Length <= column.MaxLength)
				{
					var filter = new ZQuery(column, SQLComparisonOperator.Equal, columnValue);
					if (locationType == typeof(RefZoneHeader) && AllowZones && locationCollection.RequiredZoneTypes != null)
					{
						filter.AddToFilter(locationCollection.AdditionalObligatoryZoneFilter, JoinCondition.And);
					}

					return List.Factory.LoadTop1(locationType, filter);
				}

				return null;
			}

			#region IFindBoxListProviderEx Members

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard coded constant")]
			public override IList<AlternateKey> AlternateKeys
			{
				get { return new AlternateKey[] { new AlternateKey((NoResString)"Description", SchemaColumnType.String, "Description") }; }
			}

			public override ZGuid PrimaryKeyFromAlternateKey(string columnName, IZType value)
			{
				var types = new List<Type>();
				var queries = new List<ZQuery>();

				types.Add(typeof(RefUNLOCO));
				queries.Add(new ZQuery(RefUNLOCOSchema.RL_PortName, value));

				if (AllowCountries)
				{
					types.Add(typeof(RefCountry));
					queries.Add(new ZQuery(RefCountrySchema.RN_Desc, value));
				}

				if (AllowZones)
				{
					types.Add(typeof(RefZoneHeader));
					queries.Add(new ZQuery(RefZoneHeaderSchema.FZ_Description, value));
					if (locationCollection.RequiredZoneTypes != null)
					{
						queries.Add(locationCollection.AdditionalObligatoryZoneFilter);
					}
				}

				var result = ZGuid.Empty;
				for (int i = 0; i < types.Count; i++)
				{
					var bizObj = GetBizObj(types[i], queries[i], value);
					if (bizObj != null)
					{
						result = bizObj.PK;
						break;
					}
				}

				return result;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Column name")]
			public override IZType AlternateKeyFromPrimaryKey(string columnName, ZGuid pk)
			{
				var location = GetBusinessObjectFromPK(pk) as ILocation;
				if (location != null && columnName == "Description")
				{
					return location.Description;
				}

				return null;
			}

			#endregion
		}

		#endregion

		#region ICompositeCollection Members

		Type ICompositeCollection.TypeOfElementFromPK(ZGuid pk)
		{
			var bizObj = ((LocationFindBoxListProvider)FindBoxListProvider).GetBusinessObjectFromPK(pk);
			var result = bizObj != null ? bizObj.GetType() : typeof(RefUNLOCO);

			return result;
		}

		Type ICompositeCollection.TypeOfElementFromCode(ZString code)
		{
			var bizObj = ((LocationFindBoxListProvider)FindBoxListProvider).GetBusinessObjectFromCode(code);
			var result = bizObj != null ? bizObj.GetType() : typeof(RefUNLOCO);

			return result;
		}

		int ICompositeCollection.MaxLength
		{
			get { return RefUNLOCO.Schema.RL_Code.Length; }
		}

		#endregion
	}
}
