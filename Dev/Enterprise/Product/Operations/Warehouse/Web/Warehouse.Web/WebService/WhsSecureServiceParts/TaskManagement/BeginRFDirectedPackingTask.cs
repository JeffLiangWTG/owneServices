using System;
using System.Collections.Generic;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region BeginRFDirectedPackingTask

		[WebMethod(Description = "Begin RF Directed Packing Task")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public BeginRFDirectedPackingTaskWebServiceResponse BeginRFDirectedPackingTask(Guid taskPK)
		{
			return HandleWebServiceRequest((BeginRFDirectedPackingTaskWebServiceResponse r) => BeginRFDirectedPackingTaskCore(r, taskPK));
		}

		void BeginRFDirectedPackingTaskCore(BeginRFDirectedPackingTaskWebServiceResponse response, Guid taskPK)
		{
			var task = Factory.Load<ProcessTask>(taskPK);
			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			PrepareTaskForDirectedPacking(response, task, staff);
		}

		void PrepareTaskForDirectedPacking(BeginRFDirectedPackingTaskWebServiceResponse response, ProcessTask task, GlbStaff staff)
		{
			var shouldSave = BeginRFTaskHelper.BeginRFTask(response, task, WarehouseTaskFormFlowTypes.DirectedPackingJob, staff);

			if (response.NoError())
			{
				BuildDirectedPackingTaskWebServiceResponse(response, task, staff.GS_Code);

				if (shouldSave)
				{
					WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => BeginRFTaskHelper.TaskConcurrencyErrorMessage);
				}
			}

			void BuildDirectedPackingTaskWebServiceResponse(BeginRFDirectedPackingTaskWebServiceResponse response, ProcessTask task, string staffCode)
			{
				var order = Factory.LoadTop1<WhsOrder>(new ZQuery(WhsDocketSchema.WD_P9_PackingTask, task.PK));
				var orderInfos = new List<WhsDocketInfo>();
				GetReadyToPackOrders(orderInfos, new [] { order });
				var ordersReadyToPack = orderInfos.ToArray();
				if (ordersReadyToPack.Length > 0)
				{
					var pick = order.Pick;

					var packingStation = Factory.Load<WhsLocation>(order.Lines[0].PickLines[0].InventoryLine.WE_WL);
					if (packingStation != null && packingStation.WLV_LocationClass == LocationClasses.Codes.PST)
					{
						order.WD_GS_NKAssignedPacker = staffCode;
						response.Orders = ordersReadyToPack;
						response.WhsPickPK = pick.PK.ToGuid();
						response.PackingStation = new WhsLocationInfo(packingStation.PK.ToGuid(), packingStation.WLV_LocationString, packingStation.WLV_LocationString_UserFriendly, LocationClasses.Codes.PST);
					}
					else
					{
						response.LogBusinessValidationError(Res.GetString("4b6bef57-459c-4677-bb2b-73ac6b96cb2a", "No available Packing Station."));
					}
				}
				else
				{
					response.LogBusinessValidationError(Res.GetString("83f384cc-19e9-4cb2-aec3-e4a7d429c7a5", "No order to Pack."));
				}
			}
		}

		#endregion
	}
}
