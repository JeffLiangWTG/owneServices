using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class CommissionAgreementFilterBusinessObject : FilterStripBusinessObject
	{
		public CommissionAgreementFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			filters.AddGuidFilter(FilterDescription.Opportunity, ModuleIDs.Opportunity, OrgCommissionAgreementSchema.CA0_P8, Opportunities)
				.MultilingualDescription = ResString.GetMultilingualString("bdd32422-53e2-4318-9e6c-357258b0d08e", "Opportunity");

			filters.AddGuidFilter(FilterDescription.Customer, ModuleIDs.Organisation, OrgCommissionAgreementSchema.CA0_OH_Customer, Organisations)
				.MultilingualDescription = ResString.GetMultilingualString("7d902093-19f6-44bd-a74c-d43a308a3315", "Customer");

			filters.AddTextFilter(FilterDescription.CommissionStream, OrgCommissionAgreementSchema.CA0_CommissionStream, CommissionStreams)
				.MultilingualDescription = ResString.GetMultilingualString("295e5ba1-c24c-4ff9-a799-7ba055c1256f", "Commission Stream");

			filters.AddTextFilter(FilterDescription.Basis, OrgCommissionAgreementSchema.CA0_CommissionBasis, Basis)
				.MultilingualDescription = ResString.GetMultilingualString("21FA1D9E-85A8-4648-963E-18C43618F449", "Basis");

			filters.AddTextFilter(FilterDescription.TriggerTypes, OrgCommissionAgreementSchema.CA0_CommissionTriggerType, TriggerTypes)
				.MultilingualDescription = ResString.GetMultilingualString("5aa09350-03e1-4193-8160-5de7414c3474", "Effective Trigger");

			var recipientSubGroup = new CommissionAgreementRecipientSubGroup();

			var staffInWolfPackFilter = filters.AddNkFilter(FilterDescription.StaffIsRecipient, OrgCommissionAgreementRecipientSchema.CAR_GS_NKStaff, ModuleIDs.GlbStaff, Staff);
			staffInWolfPackFilter.MultilingualDescription = ResString.GetMultilingualString("06263ae4-1e38-4e8e-86b1-30c9c6b5ac88", "Staff in Wolf Pack");
			staffInWolfPackFilter.SubGroup = recipientSubGroup;

			var organizationInWolfPackFilter = filters.AddGuidFilter(FilterDescription.PartyIsRecipient, ModuleIDs.Organisation, GetOrganisationEntityInWolfPackQuery, Organisations);
			organizationInWolfPackFilter.MultilingualDescription = ResString.GetMultilingualString("89dd711e-7a76-40f8-8e74-e4549a7a28e1", "Organization in Wolf Pack");
			organizationInWolfPackFilter.SubGroup = recipientSubGroup;

			var flagFilter = filters.AddFlagsFilter(FilterDescription.IsApproved,
				new[]
				{
					Res.GetString("6F68C05D-3FF0-4AB1-ACFF-129F40236DD2", "Yes")
				},
				new GetFlagsQuery[] { GetIsApprovedQuery });
			flagFilter.MultilingualDescription = ResString.GetMultilingualString("A2ED4931-EC07-4C33-B3DA-D04AE38ABD0A", "Is Approved");

			filters.AddDateFilter(FilterDescription.LastApprovedDate, OrgCommissionAgreementSchema.CA0_LastApprovedDateUtc, true)
				.MultilingualDescription = ResString.GetMultilingualString("3ebec973-f077-4eb5-9c24-263c7d0743a5", "Last Approved Date");

			var queueFilter = filters.AddFlagsFilter(FilterDescription.IsInCalculationQueue,
				new[]
				{
					Res.GetString("6F68C05D-3FF0-4AB1-ACFF-129F40236DD2", "Yes")
				},
				new GetFlagsQuery[] { GetIsInQueueQuery });

			queueFilter.MultilingualDescription = ResString.GetMultilingualString("E97B0DED-AE21-42CE-9A9B-BECE80F8AE39", "Is in Calculation Queue");

			return filters;
		}

		ZQuery GetIsApprovedQuery(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(OrgCommissionAgreement));
			query.AddToFilter(OrgCommissionAgreementSchema.CA0_LastApprovedDateUtc, value ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal, null);

			var noDraftSubquery = new ZDBOnlySubQuery(typeof(OrgCommissionAgreement), OrgCommissionAgreementSchema.CA0_CA0_ParentVersion, true); //outdated agreements are ones which are referred to - eliminate them
			query.AddSubQuery(noDraftSubquery, JoinCondition.And);

			return query;
		}

		ZQuery GetIsInQueueQuery(ZBool filter)
		{
			var query = new ZDBOnlyQuery(typeof(OrgCommissionAgreement));
			var subQuery = new ZDBOnlySubQuery(typeof(OrgCommissionCalculationQueue), OrgCommissionCalculationQueueSchema.CAQ_CA0, !filter);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		public static ZQuery GetOrganisationEntityInWolfPackQuery(ZGuid value)
		{
			var result = new ZDBOnlyQuery(typeof(OrgCommissionAgreementRecipient));

			result.AddToFilter(JoinCondition.Or, OrgCommissionAgreementRecipientSchema.CAR_OH_Party, value);

			var staffSubquery = new ZDBOnlySubQuery(typeof(GlbStaff), OrgCommissionAgreementRecipientSchema.CAR_GS_NKStaff);
			var staffProxyBranchSubquery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbStaffSchema.GS_GB_HomeBranch);
			var staffProxyCompanySubquery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbBranchSchema.GB_GC);
			var staffProxyOrgSubquery = new ZDBOnlySubQuery(typeof(OrgHeader), GlbCompanySchema.GC_OH_OrgProxy);
			staffProxyOrgSubquery.AddToFilter(OrgHeaderSchema.PK, value);
			staffProxyCompanySubquery.AddSubQuery(staffProxyOrgSubquery, JoinCondition.And);
			staffProxyBranchSubquery.AddSubQuery(staffProxyCompanySubquery, JoinCondition.And);
			staffSubquery.AddSubQuery(staffProxyBranchSubquery, JoinCondition.And);
			result.AddSubQuery(OrgCommissionAgreementRecipientSchema.CAR_GS_NKStaff, GlbStaffSchema.GS_Code, staffSubquery, JoinCondition.Or);

			return result;
		}

		class CommissionAgreementRecipientSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(OrgCommissionAgreement));
				var recipientSubQuery = new ZDBOnlySubQuery(typeof(OrgCommissionAgreementRecipient), OrgCommissionAgreementRecipientSchema.CAR_CA0);
				recipientSubQuery.AddToFilter(filter);
				result.AddSubQuery(recipientSubQuery, JoinCondition.And);

				return result;
			}
		}

		#endregion

		#region Lookups

		#region Commission Streams

		ICodeDescriptionPairList CommissionStreams
		{
			get { return commissionStreams ?? (commissionStreams = CommissionLookups.New(Factory).CommissionStreams_Active); }
		}
		ICodeDescriptionPairList commissionStreams;

		#endregion

		#region Basis

		ICodeDescriptionPairList Basis => basis ?? (basis = CommissionLookups.New(Factory).CommissionBasisType);
		ICodeDescriptionPairList basis;

		#endregion

		#region Trigger Types

		ReadOnlyCodeDescriptionPairList TriggerTypes
		{
			get
			{
				if (triggerTypes == null)
				{
					var types = new CodeDescriptionPairList();
					types.AddRange(CommissionLookups.New(Factory).TriggerTypes);
					types.AddRange(new OrgCommissionAgreementTriggerTypes());

					triggerTypes = types;
				}
				return triggerTypes;
			}
		}
		ReadOnlyCodeDescriptionPairList triggerTypes;

		#endregion

		#region Opportunities

		OrgOpportunityCollection Opportunities
		{
			get
			{
				if (opportunities == null)
				{
					opportunities = new OrgOpportunityCollection(Factory);
				}

				return opportunities;
			}
		}
		OrgOpportunityCollection opportunities;

		#endregion

		#region Organisations

		OrgHeaderCollection Organisations
		{
			get
			{
				if (organisations == null)
				{
					organisations = new OrgHeaderCollection(Factory);
				}

				return organisations;
			}
		}
		OrgHeaderCollection organisations;

		#endregion

		#region Staff

		GlbStaffCollection Staff
		{
			get
			{
				if (staff == null)
				{
					staff = new GlbStaffCollection(Factory);
				}

				return staff;
			}
		}
		GlbStaffCollection staff;

		#endregion

		#endregion

		#region Filter Descriptions

		public static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string Opportunity = "Opportunity";
			public const string Customer = "Customer";
			public const string CommissionStream = "CommissionStream";
			public const string StaffIsRecipient = "StaffIsRecipient";
			public const string PartyIsRecipient = "PartyIsRecipient";
			public const string IsApproved = "IsApproved";
			public const string LastApprovedDate = "LastApprovedDate";
			public const string IsInCalculationQueue = "IsInCalculationQueue";
			public const string Basis = "Basis";
			public const string TriggerTypes = "EffectiveTrigger";

			#endregion
		}

		#endregion
	}
}
