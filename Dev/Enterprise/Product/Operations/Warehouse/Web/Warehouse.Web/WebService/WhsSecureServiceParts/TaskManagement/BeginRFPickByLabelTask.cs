using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region BeginRFPickByLabelTask

		[WebMethod(Description = "Begin RF Pick By Label Task")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public BeginRFPickByLabelTaskWebServiceResponse BeginRFPickByLabelTask(Guid taskPK)
		{
			return HandleWebServiceRequest((BeginRFPickByLabelTaskWebServiceResponse r) => BeginRFPickByLabelTaskCore(r, taskPK));
		}

		void BeginRFPickByLabelTaskCore(BeginRFPickByLabelTaskWebServiceResponse response, Guid taskPK)
		{
			var task = Factory.Load<ProcessTask>(taskPK);
			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			PrepareTaskForPickByLabel(response, task, staff);
		}

		void PrepareTaskForPickByLabel(BeginRFPickByLabelTaskWebServiceResponse response, ProcessTask task, GlbStaff staff)
		{
			BeginRFTaskHelper.BeginRFTask(response, task, WarehouseTaskFormFlowTypes.PickByLabelJob, staff);

			if (response.NoError())
			{
				SetAssignedUserOnRelatedJobs(task, staff.GS_Code);

				WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => BeginRFTaskHelper.TaskConcurrencyErrorMessage);
			}

			void SetAssignedUserOnRelatedJobs(ProcessTask task, string staffCode)
			{
				var pickByLabelJob = Factory.LoadTop1<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_P9_Task, task.PK));
				if (pickByLabelJob != null)
				{
					pickByLabelJob.WTK_GS_NKAssignedTo = staffCode;
				}
			}
		}

		#endregion
	}
}
