using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public abstract class CRMSecurityProvider<T> where T : BusinessObject
	{
		#region Abstract / Virtual Methods

		public abstract CRMSecurity CRMSecurity { get; }
		protected abstract IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns { get; }
		protected internal abstract IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns { get; }
		protected abstract IEnumerable<SchemaColumn> RelatedGlbStaffColumns { get; }

		protected abstract bool ShouldCheckJobHeader { get; }

		/// <summary>
		/// When checking the job header there is a check that the JobHeader's
		/// JH_OA_LocalChargesAddr belongs to an organisation that the current
		/// user has rights to access.
		///
		/// When <see cref="AllowNullLocalAddress"/> is True, then the above
		/// mentioned check will also pass if the JH_OA_LocalChargesAddr in the
		/// JobHeader is null.
		///
		/// When false, it will not allow null JH_OA_LocalChargesAddr.
		/// </summary>
		protected virtual bool AllowNullLocalAddress { get; }

		protected virtual void AppendAdditionalJobHeaderQuery(ZDBOnlySubQuery jobHeaderQuery) { }
		protected virtual void AppendAdditionalOrgQuery(ZDBOnlyQuery mainQuery, ZDBOnlySubQuery orgSecurityQuery, bool notIn, JoinCondition joinCondition) { }
		protected virtual void AppendAdditionalStaffPKQuery(ZDBOnlyQuery mainQuery, ZDBOnlySubQuery staffPKSecurityQuery, bool notIn, JoinCondition joinCondition) { }
		protected virtual void AppendAdditionalStaffNKQuery(ZDBOnlyQuery mainQuery, ZDBOnlySubQuery staffNKSecurityQuery, bool notIn, JoinCondition joinCondition) { }
		protected virtual IEnumerable<ZString> RelatedJobDocAddressTypes { get { return Enumerable.Empty<ZString>(); } }
		public virtual Tuple<Type, SchemaGuidColumn> SecurityTargetObjectReference => null;
		public virtual CodePairRegistryItem OSMGSecurityLevelRegistryItem => null;
		public virtual bool ShouldReturnEmptyOrgAddress => true;

		#endregion

		#region Security Checkpoint

		public SecurityCheckpoint GetSecurityCheckpoint(T targetBizObj, FormAction formAction, SecurityCheckpoint defaultCheckpoint)
		{
			if (targetBizObj == null)
			{
				return defaultCheckpoint;
			}

			var newFactory = new BusinessObjectFactory();
			var loginStaff = newFactory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			var bizObj = newFactory.Load<T>(targetBizObj.PK);

			if (loginStaff == null || bizObj == null)
			{
				return defaultCheckpoint;
			}

			if (formAction != FormAction.View)
			{
				var editCheckpoint = GetSecurityCheckpointForEdit(bizObj, loginStaff);
				if (editCheckpoint != null)
				{
					if (editCheckpoint != Env.Security.None)
					{
						return editCheckpoint;
					}

					if (formAction == FormAction.Edit)
					{
						if (defaultCheckpoint == null || !defaultCheckpoint.IsAllowed)
						{
							defaultCheckpoint = Env.Security.None;
						}
					}
				}
			}

			if (SecurityDetails.Any())
			{
				var query = new ZQuery(targetBizObj.PKSchemaColumn, targetBizObj.PK);
				var denied = SecurityDetails.FirstOrDefault(t => !newFactory.Exists(typeof(T), new ZQuery(query, t.GetFilterQuery(loginStaff.GS_Code))));

				if (denied != null)
				{
					var granted = SecurityDetails.FirstOrDefault(t => newFactory.Exists(typeof(T), new ZQuery(query, t.GetFilterQuery(loginStaff.GS_Code))));
					return granted != null ? defaultCheckpoint : denied.Checkpoint;
				}
			}

			return defaultCheckpoint;
		}

		SecurityCheckpoint GetSecurityCheckpointForEdit(T bizObj, GlbStaff loginStaff)
		{
			if (CRMSecurity.EditByStaffRoleAssignedLookup != null && CRMSecurity.EditByStaffRoleAssignedLookup.Any())
			{
				var staffRoleCheckpoints = GetRelatedOrgHeaders(bizObj).SelectMany(org => GetStaffRoleCheckpoints(org, loginStaff));

				if (staffRoleCheckpoints.Any(x => x.IsAllowed))
				{
					return Env.Security.None;
				}

				SecurityCheckpoint salesRepCheckpoint = null;

				if (CRMSecurity.EditByStaffRoleAssignedLookup.TryGetValue(StaffAssignmentRoles.Codes.SalesRep, out salesRepCheckpoint) && salesRepCheckpoint != null)
				{
					if (!GetRelatedGlbStaffs(bizObj).Any(x => x.PK == loginStaff.PK))
					{
						salesRepCheckpoint = null;
					}
					else if (salesRepCheckpoint.IsAllowed)
					{
						return Env.Security.None;
					}
				}

				var deniedCheckpoint = salesRepCheckpoint ?? staffRoleCheckpoints.FirstOrDefault();
				if (deniedCheckpoint != null)
				{
					return deniedCheckpoint;
				}
			}

			if (CRMSecurity.EditByStaffNotAssigned != null)
			{
				return CRMSecurity.EditByStaffNotAssigned.IsAllowed ? Env.Security.None : CRMSecurity.EditByStaffNotAssigned;
			}

			return null;
		}

		IEnumerable<SecurityCheckpoint> GetStaffRoleCheckpoints(OrgHeader org, GlbStaff loginStaff)
		{
			var assignments = org.StaffAssignments.OfType<OrgStaffAssignments>().Where(x => x.O8_GS_NKPersonResponsible == loginStaff.GS_Code);
			return CRMSecurity.EditByStaffRoleAssignedLookup.Where(x => assignments.Any(a => a.O8_Role == x.Key)).Select(x => x.Value).ToArray();
		}

		class SecurityDetail
		{
			public SecurityCheckpoint Checkpoint { get; set; }
			public GetNkQuery GetFilterQuery { get; set; }
			public ZString Description { get; set; }
			public MultilingualString Multilingual { get; set; }
			public bool IsApplicable { get; set; }
		}
		List<SecurityDetail> cachedSecurityDetails;
		List<SecurityDetail> SecurityDetails
		{
			get
			{
				if (cachedSecurityDetails == null)
				{
					cachedSecurityDetails = new List<SecurityDetail>()
					{
						new SecurityDetail
						{
							Checkpoint = CRMSecurity.ViewByStaffNotAssigned,
							GetFilterQuery = GetStaffAssignmentFilterQuery,
							Description = (NoResString)"Assignment and Ownership Security",
							Multilingual = ResString.GetMultilingualString("MasterFiles|CRMSecurityFilter|AssignmentAndOwnershipSecurity", "Assignment and Ownership Security"),
							IsApplicable = !IsEnhancedSecurityLevel
						},
						new SecurityDetail
						{
							Checkpoint = CRMSecurity.IgnoreTaskAssignment,
							GetFilterQuery = GetTaskCollaborationSecurityFilterQuery,
							Description = (NoResString)"Task Collaboration Security",
							Multilingual = ResString.GetMultilingualString("MasterFiles|CRMSecurityFilter|TaskCollaborationSecurity", "Task Collaboration Security"),
							IsApplicable = !IsEnhancedSecurityLevel
						},
						new SecurityDetail
						{
							Checkpoint = CRMSecurity.IgnoreOSMG,
							GetFilterQuery = GetOSMGFilterQuery,
							Description = (NoResString)"Org. Security Group Security",
							Multilingual = ResString.GetMultilingualString("MasterFiles|CRMSecurityFilter|OrgSecurityGroupSecurity", "Org. Security Group"),
							IsApplicable = true
						}
					}.Where(t => t.IsApplicable && t.Checkpoint != null && !t.Checkpoint.IsAllowed).ToList();
				}
				return cachedSecurityDetails;
			}
		}

		public bool HasRestrictions => SecurityDetails.Any();

		public IEnumerable<MultilingualString> DeniedSecurityCheckpointsPaths => SecurityDetails.Select(securityDetail => securityDetail.Checkpoint.DisplayTextPathToSecurityRight);

		#endregion

		#region Security Filter Strips

		public void AddCRMSecurityFilterStrips(BusinessObjectFactory factory, ModuleFilterCollection filters)
		{
			var staffCollection = new GlbStaffCollection(factory);

			foreach (var detail in SecurityDetails)
			{
				var filter = filters.AddNkFilter(detail.Description, detail.GetFilterQuery, ModuleIDs.GlbStaff, staffCollection);

				filter.Category = FilterCategories.CRMSecurity;
				filter.MultilingualDescription = detail.Multilingual;
				filter.IsPublishedOnWeb = false;
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.CurrentUser;

				SetMandatorySecurityFilter(filter);
			}
		}

		#endregion

		#region Implementation

		List<ZGuid> GetPKs(IEnumerable<SchemaGuidColumn> columns, T businessObject)
		{
			return columns.Select(x => businessObject[x])
				.Where(x => x is ZGuid).Select(x => (ZGuid)x).Where(x => x.IsValid).ToList();
		}

		ZDBOnlySubQuery TargetObjectSubQuery(ZDBOnlySubQuery query)
		{
			var target = SecurityTargetObjectReference;
			if (target == null)
			{
				return query;
			}
			var type = typeof(T);
			var pk = (new ZDBOnlyQuery(type)).PKColumn;
			var targetQuery = new ZDBOnlySubQuery(type, pk);
			targetQuery.AddSubQuery(target.Item2, query, JoinCondition.And);
			return targetQuery;
		}

		protected virtual IEnumerable<OrgHeader> GetRelatedOrgHeaders(T businessObject)
		{
			var orgHeaderPKs = GetPKs(RelatedOrgHeaderColumns, businessObject);
			var orgAddressPKs = GetPKs(RelatedOrgAddressColumns, businessObject);

			if (ShouldCheckJobHeader)
			{
				var jobHeader = GetJobHeader(businessObject);

				if (jobHeader != null && jobHeader.JH_OA_LocalChargesAddr.IsValid)
				{
					orgAddressPKs.Add(jobHeader.JH_OA_LocalChargesAddr);
				}
			}

			if (RelatedJobDocAddressTypes.Any())
			{
				var target = SecurityTargetObjectReference;
				var query = new ZQuery(JobDocAddressSchema.E2_ParentID, target != null ? businessObject[target.Item2] : businessObject.PK);
				query.AddToFilter(JobDocAddressSchema.E2_AddressType, RelatedJobDocAddressTypes.ToArray());
				query.AddToFilter(JobDocAddressSchema.E2_OA_Address, SQLComparisonOperator.NotEqual, null);
				orgAddressPKs.AddRange(businessObject.Factory.Load<JobDocAddress>(query).Select(x => x.E2_OA_Address));
			}

			if (orgAddressPKs.Any())
			{
				var orgAddresses = businessObject.Factory.Load<OrgAddress>(new ZQuery(OrgAddressSchema.PK, orgAddressPKs.Distinct()));
				orgHeaderPKs.AddRange(orgAddresses.Select(x => x.OA_OH));
			}

			var orgs = orgHeaderPKs.Any()
				? businessObject.Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, orgHeaderPKs.Distinct()))
				: Enumerable.Empty<OrgHeader>();

			foreach (var org in orgs)
			{
				org.StaffAssignments.CompanySpecific = false;
			}

			return orgs;
		}

		protected virtual IEnumerable<GlbStaff> GetRelatedGlbStaffs(T businessObject)
		{
			var glbStaffPKs = RelatedGlbStaffColumns.Select(x => businessObject[x])
				.Where(x => x is ZGuid).Select(x => (ZGuid)x).Where(x => x.IsValid);

			var glbStaffCodes = RelatedGlbStaffColumns.Select(x => businessObject[x])
				.Where(x => x is ZString).Select(x => (ZString)x).Where(x => !x.IsEmpty).ToList();

			if (ShouldCheckJobHeader)
			{
				var jobHeader = GetJobHeader(businessObject);

				if (jobHeader != null && !jobHeader.JH_GS_NKRepSales.IsEmpty)
				{
					glbStaffCodes.Add(jobHeader.JH_GS_NKRepSales);
				}
			}

			var query = new ZQuery(GlbStaffSchema.PK, glbStaffPKs.Distinct());
			query.AddToFilter(JoinCondition.Or, GlbStaffSchema.GS_Code, glbStaffCodes.Distinct());

			return businessObject.Factory.Load<GlbStaff>(query);
		}

		ZDBOnlyQuery GetStaffAssignmentFilterQuery(ZString staffCode)
		{
			var query = new ZDBOnlyQuery(typeof(T));

			if (IsEnhancedSecurityLevel)
			{
				return query;
			}

			var staff = new BusinessObjectFactory().LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffCode);

			var topSubQuery = new ZDBOnlySubQuery(typeof(T), query.PKColumn);
			topSubQuery.AddSubQuery(GetStaffAssignmentQuery(staff, query.PKColumn), JoinCondition.And);

			if (CRMSecurity.IgnoreOSMG != null && !CRMSecurity.IgnoreOSMG.IsAllowed)
			{
				var osmgSubQuery1 = new ZDBOnlySubQuery(typeof(T), query.PKColumn);
				osmgSubQuery1.IgnoreActiveFilter = true;
				osmgSubQuery1.AddToFilter(GetOSMGQuery(staff, query.PKColumn), JoinCondition.And);

				var osmgSubQuery = new ZDBOnlySubQuery(typeof(T), query.PKColumn);
				osmgSubQuery.IgnoreActiveFilter = true;
				osmgSubQuery.AddToFilter(query.PKColumn, ZGuid.Empty);
				osmgSubQuery.AddAsUnionQuery(osmgSubQuery1, true);

				var subQuery = new ZDBOnlySubQuery(typeof(T), query.PKColumn);
				subQuery.IgnoreActiveFilter = true;
				subQuery.AddSubQuery(osmgSubQuery, JoinCondition.And);

				topSubQuery.AddAsUnionQuery(subQuery, true);
			}

			query.AddSubQuery(topSubQuery, JoinCondition.And);

			return query;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		ZDBOnlySubQuery GetStaffAssignmentQuery(GlbStaff staff, SchemaColumn pkColumn)
		{
			var query = new ZDBOnlySubQuery(typeof(T), pkColumn);
			query.IgnoreActiveFilter = true;

			if (staff == null)
			{
				return query;
			}

			var orgColumnQuery = new ZDBOnlySubQuery(typeof(T), pkColumn);
			foreach (var orgColumn in RelatedOrgHeaderColumns)
			{
				var staffAssignmentsQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.PK);
				staffAssignmentsQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, staff.GS_Code);
				orgColumnQuery.AddSubQuery(orgColumn, OrgStaffAssignmentsSchema.O8_OH, staffAssignmentsQuery, JoinCondition.Or);
			}

			var addressColumnQuery = new ZDBOnlySubQuery(typeof(T), pkColumn);
			foreach (var orgAddressColumn in RelatedOrgAddressColumns)
			{
				var staffAssignmentsQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH);
				staffAssignmentsQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, staff.GS_Code);
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				orgAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, OrgStaffAssignmentsSchema.O8_OH, staffAssignmentsQuery, JoinCondition.And);
				addressColumnQuery.AddSubQuery(orgAddressColumn, orgAddressQuery, JoinCondition.Or);
			}

			var staffColumnQuery = new ZDBOnlySubQuery(typeof(T), pkColumn);
			foreach (var staffColumn in RelatedGlbStaffColumns)
			{
				if (staffColumn is SchemaGuidColumn)
				{
					staffColumnQuery.AddToFilter(JoinCondition.Or, staffColumn, staff.PK);
				}
				else
				{
					staffColumnQuery.AddToFilter(JoinCondition.Or, staffColumn, staff.GS_Code);
				}
			}

			var jobHeaderColumnQuery = new ZDBOnlySubQuery(typeof(T), pkColumn);
			if (ShouldCheckJobHeader)
			{
				var jobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
				jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

				var subQuery = new ZDBOnlyQuery(typeof(JobHeader));
				subQuery.AddToFilter(JoinCondition.Or, JobHeaderSchema.JH_GS_NKRepSales, staff.GS_Code);

				var staffAssignmentsQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH);
				staffAssignmentsQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, staff.GS_Code);
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				orgAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, OrgStaffAssignmentsSchema.O8_OH, staffAssignmentsQuery, JoinCondition.And);

				subQuery.AddSubQuery(JobHeaderSchema.JH_OA_LocalChargesAddr, orgAddressQuery, JoinCondition.Or);
				jobHeaderQuery.AddToFilter(subQuery, JoinCondition.And);
				AppendAdditionalJobHeaderQuery(jobHeaderQuery);

				jobHeaderColumnQuery.AddSubQuery(TargetObjectSubQuery(jobHeaderQuery), JoinCondition.Or);
			}

			var orgStaffAssignmentsQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH);
			orgStaffAssignmentsQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, staff.GS_Code);

			var orgSecurityQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgSecurityQuery.AddSubQuery(OrgHeaderSchema.PK, orgStaffAssignmentsQuery, JoinCondition.And);

			var staffPKSecurityQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.PK);
			staffPKSecurityQuery.AddToFilter(GlbStaffSchema.PK, staff.PK);

			var staffNKSecurityQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
			staffNKSecurityQuery.AddToFilter(GlbStaffSchema.GS_Code, staff.GS_Code);

			var jobDocAddressColumnQuery = new ZDBOnlySubQuery(typeof(T), pkColumn);
			if (RelatedJobDocAddressTypes.Any())
			{
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				orgAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, OrgStaffAssignmentsSchema.O8_OH, orgStaffAssignmentsQuery, JoinCondition.And);

				var jobDocAddrQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
				jobDocAddrQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, RelatedJobDocAddressTypes.ToArray());
				jobDocAddrQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_OA_Address, SQLComparisonOperator.NotEqual, null);
				jobDocAddrQuery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressQuery, JoinCondition.And);

				jobDocAddressColumnQuery.AddSubQuery(TargetObjectSubQuery(jobDocAddrQuery), JoinCondition.Or);
			}

			query.AddToFilter(pkColumn, ZGuid.Empty);
			var subQueries = new ZDBOnlySubQuery[] { orgColumnQuery, addressColumnQuery, staffColumnQuery, jobHeaderColumnQuery, jobDocAddressColumnQuery }.Where(x => !x.IsEmpty);
			if (subQueries.Any())
			{
				foreach (var subQuery in subQueries)
				{
					query.AddAsUnionQuery(subQuery, true);
				}
			}

			AppendAdditionalOrgQuery(query, orgSecurityQuery, notIn: false, joinCondition: JoinCondition.Or);
			AppendAdditionalStaffPKQuery(query, staffPKSecurityQuery, notIn: false, joinCondition: JoinCondition.Or);
			AppendAdditionalStaffNKQuery(query, staffNKSecurityQuery, notIn: false, joinCondition: JoinCondition.Or);

			return query;
		}

		ZDBOnlyQuery GetTaskCollaborationSecurityFilterQuery(ZString staffCode)
		{
			var businessObjectType = typeof(T);
			var query = new ZDBOnlyQuery(businessObjectType);
			if (IsEnhancedSecurityLevel)
			{
				return query;
			}

			var staff = new BusinessObjectFactory().LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffCode);
			if (staff == null)
			{
				return query;
			}

			var capabilities = staff.Capabilities.Select(x => x.PK).ToArray();

			var taskQuery = new ZQuery();
			taskQuery.AddToFilter(JoinCondition.Or, ProcessTasksSchema.P9_GS_NKAssignedStaffMember, staff.GS_Code);
			taskQuery.AddToFilter(JoinCondition.Or, ProcessTasksSchema.P9_G4_RequiredCapability, capabilities);

			var taskCompanyQuery = new ZQuery();
			taskCompanyQuery.AddToFilter(JoinCondition.Or, ProcessTasksSchema.P9_GC, Env.CurrentCompanyPK);
			taskCompanyQuery.AddToFilter(JoinCondition.Or, ProcessTasksSchema.P9_RespondToCascadedEvents, ZBool.True);

			var taskSubQuery = new ZDBOnlySubQuery(typeof(ProcessTasks), ProcessTasksSchema.P9_ParentID);
			taskSubQuery.AddToFilter(taskQuery, JoinCondition.And);
			taskSubQuery.AddToFilter(taskCompanyQuery, JoinCondition.And);

			query.AddSubQuery(TargetObjectSubQuery(taskSubQuery), JoinCondition.And);

			return query;
		}

		JobHeader GetJobHeader(T businessObject)
		{
			var target = SecurityTargetObjectReference;
			var query = new ZQuery(JobHeaderSchema.JH_ParentID, target != null ? businessObject[target.Item2] : businessObject.PK);
			query.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			return businessObject.Factory.Load<JobHeader>(query).FirstOrDefault();
		}

		ZDBOnlyQuery GetOSMGFilterQuery(ZString staffCode)
		{
			var query = new ZDBOnlyQuery(typeof(T));

			var staff = new BusinessObjectFactory().LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffCode);
			if (staff == null)
			{
				return query;
			}

			var topSubQuery = new ZDBOnlySubQuery(typeof(T), query.PKColumn);
			topSubQuery.IgnoreActiveFilter = true;
			topSubQuery.AddToFilter(GetOSMGQuery(staff, query.PKColumn), JoinCondition.And);

			if (!IsEnhancedSecurityLevel && CRMSecurity.ViewByStaffNotAssigned != null && !CRMSecurity.ViewByStaffNotAssigned.IsAllowed)
			{
				var staffSubQuery1 = new ZDBOnlySubQuery(typeof(T), query.PKColumn);
				staffSubQuery1.IgnoreActiveFilter = true;
				staffSubQuery1.AddSubQuery(GetStaffAssignmentQuery(staff, query.PKColumn), JoinCondition.And);

				var staffSubQuery = new ZDBOnlySubQuery(typeof(T), query.PKColumn);
				staffSubQuery.IgnoreActiveFilter = true;
				staffSubQuery.AddToFilter(query.PKColumn, ZGuid.Empty);
				staffSubQuery.AddAsUnionQuery(staffSubQuery1, true);

				var subQuery = new ZDBOnlySubQuery(typeof(T), query.PKColumn);
				subQuery.IgnoreActiveFilter = true;
				subQuery.AddSubQuery(staffSubQuery, JoinCondition.And);

				topSubQuery.AddAsUnionQuery(subQuery, true);
			}

			query.AddSubQuery(topSubQuery, JoinCondition.And);

			return query;
		}

		ZDBOnlyQuery GetOSMGQuery(GlbStaff staff, SchemaColumn pkColumn)
		{
			if (IsEnhancedSecurityLevel)
			{
				return GetOSMGQueryEnhancedSecurityLevel(staff, pkColumn);
			}

			var userGroupSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GG);
			userGroupSubQuery.AddToFilter(GlbGroupLinkSchema.GK_GS, staff.PK);

			var orgSecurityQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
			orgSecurityQuery.AddSubQuery(OrgMiscServSchema.OM_GG_OrgSecurityGroup, userGroupSubQuery, JoinCondition.And);

			var orgColumnQuery = new ZDBOnlySubQuery(typeof(T), pkColumn);
			foreach (var orgColumn in RelatedOrgHeaderColumns)
			{
				orgColumnQuery.AddSubQuery(orgColumn, orgSecurityQuery, JoinCondition.Or);
			}

			var addressColumnQuery = new ZDBOnlySubQuery(typeof(T), pkColumn);
			foreach (var orgAddressColumn in RelatedOrgAddressColumns)
			{
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				orgAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, orgSecurityQuery, JoinCondition.And);
				addressColumnQuery.AddSubQuery(orgAddressColumn, orgAddressQuery, JoinCondition.Or);

				if (ShouldReturnEmptyOrgAddress && orgAddressColumn.IsNullable)
				{
					var noOrgAddressQuery = new ZDBOnlySubQuery(typeof(T), pkColumn);
					noOrgAddressQuery.AddToFilter(orgAddressColumn, DBNull.Value);
					addressColumnQuery.AddAsUnionQuery(noOrgAddressQuery, true);
				}
			}

			var jobDocAddressColumnQuery = new ZDBOnlySubQuery(typeof(T), pkColumn);
			if (RelatedJobDocAddressTypes.Any())
			{
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				orgAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, orgSecurityQuery, JoinCondition.And);

				var jobDocAddrQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
				jobDocAddrQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, RelatedJobDocAddressTypes.ToArray());
				jobDocAddrQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_OA_Address, SQLComparisonOperator.NotEqual, null);
				jobDocAddrQuery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressQuery, JoinCondition.And);

				jobDocAddressColumnQuery.AddSubQuery(TargetObjectSubQuery(jobDocAddrQuery), JoinCondition.Or);
			}

			var jobHeaderColumnQuery = new ZDBOnlySubQuery(typeof(T), pkColumn);
			if (ShouldCheckJobHeader)
			{
				// Company is the current company
				var jobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
				jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

				// Addresses for orgs in the allowed security policies
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				orgAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, orgSecurityQuery, JoinCondition.And);

				jobHeaderQuery.AddSubQuery(JobHeaderSchema.JH_OA_LocalChargesAddr, orgAddressQuery, JoinCondition.And);
				if (AllowNullLocalAddress)
				{
					jobHeaderQuery.AddToFilter(JoinCondition.Or, JobHeaderSchema.JH_OA_LocalChargesAddr, SQLComparisonOperator.Equal, null);
				}
				AppendAdditionalJobHeaderQuery(jobHeaderQuery);

				jobHeaderColumnQuery.AddSubQuery(TargetObjectSubQuery(jobHeaderQuery), JoinCondition.Or);
			}

			var query = new ZDBOnlyQuery(typeof(T));
			query.AddToFilter(pkColumn, ZGuid.Empty);

			var subQueries = new[] { orgColumnQuery, addressColumnQuery, jobHeaderColumnQuery, jobDocAddressColumnQuery }.Where(x => !x.IsEmpty).ToList();
			if (subQueries.Any())
			{
				foreach (var subQuery in subQueries)
				{
					query.AddAsUnionQuery(subQuery, true);
				}
			}

			AppendAdditionalOrgQuery(query, orgSecurityQuery, notIn: false, joinCondition: JoinCondition.Or);

			return query;
		}

		protected virtual ZDBOnlyQuery GetOSMGQueryEnhancedSecurityLevel(GlbStaff staff, SchemaColumn pkColumn)
		{
			throw new NotSupportedException("OSMG with Enhanced Security Level has not been implemented for this module yet.");
		}

		void SetMandatorySecurityFilter(ModuleNkFilter filter)
		{
			filter.GroupOrCategory = FilterOrCategory.None;
			filter.IsGroupOrCategoryReadOnly = true;
			filter.ComparisonOperator_List.Clear();
			filter.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZGuid>.ComparisonConstants.CurrentUser);
			filter.Visibility = FilterVisibility.AlwaysApplied | FilterVisibility.AlwaysVisible;
			filter.ReadOnly = true;
			filter.OrCategory = FilterOrCategory.MandatoryFilterOrCategory;
			filter.IsOrCategoryReadOnly = true;
			filter.IsMandatorySecurityFilter = true;
			filter.PropertyValidation = (info) =>
			{
				var value = (ZString)info.Value;
				if (value != GlbStaff.CurrentUser.GS_Code)
				{
					info.AddError(ResString.GetMultilingualString("010c2cbe-006d-4b85-8009-d0f9549dda3c", @"Your current security rights only allow you to view the records where you have the Security Access.
If you think this is incorrect, please contact your system administrator."));
				}
			};
		}

		bool IsEnhancedSecurityLevel
		{
			get => OSMGSecurityLevelRegistryItem?.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty) == Core.Constants.OSMGSecurityLevels.Enhanced;
		}

		#endregion
	}
}
