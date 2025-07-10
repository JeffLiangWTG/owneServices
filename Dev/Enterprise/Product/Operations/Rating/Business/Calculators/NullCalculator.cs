using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Rating.Business
{
	public class NullCalculator : Calculator
	{
		public const string Code = "   ";

		public NullCalculator(IRateLine master)
			: base(master) { }

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders
		{
			get { return false; }
		}

		protected override IEnumerable<IRateLineItem> CheckOrCreateItems()
		{
			return Enumerable.Empty<IRateLineItem>();
		}

		protected override void PerformPostItemCreationActions() { }
		public override void ValidateTM_Value(RateLineItem lineItem) { }
	}
}
