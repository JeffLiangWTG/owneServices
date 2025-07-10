using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public sealed class PricingPageRateLineList : List<RateLine>
	{
		public PricingPageRateLineList()
		{
			containerType = new Dictionary<IRateLine, ZString>();
		}

		public void AddContainerType(RateLine rateLine, ZString newContainerType)
		{
			if (rateLine.IsCloned)
			{
				rateLine = rateLine.ClonedLineMaster;
			}

			var oldContainerType = GetContainerType(rateLine);

			if (!oldContainerType.IsEmpty && !newContainerType.IsEmpty)
			{
				oldContainerType += ", ";
			}

			if (oldContainerType.IndexOf(newContainerType + ", ") < 0)
			{
				containerType[rateLine] = new ZString(oldContainerType + newContainerType);
			}
		}

		public ZString GetContainerType(IRateLine line)
		{
			if ((line is RateLine rateLine) && rateLine.IsCloned)
			{
				line = rateLine.ClonedLineMaster;
			}

			ZString result;

			if (!containerType.TryGetValue(line, out result))
			{
				var container = line.ParentRateEntry.Container;
				result = container == null ? ZString.Empty : container.RC_Code;
				containerType.Add(line, result);
			}

			return result;
		}

		public bool ShowEquipmentType
		{
			get
			{
				var hasCartageCalculators = false;
				var hasNonCartageCalculators = false;

				foreach (var rateLine in this)
				{
					if (rateLine.Calculator.ShowEquipmentType)
					{
						hasCartageCalculators = true;
					}
					else
					{
						hasNonCartageCalculators = true;
					}

					if (hasCartageCalculators && hasNonCartageCalculators)
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool ContainsSameLinesAs(object obj)
		{
			var list = obj as PricingPageRateLineList;
			return list != null && list.Count == Count && Enumerable.All(list, Contains);
		}

		public static void RemoveDuplicateRateLineInSets(List<PricingPageRateLineList> listOfRateLineLists)
		{
			var existingRateLines = new List<RateLine>();

			foreach (var lineSet in listOfRateLineLists.ToArray())
			{
				foreach (var line in lineSet.ToArray())
				{
					if (!existingRateLines.Contains(line))
					{
						existingRateLines.Add(line);
					}
					else
					{
						lineSet.Remove(line);
					}
				}

				if (lineSet.Count == 0)
				{
					listOfRateLineLists.Remove(lineSet);
				}
			}
		}

		readonly Dictionary<IRateLine, ZString> containerType;

		public RateEntry ParentRateEntry { get; set; }
	}
}

