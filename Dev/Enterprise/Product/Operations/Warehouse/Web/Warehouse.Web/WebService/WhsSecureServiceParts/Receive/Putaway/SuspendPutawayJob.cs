using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		[WebMethod(Description = "Suspend current putaway job for logged in user and warehouse")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse SuspendPutawayJob()
		{
			return HandleWebServiceRequest<WebServiceResponse>(r => SuspendPutawayJob(r));
		}

		void SuspendPutawayJob(WebServiceResponse response)
		{
			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);

			if (staff != null && warehouse != null)
			{
				LoadAndSuspendPutawayJob();
			}
			else
			{
				response.LogBusinessValidationError(Res.GetString("7f020eee-dca0-4421-9fa8-776f35c91f4f", "Please provide login credentials to use this service."));
			}

			void LoadAndSuspendPutawayJob()
			{
				var job = PutawayHelper.LoadUnfinalisedPutawayJob(Factory, warehouse, staff);
				if (job == null)
				{
					response.LogBusinessValidationError(Res.GetString("1fe16b70-5b8c-40ee-96c7-311bac2bb941", "Putaway Job cannot be found."));
				}
				else
				{
					job.AddEvents(Enterprise.ZArchitecture.Business.Events.ServiceSuspended);
					job.Lines.ForEach(line => line.WPL_IsPuttingAway = false);
					var concurrencyErrorMessage = Res.GetString("4dd75022-d073-4fd9-849c-be3172ad0583", "Another user has changed the putaway job while you have been working on it. Please restart the operation and try again.");
					WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);
				}
			}
		}
	}
}
