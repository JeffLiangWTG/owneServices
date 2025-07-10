using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;

namespace Enterprise.MasterFiles.Module
{
	public class DocumentTrackingFilterStripsHelper : FilterStripsHelper
	{
		bool IsAssignableFromIHaveRequiredDocuments => typeof(IHaveRequiredDocuments).IsAssignableFrom(BusinessObjectType);

		bool IsAssignableFromIDocsAndCartageParent => typeof(IDocsAndCartageParent).IsAssignableFrom(BusinessObjectType);

		List<RefDocType> DocumentTypeList
		{
			get
			{
				if (documentTypeList == null)
				{
					var oldValue = Factory.RefreshEnabled;
					try
					{
						Factory.RefreshEnabled = false;
						var docTypes = new RefDocTypeCollection(Factory);
						docTypes.ApplySort(AutoRefDocType.Schema.RT_Desc, ListSortDirection.Ascending);

						documentTypeList = docTypes.DistinctBy(dt => dt.RT_DocType).ToList();
					}
					finally
					{
						Factory.RefreshEnabled = oldValue;
					}
				}

				return documentTypeList;
			}
		}
		List<RefDocType> documentTypeList;

		public override bool IsApplicableToBizOTypeIsAssignableFrom()
		{
			return IsAssignableFromIHaveRequiredDocuments || IsAssignableFromIDocsAndCartageParent;
		}

		protected override void AddFilterStripsForIndexSearch(ModuleFilterCollection filters, SearchField[] defaultHiddenIndexSearchFields)
		{
			AddDateReceivedFilterForIndexSearch(filters);
			AddDocTypeFilterForIndexSearch(filters);
			AddValidToDateFilterForIndexSearch(filters);
		}

		void AddDateReceivedFilterForIndexSearch(ModuleFilterCollection filters)
		{
			var searchField = SearchField.Create(fieldName: "DocumentReceivedDate", description: (NoResString)"Date Received", dataType: typeof(DateTime));
			var filter = new IndexSearchModuleDateFilter(searchField);
			filter.MultilingualDescription = DateReceivedDescription;
			filter.Category = FilterCategories.DocumentTracking;

			filters.AddFilter(filter);
		}

		void AddDocTypeFilterForIndexSearch(ModuleFilterCollection filters)
		{
			var searchField = SearchField.Create(fieldName: "DocumentType", description: (NoResString)"Document Type");
			var filter = new IndexSearchModuleTextFilter(searchField, DocumentTypeList);
			filter.MultilingualDescription = DocumentTypeDescription;
			filter.Category = FilterCategories.DocumentTracking;

			filters.AddFilter(filter);
		}

		void AddValidToDateFilterForIndexSearch(ModuleFilterCollection filters)
		{
			var searchField = SearchField.Create(fieldName: "ValidToDate", description: (NoResString)"Valid To Date", dataType: typeof(DateTime));
			var filter = new IndexSearchModuleDateFilter(searchField);
			filter.MultilingualDescription = ValidToDateDescription;
			filter.Category = FilterCategories.DocumentTracking;

			filters.AddFilter(filter);
		}

		protected override void AddFilterStrips(ModuleFilterCollection filters)
		{
			AddDateReceivedFilter(filters);
			AddDocTypeFilter(filters);
			AddValidToDateFilter(filters);
		}

		void AddDateReceivedFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddDateFilter("Date Received", JobRequiredDocumentSchema.EQ_DateReceived);
			filter.MultilingualDescription = DateReceivedDescription;
			filter.Category = FilterCategories.DocumentTracking;
			filter.SubGroup = SubGroup;
		}

