using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(AlternativeEvidenceCollection))]
sealed class AlternativeEvidenceCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AlternativeEvidenceCollection>
{
	protected override AlternativeEvidenceCollection GetCollectionToTest() => collection;

	protected override BusinessObject GetNewElementToAddToTheCollection() => new AlternativeEvidence(sendingObjectParent);

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		sendingObjectParent = new CustomsDeclarationMessageSendingObjectParent(declaration);
		collection = new AlternativeEvidenceCollection(sendingObjectParent);
	}
	CustomsDeclarationMessageSendingObjectParent sendingObjectParent;
	AlternativeEvidenceCollection collection;
}
