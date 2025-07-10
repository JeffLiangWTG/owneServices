using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobComInvChargeCloneHelper : TestCaseWithFactory
	{
		public void TestClone()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 200m, declaration.LocalCurrencyCode);

				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 10000m;
				invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

				var invoiceCharge = invoice.Charges.AddNew();
				invoiceCharge.J7_ChargeType = "~~~";
				invoiceCharge.J7_Amount = 100m;
				invoiceCharge.J7_IsDutiable = false;
				invoiceCharge.J7_IsGSTApplicable = true;
				invoiceCharge.J7_IsIncludedInITOT = true;

				var clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
				AssertEquals(1, clonedDeclaration.Invoices.Count);

				var clonedInvoice = clonedDeclaration.Invoices[0];
				AssertEquals(1, clonedInvoice.Charges.Count);
				AssertEquals(0, clonedInvoice.GroupCharges.Count);

				var clonedCharge = clonedInvoice.Charges[0];
				AssertEquals("Cloned Charge row has a J7_ParentID set to ClonedInvoice rather than the original invoice", clonedInvoice.PK, clonedCharge.J7_ParentID);
				AssertEquals("Cloned Charge Type", "~~~", clonedCharge.J7_ChargeType);
				AssertEquals("Cloned Amount", 100m, clonedCharge.J7_Amount);
				AssertEquals("Cloned Dutiable", false, clonedCharge.J7_IsDutiable);
				AssertEquals("Cloned J7_IsGSTApplicable", true, clonedCharge.J7_IsGSTApplicable);
				AssertEquals("Cloned J7_IsIncludedInITOT", true, clonedCharge.J7_IsIncludedInITOT);
				AssertEquals("Cloned charge does not have Change", false, clonedCharge.HasChanges);

				AssertEquals("No apportioned charges are cloned", 0, clonedInvoice.GroupCharges.Count);

				clonedDeclaration.ResumeApportionment();
				AssertEquals("Apportioned now", 1, clonedInvoice.GroupCharges.Count);

				var clonedApportionedCharge = clonedInvoice.GroupCharges[0];
				AssertEquals("Cloned Charge row has a J7_ParentID set to ClonedInvoice rather than the original invoice", clonedInvoice.PK, clonedApportionedCharge.J7_ParentID);
				AssertEquals("Cloned Charge Type", CustomsChargeTypeList.Codes.OverseasFreight, clonedApportionedCharge.J7_ChargeType);
				AssertEquals("Cloned Amount", 200m, clonedApportionedCharge.J7_Amount);
				AssertEquals("Cloned Dutiable", false, clonedApportionedCharge.J7_IsDutiable);
				AssertEquals("Cloned J7_IsGSTApplicable", true, clonedApportionedCharge.J7_IsGSTApplicable);
				AssertEquals("Cloned J7_IsApportionedCharge", true, clonedApportionedCharge.J7_IsApportionedCharge);
			}
		}
	}
}
