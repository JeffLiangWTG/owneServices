using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(EDIMessageCollection))]
sealed class EDIMessageCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<EDIMessage>();

	protected override BusinessObjectCollection GetCollectionToTest() => new EDIMessageCollection(Factory.New<CusEntryHeader>());
}
