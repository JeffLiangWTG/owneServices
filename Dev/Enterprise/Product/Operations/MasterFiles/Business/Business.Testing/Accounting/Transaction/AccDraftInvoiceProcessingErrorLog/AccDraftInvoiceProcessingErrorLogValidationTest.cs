using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccDraftInvoiceProcessingErrorLogValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAIL_Code()
		{
			var bizO = Factory.New<AccDraftInvoiceProcessingErrorLog>();

			var codes = AccDraftInvoiceProcessingErrorCodes.GetAllErrorCodes();
			foreach(var code in codes)
			{
				bizO.AIL_Code = code;
				AssertNoErrors(bizO.AIL_CodeInfo);
			}

			AssertEquals("PreCondition", false, codes.Contains("AAA"));
			bizO.AIL_Code = "AAA";
			AssertHasError(bizO.AIL_CodeInfo, "Enter a valid selection.");
		}
	}
}
