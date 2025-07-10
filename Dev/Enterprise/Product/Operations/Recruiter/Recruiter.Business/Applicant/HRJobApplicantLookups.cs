using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicantLookups : AutoHRJobApplicantLookups
	{
		public HRJobApplicantLookups(AutoHRJobApplicant parent)
			: base(parent)
		{
		}

		#region Work Permit Statuses

		public virtual CodeDescriptionPairList WorkPermitStatuses
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(RecruiterDataRegistry.Instance.WorkPermitStatusList.Value.GetActiveCodeDescriptionPairList());
				return result;
			}
		}

		#endregion

		#region Availabilities

		public ICodeDescriptionPairList Availabilities
		{
			get { return RecruiterDataRegistry.Instance.AvailabilityList.Value.GetActiveCodeDescriptionPairList(); }
		}

		#endregion

		#region JobRoles

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

		#region StateList

		public CodeDescriptionPairList StateList
		{
			get
			{
				var result = new UntranslatableCodeDescriptionPairList((NoResString)"States from RefCountryState table"); // Untranslatable reason
				var country = ((HRJobApplicant)Parent).Country;
				var stateList = (country != null) ? new OrgCodeLists().State_List(country) : new CodeDescriptionPairList();
				result.AddRange(stateList);

				return result;
			}
		}

		#endregion

		#region Gender

		public CodeDescriptionPairList Genders
		{
			get
			{
				return Factory.GetCachedValue("GendersList", delegate
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(Core.Constants.Genders.Woman, Core.Constants.GenderDescriptions.Woman);
					list.AddPair(Core.Constants.Genders.Man, Core.Constants.GenderDescriptions.Man);
					list.AddPair(Core.Constants.Genders.Agender, Core.Constants.GenderDescriptions.Agender);
					list.AddPair(Core.Constants.Genders.NonBinary, Core.Constants.GenderDescriptions.NonBinary);
					list.AddPair(Core.Constants.Genders.NotSpecified, Core.Constants.GenderDescriptions.NotSpecified);
					list.AddPair(Core.Constants.Genders.Custom, Core.Constants.GenderDescriptions.Custom);
					return list;
				});
			}
		}

		#endregion

		#region WorkingLanguages

		public CodeDescriptionPairList WorkingLanguages
		{
			get
			{
				return Factory.GetCachedValue("GlbStaffLookups.WorkingLanguages",
					() =>
					{
						return new CodeDescriptionPairList(OLookUpEditType.Language);
					});
			}
		}

		#endregion

		#region Countries

		public virtual RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		#endregion
	}
}
