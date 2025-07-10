using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobComInvoiceHeaderValidation_INPTest : JobComInvoiceHeaderValidation_InwardTest
	{
		public void TestSupplier()
		{
			Validation.ValidateJZ_OH_Supplier();
			AssertEquals(false, InvoiceHeader.JZ_OH_SupplierInfo.HasMessageErrors());
			JobComInvoiceLine invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
			Validation.ValidateJZ_OH_Supplier();
			Assert("JZ_OH_Supplier has no message error", !InvoiceHeader.JZ_OH_SupplierInfo.HasMessageErrors());
			Assert("No preference", !InvoiceHeader.HasPreferentialDuty);
			AssertEquals("Default value of duty preference is 'STD'", "STD", invoiceLine.JI_PrimaryPreference);
			invoiceLine.JI_PrimaryPreference = "";
			Validation.ValidateJZ_OH_Supplier();
			AssertEquals(false, InvoiceHeader.JZ_OH_SupplierInfo.HasMessageErrors());
			invoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.PRF;
			Validation.ValidateJZ_OH_Supplier();
			AssertEquals(true, InvoiceHeader.JZ_OH_SupplierInfo.HasMessageErrors());
			OrgHeader supplier = Factory.New<OrgHeader>();
			InvoiceHeader.JZ_OH_Supplier = supplier.PK;
			supplier.OH_IsConsignor = true;
			Validation.ValidateJZ_OH_Supplier();
			supplier.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			supplier.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Validation.ValidateJZ_OH_Supplier();
			AssertEquals(false, InvoiceHeader.JZ_OH_SupplierInfo.HasMessageErrors());
		}

		protected override string MessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.INP;
			}
		}
	}
}
