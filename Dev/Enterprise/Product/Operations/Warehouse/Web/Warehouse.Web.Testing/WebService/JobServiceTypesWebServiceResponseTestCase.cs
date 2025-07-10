using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class JobServiceTypesWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Properties

		protected override void TestPropertiesCore()
		{
			base.TestPropertiesCore();

			var response = new JobServiceTypesWebServiceResponse();
			response.JobServiceTypes = new JobServiceTypesInfoCollection();
			AssertNotNull(response.JobServiceTypes);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new JobServiceTypesWebServiceResponse();
		}

		protected new JobServiceTypesWebServiceResponse Response
		{
			get { return (JobServiceTypesWebServiceResponse)base.Response; }
		}

		#endregion
	}
}
