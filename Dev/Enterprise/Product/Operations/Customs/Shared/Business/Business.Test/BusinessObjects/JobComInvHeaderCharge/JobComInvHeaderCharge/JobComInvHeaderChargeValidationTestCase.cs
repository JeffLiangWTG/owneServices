using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobComInvHeaderChargeValidationTestCase : TestCaseWithFactory
	{
		public void TestValidateIfAllInvoiceLinesHaveDistributeByField()
		{
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Weight = 10m;
			invoiceLine1.JI_WeightUQ = "KG";
			invoiceLine1.JI_Volume = 20m;
			invoiceLine1.JI_VolumeUQ = "m3";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Volume = 20m;
			invoiceLine2.JI_VolumeUQ = "m3";

			var charge = invoice.Charges.AddNew();
			charge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			AssertEquals("Distribute by value", false, charge.J7_DistributeByInfo.HasNotifications());

			charge.J7_DistributeBy = ChargeDistributeByList.Codes.Volume;
			AssertNoMessageErrors("There are invoice lines that don't have a volume. The apportionment of this charge won't be correct for the invoice lines.", charge.J7_DistributeByInfo);

			charge.J7_DistributeBy = ChargeDistributeByList.Codes.Weight;
			AssertHasMessageErrors("There are invoice lines that don't have a weight. The apportionment of this charge won't be correct for the invoice lines.", charge.J7_DistributeByInfo);

			charge.J7_DistributeBy = ChargeDistributeByList.Codes.Quantity;
			AssertHasMessageErrors("There are invoice lines that don't have a invoice quantity. The apportionment of this charge won't be correct for the invoice lines.", charge.J7_DistributeByInfo);

			invoiceLine1.JI_InvoiceQuantity = 1.2m;
			invoiceLine1.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;
			invoiceLine2.JI_InvoiceQuantity = 10m;
			invoiceLine2.JI_InvoiceUQ = Core.Constants.Weight.Tonnes;
			charge.Validation.ValidateJ7_DistributeBy();
			AssertNoMessageErrors("There are invoice lines that don't have a invoice quantity. The apportionment of this charge won't be correct for the invoice lines.", charge.J7_DistributeByInfo);
			AssertHasWarning(charge.J7_DistributeByInfo, JobComInvHeaderChargeValidation.MultipleInvoiceUQs);

			invoiceLine2.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;
			charge.Validation.ValidateJ7_DistributeBy();
			AssertNoWarning(charge.J7_DistributeByInfo, JobComInvHeaderChargeValidation.MultipleInvoiceUQs);
		}

		public void TestListValidationForDistributeBy()
		{
			BaseInvoiceCharge invoiceOFT = invoice.Charges.AddNew();
			invoiceOFT.J7_DistributeBy = "XXX";
			AssertHasMessageError(invoiceOFT.J7_DistributeByInfo, ListValidation.InvalidCodeMessageError);

			invoiceOFT.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			AssertNoMessageError(invoiceOFT.J7_DistributeByInfo, ListValidation.InvalidCodeMessageError);
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
