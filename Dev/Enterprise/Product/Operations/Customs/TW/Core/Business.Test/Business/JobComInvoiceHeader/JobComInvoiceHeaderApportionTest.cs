using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class JobComInvoiceHeaderApportionTest : Customs.Business.Testing.BaseJobComInvoiceHeaderApportionTest
	{
		#region Implementation
		protected override BaseJobDeclaration GetNewDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}

		public override void TestFOBWithPreFOBCharge()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				header1.JZ_InvoiceAmount = 1000m;
				header1.JZ_RX_NKInvoice_Currency = header1.JobDeclaration.LocalCurrencyCode;
				header1.JZ_IncoTerm = "CIF";
				AssertEquals("FOB Value for $1000 FOB Invoice", 1000M, header1.JZ_Calc_FOBAmount);
				var charge = allInvoicesGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, header1.JobDeclaration.LocalCurrencyCode);
				charge.J7_IsIncludedInITOT = true;
				PrepareCharge(charge);
				testDeclaration.ResumeApportionment();
				AssertEquals("FOB value for $1000 FOB invoice", 900m, header1.JZ_Calc_FOBAmount);
				AssertEquals("Line Total For this invoice", 1000m, header1.InvoiceLineTotal);
			}
		}

		public new void TestApportionChargeWhenChargeIsRelevantForIncoTerm()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				BaseJobComInvHeaderCharge groupLanding = allInvoicesGroup.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges);
				PrepareCharge(groupLanding);
				groupLanding.J7_Amount = 100m;
				groupLanding.J7_RX_NKCurrency = allInvoicesGroup.JobDeclaration.LocalCurrencyCode;

				var header2 = allInvoicesGroup.JobComInvoiceHeaders.AddNew();

				header1.JZ_IncoTerm = "FOB";
				header1.JZ_InvoiceAmount = 1000m;
				header1.JZ_RX_NKInvoice_Currency = allInvoicesGroup.JobDeclaration.LocalCurrencyCode;

				header2.JZ_JE = testDeclaration.PK;
				header2.JZ_IncoTerm = "DDP";
				header2.JZ_InvoiceAmount = 2000m;
				header2.JZ_RX_NKInvoice_Currency = allInvoicesGroup.JobDeclaration.LocalCurrencyCode;
				testDeclaration.ResumeApportionment();
				AssertEquals("Landing Charge is not relavant for FOB invoice", 0, header1.GroupCharges.Count);
				AssertEquals("Landing Charge is relavant for DDP invoice", 1, header2.GroupCharges.Count);
			}
		}

		public new void TestChangeIncoTermUpdateApportion()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				BaseJobComInvHeaderCharge groupLanding = allInvoicesGroup.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges);
				PrepareCharge(groupLanding);
				groupLanding.J7_Amount = 100m;
				groupLanding.J7_RX_NKCurrency = allInvoicesGroup.JobDeclaration.LocalCurrencyCode;

				var header2 = allInvoicesGroup.JobComInvoiceHeaders.AddNew();
				header2.JZ_JE = testDeclaration.PK;

				header1.JZ_IncoTerm = "FOB";
				header1.JZ_InvoiceAmount = 2000m;
				header1.JZ_RX_NKInvoice_Currency = allInvoicesGroup.JobDeclaration.LocalCurrencyCode;

				header2.JZ_IncoTerm = "DDP";
				header2.JZ_InvoiceAmount = 2000m;
				header2.JZ_RX_NKInvoice_Currency = allInvoicesGroup.JobDeclaration.LocalCurrencyCode;
				testDeclaration.ResumeApportionment();
				AssertEquals("Landing Charge is not relavant for FOB invoice", 0, header1.GroupCharges.Count);
				AssertEquals("Landing Charge is relavant for DDP invoice", 1, header2.GroupCharges.Count);
			}
		}

		public void TestDefaultEntryInstruction()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				var jobDeclaration1 = Factory.NewWithValidTestData<JobDeclaration>();
				var jobDeclaration2 = Factory.NewWithValidTestData<JobDeclaration>();

				var invoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

				invoiceHeader.JZ_JE = jobDeclaration1.PK;
				AssertEquals(jobDeclaration1.CusEntryInstruction.PK, invoiceLine.JI_CEI);
				invoiceHeader.JZ_JE = jobDeclaration2.PK;
				AssertEquals(jobDeclaration2.CusEntryInstruction.PK, invoiceLine.JI_CEI);
			}
		}

		public new void TestApportionLandingCharges()
		{
			AssertApportion(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.LandingCharges, false, false));
		}

		void AssertApportion(ChargeCodeChargeKey chargeKey)
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				header1.JZ_RX_NKInvoice_Currency = header1.JobDeclaration.LocalCurrencyCode;
				header1.JZ_InvoiceAmount = 1000m;
				header1.JZ_IncoTerm = "DDP";

				PrepareCharge(allInvoicesGroup.Charges.AddNew(chargeKey.ChargeCode, 150m, header1.JobDeclaration.LocalCurrencyCode));
				testDeclaration.ResumeApportionment();
				AssertEquals("Landing Charge is relavant for DDP invoice", 1, header1.GroupCharges.Count);
			}
		}

		public void TestSetDefaultValuesFromDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MarksAndNumbers = ZString.Empty;
			AssertEquals(ZString.Empty, declaration.Invoices.AddNew().TW_MarksAndNumbers);
			declaration.JE_MarksAndNumbers = new ZString('B', 33);
			AssertEquals(new ZString('B', 33), declaration.Invoices.AddNew().TW_MarksAndNumbers);
			declaration.JE_MarksAndNumbers = new ZString('A', 600);
			AssertEquals(new ZString('A', 600), declaration.Invoices.AddNew().TW_MarksAndNumbers);

			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.TW_MarksAndNumbers = ZString.Empty;
			invoiceHeader.JZ_JE = declaration.PK;
			AssertEquals(new ZString('A', 600), invoiceHeader.TW_MarksAndNumbers);

			invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.TW_MarksAndNumbers = "XXXX";
			invoiceHeader.JZ_JE = declaration.PK;
			AssertEquals("XXXX", invoiceHeader.TW_MarksAndNumbers);

			invoiceHeader.JZ_JE = ZGuid.Empty;
			AssertEquals("XXXX", invoiceHeader.TW_MarksAndNumbers);
		}
		#endregion

		BaseJobDeclaration testDeclaration;
		BaseJobComInvoiceGroupHeader allInvoicesGroup;
		BaseJobComInvoiceHeader header1;

		protected override void SetUp()
		{
			base.SetUp();
			testDeclaration = GetNewDeclaration();
			testDeclaration.AutoCreateChargesBasedOnIncoTerm = false;

			allInvoicesGroup = testDeclaration.JobComInvoiceGroupHeaders[0];
			header1 = allInvoicesGroup.JobComInvoiceHeaders.AddNew();
			header1.JZ_JE = testDeclaration.PK;
		}
	}
}
