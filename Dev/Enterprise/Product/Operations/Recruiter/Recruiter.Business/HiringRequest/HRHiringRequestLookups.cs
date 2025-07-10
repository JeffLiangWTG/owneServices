//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHRHiringRequestLookups
//
//    This class should be used for overriding collections in AutoHRHiringRequestLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	public class HRHiringRequestLookups : AutoHRHiringRequestLookups
	{
		public HRHiringRequestLookups(AutoHRHiringRequest parent) : base(parent)
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

		#region Teams

		public GlbTeamCollection Teams
		{
			get
			{
				if (glbTeams == null)
				{
					glbTeams = new GlbTeamCollection(Factory);
				}

				return glbTeams;
			}
		}

		GlbTeamCollection glbTeams;

		#endregion
	}
}
