using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderCollection<CusEntryHeader>))]
	sealed class CusEntryHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new CusEntryHeaderCollection<CusEntryHeader>(declaration, Factory);
		}

		Mock<CusEntryHeaderCollection<CusEntryHeader>> GetCollectionMock(bool callBase = true)
		{
			var declaration = Factory.New<JobDeclaration>();
			return new Mock<CusEntryHeaderCollection<CusEntryHeader>>(declaration, Factory)
			{
				CallBase = callBase
			};
		}

		public void TestAllowNew()
		{
			var collectionMock = GetCollectionMock();
			var collection = collectionMock.Object;
			AssertEquals("(not-read-only): AllowNew", expected: true, collection.AllowNew);

			collectionMock.Setup(x => x.ReadOnly).Returns(true);
			AssertEquals("(is-read-only): AllowNew", expected: false, collection.AllowNew);
		}
	}
}
