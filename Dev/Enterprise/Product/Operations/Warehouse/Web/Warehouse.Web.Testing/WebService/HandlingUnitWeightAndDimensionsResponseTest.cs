using Enterprise.Warehouse.Web.WebService.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class HandlingUnitWeightAndDimensionsResponseTestCase : WebServiceResponseTestCase
	{
		#region TestHandlingUnitWeightAndDimensionsResponse

		public void TestHandlingUnitWeightAndDimensionsResponse()
		{
			var response = new HandlingUnitWeightAndDimensionsResponse();
			AssertEquals("Default response is empty.", null, response.PackageDimensions);

			response.PackageDimensions = new PackageDimensionsInfo
			{
				PackType = "PLT",
				EmptyWeight = 1m,
				Weight = 2m,
				WeightUQ = "KG",
				Length = 4m,
				Width = 6m,
				Height = 8m,
				DimensionUQ = "M",
			};

			AssertEquals("Value is set.", "PLT", response.PackageDimensions.PackType);
			AssertEquals("Value is set.", 1m, response.PackageDimensions.EmptyWeight);
			AssertEquals("Value is set.", 2m, response.PackageDimensions.Weight);
			AssertEquals("Value is set.", "KG", response.PackageDimensions.WeightUQ);
			AssertEquals("Value is set.", 4m, response.PackageDimensions.Length);
			AssertEquals("Value is set.", 6m, response.PackageDimensions.Width);
			AssertEquals("Value is set.", 8m, response.PackageDimensions.Height);
			AssertEquals("Value is set.", "M", response.PackageDimensions.DimensionUQ);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new HandlingUnitWeightAndDimensionsResponse();
		}

		#endregion
	}
}
