using System;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

class FileProviderTest : Customs.Business.Testing.DataProviderTestCase<FileProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null EDoc", "Value cannot be null.\r\nParameter name: eDoc", () => new FileProvider(null));
	}

	public void TestName()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty Name", string.Empty, GetProvider().Name);

			var declarationEdoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "filename.pdf", Core.Constants.RefDocTypes.DocumentOfOrigin);
			eDoc.EDoc = declarationEdoc.UniqueKey;
			AssertEquals("Not Empty Name", "filename.pdf", GetProvider().Name);
		});
	}

	public void TestCode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty Code", string.Empty, GetProvider().Code);

			eDoc.AdditionalInformation = string.Empty;
			eDoc.SupportingDocument = "ASD";
			AssertEquals("Not Empty SupportingDocument", "ASD", GetProvider().Code);

			eDoc.AdditionalInformation = "QWE";
			eDoc.SupportingDocument = string.Empty;
			AssertEquals("Not Empty AdditionalInformation", "QWE", GetProvider().Code);
		});
	}

	public void TestDescription()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty Code", string.Empty, GetProvider().Description);

			eDoc.DocumentDescription = "something something";
			AssertEquals("Not Empty Description", "something something", GetProvider().Description);
		});
	}

	protected override FileProvider GetProvider() => new FileProvider(eDoc);

	protected override void SetUp()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryInstructions.AddNew();
		declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var lineMerger = new LineMerger(declaration);
		lineMerger.DoMerge();
		sendingObjectParent = new CustomsDeclarationMessageSendingObjectParent(declaration);
		eDoc = sendingObjectParent.EDocs.AddNew();
	}

	JobDeclaration declaration;
	CustomsDeclarationMessageSendingObjectParent sendingObjectParent;
	JobDeclarationMessageSendingEDocs eDoc;
}
