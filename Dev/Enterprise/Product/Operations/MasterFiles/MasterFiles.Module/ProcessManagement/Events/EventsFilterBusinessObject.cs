using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class EventsFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string EventCode = "Event Code";
			public const string EventDescription = "Event Description";
			public const string IsCustomizable = "Is Customizable";
			public const string IsReferenceFormatOverridden = "Is Reference Format Overridden";

			#endregion
		}

		#region FilterStripBusinessObject

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new EventsModuleFilterCollection();

			filters.AddTextFilter(Descriptions.EventCode, StmEventSchema.SE_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EventsFilterBusinessObject|EventCode", "Event Code");

			filters.AddFiltersForTranslatableText(
				Descriptions.EventDescription,
				StmEventSchema.SE_Desc,
				typeof(StmEvent),
				ResString.GetMultilingualString("MasterFiles|EventsFilterBusinessObject|EventName", "Event Name"));

			filters.AddFlagsFilter(
				Descriptions.IsCustomizable,
				new string[] { Res.GetString("MasterFiles|EventsFilterBusinessObject|IsCustomizable", "Is Customizable") },
				new SchemaBoolColumn[] { StmEventSchema.SE_IsCustomizable }
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EventsFilterBusinessObject|IsCustomizable", "Is Customizable");

			filters.AddFlagsFilter(
				Descriptions.IsReferenceFormatOverridden,
				new string[] { Res.GetString("MasterFiles|EventsFilterBusinessObject|IsReferenceFormatOverridden", "Is Reference Format Overridden") },
				new SchemaBoolColumn[] { StmEventSchema.SE_IsRefernceFormatOverridden }
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EventsFilterBusinessObject|IsReferenceFormatOverridden", "Is Reference Format Overridden");

			return filters;
		}

		#endregion
	}
}
