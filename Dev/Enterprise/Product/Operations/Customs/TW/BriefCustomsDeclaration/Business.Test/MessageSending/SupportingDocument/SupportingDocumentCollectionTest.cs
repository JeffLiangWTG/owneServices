using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(SupportingDocumentCollection))]
	sealed class SupportingDocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SupportingDocumentCollection>
	{
		protected override SupportingDocumentCollection GetCollectionToTest() => new SupportingDocumentCollection(Factory, null, null, null);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new SupportingDocument(Factory);
	}
}
