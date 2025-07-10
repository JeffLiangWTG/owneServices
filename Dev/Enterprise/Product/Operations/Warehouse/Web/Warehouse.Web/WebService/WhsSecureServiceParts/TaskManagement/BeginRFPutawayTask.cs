using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region BeginRFPutawayTask

		[WebMethod(Description = "Begin RF Putaway Task")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public PutawayMultiplePalletsWebServiceResponse BeginRFPutawayTask(Guid taskPK)
		{
			return HandleWebServiceRequest((PutawayMultiplePalletsWebServiceResponse r) => BeginRFPutawayTaskCore(r, taskPK));
		}

		void BeginRFPutawayTaskCore(PutawayMultiplePalletsWebServiceResponse response, Guid taskPK)
		{
			var task = Factory.Load<ProcessTask>(taskPK);
			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			PrepareTaskForPutaway(response, task, staff);
		}

		void PrepareTaskForPutaway(PutawayMultiplePalletsWebServiceResponse response, ProcessTask task, GlbStaff staff)
		{
			var shouldSave = BeginRFTaskHelper.BeginRFTask(response, task, WarehouseTaskFormFlowTypes.PutawayJob, staff);

			if (response.NoError())
			{
				PopulatePutawayUserAndPallets(response, task, staff.GS_Code);
				if (shouldSave)
				{
					WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => BeginRFTaskHelper.TaskConcurrencyErrorMessage);
				}
			}
		}

		void PopulatePutawayUserAndPallets(PutawayMultiplePalletsWebServiceResponse response, ProcessTask task, string staffCode)
		{
			var palletInfos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var transferLines = Factory.Load<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.WE_P9_Task, task.PK));
			AddFetchHintsForSettingLocation(Factory, transferLines);
			foreach (var line in transferLines)
			{
				line.WE_GS_NKPutawayBy = staffCode;
				palletInfos.Add(line.WE_PalletID);
			}
			response.PalletInfos = palletInfos.Select(p => new PutawayPalletInfo { PalletID = p }).ToArray();
		}

		#endregion
	}
}
