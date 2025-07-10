using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GetWhsTransferLines

		[WebMethod(Description = "Get limited number of transfer lines")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsTransferLineCollectionWebServiceResponse GetWhsTransferLines(Guid[] transferPKs)
		{
			return HandleWebServiceRequest<WhsTransferLineCollectionWebServiceResponse>(r => GetWhsTransferLinesCore(r, transferPKs));
		}

		void GetWhsTransferLinesCore(WhsTransferLineCollectionWebServiceResponse response, Guid[] transferPKs)
		{
			const int firstTransferLinesToLoad = 500;
			foreach (var transferPK in transferPKs)
			{
				var transfer = Factory.Load<WhsTransfer>(transferPK);
				response.TotalTransferLinesToLoad += transfer.Lines.Count;

				if (transfer.CheckTransferIsMasterTransfer(response))
				{
					foreach (WhsTransferLine transferLine in transfer.Lines)
					{
						if (response.LineInfoCollection.Count < firstTransferLinesToLoad)
						{
							var transferLineInfo = new WhsDocketLineInfo(transferLine);
							response.LineInfoCollection.Add(transferLineInfo);
						}
						else
						{
							break;
						}
					}
				}
				else
				{
					break;
				}
			}
		}

		#endregion

	}
}
