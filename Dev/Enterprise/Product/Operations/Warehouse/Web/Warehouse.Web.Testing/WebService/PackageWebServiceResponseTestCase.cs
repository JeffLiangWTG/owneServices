namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class PackageWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Test Cases

		public void TestReleaseChoices()
		{
			AssertNotNull(Response.PackageChoices);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new PackageWebServiceResponse();
		}

		protected new PackageWebServiceResponse Response
		{
			get { return (PackageWebServiceResponse)base.Response; }
		}

		#endregion
	}
}
