using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Module;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.US.Business.JobDeclaration;

namespace Enterprise.Customs.US.Module
{
	sealed class ReconFilterStripBusinessObject : CurrentCompanyJobDeclarationFilterStripBusinessObject
	{
		public static class Schema
		{
			public const string Branch = "Branch";
			public const string DeclarationReference = "Job #";
			public const string Importer = "Importer";
			public const string Issue = "Issue Code";
			public const string FilingPort = "Filing Port";
			public const string EstReconDate = "Est. Recon Date";
			public const string PaymentType = "Payment Type";
			public const string StatementPrintDate = "Statement Print Date";
			public const string EntryNumber = "Entry #";
			public const string EntryNumberOnReconciliation = "Entry # On Reconciliation";
			public const string JobNumberOnReconciliation = "Job # On Reconciliation";
			public const string ReconciliationStatus = "Customs Status";
			public const string MessageStatus = "Message Status";
			public const string Preparer = "Preparer";
			public const string IsAggregate = "Is Aggregate";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			result.AddNumberFilter(Schema.DeclarationReference, JobDeclarationSchema.JE_DeclarationReference);
			result.AddGuidFilter(Schema.Importer, ModuleIDs.Organisation, JobDeclarationSchema.JE_OH_Importer, Lookups.Importers);
			var issue = result.AddTextFilter(Schema.Issue, GetIssueCodeQuery, Lookups.IssueCodeList);
			issue.MaxLength = USAddInfoSchema.US_IssueCode.MaxLength;
			issue.Category = FilterCategories.NumbersAndReferences;

			var port = result.AddTextFilter(Schema.FilingPort, GetFilingPortQuery, Lookups.ReconPortsList);
			port.Category = FilterCategories.NumbersAndReferences;
			port.MaxLength = USAddInfoSchema.US_SchDEntry.MaxLength;

			var payment = result.AddTextFilter(Schema.PaymentType, GetPaymentTypeQuery, Lookups.PaymentTypeList);
			payment.Category = FilterCategories.NumbersAndReferences;
			payment.MaxLength = USAddInfoSchema.US_PaymentType.MaxLength;

			var status = result.AddTextFilter(Schema.ReconciliationStatus, JobDeclarationSchema.JE_EntryStatus, Lookups.ReconMessageStatusList);
			status.Category = FilterCategories.StatusAndFlags;
			status.MaxLength = JobDeclarationSchema.JE_EntryStatus.MaxLength;

			var message = result.AddTextFilter(Schema.MessageStatus, GetMessageStatusQuery, Lookups.ReconMessageStatusList);
			message.Category = FilterCategories.StatusAndFlags;
			message.MaxLength = CusEntryHeaderSchema.CH_Status.MaxLength;

			result.AddDateFilter(Schema.EstReconDate, GetEstReconDateQuery);
			result.AddDateFilter(Schema.StatementPrintDate, GetStmPrintDateQuery);
			var isAggregate = result.AddFlagsFilter(Schema.IsAggregate, new string[] { Schema.IsAggregate }, new GetFlagsQuery[] { GetIsAggregateQuery });
			isAggregate.ArePropertiesMutuallyExclusive = true;
			isAggregate.Property1 = true;

			var number = result.AddNumberFilter(Schema.EntryNumber, GetEntryNumberQuery);
			number.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
			var reconciliation = result.AddNumberFilter(Schema.JobNumberOnReconciliation, GetDeclarationNumberOnReconciliationQuery);
			reconciliation.MaxLength = JobDeclarationSchema.JE_DeclarationReference.MaxLength;

			var entryNumberOnReconFilter = new EntryNoOnReconciliationFilter(Schema.EntryNumberOnReconciliation);
			entryNumberOnReconFilter.Category = FilterCategories.NumbersAndReferences;
			entryNumberOnReconFilter.MaxLength = CusEntryHeaderSchema.CH_BGMReference.MaxLength;
			result.AddCustomFilter(entryNumberOnReconFilter);

