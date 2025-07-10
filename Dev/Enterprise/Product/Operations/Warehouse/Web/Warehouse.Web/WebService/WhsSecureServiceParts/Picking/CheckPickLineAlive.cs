using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region CheckPickLineAlive

		[WebMethod(Description = "Check that pickLine is still alive")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsPickLineStatusWebServiceResponse CheckPickLineAlive(Guid pickLinePK)
		{
			return HandleWebServiceRequest_WithValidateWarehouseAndStaff<WhsPickLineStatusWebServiceResponse>(r => CheckPickLineAlive(r, pickLinePK));
		}

		void CheckPickLineAlive(WhsPickLineStatusWebServiceResponse response, Guid pickLinePK)
		{
			var pickLine = Factory.Load<WhsPickLine>(new ZGuid(pickLinePK));
			response.IsPickLineAlive = pickLine != null;
		}

		#endregion
	}
}