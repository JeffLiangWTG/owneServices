using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Common;
using Enterprise.Customs.Module;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Protest;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ProtestClass = Enterprise.Customs.US.Business.Protest.Protest;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	class ProtestFilterStripBusinessObject : CurrentCompanyJobDeclarationFilterStripBusinessObject, IAccountingFilterStripHolder
	{
		public static class Schema
		{
			public const string DeclarationReference = "Filer Reference #";
			public const string LinkedEntryNumber = "Linked Entry #";
			public const string InternalAdviceNumber = "Internal Advice #";
			public const string LeadProtestNumber = "Lead Protest #";
			public const string ProtestNumber514 = "Protest # 514";
			public const string ProtestNumber520 = "Protest # 520";
			public const string TestSummonsNumber = "Test Summons #";

			public const string Protestant = "Protestant";
			public const string ProtestantType = "Protestant Type";
			public const string TariffActCitation = "Tariff Act Citation";
			public const string FilingDistrictPort = "Filing District Port";
			public const string PeriodBaseDate = "Period Base Date";
			public const string MessageStatus = "Message Status";
			public const string AddressTeam = "Address Team";
			public const string SubstituteDistrictPort = "Substitute District Port";
			public const string SubstituteFilerCode = "Substitute Filer Code";
			public const string RefundParty = "Refund Party";
			public const string FurtherReview = "Further Review";
			public const string AcceleratedDisposition = "Accelerated Disposition";
			public const string Broker = "Broker";
			public const string Branch = "Branch";

			public const string Status = "Protest Status";
			public const string StatusDate = "Protest Status Date";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			#region Number Filters

			result.AddNumberFilter(Schema.DeclarationReference, JobDeclarationSchema.JE_DeclarationReference);
			var protest514 = result.AddNumberFilter(Schema.ProtestNumber514, GetProtestNumber514);
			protest514.MaxLength = USAddInfoSchema.US_P_Assoc514ProtestNo.MaxLength;
			var protest520 = result.AddNumberFilter(Schema.ProtestNumber520, GetPetitionNumber520);
			protest520.MaxLength = USAddInfoSchema.US_P_Assoc520PetitionNo.MaxLength;

			var moduleFilter = GetAddInfoTextFilter(Schema.InternalAdviceNumber, USAddInfoSchema.Constants.US_P_InternalAdviceNo.Substring(3));
			moduleFilter.Category = FilterCategories.NumbersAndReferences;
			result.AddFilter(moduleFilter);

			moduleFilter = GetAddInfoTextFilter(Schema.LeadProtestNumber, USAddInfoSchema.Constants.US_P_LeadProtestNo.Substring(3));
			moduleFilter.Category = FilterCategories.NumbersAndReferences;
			result.AddFilter(moduleFilter);

			moduleFilter = GetAddInfoTextFilter(Schema.TestSummonsNumber, USAddInfoSchema.Constants.US_P_TestSummonsNo.Substring(3));
			moduleFilter.Category = FilterCategories.NumbersAndReferences;
			result.AddFilter(moduleFilter);

			var protestNumberFilter = result.AddNumberFilter(ProtestClass.Schema.ProtestNumber, GetProtestNumberQuery);
			protestNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			protestNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			protestNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;

			var linkedEntryFilter = result.AddNumberFilter(Schema.LinkedEntryNumber, GetLinkedEntryNumberQuery);
			linkedEntryFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			linkedEntryFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			linkedEntryFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			linkedEntryFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			linkedEntryFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			linkedEntryFilter.UseMultiSearch = false;

			#endregion

			#region Status Filters

			result.AddTextFilter(Schema.MessageStatus, JobDeclarationSchema.JE_MessageStatus, ProtestMessageStatuses).Category = FilterCategories.StatusAndFlags;

			AccountingFilterStrip.AddJobManagementFilters(result, Env.Security.USProtest);

			var statusFilter = result.AddTextFilter(Schema.Status, JobDeclarationSchema.JE_EntryStatus, Factory.GetCachedValue<ProtestStatusCodesList>());
			statusFilter.Category = FilterCategories.StatusAndFlags;
			statusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			statusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			statusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			statusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			statusFilter.MaxLength = JobDeclarationSchema.JE_EntryStatus.MaxLength;

			result.AddDateFilter(Schema.StatusDate, GetProtestStatusDate);

			#endregion

			result.AddGuidFilter(Schema.Protestant, ModuleIDs.Organisation, GetProtestantQuery, Organisations);
			result.AddGuidFilter(Schema.RefundParty, ModuleIDs.Organisation, GetRefundPartyQuery, Organisations);

			var protestantType = Factory.GetCachedValue<ProtestantTypeList>();
			var protestantTypeFilter = result.AddTextFilter(Schema.ProtestantType, GetProtestantTypeQuery, protestantType);
			protestantTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			protestantTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			protestantTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			protestantTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			protestantTypeFilter.MaxLength = USAddInfoSchema.US_P_ProtestantType.MaxLength;

			var tariffActCitationFilter = result.AddTextFilter(Schema.TariffActCitation, JobDeclarationSchema.JE_MessageSubType, Factory.GetCachedValue<TariffActCitationList>());
			tariffActCitationFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			tariffActCitationFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			tariffActCitationFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			tariffActCitationFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			tariffActCitationFilter.MaxLength = JobDeclarationSchema.JE_MessageSubType.MaxLength;

			var port = result.AddTextFilter(Schema.FilingDistrictPort, GetFilingDistrictPort);
			port.MaxLength = USAddInfoSchema.US_P_FilingDDPP.MaxLength;
			result.AddDateFilter(Schema.PeriodBaseDate, GetPeriodBaseDate);
			result.AddFilter(GetAddInfoTextFilter(Schema.AddressTeam, USAddInfoSchema.Constants.US_P_AddressTeam.Substring(3)));
			result.AddFilter(GetAddInfoTextFilter(Schema.SubstituteDistrictPort, USAddInfoSchema.Constants.US_P_SubstituteDDPP.Substring(3)));
			result.AddFilter(GetAddInfoTextFilter(Schema.SubstituteFilerCode, USAddInfoSchema.Constants.US_P_SubstituteFilerCode.Substring(3)));
			result.AddFlagsFilter(Schema.FurtherReview, new string[] { Schema.FurtherReview }, new GetFlagsQuery[] { GetFurtherReviewQuery });
			result.AddFlagsFilter(Schema.AcceleratedDisposition, new string[] { Schema.AcceleratedDisposition }, new GetFlagsQuery[] { GetAcceleratedDispositionQuery });
			result.AddGuidFilter(Schema.Branch, ModuleIDs.GlbBranch, JobDeclarationSchema.JE_GB, BranchList).Category = FilterCategories.Organisations;
			var broker = result.AddNkFilter(Schema.Broker, JobDeclarationSchema.JE_GS_NKCusAgent, ModuleIDs.GlbStaff, StaffList);
			broker.Category = FilterCategories.Organisations;
			broker.MaxLength = JobDeclarationSchema.JE_GS_NKCusAgent.MaxLength;

			return result;
		}

		protected GenAddOnColumnQueryHelper SimpleQueryHelper
		{
			get
			{
				if (simpleQueryHelper == null)
				{
					simpleQueryHelper = new GenAddOnColumnQueryHelper(typeof(JobDeclaration), typeof(JobDeclaration), JobDeclarationSchema.PK);
				}
				return simpleQueryHelper;
			}
		}
		GenAddOnColumnQueryHelper simpleQueryHelper;

		ZQuery GetProtestantQuery(ZGuid protestantOrganizationPK)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			var jobDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.PK, protestantOrganizationPK);

			orgAddressSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
			jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, JobDeclarationSchema.Constants.Prefix);
			jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.ProtestantAddress);
			jobDocAddressSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
			result.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetRefundPartyQuery(ZGuid refundPartyOrgPK)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			var jobDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.PK, refundPartyOrgPK);

			orgAddressSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
			jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, JobDeclarationSchema.Constants.Prefix);
			jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.RefundParty);
			jobDocAddressSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
			result.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetProtestantTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GenAddOnColumnHelper.SimpleQueryHelper.GetQueryHandlingBlanks(USAddInfoSchema.Constants.US_P_ProtestantType, comparisonOperator, value);
		}

		ZQuery GetFilingDistrictPort(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GenAddOnColumnHelper.SimpleQueryHelper.GetQueryHandlingBlanks(USAddInfoSchema.Constants.US_P_FilingDDPP, comparisonOperator, value);
		}

		ZQuery GetPeriodBaseDate(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return SimpleQueryHelper.GetQueryWithSubQueryOnGenAddOnColumn(USAddInfoSchema.Constants.US_P_PeriodBaseDate, comparisonOperator, date1, date2);
		}

		ZQuery GetProtestNumber514(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GenAddOnColumnHelper.SimpleQueryHelper.GetQueryHandlingBlanks(USAddInfoSchema.Constants.US_P_Assoc514ProtestNo, comparisonOperator, value);
		}

		ZQuery GetPetitionNumber520(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GenAddOnColumnHelper.SimpleQueryHelper.GetQueryHandlingBlanks(USAddInfoSchema.Constants.US_P_Assoc520PetitionNo, comparisonOperator, value);
		}

		ZQuery GetLinkedEntryNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			var subQuery = new ZDBOnlySubQuery(typeof(Customs.Business.MultiLineAddInfos.CusAddInfo), CusAddInfoSchema.B7_ParentID, comparisonOperator == SQLComparisonOperator.DoesNotStartWith);
			subQuery.AddToFilter(CusAddInfoSchema.B7_Type, Enterprise.Customs.Business.MultiLineAddInfos.CusAddInfoTypeAttribute.Codes.USLinkedEntry);

			var addInfoQuery = new ZQuery();
			var addInfoProperty = USLinkedEntryAddInfo.Schema.US_LE_EntryNumber.Substring(3);
			if (comparisonOperator == SQLComparisonOperator.DoesNotStartWith)
			{
				addInfoQuery.AddToFilter(JoinCondition.And, CusAddInfoSchema.B7_AddInfoData, SQLComparisonOperator.Contains, addInfoProperty + "=" + value);
			}
			else
			{
				addInfoQuery = Enterprise.Customs.Business.AddInfoFilterRepository.GetAddInfoQuery(
					comparisonOperator, value, CusAddInfoSchema.B7_AddInfoData, addInfoProperty);
			}
			subQuery.AddToFilter(addInfoQuery);
			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetProtestNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			var entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, comparisonOperator.IsNegativeSQLOperator());
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.UnitedStates.Protest);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);
			entryNumberQuery.AddToFilter_PossiblyCommaSeparated(CusEntryNumSchema.CE_EntryNum, comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), value);
			result.AddSubQuery(entryNumberQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetProtestStatusDate(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return SimpleQueryHelper.GetQueryWithSubQueryOnGenAddOnColumn(USAddInfoSchema.Constants.US_P_StatusDate, comparisonOperator, date1, date2);
		}

		ModuleTextFilter GetAddInfoTextFilter(ZString description, string addInfoPropertyName)
		{
			return new AddInfoModuleTextFilter(description, JobDeclarationSchema.JE_AddInfo, addInfoPropertyName);
		}

		ZQuery GetFurtherReviewQuery(ZBool showFlaggedOnly)
		{
			var result = new ZQuery();

			if (showFlaggedOnly)
			{
				result = SimpleQueryHelper.GetQueryOnGenAddOnColumn(USAddInfoSchema.Constants.US_P_ApplicationFurtherReview, ZBool.True.ToString());
			}

			return result;
		}

		ZQuery GetAcceleratedDispositionQuery(ZBool showFlaggedOnly)
		{
			var result = new ZQuery();

			if (showFlaggedOnly)
			{
				result = SimpleQueryHelper.GetQueryOnGenAddOnColumn(USAddInfoSchema.Constants.US_P_AcceleratedDispositionInd, ZBool.True.ToString());
			}

			return result;
		}

		#region Overrides

		public override ZQuery Filter
		{
			get
			{
				var baseFilter = base.Filter;
				baseFilter.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_MessageType, JobMessageTypeList.MoreCodes.Protest);

				return baseFilter;
			}
		}

		#endregion

		#region Lookup List

		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public GlbBranchCollection BranchList
		{
			get { return new GlbBranchCollection(Factory); }
		}

		public GlbStaffCollection StaffList
		{
			get { return new GlbStaffCollection(Factory); }
		}

		public ProtestMessageStatusList ProtestMessageStatuses
		{
			get { return Factory.GetCachedValue("ProtestMessageStatusList", delegate { return new ProtestMessageStatusList(); }); }
		}

		#endregion

		GenAddOnColumnHelper GenAddOnColumnHelper
		{
			get { return helper ?? (helper = new GenAddOnColumnHelper()); }
		}
		GenAddOnColumnHelper helper;

		IAccountingFilterStrip AccountingFilterStrip
		{
			get
			{
				if (accountingFilterStrip == null)
				{
					accountingFilterStrip = (IAccountingFilterStrip)Activator.CreateInstance(ObjectFactory.GetType<IAccountingFilterStrip>(), this);
					accountingFilterStrip.Initialize(addOrganisationFilters: false, addDateFilters: false, addAmountFilters: false, addNumbersAndReferencesFilters: false);
				}
				return accountingFilterStrip;
			}
		}
		IAccountingFilterStrip accountingFilterStrip;

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return result;
		}

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new Dictionary<string, object>()
		{
			{ AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride, ResString.GetMultilingualString("CustomsUS|ProtestFilter|JobStatus", "Job Status") }
		};

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride => null;

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride => null;

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories => null;
	}
}
