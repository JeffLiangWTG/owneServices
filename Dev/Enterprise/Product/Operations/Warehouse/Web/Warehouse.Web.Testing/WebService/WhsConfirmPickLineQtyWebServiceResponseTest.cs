using System;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsConfirmPickLineQtyWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Test Cases

		public void TestPick()
		{
			var response = new WhsConfirmPickLineQtyWebServiceResponse();
			AssertNotNull(response.ShortedOrderLinePKs);
			AssertContainsExactElementsInAnyOrder(Array.Empty<Guid>(), response.ShortedOrderLinePKs);

			var pk = new Guid();
			response.ShortedOrderLinePKs = new[] { pk };
			AssertContainsExactElementsInAnyOrder(new[] { pk }, response.ShortedOrderLinePKs);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsConfirmPickLineQtyWebServiceResponse();
		}

		protected new WhsConfirmPickLineQtyWebServiceResponse Response
		{
			get
			{
				return (WhsConfirmPickLineQtyWebServiceResponse)base.Response;
			}
		}

		#endregion
	}
}
