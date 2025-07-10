using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrgMatchApprovalFilterBusinessObject : FilterStripBusinessObject
	{
		public OrgMatchApprovalFilterBusinessObject()
		{
			ReadOnly = !IsCurrentUserSupervisor;
		}

		public virtual bool IsCurrentUserSupervisor
		{
			get { return OrgMatchApproval.IsTheCurrentUserSupervisor; }
		}

		#region Filters

		public override ZQuery Filter
		{
			get
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgMatchApproval));
				ZDBOnlySubQuery addressSubQuery = new ZDBOnlySubQuery(typeof(OrgPatternMatchAddress), OrgMatchApprovalSchema.P2_ParentID);
				ZDBOnlySubQuery stmALogSubQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
				addressSubQuery.AddSubQuery(OrgPatternMatchAddressSchema.P3_ParentID, stmALogSubQuery, JoinCondition.And);
				query.AddSubQuery(addressSubQuery, JoinCondition.And);

				query.AddToFilter(base.Filter);

				AddFlagFilters(query);

				return query;
			}
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			AddTextFilters(filters);
			AddStatusFilters(filters);

			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Master Bill", GetMasterBillQuery);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgMatchApprovalFilter|MasterBill", "Master Bill");
			filter.SubGroup = new MasterBillSubGroup();
		}

		class MasterBillSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgMatchApproval));

				ZDBOnlySubQuery addressSubQuery = new ZDBOnlySubQuery(typeof(OrgPatternMatchAddress), OrgMatchApprovalSchema.P2_ParentID);
				ZDBOnlySubQuery cusHAWBSubQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.AU.ICusHAWB), OrgPatternMatchAddressSchema.P3_ParentID);
				ZDBOnlySubQuery cusMAWBSubQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.AU.ICusMAWB), CusHAWBSchema.CS_CM);

				cusMAWBSubQuery.AddToFilter(filter);

				cusHAWBSubQuery.AddSubQuery(cusMAWBSubQuery, JoinCondition.And);
				addressSubQuery.AddSubQuery(cusHAWBSubQuery, JoinCondition.And);
				result.AddSubQuery(addressSubQuery, JoinCondition.And);

				return result;
			}
		}

		ZQuery GetMasterBillQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZQuery();

			if (!value.IsEmpty)
			{
				ZString masterBillWithoutDashesOrSpaces = value.Replace("-", "").Replace(" ", "");
				result.AddToFilter(CusMAWBSchema.CM_MAWB, comparisonOperator, masterBillWithoutDashesOrSpaces.SubstringSafe(0, CusMAWBSchema.CM_MAWB.MaxLength));
			}

			return result;
		}

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			ModuleTextFilter referenceFilter = new ModuleTextFilter("Tracking Number", GetReferenceQuery);
			referenceFilter.DefaultProperty = ZString.Empty;
			referenceFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgMatchApprovalFilter|TrackingNumber", "Tracking Number");
			return referenceFilter;
		}

		ZQuery GetReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();

			ZString referenceWithoutDashesAndSpaces = value.Replace("-", "").Replace(" ", "");
			if (!referenceWithoutDashesAndSpaces.IsEmpty)
			{
				query.AddToFilter(OrgMatchApprovalSchema.P2_Reference, referenceWithoutDashesAndSpaces);
			}

			return query;
		}

		#endregion

		#region Status

		// change to a single Status filter

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			ModuleFlagsFilter statusFilter = filters.AddFlagsFilter("Status", new string[]
				{
					Res.GetString("MasterFiles|OrgMatchApprovalFilter|UnmatchedForCurrentUser", "Unmatched for Current User"),
					Res.GetString("MasterFiles|OrgMatchApprovalFilter|NoMatchesLogged", "No Matches Logged"),
					Res.GetString("MasterFiles|OrgMatchApprovalFilter|OneMatchCompleted", "One Match Completed"),
					Res.GetString("MasterFiles|OrgMatchApprovalFilter|MatchingCompletedConflict", "Matching Completed - Conflict"),
					Res.GetString("MasterFiles|OrgMatchApprovalFilter|MatchingCompletedNoMatchFound", "Matching Completed - No Match Found")
				}, new GetFlagsQuery[]
				{
					delegate { return new ZQuery(); },
					delegate { return new ZQuery(); },
					delegate { return new ZQuery(); },
					delegate { return new ZQuery(); },
					delegate { return new ZQuery(); }
				});
			statusFilter.DefaultProperties[Res.GetString("MasterFiles|OrgMatchApprovalFilter|UnmatchedForCurrentUser", "Unmatched for Current User")] = true;
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgMatchApprovalFilter|Status", "Status");
		}

		void AddFlagFilters(ZQuery query)
		{
			ZQuery flagsQuery = new ZQuery();

			if (this["Status"] != null)
			{
				if (((ModuleFlagsFilter)this["Status"]).Property0)
				{
					flagsQuery.AddToFilter(GetUnmatchedQuery, JoinCondition.Or);
				}
				if (((ModuleFlagsFilter)this["Status"]).Property1)
				{
					flagsQuery.AddToFilter(GetNoMatchesQuery, JoinCondition.Or);
				}
				if (((ModuleFlagsFilter)this["Status"]).Property2)
				{
					flagsQuery.AddToFilter(GetOneMatchQuery, JoinCondition.Or);
				}
				if (((ModuleFlagsFilter)this["Status"]).Property3)
				{
					flagsQuery.AddToFilter(GetMatchingConflictQuery, JoinCondition.Or);
				}
				if (((ModuleFlagsFilter)this["Status"]).Property4)
				{
					flagsQuery.AddToFilter(GetMatchingNoMatchFound, JoinCondition.Or);
				}
			}

			query.AddToFilter(flagsQuery);
		}

		ZQuery GetUnmatchedQuery
		{
			get
			{
				ZQuery notApproved = new ZQuery();
				notApproved.AddToFilter(JoinCondition.Or, OrgMatchApprovalSchema.P2_MatchUser1, SQLComparisonOperator.Equal, ZString.Empty);
				notApproved.AddToFilter(JoinCondition.Or, OrgMatchApprovalSchema.P2_MatchUser2, SQLComparisonOperator.Equal, ZString.Empty);

				ZQuery result = new ZQuery();
				result.AddToFilter(JoinCondition.And, OrgMatchApprovalSchema.P2_MatchUser1, SQLComparisonOperator.NotEqual, GlbStaff.CurrentUser.GS_Code);
				result.AddToFilter(JoinCondition.And, OrgMatchApprovalSchema.P2_MatchUser2, SQLComparisonOperator.NotEqual, GlbStaff.CurrentUser.GS_Code);
				result.AddToFilter(notApproved, JoinCondition.And);

				return result;
			}
		}

		ZQuery GetNoMatchesQuery
		{
			get
			{
				ZQuery query = new ZQuery();

				query.AddToFilter(OrgMatchApprovalSchema.P2_MatchUser1, SQLComparisonOperator.Equal, ZString.Empty);
				query.AddToFilter(JoinCondition.And, OrgMatchApprovalSchema.P2_MatchUser2, SQLComparisonOperator.Equal, ZString.Empty);
				query.AddToFilter(JoinCondition.And, OrgMatchApprovalSchema.P2_OH_MatchOrg1, SQLComparisonOperator.Equal, null);
				query.AddToFilter(JoinCondition.And, OrgMatchApprovalSchema.P2_OH_MatchOrg2, SQLComparisonOperator.Equal, null);

				return query;
			}
		}

		ZQuery GetOneMatchQuery
		{
			get
			{
				ZQuery result1 = new ZQuery();
				result1.AddToFilter(OrgMatchApprovalSchema.P2_MatchUser1, SQLComparisonOperator.NotEqual, ZString.Empty);
				result1.AddToFilter(JoinCondition.Or, OrgMatchApprovalSchema.P2_MatchUser2, SQLComparisonOperator.NotEqual, ZString.Empty);
				ZQuery result2 = new ZQuery();
				result2.AddToFilter(OrgMatchApprovalSchema.P2_MatchUser1, SQLComparisonOperator.Equal, ZString.Empty);
				result2.AddToFilter(JoinCondition.Or, OrgMatchApprovalSchema.P2_MatchUser2, SQLComparisonOperator.Equal, ZString.Empty);

				ZQuery result = new ZQuery();
				result.AddToFilter(result1);
				result.AddToFilter(result2, JoinCondition.And);

				return result;
			}
		}

		ZQuery GetMatchingConflictQuery
		{
			get
			{
				ZQuery matchCompletedQuery = new ZQuery();

				matchCompletedQuery.AddToFilter(JoinCondition.And, OrgMatchApprovalSchema.P2_MatchUser1, SQLComparisonOperator.NotEqual, string.Empty);
				matchCompletedQuery.AddToFilter(JoinCondition.And, OrgMatchApprovalSchema.P2_MatchUser2, SQLComparisonOperator.NotEqual, string.Empty);

				ZQuery orgDiffQuery = new ZQuery();
				orgDiffQuery.AddToFilter(OrgMatchApprovalSchema.P2_OH_MatchOrg1, SQLComparisonOperator.NotEqual, OrgMatchApprovalSchema.P2_OH_MatchOrg2);

				ZQuery user1Subquery = new ZQuery();
				user1Subquery.AddToFilter(OrgMatchApprovalSchema.P2_OH_MatchOrg1, SQLComparisonOperator.Equal, null);
				user1Subquery.AddToFilter(JoinCondition.And, OrgMatchApprovalSchema.P2_OH_MatchOrg2, SQLComparisonOperator.NotEqual, null);

				ZQuery user2Subquery = new ZQuery();
				user2Subquery.AddToFilter(OrgMatchApprovalSchema.P2_OH_MatchOrg2, SQLComparisonOperator.Equal, null);
				user2Subquery.AddToFilter(JoinCondition.And, OrgMatchApprovalSchema.P2_OH_MatchOrg1, SQLComparisonOperator.NotEqual, null);

				orgDiffQuery.AddToFilter(user1Subquery, JoinCondition.Or);
				orgDiffQuery.AddToFilter(user2Subquery, JoinCondition.Or);

				matchCompletedQuery.AddToFilter(orgDiffQuery, JoinCondition.And);

				return matchCompletedQuery;
			}
		}

		ZQuery GetMatchingNoMatchFound
		{
			get
			{
				ZQuery result1 = new ZQuery();
				result1.AddToFilter(OrgMatchApprovalSchema.P2_MatchUser1, SQLComparisonOperator.NotEqual, ZString.Empty);
				result1.AddToFilter(JoinCondition.And, OrgMatchApprovalSchema.P2_OH_MatchOrg1, SQLComparisonOperator.Equal, null);
				ZQuery result2 = new ZQuery();
				result2.AddToFilter(OrgMatchApprovalSchema.P2_MatchUser2, SQLComparisonOperator.NotEqual, ZString.Empty);
				result2.AddToFilter(JoinCondition.And, OrgMatchApprovalSchema.P2_OH_MatchOrg2, SQLComparisonOperator.Equal, null);

				ZQuery result = new ZQuery();
				result.AddToFilter(result1);
				result.AddToFilter(result2, JoinCondition.And);

				return result;
			}
		}

		#endregion

		#endregion

		#region RevertFilterToUnmatchedByCurrentUser / IsUnmatchForCurrentUserOnly

		public void RevertFilterToUnmatchedByCurrentUser()
		{
			((ModuleTextFilter)this[Res.GetString("MasterFiles|OrgMatchApprovalFilter|MasterBill", "Master Bill")]).Property = "";
			((ModuleTextFilter)this[Res.GetString("MasterFiles|OrgMatchApprovalFilter|TrackingNumber", "Tracking Number")]).Property = "";
			((ModuleFlagsFilter)this[Res.GetString("MasterFiles|OrgMatchApprovalFilter|Status", "Status")]).Property0 = true;
			((ModuleFlagsFilter)this[Res.GetString("MasterFiles|OrgMatchApprovalFilter|Status", "Status")]).Property1 = false;
			((ModuleFlagsFilter)this[Res.GetString("MasterFiles|OrgMatchApprovalFilter|Status", "Status")]).Property2 = false;
			((ModuleFlagsFilter)this[Res.GetString("MasterFiles|OrgMatchApprovalFilter|Status", "Status")]).Property3 = false;
			((ModuleFlagsFilter)this[Res.GetString("MasterFiles|OrgMatchApprovalFilter|Status", "Status")]).Property4 = false;
		}

		public bool IsUnmatchForCurrentUserOnly
		{
			get
			{
				bool result = true;

				if (this["Master Bill"] != null && ((ModuleTextFilter)this["Master Bill"]).Property != ZString.Empty)
				{
					result = false;
				}
				else if (this["Tracking Number"] != null && ((ModuleTextFilter)this["Tracking Number"]).Property != ZString.Empty)
				{
					result = false;
				}
				else if (this["Status"] != null)
				{
					if (!((ModuleFlagsFilter)this["Status"]).Property0)
					{
						result = false;
					}
					if (((ModuleFlagsFilter)this["Status"]).Property1)
					{
						result = false;
					}
					if (((ModuleFlagsFilter)this["Status"]).Property2)
					{
						result = false;
					}
					if (((ModuleFlagsFilter)this["Status"]).Property3)
					{
						result = false;
					}
					if (((ModuleFlagsFilter)this["Status"]).Property4)
					{
						result = false;
					}
				}

				return result;
			}
		}

		#endregion
	}
}
