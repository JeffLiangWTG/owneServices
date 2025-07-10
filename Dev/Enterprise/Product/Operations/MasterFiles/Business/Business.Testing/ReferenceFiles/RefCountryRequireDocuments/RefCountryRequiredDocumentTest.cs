using Enterprise.Core;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefCountryRequiredDocument))]
	sealed class RefCountryRequiredDocumentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDocTypeDescription()
		{
			RefDocType docType = Factory.New<RefDocType>();
			docType.RT_DocType = "XXX";
			docType.RT_ReferenceType = Constants.ReferenceTypes.SupplyChainLogistics;

			RefCountryRequiredDocument requiredDocument = Factory.New<RefCountryRequiredDocument>();
			requiredDocument.RD_DocType = docType.RT_DocType;
			AssertEquals("Document Description", "", requiredDocument.DocTypeDescription);

			docType.RT_Desc = "Hello";
			AssertEquals("Document Description", "Hello", requiredDocument.DocTypeDescription);
		}
	}
}
