using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.GUI
{
	public class AirlineCommodityFilterStripBusinessObject : FilterStripBusinessObject
	{
		public AirlineCommodityFilterStripBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "AirlineCommodityFilterStripBusinessObject";
		}

		public static class FilterDescriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string Code = "Code";
			public const string Description = "Description";

			#endregion
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var codeFilter = filters.AddTextFilter(FilterDescriptions.Code, GetEmptyStringZQuery);
			codeFilter.MultilingualDescription = ResString.GetMultilingualString("AirlineCommodityFilter|Code", "Code");
			codeFilter.Visibility = FilterVisibility.AlwaysVisible;

			var descriptionFilter = filters.AddTextFilter(FilterDescriptions.Description, GetEmptyStringZQuery);
			descriptionFilter.MultilingualDescription = ResString.GetMultilingualString("AirlineCommodityFilter|Description", "Description");
			descriptionFilter.Visibility = FilterVisibility.AlwaysVisible;

			return filters;
		}

		public ModuleTextFilter CodeFilter => (ModuleTextFilter)this[FilterDescriptions.Code];

		public ModuleTextFilter DescriptionFilter => (ModuleTextFilter)this[FilterDescriptions.Description];

		ZQuery GetEmptyStringZQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery();
		}
	}
}
