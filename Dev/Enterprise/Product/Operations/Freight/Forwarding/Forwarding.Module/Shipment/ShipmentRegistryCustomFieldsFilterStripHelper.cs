using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;

namespace Enterprise.Freight.Forwarding.Module
{
	class ShipmentRegistryCustomFieldsFilterStripHelper : IFilterStripsHelper
	{
		public BusinessObjectFactory Factory { get; private set; }
		static FilterCategory Category => FilterCategories.GetOrCreateFilterCategory(FilterStripBusinessObject.CustomFieldCategoryDescription);

		public Type BusinessObjectType => typeof(ForwardingShipment);

		public void AddFilterStrips(IModuleFilterCollection filters)
		{
			var moduleFilters = (ModuleFilterCollection)filters;
			var allActiveCustomFields = new JobDocsAndCartageCustomFieldsDescriptor().ActiveCustomFieldsInfos.ToList();
			AddTextFilters(moduleFilters, allActiveCustomFields.Where(f => typeof(ZString).IsAssignableFrom(f.ZDataType)));
			AddDateTimeFilters(moduleFilters, allActiveCustomFields.Where(f => typeof(ZDateTime).IsAssignableFrom(f.ZDataType)));
			AddBoolFilters(moduleFilters, allActiveCustomFields.Where(f => typeof(ZBool).IsAssignableFrom(f.ZDataType)));
			AddDecimalFilters(moduleFilters, allActiveCustomFields.Where(f => typeof(ZDecimal).IsAssignableFrom(f.ZDataType)));
		}

		void AddTextFilters(ModuleFilterCollection filters, IEnumerable<CustomFieldInfo> textCustomFields)
		{
			foreach (var textCustomField in textCustomFields)
			{
				var filter = filters.AddTextFilter(GetFilterName(textCustomField.Caption), GetSchemaColumn<SchemaStringColumn>(textCustomField.SchemaColumnName));
				filter.Category = Category;
				filter.SubGroup = JobDocsAndCartageFilterSubGroupProcessor;
			}
		}

		void AddDateTimeFilters(ModuleFilterCollection filters, IEnumerable<CustomFieldInfo> dateTimeCustomFields)
		{
			foreach (var dateTimeCustomField in dateTimeCustomFields)
			{
				var filter = filters.AddDateFilter(GetFilterName(dateTimeCustomField.Caption), GetSchemaColumn<SchemaDateTimeColumn>(dateTimeCustomField.SchemaColumnName));
				filter.Category = Category;
				filter.SubGroup = JobDocsAndCartageFilterSubGroupProcessor;
			}
		}

		void AddBoolFilters(ModuleFilterCollection filters, IEnumerable<CustomFieldInfo> boolCustomFields)
		{
			foreach (var boolCustomField in boolCustomFields)
			{
				var filter = filters.AddFlagsFilter(GetFilterName(boolCustomField.Caption), new[] { boolCustomField.Caption }, new[] { GetSchemaColumn<SchemaBoolColumn>(boolCustomField.SchemaColumnName) });
				filter.Category = Category;
				filter.SubGroup = JobDocsAndCartageFilterSubGroupProcessor;
			}
		}

		void AddDecimalFilters(ModuleFilterCollection filters, IEnumerable<CustomFieldInfo> decimalCustomFields)
		{
			foreach (var decimalCustomField in decimalCustomFields)
			{
				var filter = filters.AddNumberRangeFilter(GetFilterName(decimalCustomField.Caption), GetSchemaColumn<SchemaNumericColumn>(decimalCustomField.SchemaColumnName));
				filter.Category = Category;
				filter.SubGroup = JobDocsAndCartageFilterSubGroupProcessor;
			}
		}

		string GetFilterName(string caption)
		{
			return Res.GetString("b794a84f-a0d4-0aa8-48be-bbee4379bb24", "{0} (Custom)", caption.Trim());
		}

		public bool CanAddFilters() => true;

		public string GetAutomaticFilterTestCaseName_ForObjectFactory() => "ShipmentRegistryCustomFieldsFilterStripHelperAutomaticFilterTest";

		public void Initialise(Type businessObjectType, BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public bool IsApplicableToBizOTypeIsAssignableFrom() => true;

		static TSchemaColumn GetSchemaColumn<TSchemaColumn>(string columnName) where TSchemaColumn : SchemaColumn
		{
			return (TSchemaColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(columnName, JobDocsAndCartageSchema.Constants.TableName);
		}

		public void AddFilterStripsForIndexSearch(IModuleFilterCollection filters, SearchField[] defaultHiddenIndexSearchFields)
		{
		}

		class JobDocsAndCartageFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
				var subQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);

				subQuery.AddToFilter(filter);
				result.AddSubQuery(subQuery, JoinCondition.And);

				return result;
			}
		}

		ModuleFilterSubGroup JobDocsAndCartageFilterSubGroupProcessor
		{
			get { return jobDocsAndCartageFilterSubGroup ?? (jobDocsAndCartageFilterSubGroup = new JobDocsAndCartageFilterSubGroup()); }
		}
		JobDocsAndCartageFilterSubGroup jobDocsAndCartageFilterSubGroup;
	}
}
