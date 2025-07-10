using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicationLookups : AutoHRJobApplicationLookups
	{
		public HRJobApplicationLookups(AutoHRJobApplication parent) : base(parent)
		{
		}

		#region Applicants

		public HRJobApplicantCollection Applicants
		{
			get
			{
				if (fApplicants == null)
				{
					fApplicants = new HRJobApplicantCollection(Factory);
				}

				return fApplicants;
			}
		}

		HRJobApplicantCollection fApplicants;

		#endregion

		#region Campaigns

		public HRRecruitmentJobCampaignCollection Campaigns
		{
			get
			{
				if (fCampaigns == null)
				{
					fCampaigns = new HRRecruitmentJobCampaignCollection(Factory);
				}
				return fCampaigns;
			}
		}
		HRRecruitmentJobCampaignCollection fCampaigns;

		#endregion

		#region  ApplicationStatuses

		public ReadOnlyCodeDescriptionPairList ApplicationStatuses
		{
			get { return RecruiterDataRegistry.Instance.ApplicationStatuses.Value.AsCodeDescriptionPairList(); }
		}

		#endregion

		#region OverallRatings

		public CodeDescriptionPairList OverallRatings => Factory.GetCachedValue("HRJobApplicationLookups.OverallRatings", () =>
			new CodeDescriptionPairList
			{
				new CodeDescriptionPair("-1", Res.GetString("bb70a5d9-ffa6-47d8-b3d1-e267e694dfc3", "Unrated")),
				new CodeDescriptionPair("1", Res.GetString("43d79196-a3c2-4435-819e-b58ce064728c", "Suitable")),
				new CodeDescriptionPair("2", Res.GetString("826168bf-cf2b-43bc-9805-cef3766ad6fe", "Potential")),
				new CodeDescriptionPair("3", Res.GetString("3b7ffe24-f0f5-45f8-8b04-ced57512e357", "Unsuitable")),
			});

		#endregion

		#region Referring Party

		public CodeDescriptionPairList SourceTypes =>
			Parent.Factory.GetCachedValue("HRJobApplicationLookups.SourceTypes", () =>
			{
				var sourceTypes = new CodeDescriptionPairList();
				foreach (CodeDescriptionBool item in RecruiterDataRegistry.Instance.ReferringSourcesTypes.Value)
				{
					if (item.Bool)
					{
						sourceTypes.AddPair(item.Code, item.Description);
					}
				}
				return sourceTypes;
			});

		public CodeDescriptionPairList AllSourceTypes =>
			Parent.Factory.GetCachedValue("HRJobApplicationLookups.AllSourceTypes", () =>
			{
				var allSourceTypes = new CodeDescriptionPairList();
				foreach (CodeDescriptionBool item in RecruiterDataRegistry.Instance.ReferringSourcesTypes.Value)
				{
					allSourceTypes.AddPair(item.Code, item.Description);
				}
				return allSourceTypes;
			});

		public virtual GlbStaffCollection ReferringStaffs => new GlbStaffCollection(Factory);

		public override GlbPersonCollection ReferringPersons =>
			Parent.Factory.GetCachedValue(FormattableString.Invariant($"HRJobApplicationLookups.ReferringPersons.{Parent.HP_OH_ReferringOrganisation}.{Parent.IsReferringOrganisationMandatory}"), () => // Internal key only
			{
				var result = new GlbPersonCollection(Factory);

				if (Parent.ReferringOrganisation != null)
				{
					var query = new ZDBOnlyQuery(typeof(GlbPerson));
					var subQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.OC_PER);
					subQuery.AddToFilter(OrgContactSchema.OC_OH, Parent.ReferringOrganisation.PK);
					query.AddSubQuery(subQuery, JoinCondition.And);

					result.AdditionalFilter = query;
					result.AddNotificationWhenAdditionalFilterNotMetOverride = (errors, bizObj) =>
						errors.Add(Res.GetString("fc910318-7e09-4934-8c01-a48be550742f", "A Person selected from here must have a Related Organization {0}.", Parent.ReferringOrganisation.OH_Code));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Related Organization", "Property", Parent.ReferringOrganisation.PK, false));
				}
				else if (Parent.IsReferringOrganisationMandatory)
				{
					var query = new ZDBOnlyQuery(typeof(GlbPerson));
					var subQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.OC_PER);
					query.AddSubQuery(subQuery, JoinCondition.And);
					result.AdditionalFilter = query;

					result.AddNotificationWhenAdditionalFilterNotMetOverride = (errors, bizObj) =>
						errors.Add(Res.GetString("0973818e-0bdb-41ff-86f8-2f69d1b6542d", "A Person selected from here must have an Organization Contact Context"));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Related Context", "Property0", ZBool.True, false));
				}

				return result;
			});

		public CodeDescriptionPairList ReferringOrganisationsList =>
			Parent.Factory.GetCachedValue("HRJobApplicationLookups.ReferringOrganisationsList", () =>
			{
				var referringPartiesConfiguration = RecruiterDataRegistry.Instance.ReferringPartiesConfiguration.Value.Cast<ReferringPartyConfiguration>()
											.Where(r => r.DefaultReferringSource == Parent.HP_SourceType && r.ReferringParty == OrgHeaderSchema.Constants.Prefix);

				var organizationPKs = new List<ZGuid>();
				foreach (var item in referringPartiesConfiguration)
				{
					organizationPKs.Add(item.OrganizationPK);
				}

				var query = new ZQuery(OrgHeaderSchema.PK, organizationPKs);
				var organizations = Factory.Load<OrgHeader>(query);
				var result = new CodeDescriptionPairList();

				foreach (var item in organizations)
				{
					result.AddPair(item.PK, item.OH_Code, item.OH_FullName);
				}

				return result;
			});

		#endregion

		public HRJobRoleCollection JobRoles
		{
			get
			{
				if (fJobRoles == null)
				{
					fJobRoles = new HRJobRoleCollection(Factory);
				}

				return fJobRoles;
			}
		}

		HRJobRoleCollection fJobRoles;

		protected new HRJobApplication Parent => (HRJobApplication)base.Parent;
	}
}
