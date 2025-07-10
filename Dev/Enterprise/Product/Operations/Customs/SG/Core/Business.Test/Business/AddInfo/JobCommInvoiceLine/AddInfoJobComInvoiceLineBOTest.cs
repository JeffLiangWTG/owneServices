using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(AddInfoJobComInvoiceLine))]
	public class AddInfoJobComInvoiceLineBOTest : AddInfoBOTest
	{
		public void TestInvoiceLine()
		{
			AssertEquals(InvoiceLine, AddInfoJobComInvoiceLine.InvoiceLine);
		}

		#region Implementation
		#region Declaration
		protected JobDeclaration Declaration
		{
			get
			{
				return declaration ?? (declaration = Factory.New<JobDeclaration>());
			}
		}

		JobDeclaration declaration;
		#endregion
		#region InvoiceHeader
		protected JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				return invoiceHeader ?? (invoiceHeader = Declaration.Invoices.AddNew());
			}
		}

		JobComInvoiceHeader invoiceHeader;
		#endregion
		#region InvoiceLine
		protected JobComInvoiceLine InvoiceLine
		{
			get
			{
				return invoiceLine ?? (invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew());
			}
		}

		JobComInvoiceLine invoiceLine;
		#endregion
		#region AddInfoJobComInvoiceLine
		AddInfoJobComInvoiceLine AddInfoJobComInvoiceLine
		{
			get
			{
				return addInfoJobComInvoiceLine ?? (addInfoJobComInvoiceLine = new AddInfoJobComInvoiceLine(InvoiceLine.JI_AddInfoInfo));
			}
		}

		AddInfoJobComInvoiceLine addInfoJobComInvoiceLine;
		#endregion
		#endregion
		#region Overrides
		protected override Type GetExpectedLookupsType()
		{
			return typeof(AddInfoJobComInvoiceLineLookups);
		}

		protected override Type GetExpectedValidationType()
		{
			return typeof(AddInfoJobComInvoiceLineValidation_OUT);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AddInfoJobComInvoiceLine(InvoiceLine.JI_AddInfoInfo);
		}
		#endregion
	}
}
