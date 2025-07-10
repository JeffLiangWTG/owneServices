using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region PopulateAndFinaliseTransfers

		[WebMethod(Description = "Populate Warehouse Transfers and Finalise them")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsTransfersWebServiceResponse PopulateAndFinaliseTransfers(Guid[] transferPKs, string destinationLocation, Guid processTaskPK)
		{
			return HandleWebServiceRequest<WhsTransfersWebServiceResponse>(result =>
			{
				var connection = ((IDbConnected)Factory).Connection;
				using (var manager = connection.BeginTransactionWithManager())
				{
					PopulateAndFinaliseTransfersCore(result, transferPKs, destinationLocation, processTaskPK);
					if (result.NoError())
					{
						manager.CommitTransaction();
					}
					else
					{
						manager.RollbackTransaction();
					}
				}
			});
		}

		void PopulateAndFinaliseTransfersCore(WebServiceResponse response, Guid[] transferPKs, string destinationLocation, Guid processTaskPK)
		{
			var errors = new StringBuilder();
			var processTaskPKsToDelete = processTaskPK != Guid.Empty
				? GetProcessTaskPKsToDelete(response, transferPKs, processTaskPK, Factory)
				: new HashSet<ZGuid>();

			if (response.NoError())
			{
				foreach (var transferPK in transferPKs)
				{
					var transfer = Factory.Load<WhsTransfer>(transferPK);
					if (transfer.CheckTransferIsMasterTransfer(response))
					{
						if (!response.NoError())
						{
							break;
						}
						PopulateTransfer(transfer, destinationLocation, processTaskPK);
						transfer.AddEvents(ZArchitecture.Business.Events.ServiceCompleted);
						transfer.RunPreSaveValidation();
						if (!transfer.NotificationsIncludingChildren.HasErrors())
						{
							WebServiceHelper.FinaliseAndSaveDocket(Factory, transfer, Res.GetString("29022982-0055-40fa-832b-fc176334a665", "Transfer"));
						}
						if (!transfer.IsFinalised)
						{
							errors.Append(GetErrorMessageForDocket(transfer));
						}
					}
				}

				if (response.NoError())
				{
					var allErrors = errors.ToString();
					if (!allErrors.IsNullOrEmpty())
					{
						response.LogBusinessValidationError(allErrors);
					}
					else
					{
						var processTasks = Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.PK, processTaskPKsToDelete));
						ProcessTaskHelper.DeleteProcessTasksAndRelatedProcessHeader(Factory, processTasks);
						WebServiceHelper.SaveFactoryWithExceptionHandling(
							Factory,
							response,
							(concurrencyException) => Res.GetString("c4ed4398-5f0c-4746-a8c3-739903dd39b3", "Another user has changed the transfer while you have been working on it. Please restart the operation and try again."));
					}
				}
			}
		}

		static HashSet<ZGuid> GetProcessTaskPKsToDelete(WebServiceResponse response, Guid[] transferPKs, Guid processTaskPK, BusinessObjectFactory factory)
		{
			var distinctTransferPKs = transferPKs.Distinct().ToArray();
			var transfers = factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.PK, distinctTransferPKs));
			var relatedProcessTaskPKs = transfers
				.SelectMany(t => t.Lines)
				.Select(l => l.WE_P9_Task)
				.ToHashSet();

			if (!relatedProcessTaskPKs.Contains(processTaskPK))
			{
				response.LogBusinessValidationError(Res.GetString("572967a1-55ab-4d3b-8d9d-00e1b31ec67d", "Process task does not match any of the transfers to finalize."));
			}
			
			return response.NoError()
				? relatedProcessTaskPKs.Where(pk => pk != processTaskPK).ToHashSet()
				: new HashSet<ZGuid>();
		}

		void PopulateTransfer(WhsTransfer transfer, string destinationLocation, Guid processTaskPK)
		{
			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			foreach (var transferLine in transfer.Lines)
			{
				PutAwayTransferLine(transferLine, transfer.WD_WW_Whs, transfer.WD_OH_Client, staff, destinationLocation, transferLine.WE_TransferFromPalletId);
				if (processTaskPK != Guid.Empty && transferLine.WE_P9_Task != processTaskPK)
				{
					transferLine.WE_P9_Task = processTaskPK;
				}
			}
		}

		void PutAwayTransferLine(WhsDocketLine transferLine, ZGuid warehousePK, ZGuid clientPK, GlbStaff staff, string destinationLocation, string destinationPalletId)
		{
			LogTransferLineChange(transferLine, destinationLocation, destinationPalletId);
			transferLine.LocationString = destinationLocation;
			transferLine.WE_PalletID = ShouldRetainPalletID(warehousePK, clientPK, transferLine.Product, transferLine.LocationString) ? destinationPalletId : "";
			transferLine.WE_GS_NKPutawayBy = staff.GS_Code;
		}

		static void LogTransferLineChange(WhsDocketLine transferLine, string destinationLocation, string destinationPalletID)
		{
			var builder = new ZStringBuilder("");

			var transferLineDestination = transferLine.Location;
			if (transferLineDestination != null
				&& !WhsLocationHelper.LocationStringCompare(transferLineDestination, destinationLocation))
			{
				builder.Append(string.Format(Culture.Invariant, (NoResString)"Dest. Location changed from '{0}';", transferLine.LocationString)); // RF Log message.
			}

			if (!transferLine.WE_PalletID.IsEmpty && transferLine.WE_PalletID.ToUpperInvariant() != destinationPalletID.ToUpperInvariant())
			{
				builder.Append(string.Format(Culture.Invariant, (NoResString)"Dest. Pallet ID changed from '{0}';", transferLine.WE_PalletID)); // RF Log message.
			}

			if (builder.Length > 0)
			{
				builder.Prepend(string.Format(Culture.Invariant, (NoResString)"RF: Line No. {0} - [", transferLine.WE_LineNo));  // RF Log message.
				builder.Append("]");  // RF Log message.
				transferLine.Docket.Logs.AddNew(ZArchitecture.Business.Events.ChangeOfIdentifier, builder.ToString());
			}
		}

		bool ShouldRetainPalletID(ZGuid warehousePK, ZGuid clientPK, WhsProduct product, string location)
		{
			var pickFace = product.PickFaces.Cast<WhsPickFace>().FirstOrDefault(x =>
				x.Warehouse.PK == warehousePK &&
				x.WF_OH_Client == clientPK &&
				WhsLocationHelper.LocationStringCompare(x.Location, location)
			);
			return pickFace?.Location.LocationType.WLT_RetainPalletIDsInFixedPickFaces ?? true;
		}

		#endregion

	}
}
