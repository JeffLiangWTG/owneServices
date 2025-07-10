using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class CompletionTriggerActionFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);

			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Action", ProcessTaskNotificationSchema.PQ_TriggerType, () => new WorkflowTriggerActionTypeConstants())
				.MultilingualDescription = ResString.GetMultilingualString("20f1901a-b447-4216-b3cc-91ea2908c701", "Action");
		}

		#endregion
	}
}
