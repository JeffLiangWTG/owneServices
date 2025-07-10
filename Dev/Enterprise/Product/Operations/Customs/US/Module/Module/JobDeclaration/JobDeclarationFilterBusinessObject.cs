using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.Module;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.US.Business.JobDeclaration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	public class JobDeclarationFilterBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject, Integration.Customs.US.IJobDeclarationFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();

			AddAndResetMessageStatusFilters(result);
			AddDateFilters(result);
			AddAdditionalOrganisationFilters(result);
			AddITFilters(result);
			AddStatementFilters(result);
			AddLocationsFilter(result);
			AddMiscFilters(result);

			result.AddGuidFilter(DeclarationFilterConstants.ImporterOfRecord, ModuleIDs.Organisation, GetImporterOfRecordQuery, Lookups.Consignees);

			AddUSAuditFilters(result);

			return result;
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		protected override void AddDuplicateDefaultFilterStripCollection(FilterStripCollection collection)
		{
			var parent = ParentModule as JobDeclarationModule;
			if (parent != null)
			{
				var moduleType = parent.ModuleDecisionProvider.GetType();
				if (moduleType == typeof(ReconBulkImportPopupOKButtonStrategy.ReconModuleDecisionProvider))
				{
					new DuplicateFilterStripForRecon(collection);
				}
				else
				{
					new DuplicateFilterStripForJobDeclaration(collection);
				}
			}
		}

		#region Locations Filters

		void AddLocationsFilter(ModuleFilterCollection filters)
		{
			var loadingSchedDK = filters.AddTextFilter(DeclarationFilterConstants.LoadingSchedDK, GetLoadingSchedDKQuery);
			loadingSchedDK.Category = FilterCategories.Locations;
			loadingSchedDK.MaxLength = USAddInfoSchema.US_SchDLoading.MaxLength;

			var dischargeSchedDK = filters.AddTextFilter(DeclarationFilterConstants.DischargeSchedDK, GetDischargeSchedDKQuery);
			dischargeSchedDK.Category = FilterCategories.Locations;
			dischargeSchedDK.MaxLength = USAddInfoSchema.US_SchDArrival.MaxLength;

			var entrySchedD = filters.AddNkFilter(DeclarationFilterConstants.PortOfEntry, GetPortOfEntryQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, Lookups.RegionalPorts);
			entrySchedD.Category = FilterCategories.Locations;
			entrySchedD.MaxLength = USAddInfoSchema.US_SchDEntry.MaxLength;

			var exportPort = filters.AddNkFilter(DeclarationFilterConstants.ExportPort, GetExportPortQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, Lookups.RegionalPorts);
			exportPort.Category = FilterCategories.Locations;
			exportPort.MaxLength = USAddInfoSchema.US_SchDExport.MaxLength;

			var preparerSchedDK = filters.AddNkFilter(DeclarationFilterConstants.PreparerDistrictPort, GetPreparerDistrictPortQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, Lookups.RegionalPorts);
			preparerSchedDK.Category = FilterCategories.Locations;
			preparerSchedDK.MaxLength = USAddInfoSchema.US_PreparerDistrictPort.MaxLength;
		}

		ZQuery GetLoadingSchedDKQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_SchDLoading", filterOperator, value);
		}

		ZQuery GetDischargeSchedDKQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_SchDArrival", filterOperator, value);
		}

		ZQuery GetExportPortQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_SchDExport", filterOperator, value);
		}

		#endregion

		#region Audit Filters

		protected void AddUSAuditFilters(ModuleFilterCollection filters)
		{
			var spiAuditFilter = filters.AddFlagsFilter(DeclarationFilterConstants.SPINotApplicableAudit, new string[] { DeclarationFilterConstants.Audited, DeclarationFilterConstants.AuditRequired }, new GetFlagsQuery[] { GetSPIAuditedQuery, GetSPIAuditRequiredQuery });
			spiAuditFilter.Category = FilterCategories.AuditInformation;
			spiAuditFilter.ArePropertiesMutuallyExclusive = true;
			spiAuditFilter.Property1 = true;

			var spiAuditDateFilter = filters.AddDateFilter(DeclarationFilterConstants.SPINotApplicableAuditDate, GetSPIAuditDateQuery);
			spiAuditDateFilter.Category = FilterCategories.AuditInformation;

			var fdaAuditFilter = filters.AddFlagsFilter(DeclarationFilterConstants.FDANotApplicableAudit, new string[] { DeclarationFilterConstants.Audited, DeclarationFilterConstants.AuditRequired }, new GetFlagsQuery[] { GetFDAAuditedQuery, GetFDAAuditRequiredQuery });
			fdaAuditFilter.Category = FilterCategories.AuditInformation;
			fdaAuditFilter.ArePropertiesMutuallyExclusive = true;
			fdaAuditFilter.Property1 = true;

			var fdaAuditDateFilter = filters.AddDateFilter(DeclarationFilterConstants.FDANotApplicableAuditDate, GetFDAAuditDateQuery);
			fdaAuditDateFilter.Category = FilterCategories.AuditInformation;

			var cwoAuditFilter = filters.AddFlagsFilter(DeclarationFilterConstants.CWNotAuditedAudit, new string[] { DeclarationFilterConstants.Audited, DeclarationFilterConstants.AuditRequired }, new GetFlagsQuery[] { GetCWOAuditedQuery, GetCWOAuditRequiredQuery });
			cwoAuditFilter.Category = FilterCategories.AuditInformation;
			cwoAuditFilter.ArePropertiesMutuallyExclusive = true;
			cwoAuditFilter.Property1 = true;

			var cwoAuditDateFilter = filters.AddDateFilter(DeclarationFilterConstants.CWOAuditDate, GetCWOAuditDateQuery);
			cwoAuditDateFilter.Category = FilterCategories.AuditInformation;

			var tIBClosedFilter = filters.AddFlagsFilter(DeclarationFilterConstants.TIBClosed, new string[] { DeclarationFilterConstants.Closed, DeclarationFilterConstants.ClosingRequired }, new GetFlagsQuery[] { GetTIBClosedQuery, GetTIBClosingRequiredQuery });
			tIBClosedFilter.Category = FilterCategories.AuditInformation;
			tIBClosedFilter.ArePropertiesMutuallyExclusive = true;
			tIBClosedFilter.Property1 = true;

			var tIBClosedDateFilter = filters.AddDateFilter(DeclarationFilterConstants.TIBClosedDate, GetTIBClosedDateQuery);
			tIBClosedDateFilter.Category = FilterCategories.AuditInformation;
		}

		ZQuery GetAuditDateQuery(DateComparisonOperator sqlOperator, ZDateTime dateFrom, ZDateTime dateTo, string auditField, string[] messageTypes)
		{
			return GetLoggingDateQuery(sqlOperator, dateFrom, dateTo, auditField, messageTypes, Events.RecordAudited.Code);
		}

		ZQuery GetLoggingDateQuery(DateComparisonOperator sqlOperator, ZDateTime dateFrom, ZDateTime dateTo, string auditField, string[] messageTypes, string eventCode)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddToFilter(JobDeclarationSchema.JE_MessageType, messageTypes);

			var auditQuery = GetLogSubQuery(auditField, sqlOperator != DateComparisonOperator.HasNoDateEntered, eventCode);
			if (!dateFrom.Date.IsEmpty || !dateTo.Date.IsEmpty)
			{
				AddDateRange(auditQuery, sqlOperator, JoinCondition.And, StmALogSchema.SL_EventTime, dateFrom.Date, dateTo.Date);
			}
			result.AddSubQuery(auditQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetAuditedQuery(ZBool show, string auditField, string[] messageTypes, string value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			if (show)
			{
				result.AddToFilter(JobDeclarationSchema.JE_MessageType, messageTypes);

				result.AddSubQuery(GetLogSubQuery(auditField, true, Events.RecordAudited.Code), JoinCondition.And);
				result.AddSubQuery(GetInvoiceLineSubQueryToDeclaration(JobComInvoiceLineSchema.JI_AddInfo, SQLComparisonOperator.Contains, value), JoinCondition.And);
			}
			return result;
		}

		ZQuery GetAuditRequiredQuery(ZBool show, string auditField, string[] messageTypes, string value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			if (show)
			{
				result.AddToFilter(JobDeclarationSchema.JE_MessageType, messageTypes);

				result.AddSubQuery(GetLogSubQuery(auditField, false, Events.RecordAudited.Code), JoinCondition.And);
				result.AddSubQuery(GetStatusQueryAgainstEntry(SQLComparisonOperator.NotEqual, ImportMessageStatusList.Codes.ClearEntrySummaryDelete, new string[] { CusEntryHeaderMessageTypeList.Codes.EntrySummary }), JoinCondition.And);
				result.AddSubQuery(GetInvoiceLineSubQueryToDeclaration(JobComInvoiceLineSchema.JI_AddInfo, SQLComparisonOperator.Contains, value), JoinCondition.And);
			}
			return result;
		}

		ZDBOnlySubQuery GetInvoiceLineSubQueryToDeclaration(SchemaColumn invoiceLineColumn, SQLComparisonOperator sqlOperator, object value)
		{
			var result = new ZDBOnlySubQuery(typeof(JobComInvoiceLine), JobComInvoiceLineSchema.JI_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
			result.AddToFilter(invoiceLineColumn, sqlOperator, value);
			return result;
		}

		ZDBOnlySubQuery GetLogSubQuery(string auditField, bool showAudited, string eventCode)
		{
			var result = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent, !showAudited);

			result.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventCode);
			result.AddToFilter(StmALogSchema.SL_IsCancelled, ZBool.False);
			result.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger.PrefixIndicator + auditField);

			return result;
		}

		#region SPI

		ZQuery GetSPIAuditDateQuery(DateComparisonOperator sqlOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			return GetAuditDateQuery(sqlOperator, dateFrom, dateTo, Customs.US.Business.AuditFieldsList.Codes.SPI, new string[] { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.Recon });
		}

		ZQuery GetSPIAuditedQuery(ZBool show)
		{
			return GetAuditedQuery(show, Customs.US.Business.AuditFieldsList.Codes.SPI, new string[] { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.Recon }, USAddInfoSchema.Constants.US_SPI.Substring(3) + SPINotApplicableValue);
		}

		ZQuery GetSPIAuditRequiredQuery(ZBool show)
		{
			return GetAuditRequiredQuery(show, Customs.US.Business.AuditFieldsList.Codes.SPI, new string[] { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.Recon }, USAddInfoSchema.Constants.US_SPI.Substring(3) + SPINotApplicableValue);
		}
		const string SPINotApplicableValue = "=N/A";

		#endregion

		#region FDA

		ZQuery GetFDAAuditDateQuery(DateComparisonOperator sqlOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			return GetAuditDateQuery(sqlOperator, dateFrom, dateTo, Customs.US.Business.AuditFieldsList.Codes.FDA, new string[] { JobMessageTypeList.Codes.Import });
		}

		ZQuery GetFDAAuditedQuery(ZBool show)
		{
			return GetAuditedQuery(show, Customs.US.Business.AuditFieldsList.Codes.FDA, new string[] { JobMessageTypeList.Codes.Import }, USAddInfoSchema.Constants.US_FDAIndicator.Substring(3) + FDADisclaimedValue);
		}

		ZQuery GetFDAAuditRequiredQuery(ZBool show)
		{
			return GetAuditRequiredQuery(show, Customs.US.Business.AuditFieldsList.Codes.FDA, new string[] { JobMessageTypeList.Codes.Import }, USAddInfoSchema.Constants.US_FDAIndicator.Substring(3) + FDADisclaimedValue);
		}
		const string FDADisclaimedValue = "=C";

		#endregion

		#region TIB

		ZQuery GetTIBClosedQuery(ZBool show)
		{
			if (show)
			{
				return GetStatusUpdateLogQuery(true);
			}
			return new ZQuery();
		}

		ZQuery GetTIBClosingRequiredQuery(ZBool show)
		{
			if (show)
			{
				return GetStatusUpdateLogQuery(false);
			}
			return new ZQuery();
		}

		ZQuery GetTIBClosedDateQuery(DateComparisonOperator sqlOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			return GetLoggingDateQuery(sqlOperator, dateFrom, dateTo, Customs.US.Business.AuditFieldsList.Codes.TIB, new string[] { JobMessageTypeList.Codes.Import }, Events.StatusUpdated.Code);
		}

		ZDBOnlyQuery GetStatusUpdateLogQuery(bool showClosed)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddToFilter(ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_EntryType", SQLComparisonOperator.Equal, "23"));
			result.AddSubQuery(GetLogSubQuery(AuditFieldsList.Codes.TIB, showClosed, Events.StatusUpdated.Code), JoinCondition.And);
			return result;
		}

		#endregion

		#region Census Warning Override

		ZQuery GetCWOAuditedQuery(ZBool show)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			if (show)
			{
				result.AddToFilter(JobDeclarationSchema.JE_MessageType, new ZString[] { JobMessageTypeList.Codes.Import });
				result.AddSubQuery(GetLogSubQuery(AuditFieldsList.Codes.CensusWarning, true, Events.RecordAudited.Code), JoinCondition.And);
			}

			return result;
		}

		ZQuery GetCWOAuditRequiredQuery(ZBool show)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			if (show)
			{
				result.AddToFilter(JobDeclarationSchema.JE_MessageType, new ZString[] { JobMessageTypeList.Codes.Import });
				result.AddSubQuery(GetLogSubQuery(AuditFieldsList.Codes.CensusWarning, false, Events.RecordAudited.Code), JoinCondition.And);

				var entryQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
				entryQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
				entryQuery.AddToFilter(CusEntryHeaderSchema.CH_Status, new ZString[] { ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings, ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings });

				result.AddSubQuery(entryQuery, JoinCondition.And);
			}

			return result;
		}

		ZQuery GetCWOAuditDateQuery(DateComparisonOperator sqlOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			return GetAuditDateQuery(sqlOperator, dateFrom, dateTo, AuditFieldsList.Codes.CensusWarning, new string[] { JobMessageTypeList.Codes.Import });
		}

		#endregion

		#endregion

		#region MessageStatusFilters

		void AddAndResetMessageStatusFilters(ModuleFilterCollection filters)
		{
			var releaseStatusFilter = filters.AddTextFilter(ReleaseStatusDescription, GetReleaseStatusQuery, ReleaseStatusList);
			SetFilterConstraints(releaseStatusFilter);
			releaseStatusFilter.MaxLength = CusEntryHeaderSchema.CH_Status.MaxLength;

			var cargoReleaseStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.CargoReleaseStatus, GetCargoReleaseStatusQuery, Lookups.MessageStatusListForCRL);
			SetFilterConstraints(cargoReleaseStatusFilter);
			cargoReleaseStatusFilter.MaxLength = CusEntryHeaderSchema.CH_Status.MaxLength;

			var seBillDispositionFilter = filters.AddTextFilter(DeclarationFilterConstants.SimplifiedEntryBillStatus, GetSEBillStatusQuery, Lookups.SEBillStatusList);
			SetFilterConstraints(seBillDispositionFilter);
			seBillDispositionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			seBillDispositionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			seBillDispositionFilter.MaxLength = AutoUSDispositionDataAddInfo.Schema.US_CodeMaxLength;

			var billHoldOrExamFilter = filters.AddTextFilter(DeclarationFilterConstants.BillHoldOrExam, GetBillHoldOrExamStatusQuery, Lookups.HLDOrEXMStatusList);
			SetFilterConstraints(billHoldOrExamFilter);
			billHoldOrExamFilter.DefaultProperty = DeclarationFilterConstants.ALL;

			var entrySummaryStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.EntrySummaryStatus, GetEntrySummaryStatusQuery, Lookups.MessageStatusListForENS);
			SetFilterConstraints(entrySummaryStatusFilter);
			entrySummaryStatusFilter.MaxLength = CusEntryHeaderSchema.CH_Status.MaxLength;

			var exportStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.ExportStatus, GetExportStatusQuery, Lookups.MessageStatus_Export_List);
			SetFilterConstraints(exportStatusFilter);
			exportStatusFilter.MaxLength = CusEntryHeaderSchema.CH_Status.MaxLength;

			var electronicInvoiceStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.ElectronicInvoiceStatus, GetElectronicInvoiceStatusQuery, Lookups.ElectronicInvoiceStatusListForFilter);
			SetFilterConstraints(electronicInvoiceStatusFilter);
			electronicInvoiceStatusFilter.MaxLength = JobComInvoiceHeaderSchema.JZ_MessageStatus.MaxLength;

			var electronicInvoiceRequestedFilter = filters.AddFlagsFilter(DeclarationFilterConstants.ElectronicInvoiceRequested, new string[] { DeclarationFilterConstants.ElectronicInvoiceRequested }, new GetFlagsQuery[] { GetElectronicInvoiceRequestedQuery });
			electronicInvoiceRequestedFilter.Category = FilterCategories.StatusAndFlags;
			electronicInvoiceRequestedFilter.Property0 = true;

			var bluMessageStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.BLUMessageStatus, GetBLUStatusQuery, Lookups.MessageStatusListForBLU);
			SetFilterConstraints(bluMessageStatusFilter);
			bluMessageStatusFilter.MaxLength = GenAddOnColumnMaxLength.BLUStatus;

			var fdaMsgStatusFilter = filters.AddTextFilter(FDAMsgStatusDescription, GetFDAMsgStatusQuery, FDAMsgStatusList);
			SetFilterConstraints(fdaMsgStatusFilter);
			fdaMsgStatusFilter.MaxLength = GenAddOnColumnMaxLength.FDAMsgStatus;

			var fdaStatusFilter = filters.AddTextFilter(FDAStatusDescription, GetFDAStatusQuery, FDAStatusList);
			SetFilterConstraints(fdaStatusFilter);
			fdaStatusFilter.MaxLength = GenAddOnColumnMaxLength.FDAStatus;

			var entryStatus = (EntryStatusFilter)filters[EntryStatusText];
			if (entryStatus != null)
			{
				entryStatus.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			}

			var eBondMessageStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.EBondMessageStatus, GetInsuranceDisposition, Lookups.InsuranceDispositionCodeList);
			eBondMessageStatusFilter.Category = FilterCategories.StatusAndFlags;
			eBondMessageStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			eBondMessageStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			eBondMessageStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			eBondMessageStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			eBondMessageStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			eBondMessageStatusFilter.MaxLength = USAddInfoSchema.US_InsuranceDisposition.MaxLength;

			var paperlessFilter = filters.AddTextFilter(DeclarationFilterConstants.Paperless, GetPaperlessQuery, Lookups.YesNoList);
			paperlessFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			SetFilterConstraints(paperlessFilter);
			paperlessFilter.MaxLength = USAddInfoSchema.US_PaperlessEntry.MaxLength;

			var statusNotificationDispositionCodeFilter = filters.AddTextFilter(DeclarationFilterConstants.StatusNotificationDispositionCode, GetStatusNotificationDispositionCodeQuery, Lookups.StatusDispositionList);
			SetFilterConstraints(statusNotificationDispositionCodeFilter);
			statusNotificationDispositionCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			statusNotificationDispositionCodeFilter.MaxLength = GenAddOnColumnMaxLength.StatusNotificationDispositionCode;
			statusNotificationDispositionCodeFilter.MultiValueQueryDelegate = GetStatusNotificationDispositionCodeMultiValueQuery;

			var entrySummaryActionsFilter = filters.AddTextFilter(DeclarationFilterConstants.EntrySummaryActions, GetEntrySummaryActionsQuery, Lookups.EntrySummaryActionsList);
			SetFilterConstraints(entrySummaryActionsFilter);
			entrySummaryActionsFilter.Category = FilterCategories.StatusAndFlags;
			entrySummaryActionsFilter.DefaultProperty = DeclarationFilterConstants.ALL;

			var cargReleaseComments = filters.AddTextFilter(DeclarationFilterConstants.CargoReleaseComments, GetCargoReleaseCommentsActionsQuery, Lookups.EntrySummaryActionsList);
			cargReleaseComments.Category = FilterCategories.StatusAndFlags;
			cargReleaseComments.DefaultProperty = DeclarationFilterConstants.ALL;

			var pscFilter = filters.AddFlagsFilter(DeclarationFilterConstants.PSCIndicator, new string[] { DeclarationFilterConstants.Show }, new GetFlagsQuery[] { GetPSCQuery });
			pscFilter.Property0 = true;
			pscFilter.Category = FilterCategories.StatusAndFlags;

			var pgaReplaceUpdateFilter = filters.AddFlagsFilter(DeclarationFilterConstants.PGAReplaceUpdateNeeded, new string[] { DeclarationFilterConstants.Show }, new GetFlagsQuery[] { GetPGACorrectionRequiredQuery });
			pgaReplaceUpdateFilter.Property0 = true;
			pgaReplaceUpdateFilter.Category = FilterCategories.StatusAndFlags;

			var pgaExpeditedReleasFilter = filters.AddFlagsFilter(DeclarationFilterConstants.PGAExpeditedRelease, new string[] { DeclarationFilterConstants.PGAExpeditedRelease }, new GetFlagsQuery[] { GetPGAExpeditedReleaseQuery });
			pgaExpeditedReleasFilter.Property0 = true;
			pgaExpeditedReleasFilter.Category = FilterCategories.StatusAndFlags;

			var pgaCorrectionStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.PGACorrectionStatus, GetPGACorrectionStatusQuery, Factory.GetCachedValue<PGACorrectionStatusList>());
			pgaCorrectionStatusFilter.Category = FilterCategories.StatusAndFlags;
			pgaCorrectionStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			pgaCorrectionStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			pgaCorrectionStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			pgaCorrectionStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			pgaCorrectionStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			pgaCorrectionStatusFilter.MaxLength = GenAddOnColumnMaxLength.US_PGACorrectionStatus;

			var quotaStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.QuotaStatus, GetQuotaStatusQuery, CargoReleaseProcessingResultList.GetListForQuotaStatus(Factory));
			quotaStatusFilter.Category = FilterCategories.StatusAndFlags;
			quotaStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			quotaStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			quotaStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			quotaStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			quotaStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			quotaStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			quotaStatusFilter.MaxLength = GenAddOnColumnMaxLength.US_QuotaStatus;

			var censusWarningsOverriden = filters.AddFlagsFilter(DeclarationFilterConstants.CensusWarningsOverriden, new string[] { DeclarationFilterConstants.Show }, new GetFlagsQuery[] { GetCensusWarningOverrideQuery });
			censusWarningsOverriden.Property0 = true;
			censusWarningsOverriden.Category = FilterCategories.StatusAndFlags;

			var isfBillStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.ISFBillStatus, GetISFBillStatusQuery, Lookups.ISFBillStatusList);
			isfBillStatusFilter.Category = FilterCategories.StatusAndFlags;
			isfBillStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			isfBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			isfBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			isfBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			isfBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			isfBillStatusFilter.MaxLength = CusISFBillSchema.BB_CustomsStatus.MaxLength;

			var ftzAdmissionStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.FTZAdmissionStatus, GetFTZAdmissionStatusQuery, Lookups.FTZAdmissionStatusList);
			SetFilterConstraints(ftzAdmissionStatusFilter);
			ftzAdmissionStatusFilter.MaxLength = GenAddOnColumnMaxLength.FTZAdmissionStatus;

			var ftzConcurrenceStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.FTZConcurrenceStatus, GetFTZConcurrenceStatusQuery, Lookups.FTZConcurrenceStatusList);
			SetFilterConstraints(ftzConcurrenceStatusFilter);
			ftzConcurrenceStatusFilter.MaxLength = GenAddOnColumnMaxLength.FTZConcurrenceStatus;

			var ftzDeliveryOfGoodsStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.FTZDeliveryOfGoodsStatus, GetFTZDeliveryOfGoodsStatusQuery, Lookups.FTZDeliveryOfGoodsStatusList);
			SetFilterConstraints(ftzDeliveryOfGoodsStatusFilter);
			ftzDeliveryOfGoodsStatusFilter.MaxLength = GenAddOnColumnMaxLength.FTZDeliveryOfGoodsStatus;

			var ftzGoodsArrivalStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.FTZGoodsArrivalStatus, GetFTZGoodsArrivalStatusQuery, Lookups.FTZGoodsArrivalStatusList);
			SetFilterConstraints(ftzGoodsArrivalStatusFilter);
			ftzGoodsArrivalStatusFilter.MaxLength = GenAddOnColumnMaxLength.FTZArrivalStatus;

			var ftzPTTStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.FTZPTTStatus, GetFTZPTTStatusQuery, Lookups.FTZPTTStatusList);
			SetFilterConstraints(ftzPTTStatusFilter);
			ftzPTTStatusFilter.MaxLength = GenAddOnColumnMaxLength.FTZPTTStatus;

			var splitShipmentFilter = filters.AddFlagsFilter(DeclarationFilterConstants.ContainsSplitShipments, new string[] { DeclarationFilterConstants.ContainsSplitShipments }, new GetFlagsQuery[] { GetSplitShipmentQuery });
			splitShipmentFilter.Category = FilterCategories.StatusAndFlags;
			splitShipmentFilter.Property0 = true;

			var disStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.DISStatus, GetDISStatusQuery, Lookups.DISStatusList);
			SetFilterConstraints(disStatusFilter);
			disStatusFilter.MaxLength = JobRequiredDocumentAddInfoSchema.EX_Status.MaxLength;

			var spiInvoiceLineFilter = filters.AddTextFilter(DeclarationFilterConstants.SPIInvLine, GetSPIInvLineFilterQuery, Lookups.SPIList);
			SetFilterConstraints(spiInvoiceLineFilter);
			spiInvoiceLineFilter.MaxLength = USAddInfoSchema.US_SPI.MaxLength;

			var pgaStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.PGAStatus, GetPGAStatusQuery, Lookups.PGAStatus);
			SetFilterConstraints(pgaStatusFilter);

			var aesFilterOperatorCodes = new []
			{
				ModuleTextFilter.ComparisonConstants.Exact,
				ModuleTextFilter.ComparisonConstants.NotEqual,
				ModuleTextFilter.ComparisonConstants.IsBlank,
				ModuleTextFilter.ComparisonConstants.IsNotBlank
			};
			var aesSeverityFilter = filters.AddTextFilter(DeclarationFilterConstants.AESSeverity, GetAESDispositionViaSeverityQuery,
				Lookups.AESSeverityList);
			aesSeverityFilter.Category = FilterCategories.StatusAndFlags;
			SetFilterOperators(aesSeverityFilter.ComparisonOperator_List, aesFilterOperatorCodes);

			var aesResponseCodeFilter = filters.AddNkFilter(DeclarationFilterConstants.AESResponseCode, GetAESDispositionViaResponseCodeQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, Lookups.AESResponseCodeList);
			aesResponseCodeFilter.Category = FilterCategories.StatusAndFlags;
		}

		ZQuery GetAESDispositionViaResponseCodeQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetAESDispositionQuery(filterOperator, value, CusDispositionSchema.CDI_Status);
		}

		ZQuery GetAESDispositionViaSeverityQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetAESDispositionQuery(filterOperator, value, CusDispositionSchema.CDI_StatusKey);
		}

		ZQuery GetAESDispositionQuery(SQLComparisonOperator filterOperator, ZString value, SchemaStringColumn column)
		{
			var inOrNotIn = filterOperator == SQLComparisonOperator.Equal || filterOperator == SQLComparisonOperator.IsNotBlank ? "IN" : "NOT IN";
			var additionalColumns = ZString.Empty;
			var extraCondition = ZString.Empty;
			var parameterCollection = new ZSqlParameterCollection();
			if (filterOperator == SQLComparisonOperator.Equal || filterOperator == SQLComparisonOperator.NotEqual)
			{
				additionalColumns = @", CDI_StatusKey, CDI_Status, ROW_NUMBER() OVER (PARTITION BY CDI_ParentID ORDER BY CDI_SystemCreateTimeUtc DESC) AS RowNumber";
				extraCondition = $" WHERE AESDisposition.RowNumber = 1 AND {column.Name} = @Value";
				parameterCollection.Add(ZSqlParameter.New("@Value", value, column));
			}

			var queryText = @$"
JE_PK {inOrNotIn}
(
	SELECT CH_JE
	FROM
	(
		SELECT CH_JE" + additionalColumns + @$"
		FROM dbo.CusDisposition
		JOIN dbo.CusEntryHeader ON CDI_ParentID = CH_PK
		WHERE CDI_ParentTableCode = '{CusEntryHeaderSchema.Constants.Prefix}' AND CDI_Type = '{Customs.Business.CusDispositionTypeCodeList.Codes.USAESEntryStatus}'
	) AS AESDisposition" + extraCondition + ")";

			return new ZDBOnlyQuery(typeof(JobDeclaration)).AddFilterAndZSQLParameterCollection(queryText, parameterCollection);
		}

		void SetFilterOperators(CodeDescriptionPairList operatorList,  string[] operatorCodes)
		{
			operatorList.GetAllCodes().Where(x => !operatorCodes.Contains(x)).ForEach(operatorList.RemoveCode);
		}

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
		protected override void AddMessageStatusFilter(ModuleFilterCollection filters)
		{
			return;
		}

		ZQuery GetPGAStatusQuery(ZString value)
		{
			var result = new ZQuery();
			{
				var queryText = string.Format(CultureInfo.InvariantCulture, @"
				JE_PK IN
				(
					{0}
				) AND (JE_AddInfo like '%CargoReleaseType=SE%' OR JE_AddInfo like '%CargoReleaseType=ACE%')", GetPGAStatusSqlScript(value));
				var pgastatusQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				pgastatusQuery.AddFilterAndZSQLParameterCollection(queryText, new ZSqlParameterCollection());
				result.AddToFilter(pgastatusQuery);
			}
			result.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_ApplicationCode, JobApplicationCodeList.Codes.ACE);
			return result;
		}

		ZString GetPGAStatusSqlScript(ZString value)
		{
			var sqlScript = ZString.Format(@"
	SELECT JE_PK 
	FROM dbo.JobDeclaration
	LEFT JOIN 
	(
		SELECT CDI_ParentID, MAX(CDI_Status) AS MaxStatus, MIN(CDI_Status) AS MinStatus, MAX(CASE WHEN CDI_Status IN ('02') THEN 'Y' ELSE '' END) AS HoldIntact
		FROM dbo.CusDisposition
		GROUP BY CDI_ParentID
	) AS PGADispositions ON PGADispositions.CDI_ParentID = JE_PK

	WHERE ('{0}' = 'All May Proceed/Manually Closed' AND PGADispositions.MinStatus in ('07', 'MC'))
			OR ('{0}' = 'Has Any PGA Status' AND PGADispositions.CDI_ParentID is not null)
			OR ('{0}' = 'Has No PGA Status' AND PGADispositions.CDI_ParentID is null)
			OR ('{0}' = 'Has Any Manually Closed' AND PGADispositions.MaxStatus  = 'MC')
			OR ('{0}' = 'May NOT Proceed' AND PGADispositions.MinStatus  in ('01', '02'))
			OR ('{0}' = 'Hold Intact' AND ISNULL(PGADispositions.HoldIntact, '') = 'Y')
", value);

			return sqlScript;
		}

		public static ZQuery GetDISStatusQuery(ZString value)
		{
			var query = new ZQuery();

			if (!value.IsEmpty)
			{
				var result = new ZDBOnlyQuery(typeof(JobDeclaration));

				var rquiredDocumentAddInfoQuery = new ZDBOnlySubQuery(typeof(JobRequiredDocumentAddInfo), JobRequiredDocumentAddInfoSchema.EX_EQ_RequiredDocument);
				rquiredDocumentAddInfoQuery.AddToFilter(JobRequiredDocumentAddInfoSchema.EX_ApplicationCode, Core.Constants.Customs.DocumentImageSystemIDs.US_DIS);
				rquiredDocumentAddInfoQuery.AddToFilter(JobRequiredDocumentAddInfoSchema.EX_Status, value);

				var rquiredDocumentQuery = new ZDBOnlySubQuery(typeof(JobRequiredDocument), JobRequiredDocumentSchema.EQ_ParentID);
				rquiredDocumentQuery.AddSubQuery(rquiredDocumentAddInfoQuery, JoinCondition.And);

				var docsAndCartageQuery = new ZDBOnlySubQuery(typeof(Freight.Business.JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
				docsAndCartageQuery.AddSubQuery(rquiredDocumentQuery, JoinCondition.And);

				result.AddSubQuery(docsAndCartageQuery, JoinCondition.And);
				result.AddSubQuery(JobDeclarationSchema.JE_JS, docsAndCartageQuery, JoinCondition.Or);

				query = result;
			}

			return query;
		}

		ZQuery GetSplitShipmentQuery(ZBool show)
		{
			var query = new ZQuery();
			if (show)
			{
				var result = new ZDBOnlyQuery(typeof(JobDeclaration));
				result.AddToFilter(JobDeclarationSchema.JE_ApplicationCode, JobApplicationCodeList.Codes.ACE);

				var billQuery = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.CU_JE);
				billQuery.AddToFilter(CusDecHouseBillSchema.CU_AddInfo, SQLComparisonOperator.Like, "%SESplitShip=Y%");

				result.AddSubQuery(billQuery, JoinCondition.And);
				return result;
			}
			return query;
		}

		ZQuery GetCensusWarningOverrideQuery(ZBool value)
		{
			ZQuery result = new ZDBOnlyQuery(typeof(JobDeclaration));
			if (value)
			{
				ZString queryText = @"EXISTS
				(
					SELECT JZ_PK
					FROM dbo.JobComInvoiceHeader 
					LEFT JOIN dbo.JobComInvoiceLine  ON JZ_PK = JI_JZ
					LEFT JOIN dbo.CusCodeData  ON CY_ParentID = JI_PK
					WHERE CY_ParentTableCode = @JobComInvoiceLineTableCode AND CY_Type = @Type AND JZ_JE = JE_PK AND ISNULL(CY_Data, '') != ''
				)";
				var zsqlParams = new ZSqlParameterCollection(
					ZSqlParameter.New("@JobComInvoiceLineTableCode", JobComInvoiceLineSchema.Constants.Prefix, CusCodeDataSchema.CY_ParentTableCode),
					ZSqlParameter.New("@Type", CusCodeDataTypeList.Codes.CensusWarningOverride, CusCodeDataSchema.CY_Type));
				result.AddFilterAndZSQLParameterCollection(queryText, zsqlParams);
			}
			return result;
		}

		ZQuery GetPSCQuery(ZBool value)
		{
			if (value)
			{ return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_PSC", SQLComparisonOperator.Equal, true); }
			else
			{ return new ZQuery(); }
		}

		ZQuery GetPGACorrectionRequiredQuery(ZBool value)
		{
			var result = new ZQuery();
			if (value)
			{
				result = SimpleQueryHelper.GetQueryOnGenAddOnColumn(JobDeclaration.Constants.GenAddOnColumnFieldName.US_PGAReplaceUpdateNeeded, "Y", false);
			}

			return result;
		}

		ZQuery GetPGAExpeditedReleaseQuery(ZBool value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			var sqlOperator = value ? SQLComparisonOperator.Contains : SQLComparisonOperator.NotContains;
			result.AddToFilter(JobDeclarationSchema.JE_AddInfo, sqlOperator, USAddInfoSchema.Constants.US_PGAExpeditedRelease.Substring(3) + "=Y");
			return result;
		}

		ZQuery GetPGACorrectionStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return SimpleQueryHelper.GetQueryHandlingBlanks(JobDeclaration.Constants.GenAddOnColumnFieldName.US_PGACorrectionStatus, filterOperator, value);
		}

		ZQuery GetQuotaStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return SimpleQueryHelper.GetQueryHandlingBlanks(JobDeclaration.Constants.GenAddOnColumnFieldName.US_QuotaStatus, filterOperator, value);
		}

		protected override void AddEntryStatusFilter(ModuleFilterCollection filters)
		{
			var entryStatusFilter = new EntryStatusFilter(DeclarationFilterConstants.EntryStatusText,
				(ZString status) => GetEntryStatusQuery(SQLComparisonOperator.NotSpecified,  status), Lookups.EntryStatusList, false, false).WithMaxLengthOf<EntryStatusFilter>(JobDeclarationSchema.JE_EntryStatus);

			filters.AddFilter(entryStatusFilter);

			entryStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			entryStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			entryStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			entryStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			entryStatusFilter.Category = FilterCategories.StatusAndFlags;
			entryStatusFilter.MultilingualDescription = EntryStatusText;
			entryStatusFilter.ComparisonOperatorChanged += CustomsEntryStatusFilter_ComparisonOperatorChanged;
		}

		protected override ZQuery GetEntryStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery();
		}

		protected void SetFilterConstraints(ModuleTextFilter moduleFilter)
		{
			DBStatusQueryAndFilterConstraints.SetFilterConstraints(moduleFilter, FilterCategories.StatusAndFlags);
		}

		#region ISF Bill Status
		ZQuery GetISFBillStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var isMultiple = value == ISFStatusHelper.Multiple;
			var isNotEqualAndNotMultiple = filterOperator == SQLComparisonOperator.NotEqual && !isMultiple;
			var isNotMatching = filterOperator == SpecialComparisonOperator.IsBlank || filterOperator == SQLComparisonOperator.NotEqual;
			var isBlackTypeFilter = filterOperator == SpecialComparisonOperator.IsNotBlank || filterOperator == SpecialComparisonOperator.IsBlank;
			var recyclePeriod = ISFStatusHelper.TimeFrame6MonthsForSearching;
			var sqlFilter = string.Format(@"
{0} {20} IN (
	SELECT {0}
	FROM
	(
" + (isMultiple ? @"		SELECT CASE WHEN COUNT({0}) > 1 THEN @MultipleValue ELSE MAX({1}) END AS {1}, {0}
		FROM 
		(
" : "") + @"			SELECT " + (isMultiple ? @"DISTINCT CASE WHEN {22} IS NULL THEN '' ELSE {1} END AS" : "") + @" {1}, {0}, {22}
			FROM {3}
			INNER JOIN {4} ON {5} = {0} AND {23} = {24}
				AND {6} NOT IN (SELECT {7}
								FROM {4}
								WHERE {7} IS NOT NULL)
			LEFT JOIN {8} ON {9} = ((SELECT VALUE FROM dbo.csfn_GetAddInfoValueFromCodeInlineToReturnEmptyIfNull({10}, @BillIssuerSCAC)) + {11})
				AND {12} IN (@OceanBillType, @HouseBillType)
				AND {13} IN (SELECT {14}
							FROM {15}
							WHERE {16} >= DATEADD(MONTH, -{21}, {17})
							AND {16} <= DATEADD(MONTH, {21}, {17}))
			WHERE {18} IN ('IMP','IMX','FTZ','MSC') AND {19} = @SeaTransportMode
" + (isMultiple ? @"		) AS ISFBillStatus
		GROUP BY {0}
" : "") + @"	) AS ISFBillStatus
	WHERE ISFBillStatus.{1} {2} @CustomsStatus
)"
+ (isNotEqualAndNotMultiple ?
@"
OR
{0} IN (
	SELECT {0}
	FROM
	(
		SELECT {1}, {0}
		FROM {3}
		INNER JOIN {4} ON {5} = {0} AND {23} = {24}
			AND {6} NOT IN (SELECT {7}
							FROM {4}
							WHERE {7} IS NOT NULL)
		LEFT JOIN {8} ON {9} = ((SELECT VALUE FROM dbo.csfn_GetAddInfoValueFromCodeInlineToReturnEmptyIfNull({10}, @BillIssuerSCAC)) + {11})
			AND {12} IN (@OceanBillType, @HouseBillType)
			AND {13} IN (SELECT {14}
						FROM {15}
						WHERE {16} >= DATEADD(MONTH, -{21}, {17})
						AND {16} <= DATEADD(MONTH, {21}, {17}))
		WHERE {18} IN ('IMP','IMX','FTZ','MSC') AND {19} = @SeaTransportMode
	) AS ISFBillStatus
	WHERE ISFBillStatus.{1} <> @CustomsStatus
)" : "")
, JobDeclarationSchema.Constants.PK //{0}
	, CusISFBillSchema.Constants.BB_CustomsStatus // {1}
	, isBlackTypeFilter ? "<>" : "=" // {2}
	, JobDeclarationSchema.Constants.TableName // {3}
	, CusDecHouseBillSchema.Constants.TableName // {4}
	, CusDecHouseBillSchema.Constants.CU_JE // {5}
	, CusDecHouseBillSchema.Constants.PK // {6}
	, CusDecHouseBillSchema.Constants.CU_CU_ParentBill // {7}
	, CusISFBillSchema.Constants.TableName // {8}
	, CusISFBillSchema.Constants.BB_BillNum // {9}
	, CusDecHouseBillSchema.Constants.CU_AddInfo // {10}
	, CusDecHouseBillSchema.Constants.CU_BillNum // {11}
	, CusISFBillSchema.Constants.BB_BillType // {12}
	, CusISFBillSchema.Constants.BB_BF // {13}
	, CusISFHeaderSchema.Constants.PK // {14}
	, CusISFHeaderSchema.Constants.TableName // {15}
	, CusISFHeaderSchema.Constants.BF_SystemCreateTimeUtc // {16}
	, JobDeclarationSchema.Constants.JE_SystemCreateTimeUtc // {17}
	, JobDeclarationSchema.Constants.JE_MessageType // {18}
	, JobDeclarationSchema.Constants.JE_TransportMode // {19}
	, isNotMatching ? "NOT" : "" // {20}
	, recyclePeriod // {21}
	, CusISFBillSchema.Constants.PK // {22}
	, CusDecHouseBillSchema.Constants.CU_ClusterKey // {23}
	, JobDeclarationSchema.Constants.JE_ClusterKey // {24}
	); // Is part of SQL expression

			var sqlFilterParameters = new ZSqlParameterCollection(
		ZSqlParameter.New("@BillIssuerSCAC", "UI_NKBillIssuerSCAC", CusDecHouseBillSchema.CU_AddInfo),// AddInfo fields
		ZSqlParameter.New("@OceanBillType", BillTypeList.Codes.OceanBillOfLading, CusISFBillSchema.BB_BillType),
		ZSqlParameter.New("@HouseBillType", BillTypeList.Codes.HouseBillOfLading, CusISFBillSchema.BB_BillType),
		ZSqlParameter.New("@SeaTransportMode", Core.Constants.TransportModes.Sea, JobDeclarationSchema.JE_TransportMode),
		ZSqlParameter.New("@CustomsStatus", isBlackTypeFilter ? ZString.Empty : value, CusISFBillSchema.BB_CustomsStatus)
		);
			if (isMultiple)
			{
				sqlFilterParameters.Add(ZSqlParameter.New("@MultipleValue", ISFStatusHelper.Multiple, CusISFBillSchema.BB_CustomsStatus));
			}

			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddFilterAndZSQLParameterCollection(sqlFilter, sqlFilterParameters);
			return result;
		}

		#endregion

		ZQuery GetCargoReleaseStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Import);
			result.AddSubQuery(GetStatusQueryAgainstEntry(filterOperator, value, new string[] { CusEntryHeaderMessageTypeList.Codes.CargoRelease, CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease, CusEntryHeaderMessageTypeList.Codes.ACECargoRelease }), JoinCondition.And);
			return result;
		}

		ZQuery GetSEBillStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var provider = ObjectFactory.Get<Integration.Customs.US.IForwardingShipmentCustomsQueryProvider>();
			return provider.GetSimplifiedEntryBillStatusQuery(filterOperator, value);
		}

		ZQuery GetBillHoldOrExamStatusQuery(ZString value)
		{
			var provider = ObjectFactory.Get<Integration.Customs.US.IForwardingShipmentCustomsQueryProvider>();
			return provider.GetHoldExamBillStatusQuery(value);
		}

		ZQuery GetEntrySummaryStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Import);
			result.AddSubQuery(GetStatusQueryAgainstEntry(filterOperator, value, new string[] { CusEntryHeaderMessageTypeList.Codes.EntrySummary }), JoinCondition.And);
			return result;
		}

		ZQuery GetExportStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			if (value == AESDirectCustomsEntryStatus.Codes.MultipleEntriesStatus)
			{
				return GetMultipleExportStatusQuery(filterOperator, value);
			}
			else
			{
				var result = new ZDBOnlyQuery(typeof(JobDeclaration));
				result.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Export);
				result.AddSubQuery(GetStatusQueryAgainstEntry(filterOperator, value, new string[] { CusEntryHeaderMessageTypeList.Codes.Export }), JoinCondition.And);

				return result;
			}
		}

		ZQuery GetMultipleExportStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Export);

			var queryNotIn = filterOperator == SQLComparisonOperator.NotEqual;
			var multipleStatusQueryText = @" CH_MessageType = 'ITN' and CH_AddInfo not like '%IsDeactivated=Y%' group by CH_JE having count(distinct CH_Status) > 1"; // special case filter requires SQL statement

			var multipleStatusQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, queryNotIn);
			multipleStatusQuery.AddFilterAndZSQLParameterCollection(multipleStatusQueryText, new ZSqlParameterCollection());
			result.AddSubQuery(multipleStatusQuery, JoinCondition.And);

			return result;
		}

		/// <summary>
		/// This method is used for Entry Summary/Inbond/Export/NAFTA & Export queries.
		/// </summary>
		ZDBOnlySubQuery GetStatusQueryAgainstEntry(SQLComparisonOperator filterOperator, ZString value, string[] cH_MessageTypes)
		{
			return DBStatusQueryAndFilterConstraints.GetStatusQueryAgainstEntry(filterOperator, value, cH_MessageTypes);
		}

		ZQuery GetElectronicInvoiceStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			if (value == MessageStatusListEI.Codes.Multiple)
			{
				return GetMultipleElectronicInvoiceStatusQuery(filterOperator, value);
			}
			else
			{
				return GetStandardElectronicInvoiceStatusQuery(filterOperator, value);
			}
		}

		ZQuery GetMultipleElectronicInvoiceStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			var queryNotIn = filterOperator == SQLComparisonOperator.NotEqual;
			var multipleStatusQueryText = @"JZ_GroupInvoice = 0 GROUP BY JZ_JE HAVING COUNT(DISTINCT(JZ_MessageStatus)) > 1"; // special case filter requires SQL statement

			var multipleStatusQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE, queryNotIn);
			multipleStatusQuery.AddFilterAndZSQLParameterCollection(multipleStatusQueryText, new ZSqlParameterCollection());
			result.AddSubQuery(multipleStatusQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetStandardElectronicInvoiceStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			if (!value.IsEmpty)
			{
				result.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Import);
				var queryNotIn = filterOperator == SQLComparisonOperator.NotEqual;
				var msgNotSentSearch = false;

				if (value == DeclarationFilterConstants.MessageStatus.NotSentForFilter && filterOperator != SQLComparisonOperator.Equal)
				{
					result.IsNoResultQuery = true;
				}
				else
				{
					if (value == DeclarationFilterConstants.MessageStatus.NotSentForFilter)
					{
						value = ZString.Empty;
						queryNotIn = true;
						msgNotSentSearch = true;
					}

					var invoiceHeaderQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE, queryNotIn);
					if (filterOperator != SQLComparisonOperator.NotEqual)
					{
						invoiceHeaderQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, 'N');
					}

					if (filterOperator == SQLComparisonOperator.StartsWith)
					{
						invoiceHeaderQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_MessageStatus, filterOperator, value);
					}
					else if (value.IsEmpty)
					{
						invoiceHeaderQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_MessageStatus, SQLComparisonOperator.NotEqual, value);
					}
					else
					{
						invoiceHeaderQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_MessageStatus, value);
					}

					result.AddSubQuery(invoiceHeaderQuery, JoinCondition.And);

					if (msgNotSentSearch)
					{
						var multiStatusJobsQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);

						multiStatusJobsQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, 'N');
						multiStatusJobsQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_MessageStatus, SQLComparisonOperator.Equal, value);
						multiStatusJobsQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Import);

						result.AddSubQuery(multiStatusJobsQuery, JoinCondition.Or);
					}

					if (filterOperator == SQLComparisonOperator.NotEqual)
					{
						var multipleStatusQueryText = @"JZ_GroupInvoice = 0 GROUP BY JZ_JE HAVING COUNT(DISTINCT(JZ_MessageStatus)) > 1"; // special case filter requires SQL statement

						var multipleStatusQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
						multipleStatusQuery.AddFilterAndZSQLParameterCollection(multipleStatusQueryText, new ZSqlParameterCollection());

						result.AddSubQuery(multipleStatusQuery, JoinCondition.Or);
					}
				}
			}

			return result;
		}

		ZQuery GetElectronicInvoiceRequestedQuery(ZBool show)
		{
			var query = new ZQuery();
			if (show)
			{
				return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_IsAIIRequested", SQLComparisonOperator.Equal, true);
			}
			return query;
		}

		ZQuery GetBLUStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return SimpleQueryHelper.GetQueryHandlingBlanks(JobDeclaration.Constants.GenAddOnColumnFieldName.BLUStatus, filterOperator, value);
		}

		ZQuery GetFDAMsgStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return SimpleQueryHelper.GetQueryHandlingBlanks(JobDeclaration.Constants.GenAddOnColumnFieldName.FDAMsgStatus, filterOperator, value);
		}

		ZQuery GetFDAStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return SimpleQueryHelper.GetQueryHandlingBlanks(JobDeclaration.Constants.GenAddOnColumnFieldName.FDAStatus, filterOperator, value);
		}

		ZQuery GetReleaseStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return SimpleQueryHelper.GetQueryHandlingBlanks(JobDeclaration.Constants.GenAddOnColumnFieldName.ReleaseStatus, filterOperator, value);
		}

		ZQuery GetPaperlessQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_PaperlessEntry", filterOperator, value);
		}

		ZQuery GetStatusNotificationDispositionCodeQuery(SQLComparisonOperator filterOperator, ZString value) => GetStatusNotificationDispositionCodeMultiValueQuery(new List<ZString> { value }, filterOperator);

		ZQuery GetStatusNotificationDispositionCodeMultiValueQuery(object value, SQLComparisonOperator filterOperator)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			if (value is List<ZString> codeList && codeList.Count > 0)
			{
				var entryQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
				entryQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.EntrySummary);

				var messageQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_LinkUniqueID);
				messageQuery.AddToFilter(EDIMessageSchema.EM_MessageType, ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification);
				messageQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.USCustomsImport);

				var isFilterOperatorNotEqual = filterOperator == SQLComparisonOperator.NotEqual;
				var sqlOperator = isFilterOperatorNotEqual ? SQLComparisonOperator.DoesNotStartWith : SQLComparisonOperator.StartsWith;
				var joinCondition = isFilterOperatorNotEqual ? JoinCondition.And : JoinCondition.Or;
				var subQuery = new ZQuery();
				foreach (var code in codeList)
				{
					subQuery.AddToFilter(joinCondition, EDIMessageSchema.EM_ApplicationReference, sqlOperator, code);
				}

				messageQuery.AddToFilter(subQuery);
				entryQuery.AddSubQuery(messageQuery, JoinCondition.And);
				result.AddSubQuery(entryQuery, JoinCondition.And);
			}

			return result;
		}

		ZQuery GetCargoReleaseCommentsActionsQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Import);

			var comparisonOperator = SQLComparisonOperator.Equal;
			if (value == DeclarationFilterConstants.ALL)
			{
				comparisonOperator = SQLComparisonOperator.NotEqual;
				value = ZString.Empty;
			}
			result.AddToFilter(ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.JE_ClusterKey, JobDeclaration.Schema.JE_ClusterKey, ModelViewSchema.TableName, ModelViewSchema.JE_CRLAction, comparisonOperator, value));

			var entryQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_ClusterKey);
			entryQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.ACECargoRelease);
			result.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, entryQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetEntrySummaryActionsQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Import);

			var comparisonOperator = SQLComparisonOperator.Equal;
			if (value == DeclarationFilterConstants.ALL)
			{
				comparisonOperator = SQLComparisonOperator.NotEqual;
				value = ZString.Empty;
			}
			result.AddToFilter(ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.JE_ClusterKey, JobDeclaration.Schema.JE_ClusterKey, ModelViewSchema.TableName, ModelViewSchema.JE_ENSAction, comparisonOperator, value));

			var entryQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_ClusterKey);
			entryQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
			result.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, entryQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetFTZAdmissionStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetFTZStatusQuery(JobDeclaration.Constants.GenAddOnColumnFieldName.FTZAdmissionStatus, filterOperator, value);
		}

		ZQuery GetFTZConcurrenceStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetFTZStatusQuery(JobDeclaration.Constants.GenAddOnColumnFieldName.FTZConcurrenceStatus, filterOperator, value);
		}

		ZQuery GetFTZDeliveryOfGoodsStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetFTZStatusQuery(JobDeclaration.Constants.GenAddOnColumnFieldName.FTZDeliveryOfGoodsStatus, filterOperator, value);
		}

		ZQuery GetFTZGoodsArrivalStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetFTZStatusQuery(JobDeclaration.Constants.GenAddOnColumnFieldName.FTZArrivalStatus, filterOperator, value);
		}

		ZQuery GetFTZPTTStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetFTZStatusQuery(JobDeclaration.Constants.GenAddOnColumnFieldName.FTZPTTStatus, filterOperator, value);
		}

		ZQuery GetFTZStatusQuery(string columnName, SQLComparisonOperator filterOperator, ZString value)
		{
			if (value == DeclarationFilterConstants.MessageStatus.NotSentForFilter)
			{
				var result = new ZDBOnlyQuery(typeof(JobDeclaration));
				if (filterOperator != SQLComparisonOperator.Equal)
				{
					result.IsNoResultQuery = true;
				}
				else
				{
					result.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.FTZ);
					var matchedQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID, true);
					matchedQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, columnName);
					result.AddSubQuery(matchedQuery, JoinCondition.And);
				}
				return result;
			}
			else
			{
				var query = SimpleQueryHelper.GetQueryHandlingBlanks(columnName, filterOperator, value);
				query.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.FTZ);
				return query;
			}
		}

		ZQuery GetSPIInvLineFilterQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetSpecificFieldQueryFromInvoiceLine(USAddInfoSchema.US_SPI, comparisonOperator, value, true);
		}

		#endregion

		#region Mode Filters

		protected override void AddModeFilters(ModuleFilterCollection filters)
		{
			var containerModeFilter = filters.AddTextFilter(DeclarationFilterConstants.NumberFilterTypes.ContainerModeCustoms, JobDeclarationSchema.JE_ContainerMode, Lookups.ContainerModeList);
			containerModeFilter.Category = FilterCategories.ModesAndTypes;

			var entryModeFilter = filters.AddTextFilter(DeclarationFilterConstants.EntryMode, GetEntryModeQuery, Lookups.EntryModes);
			entryModeFilter.Category = FilterCategories.ModesAndTypes;
			entryModeFilter.MaxLength = USAddInfoSchema.US_EntryMode.MaxLength;

			var flightVoyageVesselFilter = filters.AddTextAndNkFilter(DeclarationFilterConstants.FlightVoyageVessel, GetDeclarationFlightVoyageAndVesselQuery, ModuleIDs.RefVessel, Lookups.VesselList).WithMaxLengthOf(JobDeclarationSchema.JE_VoyageFlightNo, JobDeclarationSchema.JE_VesselName);
			flightVoyageVesselFilter.Category = FilterCategories.ModesAndTypes;

			var shipTypeFilter = filters.AddTextFilter(DeclarationFilterConstants.ShipmentType, JobDeclarationSchema.JE_MessageType, Lookups.MessageTypeList);
			shipTypeFilter.Category = FilterCategories.ModesAndTypes;
			shipTypeFilter.MaxLength = JobDeclarationSchema.JE_MessageType.MaxLength;

			var transportModeFilter = filters.AddTextFilter(DeclarationFilterConstants.TransportMode, JobDeclarationSchema.JE_TransportMode, Lookups.TransportTypeList);
			transportModeFilter.Category = FilterCategories.ModesAndTypes;
			transportModeFilter.MaxLength = JobDeclarationSchema.JE_TransportMode.MaxLength;

			var entryTypeFilter = filters.AddTextFilter(DeclarationFilterConstants.EntryType, GetEntryTypeQuery, Lookups.EntryTypeList);
			entryTypeFilter.Category = FilterCategories.ModesAndTypes;
			entryTypeFilter.MaxLength = USAddInfoSchema.US_EntryType.MaxLength;

			var reconIssueFilter = filters.AddTextFilter(DeclarationFilterConstants.ReconIssue, GetReconIssueQuery, Lookups.ReconIssueList);
			reconIssueFilter.Category = FilterCategories.ModesAndTypes;
			reconIssueFilter.MaxLength = USAddInfoSchema.US_OtherReconIndicator.MaxLength;

			var nAFTAReconIssueFilter = filters.AddFlagsFilter(DeclarationFilterConstants.FTAReconIndicator, new string[] { DeclarationFilterConstants.Flagged }, new GetFlagsQuery[] { GetFTAReconIssueQuery });
			nAFTAReconIssueFilter.Category = FilterCategories.ModesAndTypes;

			var includeIORFilingTheirOwnRec = filters.AddFlagsFilter(DeclarationFilterConstants.ExcludeIORFilingTheirOwnRec, new string[] { DeclarationFilterConstants.Exclude }, new GetFlagsQuery[] { GetExcludeIORFilingTheirOwnRecQuery });
			includeIORFilingTheirOwnRec.Category = FilterCategories.ModesAndTypes;

			var paymentType = filters.AddTextFilter(DeclarationFilterConstants.PaymentType, GetPaymentTypeQuery, Lookups.PaymentTypeList);
			paymentType.Category = FilterCategories.ModesAndTypes;
			paymentType.MaxLength = USAddInfoSchema.US_PaymentType.MaxLength;

			var paymentByBroker = filters.AddTextFilter(DeclarationFilterConstants.PaymentByBroker, GetPaymentByBrokerQuery, Lookups.YesNoList);
			paymentByBroker.Category = FilterCategories.ModesAndTypes;
			paymentByBroker.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			paymentByBroker.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			paymentByBroker.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			paymentByBroker.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			paymentByBroker.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			paymentByBroker.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			paymentByBroker.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);

			var applicationCodeFilter = filters.AddTextFilter(DeclarationFilterConstants.ApplicationCode, JobDeclarationSchema.JE_ApplicationCode, Lookups.JobApplicationCodeList);
			applicationCodeFilter.Category = FilterCategories.ModesAndTypes;
			applicationCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			applicationCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			applicationCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			applicationCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			applicationCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			applicationCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			applicationCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			applicationCodeFilter.MaxLength = JobDeclarationSchema.JE_ApplicationCode.MaxLength;

			var taxDeferredIndicatorFilter = filters.AddTextFilter(DeclarationFilterConstants.DeferredIndicator, GetDeferredIndicatorQuery, Lookups.TaxDeferIndicatorList);
			taxDeferredIndicatorFilter.Category = FilterCategories.ModesAndTypes;
			taxDeferredIndicatorFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			taxDeferredIndicatorFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			taxDeferredIndicatorFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			taxDeferredIndicatorFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			taxDeferredIndicatorFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			taxDeferredIndicatorFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			taxDeferredIndicatorFilter.MaxLength = USAddInfoSchema.US_TaxDeferIndicator.MaxLength;

			var cargoReleaseTypeFilter = filters.AddTextFilter(DeclarationFilterConstants.CargoReleaseType, GetCargoReleaseTypeQuery, Lookups.CargoReleaseTypes);
			cargoReleaseTypeFilter.Category = FilterCategories.ModesAndTypes;
			cargoReleaseTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			cargoReleaseTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			cargoReleaseTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			cargoReleaseTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			cargoReleaseTypeFilter.MaxLength = USAddInfoSchema.US_CargoReleaseType.MaxLength;

			var filingOptionFilter = filters.AddTextFilter(DeclarationFilterConstants.FilingOption, GetFilingOptionQuery, Lookups.FilingOptionList);
			filingOptionFilter.Category = FilterCategories.ModesAndTypes;
			filingOptionFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filingOptionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			filingOptionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			filingOptionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			filingOptionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			filingOptionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			filingOptionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			filingOptionFilter.MaxLength = USAddInfoSchema.US_CommodityFilingOption.MaxLength;

			var soldEnRouteFilter = filters.AddTextFilter(DeclarationFilterConstants.SoldEnRoute, GetSoldEnRouteQuery, Lookups.YesNoList);
			soldEnRouteFilter.Category = FilterCategories.ModesAndTypes;
			soldEnRouteFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			soldEnRouteFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			soldEnRouteFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			soldEnRouteFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			soldEnRouteFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			soldEnRouteFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			soldEnRouteFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			soldEnRouteFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			soldEnRouteFilter.MaxLength = USAddInfoSchema.US_SoldEnRouteIndicator.MaxLength;

			var basicDispositionCodeFilter = filters.AddTextFilter(DeclarationFilterConstants.BasicSTBDisposition, GetBaseDispositionCode, Lookups.BondDispositionCodeList);
			basicDispositionCodeFilter.Category = FilterCategories.ModesAndTypes;
			basicDispositionCodeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			basicDispositionCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			basicDispositionCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			basicDispositionCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			basicDispositionCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			basicDispositionCodeFilter.MaxLength = USAddInfoSchema.US_BondDispositionCode.MaxLength;

			var additionalDispositionCodeFilter = filters.AddTextFilter(DeclarationFilterConstants.AdditionalBondDisposition, GetAdditionalDispositionCode, Lookups.BondDispositionCodeList);
			additionalDispositionCodeFilter.Category = FilterCategories.ModesAndTypes;
			additionalDispositionCodeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			additionalDispositionCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			additionalDispositionCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			additionalDispositionCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			additionalDispositionCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			additionalDispositionCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			additionalDispositionCodeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			additionalDispositionCodeFilter.MaxLength = USAddInfoSchema.US_BondDispositionCode2.MaxLength;

			AddServiceTypeFilter(filters);
			AddPickupDeliveryDropModeFilters(filters);
			AddPickupDeliveryTransportCompanyFilters(filters);
		}

		ZQuery GetEntryModeQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_EntryMode", filterOperator, value);
		}

		ZQuery GetBaseDispositionCode(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetDispositionCore(filterOperator, value, "JE_BondDispositionCode");
		}

		ZQuery GetAdditionalDispositionCode(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetDispositionCore(filterOperator, value, "JE_BondDispositionCode2");
		}

		ZQuery GetInsuranceDisposition(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetDispositionCore(filterOperator, value, "JE_InsuranceDisposition");
		}

		ZQuery GetDispositionCore(SQLComparisonOperator filterOperator, ZString value, ZString fieldName)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Import);
			result.AddToFilter(JobDeclarationSchema.JE_ApplicationCode, JobApplicationCodeList.Codes.ACE);

			var bondTypeQuery = ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_BondType", SQLComparisonOperator.Equal, BondTypeList.Codes.SingleTransactionBond);
			result.AddToFilter(bondTypeQuery);

			result.AddToFilter(ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, fieldName, filterOperator, value));

			return result;
		}

		ZQuery GetPaymentByBrokerQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var predicateForSearch = Array.Empty<ZString>();
			var filterOperatorIsBlank = filterOperator == Enterprise.ZArchitecture.Business.SpecialComparisonOperator.IsBlank;
			if (filterOperatorIsBlank || value.IsEmpty)
			{
				predicateForSearch = new ZString[] { "", Customs.Business.PaymentPartyCodeDescriptionList.Codes.Default };
			}
			else
			{
				var paymentMethodForSearch = ZString.Empty;
				if (value == YesNoDefaultList.Codes.Yes)
				{
					paymentMethodForSearch = Customs.Business.PaymentPartyCodeDescriptionList.Codes.Broker;
				}
				else
				{
					paymentMethodForSearch = Customs.Business.PaymentPartyCodeDescriptionList.Codes.Importer;
				}
				predicateForSearch = new ZString[] { paymentMethodForSearch };
			}

			return new ZQuery(JobDeclarationSchema.JE_PaymentMethod, predicateForSearch);
		}

		ZQuery GetFilingOptionQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			result.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Export);

			var compOperator = filterOperator == SQLComparisonOperator.NotEqual ? new InexactComparisonOperator("not like", "", "") : SQLComparisonOperator.Like;
			result.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_AddInfo, compOperator, string.Format("%CommodityFilingOption={0}%", value));
			return result;
		}

		ZQuery GetSoldEnRouteQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Export);
			result.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_AddInfo, SQLComparisonOperator.Like, string.Format("%SoldEnRouteIndicator={0}%", value));
			return result;
		}

		ZQuery GetCargoReleaseTypeQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddToFilter(JobDeclarationSchema.JE_MessageType, new ZString[] { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.ImportByExternalBroker, JobMessageTypeList.Codes.Miscellaneous });

			var compOperator = filterOperator == SQLComparisonOperator.NotEqual || filterOperator == SpecialComparisonOperator.IsBlank ?
				new InexactComparisonOperator("not like", "", "") : SQLComparisonOperator.Like;

			result.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_AddInfo, compOperator, string.Format("%CargoReleaseType={0}%", value));
			return result;
		}

		#endregion

		#region Add Numbers And References

		protected override void AddNumberFilters(ModuleFilterCollection filters)
		{
			base.AddNumberFilters(filters);

			var filter = filters.AddTextFilter(DeclarationFilterConstants.Filer, GetEntryFilerCodeQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MaxLength = USAddInfoSchema.US_EntryFilerCode.MaxLength;

			filter = filters.AddTextFilter(DeclarationFilterConstants.BIRDBrokerRef, GetBirdBrokerRefQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MaxLength = USAddInfoSchema.US_BRDRefNo.MaxLength;

			filter = filters.AddTextFilter(DeclarationFilterConstants.BondNumber, GetBondNumberFilter);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MaxLength = USAddInfoSchema.US_BondProducerAccNo.MaxLength;

			filter = filters.AddTextFilter(DeclarationFilterConstants.ConsolidatedJobNo, GetConsolidatedJobNumberFilter);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MaxLength = JobDeclarationSchema.JE_DeclarationReference.MaxLength;

			filter = filters.AddTextFilter(DeclarationFilterConstants.StatementNo, this.GetStatementNoQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MaxLength = CusStatementHeaderSchema.B2_StatementNumber.MaxLength;

			filter = filters.AddTextFilter(DeclarationFilterConstants.ShipperReferenceNumber, GetShipperReferenceNumberFilter);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MaxLength = CusEntryHeaderSchema.CH_BGMReference.MaxLength;

			var ftzAdmissionNumberFilter = new FTZAdmissionNumberFilter(DeclarationFilterConstants.FTZAdmissionNumber);
			ftzAdmissionNumberFilter.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(ftzAdmissionNumberFilter);

			filter = GetAddInfoTextFilter(DeclarationFilterConstants.WarehouseEntryNumber, USAddInfoSchema.Constants.US_WHSEntryNumber.Substring(3));
			filter.Category = FilterCategories.NumbersAndReferences;
			filters.AddFilter(filter);

			filter = GetAddInfoTextFilter(DeclarationFilterConstants.WarehouseEntryFiler, USAddInfoSchema.Constants.US_WHSEntryFilerCode.Substring(3));
			filter.Category = FilterCategories.NumbersAndReferences;
			filters.AddFilter(filter);

			var importerEINSubGroup = new ImporterEINSubGroup();
			filter = filters.AddNumberFilter(DeclarationFilterConstants.ImporterEIN, OrgCusCodeSchema.OK_CustomsRegNo);
			filter.MaxLength = OrgCusCodeSchema.OK_CustomsRegNo.MaxLength;
			filter.SubGroup = importerEINSubGroup;

			filter = filters.AddNumberFilter(DeclarationFilterConstants.ImporterOfRecordEIN, GetImporterOfRecordEINFilterQuery);
			filter.MaxLength = OrgCusCodeSchema.OK_CustomsRegNo.MaxLength;
		}

		ZQuery GetEntryFilerCodeQuery(SQLComparisonOperator filterOperator, ZString filer)
		{
			var reconFilter = ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView_Recon, "JE_EntryFilerCode", filterOperator, filer);
			var query = ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_EntryFilerCode", filterOperator, filer);
			query.AddToFilter(reconFilter, JoinCondition.Or);
			return query;
		}

		ZQuery GetBirdBrokerRefQuery(SQLComparisonOperator filterOperator, ZString refNo)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_BRDRefNo", filterOperator, refNo);
		}

		ZQuery GetBondNumberFilter(SQLComparisonOperator comparisonOperator, ZString bondNo)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_BondProducerAccNo", comparisonOperator, bondNo);
		}

		ZQuery GetConsolidatedJobNumberFilter(SQLComparisonOperator comparisonOperator, ZString consolJobNo)
		{
			return SimpleQueryHelper.GetQueryHandlingBlanks(JobDeclaration.Constants.GenAddOnColumnFieldName.US_ConsolidatedJobNumber, comparisonOperator, consolJobNo);
		}

		ZQuery GetShipperReferenceNumberFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var filterOnBGMReference = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
			filterOnBGMReference.AddToFilter(CusEntryHeaderSchema.CH_MessageType, SQLComparisonOperator.Equal, CusEntryHeaderMessageTypeList.Codes.Export);
			filterOnBGMReference.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, comparisonOperator, value);

			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddSubQuery(filterOnBGMReference, JoinCondition.And);
			return result;
		}

		ZQuery GetImporterOfRecordEINFilterQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var orgCusCodeSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
			orgCusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.EmployerIdentificationNumber);
			orgCusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.UnitedStates);
			orgCusCodeSubQuery.AddToFilter_PossiblyCommaSeparated(OrgCusCodeSchema.OK_CustomsRegNo, comparisonOperator, value);

			var orgHeaderQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgHeaderQuery.AddSubQuery(OrgHeaderSchema.PK, orgCusCodeSubQuery, JoinCondition.And);
			var subAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			subAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, orgHeaderQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddSubQuery(JobDeclarationSchema.JE_OA_DeclarantAddress, subAddressQuery, JoinCondition.And);
			return result;
		}

		class ImporterEINSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var orgCusCodeSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
				orgCusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.EmployerIdentificationNumber);
				orgCusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.UnitedStates);
				orgCusCodeSubQuery.AddToFilter(filter);

				var orgHeaderQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				orgHeaderQuery.AddSubQuery(orgCusCodeSubQuery, JoinCondition.And);

				var result = new ZDBOnlyQuery(typeof(JobDeclaration));
				result.AddSubQuery(JobDeclarationSchema.JE_OH_Importer, orgHeaderQuery, JoinCondition.And);
				return result;
			}
		}

		ZQuery GetPreparerDistrictPortQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_PreparerDistrictPort", comparisonOperator, value);
		}

		#endregion

		#region Date Filters

		protected override void AddSubmittedDate(ModuleFilterCollection filters)
		{
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			var tIBFilter = filters.AddDateFilter(DeclarationFilterConstants.TIBExpiryDate, GetTIBExpiryDate);
			tIBFilter.Category = FilterCategories.Dates;

			filters.AddDateFilter(DeclarationFilterConstants.EntryReleaseDate, JobDeclarationSchema.JE_EntryAuthorisationDate);

			var entrySubmittedFilter = filters.AddDateFilter(DeclarationFilterConstants.EntrySubmittedDate, GetEntrySubmittedDate);
			entrySubmittedFilter.Category = FilterCategories.Dates;

			var paymentDueDateFilter = filters.AddDateFilter(DeclarationFilterConstants.PaymentDueDate, GetPaymentDueDate);
			paymentDueDateFilter.Category = FilterCategories.Dates;

			var liquidationDateFilter = filters.AddDateFilter(DeclarationFilterConstants.LiquidationDate, this.GetLiquidationDateQuery);
			liquidationDateFilter.Category = FilterCategories.Dates;

			var preliminaryStatementPrintDateFilter = filters.AddDateFilter(DeclarationFilterConstants.PrelimStatemPrintDate, GetPreliminaryStatementPrintDate);
			preliminaryStatementPrintDateFilter.Category = FilterCategories.Dates;

			var estimatedEntryDateFilter = filters.AddDateFilter(DeclarationFilterConstants.EstimatedEntryDate, GetEstimatedEntryDate);
			estimatedEntryDateFilter.Category = FilterCategories.Dates;

			var entryDateFilter = filters.AddDateFilter(DeclarationFilterConstants.EntryDate, GetEntryDate);
			entryDateFilter.Category = FilterCategories.Dates;

			var exportDateFilter = filters.AddDateFilter(DeclarationFilterConstants.ExportDate, GetDateOfExport);
			exportDateFilter.Category = FilterCategories.Dates;

			var presentationDateFilter = filters.AddDateFilter(DeclarationFilterConstants.PresentationDate, GetPresentationDateQuery);
			presentationDateFilter.Category = FilterCategories.Dates;

			var anticipatedLiquidationDateFilter = filters.AddDateFilter(DeclarationFilterConstants.AnticipLiquidationDate, GetAnticipLiquidationDateQuery);
			anticipatedLiquidationDateFilter.Category = FilterCategories.Dates;

			var deferredTaxDueDateFilter = filters.AddDateFilter(DeclarationFilterConstants.DeferredTaxDueDate, GetDeferredTaxDueDateQuery);
			deferredTaxDueDateFilter.Category = FilterCategories.Dates;

			var invoiceExportDateFilter = filters.AddDateFilter(DeclarationFilterConstants.InvoiceExportDate, GetInvoiceExportDateQuery);
			invoiceExportDateFilter.Category = FilterCategories.Dates;
		}

		ZQuery GetPresentationDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			return ModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_PresentationDate", comparisonOperator, startDate, endDate);
		}

		ZQuery GetAnticipLiquidationDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			var query = new ZQuery();
			var queryOnEntry = GenAddOnColumnHelper.QueryHelper.GetQueryWithSubQueryOnGenAddOnColumn(USAddInfoSchema.Constants.US_ALDate, comparisonOperator, startDate, endDate);
			queryOnEntry.AddToFilter(JobDeclarationSchema.JE_MessageType, SQLComparisonOperator.NotEqual, JobMessageTypeList.Codes.Drawback);
			query.AddToFilter(queryOnEntry);

			var queryOnDeclaration = ModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_ALDate", comparisonOperator, startDate, endDate);
			queryOnDeclaration.AddToFilter(JobDeclarationSchema.JE_MessageType, SQLComparisonOperator.Equal, JobMessageTypeList.Codes.Drawback);
			query.AddToFilter(queryOnDeclaration, JoinCondition.Or);
			return query;
		}

		ZQuery GetTIBExpiryDate(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			return GenAddOnColumnHelper.QueryHelper.GetQueryWithSubQueryOnGenAddOnColumn(USAddInfoSchema.Constants.US_TIBExpiryDate, comparisonOperator, startDate, endDate);
		}

		ZQuery GetEntrySubmittedDate(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered || comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				return EntrySubmittedDateInNotInQuery(comparisonOperator);
			}
			else
			{
				return EntrySubmittedDateInDateRangeQuery(comparisonOperator, date1, date2);
			}
		}

		ZQuery EntrySubmittedDateInDateRangeQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			var entrySubmittedDateQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
			entrySubmittedDateQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
			AddDateRange(entrySubmittedDateQuery, comparisonOperator, JoinCondition.And, CusEntryHeaderSchema.CH_EntrySubmittedDate, date1.Date, date2.Date);

			result.AddSubQuery(entrySubmittedDateQuery, JoinCondition.And);

			return result;
		}

		ZQuery EntrySubmittedDateInNotInQuery(DateComparisonOperator comparisonOperator)
		{
			ZQuery result = new ZDBOnlyQuery(typeof(JobDeclaration));

			var queryInNotIn = " IN ";
			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				queryInNotIn = " NOT IN ";
			}

			var queryText = string.Format(@" JE_PK " + queryInNotIn + " ( SELECT CH_JE FROM dbo.CusEntryHeader WHERE CH_EntrySubmittedDate is not null and CH_MessageType = '" + CusEntryHeaderMessageTypeList.Codes.EntrySummary + "')"); // direct query required for HasNoDateEntered

			result.AddFilterAndZSQLParameterCollection(queryText, new ZSqlParameterCollection());

			return result;
		}

		ZQuery GetPaymentDueDate(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			return ModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_PaymentDueDate", comparisonOperator, startDate, endDate);
		}

		ZQuery GetPreliminaryStatementPrintDate(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			var reconFilter = ModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView_Recon, "JE_PreliminaryStatementPrintDate", comparisonOperator, startDate, endDate);
			var filter = ModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_PreliminaryStatementPrintDate", comparisonOperator, startDate, endDate);
			filter.AddToFilter(reconFilter, JoinCondition.Or);
			return filter;
		}

		ZQuery GetEstimatedEntryDate(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			var reconFilter = ModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView_Recon, "JE_EstimatedEntryDate", comparisonOperator, startDate, endDate);
			var filter = ModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_EstimatedEntryDate", comparisonOperator, startDate, endDate);
			filter.AddToFilter(reconFilter, JoinCondition.Or);
			return filter;
		}

		ZQuery GetEntryDate(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			return ModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_EntryDate", comparisonOperator, startDate, endDate);
		}

		ZQuery GetDateOfExport(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			var query = ModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_DateOfExport", comparisonOperator, startDate, endDate);
			var queryJE_ExportDate = new DateQueryBuilder().CreateDateTimeRange(comparisonOperator, JobDeclarationSchema.JE_ExportDate, startDate, endDate, true, true);

			switch (comparisonOperator)
			{
				case DateComparisonOperator.HasNoDateEntered:
					{
						query.AddToFilter(queryJE_ExportDate, JoinCondition.And);
						break;
					}
				case DateComparisonOperator.HasDateEntered:
					{
						query.AddToFilter(queryJE_ExportDate, JoinCondition.Or);
						break;
					}
				case DateComparisonOperator.HasDateInRange:
					{
						var queryUS_DateOfExportIsNull = new ZDBOnlyQuery(typeof(JobDeclaration));
						queryUS_DateOfExportIsNull.AddToFilter(ModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_DateOfExport", DateComparisonOperator.HasNoDateEntered, ZDateTime.Empty, ZDateTime.Empty));
						queryJE_ExportDate.AddToFilter(queryUS_DateOfExportIsNull, JoinCondition.And);
						query.AddToFilter(queryJE_ExportDate, JoinCondition.Or);
						break;
					}
			}
			return query;
		}

		ZQuery GetDeferredTaxDueDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			return ModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_DeferredTaxDueDate", comparisonOperator, startDate, endDate);
		}

		ZQuery GetInvoiceExportDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var sqlFilter = ZString.Empty;
			if (comparisonOperator == DateComparisonOperator.HasDateInRange)
			{
				sqlFilter = @"JE_PK IN " + // direct query required for this date filter
@"(
SELECT JE_PK FROM dbo.JobDeclaration
INNER JOIN dbo.JobComInvoiceHeader ON JZ_JE = JE_PK
INNER JOIN dbo.JobComInvoiceLine ON JI_JZ = JZ_PK
CROSS APPLY csfn_GetAddInfoValueFromCodeInlineAsSmallDateTime(JE_AddInfo, 'DateOfExport') as JEDateOfExport
CROSS APPLY csfn_GetAddInfoValueFromCodeInlineAsSmallDateTime(JZ_AddInfo, 'DateOfExport') as JZDateOfExport
CROSS APPLY csfn_GetAddInfoValueFromCodeInlineAsSmallDateTime(JI_AddInfo, 'DateOfExport') as JIDateOfExport
WHERE JE_MessageType = 'EXP'
	AND 
	(
		(JEDateOfExport.ValueAsSmallDateTime >= @FromTime AND JEDateOfExport.ValueAsSmallDateTime <= @ToTime )
		OR
		(JZDateOfExport.ValueAsSmallDateTime >= @FromTime AND JZDateOfExport.ValueAsSmallDateTime <= @ToTime )
		OR
		(JIDateOfExport.ValueAsSmallDateTime >= @FromTime AND JIDateOfExport.ValueAsSmallDateTime <= @ToTime )
		OR 
		(JE_ExportDate >= @FromTime AND JE_ExportDate <= @ToTime)
	)
)
";// direct query required for this date filter
			}
			else if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				sqlFilter = @"JE_PK IN " + // direct query required for this date filter
