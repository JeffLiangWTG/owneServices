using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class ResponsibleOverallItemsQueryBuilder
	{
		public static ZDBOnlyQuery New(ZGuid opportunityClientPk, CommissionItemArgs itemArgs, ZString? commissionStream = null, DataTable effectiveDateCacheTable = null)
		{
			return new BuilderHelper(opportunityClientPk, itemArgs, commissionStream, effectiveDateCacheTable).GenerateQuery();
		}

		class BuilderHelper
		{
			readonly ZGuid clientPk;
			readonly CommissionItemArgs args;
			readonly ZString? stream;
			readonly DataTable effectiveDateCacheTable;

			int paramCounter;
			public BuilderHelper(ZGuid opportunityClientPk, CommissionItemArgs itemArgs, ZString? commissionStream = null, DataTable effectiveDateCacheTable = null)
			{
				this.clientPk = opportunityClientPk;
				this.args = itemArgs;
				this.stream = commissionStream;
				this.effectiveDateCacheTable = effectiveDateCacheTable;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Function in db")]
			public ZDBOnlyQuery GenerateQuery()
			{
				var query = new ZDBOnlyQuery(typeof(ViewCommissionAgreementOverallItem));

				query.AddToFilter(GetCommissionAgreementItemIncludedQuery(args));
				query.AddSubQuery(ViewCommissionAgreementOverallItemSchema.VCI_CA0, GetCommissionAgreementItemNotExcludedSubquery(args), JoinCondition.And);
				if (effectiveDateCacheTable == null)
				{
					query.AddSubQuery(GetCommissionAgreementSubQuery(clientPk, args, stream), JoinCondition.And);
				}
				else
				{
					DataTable dataTable;

					using (var command = Db.Connection.Command("SELECT VCA_PK FROM dbo.GetCommissionAgreementPrimaryKeys(@OpportunityClientPk, @CustomerPk, @CommissionStream, @CommissionDate, @EffectiveDateCacheTable)")) // Avoid to load all BOs
					{
						command.CommandType = CommandType.Text;
						command.AddParameter("@OpportunityClientPk", SqlDbType.UniqueIdentifier, clientPk.IsValid ? clientPk.ToGuid() : DBNull.Value);
						command.AddParameter("@CustomerPk", SqlDbType.UniqueIdentifier, args.CustomerPk.IsValid ? args.CustomerPk.ToGuid() : DBNull.Value);
						command.AddParameter("@CommissionStream", SqlDbType.VarChar, 3, (object)stream ?? DBNull.Value);
						command.AddParameter("@CommissionDate", SqlDbType.DateTime, args.CommissionDate.ToDateTime());
						command.AddTableValuedParameter("@EffectiveDateCacheTable", "dbo.TVP_TriggerTypeEffectiveDate", effectiveDateCacheTable);

						dataTable = DataUtils.GetDataTableFromCommand(command);
					}
					var rows = dataTable.Rows.Cast<DataRow>();
					var pks = rows.Select(r => (Guid)r["VCA_PK"]).ToArray();

					AddToFilterWithCustomizedParamName(query, ViewCommissionAgreementOverallItemSchema.VCI_CA0, pks, null, null, true);
				}

				query.OrderBy = ViewCommissionAgreementOverallItemSchema.Constants.VCI_TotalAllCodes + OrderByClause.Ascending;

				return query;
			}

			ZQuery GetCommissionAgreementItemIncludedQuery(CommissionItemArgs itemArgs)
			{
				var result = new ZQuery();

				if (!itemArgs.Product.IsEmpty)
				{
					AddToFilterWithCustomizedParamName(result, ViewCommissionAgreementOverallItemSchema.VCI_ProductIsInclude, 1);
					AddToFilterWithCustomizedParamNameAndAListOfValues(result, ViewCommissionAgreementOverallItemSchema.VCI_ProductCode, new string[] { itemArgs.Product, OrgCommissionAgreementItemLookups.AllProductsCode }.Distinct());

					if (!itemArgs.Service.IsEmpty)
					{
						AddToFilterWithCustomizedParamName(result, ViewCommissionAgreementOverallItemSchema.VCI_ServiceIsInclude, 1);
						AddToFilterWithCustomizedParamNameAndAListOfValues(result, ViewCommissionAgreementOverallItemSchema.VCI_ServiceCode, new string[] { itemArgs.Service, OrgCommissionAgreementItemLookups.AllServicesCode }.Distinct());
						if (!itemArgs.SubModule.IsEmpty)
						{
							AddToFilterWithCustomizedParamName(result, ViewCommissionAgreementOverallItemSchema.VCI_SubModuleIsInclude, 1);
							AddToFilterWithCustomizedParamNameAndAListOfValues(result, ViewCommissionAgreementOverallItemSchema.VCI_SubModuleCode, new string[] { itemArgs.SubModule, OrgCommissionAgreementItemLookups.AllSubModulesCode }.Distinct());
						}
					}

					if (!itemArgs.Mode.IsEmpty && itemArgs.Mode != OrgCommissionAgreementItemLookups.AllModesCode)
					{
						var subQuery = new ZQuery();
						AddToFilterWithCustomizedParamNameAndAListOfValues(subQuery, ViewCommissionAgreementOverallItemSchema.VCI_Mode, new string[] { itemArgs.Mode, OrgCommissionAgreementItemLookups.AllModesCode }.Distinct(), JoinCondition.Or);
						AddToFilterWithCustomizedParamName(subQuery, ViewCommissionAgreementOverallItemSchema.VCI_Mode, string.Empty, null, JoinCondition.Or);

						result.AddToFilter(subQuery);
					}
					if (!itemArgs.Origin.IsEmpty)
					{
						var subQuery = new ZQuery();
						AddToFilterWithCustomizedParamNameAndAListOfValues(subQuery, ViewCommissionAgreementOverallItemSchema.VCI_Origin, new string[] { itemArgs.Origin, itemArgs.Origin.SubstringSafe(0, 2) }, JoinCondition.Or);
						AddToFilterWithCustomizedParamName(subQuery, ViewCommissionAgreementOverallItemSchema.VCI_Origin, string.Empty, null, JoinCondition.Or);

						result.AddToFilter(subQuery);
					}
					if (!itemArgs.Destination.IsEmpty)
					{
						var subQuery = new ZQuery();
						AddToFilterWithCustomizedParamNameAndAListOfValues(subQuery, ViewCommissionAgreementOverallItemSchema.VCI_Destination, new string[] { itemArgs.Destination, itemArgs.Destination.SubstringSafe(0, 2) }, JoinCondition.Or);
						AddToFilterWithCustomizedParamName(subQuery, ViewCommissionAgreementOverallItemSchema.VCI_Destination, string.Empty, null, JoinCondition.Or);

						result.AddToFilter(subQuery);
					}
				}

				return result;
			}

			ZDBOnlySubQuery GetCommissionAgreementItemNotExcludedSubquery(CommissionItemArgs itemArgs)
			{
				var result = new ZDBOnlySubQuery(typeof(ViewCommissionAgreementOverallItem), ViewCommissionAgreementOverallItemSchema.VCI_CA0, true);

				var excludeProductFilter = new ZQuery();

				AddToFilterWithCustomizedParamName(excludeProductFilter, ViewCommissionAgreementOverallItemSchema.VCI_ProductIsInclude, 0);
				AddToFilterWithCustomizedParamName(excludeProductFilter, ViewCommissionAgreementOverallItemSchema.VCI_ProductCode, itemArgs.Product);

				if (itemArgs.Product != OrgCommissionAgreementItemLookups.AllProductsCode)
				{
					var includeProductFilter = new ZQuery();

					AddToFilterWithCustomizedParamName(includeProductFilter, ViewCommissionAgreementOverallItemSchema.VCI_ProductIsInclude, 1);
					AddToFilterWithCustomizedParamName(includeProductFilter, ViewCommissionAgreementOverallItemSchema.VCI_ProductCode, itemArgs.Product);

					var serviceFilter = new ZQuery();

					var excludeServiceFilter = new ZQuery();
					AddToFilterWithCustomizedParamName(excludeServiceFilter, ViewCommissionAgreementOverallItemSchema.VCI_ServiceIsInclude, 0);
					AddToFilterWithCustomizedParamName(excludeServiceFilter, ViewCommissionAgreementOverallItemSchema.VCI_ServiceCode, itemArgs.Service);

					if (itemArgs.Service != OrgCommissionAgreementItemLookups.AllServicesCode)
					{
						var includeServiceFilter = new ZQuery();
						AddToFilterWithCustomizedParamName(includeServiceFilter, ViewCommissionAgreementOverallItemSchema.VCI_ServiceIsInclude, 1);
						AddToFilterWithCustomizedParamName(includeServiceFilter, ViewCommissionAgreementOverallItemSchema.VCI_ServiceCode, itemArgs.Service);

						var subModuleFilter = new ZQuery();
						AddToFilterWithCustomizedParamName(subModuleFilter, ViewCommissionAgreementOverallItemSchema.VCI_SubModuleIsInclude, 0);
						AddToFilterWithCustomizedParamName(subModuleFilter, ViewCommissionAgreementOverallItemSchema.VCI_SubModuleCode, itemArgs.SubModule);

						includeServiceFilter.AddToFilter(subModuleFilter);
						serviceFilter.AddToFilter(includeServiceFilter, JoinCondition.Or);
					}

					serviceFilter.AddToFilter(excludeServiceFilter, JoinCondition.Or);

					includeProductFilter.AddToFilter(serviceFilter);
					result.AddToFilter(includeProductFilter, JoinCondition.Or);
				}

				result.AddToFilter(excludeProductFilter, JoinCondition.Or);

				return result;
			}

			ZDBOnlySubQuery GetCommissionAgreementSubQuery(ZGuid opportunityClientPk, CommissionItemArgs itemArgs, ZString? commissionStream = null)
			{
				var agreementSubquery = new ZDBOnlySubQuery(typeof(ViewCommissionAgreement), ViewCommissionAgreementOverallItemSchema.VCI_CA0);
				if (!opportunityClientPk.IsEmpty)
				{
					AddToFilterWithCustomizedParamName(agreementSubquery, ViewCommissionAgreementSchema.VCA_OH_OpportunityClient, opportunityClientPk);
				}
				if (!itemArgs.CustomerPk.IsEmpty)
				{
					AddToFilterWithCustomizedParamName(agreementSubquery, ViewCommissionAgreementSchema.VCA_OH_Customer, itemArgs.CustomerPk);
				}
				if (commissionStream.HasValue)
				{
					AddToFilterWithCustomizedParamName(agreementSubquery, ViewCommissionAgreementSchema.VCA_CommissionStream, commissionStream.Value);
				}

				AddToFilterWithCustomizedParamName(agreementSubquery, ViewCommissionAgreementSchema.VCA_EffectiveDate, itemArgs.CommissionDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly);

				var expiredFilter = new ZQuery();
				AddToFilterWithCustomizedParamName(expiredFilter, ViewCommissionAgreementSchema.VCA_ExpiredDate, itemArgs.CommissionDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly);
				AddToFilterWithCustomizedParamName(expiredFilter, ViewCommissionAgreementSchema.VCA_ExpiredDate, null, null, JoinCondition.Or);
				agreementSubquery.AddToFilter(expiredFilter);

				AddToFilterWithCustomizedParamName(agreementSubquery, ViewCommissionAgreementSchema.VCA_ReversedDateTimeUtc, null);

				var approvedQuery = new ZQuery();
				AddToFilterWithCustomizedParamName(agreementSubquery, ViewCommissionAgreementSchema.VCA_LastApprovedDateTimeUtc, null, SQLComparisonOperator.NotEqual);
				agreementSubquery.AddToFilter(approvedQuery);

				return agreementSubquery;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
			void AddToFilterWithCustomizedParamName(ZQuery query, SchemaColumn schemaColumn, object value, SQLComparisonOperator comparisonOperator = null, JoinCondition joinCondition = null, bool isTableValued = false)
			{
				string paramName = $"@COMMISSION_{++paramCounter}";

				if (comparisonOperator == null)
				{
					comparisonOperator = SQLComparisonOperator.Equal;
				}
				if (joinCondition == null)
				{
					joinCondition = JoinCondition.And;
				}

				var parametersCollection = new ZSqlParameterCollection(ZSqlParameter.New(paramName, value, schemaColumn, comparisonOperator, isTableValued));

				if (isTableValued && comparisonOperator == SQLComparisonOperator.Equal)
				{
					if (value == null)
					{
						query.AddFilterAndZSQLParameterCollection($"{schemaColumn.Name} IS NULL", parametersCollection, joinCondition);
					}
					else
					{
						query.AddFilterAndZSQLParameterCollection($"{schemaColumn.Name} IN (SELECT Value FROM {paramName})", parametersCollection, joinCondition);
					}
				}
				else
				{
					query.AddFilterAndZSQLParameterCollection($"{schemaColumn.Name} {comparisonOperator.ComparisonText(value)} {(value == null ? "NULL" : paramName)}", parametersCollection, joinCondition);
				}
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
			void AddToFilterWithCustomizedParamNameAndAListOfValues(ZQuery query, SchemaColumn schemaColumn, IEnumerable<object> values, JoinCondition joinCondition = null)
			{
				if (values == null)
				{
					return;
				}

				var parametersCollection = new ZSqlParameterCollection();
				var paramNames = new List<string>(2);
				foreach (var value in values)
				{
					string paramName = $"@COMMISSION_{++paramCounter}";
					paramNames.Add(paramName);
					parametersCollection.Add(paramName, value, schemaColumn);
				}

				string statement = string.Join(",", paramNames);

				if (joinCondition == null)
				{
					joinCondition = JoinCondition.And;
				}

				if (paramNames.Any())
				{
					query.AddFilterAndZSQLParameterCollection($"{schemaColumn.Name} IN ({statement})", parametersCollection, joinCondition);
				}
			}
		}
	}

	public class CommissionItemArgs
	{
		public CommissionItemArgs(ZGuid customerPk, ZString product, ZString service,
			ZString subModule, ZDate commissionDate, ZString mode, ZString origin, ZString destination)
		{
			CustomerPk = customerPk;
			Product = product;
			Service = service;
			SubModule = subModule;
			CommissionDate = commissionDate;
			Mode = mode;
			Origin = origin;
			Destination = destination;
		}

		public readonly ZGuid CustomerPk;
		public readonly ZString Product;
		public readonly ZString Service;
		public readonly ZString SubModule;
		public readonly ZDate CommissionDate;
		public readonly ZString Mode;
		public readonly ZString Origin;
		public readonly ZString Destination;
	}
}
