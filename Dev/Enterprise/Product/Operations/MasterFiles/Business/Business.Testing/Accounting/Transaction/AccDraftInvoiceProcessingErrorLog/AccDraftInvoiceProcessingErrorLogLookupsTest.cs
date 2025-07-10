using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccDraftInvoiceProcessingErrorLogLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestErrorCodeList()
		{
			var lookups = Factory.New<AccDraftInvoiceProcessingErrorLog>().Lookups;
			var codes = AccDraftInvoiceProcessingErrorCodes.GetAllErrorCodes();

			AssertContainsExactElementsInAnyOrder(codes, lookups.ErrorCodeList.GetAllCodes());
		}
	}
}