@"(
SELECT JE_PK FROM dbo.JobDeclaration
INNER JOIN dbo.JobComInvoiceHeader ON JZ_JE = JE_PK
INNER JOIN dbo.JobComInvoiceLine ON JI_JZ = JZ_PK
WHERE JE_MessageType = 'EXP'
	AND JE_AddInfo NOT LIKE '%DateOfExport%'
	AND JZ_AddInfo NOT LIKE '%DateOfExport%'
	AND JI_AddInfo NOT LIKE '%DateOfExport%'
	AND JE_ExportDate IS NULL
)
";// direct query required for this date filter
			}
			else if (comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				sqlFilter = @"JE_PK IN " + // direct query required for this date filter
@"(
SELECT JE_PK FROM dbo.JobDeclaration
INNER JOIN dbo.JobComInvoiceHeader ON JZ_JE = JE_PK
INNER JOIN dbo.JobComInvoiceLine ON JI_JZ = JZ_PK
WHERE JE_MessageType = 'EXP'
	AND 
	(
		JE_AddInfo LIKE '%DateOfExport%'
		OR  JZ_AddInfo LIKE '%DateOfExport%'
		OR JI_AddInfo LIKE '%DateOfExport%'
		OR JE_ExportDate IS NOT NULL
	)
)
";// direct query required for this date filter
			}

			date1 = date1.IsValid ? date1 : ZDateTime.MinSmallDateTimeValue;
			date2 = date2.IsValid ? date2 : ZDateTime.MaxSmallDateTimeValue;

			var sqlFilterParameters = new ZSqlParameterCollection(
		ZSqlParameter.New("@FromTime", date1, CargoWise.Schema.Schema.GenericDateTimeColumn),
		ZSqlParameter.New("@ToTime", date2, CargoWise.Schema.Schema.GenericDateTimeColumn)
		);// AddInfo fields

			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddFilterAndZSQLParameterCollection(sqlFilter, sqlFilterParameters);
			return result;
		}

		#endregion

		#region Organisation Filters

		protected void AddAdditionalOrganisationFilters(ModuleFilterCollection filters)
		{
			var carrierFilter = filters.AddGuidFilter(DeclarationFilterConstants.Carrier, ModuleIDs.Organisation, JobDeclarationSchema.JE_OH_ShippingLine, Lookups.ShippingLines);
			carrierFilter.Category = FilterCategories.Organisations;

			var carrierSCACFilter = filters.AddTextFilter(DeclarationFilterConstants.CarrierSCAC, GetCarrierSCACQuery);
			carrierSCACFilter.Category = FilterCategories.Organisations;
			carrierSCACFilter.MaxLength = OrgCusCodeSchema.OK_CustomsRegNo.MaxLength;

			var locationOfGoodsFilter = filters.AddTextFilter(DeclarationFilterConstants.LocationOfGoods, GetLocationOfGoodsQuery);
			locationOfGoodsFilter.Category = FilterCategories.Locations;
			locationOfGoodsFilter.MaxLength = USAddInfoSchema.US_US_NKLocationOfGoods.MaxLength;

			var notifyPartyFilter = filters.AddGuidFilter(DeclarationFilterConstants.NotifyParty, ModuleIDs.Organisation, GetNotifyPartyQuery, Lookups.NotifyParties);
			notifyPartyFilter.Category = FilterCategories.Organisations;

			var soldToPartyFilter = filters.AddGuidFilter(DeclarationFilterConstants.SoldToParty, ModuleIDs.Organisation, GetSoldToPartyQuery, Lookups.SoldToParties);
			soldToPartyFilter.Category = FilterCategories.Organisations;

			var ultimateConsigneeFilter = filters.AddGuidFilter(DeclarationFilterConstants.UltinateConsignee, ModuleIDs.Organisation, GetUltimateConsigneeQuery, Lookups.Consignees);
			ultimateConsigneeFilter.Category = FilterCategories.Organisations;
		}

		ZQuery GetUltimateConsigneeQuery(ZGuid pk)
		{
			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, pk);

			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddSubQuery(JobDeclarationSchema.JE_OA_ConsigneeAddress, orgAddressQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetNotifyPartyQuery(ZGuid pk)
		{
			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgAddressQuery.AddToFilter(OrgHeaderSchema.PK, pk);

			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddSubQuery(JobDeclarationSchema.JE_OH_NotifyParty, orgAddressQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetSoldToPartyQuery(ZGuid pk)
		{
			var addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			addressQuery.AddToFilter(OrgAddressSchema.OA_OH, pk);

			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddSubQuery(JobDeclarationSchema.JE_OA_SoldToPartyAddress, addressQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#region Carrier SCAC Query

		ZQuery GetCarrierSCACQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZQuery();
			var filterIsBlank = filterOperator == SpecialComparisonOperator.IsBlank;
			var usJobDeclarationSCACQuery = ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_UI_NKCarrierSCAC", filterOperator, value);
			if (filterIsBlank || filterOperator.IsNegativeSQLOperator())
			{
				var importUSJobDeclarationSCACQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				importUSJobDeclarationSCACQuery.AddToFilter(usJobDeclarationSCACQuery);
				importUSJobDeclarationSCACQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Import);
				result.AddToFilter(importUSJobDeclarationSCACQuery);
			}
			else
			{
				result.AddToFilter(usJobDeclarationSCACQuery);
			}

			var orgSCACCodeQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
			orgSCACCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.CarrierCode);
			orgSCACCodeQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.UnitedStates);

			var exportSCACQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			exportSCACQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Export);

			var orgSCACEmptyQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			orgSCACEmptyQuery.AddToFilter(JobDeclarationSchema.JE_OH_ShippingLine, null);

			if (filterIsBlank || filterOperator == SpecialComparisonOperator.IsNotBlank)
			{
				var orgSCACQuery = new ZDBOnlySubQuery(typeof(OrgHeader), JobDeclarationSchema.JE_OH_ShippingLine, filterIsBlank);
				orgSCACQuery.AddSubQuery(orgSCACCodeQuery, JoinCondition.And);

				if (filterIsBlank)
				{
					var exportSCACQueryBlankOp = exportSCACQuery;
					exportSCACQueryBlankOp.AddToFilter(usJobDeclarationSCACQuery);
					var orgSCACEmptyQueryBlankOp = orgSCACEmptyQuery;
					orgSCACEmptyQueryBlankOp.AddSubQuery(orgSCACQuery, JoinCondition.Or);
					exportSCACQueryBlankOp.AddToFilter(orgSCACEmptyQueryBlankOp);
					result.AddToFilter(exportSCACQueryBlankOp, JoinCondition.Or);
				}
				else
				{
					var exportSCACQueryNotBlankOp = exportSCACQuery;
					exportSCACQueryNotBlankOp.AddSubQuery(orgSCACQuery, JoinCondition.And);
					result.AddToFilter(exportSCACQueryNotBlankOp, JoinCondition.Or);
				}
			}
			else if (!value.IsEmpty)
			{
				if (filterOperator.IsNegativeSQLOperator())
				{
					var exportSCACQueryNegOp = exportSCACQuery;
					exportSCACQueryNegOp.AddToFilter(ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_UI_NKCarrierSCAC", SQLComparisonOperator.IsBlank, ZString.Empty));
					var orgSCACQuery = new ZDBOnlySubQuery(typeof(OrgHeader), JobDeclarationSchema.JE_OH_ShippingLine, true);
					var orgSCACCodeQueryNegOp = orgSCACCodeQuery;
					orgSCACCodeQueryNegOp.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, filterOperator.GetNegatingSQLOperatorIfNotInSubquery(), value);
					orgSCACQuery.AddSubQuery(orgSCACCodeQueryNegOp, JoinCondition.And);
					var orgSCACEmptyQueryNegOp = orgSCACEmptyQuery;
					orgSCACEmptyQueryNegOp.AddSubQuery(orgSCACQuery, JoinCondition.Or);
					exportSCACQueryNegOp.AddToFilter(orgSCACEmptyQueryNegOp);

					var exportOnlyGenAddOnSCACQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
					exportOnlyGenAddOnSCACQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Export);
					exportOnlyGenAddOnSCACQuery.AddToFilter(ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_UI_NKCarrierSCAC", SQLComparisonOperator.IsNotBlank, ZString.Empty));
					exportOnlyGenAddOnSCACQuery.AddToFilter(usJobDeclarationSCACQuery);
					result.AddToFilter(exportOnlyGenAddOnSCACQuery, JoinCondition.Or);
					result.AddToFilter(exportSCACQueryNegOp, JoinCondition.Or);
				}
				else
				{
					var orgSCACQuery = new ZDBOnlySubQuery(typeof(OrgHeader), JobDeclarationSchema.JE_OH_ShippingLine);
					var orgSCACCodeQueryNormalOp = orgSCACCodeQuery;
					orgSCACCodeQueryNormalOp.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, filterOperator, value);
					orgSCACQuery.AddSubQuery(orgSCACCodeQueryNormalOp, JoinCondition.And);
					var exportSCACQueryNormalOp = exportSCACQuery;
					exportSCACQueryNormalOp.AddSubQuery(orgSCACQuery, JoinCondition.And);
					exportSCACQueryNormalOp.AddToFilter(ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_UI_NKCarrierSCAC", SQLComparisonOperator.IsBlank, ZString.Empty));
					result.AddToFilter(exportSCACQueryNormalOp, JoinCondition.Or);
				}
			}
			return result;
		}

		#endregion

		#region Location of Goods Query

		ZQuery GetLocationOfGoodsQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_US_NKLocationOfGoods", filterOperator, value);
			result.AddToFilter(JobDeclarationSchema.JE_MessageType, SQLComparisonOperator.NotEqual, JobMessageTypeList.Codes.Export);
			return result;
		}

		#endregion

		#region IT (InBond) Filters

		void AddITFilters(ModuleFilterCollection filters)
		{
			var iTNumberFilter = filters.AddNumberFilter(DeclarationFilterConstants.ITNumber, GetITNumberQuery);
			iTNumberFilter.Category = ITCategory;
			iTNumberFilter.MaxLength = USITNumberAddInfoSchema.US_ITNumber.MaxLength;
			ObjectFactory.Get<Integration.Customs.US.InBond.IInBondFiltersProvider>().AddFilters(filters, typeof(JobDeclaration));
		}

		ZQuery GetITNumberQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var notIn = filterOperator == Enterprise.ZArchitecture.Business.SpecialComparisonOperator.IsBlank;

			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			var query = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.CU_JE);

			var subQuery = new ZDBOnlySubQuery(typeof(CusAddInfo), CusAddInfoSchema.B7_ParentID);

			var addOnColumnSubQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID, notIn);
			addOnColumnSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, USITNumberAddInfoSchema.Constants.US_ITNumber);
			if (!value.IsEmpty)
			{
				addOnColumnSubQuery.AddToFilter_PossiblyCommaSeparated(GenAddOnColumnSchema.XA_Data, filterOperator, value);
			}

			subQuery.AddSubQuery(addOnColumnSubQuery, JoinCondition.And);

			query.AddSubQuery(subQuery, JoinCondition.And);

			result.AddSubQuery(query, JoinCondition.And);

			return result;
		}

		FilterCategory ITCategory
		{
			get { return iTCategory ?? (iTCategory = new FilterCategory((NoResString)"IT (InBond)")); }
		}
		FilterCategory iTCategory;

		#endregion

		#region Statement Filters

		void AddStatementFilters(ModuleFilterCollection filters)
		{
			var statementStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.StatementStatus, GetStatementStatusQuery, Lookups.StatementStatusList);
			SetFilterConstraints(statementStatusFilter);

			var paymentStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.PaymentStatus, GetPaymentStatusQuery, Lookups.PaymentStatusList);
			SetFilterConstraints(paymentStatusFilter);

			AddInvoiceTotalFilters(filters);
		}

		protected void AddInvoiceTotalFilters(ModuleFilterCollection filters)
		{
			var paymentAmountFilter = filters.AddNumberRangeFilter(DeclarationFilterConstants.TotalDutiesAndFees, GetTotalPayableQuery);
			paymentAmountFilter.Decimals = 2;
			paymentAmountFilter.Category = FilterCategories.Other;

			var totalOutstandingFilter = filters.AddNumberRangeFilter(DeclarationFilterConstants.TotalOutstanding, GetTotalOutstandingQuery);
			totalOutstandingFilter.Decimals = 2;
			totalOutstandingFilter.Category = FilterCategories.Other;

			var totalInvoicedFilter = filters.AddNumberRangeFilter(DeclarationFilterConstants.TotalInvoiced, GetTotalInvoicedQuery);
			totalInvoicedFilter.Decimals = 2;
			totalInvoicedFilter.Category = FilterCategories.Other;

			var totalBilledFilter = filters.AddNumberRangeFilter(DeclarationFilterConstants.TotalBilled, GetTotalBilledQuery);
			totalBilledFilter.Decimals = 2;
			totalBilledFilter.Category = FilterCategories.Other;
		}

		ZQuery GetStatementStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetStatementQuery(CusStatementHeaderSchema.B2_Status, filterOperator, value);
		}

		ZQuery GetPaymentStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetStatementQuery(CusStatementHeaderSchema.B2_PaymentStatus, filterOperator, value);
		}

		ZQuery GetStatementQuery(SchemaColumn statusColumn, SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZQuery();

			var queryInNotIn = " IN ";
			if (filterOperator == SQLComparisonOperator.NotEqual)
			{
				queryInNotIn = " NOT IN ";
			}

			var valueComparision = " = ";
			if (filterOperator == SQLComparisonOperator.StartsWith)
			{
				valueComparision = " LIKE ";
				value = value + "%";
			}

			if (!value.IsEmpty)
			{
				result = new ZDBOnlyQuery(typeof(JobDeclaration));

				var queryText = string.Format(@" JE_PK " + queryInNotIn + " ( SELECT JE_PK FROM dbo.JobDeclaration  JOIN dbo.CusEntryNum  ON CE_ParentID = JE_PK JOIN dbo.CusStatementLine  ON CE_EntryNum = B3_EntryNum JOIN dbo.CusStatementHeader  ON B3_B2 = B2_PK WHERE " + statusColumn.Name + valueComparision + "@Status)");
				if (statusColumn == CusStatementHeaderSchema.B2_PaymentStatus)
				{
					queryText = string.Format(@" JE_PK " + queryInNotIn + " ( SELECT JE_PK FROM dbo.JobDeclaration  JOIN dbo.CusEntryNum  ON CE_ParentID = JE_PK JOIN dbo.CusStatementLine  ON CE_EntryNum = B3_EntryNum JOIN dbo.CusStatementHeader  ON B3_B2 = B2_PK WHERE " + statusColumn.Name + valueComparision + "@PaymentStatus AND B3_Status <> '" + StatementLineStatusList.Codes.Deleted + "')");
				}

				var queryParams = new ZSqlParameterCollection();
				if (statusColumn == CusStatementHeaderSchema.B2_Status)
				{
					queryParams.Add("@Status", value, statusColumn);
				}
				else
				{
					queryParams.Add("@PaymentStatus", value, statusColumn);
				}

				result.AddFilterAndZSQLParameterCollection(queryText, queryParams);
			}

			return result;
		}

		ZQuery GetTotalPayableQuery(INumericZType paymentFrom, INumericZType paymentTo)
		{
			var result = new ZQuery();
			var amtPayableQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			var eNSQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
			eNSQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
			if (!paymentFrom.IsEmpty && !paymentTo.IsEmpty)
			{
				ModuleNumberRangeFilter.AddToFilters(eNSQuery, CusEntryHeaderSchema.CH_TotalPaid, paymentFrom, paymentTo);
			}
			else if (!paymentFrom.IsEmpty || !paymentTo.IsEmpty)
			{
				var comparisonOperator = paymentFrom.IsEmpty ? SQLComparisonOperator.GreaterThan : SQLComparisonOperator.GreaterThanOrEqualTo;
				eNSQuery.AddToFilter(CusEntryHeaderSchema.CH_TotalPaid, comparisonOperator, paymentFrom);
				if (!paymentTo.IsEmpty)
				{
					eNSQuery.AddToFilter(CusEntryHeaderSchema.CH_TotalPaid, SQLComparisonOperator.LessThanOrEqualTo, paymentTo);
				}
			}
			else
			{
				eNSQuery.AddToFilter(CusEntryHeaderSchema.CH_TotalPaid, SQLComparisonOperator.Equal, paymentFrom);
			}
			amtPayableQuery.AddSubQuery(eNSQuery, JoinCondition.And);
			result.AddToFilter(amtPayableQuery);

			return result;
		}

		ZQuery GetTotalOutstandingQuery(INumericZType amountFrom, INumericZType amountTo)
		{
			var result = new ZQuery();
			result.AddToFilter(TransactionQuery(amountFrom, amountTo, AccTransactionHeaderSchema.AH_OutstandingAmount.Name));
			return result;
		}

		ZQuery GetTotalInvoicedQuery(INumericZType amountFrom, INumericZType amountTo)
		{
			var result = new ZQuery();
			result.AddToFilter(TransactionQuery(amountFrom, amountTo, AccTransactionHeaderSchema.AH_InvoiceAmount.Name));
			return result;
		}

		ZQuery GetTotalBilledQuery(INumericZType amountFrom, INumericZType amountTo)
		{
			var result = new ZQuery();
			result.AddToFilter(BilledQuery(amountFrom, amountTo, AccTransactionLinesSchema.AL_LineAmount.Name));
			return result;
		}

		ZDBOnlyQuery TransactionQuery(INumericZType amountFrom, INumericZType amountTo, string fieldName)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			ZString queryText = @"JE_PK IN (SELECT JE_PK FROM dbo.JobDeclaration  " +   // direct query required for summation of transactions
