using System.Collections;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class InvoiceLineComparerTest : TestCaseWithFactory
	{
		public void TestInvoiceLineComparer()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();

			var parentLine1 = declaration.InvoiceLines.AddNew();
			var parentLine2 = declaration.InvoiceLines.AddNew();

			var childLine1_1 = parentLine1.AddSecondaryInvoiceLine();
			var childLine2_1 = parentLine1.AddSecondaryInvoiceLine();
			var childLine1_2 = parentLine2.AddSecondaryInvoiceLine();

			AssertEquals("PreCondition:LineNumber", (short)1, parentLine1.JI_LineNo);
			AssertEquals("PreCondition:LineNumber", (short)2, parentLine2.JI_LineNo);
			AssertEquals("PreCondition:LineNumber", (short)3, childLine1_1.JI_LineNo);
			AssertEquals("PreCondition:LineNumber", (short)4, childLine2_1.JI_LineNo);
			AssertEquals("PreCondition:LineNumber", (short)5, childLine1_2.JI_LineNo);

			declaration.InvoiceLines.Sort((IComparer)new InvoiceLineComparer());
			AssertEquals("first element after sorted", parentLine1, declaration.InvoiceLines[0]);
			AssertEquals("second element after sorted", childLine1_1, declaration.InvoiceLines[1]);
			AssertEquals("third element after sorted", childLine2_1, declaration.InvoiceLines[2]);

			AssertEquals("forth element after sorted", parentLine2, declaration.InvoiceLines[3]);
			AssertEquals("fifth element after sorted", childLine1_2, declaration.InvoiceLines[4]);
		}

		public void TestInvoiceLineComparerForUSConsidersInvoicePKs()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV002";
			var invoice3 = declaration.Invoices.AddNew();

			var parentLine1 = declaration.InvoiceLines.AddNew();
			var childLine1_1 = parentLine1.AddSecondaryInvoiceLine();
			var childLine2_1 = parentLine1.AddSecondaryInvoiceLine();
			parentLine1.JI_JZ = invoice1.PK;

			var parentLine2 = declaration.InvoiceLines.AddNew();
			var childLine1_2 = parentLine2.AddSecondaryInvoiceLine();
			parentLine2.JI_JZ = invoice1.PK;

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_JZ = invoice2.PK;
			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_JZ = invoice2.PK;

			var invoiceLine5 = declaration.InvoiceLines.AddNew();
			invoiceLine5.JI_JZ = invoice3.PK;

			var invoiceLine6 = declaration.InvoiceLines.AddNew();
			invoiceLine6.JI_JZ = invoice2.PK;

			var invoiceLine7 = declaration.InvoiceLines.AddNew();
			invoiceLine7.JI_JZ = invoice1.PK;

			AssertEquals("invoice lines", 10, declaration.InvoiceLines.Count);

			AssertEquals("PreCondition: declaration lines", invoice1.PK, declaration.InvoiceLines[0].JI_JZ);
			AssertEquals("PreCondition: declaration lines", invoice1.PK, declaration.InvoiceLines[1].JI_JZ);
			AssertEquals("PreCondition: declaration lines", invoice1.PK, declaration.InvoiceLines[2].JI_JZ);
			AssertEquals("PreCondition: declaration lines", invoice1.PK, declaration.InvoiceLines[3].JI_JZ);
			AssertEquals("PreCondition: declaration lines", invoice1.PK, declaration.InvoiceLines[4].JI_JZ);
			AssertEquals("PreCondition: declaration lines", invoice2.PK, declaration.InvoiceLines[5].JI_JZ);
			AssertEquals("PreCondition: declaration lines", invoice2.PK, declaration.InvoiceLines[6].JI_JZ);
			AssertEquals("PreCondition: declaration lines", invoice3.PK, declaration.InvoiceLines[7].JI_JZ);
			AssertEquals("PreCondition: declaration lines", invoice2.PK, declaration.InvoiceLines[8].JI_JZ);
			AssertEquals("PreCondition: declaration lines", invoice1.PK, declaration.InvoiceLines[9].JI_JZ);

			declaration.InvoiceLines.Sort((IComparer)new InvoiceLineComparer());

			//Assert lines have been sorted by invoice
			if (declaration.InvoiceLines[0].JI_JZ == invoice1.PK)
			{
				AssertEquals("first line after sort - Invoice 1", parentLine1, declaration.InvoiceLines[0]);
				AssertEquals("second line after sort - Invoice 1", childLine1_1, declaration.InvoiceLines[1]);
				AssertEquals("third line after sort - Invoice 1", childLine2_1, declaration.InvoiceLines[2]);
				AssertEquals("forth line after sort - Invoice 1", parentLine2, declaration.InvoiceLines[3]);
				AssertEquals("fifth line after sort - Invoice 1", childLine1_2, declaration.InvoiceLines[4]);
				AssertEquals("sixth line after sort - Invoice 1", invoiceLine7, declaration.InvoiceLines[5]);

				if (declaration.InvoiceLines[6].JI_JZ == invoice2.PK)
				{
					AssertEquals("seventh line after sort - Invoice 2", invoiceLine3, declaration.InvoiceLines[6]);
					AssertEquals("eighth line after sort - Invoice 2", invoiceLine4, declaration.InvoiceLines[7]);
					AssertEquals("ninth line after sort - Invoice 2", invoiceLine6, declaration.InvoiceLines[8]);

					AssertEquals("tenth line after sort - Invoice 3", invoiceLine5, declaration.InvoiceLines[9]);
				}
				else if (declaration.InvoiceLines[6].JI_JZ == invoice3.PK)
				{
					AssertEquals("seventh line after sort - Invoice 3", invoiceLine5, declaration.InvoiceLines[6]);

					AssertEquals("eighth line after sort - Invoice 2", invoiceLine3, declaration.InvoiceLines[7]);
					AssertEquals("ninth line after sort - Invoice 2", invoiceLine4, declaration.InvoiceLines[8]);
					AssertEquals("tenth line after sort - Invoice 2", invoiceLine6, declaration.InvoiceLines[9]);
				}
			}
			else if (declaration.InvoiceLines[0].JI_JZ == invoice2.PK)
			{
				AssertEquals("Invoice 2", invoiceLine3, declaration.InvoiceLines[0]);
				AssertEquals("Invoice 2", invoiceLine4, declaration.InvoiceLines[1]);
				AssertEquals("Invoice 2", invoiceLine6, declaration.InvoiceLines[2]);

				if (declaration.InvoiceLines[3].JI_JZ == invoice1.PK)
				{
					AssertEquals("Invoice 1", parentLine1, declaration.InvoiceLines[3]);
					AssertEquals("Invoice 1", childLine1_1, declaration.InvoiceLines[4]);
					AssertEquals("Invoice 1", childLine2_1, declaration.InvoiceLines[5]);
					AssertEquals("Invoice 1", parentLine2, declaration.InvoiceLines[6]);
					AssertEquals("Invoice 1", childLine1_2, declaration.InvoiceLines[7]);
					AssertEquals("Invoice 1", invoiceLine7, declaration.InvoiceLines[8]);

					AssertEquals("Invoice 3", invoiceLine5, declaration.InvoiceLines[9]);
				}
				else if (declaration.InvoiceLines[3].JI_JZ == invoice3.PK)
				{
					AssertEquals("Invoice 3", invoiceLine5, declaration.InvoiceLines[3]);

					AssertEquals("Invoice 1", parentLine1, declaration.InvoiceLines[4]);
					AssertEquals("Invoice 1", childLine1_1, declaration.InvoiceLines[5]);
					AssertEquals("Invoice 1", childLine2_1, declaration.InvoiceLines[6]);
					AssertEquals("Invoice 1", parentLine2, declaration.InvoiceLines[7]);
					AssertEquals("Invoice 1", childLine1_2, declaration.InvoiceLines[8]);
					AssertEquals("Invoice 1", invoiceLine7, declaration.InvoiceLines[9]);
				}
			}
			else if (declaration.InvoiceLines[0].JI_JZ == invoice3.PK)
			{
				AssertEquals("tenth line after sort - Invoice 3", invoiceLine5, declaration.InvoiceLines[0]);

				if (declaration.InvoiceLines[1].JI_JZ == invoice1.PK)
				{
					AssertEquals("Invoice 1", parentLine1, declaration.InvoiceLines[1]);
					AssertEquals("Invoice 1", childLine1_1, declaration.InvoiceLines[2]);
					AssertEquals("Invoice 1", childLine2_1, declaration.InvoiceLines[3]);
					AssertEquals("Invoice 1", parentLine2, declaration.InvoiceLines[4]);
					AssertEquals("Invoice 1", childLine1_2, declaration.InvoiceLines[5]);
					AssertEquals("Invoice 1", invoiceLine7, declaration.InvoiceLines[6]);

					AssertEquals("Invoice 2", invoiceLine3, declaration.InvoiceLines[7]);
					AssertEquals("Invoice 2", invoiceLine4, declaration.InvoiceLines[8]);
					AssertEquals("Invoice 2", invoiceLine6, declaration.InvoiceLines[9]);
				}
				else
				{
					AssertEquals("Invoice 2", invoiceLine3, declaration.InvoiceLines[1]);
					AssertEquals("Invoice 2", invoiceLine4, declaration.InvoiceLines[2]);
					AssertEquals("Invoice 2", invoiceLine6, declaration.InvoiceLines[3]);

					AssertEquals("Invoice 1", parentLine1, declaration.InvoiceLines[4]);
					AssertEquals("Invoice 1", childLine1_1, declaration.InvoiceLines[5]);
					AssertEquals("Invoice 1", childLine2_1, declaration.InvoiceLines[6]);
					AssertEquals("Invoice 1", parentLine2, declaration.InvoiceLines[7]);
					AssertEquals("Invoice 1", childLine1_2, declaration.InvoiceLines[8]);
					AssertEquals("Invoice 1", invoiceLine7, declaration.InvoiceLines[9]);
				}
			}
		}
	}
}
