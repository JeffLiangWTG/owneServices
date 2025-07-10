using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusTWControllingMessageHeaderCollection))]
	sealed class CusTWControllingMessageHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CusTWControllingMessageHeaderCollection(Factory.New<CusEntryInstruction>());
		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusTWControllingMessageHeader>();
	}
}