@"LEFT JOIN 
(
SELECT JH_ParentID, SUM(" + fieldName + @") AS transAmount FROM dbo.JobHeader  
LEFT JOIN dbo.AccTransactionHeader  ON AH_JH = JH_PK AND AH_Ledger = 'AR' 
WHERE (AH_TransactionCategory = 'DBT' OR 
EXISTS (SELECT 1 
		  FROM dbo.AccTransactionLines 
		  INNER JOIN dbo.AccChargeCode  on AL_AC = AC_PK
		 WHERE AC_ChargeType = 'DSB'
		  AND AL_AH = AH_PK))
AND JH_GC = @branch
GROUP BY JH_ParentID
)
AS Disbursements ON Disbursements.JH_ParentID IN (JE_PK, JE_JS)
WHERE ({0}))";

			queryText = string.Format(queryText, GetSubFilterByTransAmount(amountFrom, amountTo, "transAmount"));
			var queryParams = new ZSqlParameterCollection();
			queryParams.Add("@branch", GlbCompany.CurrentCompany.PK, JobHeaderSchema.JH_GC);

			result.AddFilterAndZSQLParameterCollection(queryText, queryParams);

			return result;
		}

		ZDBOnlyQuery BilledQuery(INumericZType amountFrom, INumericZType amountTo, string fieldName)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			ZString queryText = @"JE_PK IN (SELECT JE_PK FROM dbo.JobDeclaration  " + // direct query required for summation of transactions
