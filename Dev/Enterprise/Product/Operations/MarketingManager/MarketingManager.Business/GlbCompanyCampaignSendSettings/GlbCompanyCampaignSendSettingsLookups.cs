//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbCompanyCampaignSendSettingsLookups
//
//    This class should be used for overriding collections in AutoGlbCompanyCampaignSendSettingsLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignSendSettingsLookups : AutoGlbCompanyCampaignSendSettingsLookups
	{
		public GlbCompanyCampaignSendSettingsLookups(AutoGlbCompanyCampaignSendSettings parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ScheduleTypes
		{
			get
			{
				var scheduleTypes = new CodeDescriptionPairList();
				scheduleTypes.AddPair(Codes.BAT, Res.GetString("2CC4A625-7044-421F-8D66-40DCE2060A1B", "Batch Schedule"));
				scheduleTypes.AddPair(Codes.IMM, Res.GetString("396F95E7-8FCD-49CC-B8F2-C35339478878", "Immediate Delivery"));
				scheduleTypes.AddPair(Codes.DEL, Res.GetString("45744D39-4BA7-4663-B5E3-660DA0723A95", "Delayed Delivery"));
				scheduleTypes.AddPair(Codes.FIX, Res.GetString("65FDC712-DB0E-4C1C-97CF-CF97365BE38B", "Fixed Date Delivery"));

				return scheduleTypes;
			}
		}

		public static class Codes
		{
			public const string BAT = "BAT";
			public const string IMM = "IMM";
			public const string DEL = "DEL";
			public const string FIX = "FIX";
		}
	}
}
