using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Module
{
	public class AddInfoModuleTextFilter : ModuleTextFilter
	{
		public AddInfoModuleTextFilter(ZString description, GetTextQueryWithOperator queryDelegate) : base(description, queryDelegate)
		{
		}

		public AddInfoModuleTextFilter(ZString description, SchemaStringColumn addInfoSchemaColumn, string addInfoProperty)
			: base(description, (c, v) => AddInfoExactOrStartsWithQuery(c, v, addInfoSchemaColumn, addInfoProperty))
		{
		}

		public AddInfoModuleTextFilter(ZString description, SchemaStringColumn addInfoSchemaColumn, string addInfoProperty, IList list)
			: base(description, (c, v) => AddInfoExactOrStartsWithQuery(c, v, addInfoSchemaColumn, addInfoProperty), list)
		{
		}

		public override IReadOnlyList<string> AllowedComparisonOperators
		{
			get { return AllowedComparisonOperatorsForAddInfo; }
		}

		public static string[] AllowedComparisonOperatorsForAddInfo
		{
			get
			{
				return new[]
						   {
							   string.Empty,
							   ComparisonConstants.Exact,
							   ComparisonConstants.StartsWith,
						   };
			}
		}

		static ZQuery AddInfoExactOrStartsWithQuery(SQLComparisonOperator comparisonOperator, ZString value, SchemaStringColumn addInfoSchemaColumn, string addInfoProperty)
		{
			return AddInfoFilterRepository.GetAddInfoQuery(comparisonOperator, value, addInfoSchemaColumn, addInfoProperty);
		}

		public override bool IsExpensiveQuery
		{
			get { return true; }
		}
	}
}
