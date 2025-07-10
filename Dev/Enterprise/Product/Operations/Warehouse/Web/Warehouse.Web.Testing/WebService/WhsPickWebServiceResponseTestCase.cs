using System;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsPickWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Test Cases

		public void TestPick()
		{
			AssertNotNull(Response.Pick);

			var pick = new WhsPickInfo();
			AssertNotEquals(pick, Response.Pick);

			Response.Pick = pick;
			AssertEquals(pick, Response.Pick);
		}

		public void TestTaskPK()
		{
			AssertEquals(Guid.Empty, Response.TaskPK);

			var taskPK = Guid.NewGuid();
			Response.TaskPK = taskPK;
			AssertEquals(taskPK, Response.TaskPK);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsPickWebServiceResponse();
		}

		protected new WhsPickWebServiceResponse Response
		{
			get
			{
				return (WhsPickWebServiceResponse)base.Response;
			}
		}

		#endregion
	}
}
