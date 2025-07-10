using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	public class SharedSizeGroupProvider
	{
		readonly Dictionary<string, SharedSizeGroup> sizeGroups = new Dictionary<string, SharedSizeGroup>();

		public void IncludeInSizeGroup(Control controlToInclude, string sizeGroupName)
		{
			var sizeGroup = sizeGroups.GetOrAdd(sizeGroupName.ToLowerInvariant(), () => new SharedSizeGroup());
			sizeGroup.Include(controlToInclude);
		}

		public void ExcludeFromSizeGroup(Control controlToExclude, string sizeGroupName)
		{
			if (sizeGroups.TryGetValue(sizeGroupName, out var sizeGroup))
			{
				sizeGroup.Exclude(controlToExclude);
			}
		}

		public void AdjustAll()
		{
			foreach (var sizeGroup in sizeGroups.Values)
			{
				sizeGroup.AdjustSizeInGroup();
			}
		}
	}
}
