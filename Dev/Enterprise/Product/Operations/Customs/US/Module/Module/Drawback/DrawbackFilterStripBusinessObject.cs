using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Module;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
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
	class DrawbackFilterStripBusinessObject : JobDeclarationFilterBusinessObject
	{
		public static class Constants
		{
			internal const string AnticipLiquidationDate = "Anticipated Liquidation Date";
			internal const string CoveredToDate = "Period Covered To Date";
			internal const string CoveredFromDate = "Period Covered From Date";
			internal const string EarliestExportDate = "Earliest Export Date";
			internal const string ClaimDate = "Estimated Claim Date";
			internal const string Indicators = "Indicators";
			internal const string ClaimPort = "Claim Port";
			internal const string LicensePort = "License Port";
			internal const string ClaimType = "Drawback Claim Type";
			internal const string Team = "Drawback Team";
			internal const string Status = "Drawback Status";
			internal const string MessageStatus = "Message Status";
			internal const string NaftaDrawbackCountry = "Nafta Drawback Country Code";
			internal const string OwnersReferenceNumber = "Owners Reference Number";
			internal const string Claimant = "Drawback Claimant";
			internal const string ContractNumber = "Contract Number";
			internal const string FilerCode = "Filer Code";
		}

		public override ZQuery Filter
		{
			get
			{
				var baseFilter = base.Filter;
				baseFilter.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Drawback);
				baseFilter.AddToFilter(GetCompanyQuery());

				return baseFilter;
			}
		}

		public CodeDescriptionPairList DrawbackSummaryEntryTypeList => ACEDrawbackProvisionsList.GetDrawbackProvisionList(Factory);

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			AddDrawbackNumberFilters(result);
			AddDrawbackOrganisationFilters(result);
			AddDrawbackTextFilters(result);
			AddDrawbackLocationFilters(result);
			AddDrawbackIndicatorFilters(result);
			AddDrawbackDateFilters(result);
			AddDrawbackInvoiceFilters(result);

			AddCommercialInvoiceAttributeFilter(result);
			AddAuditFilters(result);

			return result;
		}

		void AddDrawbackNumberFilters(ModuleFilterCollection filters)
		{
			AddEntryNumberFilter(filters);
			var filerFilter = filters.AddTextFilter(Constants.FilerCode, GetFilerCodeQuery);
			filerFilter.Category = FilterCategories.NumbersAndReferences;
			filerFilter.MaxLength = USAddInfoSchema.US_EntryFilerCode.MaxLength;

			var contractNumFilter = filters.AddNumberFilter(Constants.ContractNumber, GetContractNumberQuery);
			contractNumFilter.MaxLength = CusCodeDataSchema.CY_Data.MaxLength;
			contractNumFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			contractNumFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
		}

		void AddDrawbackOrganisationFilters(ModuleFilterCollection filters)
		{
			AddBranchAndBrokerFilters(filters);
			filters.AddGuidFilter(Constants.Claimant, ModuleIDs.Organisation, GetClaimantQuery, new OrgHeaderCollection(Factory));
		}

		ZQuery GetFilerCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_EntryFilerCode", comparisonOperator, value);
		}

		ZQuery GetContractNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			var subQuery = new ZDBOnlySubQuery(typeof(Customs.Business.CusCodeData), CusCodeDataSchema.CY_ParentID);
			subQuery.AddToFilter_PossiblyCommaSeparated(CusCodeDataSchema.CY_Data, comparisonOperator, value);
			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetClaimantQuery(ZGuid claimantPK)
		{
			return new ZQuery(JobDeclarationSchema.JE_OH_Importer, claimantPK);
		}

		void AddDrawbackInvoiceFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.InvoiceLineProductCode, GetInvoiceLineProductCodeQuery).MaxLength = JobComInvoiceLineSchema.JI_PartNo.MaxLength;
			filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.InvoiceNumber, JobComInvoiceHeaderSchema.JZ_InvoiceNumber).SubGroup = new EntryLineFilterBusinessObject.UsEntryNumberFilterSubGroup();
			AddInvoiceTotalFilters(filters);
		}

		void AddDrawbackTextFilters(ModuleFilterCollection filters)
		{
			var drawbackFilter = filters.AddTextFilter(Constants.ClaimType, GetClaimTypeQuery, DrawbackSummaryEntryTypeList);
			drawbackFilter.MaxLength = USAddInfoSchema.US_EntryType.MaxLength;

			var uSTeamNoForDrawbackCodeList = AddInfoJobDeclarationLookups.GetUSTeamNoForDrawbackCodeList();
			var teamFilter = filters.AddTextFilter(Constants.Team, GetDrawbackTeamQuery, uSTeamNoForDrawbackCodeList);
			teamFilter.MaxLength = USAddInfoSchema.US_TeamNo.MaxLength;

			var drawbackStatus = filters.AddTextFilter(Constants.Status, GetDrawbackStatusQuery, Factory.GetCachedValue<DrawbackSummaryStatusList>());
			drawbackStatus.Category = FilterCategories.StatusAndFlags;
			drawbackStatus.MaxLength = JobDeclarationSchema.JE_EntryStatus.MaxLength;

			var messageStatus = filters.AddTextFilter(Constants.MessageStatus, GetDrawbackMessageStatusQuery, Factory.GetCachedValue<DrawbackSummaryStatusList>());
			messageStatus.Category = FilterCategories.StatusAndFlags;
			messageStatus.MaxLength = JobDeclarationSchema.JE_MessageStatus.MaxLength;

			var naftaCountryFilter = filters.AddTextFilter(Constants.NaftaDrawbackCountry, GetDrawbackNaftaCountryQuery);
			naftaCountryFilter.Category = FilterCategories.Locations;
			naftaCountryFilter.MaxLength = USAddInfoSchema.US_NAFTADrawbackCountry.MaxLength;

			var ownersRefFilter = filters.AddTextFilter(Constants.OwnersReferenceNumber, GetOwnerRefQuery);
			ownersRefFilter.Category = FilterCategories.NumbersAndReferences;
			ownersRefFilter.MaxLength = JobDeclarationSchema.JE_OwnerRef.MaxLength;

			var entrySummaryActionsFilter = filters.AddTextFilter(DeclarationFilterConstants.EntrySummaryActions, GetEntrySummaryActionsQuery, Lookups.EntrySummaryActionsList);
			SetFilterConstraints(entrySummaryActionsFilter);
			entrySummaryActionsFilter.Category = FilterCategories.StatusAndFlags;
			entrySummaryActionsFilter.DefaultProperty = DeclarationFilterConstants.ALL;
		}

		ZQuery GetClaimTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_EntryType", comparisonOperator, value);
		}

		ZQuery GetDrawbackStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(JobDeclarationSchema.JE_EntryStatus, comparisonOperator, value);
		}

		ZQuery GetDrawbackTeamQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_TeamNo", comparisonOperator, value);
		}

		ZQuery GetDrawbackNaftaCountryQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_NAFTADrawbackCountry", comparisonOperator, value);
		}

		ZQuery GetDrawbackMessageStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(JobDeclarationSchema.JE_MessageStatus, comparisonOperator, value);
		}

		ZQuery GetOwnerRefQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(JobDeclarationSchema.JE_OwnerRef, comparisonOperator, value);
		}

		ZQuery GetEntrySummaryActionsQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Drawback);

			var comparisonOperator = SQLComparisonOperator.Equal;
			if (value == DeclarationFilterConstants.ALL)
			{
				comparisonOperator = SQLComparisonOperator.NotEqual;
				value = ZString.Empty;
			}
			result.AddToFilter(ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.JE_ClusterKey, JobDeclaration.Schema.JE_ClusterKey, ModelViewSchema.TableName, ModelViewSchema.JE_ENSAction, comparisonOperator, value));

			return result;
		}

		void AddDrawbackDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(Constants.ClaimDate, GetClaimDateQuery);
			filters.AddDateFilter(Constants.EarliestExportDate, GetEarliestExportDateQuery);
			filters.AddDateFilter(Constants.CoveredFromDate, GetCoveredFromDateQuery);
			filters.AddDateFilter(Constants.CoveredToDate, GetCoveredToDateQuery);
			filters.AddDateFilter(DeclarationFilterConstants.AnticipLiquidationDate, GetAnticipLiquidationDateQuery);
		}

		ZQuery GetClaimDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return ModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_EstimatedEntryDate", comparisonOperator, date1, date2);
		}

		ZQuery GetEarliestExportDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return ModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_EarliestExportDate", comparisonOperator, date1, date2);
		}

		ZQuery GetCoveredFromDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return ModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_DRWDatePeriodFrom", comparisonOperator, date1, date2);
		}

		ZQuery GetCoveredToDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return ModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_DRWDatePeriodTo", comparisonOperator, date1, date2);
		}

		ZQuery GetAnticipLiquidationDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			return ModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_ALDate", comparisonOperator, startDate, endDate);
		}

		void AddDrawbackLocationFilters(ModuleFilterCollection filters)
		{
			var licensePortFilter = filters.AddNkFilter(Constants.LicensePort, GetLicensePortQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, Lookups.RegionalPorts);
			licensePortFilter.Category = FilterCategories.Locations;
			licensePortFilter.MaxLength = USAddInfoSchema.US_PreparerDistrictPort.MaxLength;

			var usClaimPortCode = AddInfoJobDeclarationLookups.GetUSClaimPortCodeList();
			var claimPortFilter = filters.AddTextFilter(Constants.ClaimPort, GetClaimPortQuery, usClaimPortCode);
			claimPortFilter.Category = FilterCategories.Locations;
			claimPortFilter.MaxLength = USAddInfoSchema.US_ClaimPort.MaxLength;
		}

		ZQuery GetLicensePortQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_PreparerDistrictPort", filterOperator, value);
		}

		ZQuery GetClaimPortQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_ClaimPort", filterOperator, value);
		}

		void AddDrawbackIndicatorFilters(ModuleFilterCollection filters)
		{
			var indicatorsFilter = filters.AddFlagsFilter(
				Constants.Indicators,
				new string[]
				{
					"Exporter Summary Indicator",
					"Pre Inspection Indicator",
					"NAFTA Claim Indicator",
					"Accelerated Claim Indicator",
					"Waiver of Prior Notice Indicator",
					"Petroleum Claim Indicator",
				},
				new GetFlagsQuery[]
				{
					GetExporterSummaryIndQuery,
					GetPreInspectionIndQuery,
					GetNAFTAClaimIndQuery,
					GetAcceleratedClaimIndQuery,
					GetWaiverNoticeIndQuery,
					GetPetroleumClaimIndQuery,
				}
			);

			indicatorsFilter.Category = FilterCategories.StatusAndFlags;

			var disStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.DISStatus, GetDISStatusQuery, Lookups.DISStatusList);
			SetFilterConstraints(disStatusFilter);
		}

		ZQuery GetExporterSummaryIndQuery(ZBool show)
		{
			return show ? ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_ExporterSummaryInd", SQLComparisonOperator.Equal, true) : new ZQuery();
		}

		ZQuery GetPreInspectionIndQuery(ZBool show)
		{
			return show ? ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_PreInspectionInd", SQLComparisonOperator.Equal, true) : new ZQuery();
		}

		ZQuery GetNAFTAClaimIndQuery(ZBool show)
		{
			return show ? ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_NAFTAClaimInd", SQLComparisonOperator.Equal, true) : new ZQuery();
		}

		ZQuery GetAcceleratedClaimIndQuery(ZBool show)
		{
			return show ? ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_AcceleratedClaimInd", SQLComparisonOperator.Equal, true) : new ZQuery();
		}

		ZQuery GetWaiverNoticeIndQuery(ZBool show)
		{
			return show ? ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_WaiverNoticeInd", SQLComparisonOperator.Equal, true) : new ZQuery();
		}

		ZQuery GetPetroleumClaimIndQuery(ZBool show)
		{
			return show ? ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_PetroleumClaimInd", SQLComparisonOperator.Equal, true) : new ZQuery();
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = new List<IFilterStripsHelper>();
			helpers.Add(new WorkflowFilterStripsHelperCustoms(typeof(JobDeclaration), WorkflowDescriptors.DrawBackWorkflowDescriptorCode, Factory));

			return helpers;
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
	}
}
