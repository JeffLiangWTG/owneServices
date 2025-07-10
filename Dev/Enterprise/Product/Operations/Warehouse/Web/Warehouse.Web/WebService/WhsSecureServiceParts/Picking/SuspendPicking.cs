using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region SuspendPicking

		[WebMethod(Description = "Suspend Picking")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse SuspendPicking()
		{
			return HandleWebServiceRequest<WebServiceResponse>(r => SuspendPickingCore(r));
		}

		void SuspendPickingCore(WebServiceResponse response)
		{
			var user = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			if (user != null)
			{
				var pickinglinesForUser = new ZQuery(WhsPickLineSchema.WZ_IsPicking, true);
				pickinglinesForUser.AddToFilter(WhsPickLineSchema.WZ_GS_NKAssignedTo, user.GS_Code);
				var pickLines = Factory.Load<WhsPickLine>(pickinglinesForUser);
				pickLines.ForEach(pl => pl.WZ_IsPicking = false);

				var concurrencyErrorMessage = Res.GetString("634718b2-1a74-4062-b765-13dc9ad3c7bf", "While you have been working with this job another user has made changes. Please restart the operation and try again.");
				WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);
			}
		}

		#endregion
	}
}
