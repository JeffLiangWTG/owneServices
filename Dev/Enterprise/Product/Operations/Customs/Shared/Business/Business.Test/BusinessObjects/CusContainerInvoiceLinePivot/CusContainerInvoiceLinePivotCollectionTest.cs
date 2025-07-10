using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusContainerInvoiceLinePivotCollection))]
	sealed class CusContainerInvoiceLinePivotCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestBasicCollectionFunctionality()
		{
			BaseCusContainer container = Factory.New<BaseCusContainer>();
			CusContainerInvoiceLinePivotCollection collection = new CusContainerInvoiceLinePivotCollection(container, Factory);

			CusContainerInvoiceLinePivot pivot1 = collection.AddNew();
			CusContainerInvoiceLinePivot pivot2 = collection.AddNew();

			AssertEquals("Collection.Count", 2, collection.Count);

			foreach (CusContainerInvoiceLinePivot pivot in collection)
			{
				AssertEquals("CusContainerInvoiceLinePivot should link to CusContainer", container.PK, pivot.C2_CO);
			}
		}

		public void TestAddNewWithInvoiceLineAsParameter()
		{
			BaseJobComInvoiceLine invoiceLine1 = Factory.New<BaseJobComInvoiceLine>();
			BaseJobComInvoiceLine invoiceLine2 = Factory.New<BaseJobComInvoiceLine>();
			BaseCusContainer container = Factory.New<BaseCusContainer>();
			CusContainerInvoiceLinePivotCollection collection = new CusContainerInvoiceLinePivotCollection(container, Factory);

			CusContainerInvoiceLinePivot pivot1 = collection.AddNew(invoiceLine1);
			CusContainerInvoiceLinePivot pivot2 = collection.AddNew(invoiceLine2);

			AssertEquals("Collection.Count", 2, collection.Count);

			AssertEquals("InvoiceLine1 should be linked to Pivot1", invoiceLine1.PK, pivot1.C2_JI);
			AssertEquals("InvoiceLine2 should be linked to Pivot2", invoiceLine2.PK, pivot2.C2_JI);
		}

		public void TestInvoiceLinesAssociated()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseCusContainer container1 = testDec.CusContainers.AddNew();
			BaseCusContainer container2 = testDec.CusContainers.AddNew();

			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine3 = invoice.JobComInvoiceLines.AddNew();

			CusContainerInvoiceLinePivot pivot1ToInvoiceLine1 = container1.InvoiceLinePivotCollection.AddNew();
			pivot1ToInvoiceLine1.C2_JI = invoiceLine1.PK;

			CusContainerInvoiceLinePivot pivot2ToEmptyInvoiceLine = container2.InvoiceLinePivotCollection.AddNew();
			pivot2ToEmptyInvoiceLine.C2_JI = ZGuid.Empty;

			CusContainerInvoiceLinePivot pivot3ToInvoiceLine3 = container2.InvoiceLinePivotCollection.AddNew();
			pivot3ToInvoiceLine3.C2_JI = invoiceLine3.PK;

			CusContainerInvoiceLinePivot pivot4ToInvoiceLine3 = container2.InvoiceLinePivotCollection.AddNew();
			pivot4ToInvoiceLine3.C2_JI = invoiceLine3.PK;

			AssertEquals("InvoiceLines associated with Container1", 1, container1.InvoiceLinePivotCollection.InvoiceLinesAssociated.Length);
			AssertEquals("InvoiceLines associated with Container2", 1, container2.InvoiceLinePivotCollection.InvoiceLinesAssociated.Length);
			AssertEquals("Container1 is linked to InvoiceLine1", invoiceLine1, container1.InvoiceLinePivotCollection.InvoiceLinesAssociated[0]);
			AssertEquals("Container2 is linked to InvoiceLine3", invoiceLine3, container2.InvoiceLinePivotCollection.InvoiceLinesAssociated[0]);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			BaseCusContainer container = Factory.New<BaseCusContainer>();
			return new CusContainerInvoiceLinePivotCollection(container, Factory);
		}
	}
}
