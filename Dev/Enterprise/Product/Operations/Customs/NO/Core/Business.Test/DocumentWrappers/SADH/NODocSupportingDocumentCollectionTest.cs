using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(NODocSupportingDocumentCollection))]
sealed class NODocSupportingDocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NODocSupportingDocumentCollection>
{
	protected override BusinessObject GetNewElementToAddToTheCollection() => NODocSupportingDocument.New(Factory.New<SupportingDocument>(), Factory);

	protected override NODocSupportingDocumentCollection GetCollectionToTest() => new (Factory);
}
