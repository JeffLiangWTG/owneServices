using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module
{
	public abstract class TemplateRecordSupportedFilterGridModule : ZFilterGridModule
	{
		protected override bool IsModuleAllowAsync => false; // Because accessing FilterBusinessObject cross-thread is probably unsafe? But TODO, because Shipments are important.

		protected override FilteredGridLoader CreateSearchManager()
			=> new TemplateRecordSupportedFilterdGridLoader(FilterBusinessObject, ResultCountMessage, ModuleDecisionProvider, ID, GetNewFactory, GridCollection.TypeOfElements);

		internal bool SupportForTemplateRecords => SupportTemplateRecords;

		internal BusinessObject[] LoadCollectionCore(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			var activeModuleFilters = FilterBusinessObject.ActiveModuleFilters.OfType<ModuleTextFilter>();
			var templatesOption = GetTemplatesOption(activeModuleFilters);

			BusinessObject[] bizos;
			if (templatesOption != FilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly)
			{
				AddBusinessSpecificFiltersToBaseQuery(query);

				bizos = factory.Load(type, query);
			}
			else
			{
				bizos = Array.Empty<BusinessObject>();
				query = new ZQuery() { IsNoResultQuery = true };
			}

			if (AllowLoadTemplateRecords && (
				templatesOption == FilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly ||
				templatesOption == FilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesIncluded)
			)
			{
				var templateRecords = TryLoadTemplateRecords(factory, type, GetTemplateRecordsQuery(activeModuleFilters));
				bizos = bizos.Concat(templateRecords).ToArray();
			}

			return bizos;
		}

		internal ZString GetTemplatesOption(IEnumerable<ModuleTextFilter> activeModuleFilters) =>
			AllowLoadTemplateRecords
				? activeModuleFilters
					.Where(textFilter => textFilter.IsActive && textFilter.Description == FilterStripBusinessObject.TemplateRecordsDescription)
					.Select(textFilter => textFilter.Property).FirstOrDefault()
				: ZString.Empty;

		internal ZDBOnlyQuery GetTemplateRecordsQuery(IEnumerable<ModuleTextFilter> activeModuleFilters)
		{
			var additionalFilters = FilterBusinessObject
				.ActiveModuleFilters
				.OfType<ModuleFountainFilter>()
				.Where(textFilter => textFilter.IsActive && textFilter.OriginalCode == ModuleReferenceNumberFilterString)
				.ToList();

			var templateFilters = activeModuleFilters.Where(textFilter =>
				textFilter.IsActive &&
				(textFilter.Description == FilterStripBusinessObject.TemplateRecordsActive ||
				textFilter.Description == FilterStripBusinessObject.TemplateRecordsTemplateName)).ToList();

			var templateRecordsQuery = new ZDBOnlyQuery(typeof(StmTemplateRecord));
			templateRecordsQuery.IgnoreBlobFieldsCheck = true;
			templateRecordsQuery.MaximumRows = SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value + 1;
			templateRecordsQuery.AddToFilter(StmTemplateRecordSchema.STR_ModuleID, ID.Name);

			AddTemplateFilters(templateRecordsQuery, templateFilters);
			AddAdditionalFilters(templateRecordsQuery, additionalFilters);
			AddBusinessSpecificFiltersToTemplateQuery(templateRecordsQuery);

			return templateRecordsQuery;
		}

		protected virtual BusinessObject LoadTemplateRecordIntoCollection(BusinessObjectFactory factory, Type type, StmTemplateRecord templateRecord)
		{
			templateRecord.IsForTemplateSearch = true;
			templateRecord.Logger = new Business.ForwardingShipment.DummyLoggerForTemplate();

			var item = factory.New(type);
			((ITemplateRecordProvider)item).LoadFromTemplateRecord(templateRecord);
			return item;
		}

		protected virtual void AddBusinessSpecificFiltersToTemplateQuery(ZDBOnlyQuery query)
		{
		}

		protected virtual void AddBusinessSpecificFiltersToBaseQuery(ZQuery query)
		{
		}

		protected virtual string ModuleReferenceNumberFilterString => ZString.Empty;

		#region Implementation

		IEnumerable<BusinessObject> TryLoadTemplateRecords(BusinessObjectFactory factory, Type type, ZDBOnlyQuery query)
		{
			factory.SuspendValidation();

			try
			{
				var templateRecords = factory.Load<StmTemplateRecord>(query);
				var result = new List<BusinessObject>();

				foreach (var templateRecord in templateRecords)
				{
					var record = LoadTemplateRecordIntoCollection(factory, type, templateRecord);
					if (record != null)
					{
						result.Add(record);
					}
				}
				return result;
			}
			finally
			{
				factory.ResumeValidation();
			}
		}

		void AddTemplateFilters(ZDBOnlyQuery query, ICollection<ModuleTextFilter> templateFilters)
		{
			if (templateFilters != null)
			{
				foreach (var templateFilter in templateFilters)
				{
					if (templateFilter.Description == FilterStripBusinessObject.TemplateRecordsActive && !FilterStripBusinessObject.StatusAll.EqualsUnresolvedOrLocalized(templateFilter.Property, ignoreCase: false))
					{
						query.AddToFilter(new ZQuery(StmTemplateRecordSchema.STR_IsActive, FilterStripBusinessObject.StatusActive.EqualsUnresolvedOrLocalized(templateFilter.Property, ignoreCase: false)));
					}
					else if (templateFilter.Description == FilterStripBusinessObject.TemplateRecordsTemplateName)
					{
						query.AddToFilter(new ZQuery(StmTemplateRecordSchema.STR_TemplateName, templateFilter.SqlComparisonOperator, templateFilter.Property));
					}
				}
			}
		}

		void AddAdditionalFilters(ZDBOnlyQuery query, ICollection<ModuleFountainFilter> additionalFilters)
		{
			if (additionalFilters != null && additionalFilters.Count > 0)
			{
				var shipmentFilterQuery = new ZQuery();

				foreach (var filter in additionalFilters)
				{
					if (!string.IsNullOrWhiteSpace(filter.Property))
					{
						shipmentFilterQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.Or, StmTemplateRecordSchema.STR_ReferenceId, filter.SqlComparisonOperator, filter.Property);
					}
				}

				query.AddToFilter(shipmentFilterQuery, JoinCondition.And);
			}
		}

		#endregion
	}

	class TemplateRecordSupportedFilterdGridLoader : FilteredGridLoader
	{
		public TemplateRecordSupportedFilterdGridLoader(
			FilterStripBusinessObject filterBusinessObject,
			ResultCountMessage handler,
			IModuleDecisionProvider provider,
			ModuleIdentifier moduleId,
			Func<BusinessObjectFactory> createFactory,
			Type typeOfElements)
		: base(filterBusinessObject, handler, provider, moduleId, createFactory, typeOfElements)
		{
		}

		protected override bool PermitActiveCollectionUpdates => false;

		protected override BusinessObject[] LoadCollectionCore(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			var module = (TemplateRecordSupportedFilterGridModule)filterBusinessObject.ParentModule;

			if (!module.SupportForTemplateRecords)
			{
				return base.LoadCollectionCore(factory, type, query);
			}

			return module.LoadCollectionCore(factory, type, query);
		}

		public override int GetEstimatedLoadCount(IBusinessObjectCollection gridCollection, ZQuery query)
		{
			var module = (TemplateRecordSupportedFilterGridModule)filterBusinessObject.ParentModule;
			var activeModuleFilters = module.FilterBusinessObject.ActiveModuleFilters.OfType<ModuleTextFilter>();
			var templatesOption = module.GetTemplatesOption(activeModuleFilters);
			var templateRecordsQuery = module.GetTemplateRecordsQuery(activeModuleFilters);

			if (module.SupportForTemplateRecords)
			{
				if (templatesOption == FilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly)
				{
					return gridCollection.Factory.GetDatabaseCount(typeof(StmTemplateRecord), templateRecordsQuery);
				}
				else if (templatesOption == FilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesIncluded)
				{
					return base.GetEstimatedLoadCount(gridCollection, query) + gridCollection.Factory.GetDatabaseCount(typeof(StmTemplateRecord), templateRecordsQuery);
				}
			}

			return base.GetEstimatedLoadCount(gridCollection, query);
		}
	}
}
