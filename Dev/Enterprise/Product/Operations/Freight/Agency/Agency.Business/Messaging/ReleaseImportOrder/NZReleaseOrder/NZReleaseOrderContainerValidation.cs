namespace Enterprise.Freight.Agency.Business
{
	using Enterprise.Freight.Common.Business;

	public class NZReleaseOrderContainerValidation : JobContainerValidation
	{
		public NZReleaseOrderContainerValidation(BillOfLadingContainer parent)
			: base(parent) { }

		protected override void CheckJC_GrossWeight()
		{
			base.CheckJC_GrossWeight();

			if (Parent.JC_GrossWeight == 0)
			{
				Parent.JC_GrossWeightInfo.AddMessageError(Res.GetString("fe7d37f0-a74e-11e4-be82-902b34dc814a", "The weight is not entered."));
			}
		}
	}
}
