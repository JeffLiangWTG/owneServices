using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public class ContactDataSourceList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public static ZString CampaignTracking { get { return ResString.GetMultilingualString("ContactDataSourceList|CampaignTracking", "Campaign Tracking"); } }
			public static ZString ClientIntelligence { get { return ResString.GetMultilingualString("ContactDataSourceList|ClientIntelligence", "Organization"); } }
			public static ZString Inquiries { get { return ResString.GetMultilingualString("ContactDataSourceList|Inquiries", "Inquiry Manager"); } }
		}

		/// <summary>
		/// Shows description only in the drop down edit box
		/// </summary>
		public ContactDataSourceList(GlbCompanyCampaign parent)
		{
			AddPair(Codes.CampaignTracking, FilterModuleIDSuffixes.CampaignTracking);

			if (parent.IsMasterCampaign || !parent.IsTouchCampaign)
			{
				AddPair(Codes.ClientIntelligence, FilterModuleIDSuffixes.ClientIntelligence);
				AddPair(Codes.Inquiries, FilterModuleIDSuffixes.Inquiries);
			}
		}

		public static class FilterModuleIDSuffixes
		{
			public const string CampaignTracking = "CampaignTracking";
			public const string ClientIntelligence = "";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
			public const string Inquiries = "Inquiries";
		}
	}
}
