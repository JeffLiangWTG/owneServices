using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(BaseMessageSendingObjectCollection))]
sealed class BaseMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BaseMessageSendingObjectCollection>
{
	protected override BaseMessageSendingObjectCollection GetCollectionToTest() => new BaseMessageSendingObjectCollection(Factory);

	protected override BusinessObject GetNewElementToAddToTheCollection() => new BaseMessageSendingObject(header);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		header = declaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration declaration;
	CusEntryHeader header;
}
