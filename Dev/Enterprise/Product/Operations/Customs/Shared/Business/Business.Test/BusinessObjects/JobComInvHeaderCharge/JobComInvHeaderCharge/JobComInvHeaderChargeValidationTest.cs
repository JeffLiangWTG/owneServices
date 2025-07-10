using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	public class JobComInvHeaderChargeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateListValidationForDistributeBy()
		{
			BaseJobComInvHeaderCharge charge = invoice.Charges.AddNew();
			charge.J7_DistributeBy = "XXX";
			AssertHasMessageError(charge.J7_DistributeByInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestValidateChargeType()
		{
			BaseJobComInvHeaderCharge charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = "XXX";
			AssertEquals("Charge is not valid", true, charge.J7_ChargeTypeInfo.HasNotifications());
		}

		public void TestValidateJ7_Amount()
		{
			BaseJobComInvHeaderCharge charge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges);
			charge.J7_Amount = -1000m;
			AssertEquals("Amount should be positive", true, charge.J7_AmountInfo.HasErrors());
		}

		public void TestValidateJ7_Percentage()
		{
			BaseJobComInvHeaderCharge charge1 = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.Commission);
			charge1.J7_Percentage = -20;
			AssertEquals("Invalid Percentage", true, charge1.J7_PercentageInfo.HasErrors());

			charge1.J7_Percentage = 110;
			AssertEquals("Invalid Percentage", true, charge1.J7_PercentageInfo.HasErrors());

			charge1.J7_Percentage = 20;
			AssertEquals("Valid Percentage", false, charge1.J7_PercentageInfo.HasErrors());

			charge1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals("OFT cannot have a percentage value", true, charge1.J7_PercentageInfo.HasErrors());

			charge1.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			charge1.J7_Percentage = 1.12345m;
			Factory.Save();
			var charge = Factory.LoadTop1<BaseJobComInvHeaderCharge>(new ZQuery(JobComInvHeaderChargeSchema.PK, charge1.PK));
			AssertEquals("J7_Percentage should be 5 decimal places", 1.12345m, charge.J7_Percentage);
		}

		public void TestValidateNonDutiablePreFOBCharge()
		{
			BaseJobComInvHeaderCharge charge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 100, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsDutiable = false;
			charge.J7_IsGSTApplicable = false;
			AssertEquals("Non dutiable charge without FCL container", true, charge.J7_IsDutiableInfo.HasNotifications());
		}

		public void TestValidateOverseasFreightFlags()
		{
			BaseJobComInvHeaderCharge oFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, invoice.JobDeclaration.LocalCurrencyCode);
			oFT.J7_IsDutiable = true;
			AssertEquals("OFT is usually not dutiable", true, oFT.J7_IsDutiableInfo.HasMessageErrors());

			oFT.J7_IsGSTApplicable = false;
			AssertEquals("OFT is usually GST applicable", true, oFT.J7_IsGSTApplicableInfo.HasMessageErrors());
		}

		public void TestValidateLandingChargeFlags()
		{
			BaseJobComInvHeaderCharge lCH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 100, invoice.JobDeclaration.LocalCurrencyCode);
			lCH.J7_IsDutiable = true;
			AssertEquals("LCH is usually not dutiable", true, lCH.J7_IsDutiableInfo.HasMessageErrors());

			lCH.J7_IsGSTApplicable = true;
			AssertEquals("LCH is usually not GST applicable", true, lCH.J7_IsGSTApplicableInfo.HasMessageErrors());
		}

		public void TestValidateOtherChargeFlags()
		{
			BaseJobComInvHeaderCharge oTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100, invoice.JobDeclaration.LocalCurrencyCode);
			oTH.J7_IsDutiable = false;
			AssertEquals("OTH can be either", false, oTH.J7_IsGSTApplicableInfo.HasMessageErrors());

			oTH.J7_IsGSTApplicable = false;
			AssertEquals("OTH can be either", false, oTH.J7_IsDutiableInfo.HasMessageErrors());
		}

		public void TestValidateForeignInlandFreightFlags()
		{
			BaseJobComInvHeaderCharge fIFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 100, invoice.JobDeclaration.LocalCurrencyCode);
			fIFT.J7_IsGSTApplicable = false;
			AssertEquals("FIFT is usually GST applicable", true, fIFT.J7_IsGSTApplicableInfo.HasMessageErrors());

			fIFT.J7_IsDutiable = false;
			AssertEquals("FIFT can be either", false, fIFT.J7_IsDutiableInfo.HasMessageErrors());
		}

		public void TestValidatePackingChargeFlags()
		{
			BaseJobComInvHeaderCharge pC = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100, invoice.JobDeclaration.LocalCurrencyCode);
			pC.J7_IsGSTApplicable = false;
			AssertEquals("PC is usually GST applicable", true, pC.J7_IsGSTApplicableInfo.HasMessageErrors());
			pC.J7_IsDutiable = false;
			AssertEquals("PC is usually Dutiable", true, pC.J7_IsDutiableInfo.HasMessageErrors());
		}

		#region Implementation
		BaseJobDeclaration testDec;
		BaseJobComInvoiceGroupHeader groupHeader;
		BaseJobComInvoiceHeader invoice;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<BaseJobDeclaration>();
			groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			invoice = groupHeader.JobComInvoiceHeaders.AddNew();
		}
		#endregion
	}
}
