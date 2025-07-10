using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseGroupInvoiceChargeValidationTest : TestCaseWithFactory
	{
		public void TestValidateJ7_Calc_IsIncludedInITOTForForeignInlandFreight()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.AutoCreateChargesBasedOnIncoTerm = false;

			BaseGroupInvoiceCharge groupCharge = declaration.TopGroupInvoice.Charges.AddNew();
			groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
			groupCharge.J7_Amount = 100m;

			BaseJobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			BaseJobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeCarrier;

			groupCharge.J7_Calc_IsIncludedInITOT = GroupIsIncludedInLinesOptionList.Codes.Yes;//cannot be the case for ExWorks invoice

			((BaseGroupInvoiceChargeValidation)groupCharge.Validation).ValidateJ7_Calc_IsIncludedInITOT();
			AssertHasMessageError(groupCharge.J7_Calc_IsIncludedInITOTInfo, BaseGroupInvoiceChargeValidation.IsIncludedInLinesCannotBeAppliedForAllInvoices);

			groupCharge.J7_Calc_IsIncludedInITOT = GroupIsIncludedInLinesOptionList.Codes.No;

			((BaseGroupInvoiceChargeValidation)groupCharge.Validation).ValidateJ7_Calc_IsIncludedInITOT();
			AssertHasMessageError(groupCharge.J7_Calc_IsIncludedInITOTInfo, BaseGroupInvoiceChargeValidation.IsIncludedInLinesCannotBeAppliedForAllInvoices);

			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			((BaseGroupInvoiceChargeValidation)groupCharge.Validation).ValidateJ7_Calc_IsIncludedInITOT();
			AssertNoMessageError(groupCharge.J7_Calc_IsIncludedInITOTInfo, BaseGroupInvoiceChargeValidation.IsIncludedInLinesCannotBeAppliedForAllInvoices);
		}

		public void TestValidateGroupChargeBalance()
		{
			BaseJobComInvoiceHeader invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, groupHeader.JobDeclaration.LocalCurrencyCode);
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = groupHeader.JobDeclaration.LocalCurrencyCode;
			invoice1.JZ_IncoTerm = "FOB";

			BaseJobComInvoiceHeader invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_InvoiceAmount = 20000m;
			invoice2.JZ_RX_NKInvoice_Currency = groupHeader.JobDeclaration.LocalCurrencyCode;
			invoice2.JZ_IncoTerm = "FOB";

			groupCharge = groupHeader.Charges.AddNew();
			groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			groupCharge.J7_Amount = 1000m;
			groupCharge.J7_RX_NKCurrency = groupHeader.JobDeclaration.LocalCurrencyCode;

			groupCharge.RunPreSaveValidation();
			AssertEquals("Group Charge is apportioned and balanced", false, groupCharge.J7_AmountInfo.HasWarnings());
		}

		public void TestValidateApportion()
		{
			BaseJobComInvoiceGroupHeader childInvoiceGroup = groupHeader.JobComInvoiceGroupHeaders.AddNew();
			BaseGroupInvoiceCharge childCharge = childInvoiceGroup.Charges.AddNew();
			childCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			childCharge.J7_Amount = 100;
			childCharge.J7_RX_NKCurrency = groupHeader.JobDeclaration.LocalCurrencyCode;
			AssertEquals("No Invoices on Job at all, validation skipped", false, childCharge.J7_ChargeTypeInfo.HasMessageErrors());

			BaseJobComInvoiceHeader headerInvoice = testDec.Invoices.AddNew();
			headerInvoice.JZ_JZ_GroupInvoiceFK = groupHeader.PK;
			childCharge.RunPreSaveValidation();
			AssertEquals("No Invoices to apportion this value to but there are Invoices on the Job. Should have a Message Error.", true, childCharge.J7_ChargeTypeInfo.HasMessageErrors());

			BaseJobComInvoiceHeader childInvoice = testDec.Invoices.AddNew();
			childInvoice.JZ_JZ_GroupInvoiceFK = childInvoiceGroup.PK;
			childCharge.RunPreSaveValidation();
			AssertEquals("One invoice available for apportionment now. Should be no Message Errors.", false, childCharge.J7_ChargeTypeInfo.HasMessageErrors());

			childInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000, groupHeader.JobDeclaration.LocalCurrencyCode);
			childCharge.RunPreSaveValidation();
			AssertEquals("This invoice has its own charge. Cannot Apportion.", true, childCharge.J7_ChargeTypeInfo.HasMessageErrors());
		}

		public void TestValidateNonDutiablePreFOBChargeWithFCLContainer()
		{
			groupCharge = groupHeader.Charges.AddNew();
			groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
			groupCharge.J7_IsDutiable = false;
			AssertEquals("No FCL container", true, groupCharge.J7_IsDutiableInfo.HasNotifications());

			BaseCusContainer fCLContainer = testDec.CusContainers.AddNew();
			fCLContainer.CO_FCL_LCL_AIR = "FCL";

			groupCharge.RunPreSaveValidation();
			AssertEquals("FCL container", false, groupCharge.J7_IsDutiableInfo.HasWarnings());
		}

		public void TestValidateLandingChargeApportionable()
		{
			BaseJobComInvoiceHeader invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice.JZ_IncoTerm = "FOB";

			groupCharge = groupHeader.Charges.AddNew();
			groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
			AssertEquals("No invoice can have LCH apportioned", true, groupCharge.J7_ChargeTypeInfo.HasNotifications());
		}

		public void TestWeGetAnErrorIfWeHaveAGroupChargeAcrossInvoicesWithDifferentValuationDates()
		{
			testDec.JE_ExportDate = new ZDateTime(2005, 8, 16);
			BaseJobComInvoiceHeader invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_ValuationDateOverride = new ZDateTime(2005, 8, 16);

			groupCharge = groupHeader.Charges.AddNew();
			groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
			groupCharge.J7_RX_NKCurrency = "USD";
			groupCharge.J7_Amount = 100m;

			AssertMultiValuationDateError(groupCharge, false);
			BaseJobComInvoiceHeader invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			AssertMultiValuationDateError(groupCharge, false);
			invoice2.JZ_ValuationDateOverride = new ZDateTime(2005, 8, 17);
			AssertMultiValuationDateError(groupCharge, true);
			invoice1.JZ_ValuationDateOverride = new ZDateTime(2005, 8, 17);
			AssertMultiValuationDateError(groupCharge, false);
		}

		void AssertMultiValuationDateError(BaseGroupInvoiceCharge groupCharge, bool expectError)
		{
			groupCharge.Validation.ValidateJ7_Amount();

			ZStringBuilder result = new ZStringBuilder();
			result.Append("The system will be using the Declaration valuation date (");
			result.Append(groupCharge.GroupInvoice.JobDeclaration.DateOfValuation.ToShortDateString());
			result.Append(") for apportionment purposes.\r\nThe calculation will be approximate as the invoices have different valuation dates.");
			result.Append(" If you want to override the apportionment result, you can do so by entering values at Line Charges.");

			if (expectError)
			{
				AssertHasWarning(groupCharge.J7_AmountInfo, result.ToString());
			}
			else
			{
				AssertNoWarning(groupCharge.J7_AmountInfo, result.ToString());
			}
		}

		#region Implementation

		BaseJobDeclaration testDec;
		BaseJobComInvoiceGroupHeader groupHeader;
		BaseGroupInvoiceCharge groupCharge;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<BaseJobDeclaration>();
			groupHeader = testDec.JobComInvoiceGroupHeaders[0];
		}

		#endregion
	}
}
