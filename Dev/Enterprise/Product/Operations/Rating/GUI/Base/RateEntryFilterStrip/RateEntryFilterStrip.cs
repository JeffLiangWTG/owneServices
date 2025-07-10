using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Rating.GUI
{
	/// <summary>
	/// Filter strip for RatingHeader forms.
	/// </summary>
	class RateEntryFilterStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			var result = base.GetCurrentFilterControls(currentModuleFilter);

			if (currentModuleFilter != null &&
				currentModuleFilter.Code == RateLineModuleFilters.Constants.Codes.UnitMultiple &&
				result.Length >= 1 &&
				result[0] is ZNumberRangeControl numberRangeControl)
			{
				numberRangeControl.SetShowEmptyStringForEmptyValue(true);
			}

			return result;
		}
	}
}
