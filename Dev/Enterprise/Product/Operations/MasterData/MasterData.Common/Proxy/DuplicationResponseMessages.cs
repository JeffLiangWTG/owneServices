using System.Diagnostics.CodeAnalysis;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterData.Common
{
	public static class DuplicationResponseMessages
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly MultilingualString Timeout = ResString.GetMultilingualString("79ab3b92-c4f8-40e5-9f4e-9a5ad8880928", "The operation timed out");
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly MultilingualString PartialTimeout = ResString.GetMultilingualString("737fd38a-1f9d-40cd-a19c-8b051ba4c44d", "The operation timed out, but some results were found");
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly MultilingualString Exception = ResString.GetMultilingualString("fe35165f-5743-49eb-9202-d1e6c7b03a58", "An exception has occurred");
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly MultilingualString Exclusion = ResString.GetMultilingualString("813d3ed2-24ec-46a3-bd69-8f51f8e9622e", "The master is excluded from De-duplication");
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly MultilingualString PIGExisted = ResString.GetMultilingualString("56e1ba52-0039-42a1-a5af-bb0dfe58498d", "Scoring Pair is ignored for everybody");
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly MultilingualString TIGExisted = ResString.GetMultilingualString("8e3ab911-d287-4b58-89a9-18b12b26fea8", "Scoring Pair is ignored for this user");
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly MultilingualString MasterIsDirty = ResString.GetMultilingualString("8bd0622a-17ff-481b-abf4-b552bf8343d1", "The master is marked as dirty");
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly MultilingualString Success = ResString.GetMultilingualString("0dd92050-4dfe-4b9b-80e8-934b6eb8e1ad", "Success");
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly MultilingualString NoIgnoranceExisted = ResString.GetMultilingualString("7c4688de-5a34-4819-a3f9-55b122562d27", "Remove Ignore failed because no specified ignore existed");
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly MultilingualString NoExclusionExisted = ResString.GetMultilingualString("67f7fc47-b4ae-4c0d-8a93-ba31f8f7f836", "Remove Exclusion failed because no exclusion existed");
	}
}
