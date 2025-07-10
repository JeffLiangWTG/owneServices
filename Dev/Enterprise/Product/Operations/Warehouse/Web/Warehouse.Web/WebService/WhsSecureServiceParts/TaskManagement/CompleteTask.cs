using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region CompleteTask

		[WebMethod(Description = "Complete Task")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse CompleteTask(Guid taskPK)
		{
			return HandleWebServiceRequest((WebServiceResponse r) => CompleteTaskCore(r, taskPK));
		}

		void CompleteTaskCore(WebServiceResponse response, Guid taskPK)
		{
			var task = Factory.Load<ProcessTask>(taskPK);
			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);

			var shouldSave = BeginRFTaskHelper.CloseRFTask(response, task, staff);
			if (shouldSave)
			{
				var concurrencyErrorMessage = Res.GetString("db50a81c-1ea5-468e-8b77-c810b23e77a5", "Another user has modified this task and the status can not be updated. Please restart the operation and try again.");
				WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);
			}
		}

		#endregion
	}
}
