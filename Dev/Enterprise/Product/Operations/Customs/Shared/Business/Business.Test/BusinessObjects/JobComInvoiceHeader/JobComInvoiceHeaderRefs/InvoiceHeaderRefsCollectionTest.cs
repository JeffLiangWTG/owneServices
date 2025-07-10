using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderRefsCollection))]
	sealed class InvoiceHeaderRefsCollectionTest : ActiveBusinessObjectCollectionTestCase<InvoiceHeaderRefsCollection>
	{
		public void TestSetValueForJobComInvoiceHeaderRefsAndDeleteDuplicateWhenEmpty()
		{
			var invoiceHeaderRefs = InvoiceHeader.InvoiceHeaderRefs;
			AssertEquals("invoiceHeaderRefs.Count", 0, invoiceHeaderRefs.Count);
			var incoTermCount = 0;
			var descriptionInfo_ValueChanged = new EventHandler((object sender, EventArgs e) => incoTermCount++);
			try
			{
				InvoiceHeader.JZ_IncoTermInfo.ValueChanged += descriptionInfo_ValueChanged;
				var refNo = invoiceHeaderRefs.SetValueForJobComInvoiceHeaderRefsAndDeleteDuplicateWhenEmpty("BOB", null, "BUILDER", InvoiceHeader.JZ_IncoTermInfo);
				AssertEquals("refNo.J2_ReferenceType", "BOB", refNo.J2_ReferenceType);
				AssertEquals("refNo.J2_ReferenceNumber", "BUILDER", refNo.J2_ReferenceNumber);
				AssertEquals("incoTermCount", 1, incoTermCount);
				AssertEquals("invoiceHeaderRefs.Count", 1, invoiceHeaderRefs.Count);
				var refNo1 = invoiceHeaderRefs.SetValueForJobComInvoiceHeaderRefsAndDeleteDuplicateWhenEmpty("BOB", refNo, "DESTROYER", InvoiceHeader.JZ_IncoTermInfo);
				AssertEquals("refNo", refNo, refNo1);
				AssertEquals("refNo.J2_ReferenceType", "BOB", refNo.J2_ReferenceType);
				AssertEquals("refNo.J2_ReferenceNumber", "DESTROYER", refNo.J2_ReferenceNumber);
				AssertEquals("incoTermCount", 2, incoTermCount);
				AssertEquals("invoiceHeaderRefs.Count", 1, invoiceHeaderRefs.Count);
				var refNo2 = invoiceHeaderRefs.AddNew("BOB", "BUILDER");
				if (refNo1.PK > refNo2.PK)
				{
					refNo1 = refNo2;
					refNo2 = refNo;
				}
				var refNo3 = invoiceHeaderRefs.SetValueForJobComInvoiceHeaderRefsAndDeleteDuplicateWhenEmpty("BOB", refNo1, "THE", InvoiceHeader.JZ_IncoTermInfo);
				AssertEquals("refNo1", refNo1, refNo3);
				AssertEquals("refNo1.J2_ReferenceType", "BOB", refNo1.J2_ReferenceType);
				AssertEquals("refNo1.J2_ReferenceNumber", "THE", refNo1.J2_ReferenceNumber);
				AssertEquals("incoTermCount", 3, incoTermCount);
				AssertEquals("invoiceHeaderRefs.Count", 2, invoiceHeaderRefs.Count);
				refNo3 = invoiceHeaderRefs.SetValueForJobComInvoiceHeaderRefsAndDeleteDuplicateWhenEmpty("BOB", refNo2, "THE", InvoiceHeader.JZ_IncoTermInfo);
				AssertEquals("refNo2", refNo2, refNo3);
				AssertEquals("refNo2.J2_ReferenceType", "BOB", refNo2.J2_ReferenceType);
				AssertEquals("refNo2.J2_ReferenceNumber", "THE", refNo2.J2_ReferenceNumber);
				AssertEquals("incoTermCount", 4, incoTermCount);
				AssertEquals("invoiceHeaderRefs.Count", 2, invoiceHeaderRefs.Count);
				refNo3 = invoiceHeaderRefs.SetValueForJobComInvoiceHeaderRefsAndDeleteDuplicateWhenEmpty("BOB", null, "D", InvoiceHeader.JZ_IncoTermInfo);
				AssertEquals("refNo1", refNo1, refNo3);
				AssertEquals("refNo1.J2_ReferenceType", "BOB", refNo1.J2_ReferenceType);
				AssertEquals("refNo1.J2_ReferenceNumber", "D", refNo1.J2_ReferenceNumber);
				AssertEquals("incoTermCount", 5, incoTermCount);
				AssertEquals("invoiceHeaderRefs.Count", 2, invoiceHeaderRefs.Count);
				refNo3 = invoiceHeaderRefs.SetValueForJobComInvoiceHeaderRefsAndDeleteDuplicateWhenEmpty("BOB", null, ZString.Empty, InvoiceHeader.JZ_IncoTermInfo);
				AssertEquals("refNo1", refNo1, refNo3);
				AssertEquals("refNo1.J2_ReferenceType", "BOB", refNo1.J2_ReferenceType);
				AssertEquals("refNo1.J2_ReferenceNumber", ZString.Empty, refNo1.J2_ReferenceNumber);
				AssertEquals("incoTermCount", 6, incoTermCount);
				AssertEquals("invoiceHeaderRefs.Count", 1, invoiceHeaderRefs.Count);
				AssertEquals("refNo2.IsDeleted", true, refNo2.IsDeleted);
				refNo2 = invoiceHeaderRefs.AddNew("BOB", "BUILDER");
				refNo3 = invoiceHeaderRefs.SetValueForJobComInvoiceHeaderRefsAndDeleteDuplicateWhenEmpty("BOB", refNo2, ZString.Empty, InvoiceHeader.JZ_IncoTermInfo);
				AssertEquals("refNo2", refNo2, refNo3);
				AssertEquals("refNo2.J2_ReferenceType", "BOB", refNo2.J2_ReferenceType);
				AssertEquals("refNo2.J2_ReferenceNumber", ZString.Empty, refNo2.J2_ReferenceNumber);
				AssertEquals("incoTermCount", 7, incoTermCount);
				AssertEquals("invoiceHeaderRefs.Count", 1, invoiceHeaderRefs.Count);
				AssertEquals("refNo1.IsDeleted", true, refNo1.IsDeleted);
				refNo1 = invoiceHeaderRefs.AddNew("BOB", "BUILDER");
				refNo3 = invoiceHeaderRefs.SetValueForJobComInvoiceHeaderRefsAndDeleteDuplicateWhenEmpty("JOE", refNo1, ZString.Empty, InvoiceHeader.JZ_IncoTermInfo);
				AssertEquals("refNo1", refNo1, refNo3);
				AssertEquals("refNo1.J2_ReferenceType", "JOE", refNo1.J2_ReferenceType);
				AssertEquals("refNo1.J2_ReferenceNumber", ZString.Empty, refNo1.J2_ReferenceNumber);
				AssertEquals("incoTermCount", 8, incoTermCount);
				AssertEquals("invoiceHeaderRefs.Count", 2, invoiceHeaderRefs.Count);
				AssertCollectionContains("refNo1", refNo1, invoiceHeaderRefs);
				AssertCollectionContains("refNo2", refNo2, invoiceHeaderRefs);
				refNo3 = invoiceHeaderRefs.SetValueForJobComInvoiceHeaderRefsAndDeleteDuplicateWhenEmpty("BOB", refNo1, "D", InvoiceHeader.JZ_IncoTermInfo);
				AssertEquals("refNo1", refNo1, refNo3);
				AssertEquals("refNo1.J2_ReferenceType", "BOB", refNo1.J2_ReferenceType);
				AssertEquals("refNo1.J2_ReferenceNumber", "D", refNo1.J2_ReferenceNumber);
				AssertEquals("incoTermCount", 9, incoTermCount);
				AssertEquals("invoiceHeaderRefs.Count", 2, invoiceHeaderRefs.Count);
				AssertCollectionContains("refNo1", refNo1, invoiceHeaderRefs);
				AssertCollectionContains("refNo2", refNo2, invoiceHeaderRefs);
			}
			finally
			{
				InvoiceHeader.JZ_IncoTermInfo.ValueChanged -= descriptionInfo_ValueChanged;
			}
		}

		public void TestGetFirstJobComInvoiceHeaderRefs()
		{
			var invoiceHeaderRefs = InvoiceHeader.InvoiceHeaderRefs;
			var refNo1 = invoiceHeaderRefs.AddNew("BOB", "BUILDER");
			var refNo2 = invoiceHeaderRefs.AddNew("BOB", "DESTROYER");
			if (refNo1.PK > refNo2.PK)
			{
				var refNo = refNo1;
				refNo1 = refNo2;
				refNo2 = refNo;
			}
			AssertEquals(refNo1, invoiceHeaderRefs.GetFirstJobComInvoiceHeaderRefs("BOB"));
			invoiceHeaderRefs.ApplySort(JobComInvoiceHeaderRefs.Schema.J2_ReferenceNumber, System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals(refNo1, invoiceHeaderRefs.GetFirstJobComInvoiceHeaderRefs("BOB"));
			invoiceHeaderRefs.ApplySort(JobComInvoiceHeaderRefs.Schema.J2_ReferenceNumber, System.ComponentModel.ListSortDirection.Descending);
			AssertEquals(refNo1, invoiceHeaderRefs.GetFirstJobComInvoiceHeaderRefs("BOB"));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			JobComInvoiceHeaderRefs headerRefs = Factory.New<JobComInvoiceHeaderRefs>();
			headerRefs.J2_JZ = InvoiceHeader.PK;
			return headerRefs;
		}

		protected override InvoiceHeaderRefsCollection GetCollectionToTest()
		{
			return new InvoiceHeaderRefsCollection(InvoiceHeader);
		}

		BaseJobComInvoiceHeader InvoiceHeader
		{
			get { return invoiceHeader ?? (invoiceHeader = Factory.New<BaseJobComInvoiceHeader>()); }
		}
		BaseJobComInvoiceHeader invoiceHeader;
	}
}
