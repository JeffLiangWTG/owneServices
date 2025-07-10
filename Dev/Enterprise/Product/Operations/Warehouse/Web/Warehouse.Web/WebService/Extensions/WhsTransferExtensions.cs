using CargoWise.Common;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	public static class WhsTransferExtensions
	{
		public static bool CheckTransferIsMasterTransfer(this WhsTransfer transfer, WebServiceResponse response)
		{
			Argument.NotNull(transfer, nameof(transfer));

			if (!transfer.IsMasterTransfer)
			{
				response.LogBusinessValidationError(Res.GetString("E6673BF9-F3C7-471B-919B-E47460FC68C0", "Cannot transfer an Inter-Warehouse Transfer using the child job."));
			}
			return response.NoError();
		}
	}
}
