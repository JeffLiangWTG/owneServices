using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class LCMarginPercentages
	{
		public LCMarginPercentages()
		{
		}

		public bool HasValues
		{
			get
			{
				return !LCMarginPercentage1.IsEmpty
					|| !LCMarginPercentage2.IsEmpty
					|| !LCMarginPercentage3.IsEmpty;
			}
		}

		public ZDecimal LCMarginPercentage1;
		public ZDecimal LCMarginPercentage2;
		public ZDecimal LCMarginPercentage3;
	}
}
