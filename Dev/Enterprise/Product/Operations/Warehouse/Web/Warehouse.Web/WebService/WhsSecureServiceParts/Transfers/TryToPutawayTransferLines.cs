using System;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region TryToPutawayTransferLines

		[WebMethod(Description = "Putaway selected transfer lines.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public TransferLinesPutawayWebServiceResponse TryToPutawayTransferLines(Guid transferPK, WhsDocketLineInfo firstDocketLineInfo, Guid[] transferLinePksToPutaway, Guid transferTaskPK)
		{
			return HandleWebServiceRequest<TransferLinesPutawayWebServiceResponse>(r => TryToPutawayTransferLinesCore(r, transferPK, firstDocketLineInfo, transferLinePksToPutaway, transferTaskPK));
		}

		void TryToPutawayTransferLinesCore(TransferLinesPutawayWebServiceResponse response, Guid transferPK, WhsDocketLineInfo firstDocketLineInfo, Guid[] transferLinePksToPutaway, Guid transferTaskPK)
		{
			ValidateDocketLineInfo(response, firstDocketLineInfo);
			if (string.IsNullOrEmpty(response.ErrorMessage))
			{
				var transfer = Factory.Load<WhsTransfer>(transferPK);
				if (transfer != null)
				{
					PutawayTransferLines(response, transfer, firstDocketLineInfo, transferLinePksToPutaway, transferTaskPK);
				}
				else
				{
					response.LogBusinessValidationError(Res.GetString("51442CC2-3891-4960-8A4D-236F4C7D0F1C", "Transfer with PK = '{0}' cannot be found.", transferPK));
				}
			}
			else
			{
				response.NonFinalisedTransferLinesPks.AddRange(transferLinePksToPutaway);
			}
		}

		void PutawayTransferLines(TransferLinesPutawayWebServiceResponse response, WhsTransfer transfer, WhsDocketLineInfo firstDocketLineInfo, Guid[] transferLinePksToPutaway, Guid transferTaskPK)
		{
			if (transfer.CheckTransferIsMasterTransfer(response))
			{
				var transferLinesToPutaway = transfer.Lines.Cast<WhsTransferLine>().Where(l => transferLinePksToPutaway.Contains(l.PK.ToGuid())).ToArray();
				FinaliseTransferLines(response, transfer, transferLinesToPutaway, firstDocketLineInfo, true, true, transferTaskPK);

				var nonFinalisedTransferLinePks = transferLinesToPutaway.Where(l => !l.IsFinalised).Select(l => l.PK.ToGuid());
				response.NonFinalisedTransferLinesPks.AddRange(nonFinalisedTransferLinePks);
			}
		}

		#endregion
	}
}
