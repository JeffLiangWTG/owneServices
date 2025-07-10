using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class TemplatedItemTextFilter : ModuleTextFilter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter strip name")]
		public const string FilterStripName = "Template";

		public TemplatedItemTextFilter(GetTextQuery queryDelegate, FilterVisibility visibility = FilterVisibility.AlwaysApplied)
			: base(FilterStripName, queryDelegate, () => new TemplateFilterOptions())
		{
			MultilingualDescription = ResString.GetMultilingualString("MasterFiles|TemplatedItemTextFilter|Template", "Template");
			DefaultProperty = TemplateFilterOptions.Codes.NonTemplate;
			Visibility = visibility;
			Category = FilterCategories.StatusAndFlags;
		}
	}
}
