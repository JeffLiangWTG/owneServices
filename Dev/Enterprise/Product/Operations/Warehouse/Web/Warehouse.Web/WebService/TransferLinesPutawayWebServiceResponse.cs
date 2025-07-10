using System;
using System.Collections.Generic;

namespace Enterprise.Warehouse.Web.WebService
{
	public class TransferLinesPutawayWebServiceResponse : TransferPutawayWebServiceResponse
	{
		public TransferLinesPutawayWebServiceResponse()
			: base()
		{
		}

		public List<Guid> NonFinalisedTransferLinesPks
		{
			get { return nonFinalisedTransferLinesPks ?? (nonFinalisedTransferLinesPks = new List<Guid>()); }
		}
		List<Guid> nonFinalisedTransferLinesPks;
	}
}
