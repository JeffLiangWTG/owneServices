using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class GroupInvoiceChargeValidationTest : TestCaseWithFactory
	{
		public void TestValidateONSAndOFTForCIFInvoice()
		{
			var groupCharge = groupHeader.Charges.AddNew();
			groupCharge.J7_ChargeType = Core.Constants.Customs.CustomsCharges.Codes.OverseasFreight;
			AssertEquals("OFT cannot be on CIF invoice as it will be rejected by SG Customs", true, groupCharge.J7_ChargeTypeInfo.HasMessageErrors());
			AssertHasMessageError(groupCharge.J7_ChargeTypeInfo, "Freight and Insurance is not permitted when the INCO Term is CIF.");
			groupCharge.J7_ChargeType = Core.Constants.Customs.CustomsCharges.Codes.OverseasInsurance;
			AssertEquals("ONS cannot be on CIF invoice as it will be rejected by SG Customs", true, groupCharge.J7_ChargeTypeInfo.HasMessageErrors());
			AssertHasMessageError(groupCharge.J7_ChargeTypeInfo, "Freight and Insurance is not permitted when the INCO Term is CIF.");
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			var groupCharge2 = groupHeader.Charges.AddNew();
			groupCharge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertNoMessageError(groupCharge2.J7_ChargeTypeInfo, "Freight and Insurance is not permitted when the INCO Term is CIF.");
		}

		JobDeclaration testDec;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceGroupHeader groupHeader;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			invoiceHeader = (JobComInvoiceHeader)groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
		}
	}
}
