using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// This class compares container type of two lines based on different strategies
	/// </summary>
	class ContainerTypeColumnComparer : ColumnComparer
	{
		public ContainerTypeColumnComparer() : base(RateEntrySchema.TI_RC)
		{ }

		public override int Compare(FastLine line1, FastLine line2)
		{
			if (line1.ParentRateEntry.TI_RateCategory == RatingConstants.RateCategory.FCL
				&& line2.ParentRateEntry.TI_RateCategory == RatingConstants.RateCategory.FCL)
			{
				//FCL - For FLT calculator, we need to prioritize those lines for which RateEntry has empty container
				if (line1.Line.Uses(CalculatorType.Flat) && line2.Line.Uses(CalculatorType.Flat))
				{
					if (line1.ParentRateEntry.TI_RC.IsEmpty && !line2.ParentRateEntry.TI_RC.IsEmpty)
					{
						return 1;
					}

					if (!line1.ParentRateEntry.TI_RC.IsEmpty && line2.ParentRateEntry.TI_RC.IsEmpty)
					{
						return -1;
					}

					return 0;
				}
			}

			return base.Compare(line1, line2);
		}
	}
}
