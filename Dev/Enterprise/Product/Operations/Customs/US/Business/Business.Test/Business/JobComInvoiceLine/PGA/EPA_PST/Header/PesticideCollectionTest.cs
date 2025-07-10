using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PesticideCollection))]
	public class PesticideCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var collection = invoiceLine.PSTLines;
			AssertEquals(true, collection.AllowNew);
			collection.AllowAddNewPGALines = false;
			AssertEquals(false, collection.AllowNew);
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SetTrackingID();
			collection = invoiceLine.PSTLines;
			AssertEquals(false, collection.AllowNew);
			collection.AllowAddNewPGALines = true;
			AssertEquals(true, collection.AllowNew);
		}

		public void TestSetDefaultsForNewChild()
		{
			var newLine = InvoiceLine.PSTLines.AddNew();
			AssertEquals("Default the Certifying Ind to the Importer (IM)", newLine.US_CertifyingIndividual, PartyTypeList.Codes.Importer);
			AssertEquals("Default to CB", newLine.US_NotifyParty, PartyTypeList.Codes.CustomsBroker);
		}

		public void TestPSTNetWeightAndWeightUQIsDefaultedWhenInvoiceLineWeightUQIsKG()
		{
			var declaration = Factory.New<JobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsQuantity = 1000m;

			Factory.Save();

			var pstLine1 = invoiceLine.PSTLines.AddNew();
			AssertEquals("The net weight value of the first PST line should be defaulted to the value of the invoice line's Customs Qty", 1000m, pstLine1.US_NetWeight);
			AssertEquals("The net weight UQ of the first PST line should be defaulted to the value of the invoice line's Customs Qty", Core.Constants.Weight.Kilograms, pstLine1.US_WeightUQ);

			pstLine1.US_NetWeight = 700m;
			var pstLine2 = invoiceLine.PSTLines.AddNew();
			AssertEquals("The net weight value of the first PST line should be defaulted to the value of the invoice line's Customs Qty", 300m, pstLine2.US_NetWeight);
			AssertEquals("The net weight UQ of the first PST line should be defaulted to the value of the invoice line's Customs Qty", Core.Constants.Weight.Kilograms, pstLine2.US_WeightUQ);
		}

		public void TestPSTNetWeightAndWeightUQIsNotDefaultedWhenInvoiceLineWeightUQIsInVolume()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var imperialInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			imperialInvoiceLine.JI_CustomsUnitQty = "ML";
			imperialInvoiceLine.JI_CustomsQuantity = 1000m;

			Factory.Save();

			var imperialPstLine = imperialInvoiceLine.PSTLines.AddNew();
			AssertEquals("The net weight value should only be defaulted when using a UQ of KG", imperialPstLine.US_NetWeight.Default, imperialPstLine.US_NetWeight);
			AssertNotEquals("The net weight UQ of the first PST line should only be defaulted to the value of the invoice line's Customs Qty if it is KG", Core.Constants.Weight.Pounds, imperialPstLine.US_WeightUQ);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new PesticideCollection(InvoiceLine);
		}

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableCRL = true;
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				}
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;
	}
}
