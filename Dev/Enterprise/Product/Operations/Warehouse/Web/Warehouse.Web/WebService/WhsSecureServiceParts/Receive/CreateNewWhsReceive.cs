using System;
using System.Globalization;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region CreateNewWhsReceive

		[WebMethod(Description = "Create New Receive Job")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsDocketWebServiceResponse CreateNewWhsReceive(string reference, string clientCode, DateTime arrivalDate, string receiveType)
		{
			return HandleWebServiceRequest<WhsDocketWebServiceResponse>(result => CreateNewWhsReceiveCore(result, reference, clientCode, arrivalDate, receiveType));
		}

		#region CreateNewWhsReceiveCore

		void CreateNewWhsReceiveCore(WhsDocketWebServiceResponse response, string reference, string clientCode, DateTime arrivalDate, string receiveType)
		{
			var receive = NewWhsReceive(response, reference, clientCode, arrivalDate, receiveType);
			if (response.NoError())
			{
				if (receive != null)
				{
					response.Docket = new WhsDocketInfo(receive);
					if (receive.IsReturnReceive && receive.WD_WD_ParentDocket.IsEmpty)
					{
						response.LogError(ErrorTypes.WarningOnly, WhsReceive.ReturnReceivesRequireAnOrderToReturnWarning);
					}
				}

				response.HeldCodes = GetHeldCodesOfClient(clientCode);
				response.ShowStockOnHandWarningOnPutaway = WarehouseDataRegistry.Instance.SOHLocationWarning.Value;
				response.CanDuplicatePreviousLine = Env.Security.WhsRFScanningUnloadDuplicatePreviousLine.IsAllowed;

				WhsReceiveHelper.SetSingleDockDoorLocationDetails(Factory, response, SecurityHeader.WarehouseCode);
			}
		}

		#endregion

		#region NewWhsReceive

		WhsReceive NewWhsReceive(WebServiceResponse response, string reference, string clientCode, DateTime arrivalDate, string receiveType)
		{
			WhsReceive result = null;
			var client = WebServiceHelper.GetOrgHeader(Factory, clientCode);
			if (string.IsNullOrEmpty(clientCode) || client == null)
			{
				response.LogBusinessValidationError(Res.GetString("5b2ae1b4-0405-4031-99b5-c5a3a74aa36d", "Please provide a valid client code."));
			}
			else
			{
				if (reference.IsNullOrEmpty())
				{
					var today = ZDateTime.Now;
					reference = today.Year.ToString("0000", CultureInfo.InvariantCulture) + today.Month.ToString("00", CultureInfo.InvariantCulture) +
								today.Day.ToString("00", CultureInfo.InvariantCulture) + today.Hour.ToString("00", CultureInfo.InvariantCulture) +
								today.Minute.ToString("00", CultureInfo.InvariantCulture) + today.Second.ToString("00", CultureInfo.InvariantCulture);
				}
				else
				{
					var referenceValidationError = WarehouseValidationHelper.ValidateDocketExternalReference(reference, Res.GetString("a36412cf-8478-4876-b8f6-2978de300414", "Receive Reference"));
					if (!referenceValidationError.IsNullOrEmpty())
					{
						response.LogBusinessValidationError(referenceValidationError);
					}
				}

				if (response.NoError())
				{
					result = CreateNewWhsReceive(response, reference, arrivalDate, receiveType, client);
				}
			}

			return result;
		}

		WhsReceive CreateNewWhsReceive(WebServiceResponse response, string reference, ZDateTimeOffset arrivalDate, string receiveType, OrgHeader client)
		{
			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			var receive = Factory.New<WhsReceive>();
			receive.WD_WW_Whs = warehouse.PK;
			receive.WD_OH_Client = client.PK;
			receive.WD_ExternalReference = reference;
			receive.WD_ArrivalDate = arrivalDate == DateTimeOffset.MinValue ? ZDateTimeOffset.Now : arrivalDate;
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			receive.WD_DocketSubType = receiveType;

			if (warehouse.WW_GG_ReleaseGroup.IsValid)
			{
				CreateUnloadTask(receive, WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName));
			}

			receive.RunPreSaveValidation();
			if (receive.Notifications.GetErrors().GetUniqueMessageList().Length > 0)
			{
				response.LogBusinessValidationError(receive.Notifications.GetErrors().GetUniqueMessageList()[0]);
			}
			else if (receive.Notifications.GetMessageErrors().GetUniqueMessageList().Length > 0)
			{
				response.LogBusinessValidationError(receive.Notifications.GetMessageErrors().GetUniqueMessageList()[0]);
			}
			else
			{
				var concurrencyErrorMessage = Res.GetString("cd8a5e6a-a3d2-4512-aef2-d2429d5f29d7", "Another user has made changes while you're creating the receive. Please restart the operation and try again.");
				WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);
			}

			return response.NoError()
				? receive
				: null;
		}

		WhsReceiveProcessTasks CreateUnloadTask(WhsReceive receive, GlbStaff staff)
		{
			var unloadTask = receive.Factory.New<WhsReceiveProcessTasks>();
			unloadTask.P9_ParentID = receive.PK;
			unloadTask.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			unloadTask.P9_FormFlowType = WarehouseTaskFormFlowTypes.UnloadJob;
			SetDefaultTaskDetails(unloadTask, $"Temporary Unload Task Description", staff.GS_Code, isWorking: true);
			receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;

			return unloadTask;
		}

		static void SetDefaultTaskDetails(ProcessTask task, string description, ZString userNK, bool isWorking)
		{
			task.P9_Description = description;
			task.P9_Type = Core.Constants.Workflow.UndefinedTaskType;

			var originalUserInteractive = Globals.IsUserInteractive;
			try
			{
				// Current code does not allow to set user if user is system admin (support user is also system admin)
				Globals.IsUserInteractive = true;
				task.P9_GS_NKAssignedStaffMember = userNK;
			}
			finally
			{
				Globals.IsUserInteractive = originalUserInteractive;
			}

			task.P9_Status = isWorking ? ProcessTaskStatusCodeList.Codes.Working : ProcessTaskStatusCodeList.Codes.Assigned;
		}

		#endregion

		#endregion
	}
}
