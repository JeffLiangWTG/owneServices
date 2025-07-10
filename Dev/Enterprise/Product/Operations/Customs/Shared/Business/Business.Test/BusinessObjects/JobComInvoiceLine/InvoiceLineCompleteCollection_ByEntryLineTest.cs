using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	class InvoiceLineCompleteCollection_ByEntryLineTest : TestCaseWithFactory
	{
		public void TestByEntryLine_HasAllChildren()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();

			var invoiceLine1 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine1.JI_CL = entryLine1.PK;
			declaration.InvoiceLines.Add(invoiceLine1);

			var invoiceLine2 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine2.JI_CL = entryLine1.PK;
			declaration.InvoiceLines.Add(invoiceLine2);

			var invoiceLine3 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine3.JI_CL = entryLine2.PK;
			declaration.InvoiceLines.Add(invoiceLine3);

			var invoiceLine4 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine4.JI_CL = entryLine2.PK;
			declaration.InvoiceLines.Add(invoiceLine4);

			var collection = declaration.InvoiceLines;
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine1, invoiceLine2 }, collection.ByEntryLine[entryLine1.PK]);
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine3, invoiceLine4 }, collection.ByEntryLine[entryLine2.PK]);
		}

		public void TestByEntryLine_AddingInvoiceLine()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			var invoiceLine1 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine1.JI_CL = entryLine1.PK;
			declaration.InvoiceLines.Add(invoiceLine1);

			var collection = declaration.InvoiceLines;
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine1 }, collection.ByEntryLine[entryLine1.PK]);

			var invoiceLine2 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine2.JI_CL = entryLine1.PK;
			declaration.InvoiceLines.Add(invoiceLine2);
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine1, invoiceLine2 }, collection.ByEntryLine[entryLine1.PK]);

			AssertEquals(0, collection.ByEntryLine[entryLine2.PK].Count());

			var invoiceLine3 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine3.JI_CL = entryLine2.PK;
			declaration.InvoiceLines.Add(invoiceLine3);
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine3 }, collection.ByEntryLine[entryLine2.PK]);
		}

		public void TestByEntryLine_RemovingInvoiceLine()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader.AllEntryLines.AddNew();

			var invoiceLine1 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine1.JI_CL = entryLine1.PK;
			declaration.InvoiceLines.Add(invoiceLine1);

			var invoiceLine2 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine2.JI_CL = entryLine1.PK;
			declaration.InvoiceLines.Add(invoiceLine2);

			var collection = declaration.InvoiceLines;
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine1, invoiceLine2 }, collection.ByEntryLine[entryLine1.PK]);

			collection.Remove(invoiceLine1);
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine2 }, collection.ByEntryLine[entryLine1.PK]);
		}

		public void TestByEntryLine_LinkingInvoiceLineToDifferenceEntryLine()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();

			var invoiceLine1 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine1.JI_CL = entryLine1.PK;
			declaration.InvoiceLines.Add(invoiceLine1);

			var invoiceLine2 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine2.JI_CL = entryLine1.PK;
			declaration.InvoiceLines.Add(invoiceLine2);

			var invoiceLine3 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine3.JI_CL = entryLine2.PK;
			declaration.InvoiceLines.Add(invoiceLine3);

			var collection = declaration.InvoiceLines;
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine1, invoiceLine2 }, collection.ByEntryLine[entryLine1.PK]);
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine3 }, collection.ByEntryLine[entryLine2.PK]);

			invoiceLine2.JI_CL = entryLine2.PK;
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine1 }, collection.ByEntryLine[entryLine1.PK]);
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine2, invoiceLine3 }, collection.ByEntryLine[entryLine2.PK]);
		}

		public void TestByEntryLine_AdditionalEntryLineLinks_HasAllChildren()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();

			var invoiceLine1 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			declaration.InvoiceLines.Add(invoiceLine1);

			var invoiceLine2 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine2.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			declaration.InvoiceLines.Add(invoiceLine2);

			var invoiceLine3 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine3.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine2);
			declaration.InvoiceLines.Add(invoiceLine3);

			var invoiceLine4 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine4.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine2);
			declaration.InvoiceLines.Add(invoiceLine4);

			var collection = declaration.InvoiceLines;
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine1, invoiceLine2 }, collection.ByEntryLine[entryLine1.PK]);
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine3, invoiceLine4 }, collection.ByEntryLine[entryLine2.PK]);
		}

		public void TestByEntryLine_AdditionalEntryLineLinks_NotSupported()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				AssertEquals("Precondition: Australia does not use AdditionalEntryLineLinks", false, declaration.NeedsAdditionalLinkBetweenInvoiceLineAndEntryLine);

				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				var entryLine1 = entryHeader.AllEntryLines.AddNew();
				var invoiceLine1 = Factory.New<BaseJobComInvoiceLine>();
				invoiceLine1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
				declaration.InvoiceLines.Add(invoiceLine1);

				var collection = declaration.InvoiceLines;
				AssertEquals(0, collection.ByEntryLine[entryLine1.PK].Count());

				invoiceLine1.JI_CL = entryLine1.PK;
				AssertContainsExactElementsInAnyOrder(new[] { invoiceLine1 }, collection.ByEntryLine[entryLine1.PK]);
			}
		}

		public void TestByEntryLine_AdditionalEntryLineLinks_AddingInvoiceLine()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			var invoiceLine1 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			declaration.InvoiceLines.Add(invoiceLine1);

			var collection = declaration.InvoiceLines;
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine1 }, collection.ByEntryLine[entryLine1.PK]);

			var invoiceLine2 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine2.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			declaration.InvoiceLines.Add(invoiceLine2);
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine1, invoiceLine2 }, collection.ByEntryLine[entryLine1.PK]);

			AssertEquals(0, collection.ByEntryLine[entryLine2.PK].Count());

			var invoiceLine3 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine3.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine2);
			declaration.InvoiceLines.Add(invoiceLine3);
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine3 }, collection.ByEntryLine[entryLine2.PK]);
		}

		public void TestByEntryLine_AdditionalEntryLineLinks_RemovingInvoiceLine()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader.AllEntryLines.AddNew();

			var invoiceLine1 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			declaration.InvoiceLines.Add(invoiceLine1);

			var invoiceLine2 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine2.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			declaration.InvoiceLines.Add(invoiceLine2);

			var collection = declaration.InvoiceLines;
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine1, invoiceLine2 }, collection.ByEntryLine[entryLine1.PK]);

			collection.Remove(invoiceLine1);
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine2 }, collection.ByEntryLine[entryLine1.PK]);
		}

		public void TestByEntryLine_AdditionalEntryLineLinks_LinkingInvoiceLineToDifferentEntryLine()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();

			var invoiceLine1 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			declaration.InvoiceLines.Add(invoiceLine1);

			var invoiceLine2 = Factory.New<BaseJobComInvoiceLine>();
			var link = invoiceLine2.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			declaration.InvoiceLines.Add(invoiceLine2);

			var invoiceLine3 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine3.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine2);
			declaration.InvoiceLines.Add(invoiceLine3);

			var collection = declaration.InvoiceLines;
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine1, invoiceLine2 }, collection.ByEntryLine[entryLine1.PK]);
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine3 }, collection.ByEntryLine[entryLine2.PK]);

			link.BU_CL = entryLine2.PK;
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine1 }, collection.ByEntryLine[entryLine1.PK]);
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine2, invoiceLine3 }, collection.ByEntryLine[entryLine2.PK]);
		}

		public void TestByEntryLine_AdditionalEntryLineLinks_AddingLink()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();

			var invoiceLine1 = Factory.New<BaseJobComInvoiceLine>();
			declaration.InvoiceLines.Add(invoiceLine1);
			var collection = declaration.InvoiceLines;
			AssertEquals(0, collection.ByEntryLine[entryLine1.PK].Count());

			invoiceLine1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine1 }, collection.ByEntryLine[entryLine1.PK]);

			invoiceLine1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine2);
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine1 }, collection.ByEntryLine[entryLine1.PK]);
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine1 }, collection.ByEntryLine[entryLine2.PK]);
		}

		public void TestByEntryLine_AdditionalEntryLineLinks_RemovingLink()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader.AllEntryLines.AddNew();

			var invoiceLine1 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			declaration.InvoiceLines.Add(invoiceLine1);

			var invoiceLine2 = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine2.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			declaration.InvoiceLines.Add(invoiceLine2);

			var collection = declaration.InvoiceLines;
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine1, invoiceLine2 }, collection.ByEntryLine[entryLine1.PK]);

			invoiceLine1.AdditionalEntryLineLinks.RemoveAllFromRelationship();
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine2 }, collection.ByEntryLine[entryLine1.PK]);

			invoiceLine1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine1, invoiceLine2 }, collection.ByEntryLine[entryLine1.PK]);

			invoiceLine1.AdditionalEntryLineLinks.DeleteLinkIfExistsFor(entryLine1);
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine2 }, collection.ByEntryLine[entryLine1.PK]);
		}

		public void TestByEntryLine_AdditionalEntryLineLinks_ChangingLink()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();

			var invoiceLine1 = Factory.New<BaseJobComInvoiceLine>();
			var link1 = invoiceLine1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			declaration.InvoiceLines.Add(invoiceLine1);

			var invoiceLine2 = Factory.New<BaseJobComInvoiceLine>();
			var link2 = invoiceLine2.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine2);
			declaration.InvoiceLines.Add(invoiceLine2);

			var collection = declaration.InvoiceLines;
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine1 }, collection.ByEntryLine[entryLine1.PK]);
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine2 }, collection.ByEntryLine[entryLine2.PK]);

			link1.BU_CL = entryLine2.PK;
			link2.BU_CL = entryLine1.PK;
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine2 }, collection.ByEntryLine[entryLine1.PK]);
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine1 }, collection.ByEntryLine[entryLine2.PK]);
		}
	}
}
