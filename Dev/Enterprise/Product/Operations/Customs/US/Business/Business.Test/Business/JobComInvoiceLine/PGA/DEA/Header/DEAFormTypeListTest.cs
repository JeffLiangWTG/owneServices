using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	class DEAFormTypeListTest : TestCase
	{
		public void TestGetDocumentIdentifierFromFormType()
		{
			AssertEquals(DocumentIdentifierList.Codes.ImportLicense, DEAFormTypeList.GetDocumentIdentifierFromFormType(DEAFormTypeList.Codes.DEA35));
			AssertEquals(DocumentIdentifierList.Codes.DEA236, DEAFormTypeList.GetDocumentIdentifierFromFormType(DEAFormTypeList.Codes.DEA236));
			AssertEquals(DocumentIdentifierList.Codes.DEA486, DEAFormTypeList.GetDocumentIdentifierFromFormType(DEAFormTypeList.Codes.DEA486));
			AssertEquals(DocumentIdentifierList.Codes.DEA486A, DEAFormTypeList.GetDocumentIdentifierFromFormType(DEAFormTypeList.Codes.DEA486A));
		}

		public void TestGetFormTypeFromDocumentIdentifier()
		{
			AssertEquals(DEAFormTypeList.Codes.DEA35, DEAFormTypeList.GetFormTypeFromDocumentIdentifier(DocumentIdentifierList.Codes.ImportLicense));
			AssertEquals(DEAFormTypeList.Codes.DEA236, DEAFormTypeList.GetFormTypeFromDocumentIdentifier(DocumentIdentifierList.Codes.DEA236));
			AssertEquals(DEAFormTypeList.Codes.DEA486, DEAFormTypeList.GetFormTypeFromDocumentIdentifier(DocumentIdentifierList.Codes.DEA486));
			AssertEquals(DEAFormTypeList.Codes.DEA486A, DEAFormTypeList.GetFormTypeFromDocumentIdentifier(DocumentIdentifierList.Codes.DEA486A));
		}
	}
}
