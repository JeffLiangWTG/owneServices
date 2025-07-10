using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class LoadFilterBusinessObject : FilterStripBusinessObject
	{
		#region FilterConstants

		public static class FilterConstants
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter constant")]
			public const string LoadJobID = "Load Planning Job ID";
		}

		#endregion

		#region GetModuleFilterThatOverridesAllOtherFilters

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore() => new ModuleFountainFilter(FilterConstants.LoadJobID, WhsLoadSchema.WLO_JobID, "WL") { MultilingualDescription = ResString.GetMultilingualString("b77af1a0-4dff-4e97-8b7d-e53812c5175d", "Load Planning Job ID") };

		#endregion

		#region GetModuleFilters

		protected override ModuleFilterCollection GetModuleFiltersCore() => new ModuleFilterCollection();

		#endregion
	}
}
