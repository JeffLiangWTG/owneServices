using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class InvoiceLineDecimalPlacesCalculatorTest : TestCaseWithFactory
	{
		public void TestQuantitiesForMultipleLines()
		{
			var invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 2.254m;
			invoiceLine.JI_InvoiceUQ = "PK";
			invoiceLine.JI_NetWeight = 433.8m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_Weight = 344.8m;
			invoiceLine.JI_WeightUQ = "LB";
			invoiceLine.JI_CustomsUnitQty = "T";
			invoiceLine.JI_CustomsQuantity = 345.987021m;
			invoiceLine.JI_Volume = 34.33m;
			invoiceLine.JI_VolumeUQ = "CF";

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 2.254992999m;
			invoiceLine2.JI_InvoiceUQ = "PK";
			invoiceLine2.JI_NetWeight = 433m;
			invoiceLine2.JI_NetWeightUQ = "KG";
			invoiceLine2.JI_Weight = 344.88m;
			invoiceLine2.JI_WeightUQ = "LB";
			invoiceLine2.JI_CustomsUnitQty = "T";
			invoiceLine2.JI_CustomsQuantity = 345.98m;
			invoiceLine2.JI_Volume = 34.338m;
			invoiceLine2.JI_VolumeUQ = "CF";

			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_InvoiceQuantity = 2m;
			invoiceLine3.JI_InvoiceUQ = "PK";
			invoiceLine3.JI_NetWeight = 433.9991m;
			invoiceLine3.JI_NetWeightUQ = "KG";
			invoiceLine3.JI_Weight = 344m;
			invoiceLine3.JI_WeightUQ = "LB";
			invoiceLine3.JI_CustomsUnitQty = "T";
			invoiceLine3.JI_CustomsQuantity = 345m;
			invoiceLine3.JI_Volume = 34.3m;
			invoiceLine3.JI_VolumeUQ = "CF";

			AssertEquals(6, invoiceHeader.GetInvoiceLineMaxDecimalPlaces(JobComInvoiceLineSchema.JI_InvoiceQuantity.Name));
			AssertEquals(3, invoiceHeader.GetInvoiceLineMaxDecimalPlaces(JobComInvoiceLineSchema.JI_NetWeight.Name));
			AssertEquals(2, invoiceHeader.GetInvoiceLineMaxDecimalPlaces(JobComInvoiceLineSchema.JI_Weight.Name));
			AssertEquals(6, invoiceHeader.GetInvoiceLineMaxDecimalPlaces(JobComInvoiceLineSchema.JI_CustomsQuantity.Name));
			AssertEquals(3, invoiceHeader.GetInvoiceLineMaxDecimalPlaces(JobComInvoiceLineSchema.JI_Volume.Name));

			invoiceLine2.JI_InvoiceQuantity = 2.255m;
			AssertEquals(3, invoiceHeader.GetInvoiceLineMaxDecimalPlaces(JobComInvoiceLineSchema.JI_InvoiceQuantity.Name));
		}
	}
}
