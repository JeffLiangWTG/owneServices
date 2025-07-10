using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobComInvLineRefsCollection))]
	sealed class JobComInvLineRefsCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetValueForJobComInvLineRefsAndDeleteDuplicateWhenEmpty()
		{
			var invoiceLineRefs = InvoiceLine.InvoiceLineRefs;
			AssertEquals("invoiceLineRefs.Count", 0, invoiceLineRefs.Count);
			var descriptionCount = 0;
			var descriptionInfo_ValueChanged = new EventHandler((object sender, EventArgs e) => descriptionCount++);
			try
			{
				InvoiceLine.JI_DescriptionInfo.ValueChanged += descriptionInfo_ValueChanged;
				var refNo = invoiceLineRefs.SetValueForJobComInvLineRefsAndDeleteDuplicateWhenEmpty("BOB", null, "BUILDER", InvoiceLine.JI_DescriptionInfo);
				AssertEquals("refNo.JG_ReferenceType", "BOB", refNo.JG_ReferenceType);
				AssertEquals("refNo.JG_ReferenceNumber", "BUILDER", refNo.JG_ReferenceNumber);
				AssertEquals("descriptionCount", 1, descriptionCount);
				AssertEquals("invoiceLineRefs.Count", 1, invoiceLineRefs.Count);
				var refNo1 = invoiceLineRefs.SetValueForJobComInvLineRefsAndDeleteDuplicateWhenEmpty("BOB", refNo, "DESTROYER", InvoiceLine.JI_DescriptionInfo);
				AssertEquals("refNo", refNo, refNo1);
				AssertEquals("refNo.JG_ReferenceType", "BOB", refNo.JG_ReferenceType);
				AssertEquals("refNo.JG_ReferenceNumber", "DESTROYER", refNo.JG_ReferenceNumber);
				AssertEquals("descriptionCount", 2, descriptionCount);
				AssertEquals("invoiceLineRefs.Count", 1, invoiceLineRefs.Count);
				var refNo2 = invoiceLineRefs.AddNew("BOB", "BUILDER");
				if (refNo1.PK > refNo2.PK)
				{
					refNo1 = refNo2;
					refNo2 = refNo;
				}
				var refNo3 = invoiceLineRefs.SetValueForJobComInvLineRefsAndDeleteDuplicateWhenEmpty("BOB", refNo1, "THE", InvoiceLine.JI_DescriptionInfo);
				AssertEquals("refNo1", refNo1, refNo3);
				AssertEquals("refNo1.JG_ReferenceType", "BOB", refNo1.JG_ReferenceType);
				AssertEquals("refNo1.JG_ReferenceNumber", "THE", refNo1.JG_ReferenceNumber);
				AssertEquals("descriptionCount", 3, descriptionCount);
				AssertEquals("invoiceLineRefs.Count", 2, invoiceLineRefs.Count);
				refNo3 = invoiceLineRefs.SetValueForJobComInvLineRefsAndDeleteDuplicateWhenEmpty("BOB", refNo2, "THE", InvoiceLine.JI_DescriptionInfo);
				AssertEquals("refNo2", refNo2, refNo3);
				AssertEquals("refNo2.JG_ReferenceType", "BOB", refNo2.JG_ReferenceType);
				AssertEquals("refNo2.JG_ReferenceNumber", "THE", refNo2.JG_ReferenceNumber);
				AssertEquals("descriptionCount", 4, descriptionCount);
				AssertEquals("invoiceLineRefs.Count", 2, invoiceLineRefs.Count);
				refNo3 = invoiceLineRefs.SetValueForJobComInvLineRefsAndDeleteDuplicateWhenEmpty("BOB", null, "D", InvoiceLine.JI_DescriptionInfo);
				AssertEquals("refNo1", refNo1, refNo3);
				AssertEquals("refNo1.JG_ReferenceType", "BOB", refNo1.JG_ReferenceType);
				AssertEquals("refNo1.JG_ReferenceNumber", "D", refNo1.JG_ReferenceNumber);
				AssertEquals("descriptionCount", 5, descriptionCount);
				AssertEquals("invoiceLineRefs.Count", 2, invoiceLineRefs.Count);
				refNo3 = invoiceLineRefs.SetValueForJobComInvLineRefsAndDeleteDuplicateWhenEmpty("BOB", null, ZString.Empty, InvoiceLine.JI_DescriptionInfo);
				AssertEquals("refNo1", refNo1, refNo3);
				AssertEquals("refNo1.JG_ReferenceType", "BOB", refNo1.JG_ReferenceType);
				AssertEquals("refNo1.JG_ReferenceNumber", ZString.Empty, refNo1.JG_ReferenceNumber);
				AssertEquals("descriptionCount", 6, descriptionCount);
				AssertEquals("invoiceLineRefs.Count", 1, invoiceLineRefs.Count);
				AssertEquals("refNo2.IsDeleted", true, refNo2.IsDeleted);
				refNo2 = invoiceLineRefs.AddNew("BOB", "BUILDER");
				refNo3 = invoiceLineRefs.SetValueForJobComInvLineRefsAndDeleteDuplicateWhenEmpty("BOB", refNo2, ZString.Empty, InvoiceLine.JI_DescriptionInfo);
				AssertEquals("refNo2", refNo2, refNo3);
				AssertEquals("refNo2.JG_ReferenceType", "BOB", refNo2.JG_ReferenceType);
				AssertEquals("refNo2.JG_ReferenceNumber", ZString.Empty, refNo2.JG_ReferenceNumber);
				AssertEquals("descriptionCount", 7, descriptionCount);
				AssertEquals("invoiceLineRefs.Count", 1, invoiceLineRefs.Count);
				AssertEquals("refNo1.IsDeleted", true, refNo1.IsDeleted);
				refNo1 = invoiceLineRefs.AddNew("BOB", "BUILDER");
				refNo3 = invoiceLineRefs.SetValueForJobComInvLineRefsAndDeleteDuplicateWhenEmpty("JOE", refNo1, ZString.Empty, InvoiceLine.JI_DescriptionInfo);
				AssertEquals("refNo1", refNo1, refNo3);
				AssertEquals("refNo1.JG_ReferenceType", "JOE", refNo1.JG_ReferenceType);
				AssertEquals("refNo1.JG_ReferenceNumber", ZString.Empty, refNo1.JG_ReferenceNumber);
				AssertEquals("descriptionCount", 8, descriptionCount);
				AssertEquals("invoiceLineRefs.Count", 2, invoiceLineRefs.Count);
				AssertCollectionContains("refNo1", refNo1, invoiceLineRefs);
				AssertCollectionContains("refNo2", refNo2, invoiceLineRefs);
				refNo3 = invoiceLineRefs.SetValueForJobComInvLineRefsAndDeleteDuplicateWhenEmpty("BOB", refNo1, "D", InvoiceLine.JI_DescriptionInfo);
				AssertEquals("refNo1", refNo1, refNo3);
				AssertEquals("refNo1.JG_ReferenceType", "BOB", refNo1.JG_ReferenceType);
				AssertEquals("refNo1.JG_ReferenceNumber", "D", refNo1.JG_ReferenceNumber);
				AssertEquals("descriptionCount", 9, descriptionCount);
				AssertEquals("invoiceLineRefs.Count", 2, invoiceLineRefs.Count);
				AssertCollectionContains("refNo1", refNo1, invoiceLineRefs);
				AssertCollectionContains("refNo2", refNo2, invoiceLineRefs);
			}
			finally
			{
				InvoiceLine.JI_DescriptionInfo.ValueChanged -= descriptionInfo_ValueChanged;
			}
		}

		public void TestGetFirstJobComInvLineRefs()
		{
			var invoiceLineRefs = InvoiceLine.InvoiceLineRefs;
			var refNo1 = invoiceLineRefs.AddNew("BOB", "BUILDER");
			var refNo2 = invoiceLineRefs.AddNew("BOB", "DESTROYER");
			if (refNo1.PK > refNo2.PK)
			{
				var refNo = refNo1;
				refNo1 = refNo2;
				refNo2 = refNo;
			}
			AssertEquals(refNo1, invoiceLineRefs.GetFirstJobComInvLineRefs("BOB"));
			invoiceLineRefs.Sort(JobComInvLineRefs.Schema.JG_ReferenceNumber, System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals(refNo1, invoiceLineRefs.GetFirstJobComInvLineRefs("BOB"));
			invoiceLineRefs.Sort(JobComInvLineRefs.Schema.JG_ReferenceNumber, System.ComponentModel.ListSortDirection.Descending);
			AssertEquals(refNo1, invoiceLineRefs.GetFirstJobComInvLineRefs("BOB"));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var lineRefs = Factory.New<JobComInvLineRefs>();
			lineRefs.JG_JI = InvoiceLine.PK;
			return lineRefs;
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(JobComInvLineRefsCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new JobComInvLineRefsCollection(InvoiceLine);
		}

		BaseJobComInvoiceHeader InvoiceHeader
		{
			get { return invoiceHeader ?? (invoiceHeader = Factory.New<BaseJobComInvoiceHeader>()); }
		}
		BaseJobComInvoiceHeader invoiceHeader;

		BaseJobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew()); }
		}
		BaseJobComInvoiceLine invoiceLine;
	}
}
