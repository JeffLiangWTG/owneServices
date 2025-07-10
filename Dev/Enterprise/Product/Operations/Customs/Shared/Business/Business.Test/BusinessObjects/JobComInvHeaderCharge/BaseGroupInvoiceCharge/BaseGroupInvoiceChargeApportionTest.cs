using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseGroupInvoiceChargeApportionTest : TestCaseWithFactory
	{
		public void TestJ7_Calc_IsIncludedInITOT()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var groupCharge = declaration.TopGroupInvoice.Charges.AddNew();
			groupCharge.J7_ChargeType = "OFT";
			groupCharge.J7_Amount = 100m;
			groupCharge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;

			CombineAssertions(() =>
			{
				AssertEquals("Is Included In Lines is readonly", true, groupCharge.J7_Calc_IsIncludedInITOT_ReadOnly);
				AssertEquals("Is Included in lines should return 'N/A'", GroupIsIncludedInLinesOptionList.Codes.NotApplicable, groupCharge.J7_Calc_IsIncludedInITOT);

				groupCharge.J7_ChargeType = "OTH";
				AssertEquals("Is Included In Lines is readonly", false, groupCharge.J7_Calc_IsIncludedInITOT_ReadOnly);
				AssertEquals("For incoterm-neutral, chances are group charges are likely to be excluded from invoice & lines", false, groupCharge.J7_IsIncludedInITOT);

				groupCharge.J7_Calc_IsIncludedInITOT = GroupIsIncludedInLinesOptionList.Codes.Yes;
				AssertEquals("Is Included in lines should return 'N/A'", GroupIsIncludedInLinesOptionList.Codes.Yes, groupCharge.J7_Calc_IsIncludedInITOT);

				groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
				AssertEquals("Is Included In Lines is readonly", true, groupCharge.J7_Calc_IsIncludedInITOT_ReadOnly);
				AssertEquals("Is Included in lines should return 'N/A'", GroupIsIncludedInLinesOptionList.Codes.NotApplicable, groupCharge.J7_Calc_IsIncludedInITOT);

				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
				AssertEquals("Is Included in lines should return 'N/A'", GroupIsIncludedInLinesOptionList.Codes.NotApplicable, groupCharge.J7_Calc_IsIncludedInITOT);

				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				AssertEquals("Is Included in lines should return 'N/A'", GroupIsIncludedInLinesOptionList.Codes.NotApplicable, groupCharge.J7_Calc_IsIncludedInITOT);

				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeCarrier;
				AssertEquals("Is Included In Lines is editable and should be indicated by users", false, groupCharge.J7_Calc_IsIncludedInITOT_ReadOnly);
				AssertEquals("Is Included in lines is set", GroupIsIncludedInLinesOptionList.Codes.Yes, groupCharge.J7_Calc_IsIncludedInITOT);
			});
		}

		public void TestOverseasInsuranceForDeliveryIncoterms()
		{
			void AssertIsIncludedInITOT(string incoterm, string messageForReadOnly, bool isReadOnly, string expectedValue)
			{
				var declaration = Factory.New<BaseJobDeclaration>();

				if (!string.IsNullOrWhiteSpace(incoterm))
				{
					var invoice = declaration.Invoices.AddNew();
					invoice.JZ_IncoTerm = incoterm;
				}

				var groupCharge = declaration.TopGroupInvoice.Charges.AddNew();
				groupCharge.J7_ChargeType = "ONS";
				groupCharge.J7_Amount = 100m;
				groupCharge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;

				AssertEquals(messageForReadOnly, isReadOnly, groupCharge.J7_Calc_IsIncludedInITOT_ReadOnly);
				AssertEquals("Default value is set to", expectedValue, groupCharge.J7_Calc_IsIncludedInITOT);
			}

			var message = "ReadOnly before any invoice is entered.";
			CombineAssertions(() =>
			{
				AssertIsIncludedInITOT(string.Empty, message, true, GroupIsIncludedInLinesOptionList.Codes.NotApplicable);

				message = "Editable as ONS can be part of invoice or not. Users should indicate.";

				AssertIsIncludedInITOT(Core.Constants.IncoTerms.DeliveredAtPlace, message, false, GroupIsIncludedInLinesOptionList.Codes.Yes);
				AssertIsIncludedInITOT(Core.Constants.IncoTerms.DeliveredAtTerminal, message, false, GroupIsIncludedInLinesOptionList.Codes.Yes);
				AssertIsIncludedInITOT(Core.Constants.IncoTerms.DeliveredDutyPaid, message, false, GroupIsIncludedInLinesOptionList.Codes.Yes);

				message = "ReadOnly as system calculates the flag at each invoice.";

				AssertIsIncludedInITOT(Core.Constants.IncoTerms.FreeOnBoard, message, true, GroupIsIncludedInLinesOptionList.Codes.NotApplicable);
			});
		}

		public void TestJ7_IsIncludedInITOTReadonly()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			var groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			var groupCharge = groupHeader.Charges.AddNew();
			groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			CombineAssertions(() =>
			{
				AssertEquals(false, groupCharge.J7_IsIncludedInITOTInfo.ReadOnly);

				groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.PackingCost;
				AssertEquals(false, groupCharge.J7_IsIncludedInITOTInfo.ReadOnly);

				groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
				AssertEquals(false, groupCharge.J7_IsIncludedInITOTInfo.ReadOnly);

				groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
				AssertEquals(false, groupCharge.J7_IsIncludedInITOTInfo.ReadOnly);

				groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				AssertEquals(true, groupCharge.J7_IsIncludedInITOTInfo.ReadOnly);
			});
		}

		public void TestApportionmentDirtyWhenAmountChanges()
		{
			CombineAssertions(() =>
			{
				testDec.ApportionmentDirty = false;
				AssertEquals("JobDeclaration Apportionment is not dirty", false, testDec.ApportionmentDirty);

				var testCharge = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
				testCharge.J7_Amount = 100m;
				AssertEquals("JobDeclaration Apportionment is dirty now", true, testDec.ApportionmentDirty);
			});
		}

		public void TestApportionmentDirtyWhenTypeChanges()
		{
			CombineAssertions(() =>
			{
				testDec.ApportionmentDirty = false;
				AssertEquals("JobDeclaration Apportionment is not dirty", false, testDec.ApportionmentDirty);

				var testCharge = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
				testCharge.J7_ChargeType = "OTH";
				AssertEquals("JobDeclaration Apportionment is dirty now", true, testDec.ApportionmentDirty);
			});
		}

		public void TestApportionmentDirtyWhenCurrencyChanges()
		{
			CombineAssertions(() =>
			{
				testDec.ApportionmentDirty = false;
				AssertEquals("JobDeclaration Apportionment is not dirty", false, testDec.ApportionmentDirty);

				var testCharge = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
				testCharge.J7_RX_NKCurrency = "AUD";
				AssertEquals("JobDeclaration Apportionment is dirty now", true, testDec.ApportionmentDirty);
			});
		}

		public void TestApportionmentDirtyWhenIsDutiableChanges()
		{
			CombineAssertions(() =>
			{
				testDec.ApportionmentDirty = false;
				AssertEquals("JobDeclaration Apportionment is not dirty", false, testDec.ApportionmentDirty);

				var testCharge = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
				testCharge.J7_IsDutiable = !testCharge.J7_IsDutiable;
				AssertEquals("JobDeclaration Apportionment is dirty now", true, testDec.ApportionmentDirty);
			});
		}

		public void TestApportionmentDirtyWhenIsGSTApplicableChanges()
		{
			CombineAssertions(() =>
			{
				testDec.ApportionmentDirty = false;
				AssertEquals("JobDeclaration Apportionment is not dirty", false, testDec.ApportionmentDirty);

				var testCharge = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
				testCharge.J7_IsGSTApplicable = !testCharge.J7_IsGSTApplicable;
				AssertEquals("JobDeclaration Apportionment is dirty now", true, testDec.ApportionmentDirty);
			});
		}

		public void TestChangingJ7_ChargeTypeWhenCurrencyPrevChargeTypeAreValid()
		{
			var charge = topGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, topGroupHeader.JobDeclaration.LocalCurrencyCode);

			var invoice = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice, 10000m);
			testDec.ResumeApportionment();
			CombineAssertions(() =>
			{
				AssertEquals("PreCondition:One apportioned charge", 1, invoice.GroupCharges.Count);
				AssertEquals("PreCondition:OTH is apportioned", CustomsChargeTypeList.Codes.OtherCharges, invoice.GroupCharges[0].J7_ChargeType);

				charge.J7_ChargeType = CustomsChargeTypeList.Codes.ExWorks;
				testDec.ResumeApportionment();
				AssertEquals("OTH is cleared and EXW is apportioned", 1, invoice.GroupCharges.Count);
				AssertEquals("EXW is the one that was apportioned now", CustomsChargeTypeList.Codes.ExWorks, invoice.GroupCharges[0].J7_ChargeType);
			});
		}

		public void TestChangingJ7_ChargeTypeForMultiLevelGroups()
		{
			var subGroupHeader = topGroupHeader.JobComInvoiceGroupHeaders.AddNew();
			topGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			var subOTH = subGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 70m, topGroupHeader.JobDeclaration.LocalCurrencyCode);

			var invoiceFromTop = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoiceFromTop, 10000m);
			var invoiceFromSub = subGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoiceFromSub, 10000m);
			testDec.ResumeApportionment();
			CombineAssertions(() =>
			{
				AssertEquals("PreCondition:InvoiceFromTop apportioned 30 dollars", 30m, invoiceFromTop.GroupCharges[0].J7_Amount);
				AssertEquals("PreCondition:InvoiceFromSub apportioned 70 dollars", 70m, invoiceFromSub.GroupCharges[0].J7_Amount);

				subOTH.J7_ChargeType = CustomsChargeTypeList.Codes.ExWorks;
				testDec.ResumeApportionment();
				AssertEquals("InvoiceFromSub now has two apportioned charges", 2, invoiceFromSub.GroupCharges.Count);
				AssertEquals("InvoiceFromTop has one apportioned charge", 1, invoiceFromTop.GroupCharges.Count);
				AssertEquals("InvoiceFromTop apportioned 50 dollars", 50m, invoiceFromTop.GroupCharges[0].J7_Amount);
			});
		}

		public void TestChangingDutiableWhenCurrencyChargeTypeAreValid()
		{
			var charge = topGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, topGroupHeader.JobDeclaration.LocalCurrencyCode);

			var invoice = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice, 10000m);
			testDec.ResumeApportionment();
			CombineAssertions(() =>
			{
				AssertEquals("PreCondition:One apportioned charge", 1, invoice.GroupCharges.Count);
				AssertEquals("PreCondition:OTH is apportioned", CustomsChargeTypeList.Codes.OtherCharges, invoice.GroupCharges[0].J7_ChargeType);

				charge.J7_IsDutiable = !charge.J7_IsDutiable;
				testDec.ResumeApportionment();
				AssertEquals("Still one apportioned charge", 1, invoice.GroupCharges.Count);
				AssertEquals("Dutiable property should be the same as group charge", charge.J7_IsDutiable, invoice.GroupCharges[0].J7_IsDutiable);
			});
		}

		public void TestChangingGSTApplicableWhenCurrencyChargeTypeAreValid()
		{
			var charge = topGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, topGroupHeader.JobDeclaration.LocalCurrencyCode);

			var invoice = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice, 10000m);
			testDec.ResumeApportionment();
			CombineAssertions(() =>
			{
				AssertEquals("PreCondition:One apportioned charge", 1, invoice.GroupCharges.Count);
				AssertEquals("PreCondition:OTH is apportioned", CustomsChargeTypeList.Codes.OtherCharges, invoice.GroupCharges[0].J7_ChargeType);

				charge.J7_IsGSTApplicable = !charge.J7_IsGSTApplicable;
				testDec.ResumeApportionment();
				AssertEquals("Still one apportioned charge", 1, invoice.GroupCharges.Count);
				AssertEquals("J7_IsGSTApplicable property should be the same as group charge", charge.J7_IsGSTApplicable, invoice.GroupCharges[0].J7_IsGSTApplicable);
			});
		}

		public void TestChangingDutiableForMultiLevelGroups()
		{
			var subGroupHeader = topGroupHeader.JobComInvoiceGroupHeaders.AddNew();
			topGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			var subOTH = subGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 70m, topGroupHeader.JobDeclaration.LocalCurrencyCode);

			var invoiceFromTop = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoiceFromTop, 10000m);
			var invoiceFromSub = subGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoiceFromSub, 10000m);
			testDec.ResumeApportionment();
			CombineAssertions(() =>
			{
				AssertEquals("PreCondition:InvoiceFromTop apportioned 30 dollars", 30m, invoiceFromTop.GroupCharges[0].J7_Amount);
				AssertEquals("PreCondition:InvoiceFromSub apportioned 70 dollars", 70m, invoiceFromSub.GroupCharges[0].J7_Amount);

				subOTH.J7_IsDutiable = !subOTH.J7_IsDutiable;
				testDec.ResumeApportionment();
				AssertEquals("InvoiceFromSub now has two apportioned charges", 2, invoiceFromSub.GroupCharges.Count);
				AssertEquals("InvoiceFromTop now has 50 dollars of apportioned charge", 50m, invoiceFromTop.GroupCharges[0].J7_Amount);
			});
		}

		public void TestChangingGSTApplicableForMultiLevelGroups()
		{
			var subGroupHeader = topGroupHeader.JobComInvoiceGroupHeaders.AddNew();
			topGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			var subOTH = subGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 70m, topGroupHeader.JobDeclaration.LocalCurrencyCode);

			var invoiceFromTop = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoiceFromTop, 10000m);
			var invoiceFromSub = subGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoiceFromSub, 10000m);
			testDec.ResumeApportionment();
			CombineAssertions(() =>
			{
				AssertEquals("PreCondition:InvoiceFromTop apportioned 30 dollars", 30m, invoiceFromTop.GroupCharges[0].J7_Amount);
				AssertEquals("PreCondition:InvoiceFromSub apportioned 70 dollars", 70m, invoiceFromSub.GroupCharges[0].J7_Amount);

				subOTH.J7_IsGSTApplicable = !subOTH.J7_IsGSTApplicable;
				testDec.ResumeApportionment();
				AssertEquals("InvoiceFromSub now has two apportioned charges", 2, invoiceFromSub.GroupCharges.Count);
				AssertEquals("InvoiceFromTop now has 50 dollars of apportioned charge", 50m, invoiceFromTop.GroupCharges[0].J7_Amount);
			});
		}

		public void TestGroupChargeWithZeroAmountAndCurrencyIsApportioned()
		{
			topGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 0, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			var invoice = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice, 10000m);
			testDec.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertEquals("Zero Amount is apportioned as it has a valid currency", 1, invoice.GroupCharges.Count);
				AssertEquals("The amount is zero", 0m, invoice.GroupCharges[0].J7_Amount);
				AssertEquals("Currency is assigned", false, invoice.GroupCharges[0].J7_RX_NKCurrency.IsEmpty);
			});
		}

		public void TestChangedAmountReapportioned()
		{
			var oTH = topGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			var invoice = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice, 10000m);
			testDec.ResumeApportionment();
			CombineAssertions(() =>
			{
				AssertEquals("PreCondition:one apportioned charge", 1, invoice.GroupCharges.Count);
				AssertEquals("PreCondition:100 apportioned", 100m, invoice.GroupCharges[0].J7_Amount);

				oTH.J7_Amount = 200m;
				testDec.ResumeApportionment();
				AssertEquals("one apportioned charge", 1, invoice.GroupCharges.Count);
				AssertEquals("200 apportioned", 200m, invoice.GroupCharges[0].J7_Amount);
			});
		}

		public void TestChangedCurrencyReapportioned()
		{
			var oTH = topGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			var invoice = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice, 10000m);
			testDec.ResumeApportionment();
			CombineAssertions(() =>
			{
				AssertEquals("PreCondition:one apportioned charge", 1, invoice.GroupCharges.Count);
				AssertEquals("PreCondition:100 apportioned", 100m, invoice.GroupCharges[0].J7_Amount);

				oTH.J7_RX_NKCurrency = "USD";
				testDec.ResumeApportionment();
				AssertEquals("one apportioned charge", 1, invoice.GroupCharges.Count);
				AssertEquals("200 apportioned", oTH.J7_RX_NKCurrency, invoice.GroupCharges[0].J7_RX_NKCurrency);
			});
		}

		public void TestClearingCurrencyClearsApportionedCharges()
		{
			var oTH = topGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			var invoice = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice, 10000m);
			testDec.ResumeApportionment();
			CombineAssertions(() =>
			{
				AssertEquals("PreCondition:one apportioned charge", 1, invoice.GroupCharges.Count);
				AssertEquals("PreCondition:100 apportioned", 100m, invoice.GroupCharges[0].J7_Amount);

				oTH.J7_RX_NKCurrency = ZString.Empty;
				testDec.ResumeApportionment();
				AssertEquals("Apportioned charge is cleared", 0, invoice.GroupCharges.Count);
			});
		}

		#region Implementation
		void SetUpInvoice(BaseJobComInvoiceHeader invoice, ZDecimal amount)
		{
			invoice.JZ_InvoiceAmount = amount;
			invoice.JZ_RX_NKInvoice_Currency = invoice.JobDeclaration.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";
		}

		BaseJobDeclaration testDec;
		BaseJobComInvoiceGroupHeader topGroupHeader;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<BaseJobDeclaration>();
			testDec.AutoCreateChargesBasedOnIncoTerm = false;
			topGroupHeader = testDec.JobComInvoiceGroupHeaders[0];
			distributeByForExport = DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value);
		}

		protected override void TearDown()
		{
			base.TearDown();
			distributeByForExport?.Dispose();
		}

		IDisposable distributeByForExport;

		#endregion
	}
}
