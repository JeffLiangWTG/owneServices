using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseApportionedChargeValidationTest : TestCaseWithFactory
	{
		public void TestAddGroupChargeThatShouldBeExcluded()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				invoice.JZ_InvoiceAmount = 1000;
				invoice.JZ_RX_NKInvoice_Currency = invoice.JobDeclaration.LocalCurrencyCode;
				invoice.JZ_IncoTerm = invoice.IncotermEquivalentToCFRForTesting;

				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100, invoice.JobDeclaration.LocalCurrencyCode);
				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, invoice.JobDeclaration.LocalCurrencyCode);
				testDec.ResumeApportionment();

				AssertEquals("ONS cannot be included in C&F invoice", false, invoice.GroupCharges[0].J7_IsIncludedInITOT);
				AssertEquals("OFT can be included in C&F invoice", true, invoice.GroupCharges[1].J7_IsIncludedInITOT);
			}
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
