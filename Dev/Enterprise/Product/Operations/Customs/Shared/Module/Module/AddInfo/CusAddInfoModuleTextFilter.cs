using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This will be used by Customs")]
	public class CusAddInfoModuleTextFilter : ModuleTextFilter
	{
		public CusAddInfoModuleTextFilter(ZString description, SchemaStringColumn addInfoSchemaColumn, string addInfoProperty, Type typeOfBusinessObjectToQuery, string cusAddInfoTypeAttribute)
			: base(description, (c, v) => AddInfoExactOrStartsWithQuery(c, v, addInfoSchemaColumn, addInfoProperty, typeOfBusinessObjectToQuery, cusAddInfoTypeAttribute))
		{
		}

		public CusAddInfoModuleTextFilter(ZString description, SchemaStringColumn addInfoSchemaColumn, string addInfoProperty, Type typeOfBusinessObjectToQuery, string cusAddInfoTypeAttribute, IList list)
			: base(description, (c, v) => AddInfoExactOrStartsWithQuery(c, v, addInfoSchemaColumn, addInfoProperty, typeOfBusinessObjectToQuery, cusAddInfoTypeAttribute), list)
		{
		}

		public override IReadOnlyList<string> AllowedComparisonOperators
		{
			get
			{
				return new[]
						{
							ComparisonConstants.Exact,
							ComparisonConstants.StartsWith,
						};
			}
		}

		static ZQuery AddInfoExactOrStartsWithQuery(SQLComparisonOperator comparisonOperator, ZString value, SchemaStringColumn addInfoSchemaColumn, string addInfoProperty, Type typeOfBusinessObjectToQuery, string cusAddInfoTypeAttribute)
		{
			var query = new ZDBOnlyQuery(typeOfBusinessObjectToQuery);
			var subQuery = new ZDBOnlySubQuery(typeof(CusAddInfo), CusAddInfoSchema.B7_ParentID);
			subQuery.AddToFilter(CusAddInfoSchema.B7_Type, cusAddInfoTypeAttribute);
			var filterQuery = AddInfoFilterRepository.GetAddInfoQuery(comparisonOperator, value, addInfoSchemaColumn, addInfoProperty);
			subQuery.AddToFilter(filterQuery);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}
	}
}
