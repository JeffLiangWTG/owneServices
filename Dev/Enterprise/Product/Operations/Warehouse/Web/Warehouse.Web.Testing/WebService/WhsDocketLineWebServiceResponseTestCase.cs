using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsDocketLineWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Test Cases

		public void TestLineInfo()
		{
			AssertNotNull(Response.LineInfo);

			WhsDocketLineInfo lineInfo = new WhsDocketLineInfo();
			AssertNotEquals(lineInfo, Response.LineInfo);

			Response.LineInfo = lineInfo;
			AssertEquals(lineInfo, Response.LineInfo);
		}

		#endregion

		#region Implementation

		protected new WhsDocketLineAndConversionsWebServiceResponse Response
		{
			get
			{
				return (WhsDocketLineAndConversionsWebServiceResponse)base.Response;
			}
		}

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsDocketLineAndConversionsWebServiceResponse();
		}

		#endregion
	}
}
