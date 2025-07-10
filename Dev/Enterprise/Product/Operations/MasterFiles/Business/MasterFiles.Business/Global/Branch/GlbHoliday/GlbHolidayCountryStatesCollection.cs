using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbHolidayCountryStatesCollection : ActiveBusinessObjectCollection<GlbHoliday>
	{
		readonly RefCountryStates fCountryStates;
		readonly RefCountry fCountry;
		readonly BusinessObjectFactory fFactory;
		readonly GlbHolidayCountryStateCollectionTypes fCollectionTypes;

		public GlbHolidayCountryStatesCollection(RefCountryStates countryStates, BusinessObjectFactory factory, GlbHolidayCountryStateCollectionTypes collectionTypes)
			: base(factory, new ZQuery())
		{
			fCountryStates = countryStates;
			fCollectionTypes = collectionTypes;
		}

		public GlbHolidayCountryStatesCollection(RefCountry country, BusinessObjectFactory factory, GlbHolidayCountryStateCollectionTypes collectionTypes)
			: base(factory, new ZQuery())
		{
			fCountry = country;
			fCollectionTypes = collectionTypes;
			fFactory = factory;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = new ZQuery();
			AddToQueryBasedOnCollectionTypes(query);
			if (IsState)
			{
				var country = fCountryStates.Country;
				query.AddToFilter(GlbHolidaySchema.GH_ParentID, fCountryStates.PK);
				query.AddToFilter(GlbHolidaySchema.GH_ParentTableCode, fCountryStates.TablePrefix);
				if (country != null && fCollectionTypes == GlbHolidayCountryStateCollectionTypes.Holiday)
				{
					var countryQuery = new ZQuery(GlbHolidaySchema.GH_ParentID, country.PK);
					countryQuery.AddToFilter(GlbHolidaySchema.GH_ParentTableCode, country.TablePrefix);
					AddToQueryBasedOnCollectionTypes(countryQuery);
					query.AddToFilter(countryQuery, JoinCondition.Or);
				}
			}
			else
			{
				query.AddToFilter(GlbHolidaySchema.GH_ParentID, fCountry.PK);
				query.AddToFilter(GlbHolidaySchema.GH_ParentTableCode, fCountry.TablePrefix);
				if (fCollectionTypes == GlbHolidayCountryStateCollectionTypes.Holiday)
				{
					var countryStates = fCountry.States;
					if (countryStates != null)
					{
						var statesHolidays = fFactory.Load<GlbHoliday>(CreateQueryForCountryStatesHolidays(countryStates));
						//merge states holidays to show only one record.
						var groupStatesHolidays = new MultiGroupCollection<GlbHoliday>(new GlbHolidayCountryStatesComparer());
						groupStatesHolidays.GroupMerge(statesHolidays);

						if (groupStatesHolidays.Any())
						{
							var holidayQuery = new ZQuery(GlbHolidaySchema.PK, groupStatesHolidays.Select(x => x.Key.PK));
							AddToQueryBasedOnCollectionTypes(holidayQuery);
							query.AddToFilter(holidayQuery, JoinCondition.Or);
						}
					}
				}
			}
			return query;
		}

		ZQuery CreateQueryForCountryStatesHolidays(RefCountryStatesDependentCollection countryStates)
		{
			var query = new ZQuery(GlbHolidaySchema.GH_ParentTableCode, RefCountryStatesSchema.Constants.Prefix);
			var stateQuery = new ZQuery(GlbHolidaySchema.GH_ParentID, countryStates.Select(x => x.PK));
			query.AddToFilter(stateQuery);
			return query;
		}

		void AddToQueryBasedOnCollectionTypes(ZQuery query)
		{
			switch (fCollectionTypes)
			{
				case GlbHolidayCountryStateCollectionTypes.Holiday:
					query.AddToFilter(GlbHolidaySchema.GH_RecurrType, SQLComparisonOperator.NotEqual, GlbHolidayRecurTypeCodeList.Codes.Weekly);
					break;

				case GlbHolidayCountryStateCollectionTypes.Weekend:
					query.AddToFilter(GlbHolidaySchema.GH_RecurrType, GlbHolidayRecurTypeCodeList.Codes.Weekly);
					break;
			}
		}

		public bool IsState => fCountryStates != null;
		protected override bool AllowNew => false;
	}

	class GlbHolidayCountryStatesComparer : IEqualityComparer<GlbHoliday>
	{
		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		public bool Equals(GlbHoliday x, GlbHoliday y)
		{
			return x.GH_HolidayName == y.GH_HolidayName
				&& x.GH_Date == y.GH_Date
				&& x.GH_ParentTableCode == y.GH_ParentTableCode
				&& x.GH_IsWorkingDay == y.GH_IsWorkingDay
				&& x.GH_RecurrType == y.GH_RecurrType
				&& x.GH_Recurring == y.GH_Recurring
				&& x.GH_IsActive == y.GH_IsActive;
		}

		public int GetHashCode(GlbHoliday obj)
		{
			return base.GetHashCode();
		}
	}

	public enum GlbHolidayCountryStateCollectionTypes
	{
		Weekend,
		Holiday,
	}
}
