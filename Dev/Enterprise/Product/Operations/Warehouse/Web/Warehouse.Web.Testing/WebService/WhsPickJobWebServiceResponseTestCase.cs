using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public abstract class WhsPickJobWebServiceResponseTestCase<TPickJobWebServiceResponse, TPickJob> : WebServiceResponseTestCase
			where TPickJobWebServiceResponse : WhsPickJobWebServiceResponse<TPickJob>
			where TPickJob : WhsPickJobInfo, new()
	{
		#region Properties

		protected override void TestPropertiesCore()
		{
			base.TestPropertiesCore();
			AssertNull("Precondition", Response.Job);

			var pickJob = new TPickJob();
			Response.Job = pickJob;
			AssertEquals("Job", pickJob, Response.Job);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return GetNewPickJobWebServiceResponse();
		}

		protected new TPickJobWebServiceResponse Response
		{
			get { return (TPickJobWebServiceResponse)base.Response; }
		}

		protected abstract TPickJobWebServiceResponse GetNewPickJobWebServiceResponse();

		#endregion
	}
}
