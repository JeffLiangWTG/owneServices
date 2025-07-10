using System;
using System.Collections;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class CountryStatesGlbHolidayCountryStateModuleFilter : CountryStateModuleFilter<ZGuid>
	{
		readonly SchemaColumn ParentIdColumn;
		readonly SchemaColumn ParentTableCodeColumn;
		#region Construction

		protected CountryStatesGlbHolidayCountryStateModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
			CountryStatesColumnForProperty2Validation = RefCountryStatesSchema.PK;
		}

		public CountryStatesGlbHolidayCountryStateModuleFilter(ZString description, SchemaColumn parentIdColumn, SchemaColumn parentTableCodeColumn)
			: base(description, parentIdColumn, parentTableCodeColumn)
		{
			CountryStatesColumnForProperty2Validation = RefCountryStatesSchema.PK;
			ParentIdColumn = parentIdColumn;
			ParentTableCodeColumn = parentTableCodeColumn;
		}

		#endregion

		#region Implementation
		public override CodeDescriptionPairList ComparisonOperator_List
		{
			get
			{
				if (fComparisonOperator_List == null)
				{
					fComparisonOperator_List = new CodeDescriptionPairList();
					fComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact, Res.GetString("a2c341f2-9635-4983-ac04-ce5d578cbc0f", "Search for State inside specified Country"));
				}
				return fComparisonOperator_List;
			}
		}

		protected override void ApplyStatesFilterCore(RefCountryStatesCollection states, ZString countryCode)
		{
			states.AdditionalFilter = new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, countryCode);
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var factory = new BusinessObjectFactory();
			ZQuery result = new ZQuery();
			if (ComparisonOperator == ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact)
			{
				if (!Property1.IsEmpty)
				{
					var country = factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Property1));
					var countryQuery = new ZQuery(ParentIdColumn, country.PK);
					countryQuery.AddToFilter(ParentTableCodeColumn, RefCountrySchema.Constants.Prefix);
					result.AddToFilter(countryQuery, JoinCondition.Or);
					if (!Property2.IsEmpty)
					{
						var stateQuery = new ZQuery(ParentIdColumn, Property2);
						stateQuery.AddToFilter(ParentTableCodeColumn, RefCountryStatesSchema.Constants.Prefix);
						result.AddToFilter(stateQuery, JoinCondition.Or);
					}
					else
					{
						var stateQuery = new ZQuery(ParentIdColumn, country.States.GetPKs());
						stateQuery.AddToFilter(ParentTableCodeColumn, RefCountryStatesSchema.Constants.Prefix);
						result.AddToFilter(stateQuery, JoinCondition.Or);
					}
				}
			}
			return result;
		}

		#endregion

		#region GetNewCommonModuleFilter

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new CountryStatesGlbHolidayCountryStateModuleFilter(category, parentCollection);
		}

		#endregion

		#region Serialization

		protected override void DeserializePropertiesFromXmlForProperty2(XmlReader reader)
		{
			if (reader.Name == "Property2")
			{
				Property2 = new Guid(reader.ReadElementString("Property2"));
			}
		}

		#endregion

		#region CopyPersistantValuesFromFilter

		protected override void CopyPersistantValuesFromFilterForProperty2(ModuleFilter filterToCopyFrom)
		{
			ModuleCodeFilter filter = (ModuleCodeFilter)filterToCopyFrom;
			Property2 = new Guid(filter.Property2);
		}

		#endregion

		#region GetNewValidation

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new CountryStatesGlbHolidayCountryStateModuleFilterValidation(this);
		}

		public class CountryStatesGlbHolidayCountryStateModuleFilterValidation : CountryStateFilterValidation<ZGuid>
		{
			public CountryStatesGlbHolidayCountryStateModuleFilterValidation(CountryStateModuleFilter<ZGuid> parent)
				: base(parent)
			{
			}
			protected override void ErrorIfInvalid(ZPropertyInfo propertyInfo, IList states)
			{
				ErrorIfInvalidPK(propertyInfo, states);
			}
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			base.FillWithValidTestFilterValueCore();
			Property2 = ZGuid.NewZGuid();
		}

#endif
		#endregion
	}
}
