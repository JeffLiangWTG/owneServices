using Enterprise.ZArchitecture.Core;
using CodeConsts = Enterprise.DeniedPartyScreening.Common.DeniedPartyConstants.ScreeningStatus.Codes;
using EnglishTextConsts = Enterprise.DeniedPartyScreening.Common.DeniedPartyConstants.ScreeningStatus.EnglishTexts;
using ResourceKeyConsts = Enterprise.DeniedPartyScreening.Common.DeniedPartyConstants.ScreeningStatus.ResourceKeys;

namespace Enterprise.MasterFiles.Integration
{
	public sealed class ScreeningStatusesList : CodeDescriptionPairList
	{
		public sealed class Codes : CodeConsts { }

		public static class Descriptions
		{
			public static MultilingualString Block => ResString.GetMultilingualString(ResourceKeyConsts.Block, EnglishTextConsts.Block);
			public static MultilingualString Canceled => ResString.GetMultilingualString(ResourceKeyConsts.Canceled, EnglishTextConsts.Canceled);
			public static MultilingualString Clear => ResString.GetMultilingualString(ResourceKeyConsts.Clear, EnglishTextConsts.Clear);
			public static MultilingualString JobBlockedExternal => ResString.GetMultilingualString(ResourceKeyConsts.JobBlockedExternal, EnglishTextConsts.JobBlockedExternal);
			public static MultilingualString JobCleared => ResString.GetMultilingualString(ResourceKeyConsts.JobCleared, EnglishTextConsts.JobCleared);
			public static MultilingualString JobClearedExternal => ResString.GetMultilingualString(ResourceKeyConsts.JobClearedExternal, EnglishTextConsts.JobClearedExternal);
			public static MultilingualString Matched => ResString.GetMultilingualString(ResourceKeyConsts.Matched, EnglishTextConsts.Matched);
			public static MultilingualString NeedsScreening => ResString.GetMultilingualString(ResourceKeyConsts.NeedsScreening, EnglishTextConsts.NeedsScreening);
			public static MultilingualString NotScreened => ResString.GetMultilingualString(ResourceKeyConsts.NotScreened, EnglishTextConsts.NotScreened);
			public static MultilingualString PermanentClear => ResString.GetMultilingualString(ResourceKeyConsts.PermanentClear, EnglishTextConsts.PermanentClear);
			public static MultilingualString Release => ResString.GetMultilingualString(ResourceKeyConsts.Release, EnglishTextConsts.Release);
			public static MultilingualString RequiresReview => ResString.GetMultilingualString(ResourceKeyConsts.RequiresReview, EnglishTextConsts.RequiresReview);
			public static MultilingualString Unknown => ResString.GetMultilingualString(ResourceKeyConsts.Unknown, EnglishTextConsts.Unknown);
		}

		public ScreeningStatusesList()
		{
			AddPair(CodeConsts.Block, Descriptions.Block);
			AddPair(CodeConsts.Canceled, Descriptions.Canceled);
			AddPair(CodeConsts.Clear, Descriptions.Clear);
			AddPair(CodeConsts.JobBlockedExternal, Descriptions.JobBlockedExternal);
			AddPair(CodeConsts.JobCleared, Descriptions.JobCleared);
			AddPair(CodeConsts.JobClearedExternal, Descriptions.JobClearedExternal);
			AddPair(CodeConsts.Matched, Descriptions.Matched);
			AddPair(CodeConsts.NeedsScreening, Descriptions.NeedsScreening);
			AddPair(CodeConsts.NotScreened, Descriptions.NotScreened);
			AddPair(CodeConsts.PermanentClear, Descriptions.PermanentClear);
			AddPair(CodeConsts.Release, Descriptions.Release);
			AddPair(CodeConsts.RequiresReview, Descriptions.RequiresReview);
			AddPair(CodeConsts.Unknown, Descriptions.Unknown);
		}
	}
}
