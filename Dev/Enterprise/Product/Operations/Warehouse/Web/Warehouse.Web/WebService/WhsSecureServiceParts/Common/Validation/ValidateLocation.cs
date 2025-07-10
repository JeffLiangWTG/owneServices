using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region ValidateLocation

		[WebMethod(Description = "Validate Location by LocationPK , ClientCode , ProductCode")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse ValidateLocation(Guid locationPK, string clientCode, Guid productPK)
		{
			return HandleWebServiceRequest<WebServiceResponse>(result => ValidateLocationCore(result, locationPK, clientCode, productPK));
		}

		#region ValidateLocationCore

		void ValidateLocationCore(WebServiceResponse response, Guid locationPK, string clientCode, Guid productPK)
		{
			if (locationPK == Guid.Empty || clientCode.IsNullOrEmpty() || productPK == Guid.Empty)
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("add66cd8-bb94-470d-916c-be327629681f", "Please enter valid data Empty Location: '{0}', Client Code: '{1}', Empty Product: '{2}'",
					(locationPK == Guid.Empty) ? Res.GetString("aa95b285-4084-4162-9bfb-778a72a07ad8", "Yes") : Res.GetString("aa0fa231-8519-4a55-9810-23614a330311", "No"), clientCode, (productPK == Guid.Empty) ? Res.GetString("aa95b285-4084-4162-9bfb-778a72a07ad8", "Yes") : Res.GetString("aa0fa231-8519-4a55-9810-23614a330311", "No"));
			}
			else
			{
				var client = OrgHeader.LoadFromCode(Factory, clientCode);
				var supplierPart = Factory.Load<OrgSupplierPart>(new ZGuid(productPK));
				var location = Factory.Load<WhsLocation>(new ZGuid(locationPK));
				var error = WhsValidationHelper.GetErrorFixLocationAndProductAssignedToTheLocation(location, client, supplierPart);
				if (!error.IsNullOrEmpty())
				{
					response.Error = ErrorTypes.BusinessValidationError;
					response.ErrorMessage = error;
				}
			}
		}

		#endregion

		#endregion
	}
}
