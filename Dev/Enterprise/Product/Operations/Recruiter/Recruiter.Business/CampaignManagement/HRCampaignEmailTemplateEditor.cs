using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	public class HRCampaignEmailTemplateEditor : CampaignEmailTemplateEditor
	{
		public HRCampaignEmailTemplateEditor(GlbCompanyCampaign campaign) : base(campaign)
		{
		}

		[List("StaffContactList")]
		public ZGuid StaffSimulationContactPK
		{
			get => SimulationContactPK;
			set
			{
				SimulationContactPK = value;
				StaffSimulationContactPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StaffSimulationContactPKInfo
		{
			get { return GetZPropertyInfo(nameof(StaffSimulationContactPK)); }
		}

		public GlbStaffCollection StaffContactList
		{
			get
			{
				if (staffContactList == null)
				{
					if (Campaign != null)
					{
						staffContactList = new GlbStaffCollection(Campaign.Factory);
					}
				}

				return staffContactList;
			}
		}

		GlbStaffCollection staffContactList;

		[List("ApplicantContactList")]
		public ZGuid ApplicantSimulationContactPK
		{
			get => SimulationContactPK;
			set
			{
				SimulationContactPK = value;
				ApplicantSimulationContactPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ApplicantSimulationContactPKInfo
		{
			get { return GetZPropertyInfo(nameof(ApplicantSimulationContactPK)); }
		}

		public HRJobApplicantCollection ApplicantContactList
		{
			get
			{
				if (applicantContactList == null)
				{
					if (Campaign != null)
					{
						applicantContactList = new HRJobApplicantCollection(Campaign.Factory);
					}
				}

				return applicantContactList;
			}
		}

		HRJobApplicantCollection applicantContactList;
	}
}
