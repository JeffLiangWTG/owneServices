using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public abstract class JobComInvoiceLineValidation_InwardTest : JobComInvoiceLineValidationTest
	{
		public void TestJI_PrimaryPreference()
		{
			InvoiceLine.Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			InvoiceLine.JI_PrimaryPreference = ZString.Empty;
			AssertNoMessageErrors("User can blank out this field", InvoiceLine.JI_PrimaryPreferenceInfo);
			InvoiceLine.JI_PrimaryPreference = "123";
			AssertHasMessageError(InvoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.STD;
			AssertNoMessageError(InvoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.PRI;
			AssertHasMessageError("PRI is not valid for Import", InvoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.PRF;
			AssertNoMessageErrorContaining(InvoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
