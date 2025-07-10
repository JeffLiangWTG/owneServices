using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseInvoiceChargePercentageTest : TestCaseWithFactory
	{
		public void TestCalculateBackAmountBasedOnPercentage()
		{
			BaseInvoiceCharge invDIS = invoice.Charges.AddNew();
			invDIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			invDIS.J7_Percentage = 10m;
			testDec.ResumeApportionment();
			AssertEquals("Line 1 DIS calculated amount", 500m, line1.ApportionedCharges.GetCharge(invDIS.ChargeKey).Amount);
			AssertEquals("Line 2 DIS calculated amount", 400m, line2.ApportionedCharges.GetCharge(invDIS.ChargeKey).Amount);
			AssertEquals("Discount amount calculated based on line total", 900m, invDIS.J7_Amount);
		}

		public void TestAddInvoiceAfterPercentageAddedDefaultsThePercentageInNewInvoice()
		{
			BaseGroupInvoiceCharge groupDIS = topGroup.Charges.AddNew();
			groupDIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			groupDIS.J7_Percentage = 10m;
			testDec.ResumeApportionment();
			AssertEquals("Invoice DIS defaulted", 900m, invoice.GroupCharges.GetCharge(groupDIS.ChargeKey).Amount);

			BaseJobComInvoiceHeader newInvoice = topGroup.JobComInvoiceHeaders.AddNew();
			newInvoice.JZ_InvoiceAmount = 10000m;
			newInvoice.JZ_RX_NKInvoice_Currency = invoice.JobDeclaration.LocalCurrencyCode;
			BaseJobComInvoiceLine newInvoiceLine = newInvoice.JobComInvoiceLines.AddNew();
			newInvoiceLine.JI_LinePrice = 10000m;
			testDec.ResumeApportionment();
			AssertEquals("New invoice should have the discount populated", 1000m, newInvoice.GroupCharges.GetCharge(groupDIS.ChargeKey).Amount);
		}

		public void TestChargeOverridenInInvoiceAsPercentageWhenGroupHasPercentage()
		{
			BaseJobComInvoiceHeader invoice2 = topGroup.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_InvoiceAmount = 20000m;
			invoice2.JZ_IncoTerm = "FOB";
			invoice2.JZ_RX_NKInvoice_Currency = topGroup.JobDeclaration.LocalCurrencyCode;

			BaseJobComInvoiceLine lineOfInv2 = invoice2.JobComInvoiceLines.AddNew();
			lineOfInv2.JI_LinePrice = 20000m;

			BaseGroupInvoiceCharge groupDIS = topGroup.Charges.AddNew();
			groupDIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			groupDIS.J7_Percentage = 10m;
			testDec.ResumeApportionment();
			AssertEquals("Invoice1 amount calculated from LinePrice * 10 %", 900m, invoice.GroupCharges[0].J7_Amount);
			AssertEquals("Invoice2 amount calculated from LinePrice", 2000m, invoice2.GroupCharges[0].J7_Amount);

			BaseInvoiceCharge invDIS = invoice.Charges.AddNew();
			invDIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			invDIS.J7_Percentage = 15m;
			invDIS.J7_IsNotIncludedInInvoice = true;
			testDec.ResumeApportionment();
			AssertEquals("Invoice line 1($5000)'s Discount Amount including inv level discount", 1250m, line1.ApportionedCharges.GetCharge(invDIS.ChargeKey).Amount);
			AssertEquals("Invoice line 2($4000)'s Discount Amount including inv level discount", 1000m, line2.ApportionedCharges.GetCharge(invDIS.ChargeKey).Amount);
			AssertEquals("Discount calculated based on line price and percentages", 1350m, invDIS.J7_Amount);
			AssertEquals("Invoice 2 still has the 10% from Group", 2000m, invoice2.GroupCharges.GetCharge(invDIS.ChargeKey).Amount);
		}

		public void TestChargeOverridenInInvoiceAsAmountWhenGroupHasPercentage()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				BaseJobComInvoiceHeader invoice2 = topGroup.JobComInvoiceHeaders.AddNew();
				invoice2.JZ_InvoiceAmount = 20000m;
				invoice2.JZ_IncoTerm = "FOB";
				invoice2.JZ_RX_NKInvoice_Currency = topGroup.JobDeclaration.LocalCurrencyCode;

				BaseJobComInvoiceLine lineOfInv2 = invoice2.JobComInvoiceLines.AddNew();
				lineOfInv2.JI_LinePrice = 20000m;

				BaseGroupInvoiceCharge groupDIS = topGroup.Charges.AddNew();
				groupDIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
				groupDIS.J7_Percentage = 10m;
				testDec.ResumeApportionment();
				AssertEquals("Invoice1 amount calculated from LinePrice * 10 %", 900m, invoice.GroupCharges[0].J7_Amount);
				AssertEquals("Invoice2 amount calculated from LinePrice", 2000m, invoice2.GroupCharges[0].J7_Amount);

				BaseInvoiceCharge invDIS = invoice.Charges.AddNew();
				invDIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
				invDIS.J7_Amount = 90m;
				invDIS.J7_RX_NKCurrency = topGroup.JobDeclaration.LocalCurrencyCode;
				invDIS.J7_IsNotIncludedInInvoice = true;
				testDec.ResumeApportionment();
				AssertEquals("Line1 ($5000)'s Discount Amount", 550m, line1.ApportionedCharges.GetCharge(invDIS.ChargeKey).Amount);
				AssertEquals("Line2 ($4000)'s Discount Amount", 440m, line2.ApportionedCharges.GetCharge(invDIS.ChargeKey).Amount);
				AssertEquals("LIneofInv2 still has the percentage amount", 2000m, lineOfInv2.ApportionedCharges.GetCharge(invDIS.ChargeKey).Amount);
			}
		}

		public void TestAssignAmountWhenChargeOverridenInInvoiceAsPercentageWhenGroupHasPercentage()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];

			BaseJobComInvoiceHeader invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_IncoTerm = "FOB";
			invoice1.JZ_InvoiceAmount = 15000m;
			invoice1.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			BaseJobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 15000m;

			BaseJobComInvoiceHeader invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_IncoTerm = "FOB";
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			BaseJobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 10000m;

			BaseGroupInvoiceCharge groupCOM = groupHeader.Charges.AddNew();
			groupCOM.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			groupCOM.J7_Percentage = 10m;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition:Line1 defaulted with percentage", 10m, line1.ApportionedCharges[0].J7_Percentage);
			AssertEquals("PreCondition:Line2 defaulted with percentage", 10m, line2.ApportionedCharges[0].J7_Percentage);

			BaseInvoiceCharge invCOM = invoice1.Charges.AddNew();
			invCOM.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			invCOM.J7_Percentage = 5m;
			invCOM.J7_IsNotIncludedInInvoice = true;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition: Line1 defaulted with InvCOM", 5m, line1.ApportionedCharges[0].J7_Percentage);
			AssertEquals("PreCondition: Line2 defaulted with GroupCOM", 10m, line2.ApportionedCharges[0].J7_Percentage);
			AssertEquals("Amount calculated against percentage", 750m, line1.ApportionedCharges[0].J7_Amount);
			AssertEquals("Amount calculated aaginst percentage", 1000m, line2.ApportionedCharges[0].J7_Amount);
			AssertEquals("GroupCOM doesn t have an amount any more", 0m, groupCOM.J7_Amount);
			AssertEquals("Inv1 has an amount calculated based on percentages", 750m, invCOM.J7_Amount);
			AssertEquals("Inv2 has an amount calculated against in apportioned charges", 1000m, invoice2.GroupCharges.GetCharge(groupCOM.ChargeKey).Amount);
		}

		#region Implementation

		BaseJobDeclaration testDec;
		BaseJobComInvoiceGroupHeader topGroup;
		BaseJobComInvoiceHeader invoice;
		BaseJobComInvoiceLine line1;
		BaseJobComInvoiceLine line2;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<BaseJobDeclaration>();
			topGroup = testDec.JobComInvoiceGroupHeaders[0];
			invoice = topGroup.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 10000;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, testDec.LocalCurrencyCode);

			line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 5000m;
			line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 4000m;
		}

		#endregion
	}
}