			var entrySummaryActionsFilter = result.AddTextFilter(DeclarationFilterConstants.EntrySummaryActions, GetEntrySummaryActionsQuery, Lookups.EntrySummaryActionsList);
			SetFilterConstraints(entrySummaryActionsFilter);
			entrySummaryActionsFilter.Category = FilterCategories.StatusAndFlags;
			entrySummaryActionsFilter.DefaultProperty = DeclarationFilterConstants.ALL;

			var suretyCodeFilter = result.AddTextFilter(DeclarationFilterConstants.SuretyCode, GetSuretyCodeQuery);
			suretyCodeFilter.Category = FilterCategories.TextSearch;
			suretyCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			suretyCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			suretyCodeFilter.MaxLength = USAddInfoSchema.US_SuretyCode.MaxLength;

			var brokerFilter = result.AddNkFilter(Schema.Preparer, JobDeclarationSchema.JE_GS_NKCusAgent, ModuleIDs.GlbStaff, Lookups.StaffList);
			brokerFilter.Category = FilterCategories.Organisations;
			brokerFilter.IsPublishedOnWeb = false;

			var branchFilter = result.AddGuidFilter(Schema.Branch, ModuleIDs.GlbBranch, JobDeclarationSchema.JE_GB, Lookups.BranchList);
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.IsPublishedOnWeb = false;

			var liquidationDateFilter = result.AddDateFilter(DeclarationFilterConstants.LiquidationDate, this.GetLiquidationDateQuery);
			liquidationDateFilter.Category = FilterCategories.Dates;

			var statementNoFilter = result.AddTextFilter(DeclarationFilterConstants.StatementNo, this.GetStatementNoQuery);
			statementNoFilter.Category = FilterCategories.NumbersAndReferences;
			statementNoFilter.MaxLength = CusStatementHeaderSchema.B2_StatementNumber.MaxLength;

			var disStatusFilter = result.AddTextFilter(DeclarationFilterConstants.DISStatus, JobDeclarationFilterBusinessObject.GetDISStatusQuery, Lookups.DISStatusList);
			DBStatusQueryAndFilterConstraints.SetFilterConstraints(disStatusFilter, FilterCategories.StatusAndFlags);

			var anticipatedLiquidationDateFilter = result.AddDateFilter(DeclarationFilterConstants.AnticipLiquidationDate, GetAnticipLiquidationDateQuery);
			anticipatedLiquidationDateFilter.Category = FilterCategories.Dates;

			return result;
		}

		ZQuery GetAnticipLiquidationDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			var entryHeaderSubQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
			var modelViewQuery = EntryHeaderModelViewHelper.GetDateFilterQuery(CusEntryHeader.Schema.PK, ModelViewPK_CusEntryHeader, ModelView_CusEntryHeader, "CH_ALDate", comparisonOperator, startDate, endDate);
			entryHeaderSubQuery.AddToFilter(modelViewQuery);
			entryHeaderSubQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.ReconEntry);
			query.AddSubQuery(entryHeaderSubQuery, JoinCondition.And);

			return query;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			helpers.Add(new WorkflowFilterStripsHelperCustoms(typeof(JobDeclaration), WorkflowDescriptors.ReconWorkflowDescriptorCode, Factory));

