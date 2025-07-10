using System;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(EmmaDocumentProvider))]
sealed class EmmaDocumentProviderTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("When entryHeader is null", () => new EmmaDocumentProvider(null));
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertExceptionThrown<ArgumentNullException>("When entryHeader.Declaration is null", () => new EmmaDocumentProvider(entryHeader));

		var declaration = Factory.New<JobDeclaration>();
		entryHeader.CH_JE = declaration.PK;
		AssertNoExceptionThrown("When entryHeader with declaration is provided", () => new EmmaDocumentProvider(entryHeader));
	});

	[TestDate(2025, 1, 1)]
	public void TestGetDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.ActiveEntryHeaders.AddNew();

		entryHeader.SetEntryReleaseNumber("12553343");
		var entryHeaderDocManager = entryHeader.DocManagerInfo();
		var sadhDoc = entryHeaderDocManager.AddFileOrDocument(Encoding.ASCII.GetBytes("stub SADH NO document 2"), "sadh-file.pdf", Core.Constants.RefDocTypes.EntryPrint);
		_ = entryHeaderDocManager.AddFileOrDocument(Encoding.ASCII.GetBytes("stub Misc document"), "misc-file.png", Core.Constants.RefDocTypes.MiscellaneousDocument);

		var declarationDocManager = declaration.DocManagerInfo();
		var invoiceDoc = declarationDocManager.AddFileOrDocument(Encoding.ASCII.GetBytes("stub Invoice document"), "inv-file.txt", Core.Constants.RefDocTypes.Invoice);

		var documentsDataProvider = new EmmaDocumentProvider(entryHeader);
		var attachedDocs = documentsDataProvider.GetDocuments();
		CombineAssertions("When Entry Release Number is present", () =>
		{
			AssertEquals("Total attached document", 2, attachedDocs.Count);
			AssertEmmaDocument("INV-12553343-inv-file.txt", invoiceDoc);
			AssertEmmaDocument("EPR-12553343-sadh-file.pdf", sadhDoc);
		});

		entryHeader.SetEntryReleaseNumber(ZString.Empty);
		documentsDataProvider = new EmmaDocumentProvider(entryHeader);
		attachedDocs = documentsDataProvider.GetDocuments();
		CombineAssertions("When Entry Release Number is not present", () =>
		{
			AssertEquals("Total attached document", 2, attachedDocs.Count);
			AssertEmmaDocument("INV-inv-file.txt", invoiceDoc);
			AssertEmmaDocument("EPR-sadh-file.pdf", sadhDoc);
		});
		void AssertEmmaDocument(string fileName, IeDoc originalDocument)
		{
			var document = attachedDocs.FirstOrDefault(f => f.FileName == fileName);
			AssertNotNull($"{fileName}", document);
			AssertEquals("Description", originalDocument.Description, document.Description);
			AssertEquals("Document ID", originalDocument.UniqueKey, document.DocumentId);
			AssertEquals("Date Added", originalDocument.DateAdded.ToLongTimeString(), document.DateAdded.ToLongTimeString());
		}
	}
}
