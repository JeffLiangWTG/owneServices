using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class CodeDescriptionPairWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Properties

		protected override void TestPropertiesCore()
		{
			base.TestPropertiesCore();

			var response = new CodeDescriptionPairWebServiceResponse();
			response.CodeDescriptionPairs = new CodeDescriptionPairInfoCollection();
			AssertNotNull(response.CodeDescriptionPairs);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new CodeDescriptionPairWebServiceResponse();
		}

		protected new CodeDescriptionPairWebServiceResponse Response
		{
			get { return (CodeDescriptionPairWebServiceResponse)base.Response; }
		}

		#endregion
	}
}
