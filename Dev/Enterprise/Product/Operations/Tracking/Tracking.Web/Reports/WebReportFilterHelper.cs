using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web
{
	public class WebReportFilterHelper : IWebReportFilterHelper
	{
		public bool HasWebModule(LookupFilterFieldBase filterField)
		{
			return WebModuleIDs.GetWebModuleIDFromModuleID(filterField.ModuleID) != WebModuleIDs.NotAssigned;
		}

		public bool IsSupportedOnWeb(FilterField filterField)
		{
			return filterField is TextField
				   || filterField is NumberField
				   || filterField is DateField
				   || filterField is DateTimeOffsetField
				   || filterField is DateRangeField
				   || filterField is DateTimeOffsetRangeField
				   || filterField is MultipleChoice
				   || filterField is CodeListMultipleChoice
				   || (filterField is CodeLookupField && HasWebModule((CodeLookupField)filterField))
				   || (filterField is LookupField && HasWebModule((LookupField)filterField))
				   || filterField is OptionGroup;
		}
	}
}
