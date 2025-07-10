using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(TTBCigar))]
	public class TTBCigarTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<TTBCigar>
	{
		public void TestITTBCigarMembers()
		{
			var cigar = TTBLine.Cigars.AddNew();
			cigar.US_Quantity = 3000;
			cigar.US_UnitPrice = 53.23m;

			ITTBCigar line = cigar;
			AssertEquals("Quantity", 3000, line.Quantity);
			AssertEquals("UnitPrice", 53.23m, line.UnitPrice);
		}

		public void TestProperties()
		{
			var cigar = TTBLine.Cigars.AddNew();
			cigar.US_UnitPrice = 10m;
			cigar.US_IsSmall = ZBool.True;
			AssertEquals("cigar.IsMaximumRateInfo.ReadOnly", true, cigar.IsMaximumRateInfo.ReadOnly);
			AssertEquals("cigar.IsMaximumRate", ZBool.False, cigar.IsMaximumRate);
			AssertEquals("cigar.US_UnitPriceInfo.ReadOnly", true, cigar.US_UnitPriceInfo.ReadOnly);
			AssertEquals("cigar.US_UnitPrice", ZDecimal.Zero, cigar.US_UnitPrice);
			cigar.US_UnitPrice = 10m;
			cigar.US_IsSmall = ZBool.False;
			AssertEquals("cigar.IsMaximumRateInfo.ReadOnly", false, cigar.IsMaximumRateInfo.ReadOnly);
			AssertEquals("cigar.IsMaximumRate", ZBool.False, cigar.IsMaximumRate);
			AssertEquals("cigar.US_UnitPriceInfo.ReadOnly", false, cigar.US_UnitPriceInfo.ReadOnly);
			AssertEquals("cigar.US_UnitPrice", 10m, cigar.US_UnitPrice);
			cigar.IsMaximumRate = ZBool.True;
			AssertEquals("cigar.IsMaximumRate", ZBool.True, cigar.IsMaximumRate);
			AssertEquals("cigar.US_UnitPrice", TTBCigar.MaximuSalePrice, cigar.US_UnitPrice);
			AssertEquals("cigar.US_UnitPriceInfo.ReadOnly", false, cigar.US_UnitPriceInfo.ReadOnly);
			cigar.IsMaximumRate = ZBool.False;
			AssertEquals("cigar.US_UnitPrice", ZDecimal.Zero, cigar.US_UnitPrice);
			AssertEquals("cigar.US_UnitPriceInfo.ReadOnly", false, cigar.US_UnitPriceInfo.ReadOnly);
			cigar.US_UnitPrice = TTBCigar.MaximuSalePrice;
			AssertEquals("cigar.IsMaximumRate", ZBool.True, cigar.IsMaximumRate);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			var result = ttbLine.Cigars.AddNew();
			result.US_Quantity = 1;
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return TTBLine.Cigars.AddNew();
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_EnableCRL = true;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		TTBLine TTBLine
		{
			get { return ttbLine ?? (ttbLine = InvoiceLine.TTBLines.AddNew()); }
		}
		TTBLine ttbLine;
		#endregion
	}
}