		void AddDocTypeFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Document Type", JobRequiredDocumentSchema.EQ_DocType, DocumentTypeList);
			filter.MultilingualDescription = DocumentTypeDescription;
			filter.Category = FilterCategories.DocumentTracking;
			filter.SubGroup = SubGroup;
		}

		void AddValidToDateFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddDateFilter("Valid To Date", JobRequiredDocumentSchema.EQ_ValidToDate);
			filter.MultilingualDescription = ValidToDateDescription;
			filter.Category = FilterCategories.DocumentTracking;
			filter.SubGroup = SubGroup;
		}

		ResourceString DateReceivedDescription => ResString.GetMultilingualString("MasterFiles|DocumentTrackingFilterStripsHelper|DocumentDateReceived", "Documents - Date Received");

		ResourceString DocumentTypeDescription => ResString.GetMultilingualString("MasterFiles|DocumentTrackingFilterStripsHelper|DocumentType", "Documents - Type");

		ResourceString ValidToDateDescription => ResString.GetMultilingualString("MasterFiles|DocumentTrackingFilterStripsHelper|DocumentValidToDate", "Documents - Valid To Date");

		ModuleFilterSubGroup SubGroup => subGroup ?? (subGroup = GetNewSubGroup());
		ModuleFilterSubGroup subGroup;
		protected virtual ModuleFilterSubGroup GetNewSubGroup() => new DocumentsFilterCommonSubGroup(BusinessObjectType);

		public class DocumentsFilterCommonSubGroup : ModuleFilterSubGroup
		{
			public DocumentsFilterCommonSubGroup(Type businessObjectType)
			{
				this.businessObjectType = businessObjectType;
			}

			readonly Type businessObjectType;

			string ParentTableCode => ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(BusinessObjectFactory.GetTableNameFromType(businessObjectType));

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return GetSubQueryCore(filter);
			}

			ZQuery GetSubQueryCore(ZQuery filter)
			{
				var query = GetSubQueryForJobRequiredDocument(filter);

				AddJobRequiredDocumentSubQueryForNotOperator(filter, query);

				return query;
			}

			void AddJobRequiredDocumentSubQueryForNotOperator(ZQuery filter, ZQuery query)
			{
				var clonedQuery = filter.DeepClone();
				clonedQuery.Simplify();

				var notInOperators = new List<JoinCondition>();
				var isBlankOperators = new List<JoinCondition>();
				var otherOperators = new List<JoinCondition>();
				AddJobRequiredDocumentSubQueryForNotOperatorCore(((IFilterPartsProvider)clonedQuery).FilterParts, query, JoinCondition.And, ref notInOperators, ref isBlankOperators, ref otherOperators);

				if ((notInOperators.Count > 0 && (otherOperators.Count == 0 || notInOperators.Any(c => c == JoinCondition.Or) || isBlankOperators.Count > 0))
					|| (isBlankOperators.Count > 0 && notInOperators.Count == 0 && otherOperators.Count == 0))
				{
					query.AddToFilter(GetSubQueryForJobRequiredDocument(new ZQuery(), true), JoinCondition.Or);
				}
			}

			void AddJobRequiredDocumentSubQueryForNotOperatorCore(IFilterPart[] parts, ZQuery query, JoinCondition joinCondition, ref List<JoinCondition> notInOperators, ref List<JoinCondition> isBlankOperators, ref List<JoinCondition> otherOperators)
			{
				for (var i = 0; i < parts.Length; i++)
				{
					var condition = joinCondition;
					if (i - 1 > 0)
					{
						condition = parts[i - 1] as JoinCondition;
					}

					if (parts[i] is ZSqlParameter parameter)
					{
						var paramOperator = parameter.ComparisonOperator;
						if (SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref paramOperator)
							&& !(paramOperator == SQLComparisonOperator.Equal && parameter.Value == DBNull.Value))
						{
							if (i == 0 && i + 1 < parts.Length)
							{
								notInOperators.Add(parts[i + 1] as JoinCondition);
							}
							else
							{
								notInOperators.Add(condition);
							}

							query.AddToFilter(GetSubQueryForJobRequiredDocument(new ZQuery(parameter.SchemaColumn, paramOperator, parameter.Value), true));
						}
						else if (paramOperator == SQLComparisonOperator.IsBlank)
						{
							isBlankOperators.Add(condition);
							query.AddToFilter(GetSubQueryForJobRequiredDocument(new ZQuery(), true), condition);
						}
						else
						{
							otherOperators.Add(condition);
						}
					}
					else if (parts[i] is IFilterPartsProvider provider)
					{
						AddJobRequiredDocumentSubQueryForNotOperatorCore(provider.FilterParts, query, condition, ref notInOperators, ref isBlankOperators, ref otherOperators);
					}
				}
			}

			ZQuery GetSubQueryForJobRequiredDocument(ZQuery filter, bool forNotOperator = false)
			{
				var query = new ZDBOnlyQuery(businessObjectType);

				var subQueryForJobRequiredDocument = new ZDBOnlySubQuery(typeof(JobRequiredDocument), JobRequiredDocumentSchema.EQ_ParentID, forNotOperator);

				if (typeof(IDocsAndCartageParent).IsAssignableFrom(businessObjectType))
				{
					var subQueryForForwardingDocsAndCartage = new ZDBOnlySubQuery(typeof(Forwarding.IForwardingDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);

					subQueryForJobRequiredDocument.AddToFilter(JobRequiredDocumentSchema.EQ_ParentTableCode, JobDocsAndCartageSchema.Constants.Prefix);
					subQueryForJobRequiredDocument.AddToFilter(filter);

					subQueryForForwardingDocsAndCartage.AddSubQuery(subQueryForJobRequiredDocument, JoinCondition.And);
					AddSubQuery(query, subQueryForForwardingDocsAndCartage);
				}
				else
				{
					subQueryForJobRequiredDocument.AddToFilter(filter);
					subQueryForJobRequiredDocument.AddToFilter(JobRequiredDocumentSchema.EQ_ParentTableCode, ParentTableCode);

					query.AddSubQuery(subQueryForJobRequiredDocument, JoinCondition.And);
				}

				return query;
			}

			protected virtual void AddSubQuery(ZDBOnlyQuery query, ZDBOnlySubQuery subQueryForForwardingDocsAndCartage)
			{
				query.AddSubQuery(subQueryForForwardingDocsAndCartage, JoinCondition.And);
			}
		}

#if DEBUG
		public override string GetAutomaticFilterTestCaseName_ForObjectFactory() => "DocumentTrackingFilterStripsHelperAutomaticFilterTest";
#endif
	}
}
