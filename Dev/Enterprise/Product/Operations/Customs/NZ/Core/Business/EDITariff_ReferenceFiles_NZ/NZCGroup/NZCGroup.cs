using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCGroup : AutoNZCGroup
	{
		public NZCGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		internal static NZCGroup[] FromCountry(ZString countryOfOrigin, ZDateTime dateForDutyRate, BusinessObjectFactory factory)
		{
			ListCache cache = factory.GetCachedValue("NZCGroupForCountryCache", delegate
			{ return new ListCache() { DateForDutyRate = dateForDutyRate }; });
			if (dateForDutyRate != cache.DateForDutyRate)
			{
				cache.Clear();
				cache.DateForDutyRate = dateForDutyRate;
			}

			NZCGroup[] result;
			if (!cache.TryGetValue(countryOfOrigin, out result))
			{
				cache[countryOfOrigin] = GetAllGroupsForCountry(countryOfOrigin, dateForDutyRate, factory);
				result = cache[countryOfOrigin];
			}

			return result;
		}

		static NZCGroup[] GetAllGroupsForCountry(ZString country, ZDateTime dateForDutyRate, BusinessObjectFactory factory)
		{
			if (dateForDutyRate.IsEmpty)
			{
				dateForDutyRate = ZDateTime.Today;
			}

			ZDBOnlyQuery groupQuery = new ZDBOnlyQuery(typeof(NZCGroup));
			groupQuery.AddToFilter(NZCGroupSchema.Q4_Code, country);

			ZDBOnlySubQuery countryGroupSubQuery = new ZDBOnlySubQuery(typeof(NZCCountryGroup), NZCCountryGroupSchema.U4_Group);
			NZCCountryGroup.AddFilterForCountryGroups(countryGroupSubQuery, country, dateForDutyRate);

			groupQuery.AddSubQuery(NZCGroupSchema.Q4_Code, NZCCountryGroupSchema.U4_Group, countryGroupSubQuery, JoinCondition.Or);
			groupQuery.OrderBy = NZCGroupSchema.Q4_IsCountry.Name + " desc, " + NZCGroupSchema.Q4_Code.Name;

			return factory.Load<NZCGroup>(groupQuery);
		}

		class ListCache : Dictionary<string, NZCGroup[]>
		{
			internal ZDateTime DateForDutyRate;
		}
	}

	static class MethodExtensions
	{
		internal static ZQuery Or(this ZQuery query, SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, object value)
		{
			query.AddToFilter(JoinCondition.Or, schemaColumn, comparisonOperator, value);
			return query;
		}

		internal static ZQuery And(this ZQuery query, SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, object value)
		{
			query.AddToFilter(JoinCondition.And, schemaColumn, comparisonOperator, value);
			return query;
		}
	}
}
