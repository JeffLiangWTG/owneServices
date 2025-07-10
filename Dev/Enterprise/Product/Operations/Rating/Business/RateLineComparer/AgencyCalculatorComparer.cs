using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class AgencyCalculatorComparer : BaseRateLineComparer
	{
		public override int Compare(FastLine line1, FastLine line2)
		{
			var calc1 = line1.Calculator;
			var calc2 = line2.Calculator;

			if (calc1.ShowMessageTypeSubType && calc2.ShowMessageTypeSubType)
			{
				if (!calc1.MessageType.IsEmpty && calc2.MessageType.IsEmpty)
				{
					return 1;
				}

				if (calc1.MessageType.IsEmpty && !calc2.MessageType.IsEmpty)
				{
					return -1;
				}

				if (calc1.MessageType == calc2.MessageType)
				{
					if (!calc1.MessageSubType.IsEmpty && calc2.MessageSubType.IsEmpty)
					{
						return 1;
					}

					if (calc1.MessageSubType.IsEmpty && !calc2.MessageSubType.IsEmpty)
					{
						return -1;
					}
				}
			}

			return 0;
		}

		protected override string GetName()
		{
			return (NoResString)"Agency Calculator Comparer"; // log message, subject to change, more for support people as of now
		}
	}
}
