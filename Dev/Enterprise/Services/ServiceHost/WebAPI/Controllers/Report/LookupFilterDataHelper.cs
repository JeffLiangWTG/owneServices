using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Services.ServiceHost
{
	static class LookupFilterDataHelper
	{
		internal static IBusinessObjectCollection LoadModuleDataForContact(ModuleIdentifier moduleId, LookupFilterSearchArgs searchParam, ZGuid contactPK)
		{
			var webModuleID = WebModuleIDs.GetWebModuleIDFromModuleID(moduleId);
			if (webModuleID == WebModuleIDs.NotAssigned)
			{
				throw new ReportServiceException(new[] { $"Invalid lookup Type {searchParam.LookupType}, Web module is not assigned." }, ReportServiceErrorType.LookupError);
			}

			using (var filterModule = ZFilterModule.GetZFilterModule(moduleId))// cannot use ZWebModuleFactory.Create(WebModuleIDs.GetWebModuleIDFromModuleID(moduleId), factory, null) because the web module is high coupled with ZPage and Site User.
			{
				if (webModuleID == WebModuleIDs.Location)
				{
					return LoadCollectionForLocation(searchParam, filterModule.GridCollection);
				}
				else
				{
					var query = ObjectFactory.Get<IContactDataRestrictionProvider>().GetFilter(webModuleID, contactPK, filterModule.GridCollection.Factory);
					if (query.IsNoResultQuery)
					{
						if (filterModule is IGlowReportingModuleDataQueryProvider glowReportingModuleDataQueryProvider)
						{
							query = glowReportingModuleDataQueryProvider.BuildQuery();
						}
					}

					if (!query.IsNoResultQuery)
					{
						query.ModificationsEnabled = true;
						query.MaximumRows = searchParam.Top;
						BuildCodeDescriptionQuery(query, filterModule.GridCollection.TypeOfElements, searchParam.SearchTerms);
					}

					LoadCollection(query, filterModule.GridCollection);
					return filterModule.GridCollection;
				}
			}
		}

		internal static IBusinessObjectCollection LoadModuleDataForStaff(ModuleIdentifier moduleId, IBusinessObjectCollection collection, ReportLookupSearchArgs searchArgs)
		{
			using (var filterModule = ZFilterModule.GetZFilterModule(moduleId))
			{
				if (collection == null)
				{
					collection = filterModule.GridCollection;
				}
				if (moduleId == ModuleIDs.Location)
				{
					return LoadCollectionForLocation(searchArgs, collection);
				}
				else
				{
					var query = new ZQuery();
					if (filterModule is IGlowReportingModuleDataQueryProvider glowReportingModuleDataQueryProvider)
					{
						query.AddToFilter(glowReportingModuleDataQueryProvider.BuildQuery());
					}
					query.ModificationsEnabled = true;
					query.MaximumRows = searchArgs.Top;
					BuildCodeDescriptionQuery(query, collection.TypeOfElements, searchArgs.SearchTerms);

					LoadCollection(query, collection);
					return collection;
				}
			}
		}

		internal static List<CodeDescription> RetrieveMatchesCodeDescriptions(IEnumerable<BusinessObject> collection)
		{
			var lookupData = new List<CodeDescription>();
			if (collection != null)
			{
				foreach (var businessObject in collection)
				{
					var codeDescription = new CodeDescription
					{
						Pk = businessObject.PK.ToGuid(),
						Code = string.Empty,
						Description = string.Empty
					};

					if (!string.IsNullOrEmpty(GetCodePropertyName(businessObject.GetType())))
					{
						codeDescription.Code = CodePropertyAttribute.CodeFromBusinessObject(businessObject);
					}

					if (!string.IsNullOrEmpty(GetDescriptionPropertyName(businessObject.GetType())))
					{
						codeDescription.Description = DescriptionPropertyAttribute.DescriptionFromBusinessObject(businessObject);
					}

					lookupData.Add(codeDescription);
				}
			}

			return lookupData;
		}

		static void LoadCollection(ZQuery query, IBusinessObjectCollection collection)
		{
			if (collection is BusinessObjectCollection legacyCollection)
			{
				legacyCollection.LoadWithMoreFiltering(query);
			}

			if (collection is IActiveBusinessObjectCollection flyweightCollection)
			{
				flyweightCollection.AdditionalFilter = query;
			}
		}

		internal static void BuildCodeDescriptionQuery(ZQuery query, Type elementType, string searchTerms)
		{
			if (!string.IsNullOrEmpty(searchTerms))
			{
				var pkCodeDescQuery = new ZQuery();
				if (Guid.TryParse(searchTerms, out var pk) && BuildPKQuery(elementType, pk) is ZQuery pkQuery)
				{
					pkCodeDescQuery.AddToFilter(pkQuery);
				}

				var codeDescriptionQuery = new ZQuery();
				if (BuildCodeQuery(elementType, searchTerms) is ZQuery codeQuery)
				{
					codeDescriptionQuery.AddToFilter(codeQuery);
				}

				if (BuildDescriptionQuery(elementType, searchTerms) is ZQuery descriptionQuery)
				{
					var joinCondition = codeDescriptionQuery.IsEmpty ? JoinCondition.And : JoinCondition.Or;
					if (descriptionQuery.LiteralTextSqlFormatted != codeDescriptionQuery.LiteralTextSqlFormatted)
					{
						codeDescriptionQuery.AddToFilter(descriptionQuery, joinCondition);
					}
				}

				if (!codeDescriptionQuery.IsEmpty)
				{
					pkCodeDescQuery.AddToFilter(codeDescriptionQuery, JoinCondition.Or);
				}

				if (!pkCodeDescQuery.IsEmpty)
				{
					query.AddToFilter(pkCodeDescQuery);
				}
			}
		}

		static ZQuery BuildPKQuery(Type elementType, Guid pk)
		{
			var pkColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumnSafe(BusinessObjectFactory.GetTableNameFromType(elementType, throwOnError: false));
			return pkColumn == null ? null : new ZQuery(pkColumn, pk);
		}

		static ZQuery BuildCodeQuery(Type elementType, string searchTerms)
		{
			var codePropertyName = GetCodePropertyName(elementType);
			if (!string.IsNullOrEmpty(codePropertyName))
			{
				var likeSearchTerms = (ZString)$"%{searchTerms}%";

				var column = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(codePropertyName, BusinessObjectFactory.GetTableNameFromType(elementType, throwOnError: false));
				if (column == null)
				{
					var queryMethod = CodePropertyAttribute.QueryMethodFromType(elementType);
					var codeQuery = queryMethod?.Invoke(null, new object[] { SQLComparisonOperator.Like, likeSearchTerms });
					if (codeQuery is ZQuery query)
					{
						return query;
					}
				}
				else
				{
					return new ZQuery(column, SQLComparisonOperator.Like, likeSearchTerms);
				}
			}

			return null;
		}

		static ZQuery BuildDescriptionQuery(Type elementType, string searchTerms)
		{
			var descriptionPropertyName = GetDescriptionPropertyName(elementType);
			if (!string.IsNullOrEmpty(descriptionPropertyName))
			{
				var likeSearchTerms = (ZString)$"%{searchTerms}%";
				var column = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(descriptionPropertyName, BusinessObjectFactory.GetTableNameFromType(elementType, throwOnError: false));
				if (column == null)
				{
					var queryMethod = DescriptionPropertyAttribute.QueryMethodFromType(elementType);
					var descQuery = queryMethod?.Invoke(null, new object[] { SQLComparisonOperator.Like, likeSearchTerms });
					if (descQuery is ZQuery query)
					{
						return query;
					}
				}
				else
				{
					var searchQuery = new ZQuery(column, SQLComparisonOperator.Like, likeSearchTerms);
					var source = (TranslatableDataFieldAttribute)Attribute.GetCustomAttribute(elementType.GetProperty(column.Name), typeof(TranslatableDataFieldAttribute));
					if (source != null)
					{
						var predicate = SQLComparisonOperator.Contains.GetPredicate(searchTerms);
						var captions = source.GetRuntimeCaptions().Cast<ResourceString>().Where(caption => predicate.Invoke(caption)).Select(caption => caption.GetUnresolvedString());
						searchQuery.AddToFilter(new ZQuery(column, SQLComparisonOperator.Equal, captions.ToArray()), JoinCondition.Or);
					}
					return searchQuery;
				}
			}

			return null;
		}

		static string GetCodePropertyName(Type bizoType)
		{
			string propertyName;
			try
			{
				propertyName = CodePropertyAttribute.CodePropertyNameFromType(bizoType);
			}
			catch (NoCodePropertyException ex)
			{
				throw new ReportServiceException(new[] { ex.Message }, ReportServiceErrorType.LookupError);
			}
			return propertyName;
		}

		static string GetDescriptionPropertyName(Type bizoType)
		{
			string propertyName;
			try
			{
				propertyName = DescriptionPropertyAttribute.DescriptionPropertyNameFromTypeWithNoMultilingual(bizoType);
			}
			catch (NoCodePropertyException ex)
			{
				throw new ReportServiceException(new[] { ex.Message }, ReportServiceErrorType.LookupError);
			}
			return propertyName;
		}

		static IBusinessObjectCollection LoadCollectionForLocation(ReportLookupSearchArgs searchArgs, IBusinessObjectCollection collection)
		{
			if (collection is LocationCollection locationCollection)
			{
				var result = new LocationCollection(locationCollection.Factory);

				var locationProvider = ObjectFactory.Get<ILocationProvider>();
				var portQuery = locationProvider.GetLocationQuery(LocationTypeEnum.Port, searchArgs.SearchTerms);
				portQuery.MaximumRows = searchArgs.Top;

				locationCollection.LoadUNLoco(portQuery);
				result.AddRange(locationCollection);

				var countryQuery = locationProvider.GetLocationQuery(LocationTypeEnum.Country, searchArgs.SearchTerms);
				countryQuery.MaximumRows = searchArgs.Top;

				locationCollection.LoadCountry(countryQuery);
				result.AddRange(locationCollection);

				var zoneQuery = locationProvider.GetLocationQuery(LocationTypeEnum.InternationalZone, searchArgs.SearchTerms);
				zoneQuery.MaximumRows = searchArgs.Top;

				locationCollection.LoadZone(zoneQuery);
				result.AddRange(locationCollection);

				return result;
			}
			return null;
		}
	}
}
