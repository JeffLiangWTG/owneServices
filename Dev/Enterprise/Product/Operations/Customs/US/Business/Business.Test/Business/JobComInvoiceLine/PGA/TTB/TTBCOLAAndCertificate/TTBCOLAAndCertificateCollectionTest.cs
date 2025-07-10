using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(TTBCOLAAndCertificateCollection))]
	public class TTBCOLAAndCertificateCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			var colaAndCertificates = TTBLine.COLAAndCertificates;
			TTBLine.US_ProgramCode = ZString.Empty;
			AssertEquals("AllowNew", true, colaAndCertificates.AllowNew);
			var list = new TTBProgramCodeList();
			list.RemoveCode(TTBProgramCodeList.Codes.Tobacco);
			foreach (ICodeDescription pair in list)
			{
				TTBLine.US_ProgramCode = pair.Code;
				AssertEquals("AllowNew", true, colaAndCertificates.AllowNew);
			}
			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			AssertEquals("AllowNew", false, colaAndCertificates.AllowNew);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TTBCOLAAndCertificateCollection(TTBLine);
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
