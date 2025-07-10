using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class TestQuery : IAccountingAP_ARInvoiceQuery
	{
		public AP_ARInvoiceQueryResult Result;

		public void ClearServiceCache(BusinessObjectFactory factory) { }

		public AP_ARInvoiceQueryResult GetInvoiceAmount(ICustomsJobInfo declaration, List<ZGuid> chargeCodesToMatch)
		{
			return Result;
		}

		public AP_ARInvoiceQueryResult GetPostingDetails(ICustomsJobInfo declaration, ZGuid[] chargeCodesToMatch, bool postAP, bool postAR)
		{
			return Result;
		}

		public AP_ARInvoiceQueryResult GetTotalInvoicedDetails(ICustomsJobInfo declaration, List<ZGuid> chargeCodesToMatch, string descToMatch = "")
		{
			return Result;
		}
	}
}
