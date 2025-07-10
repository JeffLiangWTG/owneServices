using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseInvoiceLineChargeApportionForGroupChargeTest : TestCaseWithFactory
	{
		public void TestTypeDeciderForInvoiceLineCharge()
		{
			AssertEquals("Type decider type", typeof(InvoiceLineChargeTypeDecider), BaseInvoiceLineCharge.TypeDecider.GetType());
		}

		public void TestSetDefaultValue()
		{
			BaseInvoiceLineCharge charge = Factory.New<BaseInvoiceLineCharge>();
			AssertEquals("IsApportioned", false, charge.J7_IsApportionedCharge);

			BaseJobComInvoiceLine line = invoice1.JobComInvoiceLines.AddNew();
			charge.Parent = line;
			AssertEquals("ParentTableCode", "JI", charge.J7_ParentTableCode);
		}

		public void TestApportionmentDirtyWhenAmountChanges()
		{
			BaseJobComInvoiceLine line = invoice1.JobComInvoiceLines.AddNew();
			BaseInvoiceLineCharge lineCharge = line.Charges.AddNew();

			testDec.ApportionmentDirty = false;
			AssertEquals("JobDeclaration Apportionment is not dirty", false, testDec.ApportionmentDirty);

			lineCharge.J7_Amount = 100m;
			AssertEquals("JobDeclaration Apportionment is dirty now", true, testDec.ApportionmentDirty);
		}

		public void TestApportionmentDirtyWhenTypeChanges()
		{
			BaseJobComInvoiceLine line = invoice1.JobComInvoiceLines.AddNew();
			BaseInvoiceLineCharge lineCharge = line.Charges.AddNew();

			testDec.ApportionmentDirty = false;
			AssertEquals("JobDeclaration Apportionment is not dirty", false, testDec.ApportionmentDirty);

			lineCharge.J7_ChargeType = "OTH";
			AssertEquals("JobDeclaration Apportionment is dirty now", true, testDec.ApportionmentDirty);
		}

		public void TestApportionmentDirtyWhenCurrencyChanges()
		{
			BaseJobComInvoiceLine line = invoice1.JobComInvoiceLines.AddNew();
			BaseInvoiceLineCharge lineCharge = line.Charges.AddNew();

			testDec.ApportionmentDirty = false;
			AssertEquals("JobDeclaration Apportionment is not dirty", false, testDec.ApportionmentDirty);

			lineCharge.J7_RX_NKCurrency = "AUD";
			AssertEquals("JobDeclaration Apportionment is dirty now", true, testDec.ApportionmentDirty);
		}

		public void TestApportionmentDirtyWhenIsDutiableChanges()
		{
			BaseJobComInvoiceLine line = invoice1.JobComInvoiceLines.AddNew();
			BaseInvoiceLineCharge lineCharge = line.Charges.AddNew();

			testDec.ApportionmentDirty = false;
			AssertEquals("JobDeclaration Apportionment is not dirty", false, testDec.ApportionmentDirty);

			lineCharge.J7_IsDutiable = !lineCharge.J7_IsDutiable;
			AssertEquals("JobDeclaration Apportionment is dirty now", true, testDec.ApportionmentDirty);
		}

		public void TestApportionmentDirtyWhenIsGSTApplicableChanges()
		{
			BaseJobComInvoiceLine line = invoice1.JobComInvoiceLines.AddNew();
			BaseInvoiceLineCharge lineCharge = line.Charges.AddNew();

			testDec.ApportionmentDirty = false;
			AssertEquals("JobDeclaration Apportionment is not dirty", false, testDec.ApportionmentDirty);

			lineCharge.J7_IsGSTApplicable = !lineCharge.J7_IsGSTApplicable;
			AssertEquals("JobDeclaration Apportionment is dirty now", true, testDec.ApportionmentDirty);
		}

		public void TestChangingAmountLeadsToReapportion()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				BaseJobComInvHeaderCharge groupCharge = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, groupHeader.JobDeclaration.LocalCurrencyCode);

				BaseJobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
				line1.JI_LinePrice = 10000m;
				BaseJobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
				line2.JI_LinePrice = 10000m;
				testDec.ResumeApportionment();
				AssertEquals("PreCondition:Invoice1 has an apportioned charge", 1, invoice1.GroupCharges.Count);
				AssertEquals("PreCondition:Invoice 1 apportioned amount", 500m, invoice1.GroupCharges[0].J7_Amount);
				AssertEquals("PreCondition:Invoice2 has an apportioned charge", 1, invoice2.GroupCharges.Count);
				AssertEquals("PreCondition:Invoice 2 apportioned amount", 500m, invoice2.GroupCharges[0].J7_Amount);
				AssertEquals("PreCondition: Line 1 has an apportioned charge", 1, line1.ApportionedCharges.Count);
				AssertEquals("PreCondition:Line 1 apportioned amount", 500m, line1.ApportionedCharges[0].J7_Amount);
				AssertEquals("PreCondition: Line 2 has an apportioned charge", 1, line2.ApportionedCharges.Count);
				AssertEquals("PreCondition:Line 2 apportioned amount", 500m, line2.ApportionedCharges[0].J7_Amount);

				BaseJobComInvHeaderCharge lineCharge = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 600m, groupHeader.JobDeclaration.LocalCurrencyCode);
				lineCharge.J7_IsNotIncludedInInvoice = true;
				testDec.ResumeApportionment();
				AssertEquals("Line 1 apportioned charge cleared", 0, line1.ApportionedCharges.Count);
				AssertEquals("Line 2 apportioned charge now 400m", 400m, line2.ApportionedCharges[0].J7_Amount);
				AssertEquals("Invoice 1 apportioned from line1", 600m, invoice1.GroupCharges[0].J7_Amount);
				AssertEquals("Invoice 2 apportioned", 400m, invoice2.GroupCharges[0].J7_Amount);

				lineCharge.J7_Amount = 300m;
				testDec.ResumeApportionment();
				AssertEquals("Line 1 apportioned charge cleared", 0, line1.ApportionedCharges.Count);
				AssertEquals("Line 2 apportioned charge now 400m", 700m, line2.ApportionedCharges[0].J7_Amount);
				AssertEquals("Invoice 1 apportioned from line1", 300m, invoice1.GroupCharges[0].J7_Amount);
				AssertEquals("Invoice 2 apportioned", 700m, invoice2.GroupCharges[0].J7_Amount);
			}
		}

		public void TestChangingCurrencyLeadsToRapportion()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				BaseJobComInvHeaderCharge groupCharge = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, groupHeader.JobDeclaration.LocalCurrencyCode);

				BaseJobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
				line1.JI_LinePrice = 10000m;
				BaseJobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
				line2.JI_LinePrice = 10000m;

				BaseJobComInvHeaderCharge lineCharge = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 600m, groupHeader.JobDeclaration.LocalCurrencyCode);
				lineCharge.J7_IsNotIncludedInInvoice = true;
				testDec.ResumeApportionment();
				AssertEquals("Line 1 apportioned charge cleared", 0, line1.ApportionedCharges.Count);
				AssertEquals("Line 2 apportioned charge now 400m", 400m, line2.ApportionedCharges[0].J7_Amount);
				AssertEquals("Invoice 1 apportioned from line1", 600m, invoice1.GroupCharges[0].J7_Amount);
				AssertEquals("Invoice 2 apportioned", 400m, invoice2.GroupCharges[0].J7_Amount);

				lineCharge.J7_RX_NKCurrency = "USD";
				testDec.ResumeApportionment();
				Assert("Line 2 apportioned charge now is not 400m as the other is foreign currency", 400m != line2.ApportionedCharges[0].J7_Amount);
			}
		}

		public void TestZeroAmountWithCurrencyClearsApportionedCharge()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				BaseJobComInvHeaderCharge groupCharge = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, groupHeader.JobDeclaration.LocalCurrencyCode);

				BaseJobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
				line1.JI_LinePrice = 10000m;
				BaseJobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
				line2.JI_LinePrice = 10000m;

				line1.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 0m, groupHeader.JobDeclaration.LocalCurrencyCode);
				line1.Charges[0].J7_IsNotIncludedInInvoice = true;
				testDec.ResumeApportionment();
				AssertEquals("Line 2 apportioned all", 1000m, line2.ApportionedCharges[0].J7_Amount);
			}
		}

		public void TestClearingCurrencyReapportion()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				BaseJobComInvHeaderCharge groupCharge = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, groupHeader.JobDeclaration.LocalCurrencyCode);

				BaseJobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
				line1.JI_LinePrice = 10000m;
				BaseJobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
				line2.JI_LinePrice = 10000m;

				BaseJobComInvHeaderCharge lineCharge = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 600m, groupHeader.JobDeclaration.LocalCurrencyCode);
				lineCharge.J7_IsNotIncludedInInvoice = true;
				testDec.ResumeApportionment();
				AssertEquals("Line 1 apportioned charge cleared", 0, line1.ApportionedCharges.Count);
				AssertEquals("Line 2 apportioned charge now 400m", 400m, line2.ApportionedCharges[0].J7_Amount);

				lineCharge.J7_RX_NKCurrency = ZString.Empty;
				testDec.ResumeApportionment();
				AssertEquals("Line 1 apportioned", 500m, line1.ApportionedCharges[0].J7_Amount);
				AssertEquals("Line 2 apportioned", 500m, line2.ApportionedCharges[0].J7_Amount);
			}
		}

		public void TestChangingChargeTypeReapportion()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				BaseJobComInvHeaderCharge groupCharge = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, groupHeader.JobDeclaration.LocalCurrencyCode);

				BaseJobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
				line1.JI_LinePrice = 10000m;
				BaseJobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
				line2.JI_LinePrice = 10000m;

				BaseJobComInvHeaderCharge lineCharge = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 600m, groupHeader.JobDeclaration.LocalCurrencyCode);
				lineCharge.J7_IsNotIncludedInInvoice = true;
				testDec.ResumeApportionment();
				AssertEquals("Line 1 apportioned charge cleared", 0, line1.ApportionedCharges.Count);
				AssertEquals("Line 2 apportioned charge now 400m", 400m, line2.ApportionedCharges[0].J7_Amount);

				lineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.ExWorks;
				testDec.ResumeApportionment();
				AssertEquals("Line1 apportioned again", 1, line1.ApportionedCharges.Count);
				AssertEquals("Line1 apportioned OTH", CustomsChargeTypeList.Codes.OtherCharges, line1.ApportionedCharges[0].J7_ChargeType);
				AssertEquals("Line 1 apportioned amount", 500m, line1.ApportionedCharges[0].J7_Amount);
				AssertEquals("Line 2 apportioned amount", 500m, line2.ApportionedCharges[0].J7_Amount);
			}
		}

		public void TestChangingIsDutiableLeadsToReapportion()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				BaseJobComInvHeaderCharge groupCharge = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, groupHeader.JobDeclaration.LocalCurrencyCode);

				BaseJobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
				line1.JI_LinePrice = 10000m;
				BaseJobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
				line2.JI_LinePrice = 10000m;

				BaseJobComInvHeaderCharge lineCharge = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 600m, groupHeader.JobDeclaration.LocalCurrencyCode);
				lineCharge.J7_IsNotIncludedInInvoice = true;
				testDec.ResumeApportionment();
				AssertEquals("Line 1 apportioned charge cleared", 0, line1.ApportionedCharges.Count);
				AssertEquals("Line 2 apportioned charge now 400m", 400m, line2.ApportionedCharges[0].J7_Amount);

				lineCharge.J7_IsDutiable = !lineCharge.J7_IsDutiable;
				testDec.ResumeApportionment();
				AssertEquals("Line1 apportioned again", 1, line1.ApportionedCharges.Count);
				AssertEquals("Line1 apportioned OTH", CustomsChargeTypeList.Codes.OtherCharges, line1.ApportionedCharges[0].J7_ChargeType);
				AssertEquals("Line 1 apportioned amount", 500m, line1.ApportionedCharges[0].J7_Amount);
				AssertEquals("Line 2 apportioned amount", 500m, line2.ApportionedCharges[0].J7_Amount);
			}
		}

		public void TestChangingIsGSTLeadsToReapportion()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				BaseJobComInvHeaderCharge groupCharge = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, groupHeader.JobDeclaration.LocalCurrencyCode);

				BaseJobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
				line1.JI_LinePrice = 10000m;
				BaseJobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
				line2.JI_LinePrice = 10000m;

				BaseJobComInvHeaderCharge lineCharge = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 600m, groupHeader.JobDeclaration.LocalCurrencyCode);
				lineCharge.J7_IsNotIncludedInInvoice = true;
				testDec.ResumeApportionment();
				AssertEquals("Line 1 apportioned charge cleared", 0, line1.ApportionedCharges.Count);
				AssertEquals("Line 2 apportioned charge now 400m", 400m, line2.ApportionedCharges[0].J7_Amount);

				lineCharge.J7_IsGSTApplicable = !lineCharge.J7_IsGSTApplicable;
				testDec.ResumeApportionment();
				AssertEquals("Line1 apportioned again", 1, line1.ApportionedCharges.Count);
				AssertEquals("Line1 apportioned OTH", CustomsChargeTypeList.Codes.OtherCharges, line1.ApportionedCharges[0].J7_ChargeType);
				AssertEquals("Line 1 apportioned amount", 500m, line1.ApportionedCharges[0].J7_Amount);
				AssertEquals("Line 2 apportioned amount", 500m, line2.ApportionedCharges[0].J7_Amount);
			}
		}

		#region Implementation

		BaseJobDeclaration testDec;
		BaseJobComInvoiceGroupHeader groupHeader;
		BaseJobComInvoiceHeader invoice1;
		BaseJobComInvoiceHeader invoice2;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<BaseJobDeclaration>();
			groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice1);
			SetUpInvoice(invoice2);
		}

		void SetUpInvoice(BaseJobComInvoiceHeader invoice)
		{
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = invoice.JobDeclaration.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";
		}

		#endregion
	}
}
