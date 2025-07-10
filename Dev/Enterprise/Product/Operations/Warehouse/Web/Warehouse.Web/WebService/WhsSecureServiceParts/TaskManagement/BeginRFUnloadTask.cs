using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region BeginRFUnloadTask

		[WebMethod(Description = "Begin RF Unload Task")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsDocketsWebServiceResponse BeginRFUnloadTask(Guid taskPK)
		{
			return HandleWebServiceRequest((WhsDocketsWebServiceResponse r) => BeginRFUnloadTaskCore(r, taskPK));
		}

		void BeginRFUnloadTaskCore(WhsDocketsWebServiceResponse response, Guid taskPK)
		{
			var task = Factory.Load<ProcessTask>(taskPK);
			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);

			BeginRFTaskHelper.BeginRFTask(response, task, WarehouseTaskFormFlowTypes.UnloadJob, staff);

			if (response.NoError())
			{
				var query = new ZQuery(WhsDocketSchema.PK, task.P9_ParentID);
				query.AddToFilter(WhsDocketSchema.WD_WP_ParentPickForReceive, SQLComparisonOperator.Equal, null);
				var receive = Factory.LoadTop1<WhsReceive>(query);
				if (receive == null)
				{
					response.LogBusinessValidationError(Res.GetString("0475ba54-3ca8-430a-8b9d-6ba627146970", "Receive not found."));
				}
				else if (receive.IsFinalisedOrCancelled)
				{
					response.LogBusinessValidationError(Res.GetString("6a927250-a9b7-4467-82ca-838bba5b6cd6", "Cannot unload finalized or canceled Receives."));
				}
				else if (receive.WD_DocketSubType == ReceiveType.Codes.Customs)
				{
					response.LogBusinessValidationError(Res.GetString("aab0749c-fc5d-4e04-b8c0-ad354a92ee90", "Cannot unload Customs Receives with RF device."));
				}
				else
				{
					AddFetchHintsForReceiveTask(receive);
					GetWhsReceivesCore(response, [receive], isUnloadProcess: true);
				}
			}
		}

		void AddFetchHintsForReceiveTask(WhsReceive receive)
		{
			var receiveLines = Factory.Load<WhsReceiveLine>(new ZQuery(WhsDocketLineSchema.WE_WD, receive.PK));
			foreach (var line in receiveLines)
			{
				Factory.AddFetchHint(StmALogSchema.SL_Parent, line.PK);
				Factory.AddFetchHint(WhsInventoryViewSchema.WI_WE_InDocketLine, line.PK);
				Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, line.PK);
			}
		}

		#endregion
	}
}