			return helpers;
		}

		ZQuery GetSuretyCodeQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_SuretyCode", filterOperator, value);
		}

		ZQuery GetIssueCodeQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_IssueCode", filterOperator, value);
		}

		ZQuery GetFilingPortQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_SchDEntry", filterOperator, value);
		}

		ZQuery GetEstReconDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			return ModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_EstimatedEntryDate", comparisonOperator, startDate, endDate);
		}

		ZQuery GetPaymentTypeQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_PaymentType", filterOperator, value);
		}

		ZQuery GetStmPrintDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			return ModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_PreliminaryStatementPrintDate", comparisonOperator, startDate, endDate);
		}

		ZQuery GetIsAggregateQuery(ZBool value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_IsAggregate", SQLComparisonOperator.Equal, value);
		}

		ZQuery GetMessageStatusQuery(ZString value)
		{
			var decQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			decQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Recon);

			var reconEntryQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
			reconEntryQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.ReconEntry);
			reconEntryQuery.AddToFilter(CusEntryHeaderSchema.CH_Status, value);

			decQuery.AddSubQuery(reconEntryQuery, JoinCondition.And);
			return decQuery;
		}

		ZQuery GetEntryNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var filterOperatorIsBlank = comparisonOperator == Enterprise.ZArchitecture.Business.SpecialComparisonOperator.IsBlank;

			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Recon);

			var entryNumberSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);

			if (!filterOperatorIsBlank)
			{
				entryNumberSubQuery.AddToFilter_PossiblyCommaSeparated(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			}

			entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
			result.AddSubQuery(entryNumberSubQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetDeclarationNumberOnReconciliationQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var notContains = comparisonOperator == SQLComparisonOperator.NotContains;
			var filterOperatorIsBlank = comparisonOperator == Enterprise.ZArchitecture.Business.SpecialComparisonOperator.IsBlank;
			var notIn = notContains || filterOperatorIsBlank;

			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			var entryQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, notIn);
			entryQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry);

			var originalEntryQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.PK);
			var originalDeclarationSubQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
			if (notContains)
			{
				originalDeclarationSubQuery.AddToFilter_PossiblyCommaSeparated(JobDeclarationSchema.JE_DeclarationReference, SQLComparisonOperator.Contains, value);
			}
			else if (!filterOperatorIsBlank)
			{
				originalDeclarationSubQuery.AddToFilter_PossiblyCommaSeparated(JobDeclarationSchema.JE_DeclarationReference, comparisonOperator, value);
			}

			originalEntryQuery.AddSubQuery(originalDeclarationSubQuery, JoinCondition.And);

			entryQuery.AddSubQuery(CusEntryHeaderSchema.CH_CH_PrimeEntry, originalEntryQuery, JoinCondition.And);
			result.AddSubQuery(entryQuery, JoinCondition.And);
			return result;
		}

		public override ZQuery Filter
		{
			get
			{
				var baseFilter = base.Filter;
				baseFilter.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Recon);

				return baseFilter;
			}
		}

		ZQuery GetEntrySummaryActionsQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			var comparisonOperator = SQLComparisonOperator.Equal;
			if (value == DeclarationFilterConstants.ALL)
			{
				comparisonOperator = SQLComparisonOperator.NotEqual;
				value = ZString.Empty;
			}
			result.AddToFilter(ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.JE_ClusterKey, JobDeclaration.Schema.JE_ClusterKey, ModelViewSchema.TableName_Recon, ModelViewSchema.JE_ENSAction, comparisonOperator, value));

			var entryQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_ClusterKey);
			entryQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.ReconEntry);
			result.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, entryQuery, JoinCondition.And);

			return result;
		}

		void SetFilterConstraints(ModuleTextFilter moduleFilter)
		{
			DBStatusQueryAndFilterConstraints.SetFilterConstraints(moduleFilter, FilterCategories.StatusAndFlags);
		}

		protected override string GetColumnPrefixFromType(Type cancellableType) => JobDeclarationSchema.Constants.Prefix;

		Customs.Business.ModelViewColumnQueryHelper<JobDeclaration> ModelViewColumnHelper
		{
			get
			{
				return modelViewColumnHelper ?? (modelViewColumnHelper = new Customs.Business.ModelViewColumnQueryHelper<JobDeclaration>());
			}
		}
		Customs.Business.ModelViewColumnQueryHelper<JobDeclaration> modelViewColumnHelper;

		Customs.Business.ModelViewColumnQueryHelper<CusEntryHeader> EntryHeaderModelViewHelper
		{
			get
			{
				return entryHeaderModelViewHelper ?? (entryHeaderModelViewHelper = new Customs.Business.ModelViewColumnQueryHelper<CusEntryHeader>());
			}
		}
		Customs.Business.ModelViewColumnQueryHelper<CusEntryHeader> entryHeaderModelViewHelper;

		const string ModelView = "USReconJobDeclaration";
		const string ModelViewPK = "JE_PK";
		const string ModelView_CusEntryHeader = "USCusEntryHeader";
		const string ModelViewPK_CusEntryHeader = "CH_PK";

		ReconDeclarationFilterLookups lookups;
		ReconDeclarationFilterLookups Lookups => lookups ?? (lookups = new ReconDeclarationFilterLookups(Factory));
	}
}
