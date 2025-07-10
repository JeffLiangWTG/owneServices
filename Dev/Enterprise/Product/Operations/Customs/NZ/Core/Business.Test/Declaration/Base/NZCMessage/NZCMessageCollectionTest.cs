using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using NUnit.Framework;

	[TestedType(typeof(NZCMessageCollection))]
	public class NZCMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			return new NZCMessageCollection(entryHeader);
		}

		[ExpectNoExceptions]
		public void TestIndexer()
		{
			testCollection.AddNew();
			NZCMessage message = testCollection[0];
		}

		public void TestAddNewSpecificType()
		{
			BusinessObject bO = testCollection.AddNew();
			Assert("BO is typeof CusEntryLine", bO is NZCMessage);
		}

		protected NZCMessageCollection testCollection;
		protected override void SetUp()
		{
			base.SetUp();

			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			testCollection = new NZCMessageCollection(entryHeader);
		}
	}
}
