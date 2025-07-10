using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Protest.Testing
{
	[TestedType(typeof(ProtestCollection))]
	sealed class ProtestCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ProtestCollection>
	{
		public void TestNearestMatch()
		{
			var protest1 = Factory.New<Protest>();
			protest1.US_P_CBPAssignedProtestNumber = "1234";
			var protest2 = Factory.New<Protest>();
			protest2.US_P_CBPAssignedProtestNumber = "1000";
			Factory.Save();

			var collection = new ProtestCollectionForTesting(Factory);

			var match9 = collection.FindBoxListProvider_Exposed.NearestMatch("9", false, -1).Item1;
			AssertEquals("NearestMatch(9)", "9", match9);
			var match10 = collection.FindBoxListProvider_Exposed.NearestMatch("10", false, -1).Item1;
			AssertEquals("NearestMatch(10)", protest2.US_P_CBPAssignedProtestNumber.ToString(), match10);
			var match12 = collection.FindBoxListProvider_Exposed.NearestMatch("12", false, -1).Item1;
			AssertEquals("NearestMatch(12)", protest1.US_P_CBPAssignedProtestNumber.ToString(), match12);
		}

		public void TestGetBusinessObjectFromCode()
		{
			var protest1 = Factory.New<Protest>();
			protest1.US_P_CBPAssignedProtestNumber = "11";
			var protest2 = Factory.New<Protest>();
			protest2.US_P_CBPAssignedProtestNumber = "122";
			Factory.Save();

			var collection = new ProtestCollectionForTesting(Factory);

			var match1 = collection.FindBoxListProvider_Exposed.GetBusinessObjectFromCode("1");
			AssertNull("GetBusinessObjectFromCode(1)", match1);
			var match11 = collection.FindBoxListProvider_Exposed.GetBusinessObjectFromCode("11");
			AssertEquals("GetBusinessObjectFromCode(11)", protest1.PK, match11.PK);
			var match122 = collection.FindBoxListProvider_Exposed.GetBusinessObjectFromCode("122");
			AssertEquals("GetBusinessObjectFromCode(122)", protest2.PK, match122.PK);
		}

		protected override ProtestCollection GetCollectionToTest() => new ProtestCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new Protest(Factory.New<JobDeclaration>());

		sealed class ProtestCollectionForTesting : ProtestCollection
		{
			public ProtestCollectionForTesting(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public IFindBoxListProvider FindBoxListProvider_Exposed => FindBoxListProvider;
		}
	}
}
