using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class AttachedDocumentDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestGenerateAttachedDocuments()
		{
			var source = Factory.NewWithValidTestData<RefDocSource>();
			source.RDS_Desc = "Aaaaa!";
			source.RDS_Code = "AAA";

			Factory.Save();

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var invoiceeDoc1 = ((IDocManagerSupport)job).DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "TestInvoiceFile1", "INV", false, Guid.Empty, Guid.Empty, Guid.Empty, documentSource: "AAA");
			invoiceeDoc1.IsPublished = true;
			var invoiceeDoc2 = ((IDocManagerSupport)job).DocManagerInfo.AddFileOrDocument(new byte[] { 4, 5, 6 }, "TestInvoiceFile2", "INV");

			Factory.Save();

			var attachedDocuments = new AttachedDocumentDataObjectWriter().GenerateAttachedDocuments(false, invoiceeDoc1, invoiceeDoc2);
			var metaDataOnlyDocuments = new AttachedDocumentDataObjectWriter().GenerateAttachedDocuments(true, invoiceeDoc1);

			try
			{
				CombineAssertions("logData Contents", delegate
				{
					AssertEquals("number of documents created", 2, attachedDocuments.Length);
					AssertMultilineASCIIEquals("eventData.AttachedDocumentCollection", @"
INV - Invoice - TestInvoiceFile1 - Y
INV - Invoice - TestInvoiceFile2 - N
".Trim(), string.Join("\r\n", attachedDocuments.Select(doc => { return doc.Type.Code + " - " + doc.Type.Description + " - " + doc.FileName + " - " + doc.IsPublished; }).ToArray()));
					AssertArrayEqualsByElements(new byte[] { 1, 2, 3 }, attachedDocuments[0].ImageData.ToByteArray());
					AssertArrayEqualsByElements(new byte[] { 4, 5, 6 }, attachedDocuments[1].ImageData.ToByteArray());
					AssertEquals("AAA", attachedDocuments[0].Source.Code);
					AssertEquals("", attachedDocuments[1].Source.Code);
					AssertEquals(invoiceeDoc1.UniqueKey.ToString(), attachedDocuments[0].DocumentID);
					AssertEquals(invoiceeDoc2.UniqueKey.ToString(), attachedDocuments[1].DocumentID);

					AssertEquals("Expecting meta data only, ImageData should be null", null, metaDataOnlyDocuments[0].ImageData);
					AssertEquals(invoiceeDoc1.UniqueKey.ToString(), metaDataOnlyDocuments[0].DocumentID);
				});
			}
			finally
			{
				foreach (var doc in attachedDocuments)
				{
					doc.Dispose();
				}
			}
		}
	}
}
