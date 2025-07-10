using System;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Types;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region AddJobServiceCharge

		[WebMethod(Description = "Add Job Service")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse AddJobServiceCharge(Guid jobPK, JobServiceSupporterStrategy jobServiceSupporterStrategy, string chargeCode, decimal count, string serviceNote)
		{
			return HandleWebServiceRequest<WebServiceResponse>(r => AddJobServiceChargeCore(jobPK, jobServiceSupporterStrategy, chargeCode, count, serviceNote));
		}

		void AddJobServiceChargeCore(Guid jobPK, JobServiceSupporterStrategy jobServiceSupporterStrategy, string chargeCode, decimal count, string serviceNote)
		{
			var jobServiceSupporter = WebServiceHelper.GetJobServiceSupporter(Factory, jobPK, jobServiceSupporterStrategy);

			if (jobServiceSupporter != null)
			{
				var service = jobServiceSupporter.Services.AddIfNotExists(chargeCode).First();
				service.ES_ServiceCount = count;
				service.ES_ServiceNote = serviceNote;

				if (!service.IsInDatabase)
				{
					service.ES_Booked = ZDateTime.Today;
					service.ES_Completed = ZDateTime.Today;
				}

				Factory.Save();
			}
		}

		#endregion
	}
}
