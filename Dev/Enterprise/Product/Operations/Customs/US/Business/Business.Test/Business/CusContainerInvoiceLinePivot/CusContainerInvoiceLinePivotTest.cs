using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusContainerInvoiceLinePivot))]
	sealed class CusContainerInvoiceLinePivotTest : Customs.Business.Testing.CusContainerInvoiceLinePivotTest
	{
		public void TestGetGenPivotQuery()
		{
			var declaration = Factory.New<JobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLineContainer = invoiceLine.ContainersForInvoiceLinesForBindingOnly[0];
			invoiceLineContainer.IsForInvoiceLine = true;
			var pivot = invoiceLineContainer.Pivot;
			var methodInfo = pivot.GetType().GetMethod("GetGenPivotQuery", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			var result = methodInfo.Invoke(pivot, new object[] { new ZString("Test") }) as ZQuery;
			CombineAssertions("Please make sure the sql script of fetch hint on GenPiovt hint the index 'NR_RX__XX_RelationType_XX_Relation2ID' ([XX_RelationType] ASC, [XX_Relation2ID] ASC)", () =>
			{
				AssertNotNull(result);
				Assert("The sql script should contains 'XX_Relation2ID' and 'XX_RelationType'", result.FilterString.Contains(GenPivot.Schema.XX_Relation2ID));
				Assert("The sql script should contains 'XX_Relation2ID' and 'XX_RelationType'", result.FilterString.Contains(GenPivot.Schema.XX_RelationType));
			});
		}

		public void TestDeleteRelatedObject()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusContainer container = declaration.CusContainers.AddNew();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			NonPersistentCusContainer invoiceLineContainer = invoiceLine.ContainersForInvoiceLinesForBindingOnly[0];
			invoiceLineContainer.IsForInvoiceLine = true;
			CusContainerInvoiceLinePivot pivot = invoiceLineContainer.Pivot;
			FDA fda = invoiceLine.FDAs.AddNew();
			var fdaContainersForInvoiceLine = fda.ContainersForInvoiceLine;
			AssertEquals(1, fdaContainersForInvoiceLine.Count);
			fdaContainersForInvoiceLine[0].IsForFDALine = true;
			AssertEquals(1, fda.ContainersForFDALine.Count);
			FDARelatedContainersGenPivot fdaPivot = fda.ContainersForFDALine[0];
			AssertEquals(container, fdaPivot.Container);
			AssertEquals(pivot, fdaPivot.Relation2Object);
			PGA pga = invoiceLine.LaceyActLines.AddNew();
			var pgaContainersForInvoiceLine = pga.ContainersForInvoiceLine;
			AssertEquals(1, pgaContainersForInvoiceLine.Count);
			pgaContainersForInvoiceLine[0].IsForPGALine = true;
			AssertEquals(1, pga.ContainersForPGALine.Count);
			PGARelatedContainersGenPivot pgaPivot = pga.ContainersForPGALine[0];
			AssertEquals(container, pgaPivot.Container);
			AssertEquals(pivot, pgaPivot.Relation2Object);
			invoiceLineContainer.IsForInvoiceLine = false;
			AssertEquals(true, pivot.IsDeleted);
			AssertEquals(0, fda.ContainersForInvoiceLine.Count);
			AssertEquals(0, fda.ContainersForFDALine.Count);
			AssertEquals(true, fdaPivot.IsDeleted);
			AssertEquals(0, pga.ContainersForInvoiceLine.Count);
			AssertEquals(0, pga.ContainersForPGALine.Count);
			AssertEquals(true, pgaPivot.IsDeleted);
			invoiceLineContainer.IsForInvoiceLine = true;
			AssertNotEquals(pivot, invoiceLineContainer.Pivot);
			pivot = invoiceLineContainer.Pivot;
			fdaContainersForInvoiceLine = fda.ContainersForInvoiceLine;
			AssertEquals(1, fdaContainersForInvoiceLine.Count);
			fdaContainersForInvoiceLine[0].IsForFDALine = true;
			AssertEquals(1, fda.ContainersForFDALine.Count);
			fdaPivot = fda.ContainersForFDALine[0];
			AssertEquals(container, fdaPivot.Container);
			AssertEquals(pivot, fdaPivot.Relation2Object);
			pga = invoiceLine.LaceyActLines.AddNew();
			pgaContainersForInvoiceLine = pga.ContainersForInvoiceLine;
			AssertEquals(1, pgaContainersForInvoiceLine.Count);
			pgaContainersForInvoiceLine[0].IsForPGALine = true;
			AssertEquals(1, pga.ContainersForPGALine.Count);
			pgaPivot = pga.ContainersForPGALine[0];
			AssertEquals(container, pgaPivot.Container);
			AssertEquals(pivot, pgaPivot.Relation2Object);
			container.Delete();
			AssertEquals(0, invoiceLine.ContainersForInvoiceLinesForBindingOnly.Count);
			AssertEquals(true, pivot.IsDeleted);
			AssertEquals(0, fda.ContainersForInvoiceLine.Count);
			AssertEquals(0, fda.ContainersForFDALine.Count);
			AssertEquals(true, fdaPivot.IsDeleted);
			AssertEquals(0, pga.ContainersForInvoiceLine.Count);
			AssertEquals(0, pga.ContainersForPGALine.Count);
			AssertEquals(true, pgaPivot.IsDeleted);
		}
	}
}
