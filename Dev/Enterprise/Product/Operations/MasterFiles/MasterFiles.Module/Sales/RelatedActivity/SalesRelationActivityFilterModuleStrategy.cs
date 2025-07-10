using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	class SalesRelationActivityFilterModuleStrategy : FilterModuleStrategy
	{
		public override void RunOnFilterControlInitialisation(IFilterControl control, IBusinessObjectCollection gridCollection)
		{
			if (typeof(ISalesRelationActivity).IsAssignableFrom(gridCollection.TypeOfElements))
			{
				SalesRelationActivityFilterHelper.AddAllFilterControlColumnsAndBuilders(control);
			}
		}

		protected override IEnumerable<ModuleFilter> FiltersToAdd
		{
			get
			{
				var dummySalesRelationActivity = GetDummySalesRelationActivity();
				if (dummySalesRelationActivity != null)
				{
					var schema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(dummySalesRelationActivity.TableName);
					var tablePrefix = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(dummySalesRelationActivity.TableName);
					var lastEditDateColumn = (SchemaDateTimeColumn)schema.All[tablePrefix + "_SystemLastEditTimeUtc"];

					return SalesRelationActivityFilterHelper.GetAllModuleFilters(Factory, schema, tablePrefix, lastEditDateColumn, BizoType);
				}

				return Enumerable.Empty<ModuleFilter>();
			}
		}

		protected override MultilingualString GetUniqueMultilingualDescription(ModuleFilter filter, ModuleFilterCollection filters)
		{
			if (filters.Any(x => x.LocalizedDescription.ToString().Equals(filter.MultilingualDescription, System.StringComparison.OrdinalIgnoreCase)))
			{
				return base.GetUniqueMultilingualDescription(filter, filters);
			}

			return filter.MultilingualDescription;
		}

		#region Implementation

		ISalesRelationActivity GetDummySalesRelationActivity()
		{
			ISalesRelationActivity dummySalesRelationActivity = null;

			if (typeof(ISalesRelationActivity).IsAssignableFrom(BizoType))
			{
				try
				{
					var query = new ZQuery { FetchOnlyFromLocalCache = true };

					dummySalesRelationActivity = Factory.LoadTop1(BizoType, query) as ISalesRelationActivity;
					if (dummySalesRelationActivity == null)
					{
						dummySalesRelationActivity = Factory.GetNull(BizoType) as ISalesRelationActivity;
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					dummySalesRelationActivity = null;
				}
			}

			return dummySalesRelationActivity;
		}

		#endregion
	}
}
