using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(EDIMessageCollection))]
sealed class EDIMessageCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<EDIMessage>();

	protected override BusinessObjectCollection GetCollectionToTest() => new EDIMessageCollection(Factory.New<NctsHeader>());
}
