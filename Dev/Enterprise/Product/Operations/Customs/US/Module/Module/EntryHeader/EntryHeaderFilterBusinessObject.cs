using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using BillTypeList = Enterprise.Customs.Business.BillTypeList;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	public class EntryHeaderFilterBusinessObject : Customs.Module.EntryHeaderFilterBusinessObject
	{
		public static class Schema
		{
			public const string EntryNumber = ModuleEntryHeaderCollection.USFilterConstants.EntryNumber;
			public const string ImportationDate = ModuleEntryHeaderCollection.USFilterConstants.ImportationDate;
			public const string ReconIssue = ModuleEntryHeaderCollection.USFilterConstants.ReconIssue;
			public const string ImporterOfRecord = ModuleEntryHeaderCollection.USFilterConstants.ImporterOfRecord;
			public const string PortOfEntry = ModuleEntryHeaderCollection.USFilterConstants.PortOfEntry;
			public const string SuretyCode = ModuleEntryHeaderCollection.USFilterConstants.SuretyCode;
			public const string EntryType = ModuleEntryHeaderCollection.USFilterConstants.EntryType;
			public const string ImportSource = ModuleEntryHeaderCollection.USFilterConstants.ImportSource;
			public const string HouseBill = ModuleEntryHeaderCollection.USFilterConstants.HouseBill;
			public const string MasterBill = ModuleEntryHeaderCollection.USFilterConstants.MasterBill;
		}

		Customs.Business.ModelViewColumnQueryHelper<JobDeclaration> ModelViewColumnHelper
		{
			get
			{
				return modelViewColumnHelper ?? (modelViewColumnHelper = new Customs.Business.ModelViewColumnQueryHelper<JobDeclaration>());
			}
		}
		Customs.Business.ModelViewColumnQueryHelper<JobDeclaration> modelViewColumnHelper;
		const string ModelView = "USJobDeclaration";
		const string ModelViewPK = "JE_PK";

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			result.AddDateFilter(Constants.SubmissionDate, CusEntryHeaderSchema.CH_EntrySubmittedDate);

			var entryStatusfilter = result.AddTextFilter(Constants.EntryStatus, GetEntryStatusQuery, GetEntryStatusList);
			entryStatusfilter.Category = FilterCategories.StatusAndFlags;

			var messageStatusfilter = result.AddTextFilter(Constants.MessageStatus, GetMessageStatusQuery, Lookups.MessageStatusList);
			messageStatusfilter.Category = FilterCategories.StatusAndFlags;

			var usEntryNumberSubGroup = new USEntryNumberSubGroup(JobDeclarationSubGroup);
			var entryNumberFilter = result.AddNumberFilter(Schema.EntryNumber, GetEntryNumberQuery);
			entryNumberFilter.SubGroup = usEntryNumberSubGroup;
			entryNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;

			var usHouseBillSubGroup = new USHouseBillSubGroup(JobDeclarationSubGroup);
			var houseBillFilter = result.AddNumberFilter(Schema.HouseBill, GetBillQuery);
			houseBillFilter.SubGroup = usHouseBillSubGroup;
			houseBillFilter.MaxLength = CusDecHouseBillSchema.CU_BillNum.MaxLength;

			var usMasterBillSubGroup = new USMasterBillSubGroup(JobDeclarationSubGroup);
			var masterBillFilter = result.AddNumberFilter(Schema.MasterBill, GetBillQuery);
			masterBillFilter.SubGroup = usMasterBillSubGroup;
			masterBillFilter.MaxLength = CusDecHouseBillSchema.CU_BillNum.MaxLength;

			result.AddDateFilter(Constants.ReleaseDate, GetReleaseDateQuery);
			result.AddDateFilter(Schema.ImportationDate, GetImportationDateQuery);

			result.AddTextFilter(Schema.ReconIssue, GetOtherReconIndicatorQuery, Factory.GetCachedValue<ReconIssueCodeList>());

			result.AddTextFilter(Schema.ImportSource, GetImportSourceQuery, Factory.GetCachedValue<ReconciliationImportEntrySourceList>());

			var fTAReconIssueFilter = result.AddFlagsFilter(DeclarationFilterConstants.FTAReconIndicator, new string[] { DeclarationFilterConstants.Flagged }, new GetFlagsQuery[] { GetFTAReconIssueQuery });
			fTAReconIssueFilter.Category = FilterCategories.ModesAndTypes;

			ModuleFilter includeIORFilingTheirOwnRec = result.AddFlagsFilter(DeclarationFilterConstants.ExcludeIORFilingTheirOwnRec, new string[] { DeclarationFilterConstants.Exclude }, new GetFlagsQuery[] { GetExcludeIORFilingTheirOwnRecQuery });
			includeIORFilingTheirOwnRec.Category = FilterCategories.ModesAndTypes;

			AddEntryTypeQuery(result);

			AddSuretyCodeQuery(result);

			result.AddGuidFilter(Schema.ImporterOfRecord, ModuleIDs.Organisation, GetImporterOfRecordQuery, new OrgHeaderCollection(Factory));

			result.AddNkFilter(Schema.PortOfEntry, GetPortOfEntryQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today));

			ModuleFilter notReconciledFilter = result.AddFlagsFilter(DeclarationFilterConstants.Reconciliation, new string[] { DeclarationFilterConstants.NotYetReconciled }, new GetFlagsQuery[] { GetNotReconciledQuery });
			notReconciledFilter.Category = FilterCategories.StatusAndFlags;

			var entrySummaryStatusFilter = result.AddTextFilter(DeclarationFilterConstants.EntrySummaryStatus, GetEntrySummaryStatusQuery, Lookups.MessageStatusListForENS);
			entrySummaryStatusFilter.MaxLength = CusEntryHeaderSchema.CH_Status.MaxLength;
			DBStatusQueryAndFilterConstraints.SetFilterConstraints(entrySummaryStatusFilter, FilterCategories.StatusAndFlags);

			return result;
		}

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			var jobNumberFilter = new ModuleNumberFilter(Constants.JobNumber, JobDeclarationSchema.JE_DeclarationReference);
			jobNumberFilter.SubGroup = JobDeclarationSubGroup;
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|USJobNumber", Constants.JobNumber);
			return jobNumberFilter;
		}

		public new EntryHeaderFilterLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = GetNewLookups();
				}
				return lookups;
			}
		}
		EntryHeaderFilterLookups lookups;

		protected new EntryHeaderFilterLookups GetNewLookups() => new EntryHeaderFilterLookups(this);

		ZQuery GetNotReconciledQuery(ZBool value)
		{
			var result = new ZDBOnlyQuery(typeof(CusEntryHeader));
			result.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.EntrySummary);

			if (value)
			{
				var declarationQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
				declarationQuery.AddToFilter(ModelViewColumnHelper.GetModuleFilterQuery(CusEntryHeader.Schema.CH_JE, ModelViewPK, ModelView, true, new (ZString, object)[] { ("JE_OtherReconIndicator", ZString.Empty), ("JE_NAFTAReconIndicator", false) }));
				result.AddSubQuery(declarationQuery, JoinCondition.And);

				//Recon Entry exists?
				var reconEntryQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_CH_PrimeEntry, true);
				reconEntryQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry);
				reconEntryQuery.AddToFilter(CusEntryHeaderSchema.CH_CH_PrimeEntry, SQLComparisonOperator.NotEqual, DBNull.Value);
				result.AddSubQuery(reconEntryQuery, JoinCondition.And);
			}

			return result;
		}

		void AddEntryTypeQuery(ModuleFilterCollection result)
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(EntryTypeList.Codes.ConsumptionFreeDutiable, EntryTypeList.Descriptions.ConsumptionFreeDutiable);

			var parent = ParentModule as EntryHeaderModule;
			if (parent != null)
			{
				if (parent.ParentModalFormOwner != null && parent.ParentModalFormOwner.GetType() == typeof(ReconDeclarationForm))
				{
					list.AddPair(EntryTypeList.Codes.ConsumptionQuotaVisa, EntryTypeList.Descriptions.ConsumptionQuotaVisa);
					list.AddPair(EntryTypeList.Codes.ConsumptionFTZ, EntryTypeList.Descriptions.ConsumptionFTZ);
				}
				else
				{
					list.AddPair(EntryTypeList.Codes.InformalFreeDutiable, EntryTypeList.Descriptions.InformalFreeDutiable);
				}
			}
			result.AddTextFilter(Schema.EntryType, GetEntryTypeQuery, list);
		}

		ZQuery GetEntryTypeQuery(ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(CusEntryHeader.Schema.CH_JE, ModelViewPK, ModelView, "JE_EntryType", SQLComparisonOperator.Equal, value);
		}

		void AddSuretyCodeQuery(ModuleFilterCollection result)
		{
			var suretyCodeFilter = result.AddNumberFilter(Schema.SuretyCode, GetSuretyCodeQuery);
			suretyCodeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			suretyCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			suretyCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			suretyCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			suretyCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			suretyCodeFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Exact;
			suretyCodeFilter.MaxLength = USAddInfoSchema.US_SuretyCode.MaxLength;
		}

		ZQuery GetImportationDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = new ZDBOnlyQuery(typeof(CusEntryHeader));

			var dateQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
			AddDateRange(dateQuery, comparisonOperator, JoinCondition.And, JobDeclarationSchema.JE_DateOfArrival, date1.Date, date2.Date);

			if (!date1.IsValid && !date2.IsValid)
			{
				dateQuery.IsNoResultQuery = true;
			}

			result.AddSubQuery(dateQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetReleaseDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = new ZDBOnlyQuery(typeof(CusEntryHeader));

			var dateQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
			AddDateRange(dateQuery, comparisonOperator, JoinCondition.And, JobDeclarationSchema.JE_EntryAuthorisationDate, date1.Date, date2.Date);

			if (!date1.IsValid && !date2.IsValid)
			{
				dateQuery.IsNoResultQuery = true;
			}

			result.AddSubQuery(dateQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetSuretyCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(CusEntryHeader.Schema.CH_JE, ModelViewPK, ModelView, "JE_SuretyCode", comparisonOperator, value);
		}

		ZQuery GetOtherReconIndicatorQuery(ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(CusEntryHeader.Schema.CH_JE, ModelViewPK, ModelView, "JE_OtherReconIndicator", SQLComparisonOperator.Equal, value);
		}

		ZQuery GetImportSourceQuery(ZString value)
		{
			switch (value)
			{
				case ReconciliationImportEntrySourceList.Codes.PuertoRico:
					return ModelViewColumnHelper.GetModuleFilterQuery(CusEntryHeader.Schema.CH_JE, ModelViewPK, ModelView, "JE_SchDEntry", SQLComparisonOperator.StartsWith, "49");
				case ReconciliationImportEntrySourceList.Codes.VirginIslands:
					return ModelViewColumnHelper.GetModuleFilterQuery(CusEntryHeader.Schema.CH_JE, ModelViewPK, ModelView, "JE_SchDEntry", SQLComparisonOperator.StartsWith, "51");
				case ReconciliationImportEntrySourceList.Codes.FiftyStates:
					var query = ModelViewColumnHelper.GetModuleFilterQuery(CusEntryHeader.Schema.CH_JE, ModelViewPK, ModelView, "JE_SchDEntry", SQLComparisonOperator.DoesNotStartWith, "49");
					query.AddToFilter(ModelViewColumnHelper.GetModuleFilterQuery(CusEntryHeader.Schema.CH_JE, ModelViewPK, ModelView, "JE_SchDEntry", SQLComparisonOperator.DoesNotStartWith, "51"));
					return query;
				default:
					return new ZQuery();
			}
		}

		ZQuery GetFTAReconIssueQuery(ZBool value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(CusEntryHeader.Schema.CH_JE, ModelViewPK, ModelView, "JE_NAFTAReconIndicator", SQLComparisonOperator.Equal, value);
		}

		ZQuery GetExcludeIORFilingTheirOwnRecQuery(ZBool value)
		{
			var result = new ZQuery();

			if (value)
			{
				ZString filterValue = USAddInfoSchema.Constants.US_FileTheirOwnRecon.Substring(3) + "=Y";
				result = GetDBOnlyQuery(JobDeclarationSchema.JE_AddInfo, SQLComparisonOperator.NotContains, filterValue);
			}

			return result;
		}

		ZDBOnlyQuery GetDBOnlyQuery(SchemaColumn declarationColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			var result = new ZDBOnlyQuery(typeof(CusEntryHeader));

			var subQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
			subQuery.AddToFilter(declarationColumn, comparisonOperator, value);
			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}

		ZDBOnlyQuery GetImporterOfRecordQuery(ZGuid importerOfRecordPK)
		{
			var result = new ZDBOnlyQuery(typeof(CusEntryHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
			var subAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDeclarationSchema.JE_OA_DeclarantAddress);
			subAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, SQLComparisonOperator.Equal, importerOfRecordPK);
			subQuery.AddSubQuery(subAddressQuery, JoinCondition.And);
			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetPortOfEntryQuery(ZString portOfEntry)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(CusEntryHeader.Schema.CH_JE, ModelViewPK, ModelView, "JE_SchDEntry", SQLComparisonOperator.Equal, portOfEntry);
		}

		protected override ZQuery GetEntryNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZQuery().AddToFilter_PossiblyCommaSeparated(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			result.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
			return result;
		}

		ZQuery GetBillQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery().AddToFilter_PossiblyCommaSeparated(CusDecHouseBillSchema.CU_BillNum, comparisonOperator, value);
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		protected override void AddDuplicateDefaultFilterStripCollection(FilterStripCollection collection)
		{
			var parent = ParentModule as EntryHeaderModule;
			if (parent != null)
			{
				if (parent.ParentModalFormOwner != null && parent.ParentModalFormOwner.GetType() == typeof(ReconDeclarationForm))
				{
					new DuplicateFilterStripForRecon(collection);
				}
				else
				{
					new DuplicateFilterStripForJobDeclaration(collection);
				}
			}
		}

		ZQuery GetEntrySummaryStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZQuery();
			result.AddToFilter(JoinCondition.And, CusEntryHeaderSchema.CH_Status, filterOperator, value);
			return result;
		}

		class USEntryNumberSubGroup : ModuleFilterSubGroup
		{
			public USEntryNumberSubGroup(ModuleFilterSubGroup jobDeclarationSubGroup)
				: base(jobDeclarationSubGroup)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(JobDeclaration));

				var entryNumberSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				entryNumberSubQuery.AddToFilter(filter);
				result.AddSubQuery(entryNumberSubQuery, JoinCondition.And);

				return result;
			}
		}

		class USHouseBillSubGroup : ModuleFilterSubGroup
		{
			public USHouseBillSubGroup(ModuleFilterSubGroup jobDeclarationSubGroup)
				: base(jobDeclarationSubGroup)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(JobDeclaration));
				var houseBillSubQuery = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.CU_JE);
				houseBillSubQuery.AddToFilter(filter);

				var billTypeFilter = new ZQuery(CusDecHouseBillSchema.CU_BillType, BillTypeList.Codes.SubHouseBill);
				billTypeFilter.AddToFilter(JoinCondition.Or, CusDecHouseBillSchema.CU_BillType, SQLComparisonOperator.Equal, BillTypeList.Codes.HouseBill);
				houseBillSubQuery.AddToFilter(billTypeFilter);
				result.AddSubQuery(houseBillSubQuery, JoinCondition.And);
				return result;
			}
		}

		class USMasterBillSubGroup : ModuleFilterSubGroup
		{
			public USMasterBillSubGroup(ModuleFilterSubGroup jobDeclarationSubGroup)
				: base(jobDeclarationSubGroup)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(JobDeclaration));
				var masterBillSubQuery = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.CU_JE);
				masterBillSubQuery.AddToFilter(CusDecHouseBillSchema.CU_BillType, BillTypeList.Codes.MasterBill);
				masterBillSubQuery.AddToFilter(filter);

				result.AddSubQuery(masterBillSubQuery, JoinCondition.And);
				return result;
			}
		}
	}
}
