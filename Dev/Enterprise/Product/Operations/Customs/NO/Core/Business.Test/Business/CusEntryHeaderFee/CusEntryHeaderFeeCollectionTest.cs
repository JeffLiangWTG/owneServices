using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderFeeCollection))]
	sealed class CusEntryHeaderFeeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CusEntryHeaderFeeCollection>
	{
		protected override CusEntryHeaderFeeCollection GetCollectionToTest() => new CusEntryHeaderFeeCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CusEntryHeaderFee();
	}
}
