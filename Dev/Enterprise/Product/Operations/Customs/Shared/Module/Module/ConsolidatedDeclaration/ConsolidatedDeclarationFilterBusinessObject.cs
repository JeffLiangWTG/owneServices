using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class ConsolidatedDeclarationFilterBusinessObject : FilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		public static class Constants
		{
			public const string PeriodTo = "Entry Period Date";
			public const string LoadDischarge = "Load/Discharge";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			using (var jobDeclarationModule = ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration) as JobDeclarationModule)
			{
				jobDeclarationFilterBusinessObject = jobDeclarationModule.FilterBusinessObject as JobDeclarationFilterBusinessObject;
				foreach (var filter in jobDeclarationFilterBusinessObject)
				{
					if (effectiveJobDeclarationFilters.Contains(filter.Code))
					{
						if (filter.SubGroup == null)
						{
							filter.SubGroup = new EmptyJobDeclarationSubGroup();
						}
						result.AddFilter(filter);
					}
				}
			}
			AddEntryStatusFilter(result);
			AddMessageStatusFilter(result);
			AddPeriodToFilter(result);
			var loadDischargeFilter = result[DeclarationFilterConstants.PortFilterTypes.LoadDischarge];
			if (loadDischargeFilter != null)
			{
				loadDischargeFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|ConsolidatedDeclarationFilter|LoadDischarge", Constants.LoadDischarge);
			}
			return result;
		}

		protected override ZQuery CombineModuleFilters()
		{
			List<ModuleFilter> consolidatedDeclarationFilters = new List<ModuleFilter>();
			List<ModuleFilter> jobDeclarationFilters = new List<ModuleFilter>();
			foreach (var filter in ActiveModuleFiltersForQuery)
			{
				if (effectiveJobDeclarationFilters.Contains(filter.Description))
				{
					jobDeclarationFilters.Add(filter);
				}
				else
				{
					consolidatedDeclarationFilters.Add(filter);
				}
			}

			if (jobDeclarationFilters.Count > 0)
			{
				var result = new ZDBOnlyQuery(typeof(CusReconDeclaration));
				ModuleFilters.InvalidateCachedQuery();
				result.AddToFilter(ModuleFilters.GetFilterQuery(consolidatedDeclarationFilters, ApplyToFilterGroups));
				var jobDeclarationSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), CusReconDeclarationSchema.CRD_JE_LeadDeclaration);
				jobDeclarationSubQuery.AddToFilter(ModuleFilters.GetFilterQuery(jobDeclarationFilters, ApplyToFilterGroups));
				result.AddSubQuery(jobDeclarationSubQuery, JoinCondition.And);
				return result;
			}
			else
			{
				return base.CombineModuleFilters();
			}
		}

		protected override void AddInitialAuditFilters(ModuleFilterCollection filters)
		{
			base.AddInitialAuditFilters(filters);

			var createdTimeFilter = filters[FilterDescriptions.CreatedTime] as ModuleDateFilter;
			if (createdTimeFilter != null)
			{
				createdTimeFilter.Visibility = FilterVisibility.AlwaysVisible;
				createdTimeFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last3Mths;
			}
		}

		#region Job #
		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			ModuleNumberFilter declarationFilter = new ModuleNumberFilter(DeclarationFilterConstants.NumberFilterTypes.DeclarationReference, CusReconDeclarationSchema.CRD_JobReferenceNumber);
			declarationFilter.MaxLength = CusReconDeclarationSchema.CRD_JobReferenceNumber.MaxLength;
			declarationFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|DeclarationReference", DeclarationFilterConstants.NumberFilterTypes.DeclarationReference);
			return declarationFilter;
		}
		#endregion

		#region Entry Status
		protected virtual ModuleFilter AddEntryStatusFilter(ModuleFilterCollection filters)
		{
			var entryStatusfilter = filters.AddTextFilter(DeclarationFilterConstants.EntryStatusText, GetEntryStatusQuery, jobDeclarationFilterBusinessObject.Lookups.EntryStatusList)
				.WithMaxLengthOf<ModuleTextFilter>(JobDeclarationSchema.JE_EntryStatus);
			entryStatusfilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			entryStatusfilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			entryStatusfilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			entryStatusfilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			entryStatusfilter.Category = FilterCategories.StatusAndFlags;
			entryStatusfilter.MultilingualDescription = jobDeclarationFilterBusinessObject.EntryStatusText;
			entryStatusfilter.ComparisonOperatorChanged += CustomsEntryStatusFilter_ComparisonOperatorChanged;
			return entryStatusfilter;
		}

		protected void CustomsEntryStatusFilter_ComparisonOperatorChanged(object sender, EventArgs e)
		{
			if (sender is ModuleTextFilter moduleTextFilter && !moduleTextFilter.ComparisonOperator_List.ContainsCode(moduleTextFilter.ComparisonOperator))
			{
				moduleTextFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			}
		}

		protected virtual ZQuery GetEntryStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (value == DeclarationFilterConstants.EntryStatus.NotSentForFilter)
			{
				value = ZString.Empty;
			}

			return new ZQuery(CusReconDeclarationSchema.CRD_CustomsStatus, comparisonOperator, value);
		}
		#endregion

		#region Message Status
		protected virtual ModuleFilter AddMessageStatusFilter(ModuleFilterCollection filters)
		{
			var messageStatusfilter = filters.AddTextFilter(DeclarationFilterConstants.MessageStatusText, GetMessageStatusQuery, MessageStatusList);
			messageStatusfilter.Category = FilterCategories.StatusAndFlags;
			messageStatusfilter.MultilingualDescription = jobDeclarationFilterBusinessObject.MessageStatusText;
			return messageStatusfilter;
		}

		protected virtual ZQuery GetMessageStatusQuery(ZString value)
		{
			if (value == DeclarationFilterConstants.EntryStatus.NotSentForFilter)
			{
				value = ZString.Empty;
			}

			return new ZQuery(CusReconDeclarationSchema.CRD_MessageStatus, value);
		}

		protected virtual CodeDescriptionPairList MessageStatusList => jobDeclarationFilterBusinessObject.Lookups.MessageStatusList();
		#endregion

		#region Period To
		ModuleFilter AddPeriodToFilter(ModuleFilterCollection filters)
		{
			var result = filters.AddDateFilter(Constants.PeriodTo, CusReconDeclarationSchema.CRD_PeriodTo);
			result.MultilingualDescription = ResString.GetMultilingualString("Customs|ConsolidatedDeclarationFilter|Period To", Constants.PeriodTo);
			return result;
		}
		#endregion

		protected JobDeclarationFilterBusinessObject jobDeclarationFilterBusinessObject;

		readonly ImmutableHashSet<string> effectiveJobDeclarationFilters = new[]
		{
			DeclarationFilterConstants.NumberFilterTypes.EntryNumber,
			DeclarationFilterConstants.NumberFilterTypes.MasterBill,
			DeclarationFilterConstants.PortFilterTypes.LoadDischarge,
			DeclarationFilterConstants.DateFilterTypes.DateOfExport,
			DeclarationFilterConstants.DateFilterTypes.DateOfArrival,
			DeclarationFilterConstants.TransportMode,
			DeclarationFilterConstants.FlightVoyageVessel,
			DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier,
			DeclarationFilterConstants.DateFilterTypes.Submitted,
		}.ToImmutableHashSet();
	}

	class EmptyJobDeclarationSubGroup : ModuleFilterSubGroup
	{
		public override ZQuery GetSubQuery(ZQuery filter) => filter;
	}
}
