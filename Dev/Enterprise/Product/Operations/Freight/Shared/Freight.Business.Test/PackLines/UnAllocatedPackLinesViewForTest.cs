namespace Enterprise.Freight.Business.Testing
{
	sealed class UnAllocatedPackLinesViewForTest : UnAllocatedPackLinesView
	{
		public UnAllocatedPackLinesViewForTest(PackLineNonDependentCollection packLines) : base(packLines)
		{
		}

		protected override bool IsPacked(PackLine line)
		{
			return line.Containers.Count > 0;
		}
	}
}
