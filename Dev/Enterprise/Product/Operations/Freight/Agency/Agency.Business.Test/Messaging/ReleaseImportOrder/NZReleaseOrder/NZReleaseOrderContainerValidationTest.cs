namespace Enterprise.Freight.Agency.Business.Testing
{
	internal sealed class NZReleaseOrderContainerValidationTest : BaseAgencyTest
	{
		public void TestJC_GrossWeight_ValueIsZero_AddMessageError()
		{
			var container = Factory.New<BillOfLadingContainer>();
			container.JC_GrossWeight = 0;

			var validation = new NZReleaseOrderContainerValidation(container);
			validation.ValidateJC_GrossWeight();
			AssertHasMessageErrors("JC_GrossWeight", container.JC_GrossWeightInfo);
		}

		public void TestJC_GrossWeight_ValueIsPositive_DoNotAddMessageError()
		{
			var container = Factory.New<BillOfLadingContainer>();
			container.JC_GrossWeight = 5;

			var validation = new NZReleaseOrderContainerValidation(container);
			validation.ValidateJC_GrossWeight();
			AssertNoMessageErrors("JC_GrossWeight", container.JC_GrossWeightInfo);
		}
	}
}