@"LEFT JOIN 
(
SELECT JH_ParentID, SUM(" + fieldName + @") AS transAmount FROM dbo.JobHeader  
LEFT JOIN dbo.AccTransactionHeader  ON AH_JH = JH_PK AND AH_Ledger = 'AR' 
INNER JOIN dbo.AccTransactionLines  ON AL_AH = AH_PK 
INNER JOIN dbo.AccChargeCode  ON AL_AC = AC_PK 
WHERE AC_ChargeType = 'DSB'
AND JH_GC = @branch
GROUP BY JH_ParentID
)
AS Billed ON Billed.JH_ParentID IN (JE_PK, JE_JS)
WHERE ({0}))";
			queryText = string.Format(queryText, GetSubFilterByTransAmount(amountFrom, amountTo, "transAmount"));
			var queryParams = new ZSqlParameterCollection();
			queryParams.Add("@branch", GlbCompany.CurrentCompany.PK, JobHeaderSchema.JH_GC);

			result.AddFilterAndZSQLParameterCollection(queryText, queryParams);

			return result;
		}

		string GetSubFilterByTransAmount(INumericZType amountFrom, INumericZType amountTo, string fieldName)
		{
			var result = string.Empty;
			if (amountFrom.IsEmpty && amountTo.IsEmpty)
			{
				result = string.Format("{0} = {1}", fieldName, amountFrom);
			}
			else
			{
				var comparisonOperator = amountFrom.IsEmpty ? " > " : " >= ";
				result = string.Format("{0} {1} {2} AND {0} <= {3}", fieldName, comparisonOperator, amountFrom.ToString(), amountTo.ToString());
			}
			return result;
		}

		#endregion

		#region Misc Filters

		void AddMiscFilters(ModuleFilterCollection filters)
		{
			var suretyCodeFilter = filters.AddTextFilter(DeclarationFilterConstants.SuretyCode, GetSuretyCodeQuery);
			suretyCodeFilter.Category = FilterCategories.TextSearch;
			suretyCodeFilter.MaxLength = USAddInfoSchema.US_SuretyCode.MaxLength;

			var notReconciledFilter = filters.AddTextFilter(DeclarationFilterConstants.NotYetReconciled, GetSpecificNotReconciledQuery, Lookups.IssueCodeList);
			notReconciledFilter.Category = FilterCategories.TextSearch;
			notReconciledFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			notReconciledFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			notReconciledFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			notReconciledFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			notReconciledFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			notReconciledFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			notReconciledFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			notReconciledFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);

			var impSpecialistTeamFilter = filters.AddTextFilter(DeclarationFilterConstants.ImportSpecialistTeam, GetImportSpecialistTeamQuery);
			impSpecialistTeamFilter.Category = FilterCategories.TextSearch;
			impSpecialistTeamFilter.MaxLength = USAddInfoSchema.US_TeamNo.MaxLength;
		}

		ZQuery GetImportSpecialistTeamQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var reconFilter = ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView_Recon, "JE_TeamNo", filterOperator, value);
			var filter = ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_TeamNo", filterOperator, value);
			filter.AddToFilter(reconFilter, JoinCondition.Or);
			return filter;
		}

		ZQuery GetSuretyCodeQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var reconFilter = ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView_Recon, "JE_SuretyCode", filterOperator, value);
			var filter = ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_SuretyCode", filterOperator, value);
			filter.AddToFilter(reconFilter, JoinCondition.Or);
			return filter;
		}

		ZQuery GetSpecificNotReconciledQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var decQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			decQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Import);

			if (!value.IsEmpty)
			{
				if (value == "NF")
				{
					decQuery.AddToFilter(ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_NAFTAReconIndicator", SQLComparisonOperator.Equal, true));
				}
				else
				{
					decQuery.AddToFilter(ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_OtherReconIndicator", SQLComparisonOperator.Equal, value));
				}

				var originalImportEntryQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
				originalImportEntryQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.EntrySummary);

				var reconEntryQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_CH_PrimeEntry, true);
				reconEntryQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry);
				reconEntryQuery.AddToFilter(CusEntryHeaderSchema.CH_CH_PrimeEntry, SQLComparisonOperator.NotEqual, DBNull.Value);
				reconEntryQuery.AddToFilter(ModelViewColumnHelper.GetModuleFilterQuery(CusEntryHeader.Schema.CH_JE, ModelViewPK, ModelView_Recon, "JE_IssueCode", SQLComparisonOperator.Equal, value));

				originalImportEntryQuery.AddSubQuery(reconEntryQuery, JoinCondition.And);
				decQuery.AddSubQuery(originalImportEntryQuery, JoinCondition.And);
			}

			return decQuery;
		}

		#endregion

		#region Extra Filters

		ZQuery GetReconIssueQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_OtherReconIndicator", filterOperator, value);
		}

		ZQuery GetFTAReconIssueQuery(ZBool value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_NAFTAReconIndicator", SQLComparisonOperator.Equal, value);
		}

		ZQuery GetExcludeIORFilingTheirOwnRecQuery(ZBool value)
		{
			var result = new ZQuery();

			if (value)
			{
				result.AddToFilter(JobDeclarationSchema.JE_AddInfo, SQLComparisonOperator.NotContains, USAddInfoSchema.Constants.US_FileTheirOwnRecon.Substring(3) + "=Y");
			}

			return result;
		}

		ZQuery GetEntryTypeQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_EntryType", filterOperator, value);
		}

		ZQuery GetPaymentTypeQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var reconFilter = ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView_Recon, "JE_PaymentType", filterOperator, value);
			var filter = ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_PaymentType", filterOperator, value);
			filter.AddToFilter(reconFilter, JoinCondition.Or);
			return filter;
		}

		ZQuery GetImporterOfRecordQuery(ZGuid value)
		{
			var addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			addressQuery.AddToFilter(OrgAddressSchema.OA_OH, value);
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddSubQuery(JobDeclarationSchema.JE_OA_DeclarantAddress, addressQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetPortOfEntryQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var reconFilter = ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView_Recon, "JE_SchDEntry", filterOperator, value);
			var filter = ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_SchDEntry", filterOperator, value);
			filter.AddToFilter(reconFilter, JoinCondition.Or);
			return filter;
		}

		ZQuery GetDeferredIndicatorQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			result.AddToFilter(JobDeclarationSchema.JE_MessageType, new ZString[] { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.ImportByExternalBroker, JobMessageTypeList.Codes.Miscellaneous });

			var compOperator = filterOperator == SQLComparisonOperator.NotEqual ? new InexactComparisonOperator("not like", "", "") : SQLComparisonOperator.Like;
			result.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_AddInfo, compOperator, string.Format("%TaxDeferIndicator={0}%", value));
			return result;
		}

		#endregion

		#region Lookups

		public new JobDeclarationFilterLookups Lookups
		{
			get { return (JobDeclarationFilterLookups)base.Lookups; }
		}

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups()
		{
			return new JobDeclarationFilterLookups(this);
		}

		#endregion

		GenAddOnColumnHelper GenAddOnColumnHelper => helper ?? (helper = new GenAddOnColumnHelper());
		GenAddOnColumnHelper helper;

		#region Model View

		Customs.Business.ModelViewColumnQueryHelper<JobDeclaration> ModelViewColumnHelper
		{
			get
			{
				return modelViewColumnHelper ?? (modelViewColumnHelper = new Customs.Business.ModelViewColumnQueryHelper<JobDeclaration>());
			}
		}
		Customs.Business.ModelViewColumnQueryHelper<JobDeclaration> modelViewColumnHelper;

		const string ModelView = JobDeclaration.ModelViewSchema.TableName;
		const string ModelView_Recon = JobDeclaration.ModelViewSchema.TableName_Recon;
		const string ModelViewPK = JobDeclaration.ModelViewSchema.PK;

		#endregion

		internal GenAddOnColumnQueryHelper SimpleQueryHelper => GenAddOnColumnHelper.SimpleQueryHelper;

		#region Release Status

		object Integration.Customs.US.IJobDeclarationFilterBusinessObject.GetReleaseStatusTextQueryWithOperator
		{
			get { return new GetTextQueryWithOperator(GetReleaseStatusQuery); }
		}

		public ZString ReleaseStatusDescription
		{
			get { return DeclarationFilterConstants.ReleaseStatus; }
		}

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		public IList ReleaseStatusList
		{
			get { return Lookups.ReleaseStatusList; }
		}

		#endregion

		#region FDA Message Status

		public ZString FDAMsgStatusDescription
		{
			get { return DeclarationFilterConstants.FDAMsgStatus; }
		}

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		public IList FDAMsgStatusList
		{
			get { return Lookups.FDAMsgStatusList; }
		}

		object Integration.Customs.US.IJobDeclarationFilterBusinessObject.GetFDAMsgStatusTextQueryWithOperator
		{
			get { return new GetTextQueryWithOperator(GetFDAMsgStatusQuery); }
		}

		#endregion

		#region FDA Status

		public ZString FDAStatusDescription
		{
			get { return DeclarationFilterConstants.FDAStatus; }
		}

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		public IList FDAStatusList
		{
			get { return Lookups.FDAStatusList; }
		}

		object Integration.Customs.US.IJobDeclarationFilterBusinessObject.GetFDAStatusTextQueryWithOperator
		{
			get { return new GetTextQueryWithOperator(GetFDAStatusQuery); }
		}

		#endregion
	}
}
