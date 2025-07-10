using System;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Set IsPicking = false when user suspend the transfer.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse SuspendPickingForTransfer(Guid transferPK)
		{
			return HandleWebServiceRequest<WebServiceResponse>(r => SuspendPickingForTransferCore(r, transferPK));
		}

		void SuspendPickingForTransferCore(WebServiceResponse response, Guid transferPK)
		{
			var transfer = Factory.Load<WhsTransfer>(transferPK);
			if (transfer != null)
			{
				if (!transfer.IsFinalised)
				{
					var transferLines = transfer.Lines.Cast<WhsTransferLine>().Where(l => !l.IsPicked);
					if (transferLines.Any())
					{
						foreach (var transferLine in transferLines.Where(tl => !tl.WE_GS_NKPutawayBy.IsEmpty))
						{
							transferLine.WE_GS_NKPutawayBy = string.Empty;
						}

						var pickLines = transferLines.SelectMany(l => l.PickLines.Where(pl => pl.WZ_IsPicking));
						var pickLinesInMatchingLines = transfer.Lines.Cast<WhsTransferLine>().SelectMany(l => l.MatchingLines.SelectMany(ml => ml.PickLines.Where(pl => pl.WZ_IsPicking)));
						foreach (var pickLine in pickLines.Concat(pickLinesInMatchingLines))
						{
							pickLine.WZ_IsPicking = false;
							pickLine.WZ_GS_NKAssignedTo = string.Empty;
						}

						Factory.Save();
					}
				}
				else
				{
					response.ErrorMessage = Res.GetString("05a2a977-028b-4370-a692-26ab5eae2667", "Transfer is finalized, cannot set Is Picking.");
					response.Error = ErrorTypes.BusinessValidationError;
				}
			}
			else
			{
				response.ErrorMessage = Res.GetString("bb7bb328-414e-4ee9-9443-ebba4f059ad8", "Cannot find out Transfer.");
				response.Error = ErrorTypes.BusinessValidationError;
			}
		}
	}
}
