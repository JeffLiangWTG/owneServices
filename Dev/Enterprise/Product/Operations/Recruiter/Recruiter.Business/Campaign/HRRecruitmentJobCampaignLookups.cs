using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	public class HRRecruitmentJobCampaignLookups : AutoHRRecruitmentJobCampaignLookups
	{
		public HRRecruitmentJobCampaignLookups(AutoHRRecruitmentJobCampaign parent) : base(parent)
		{
		}

		#region Parent

		public new HRRecruitmentJobCampaign Parent
		{
			get { return (HRRecruitmentJobCampaign)base.Parent; }
		}

		#endregion

		#region JobRoles

		public HRJobRoleCollection JobRoles
		{
			get { return new HRJobRoleCollection(Factory); }
		}

		#endregion

		#region ClientAccounts

		public override OrgHeaderCollection ClientAccounts
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		#endregion

		#region ClientAddresses

		public new OrgAddressDependentCollection ClientAddresses
		{
			get
			{
				OrgAddressDependentCollection result;
				if (Parent.ClientAccount == null)
				{
					result = new OrgAddressDependentCollection(Factory);
				}
				else
				{
					result = new OrgAddressDependentCollection(Parent.ClientAccount);
					result.Load();
				}
				return result;
			}
		}

		#endregion

		#region ClientContacts

		public new OrgContactDependentCollection ClientContacts
		{
			get
			{
				OrgContactDependentCollection result;
				if (Parent.ClientAccount == null)
				{
					result = new OrgContactDependentCollection(Factory);
				}
				else
				{
					result = new OrgContactDependentCollection(Parent.ClientAccount, Factory);
					result.Load();
				}
				return result;
			}
		}

		#endregion

	}
}
