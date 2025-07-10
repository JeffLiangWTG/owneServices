using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobComInvoiceLineValidation_OUTTest : JobComInvoiceLineValidationTest
	{
		protected override string MessageType => MessageTypeCodeList.Codes.OUT;
		public void TestJI_PrimaryPreference()
		{
			InvoiceLine.Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			InvoiceLine.JI_PrimaryPreference = ZString.Empty;
			AssertNoMessageErrors("User can blank out field", InvoiceLine.JI_PrimaryPreferenceInfo);
			InvoiceLine.JI_PrimaryPreference = "123";
			AssertHasMessageError(InvoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.PRF;
			AssertHasMessageError("PRF is not valid for Export", InvoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.STD;
			AssertNoMessageError(InvoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.PRI;
			AssertNoMessageError(InvoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
