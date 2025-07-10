using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruiter.Business
{
	public class HRContactDataSourceList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public static ZString Staff { get { return ResString.GetMultilingualString("HRContactDataSourceList|Staff", "Staff"); } }
			public static ZString JobApplicant { get { return ResString.GetMultilingualString("HRContactDataSourceList|JobApplicant", "Job Applicant"); } }
			public static ZString CampaignTracking { get { return ResString.GetMultilingualString("HRContactDataSourceList|CampaignTracking", "Campaign Tracking"); } }
		}

		/// <summary>
		/// Shows description only in the drop down edit box
		/// </summary>
		public HRContactDataSourceList(GlbCompanyCampaign parent)
		{
			AddPair(Codes.CampaignTracking, FilterModuleIDSuffixes.CampaignTracking);

			if (parent.IsMasterCampaign || !parent.IsTouchCampaign)
			{
				AddPair(Codes.Staff, FilterModuleIDSuffixes.Staff);
				AddPair(Codes.JobApplicant, FilterModuleIDSuffixes.JobApplicant);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter suffix constants")]
		public static class FilterModuleIDSuffixes
		{
			public const string Staff = "Staff";
			public const string JobApplicant = "JobApplicant";
			public const string CampaignTracking = "CampaignTracking";
		}
	}
}
