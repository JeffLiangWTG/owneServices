using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class HBLDeliveryModeComparer : BaseRateLineComparer
	{
		public HBLDeliveryModeComparer(RatingCriteria criteria)
		{
			this.criteria = criteria;
		}

		readonly RatingCriteria criteria;

		public override int Compare(FastLine line1, FastLine line2)
		{
			if (!_Rating.Cost && criteria != null)
			{
				var hblDeliveryMode = criteria.HBLDeliveryMode;

				if (hblDeliveryMode.IsEmpty)
				{
					return 0;
				}

				if (line1.ParentRateEntry.TI_HBLDeliveryMode.IsEmpty && line2.ParentRateEntry.TI_HBLDeliveryMode.IsEmpty)
				{
					return 0;
				}

				var hblDeliveryModePriorities = new List<ZString> { hblDeliveryMode };

				var hblDeliveryPriorityConfig = RatingDataRegistry
					.Instance
					.HBLDeliveryPriority
					.Value
					.Cast<HBLDeliveryPriorityConfig>()
					.FirstOrDefault(x => x.ContainerMode == criteria.ContainerMode && x.HBLDeliveryMode == hblDeliveryMode);

				var fallbacksInRegistry =
					hblDeliveryPriorityConfig?
					.Settings?
					.Cast<HBLDeliveryPrioritySetting>()?
					.Select(i => i.HBLDeliveryModePriority)?
					.Where(i => !i.IsEmpty)?
					.ToList();

				if (fallbacksInRegistry?.Any() ?? false)
				{
					hblDeliveryModePriorities = fallbacksInRegistry;
				}

				var line1Index = hblDeliveryModePriorities.IndexOf(line1.ParentRateEntry.TI_HBLDeliveryMode);

				if (line1Index < 0)
				{
					return -1;
				}

				var line2Index = hblDeliveryModePriorities.IndexOf(line2.ParentRateEntry.TI_HBLDeliveryMode);

				if (line2Index < 0)
				{
					return 1;
				}

				return line2Index - line1Index;
			}

			return 0;
		}

		protected override string GetName()
		{
			return (NoResString)"HBL Delivery Mode"; // log message, subject to change, more for support people as of now
		}
	}
}
