using System.Collections.Generic;

namespace Enterprise.Customs.US.Business
{
	class AIILineComparer : IComparer<AIILine>
	{
		public int Compare(AIILine x, AIILine y)
		{
			int result = x.US_LineNo.CompareTo(y.US_LineNo);
			if (result == 0)
			{
				InvoiceLineGroupingRange xRange = x.LineGroupRef;
				InvoiceLineGroupingRange yRange = y.LineGroupRef;
				if (xRange == null && yRange != null)
				{
					result = 1;
				}
				else if (xRange != null && yRange == null)
				{
					result = -1;
				}
				else if (xRange != null && yRange != null)
				{
					result = xRange.US_StartSequenceNo.CompareTo(yRange.US_StartSequenceNo);
					if (result == 0)
					{
						result = xRange.US_EndSequenceNo.CompareTo(yRange.US_EndSequenceNo);
					}
				}
			}
			return result;
		}

		public override bool Equals(object obj)
		{
			return obj != null && obj.GetType() == GetType();
		}

		public override int GetHashCode()
		{
			return GetType().GetHashCode();
		}
	}
}
