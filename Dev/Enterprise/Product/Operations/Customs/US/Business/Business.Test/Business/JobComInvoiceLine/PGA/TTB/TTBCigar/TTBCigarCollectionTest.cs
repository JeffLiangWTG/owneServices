using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(TTBCigarCollection))]
	public class TTBCigarCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			var cigars = TTBLine.Cigars;
			TTBLine.US_ProgramCode = ZString.Empty;
			AssertEquals("AllowNew", false, cigars.AllowNew);
			var list = new TTBProgramCodeList();
			list.RemoveCode(TTBProgramCodeList.Codes.Tobacco);
			foreach (ICodeDescription pair in list)
			{
				TTBLine.US_ProgramCode = pair.Code;
				AssertEquals("AllowNew", false, cigars.AllowNew);
			}
			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			AssertEquals("AllowNew", true, cigars.AllowNew);

			foreach (var code in new[]
				{
					TTBTOBProcessingCodeList.Codes.T51,
					TTBTOBProcessingCodeList.Codes.T52,
					TTBTOBProcessingCodeList.Codes.T54,
					TTBTOBProcessingCodeList.Codes.T55
				})
			{
				TTBLine.US_ProcessingCode = code;
				AssertEquals("AllowNew", false, cigars.AllowNew);
			}
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TTBCigarCollection(TTBLine);
		}

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
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
