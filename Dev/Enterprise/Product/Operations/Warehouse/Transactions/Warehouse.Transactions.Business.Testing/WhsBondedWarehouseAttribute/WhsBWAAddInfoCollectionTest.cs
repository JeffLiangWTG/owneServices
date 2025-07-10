using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsBWAAddInfoCollection))]
	public class WhsBWAAddInfoCollectionTest : NonPersistentBusinessObjectCollectionTestCase<WhsBWAAddInfoCollection>
	{
		public void TestSerialise()
		{
			var infos = GetCollectionToTest();
			AssertEquals("Empty collection", 0, infos.Count);
			AssertEquals("Nothing to serialize.", ZString.Empty, infos.Serialize());

			infos.Add(new WhsBWAAddInfo("PROPERTY1", "VALUE1"));
			AssertEquals("Empty collection", 1, infos.Count);
			AssertEquals("The only add info should be serialized.", "PROPERTY1=VALUE1", infos.Serialize());

			infos.Add(new WhsBWAAddInfo("PROPERTY2", "VALUE2"));
			AssertEquals("Empty collection", 2, infos.Count);
			AssertEquals("The 2 add info should be serialized.", "PROPERTY1=VALUE1*PROPERTY2=VALUE2", infos.Serialize());
		}

		public void TestProperties()
		{
			var collection = GetCollectionToTest();
			AssertEquals("Empty collection", 0, collection.Count);
			AssertEquals("AllowNew", false, collection.AllowNew);
			AssertEquals("AllowRemove", false, collection.AllowRemove);
		}

		public void TestUniqueness()
		{
			var infos = GetCollectionToTest();
			AssertEquals("Empty collection", 0, infos.Count);

			var info1 = (WhsBWAAddInfo)GetNewElementToAddToTheCollection();
			infos.Add(info1);
			AssertEquals("1 line", 1, infos.Count);
			AssertEquals("First element is correct", info1, infos[0]);

			infos.Add(info1);
			AssertEquals("Still only 1 line", 1, infos.Count);
			AssertEquals("First element is correct", info1, infos[0]);

			var info2 = new WhsBWAAddInfo("key2", "value2");
			infos.Add(info2);
			AssertEquals("2 lines", 2, infos.Count);
			AssertEquals("Second element is correct", info2, infos[1]);
		}

		protected override WhsBWAAddInfoCollection GetCollectionToTest()
		{
			return new WhsBWAAddInfoCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WhsBWAAddInfo("key", "value");
		}
	}
}
