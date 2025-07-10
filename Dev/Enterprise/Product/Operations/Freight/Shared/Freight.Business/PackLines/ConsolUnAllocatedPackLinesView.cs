namespace Enterprise.Freight.Business
{
	public class ConsolUnAllocatedPackLinesView : UnAllocatedPackLinesView
	{
		public ConsolUnAllocatedPackLinesView(CommonConsol consol, PackLineNonDependentCollection packLines)
			: base(packLines)
		{
			this.consol = consol;
		}

		protected override bool IsPacked(PackLine line)
		{
			return (line.GetContainer(consol) != null);
		}

		protected override bool ShouldIncludeThisPackLine(PackLine line)
		{
			if (consol != null && !consol.IsMultiAWBMaster && !consol.Shipments.Contains(line.JL_JS))
			{
				return false;
			}

			return base.ShouldIncludeThisPackLine(line);
		}

		protected CommonConsol consol;
	}
}
