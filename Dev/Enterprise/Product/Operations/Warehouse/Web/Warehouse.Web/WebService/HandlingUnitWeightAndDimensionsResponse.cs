using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class HandlingUnitWeightAndDimensionsResponse : WebServiceResponse
	{
		public HandlingUnitWeightAndDimensionsResponse()
			: base()
		{
			PackageDimensions = null;
		}

		public PackageDimensionsInfo PackageDimensions { get; set; }
	}
}
